using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Users.Queries.GetUserList;

public class UserListDto : IMapFrom<ApplicationUser>, IPaginatedDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Login { get; set; }
    public string? Document { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool Enable { get; set; }
    public Guid EstablishmentId { get; set; }
    public string? EstablishmentName { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Modified { get; set; }
    public List<Guid> ProfileIds { get; set; } = [];
    public List<UserProfileDto>? UserProfiles { get; set; }
    
    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap<ApplicationUser, UserListDto>()
            .ForMember(d => d.EstablishmentName, opt => opt.MapFrom(s => s.Establishment != null ? s.Establishment.Name : null))
            .ForMember(d => d.ProfileIds, opt => opt.MapFrom(s => s.UserProfiles != null ? s.UserProfiles.Select(up => up.ProfileId).ToList() : new List<Guid>()));
    }
}

public class UserProfileDto : IMapFrom<UserProfile>
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public string? ProfileName { get; set; }
    
    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap<UserProfile, UserProfileDto>()
            .ForMember(d => d.ProfileName, opt => opt.MapFrom(s => s.Profile != null ? s.Profile.Name : null));
    }
} 
