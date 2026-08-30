using RoleBasedAuthenticationApi.DTO.Auth;
using RoleBasedAuthenticationApi.DTO.Token;


namespace RoleBasedAuthenticationApi.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResult> RegisterAsync(RegisterDto dto);
        Task<LoginResult> LoginAsync(LoginDto dto);
        Task<RefreshTokenResult> RefreshTokenAsync(string token);
        Task LogoutAsync(string id);
    }
}
