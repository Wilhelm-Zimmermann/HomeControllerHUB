using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HomeControllerHUB.Application.Users.Commands.CreateUser;

[Authorize(Domain = DomainNames.User, Action = SecurityActionType.Create)]
public record CreateUserCommand : IRequest<BaseEntityResponse>
{
    public string Login { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
    public Guid EstablishmentId { get; init; }
    public IList<Guid>? UserEstablishmentsIds { get; init; }
    public IList<Guid> UserProfilesIds { get; init; } = new List<Guid>();
};

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, BaseEntityResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    
    public CreateUserCommandHandler(
        UserManager<ApplicationUser> userManager, 
        ApplicationDbContext context,
        IEmailService emailService)
    {
        _userManager = userManager;
        _context = context;
        _emailService = emailService;
    }
    
    public async Task<BaseEntityResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        string emailConfirmationToken = GenerateToken();
        
        var user = new ApplicationUser()
        {
            Id = Guid.NewGuid(),
            Login = request.Login,
            Email = request.Email,
            UserName = request.Login,
            Name = request.Name,
            Document = request.Document,
            EstablishmentId = request.EstablishmentId,
            EmailConfirmed = false,
            EmailConfirmationToken = emailConfirmationToken,
            Enable = true
        };

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

        if (request.UserEstablishmentsIds?.Count > 0)
        {
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
        
        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (result.Succeeded)
        {
            await _context.SaveChangesAsync(cancellationToken);
            
            // Send confirmation email
            await _emailService.SendEmailConfirmationAsync(user.Email, user.Name, emailConfirmationToken);
            
            return new BaseEntityResponse
            {
                Id = user.Id,
            };
        }
        
        // If user creation failed, throw an error
        var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException($"Failed to create user: {errorMessage}");
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