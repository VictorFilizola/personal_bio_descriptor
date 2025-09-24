namespace LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces
{
    public class IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
