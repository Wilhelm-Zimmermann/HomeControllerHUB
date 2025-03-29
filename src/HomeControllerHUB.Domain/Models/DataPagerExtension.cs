using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Reflection;
using System.Text;
using System.Linq.Dynamic.Core;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Shared.Common.Constants;

namespace HomeControllerHUB.Domain.Models;

public static class DataPagerExtension
{
    public static async Task<PaginatedList<TModel>> PaginateAsync<TModel>(this IQueryable<TModel> source, PaginatedRequest<TModel> request, CancellationToken cancellationToken) where TModel : class, IPaginatedDto
    {
        var pageNumber = request.PageNumber < 0 ? 1 : request.PageNumber;
        var countItems = source.Count();

        if (source is IAsyncQueryProvider)
        {
            countItems = await source.CountAsync(cancellationToken);
        }

        var startRow = (pageNumber - 1) * request.PageSize;
        var result = ApplySort<TModel>(source, request.OrderBy).Skip(startRow).Take(request.PageSize).ToList();

        return new PaginatedList<TModel>(result, countItems, pageNumber, request.PageSize);
    }

    private static IQueryable<TModel> ApplySort<TModel>(IQueryable<TModel> query, string? orderByQueryString) where TModel : class
    {

        if (string.IsNullOrWhiteSpace(orderByQueryString) || !query.Any())
        {
            return query;
        }

        var orderParams = orderByQueryString.Trim().Split(Constants.Comma);
        var propertyInfos = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var orderQueryBuilder = new StringBuilder();

        foreach (var param in orderParams)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                continue;
            }

            var propertyFromQueryName = param.Split(Constants.WhiteSpace)[0];
            var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));
            if (objectProperty == null)
            {
                continue;
            }
            var sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";
            orderQueryBuilder.Append($"{objectProperty.Name} {sortingOrder}, ");
        }

        var orderQuery = orderQueryBuilder.ToString().TrimEnd(Constants.Comma[0], Constants.WhiteSpace[0]);
        return string.IsNullOrWhiteSpace(orderQuery) ? query : query.OrderBy(orderQuery);
    }
}