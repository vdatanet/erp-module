#nullable enable
using DevExpress.ExpressApp.Security;
using erp.Application.Dtos.TimeTracking.Requests;
using erp.Application.Dtos.TimeTracking.Responses;
using erp.Application.Interfaces.TimeTracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp.Blazor.Server.Controllers.TimeTracking;

[Authorize]
[ApiController]
[Route("api/v1/time")]
public class TimeController(ITime service, ISecurityProvider securityProvider) : ControllerBase
{
    [HttpPost("clock-toggle")]
    public async Task<ActionResult<ToggleResponse?>> Toggle([FromBody] ToggleRequest request)
    {
        var userId = securityProvider.GetSecurity().UserId.ToString() ?? null;

        if (userId == null) return Unauthorized();
        
        var result = await service.Toggle(request, userId);
        
        return result is null
            ? Problem(
                title: "TODO Title.", 
                detail: "TODO Detail.", 
                statusCode: StatusCodes.Status500InternalServerError)
            : Ok(result);
    }
}