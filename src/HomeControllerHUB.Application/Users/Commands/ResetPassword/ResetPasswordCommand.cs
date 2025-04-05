using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.Services;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HomeControllerHUB.Application.Users.Commands.ResetPassword;

[Authorize(Domain = DomainNames.User, Action = SecurityActionType.Update)]
public record ResetPasswordCommand : IRequest<Unit>
{
    public Guid UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly ApiUserManager _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISharedResource _resource;
    
    public ResetPasswordCommandHandler(
        ApiUserManager userManager,
        ICurrentUserService currentUserService,
        ISharedResource resource)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _resource = resource;
    }
    
    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Only allow users to reset their own password unless they are an admin
        var currentUserId = _currentUserService.UserId;
        
        // Admin check would typically be done through roles or claims
        // Adjust this based on how your application handles admin privileges
        bool isAdmin = false; // This should be replaced with actual admin check
        
        if (currentUserId != request.UserId && !isAdmin)
            throw new AppError(403, _resource.Message("Unauthorized"));
        
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new AppError(404, _resource.NotFoundMessage(nameof(ApplicationUser)));
        
        // Verify current password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!isPasswordValid)
            throw new AppError(400, _resource.Message("InvalidPassword"));
        
        // Change password
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AppError(400, errorMessage);
        }
        
        return Unit.Value;
    }
} 