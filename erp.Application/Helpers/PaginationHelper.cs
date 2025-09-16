namespace erp.Application.Helpers;

public static class PaginationHelper
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;
    private const int MinPageSize = 1;
    private const int MinPage = 1;

    private static int ValidatePage(int? page)
    {
        return Math.Max(page ?? DefaultPage, MinPage);
    }

    private static int ValidatePageSize(int? pageSize)
    {
        return Math.Clamp(pageSize ?? DefaultPageSize, MinPageSize, MaxPageSize);
    }

    public static (int validPage, int validPageSize) ValidatePagination(int? page, int? pageSize)
    {
        return (ValidatePage(page), ValidatePageSize(pageSize));
    }

    public static int CalculateSkip(int validPage, int validPageSize)
    {
        return (validPage - 1) * validPageSize;
    }
}