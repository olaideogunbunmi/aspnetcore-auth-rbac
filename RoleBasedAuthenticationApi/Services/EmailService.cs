using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using RoleBasedAuthenticationApi.Configuration;
using RoleBasedAuthenticationApi.Interfaces;


namespace RoleBasedAuthenticationApi.Services
{
    public class EmailService : IEmailServices
    {
        private readonly SmtpSettings _smtpSettings;
        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }
        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
        {
            //Read Mailtrap configurations safely

            var host = _smtpSettings.Host;
            var port = _smtpSettings.Port;
            var username = _smtpSettings.Username;
            var password = _smtpSettings.Password;
            var fromAddress = _smtpSettings.FromAddress;
            var fromName = _smtpSettings.FromName;

            //Create the message using MimeKit

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Password Reset Token";

            message.Body = new TextPart("html")
            {
                Text = $@"<p>Use this token to reset your password:</p> 
                  <p><strong>{token}</strong></p>
                  <p>Submit it to <code>/reset-password</code> endpoint along with your new password.</p>"
            };

            //Send using MailKit's production-ready SMTP client
            using var client = new SmtpClient();

            //Connect to Mailtrap.SecureSocketOptions.StartTls works perfectly on Mailtrap's ports (2525, 587)
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
             
            //Authenticate with your credentials
            await client.AuthenticateAsync(username, password);

            // Send the email asynchronously
            await client.SendAsync(message);

            // Cleanly close connection paths to prevent resource memory leaks
            await client.DisconnectAsync(true);
        }
    }
}
