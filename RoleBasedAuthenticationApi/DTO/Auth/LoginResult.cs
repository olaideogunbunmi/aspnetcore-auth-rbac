
namespace RoleBasedAuthenticationApi.DTO.Auth
{
    public enum LoginResultType
    {
        InvalidCredentials,
        AccountLocked,
        UserNotFound
    }

    public class LoginResult
    {
        public bool IsSuccess { get; init; }
        public string? AccessToken {  get; init; }
        public string? RefreshToken { get; set; }
        public LoginResultType? Failure { get; init; }
    }
}
