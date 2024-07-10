using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Threading.Tasks;

namespace WorkFlowWeb.Utility
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var senderEmail = "shrishgupta0103@gmail.com"; // Replace with your Gmail address
            var senderName = "shrishgupta0103@gmail.com"; // Replace with your name
            var senderPassword = "julpvxxkghpawewc"; // Replace with your Gmail app password

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", email)); // Empty name for recipient
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = htmlMessage;
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 465, true); // SMTP server address, port, and enable SSL
            await client.AuthenticateAsync(senderEmail, senderPassword); // Authenticate with the SMTP server

            await client.SendAsync(message);
            await client.DisconnectAsync(true); // Disconnect from the SMTP server
        }
    }
}
