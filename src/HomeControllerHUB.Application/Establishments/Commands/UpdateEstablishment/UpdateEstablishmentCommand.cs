
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
using Profile = AutoMapper.Profile;

namespace HomeControllerHUB.Application.Establishments.Commands.UpdateEstablishment;

[Authorize(Domain = DomainNames.Establishment, Action = SecurityActionType.Update)]
public record UpdateEstablishmentCommand : IRequest
{
    public Guid Id { get; init; }
    public string? Name { get; set; }
    public string? SiteName { get; set; }
    public string Document { get; set; } = null!;
    public bool Enable { get; set; } = false;
    public bool IsMaster { get; set; } = false;
    public List<Guid>? UserIds { get; set; } = new List<Guid>();
}

public class UpdateEstablishmentCommandHandler : IRequestHandler<UpdateEstablishmentCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _resource;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public UpdateEstablishmentCommandHandler(ApplicationDbContext context, ISharedResource resource, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _resource = resource;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateEstablishmentCommand request, CancellationToken cancellationToken)
    {
        var establishment = await _context.Establishments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (establishment == null) throw new AppError(404, _resource.NotFoundMessage(nameof(Establishment)));

        establishment.Document = request.Document;
        establishment.Enable = request.Enable;
        establishment.IsMaster = request.IsMaster;
        establishment.SiteName = request.SiteName;
        establishment.Name = request.Name;

        var usersToDelete = _context.UserEstablishments.Where(c => c.EstablishmentId == establishment.Id).ToList();
        _context.UserEstablishments.RemoveRange(usersToDelete);

        request.UserIds ??= new List<Guid>();
        var authUserId = new Guid(_currentUserService.UserId.ToString()!);
        if (!request.UserIds.Contains(authUserId))
        {
            request.UserIds.Add(authUserId);
        }

        foreach (var userId in request.UserIds)
        {
            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null) throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationMenu)));

            var userEstablishment = new UserEstablishment()
            {
                Establishment = establishment,
                User = user,
            };

            _context.UserEstablishments.Add(userEstablishment);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
