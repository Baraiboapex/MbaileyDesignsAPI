using MattBaileyDesignsAPI.Controllers;
using MattBaileyDesignsAPI.Services.Communication.Interfaces;
using System.Net;
using System.Net.Mail;

public class GmailEmailerService : IEmailerService
{
    private readonly IConfiguration _config;
    public GmailEmailerService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmail(OutboundDTO commentEmailObject)
    {
        try
        {
            var recipientEmailAddress = commentEmailObject.currentDtoValue["recipientEmail"]?.ToString();
            var recipientCommentTitle = commentEmailObject.currentDtoValue["title"]?.ToString();
            var recipientCommentContent = commentEmailObject.currentDtoValue["content"]?.ToString();

            if (string.IsNullOrEmpty(recipientEmailAddress) ||
                string.IsNullOrEmpty(recipientCommentTitle) ||
                string.IsNullOrEmpty(recipientCommentContent))
            {
                throw new ArgumentNullException("None of the fields inside of the outbound DTO for sending emails should be null!");
            }

            // Log configuration values
            var adminEmailAddress = _config["EmailSettings:AdminEmailAddress"];
            var appPassword = _config["EmailSettings:AdminPassword"];
            var currentEmailSmtpService = _config["EmailSettings:CurrentSMTPClient"];
            var currentEmailSmtpServicePort = _config["EmailSettings:SMTPClientPort"];

            Console.WriteLine($"Admin Email Address: {adminEmailAddress}");
            Console.WriteLine($"SMTP Service: {currentEmailSmtpService}");
            Console.WriteLine($"SMTP Service Port: {currentEmailSmtpServicePort}");

            if (string.IsNullOrEmpty(adminEmailAddress) ||
                string.IsNullOrEmpty(appPassword) ||
                string.IsNullOrEmpty(currentEmailSmtpService) ||
                string.IsNullOrEmpty(currentEmailSmtpServicePort))
            {
                throw new ArgumentNullException("Could not get relevant data from config file.");
            }

            // Use port 587 for TLS
            using var newSMTPClient = new SmtpClient(currentEmailSmtpService)
            {
                Port = 587,
                Credentials = new NetworkCredential(adminEmailAddress, appPassword.Trim()),
                EnableSsl = true, // Ensure STARTTLS is enabled
                UseDefaultCredentials = false
            };

            using var newEmailMessage = new MailMessage
            {
                From = new MailAddress(adminEmailAddress),
                Subject = recipientCommentTitle,
                Body = recipientCommentContent,
                IsBodyHtml = true
            };

            newEmailMessage.To.Add(adminEmailAddress);
            newEmailMessage.ReplyToList.Add(new MailAddress(recipientEmailAddress));

            await newSMTPClient.SendMailAsync(newEmailMessage);
        }
        catch (SmtpException ex)
        {
            Console.WriteLine($"SMTP Error: {ex.StatusCode} - {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw new Exception("Error: Cannot Send Email:", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw new Exception("Error: Cannot Send Email:", ex);
        }
    }
}
