using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        public Task SendEmailAsync(string to, string subject, string body)
        {
            // This is a mock implementation for testing.
            // Replace with a real email library (e.g., MailKit) for production.
            Console.WriteLine("---- SENDING EMAIL ----");
            Console.WriteLine($"To: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");
            Console.WriteLine("-----------------------");

            return Task.CompletedTask;
        }
    }
}