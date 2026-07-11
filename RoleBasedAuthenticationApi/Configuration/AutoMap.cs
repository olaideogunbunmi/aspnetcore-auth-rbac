using AutoMapper;
using RoleBasedAuthenticationApi.DTO.Role;
using RoleBasedAuthenticationApi.DTO.User;
using RoleBasedAuthenticationApi.Models;
using Microsoft.AspNetCore.Identity;

namespace RoleBasedAuthenticationApi.Configuration
{
    public class AutoMap : Profile
    {
        public AutoMap()
        {
            CreateMap<ApplicationUser, UserDetailsDto>().ForMember(n => n.Id, opt => opt.MapFrom(x => x.PublicId)); //for users output 

            CreateMap<UpdateUserDto, ApplicationUser>(); //for user input

            CreateMap<IdentityRole, RoleDto>(); //for roles output
            CreateMap<IdentityRole, UpdateRoleDto>().ForMember(n => n.NewName, opt => opt.MapFrom(x => x.Name)); //for role input
        }
    }
}
