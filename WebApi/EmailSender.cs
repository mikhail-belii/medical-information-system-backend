using System.Net;
using MailKit.Net.Smtp;
using MimeKit;

namespace WebApi;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task SendEmailAsync(string name, string email, string subject, string message)
    {
        var emailFrom = _configuration["Smtp:Email"];
        var password = _configuration["Smtp:Password"];
        if (string.IsNullOrEmpty(emailFrom) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException("SMTP credentials were not found in appsettings.json");
        }
        
        var credentials = new NetworkCredential(emailFrom, password);
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress("MIS Try Not To Die", emailFrom));
        msg.To.Add(new MailboxAddress(name, email));
        msg.Subject = subject;
        msg.Body = new TextPart("plain")
        {
            Text = message
        };

        using (var client = new SmtpClient()) {
            await client.ConnectAsync("smtp.mail.ru", 465, true, cancellationToken: default);
            await client.AuthenticateAsync(credentials, cancellationToken: default);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true, cancellationToken: default);
        }
    }
}