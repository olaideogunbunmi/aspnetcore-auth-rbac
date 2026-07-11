using System.Net;

namespace RoleBasedAuthenticationApi.DTO.Auth
{
    public class RegisterResult
    {
        public bool IsSuccess { get; init; }
        public List<string> Errors { get; init; } = [];
    }

}
