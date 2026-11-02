using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using PetMap.Data;

namespace PetMap.Components.Account;

public sealed class SmtpIdentityEmailSender(IConfiguration configuration) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendEmailAsync(email, "Confirm your account", $"Confirm your account: <a href=\"{WebUtility.HtmlEncode(confirmationLink)}\">Confirm email</a>");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendEmailAsync(email, "Reset your password", $"Reset your password: <a href=\"{WebUtility.HtmlEncode(resetLink)}\">Reset password</a>");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendEmailAsync(email, "Reset your password", $"Your password reset code is: <strong>{WebUtility.HtmlEncode(resetCode)}</strong>");

    private async Task SendEmailAsync(string recipient, string subject, string htmlBody)
    {
        var settings = configuration.GetSection("Email:Smtp");
        var host = settings["Host"]!;
        var port = settings.GetValue<int>("Port");
        var username = settings["Username"]!;
        var password = settings["Password"]!;
        var from = settings["FromAddress"]!;
        var fromName = settings["FromName"] ?? "PetMap";

        using var message = new MailMessage
        {
            From = new MailAddress(from, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(recipient));

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password)
        };
        await client.SendMailAsync(message);
    }
}
