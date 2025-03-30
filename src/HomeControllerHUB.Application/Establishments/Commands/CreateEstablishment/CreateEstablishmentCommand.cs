using AutoMapper;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;

namespace HomeControllerHUB.Application.Establishments.Commands.CreateEstablishment;

[Authorize(Domain = DomainNames.Establishment, Action = SecurityActionType.Create)]
public record CreateEstablishmentCommand : IRequest<BaseEntityResponse>
{
    public string? Name { get; set; }
    public string? SiteName { get; set; }
    public string Document { get; set; } = null!;
    public bool Enable { get; set; } = false;
    public bool IsMaster { get; set; } = false;
    public List<Guid>? UserIds { get; set; } = new List<Guid>();
}

public class CreateProfileCommandHandler : IRequestHandler<CreateEstablishmentCommand, BaseEntityResponse>
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

    public async Task<BaseEntityResponse> Handle(CreateEstablishmentCommand request, CancellationToken cancellationToken)
    {
        var establishment = new Establishment()
        {
            Name = request.Name,
            SiteName = request.SiteName,
            Document = request.Document,
            Enable = request.Enable,
            IsMaster = request.IsMaster
        };

        request.UserIds ??= new List<Guid>();
        var authUserId = new Guid(_currentUserService.UserId.ToString()!);
        if (!request.UserIds.Contains(authUserId)) 
        {
            request.UserIds.Add(authUserId);
        }

        foreach (var userId in request.UserIds)
        {
            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if(user == null) throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationUser)));

            var userEstablishment = new UserEstablishment()
            {
                Establishment = establishment,
                User = user,
            };

            _context.UserEstablishments.Add(userEstablishment);
        }

        _context.Establishments.Add(establishment);

        await _context.SaveChangesAsync(cancellationToken);

        return new BaseEntityResponse()
        {
            Id = establishment.Id,
        };
    }
}