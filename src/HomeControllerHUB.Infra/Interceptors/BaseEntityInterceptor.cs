using System.Reflection;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HomeControllerHUB.Infra.Interceptors;

public class BaseEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public BaseEntityInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
    
    
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context == null) return;
        var userId = _currentUserService.UserId != null ? _currentUserService.UserId : Guid.Empty;
        var userName = _currentUserService.Login != null && _currentUserService.Login != "" ? _currentUserService.Login : Assembly.GetEntryAssembly().GetName().Name != "" ? Assembly.GetEntryAssembly().GetName().Name : "Not Defined";
        foreach (var entry in context.ChangeTracker.Entries<Base>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Created = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.Modified = DateTime.UtcNow;
            }
        }
    }
}