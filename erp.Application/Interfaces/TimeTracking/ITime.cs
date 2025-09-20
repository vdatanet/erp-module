using erp.Application.Dtos.Common.Requests;
using erp.Application.Dtos.Common.Responses;
using erp.Application.Dtos.TimeTracking.Requests;
using erp.Application.Dtos.TimeTracking.Responses;

namespace erp.Application.Interfaces.TimeTracking;

public interface ITime
{
    Task<ToggleResponse?> Toggle(ToggleRequest request);
}