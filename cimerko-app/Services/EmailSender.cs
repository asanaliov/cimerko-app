using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace cimerko_app.Services;

// Sends mail through the SMTP server in the "Email" config section.
// Without a host (local development) the message is written to the log instead.
public class EmailSender : IEmailSender {
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger) {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage) {
        var settings = _configuration.GetSection("Email");
        var host = settings["Host"];
        if (string.IsNullOrWhiteSpace(host)) {
            _logger.LogWarning(
                "No SMTP host configured. Email to {Email} with subject \"{Subject}\":\n{Body}",
                email,
                subject,
                htmlMessage);
            return;
        }

        using var message = new MailMessage(settings["From"] ?? settings["UserName"]!, email, subject, htmlMessage) {
            IsBodyHtml = true
        };
        using var client = new SmtpClient(host, settings.GetValue("Port", 587)) {
            EnableSsl = settings.GetValue("EnableSsl", true),
            Credentials = new NetworkCredential(settings["UserName"], settings["Password"])
        };

        await client.SendMailAsync(message);
    }
}
