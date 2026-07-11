namespace RoleBasedAuthenticationApi.DTO.Claim
{
    public class RemoveClaimResult
    {
        public bool IsSuccess { get; init; }
        public ClaimRemoveFailure? Failure { get; init; }
        public List<string> Errors { get; init; } = [];
    }

    public enum ClaimRemoveFailure
    {
        UserNotFound,
        ClaimNotFound,
    }
}
