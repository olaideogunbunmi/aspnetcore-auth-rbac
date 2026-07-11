using System.Net;

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
        public string? Token {  get; init; }
        public LoginResultType? Failure { get; init; }
    }
}
