using AutoMapper;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Profile = HomeControllerHUB.Domain.Entities.Profile;

namespace HomeControllerHUB.Application.Profiles.Commands.CreateProfile;

[Authorize(Domain = DomainNames.Profile, Action = SecurityActionType.Create)]
public record CreateProfileCommand : IRequest<BaseEntityResponse>
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool Enable { get; set; }
    public List<Guid>? PrivilegeIds { get; set; }
}

public class CreateProfileCommandHandler : IRequestHandler<CreateProfileCommand, BaseEntityResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISharedResource _resource;
    private readonly ICurrentUserService _currentUserService;

    public CreateProfileCommandHandler(ApplicationDbContext context, IMapper mapper, ISharedResource resource, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _resource = resource;
        _currentUserService = currentUserService;
    }

    public async Task<BaseEntityResponse> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        var establishmentId = _currentUserService.EstablishmentId;

        var profile = new Profile
        {
            EstablishmentId = establishmentId,
            Name = request.Name,
            Description = request.Description,
            Enable = request.Enable,
        };

        foreach (var privilegeId in request.PrivilegeIds?.Distinct() ?? Enumerable.Empty<Guid>())
        {
            var priv = await _context.Privilege.FirstOrDefaultAsync(p => p.Id == privilegeId && p.EstablishmentId == establishmentId, cancellationToken);
            if(priv == null) throw new AppError(404, _resource.NotFoundMessage(nameof(Privilege)));

            var profilePrivilege = new ProfilePrivilege()
            {
                Profile = profile,
                PrivilegeId = privilegeId,
            };

            _context.ProfilePrivileges.Add(profilePrivilege);
        }

        _context.Profiles.Add(profile);

        await _context.SaveChangesAsync(cancellationToken);

        return new BaseEntityResponse
        {
            Id = profile.Id,
        };
    }
}
