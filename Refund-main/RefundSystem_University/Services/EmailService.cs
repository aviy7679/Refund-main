using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RefundSystem_University.Services
{
    public interface IEmailService
    {
        Task SendAsync(string[] to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpClient _client;
        private readonly string _from;

        public EmailService(string host, int port, bool enableSsl, NetworkCredential credential, string from)
        {
            _client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = credential
            };
            _from = from;
        }

        public async Task SendAsync(string[] to, string subject, string body)
        {
            if (to == null || to.Length == 0)
                throw new ArgumentException("No recipients provided", nameof(to));

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(_from);
                foreach (var address in to)
                {
                    if (!string.IsNullOrWhiteSpace(address))
                        message.To.Add(address);
                }
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                await _client.SendMailAsync(message);
            }
        }
    }
}