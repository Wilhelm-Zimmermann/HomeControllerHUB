using System;
using System.Collections.Generic;
using AutoMapper;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Users.Queries.GetUserList;

public class UserListDto : IMapFrom<ApplicationUser>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Login { get; set; }
    public string? Document { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool Enable { get; set; }
    public string? EstablishmentName { get; set; }
    public List<UserProfileDto>? UserProfiles { get; set; }
    
    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap<ApplicationUser, UserListDto>()
            .ForMember(d => d.EstablishmentName, opt => opt.MapFrom(s => s.Establishment != null ? s.Establishment.Name : null));
    }
}

public class UserProfileDto : IMapFrom<UserProfile>
{
    public Guid Id { get; set; }
    public string? ProfileName { get; set; }
    
    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap<UserProfile, UserProfileDto>()
            .ForMember(d => d.ProfileName, opt => opt.MapFrom(s => s.Profile != null ? s.Profile.Name : null));
    }
} 