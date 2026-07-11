namespace RoleBasedAuthenticationApi.DTO.Claim
{
    public class UserClaimDto //output claim dto
    {
        public string Type { get; init; }
        public string Value { get; init; }
    }

    public class GetUserClaimsResult
    {
        public bool UserNotFound { get; init; }
        public List<UserClaimDto> Claims { get; init; } = [];
    }
}
