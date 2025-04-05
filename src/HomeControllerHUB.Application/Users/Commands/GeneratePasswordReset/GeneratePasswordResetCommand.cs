using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HomeControllerHUB.Application.Users.Commands.GeneratePasswordReset;

public record GeneratePasswordResetCommand : IRequest<Unit>
{
    public string Email { get; init; } = string.Empty;
}

public class GeneratePasswordResetCommandHandler : IRequestHandler<GeneratePasswordResetCommand, Unit>
{
    private readonly ApiUserManager _userManager;
    private readonly IEmailService _emailService;
    private readonly ISharedResource _resource;
    
    public GeneratePasswordResetCommandHandler(
        ApiUserManager userManager,
        IEmailService emailService,
        ISharedResource resource)
    {
        _userManager = userManager;
        _emailService = emailService;
        _resource = resource;
    }
    
    public async Task<Unit> Handle(GeneratePasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
            throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationUser)));
        
        // Generate a password reset token
        string resetToken = GenerateToken();
        
        // Setting the token directly - in a real implementation you might want to use
        // UserManager's password reset token functionality
        user.PasswordConfirmationToken = resetToken;
        
        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AppError(400, errorMessage);
        }
        
        // Send password reset email
        await _emailService.SendPasswordResetAsync(user.Email, user.Name ?? "", resetToken);
        
        return Unit.Value;
    }
    
    private string GenerateToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[32];
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
} 