namespace RoleBasedAuthenticationApi.Configuration
{
    public class SmtpSettings
    {
        public string Host { get; init; } = string.Empty;
        public int Port { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string FromAddress { get; init; } = string.Empty;
        public string FromName { get; init; } = string.Empty;
    }
}
