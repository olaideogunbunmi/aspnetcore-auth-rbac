namespace RoleBasedAuthenticationApi.Configuration
{
    public class SmtpSettings
    {
        public string Host { get; init; } = "";
        public int Port { get; init; }
        public string Username { get; init; } = "";
        public string Password { get; init; } = "";
        public string FromAddress { get; init; } = "";
        public string FromName { get; init; } = "";
    }
}
