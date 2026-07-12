using System.Net;

namespace RoleBasedAuthenticationApi.DTO.Auth
{
    public class RegisterResult
    {
        public string? Id { get; init; }
        public bool IsSuccess { get; init; }
        public List<string> Errors { get; init; } = [];
        public RegisterFailure? Failure { get; init; }

    }

    public enum RegisterFailure
    {
        DuplicateEmail,
        ValidationFailed,
        UnexpectedError
    }
}
