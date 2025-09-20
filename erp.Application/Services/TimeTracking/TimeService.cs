using DevExpress.ExpressApp;
using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.Xpo;
using erp.Application.Dtos.TimeTracking.Requests;
using erp.Application.Dtos.TimeTracking.Responses;
using erp.Application.Interfaces.TimeTracking;
using erp.Module.BusinessObjects.TimeTracking;

namespace erp.Application.Services.TimeTracking;

public class TimeService(IDataService dataService) : ITime
{
    private readonly IObjectSpace _objectSpace = dataService.GetObjectSpace(typeof(TimesheetEntry));

    public async Task<ToggleResponse?> Toggle(ToggleRequest request, string userId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return new ToggleResponse("Invalid user");

        var activeEntry = await _objectSpace.GetObjectsQuery<TimesheetEntry>()
            .Where(e => e.Employee != null && e.Employee.Oid == userGuid && !e.EndOn.HasValue)
            .OrderByDescending(e => e.StartOn)
            .FirstOrDefaultAsync();

        if (activeEntry == null) return await Task.FromResult(new ToggleResponse($"Clock IN {userId}"));

        return await Task.FromResult(new ToggleResponse($"Clock OUT {userId}"));
    }
}