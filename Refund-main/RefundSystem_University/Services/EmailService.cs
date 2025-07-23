using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace RefundSystem_University.Services
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _port = 587;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(string username, string password, string fromEmail = null, string fromName = "מערכת החזרים")
        {
            _username = username;
            _password = password;
            _fromEmail = fromEmail ?? username;
            _fromName = fromName;
        }

        public bool SendEmail(string[] toEmails, string subject, string body, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using (var client = new SmtpClient(_smtpServer, _port))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_username, _password);

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(_fromEmail, _fromName, Encoding.UTF8);
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = true;
                        message.BodyEncoding = Encoding.UTF8;
                        message.SubjectEncoding = Encoding.UTF8;

                        foreach (var email in toEmails)
                        {
                            if (!string.IsNullOrWhiteSpace(email))
                                message.To.Add(email);
                        }

                        if (message.To.Count == 0)
                        {
                            errorMessage = "לא נמצאו כתובות מייל תקינות";
                            return false;
                        }

                        client.Send(message);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}