using RoleBasedAuthenticationApi.Models;
using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.User
{
    public class AssignRoleDto
    {
        [Required]
        public string Role { get; init; }
    }

    public class AssignRoleResult
    {
        public bool IsAssigned { get; init; }
        public List<string> Errors { get; init; } = [];
        public AssignFailure? Failure { get; init; }
    }
    public enum AssignFailure
    {
        UserNotFound,
        RoleNotFound,
        AssignFailed
    }
}
