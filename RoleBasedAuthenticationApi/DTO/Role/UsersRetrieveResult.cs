using RoleBasedAuthenticationApi.Models;

namespace RoleBasedAuthenticationApi.DTO.Role
{
    public class UsersRetrieveResult
    {
        public bool IsSuccess { get; init; }

        public RetrieveFailure? Failure { get; init; }

        public List<UserDetailsDto> Users { get; init; } = [];
    }

    public enum RetrieveFailure
    {
        RoleNotFound
    }


}
