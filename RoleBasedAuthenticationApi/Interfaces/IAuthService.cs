using RoleBasedAuthenticationApi.DTO.Auth;
using RoleBasedAuthenticationApi.DTO.Password;
using RoleBasedAuthenticationApi.DTO.Token;


namespace RoleBasedAuthenticationApi.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResult> RegisterAsync(RegisterDto dto);
        Task<LoginResult> LoginAsync(LoginDto dto);
        Task<RefreshTokenResult> RefreshTokenAsync(string token);
        Task LogoutAsync(string id);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordDto dto);
        Task<ChangePasswordResult> ChangePasswordAsync(string email, ChangePasswordDto dto);
    }
}
