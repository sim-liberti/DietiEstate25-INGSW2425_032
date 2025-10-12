using DietiEstate.Shared.Models;
using DietiEstate.Shared.Models.Templates;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace DietiEstate.WorkerService.Services;

public class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger
    ) : IEmailService
{
    public async Task SendEmailAsync(string jsonData, CancellationToken cancellationToken)
    {
        var smtpSettings = configuration.GetSection("Smtp").Get<SmtpServerTemplate>();
        if (smtpSettings is null)
        {
            logger.LogError("SMTP settings are not configured properly.");
            return;
        }

        EmailData? emailData;
        try
        {
            emailData = System.Text.Json.JsonSerializer.Deserialize<EmailData>(jsonData);
            if (emailData is null)
            {
                logger.LogError("Email data is null after deserialization.");
                return;
            }
        } catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email.");
            return;
        }
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtpSettings.FromName, smtpSettings.FromEmail));
        message.To.Add(new MailboxAddress(emailData.ToName, emailData.ToEmail));
        message.Subject = emailData.Subject;
        message.Body = new TextPart(TextFormat.Plain)
        {
            Text = emailData.Body
        };
        
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(smtpSettings.Server, 
            smtpSettings.Port, 
            MailKit.Security.SecureSocketOptions.StartTls, 
            cancellationToken);
        await smtp.AuthenticateAsync(smtpSettings.Username,
            smtpSettings.Password,
            cancellationToken);
        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
        logger.LogInformation("Email sent successfully to {Recipient}", emailData.ToName);
    }
}