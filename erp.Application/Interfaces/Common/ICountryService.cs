using erp.Application.Dtos.Common;
using erp.Application.Dtos.Common.Requests;
using erp.Application.Dtos.Common.Responses;

namespace erp.Application.Interfaces.Common;

public interface ICountryService
{
    Task<ItemsResponse<CountryDto>> GetAll(string? search);
    Task<PagedResponse<CountryDto>> GetPaged(string? search, int? page, int? pageSize);
    Task<CountryDto?> GetByOid(Guid id);
    CountryDto Add(CountryRequest request);
    Task<CountryDto?> Update(Guid id, CountryRequest request);
    Task<bool> Delete(Guid id);
}