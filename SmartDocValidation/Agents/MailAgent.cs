using System.Net;
using System.Net.Mail;

namespace SmartDocValidation.Agents
{
    public class MailAgent
    {
        private readonly string _email;
        private readonly string _password;

        public MailAgent(IConfiguration config)
        {
            _email = config["EmailSettings:Email"];
            _password = config["EmailSettings:Password"];
        }

        public async Task SendMail(string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_email, _password),
                    EnableSsl = true,
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(_email),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false,
                };

                mail.To.Add(_email);

                await smtpClient.SendMailAsync(mail);

                Console.WriteLine("✅ Mail sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Mail failed:");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
