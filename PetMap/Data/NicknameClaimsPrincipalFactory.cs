using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace PetMap.Data;

public sealed class NicknameClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var nickname = string.IsNullOrWhiteSpace(user.Nickname)
            ? await UserManager.GetUserNameAsync(user) ?? "회원"
            : user.Nickname.Trim();

        var nameClaimType = Options.ClaimsIdentity.UserNameClaimType;
        foreach (var claim in identity.FindAll(nameClaimType).ToArray())
        {
            identity.RemoveClaim(claim);
        }
        identity.AddClaim(new Claim(nameClaimType, nickname));

        return identity;
    }
}
