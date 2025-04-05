using System.Threading;
using System.Threading.Tasks;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.Services;
using HomeControllerHUB.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Users.Commands.ConfirmEmail;

public record ConfirmEmailCommand(string Token, string Email) : IRequest<Unit>;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Unit>
{
    private readonly ApiUserManager _userManager;
    private readonly ISharedResource _resource;
    
    public ConfirmEmailCommandHandler(
        ApiUserManager userManager,
        ISharedResource resource)
    {
        _userManager = userManager;
        _resource = resource;
    }
    
    public async Task<Unit> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
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