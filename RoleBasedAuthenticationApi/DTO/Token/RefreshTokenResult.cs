namespace RoleBasedAuthenticationApi.DTO.Token
{
    public class RefreshTokenResult
    {
        public bool IsSuccess { get; init; }
        public TokenFailureType? Failure { get; init; }
        public string? AccessToken { get; init; }
        public string? RefreshToken { get; init; }

    }
    public enum TokenFailureType
    {
        Invalid,
        UserNotFound
    }
}
