namespace RoleBasedAuthenticationApi.DTO.Token
{
    public class RefreshTokenResponseDto
    {
        public string? AccessToken { get; init; }
        public string? RefreshToken { get; init; }
    }
}
