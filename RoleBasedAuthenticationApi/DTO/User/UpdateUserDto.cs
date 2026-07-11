using RoleBasedAuthenticationApi.Models;
using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.User
{
    public class UpdateUserDto
    {
        [Required]
        public string FullName { get; init; }

        [Required]
        [EmailAddress]
        public string Email {  get; init; }
    }

    public class UpdateUserResult
    {
        public bool IsSuccess { get; init; }
        public List<string> Errors { get; init; } = [];
        public UserDetailsDto? User {  get; init; }
    }
}
