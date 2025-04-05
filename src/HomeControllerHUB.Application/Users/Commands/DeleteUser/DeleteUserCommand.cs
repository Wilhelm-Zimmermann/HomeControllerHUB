using System;
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

namespace HomeControllerHUB.Application.Users.Commands.DeleteUser;

[Authorize(Domain = DomainNames.User, Action = SecurityActionType.Delete)]
public record DeleteUserCommand(Guid Id) : IRequest<Unit>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _resource;
    
    public DeleteUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ISharedResource resource)
    {
        _userManager = userManager;
        _context = context;
        _resource = resource;
    }
    
    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        
        if (user == null)
            throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationUser)));
        
        // Remove related UserProfiles
        var userProfiles = await _context.UserProfiles
            .Where(p => p.UserId == request.Id)
            .ToListAsync(cancellationToken);
        
        _context.UserProfiles.RemoveRange(userProfiles);
        
        // Remove related UserEstablishments
        var userEstablishments = await _context.UserEstablishments
            .Where(e => e.UserId == request.Id)
            .ToListAsync(cancellationToken);
        
        _context.UserEstablishments.RemoveRange(userEstablishments);
        
        // Delete the user
        var result = await _userManager.DeleteAsync(user);
        
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AppError(400, errorMessage);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
} 