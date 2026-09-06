namespace RoleBasedAuthenticationApi.Interfaces
{
    public interface IEmailServices
    {
        Task SendPasswordResetEmailAsync(string toEmail, string token);
    }
}
