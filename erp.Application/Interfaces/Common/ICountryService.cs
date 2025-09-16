using erp.Application.Dtos.Common;

namespace erp.Application.Interfaces.Common;

public interface ICountryService
{
    Task<List<CountryDto>> GetAll(string? search);
    Task<PagedResponse<CountryDto>> GetPaged(string? search, int? page, int? pageSize);
    Task<CountryDto?> GetByOid(Guid id);
    Task<CountryDto> Add(CountryRequest request);
    Task<CountryDto?> Update(Guid id, CountryRequest request);
    Task<bool> Delete(Guid id);
}