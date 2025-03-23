using System.Reflection;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Shared.Normalize;
using HomeControllerHUB.Shared.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HomeControllerHUB.Infra.Interceptors;

public class NormalizedInterceptor : SaveChangesInterceptor
{
    public NormalizedInterceptor()
    {
    }
    
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAttribute(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateAttribute(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateAttribute(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<Base>())
        {
            var type = entry.Entity.GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var normalizedAttribute = property.GetCustomAttribute<NormalizedAttribute>();

                if (normalizedAttribute != null)
                {
                    var normalizeProperty = type.GetProperties().Where(p => p.Name == normalizedAttribute.PropertyName).FirstOrDefault();

                    string value = null;

                    if (normalizeProperty.GetValue(entry.Entity) is not null)
                    {
                        value = StringExtensions.Normalize(normalizeProperty.GetValue(entry.Entity).ToString());
                    }

                    property.SetValue(entry.Entity, value);
                }
            }
        }
    }
}