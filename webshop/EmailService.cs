using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;

public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}

public class EmailService
{
    private readonly SmtpSettings _settings;

    public EmailService(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_settings.UserName));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message };

        using var smtp = new SmtpClient();

        // Check if using STARTTLS (Port 587) or SMTPS (Port 465)
        if (_settings.Port == 587)
        {
            await smtp.ConnectAsync(_settings.Host, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
        }
        else if (_settings.Port == 465)
        {
            await smtp.ConnectAsync(_settings.Host, _settings.Port, MailKit.Security.SecureSocketOptions.SslOnConnect);
        }
        else
        {
            throw new InvalidOperationException("Unsupported port for SMTP connection.");
        }

        await smtp.AuthenticateAsync(_settings.UserName, _settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
