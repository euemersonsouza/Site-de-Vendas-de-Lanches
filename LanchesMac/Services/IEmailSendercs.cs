namespace LanchesMac.Services
{
    public interface IEmailSendercs
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
