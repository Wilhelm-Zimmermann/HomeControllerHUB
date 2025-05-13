using System.Threading;
using System.Threading.Tasks;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Services;
using HomeControllerHUB.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Users.Commands.ConfirmEmail;

public record ConfirmEmailCommand(string Token) : IRequest<Unit>;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Unit>
{
    private readonly ApiUserManager _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _resource;
    
    public ConfirmEmailCommandHandler(
        ApiUserManager userManager,
        ISharedResource resource, ApplicationDbContext context)
    {
        _userManager = userManager;
        _resource = resource;
        _context = context;
    }
    
    public async Task<Unit> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.EmailConfirmationToken == request.Token);
        
        if (user == null)
            throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationUser)));
        
        if (user.EmailConfirmationToken != request.Token)
            throw new AppError(400, _resource.Message("InvalidToken"));
        
        // Confirm the email
        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        
        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AppError(400, errorMessage);
        }
        
        return Unit.Value;
    }
} 