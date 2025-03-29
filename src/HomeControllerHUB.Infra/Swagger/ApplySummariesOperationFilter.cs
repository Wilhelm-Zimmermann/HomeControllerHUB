using HomeControllerHUB.Shared.Utils;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Pluralize.NET;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HomeControllerHUB.Infra.Swagger;

public class ApplySummariesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerActionDescriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;

        if (controllerActionDescriptor == null) return;

        var pluralizer = new Pluralizer();

        var defaultDescriptions = GetDefaultDescriptions(
            controllerActionDescriptor.ActionName,
            operation.Parameters.Count(p => p.Name != "version" && p.Name != "api-version"),
            pluralizer.Singularize(controllerActionDescriptor.ControllerName),
            pluralizer.Pluralize(controllerActionDescriptor.ControllerName)
        );

        // if (defaultDescriptions.TryGetValue("summary", out string? summary) && !operation.Summary.HasValue())
        //     operation.Summary = summary;
        //
        // if (defaultDescriptions.TryGetValue("description", out string? description) && !operation.Parameters[0].Description.HasValue())
        //     operation.Parameters[0].Description = description;

    }

    #region Local Functions

    private Dictionary<string, string> GetDefaultDescriptions(string actionName, int parameterCount, string singularizeName, string pluralizeName)
    {
        var descriptions = new Dictionary<string, string>();

        if (IsGetAllAction(actionName, parameterCount, singularizeName, pluralizeName))
        {
            descriptions.Add("summary", $"Returns all {pluralizeName}");

        }
        else if (IsGetAllWithPaginationAction(actionName, parameterCount, singularizeName, pluralizeName))
        {
            descriptions.Add("summary", $"Returns all {pluralizeName} With Pagination");

        }
        else if (IsGetForSelectorsAction(actionName, parameterCount, singularizeName, pluralizeName))
        {
            descriptions.Add("summary", $"Returns {pluralizeName} for selectors");

        }
        else if (IsActionName(actionName, singularizeName, "Post", "Create"))
        {
            descriptions.Add("summary", $"Creates a {singularizeName}");
            descriptions.Add("description", $"A {singularizeName} representation");

        }
        else if (IsActionName(actionName, singularizeName, "Read", "Get"))
        {
            descriptions.Add("summary", $"Retrieves a {singularizeName} by unique id");
            descriptions.Add("description", $"a unique id for the {singularizeName}");

        }
        else if (IsActionName(actionName, singularizeName, "Put", "Edit", "Update"))
        {
            //if (!operation.Parameters[0].Description.HasValue())
            //    operation.Parameters[0].Description = $"A unique id for the {singularizeName}";

            descriptions.Add("summary", $"Updates a {singularizeName} by unique id");
            descriptions.Add("description", $"A {singularizeName} representation");

        }
        else if (IsActionName(actionName, singularizeName, "Delete", "Remove"))
        {
            descriptions.Add("summary", $"Deletes a {singularizeName} by unique id");
            descriptions.Add("description", $"A unique id for the {singularizeName}");

        }

        return descriptions;
    }

    private bool IsGetAllAction(string actionName, int parameterCount, string singularizeName, string pluralizeName)
    {
        foreach (var name in new[] { "Get", "Read", "Select" })
        {
            if ((actionName.Equals(name, StringComparison.OrdinalIgnoreCase) && parameterCount == 0) ||
                actionName.Equals($"{name}All", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}{pluralizeName}", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}All{singularizeName}", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}All{pluralizeName}", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsGetAllWithPaginationAction(string actionName, int parameterCount, string singularizeName, string pluralizeName)
    {
        foreach (var name in new[] { "Get", "Read", "Select" })
        {
            if ((actionName.Equals(name, StringComparison.OrdinalIgnoreCase) && parameterCount == 0) ||
                actionName.Equals($"{name}AllWithPagination", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}{pluralizeName}WithPagination", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}All{singularizeName}WithPagination", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}All{pluralizeName}WithPagination", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsGetForSelectorsAction(string actionName, int parameterCount, string singularizeName, string pluralizeName)
    {
        foreach (var name in new[] { "Get", "Read", "Select" })
        {
            if ((actionName.Equals(name, StringComparison.OrdinalIgnoreCase) && parameterCount == 0) ||
                actionName.Equals($"{name}ForSelectors", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}{pluralizeName}ForSelectors", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}All{singularizeName}ForSelectors", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}All{pluralizeName}ForSelectors", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsActionName(string actionName, string singularizeName, params string[] names)
    {
        foreach (var name in names)
        {
            if (actionName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}ById", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}{singularizeName}", StringComparison.OrdinalIgnoreCase) ||
                actionName.Equals($"{name}{singularizeName}ById", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}

