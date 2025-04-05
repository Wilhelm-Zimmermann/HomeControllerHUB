using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HomeControllerHUB.Application.Users.Commands.ResetPasswordWithToken;

public record ResetPasswordWithTokenCommand : IRequest<Unit>
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public class ResetPasswordWithTokenCommandHandler : IRequestHandler<ResetPasswordWithTokenCommand, Unit>
{
    private readonly ApiUserManager _userManager;
    private readonly ISharedResource _resource;
    
    public ResetPasswordWithTokenCommandHandler(
        ApiUserManager userManager,
        ISharedResource resource)
    {
        _userManager = userManager;
        _resource = resource;
    }
    
    public async Task<Unit> Handle(ResetPasswordWithTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
            throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationUser)));
        
        // Verify token
        if (user.PasswordConfirmationToken != request.Token)
            throw new AppError(400, _resource.Message("InvalidToken"));
        
        // Reset password
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AppError(400, errorMessage);
        }
        
        // Clear the password reset token
        user.PasswordConfirmationToken = null;
        await _userManager.UpdateAsync(user);
        
        return Unit.Value;
    }
} 