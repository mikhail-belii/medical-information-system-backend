namespace WebApi;

public interface IEmailSender
{
    public Task SendEmailAsync(string name, string email, string subject, string message);
}