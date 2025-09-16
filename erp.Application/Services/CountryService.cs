using DevExpress.ExpressApp;
using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.Xpo;
using erp.Application.Dtos.Common;
using erp.Application.Helpers;
using erp.Application.Interfaces.Common;
using erp.Module.BusinessObjects.Common;

namespace erp.Application.Services;

public class CountryService(IDataService dataService) : ICountryService
{
    private readonly IObjectSpace _objectSpace = dataService.GetObjectSpace(typeof(Country));

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

    public CountryDto Add(CountryRequest request)
    {
        var newCountry = _objectSpace.CreateObject<Country>();
        newCountry.Name = request.Name?.Trim();
        _objectSpace.CommitChanges();
        return MapToCountryDto(newCountry);
    }

    public async Task<CountryDto?> Update(Guid oid, CountryRequest request)
    {
        var country = await _objectSpace.GetObjectsQuery<Country>(false).FirstOrDefaultAsync(x => x.Oid == oid);
        if (country == null) return null;
        country.Name = request.Name;
        _objectSpace.CommitChanges();
        return MapToCountryDto(country);
    }

    public async Task<bool> Delete(Guid oid)
    {
        var country = await _objectSpace.GetObjectsQuery<Country>().FirstOrDefaultAsync(x => x.Oid == oid);

        if (country == null)
            return false;

        _objectSpace.Delete(country);
        _objectSpace.CommitChanges();

        return true;
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