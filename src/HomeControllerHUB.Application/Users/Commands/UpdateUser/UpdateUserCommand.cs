using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Users.Commands.UpdateUser;

[Authorize(Domain = DomainNames.User, Action = SecurityActionType.Update)]
public record UpdateUserCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Login { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
    public Guid EstablishmentId { get; init; }
    public IList<Guid>? UserEstablishmentsIds { get; init; }
    public IList<Guid>? UserProfilesIds { get; init; }
    public bool Enable { get; init; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _resource;
    
    public UpdateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ISharedResource resource)
    {
        _userManager = userManager;
        _context = context;
        _resource = resource;
    }
    
    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        
        if (user == null)
            throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationUser)));

        var userWithLoginExists = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login && u.Id != request.Id, cancellationToken);

        if (userWithLoginExists != null)
            throw new AppError(400, _resource.AlreadyExistsMessage(nameof(ApplicationUser)));
        
        user.Name = request.Name;
        user.Email = request.Email;
        user.Login = request.Login;
        user.UserName = request.Login;
        user.Document = request.Document;
        user.EstablishmentId = request.EstablishmentId;
        user.Enable = request.Enable;
        
        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AppError(400, errorMessage);
        }
        
        // Update UserProfiles if provided
        if (request.UserProfilesIds != null)
        {
            // Remove existing profiles
            var existingProfiles = await _context.UserProfiles
                .Where(p => p.UserId == request.Id)
                .ToListAsync(cancellationToken);
            
            _context.UserProfiles.RemoveRange(existingProfiles);
            
            // Add new profiles
            foreach (var profileId in request.UserProfilesIds)
            {
                var userProfile = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ProfileId = profileId,
                };
                
                _context.UserProfiles.Add(userProfile);
            }
        }
        
        // Update UserEstablishments if provided
        if (request.UserEstablishmentsIds != null)
        {
            // Remove existing establishments
            var existingEstablishments = await _context.UserEstablishments
                .Where(e => e.UserId == request.Id)
                .ToListAsync(cancellationToken);
            
            _context.UserEstablishments.RemoveRange(existingEstablishments);
            
            // Add new establishments
            foreach (var establishmentId in request.UserEstablishmentsIds)
            {
                var userEstablishment = new UserEstablishment
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    EstablishmentId = establishmentId,
                };
                
                _context.UserEstablishments.Add(userEstablishment);
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
} 