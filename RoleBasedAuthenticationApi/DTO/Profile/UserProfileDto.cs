using RoleBasedAuthenticationApi.DTO.Claim;

namespace RoleBasedAuthenticationApi.DTO.Profile
{
    public class UserProfileDto
    {
        public string Id { get; init; }
        public string Email { get; init; }
        public string Name { get; init; }
        public List<string> Role { get; init; } = [];
        public List<UserClaimDto> CustomClaims { get; init; } = [];
    }
}
