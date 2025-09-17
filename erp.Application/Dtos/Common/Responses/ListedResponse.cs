namespace erp.Application.Dtos.Common.Responses;

public record ItemsResponse<T>(
    List<T> Items,
    int Count);