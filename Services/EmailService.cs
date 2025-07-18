using System.Net;
using System.Net.Mail;
using EmailRelayServer.DTOs;

namespace EmailRelayServer.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    public EmailService(IConfiguration config) => _config = config;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpEmail = _config["Smtp:Gmail:Email"];
        var smtpPass = _config["Smtp:Gmail:AppPassword"];

        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpEmail, smtpPass)
        };

        var mail = new MailMessage(from: smtpEmail ?? String.Empty, to, subject, body);
        await client.SendMailAsync(mail);
    }

    public async Task SendContactFormAsync(ContactFormRequest req, string toEmail)
    {
        var smtpEmail = _config["Smtp:Gmail:Email"];
        var smtpPass = _config["Smtp:Gmail:AppPassword"];

        var subject = $"Message from {req.Name} via EchoMail";

        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "contact.html");
        var template = await File.ReadAllTextAsync(templatePath);

        string contactInfo = "";
        if (!string.IsNullOrWhiteSpace(req.Phone))
            contactInfo += $"<br><strong>Phone:</strong> {WebUtility.HtmlEncode(req.Phone)}";
        if (!string.IsNullOrWhiteSpace(req.Website))
            contactInfo += $"<br><strong>Website:</strong> {WebUtility.HtmlEncode(req.Website)}";

        var body = template
            .Replace("{{Name}}", WebUtility.HtmlEncode(req.Name))
            .Replace("{{Email}}", WebUtility.HtmlEncode(req.Email))
            .Replace("{{ContactInfo}}", contactInfo)
            .Replace("{{Message}}", WebUtility.HtmlEncode(req.Message))
            .Replace("{{Timestamp}}", DateTime.Now.ToString("MMMM dd, yyyy 'at' h:mm tt"));

        var mail = new MailMessage
        {
            From = new MailAddress(req.Email),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(toEmail);

        using var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpEmail, smtpPass)
        };

        await client.SendMailAsync(mail);
    }

}
