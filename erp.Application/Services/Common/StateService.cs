using DevExpress.ExpressApp;
using DevExpress.ExpressApp.WebApi.Services;
using erp.Application.Dtos.Common.Requests;
using erp.Application.Dtos.Common.Responses;
using erp.Application.Interfaces.Common;
using erp.Module.BusinessObjects.Common;

namespace erp.Application.Services.Common;

public class StateService(IDataService dataService) : IStateService
{
    private readonly IObjectSpace _objectSpace = dataService.GetObjectSpace(typeof(State));
    public Task<ItemsResponse<StateDto>> GetAll(string? search)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResponse<StateDto>> GetPaged(string? search, int? page, int? pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<StateDto?> GetByOid(Guid id)
    {
        throw new NotImplementedException();
    }

    public StateDto Add(StateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<StateDto?> Update(Guid id, StateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(Guid id)
    {
        throw new NotImplementedException();
    }
}