using HomeControllerHUB.Shared.Common.Constants;
using MediatR;

namespace HomeControllerHUB.Domain.Models;

public record PaginatedRequest<TResponse> : IRequest<PaginatedList<TResponse>>
{
    public string? SearchBy { get; init; }

    public string? OrderBy { get; init; }

    public int PageNumber { get; init; } = Constants.DefaultPageIndex;

    public int PageSize { get; init; } = Constants.DefaultPageSize;
}