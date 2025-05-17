using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace CertGenAPI.Services
{
    public class EmailService
    {
        public async Task SendCertificateAsync(string toEmail, string toName, string filePath)
        {
            Console.WriteLine("Sending email...");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ECHO AND POCUS FOR BEGINNERS 2025", "maxnaufal25@gmail.com"));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = "Your Certificate – Thank You for ECHO AND POCUS FOR BEGINNERS 2025!";

            var builder = new BodyBuilder
            {
                TextBody = $"Dear {toName},\n\nWe sincerely appreciate your contribution to the success of ECHO AND POCUS FOR BEGINNERS 2025—whether as a participant or a committee member. Your engagement and dedication were instrumental in making this seminar impactful.\n\nAttached to this email, you’ll find your certificate as a token of our gratitude. We hope the knowledge shared and connections made during the event prove valuable in your practice.\n\nFor any questions or feedback, don’t hesitate to contact us. Looking forward to future collaborations!\n\nWarm Regards,\nDr Yasmin Yusof\nOrganizer of ECHO POCUS course\nHospital Banting",
            };

            Console.WriteLine("Sending email to: " + toEmail);
            // Attach the certificate
            builder.Attachments.Add(filePath);
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls); // Use true for SSL (port 465)
            await client.AuthenticateAsync("maxnaufal25@gmail.com", "bnto ognk lsxa aajb");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
