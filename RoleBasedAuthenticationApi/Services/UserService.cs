using AutoMapper;
using RoleBasedAuthenticationApi.DTO.Claim;
using RoleBasedAuthenticationApi.DTO.Role;
using RoleBasedAuthenticationApi.DTO.User;
using RoleBasedAuthenticationApi.Interfaces;
using RoleBasedAuthenticationApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static RoleBasedAuthenticationApi.DTO.Claim.AddCLaimResult;


namespace RoleBasedAuthenticationApi.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }
        public async Task<List<UserDetailsDto>> GetUsersAsync()
        {
            return await _mapper.ProjectTo<UserDetailsDto>(_userManager.Users).ToListAsync();
        }//STANDARD
        public async Task<UserDetailsDto?> GetUserProfileAsync(string id)  //DONE
        {
            return await _mapper.ProjectTo<UserDetailsDto>(_userManager.Users.Where(u => u.PublicId == id)).FirstOrDefaultAsync();
        }//STANDARD
        public async Task<UpdateUserResult?> UpdateUserAsync(string id, UpdateUserDto dto)
        {

            var user = await _userManager.Users.Where(u => u.PublicId == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            _mapper.Map(dto, user);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new UpdateUserResult { IsSuccess = false, Errors = result.Errors.Select(e => e.Description).ToList() };
            }

            return new UpdateUserResult { IsSuccess = true, User = _mapper.Map<UserDetailsDto>(user) };
        }//STANDARD
        public async Task<DeleteUserResult?> DeleteUserAsync(string id)
        {
            var user = await _userManager.Users.Where(u => u.PublicId == id).FirstOrDefaultAsync();

            if (user == null)
                return null;

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return new DeleteUserResult { IsDeleted = false, Errors = result.Errors.Select(e => e.Description).ToList() };
            }

            return new DeleteUserResult { IsDeleted = true };
        }//STANDARD
        public async Task<AssignRoleResult> AssignRoleAsync(string id, AssignRoleDto dto)
        {
            var user = await _userManager.Users.Where(u => u.PublicId == id).FirstOrDefaultAsync();

            if (user == null)
                return new AssignRoleResult { IsAssigned = false, Failure = AssignFailure.UserNotFound };

            var roleExist = await _roleManager.RoleExistsAsync(dto.Role);

            if (!roleExist)
                return new AssignRoleResult { IsAssigned = false, Failure = AssignFailure.RoleNotFound };

            var result = await _userManager.AddToRoleAsync(user, dto.Role);

            if (!result.Succeeded)
            {
                return new AssignRoleResult { Failure = AssignFailure.AssignFailed, IsAssigned = false, Errors = result.Errors.Select(e => e.Description).ToList() };
            }

            return new AssignRoleResult { IsAssigned = true };
        }//STANDARD

        public async Task<UserRoleRemoveResult> RemoveRoleFromUserAsync(string id, string name)
        {
            var user = await _userManager.Users.Where(u => u.PublicId == id).FirstOrDefaultAsync();

            if (user == null)
                return new UserRoleRemoveResult { IsSuccess = false, Failure = RemoveFailure.UserNotFound };

            var role = await _roleManager.FindByNameAsync(name);

            if (role == null)
                return new UserRoleRemoveResult { IsSuccess = false, Failure = RemoveFailure.RoleNotFound };

            var result = await _userManager.RemoveFromRoleAsync(user, name);

            if (!result.Succeeded)
            {
                return new UserRoleRemoveResult { IsSuccess = false, Failure = RemoveFailure.RemoveFailed, Errors = result.Errors.Select(e => e.Description).ToList() };
            }

            return new UserRoleRemoveResult { IsSuccess = true };
        }//STANDARD

        public async Task<GetUserRolesResult> GetUserRolesAsync(string id)
        {
            var user = await _userManager.Users.Where(u => u.PublicId == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return new GetUserRolesResult
                {
                    UserNotFound = true
                };
            }

            var roleNames = await _userManager.GetRolesAsync(user);

            var roles = await _roleManager.Roles.Where(r => roleNames.Contains(r.Name)).ToListAsync();

            return new GetUserRolesResult
            {
                Roles = _mapper.Map<List<RoleDto>>(roles)
            };
        }//STANDARD

        public async Task<AddCLaimResult> AddUserClaimsAsync(string id, ClaimDto dto)
        {
           var user = await _userManager.Users.Where(u => u.PublicId == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return new AddCLaimResult
                {
                   IsSuccess = false,
                   Failure = AddClaimFailure.UserNotFound
                };
            }

            var existingClaims = await _userManager.GetClaimsAsync(user);

            var claimExist = existingClaims.Any(c => c.Type == dto.Type && c.Value == dto.Value);

            if (claimExist)
            {
                return new AddCLaimResult
                {
                    IsSuccess = false,
                    Failure = AddClaimFailure.ClaimAlreadyExist
                };
            }

            var result = await _userManager.AddClaimAsync(user, new Claim(dto.Type, dto.Value));

            if (!result.Succeeded)
            {
                return new AddCLaimResult
                {
                    IsSuccess = false,
                    Failure = AddClaimFailure.ClaimAddFailed,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            return new AddCLaimResult { IsSuccess = true };
        }//STANDARD

        public async Task<GetUserClaimsResult> GetUserClaimsAsync(string id)
        {
            var user = await _userManager.Users.Where(u => u.PublicId == id).AsNoTracking().FirstOrDefaultAsync();

            if (user == null)
            {
                return new GetUserClaimsResult { UserNotFound = true };
            }

            var claim = await _userManager.GetClaimsAsync(user);

            return new GetUserClaimsResult
            {
                Claims = claim.Select(c => new UserClaimDto { Type = c.Type, Value = c.Value }).ToList(),
            };
        }//STANDARD

        public async Task<RemoveClaimResult> RemoveUserClaimsAsync(string id, string type, string value)
        {
            var user = await _userManager.Users.Where(u => u.PublicId == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return new RemoveClaimResult
                { 
                    IsSuccess = false,
                    Failure = ClaimRemoveFailure.UserNotFound
                };
            }

            var claims = await _userManager.GetClaimsAsync(user);

           var userClaim = claims.FirstOrDefault(c => c.Type == type && c.Value == value);

            if (userClaim == null)
            {
                return new RemoveClaimResult
                {
                    IsSuccess = false,
                    Failure = ClaimRemoveFailure.ClaimNotFound
                };
            }

            var result = await _userManager.RemoveClaimAsync(user, userClaim);

            if (!result.Succeeded)
            {
                return new RemoveClaimResult
                {
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description).ToList(),
                };
            }

            return new RemoveClaimResult { IsSuccess = true };
        }//STANDARD

        public async Task<LockedUserResult> DisableUserAsync(string id)
        {
            var user = await _userManager.Users.Where(u => u.PublicId == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return new LockedUserResult
                {
                    IsSuccess = false,
                    Failure = UserLockoutStatus.UserNotFound
                };
            }

            var lockEnable = await _userManager.SetLockoutEnabledAsync(user, true);

            if (!lockEnable.Succeeded)
            {
                return new LockedUserResult
                {
                    IsSuccess = false,
                    Errors = lockEnable.Errors.Select(e => e.Description).ToList()
                };
            }


            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            if (!result.Succeeded)
            {
                return new LockedUserResult
                {
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            //This changes the user's secret key in the database, invalidating existing auth cookies or tokens on their next API request.

            var stampResult = await _userManager.UpdateSecurityStampAsync(user);

            if (!stampResult.Succeeded)
            {
                return new LockedUserResult
                {
                    IsSuccess = false,
                    Errors = stampResult.Errors.Select(e => e.Description).ToList()
                };
            }

            return new LockedUserResult
            {
                IsSuccess = true,
            };

        }//STANDARD
        public async Task<UnlockedUserResult> EnableUserAsync(string id)
        {
            var user = await _userManager.Users.Where(u => u.PublicId == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return new UnlockedUserResult
                {
                    IsSuccess = false,
                    Failure = UserLockoutStatus.UserNotFound
                };
            }

            var lockEndDate = await _userManager.SetLockoutEndDateAsync(user, null);

            if (!lockEndDate.Succeeded)
            {
                return new UnlockedUserResult
                {
                    IsSuccess = false,
                    Errors = lockEndDate.Errors.Select(e => e.Description).ToList()
                };
            }

            var accessFailedcount = await _userManager.ResetAccessFailedCountAsync(user);

            if (!accessFailedcount.Succeeded)
            {
                return new UnlockedUserResult
                {
                    IsSuccess = false,
                    Errors = accessFailedcount.Errors.Select(e => e.Description).ToList()
                };
            }

            var stampResult = await _userManager.UpdateSecurityStampAsync(user);

            if (!stampResult.Succeeded)
            {
                return new UnlockedUserResult
                {
                    IsSuccess = false,
                    Errors = stampResult.Errors.Select(e => e.Description).ToList()
                };
            }

            return new UnlockedUserResult
            {
                IsSuccess = true
            };
        }//STANDARD




    }
}
