namespace erp.Application.Dtos.Common;

public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int? Page,
    int? PageSize);