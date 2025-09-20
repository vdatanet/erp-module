using DevExpress.ExpressApp;
using DevExpress.ExpressApp.WebApi.Services;
using erp.Application.Dtos.TimeTracking.Requests;
using erp.Application.Dtos.TimeTracking.Responses;
using erp.Application.Interfaces.TimeTracking;
using erp.Module.BusinessObjects.TimeTracking;

namespace erp.Application.Services.TimeTracking;

public class TimeService(IDataService dataService) : ITime
{
    private readonly IObjectSpace _objectSpace = dataService.GetObjectSpace(typeof(TimesheetEntry));
    
    public async Task<ToggleResponse?> Toggle(ToggleRequest request)
    {
        return await Task.FromResult(new ToggleResponse(request.Message));
    }
}