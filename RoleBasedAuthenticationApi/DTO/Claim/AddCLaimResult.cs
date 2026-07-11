namespace RoleBasedAuthenticationApi.DTO.Claim
{
    public class AddCLaimResult
    {
        public bool IsSuccess { get; init; }
        public AddClaimFailure? Failure { get; init; }
        public List<string> Errors { get; init; } = [];

        public enum AddClaimFailure
        {
            UserNotFound,
            ClaimAlreadyExist,
            ClaimAddFailed
        }
    }
}
