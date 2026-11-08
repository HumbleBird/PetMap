using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Net;
using PetMap.Components;
using PetMap.Components.Account;
using PetMap.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    // Keep startup migrations running if the runtime model check reports a false-positive drift.
    // The warning remains visible in logs so a real missing migration can still be investigated.
    options.ConfigureWarnings(warnings =>
        warnings.Log(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Password.RequiredLength = 12;
        options.Password.RequiredUniqueChars = 4;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
}
else
{
    var smtp = builder.Configuration.GetSection("Email:Smtp");
    if (string.IsNullOrWhiteSpace(smtp["Host"])
        || string.IsNullOrWhiteSpace(smtp["Username"])
        || string.IsNullOrWhiteSpace(smtp["Password"])
        || string.IsNullOrWhiteSpace(smtp["FromAddress"])
        || smtp.GetValue<int>("Port") is < 1 or > 65535)
        throw new InvalidOperationException("운영 이메일 발송을 위해 Email:Smtp 설정을 모두 구성해야 합니다.");
    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, SmtpIdentityEmailSender>();
}

builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
{
    options.Cookie.Name = "PetMap.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    var trustedProxies = builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [];
    foreach (var proxyAddress in trustedProxies)
        options.KnownProxies.Add(IPAddress.Parse(proxyAddress));
});

if (!builder.Environment.IsDevelopment())
{
    var keyDirectory = builder.Configuration["DataProtection:KeyDirectory"];
    if (string.IsNullOrWhiteSpace(keyDirectory))
        throw new InvalidOperationException("운영 환경에는 영구 Data Protection 키 경로를 설정해야 합니다.");

    Directory.CreateDirectory(keyDirectory);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory))
        .SetApplicationName("PetMap");

    var allowedHosts = builder.Configuration["AllowedHosts"];
    if (string.IsNullOrWhiteSpace(allowedHosts)
        || allowedHosts.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Contains("*"))
        throw new InvalidOperationException("운영 환경에는 실제 도메인으로 AllowedHosts를 설정해야 합니다.");

    if (connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("운영 환경에는 외부에서 접근 가능한 SQL Server 연결 문자열을 설정해야 합니다.");
}

var app = builder.Build();
app.UseForwardedHeaders();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var migrationScope = app.Services.CreateAsyncScope();
    var contextFactory = migrationScope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await using var dbContext = await contextFactory.CreateDbContextAsync();
    await dbContext.Database.MigrateAsync();
}

// Admin 역할을 보장하고 설정된 이메일 계정에 역할을 부여한다.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    const string adminRole = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRole));
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException("Admin 역할을 생성하지 못했습니다.");
        }
    }

    var adminEmail = app.Configuration["Admin:Email"];
    if (!string.IsNullOrWhiteSpace(adminEmail))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is not null && !await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            var assignment = await userManager.AddToRoleAsync(adminUser, adminRole);
            if (!assignment.Succeeded)
            {
                throw new InvalidOperationException("설정된 계정에 Admin 역할을 부여하지 못했습니다.");
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=(self)");
    await next();
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
