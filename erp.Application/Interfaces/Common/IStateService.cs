using erp.Application.Dtos.Common.Requests;
using erp.Application.Dtos.Common.Responses;

namespace erp.Application.Interfaces.Common;

public interface IStateService
{
    Task<ItemsResponse<StateDto>> GetAll(string? search);
    Task<PagedResponse<StateDto>> GetPaged(string? search, int? page, int? pageSize);
    Task<StateDto?> GetByOid(Guid id);
    StateDto Add(StateRequest request);
    Task<StateDto?> Update(Guid id, StateRequest request);
    Task<bool> Delete(Guid id);
}