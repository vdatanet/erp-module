using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using erp.Application.Dtos.Common;
using erp.Application.Helpers;
using erp.Application.Interfaces.Common;
using erp.Module.BusinessObjects.Common;

namespace erp.Application.Services;

public class CountryService(IDataService dataService) : ICountryService
{
    private readonly XPObjectSpace _objectSpace = (XPObjectSpace)dataService.GetObjectSpace(typeof(Country));

    public async Task<List<CountryDto>> GetAll(string? search)
    {
        var countries = await BuildBaseQuery(search).ToListAsync();
        return countries.Select(MapToCountryDto).ToList();
    }

    public async Task<PagedResponse<CountryDto>> GetPaged(string? search, int? page, int? pageSize)
    {
        var (validPage, validPageSize) = PaginationHelper.ValidatePagination(page, pageSize);

        var skip = PaginationHelper.CalculateSkip(validPage, validPageSize);

        var baseQuery = BuildBaseQuery(search);

        var totalCount = await baseQuery.CountAsync();

        var pagedCountries = await BuildPagedQuery(baseQuery, skip, validPageSize).ToListAsync();

        var countries = pagedCountries.Select(MapToCountryDto).ToList();

        return new PagedResponse<CountryDto>(
            countries,
            totalCount,
            validPage,
            validPageSize
        );
    }

    public async Task<CountryDto?> GetByOid(Guid oid)
    {
        var hero = await _objectSpace.GetObjectsQuery<Country>(false).FirstOrDefaultAsync(x => x.Oid == oid);
        return hero != null ? MapToCountryDto(hero) : null;
    }

    public async Task<CountryDto> Add(CountryRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<CountryDto?> Update(Guid id, CountryRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    private IQueryable<Country> BuildBaseQuery(string? search)
    {
        var query = _objectSpace.GetObjectsQuery<Country>(false);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => !string.IsNullOrEmpty(x.Name) && x.Name.Contains(search));

        return query.OrderBy(x => x.Name);
    }

    private IQueryable<Country> BuildPagedQuery(IQueryable<Country> query, int skip, int take)
    {
        return query
            .Skip(skip)
            .Take(take);
    }

    private CountryDto MapToCountryDto(Country country)
    {
        return new CountryDto(
            country.Oid,
            country.Name
        );
    }
}