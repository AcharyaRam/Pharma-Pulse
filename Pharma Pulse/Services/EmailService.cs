using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace Pharma_Pulse.Services
{
    public class EmailService
    {
        private readonly string apiKey;
        private readonly string fromEmail;

        public EmailService(IConfiguration config)
        {
            apiKey = config["SendGrid:ApiKey"]!;
            fromEmail = config["SendGrid:FromEmail"]!;
        }

        public async Task SendEmail(string to, string subject, string body)
        {
            Console.WriteLine("Inside Email Service");
            Console.WriteLine("API Key: " + (apiKey != null ? "Loaded" : "NULL"));
            Console.WriteLine("From Email: " + fromEmail);

            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(fromEmail, "Pharma Pulse");
            var toEmail = new EmailAddress(to);

            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);

            var response = await client.SendEmailAsync(msg);

            Console.WriteLine("Status: " + response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Body.ReadAsStringAsync();
                Console.WriteLine("Error: " + error);
            }
        }
    }
}