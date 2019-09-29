using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ProxiCall.CRM.Services
{
    public class EmailSender : IEmailSender
    {
        public readonly IConfiguration Configuration;
        private readonly SendGridClient _client;

        public EmailSender(IConfiguration configuration)
        {
            Configuration = configuration;
            _client = new SendGridClient(Configuration.GetSection("Sendgrid")["ApiKey"]);
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var from = new EmailAddress(
                email: Configuration.GetSection("Sendgrid")["FromEmail"], 
                name: Configuration.GetSection("Sendgrid")["FromName"]
            );
            var to = new EmailAddress(email);
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await _client.SendEmailAsync(msg);
        }
    }
}
