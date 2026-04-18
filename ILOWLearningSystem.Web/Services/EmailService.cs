using System.Net;
using System.Net.Mail;
using ILOWLearningSystem.Web.Models;
using Microsoft.Extensions.Options;

namespace ILOWLearningSystem.Web.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using var message = new MailMessage();
        message.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = false;

        using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
        {
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.AppPassword),
            EnableSsl = true
        };

        await smtp.SendMailAsync(message);
    }
}