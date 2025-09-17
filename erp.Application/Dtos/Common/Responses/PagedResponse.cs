namespace erp.Application.Dtos.Common.Responses;

public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int? Page,
    int? PageSize);