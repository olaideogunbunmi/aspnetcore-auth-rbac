using RoleBasedAuthenticationApi.DTO.Auth;


namespace RoleBasedAuthenticationApi.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResult> RegisterAsync(RegisterDto dto);
        Task<LoginResult> LoginAsync(LoginDto dto);
    }
}
