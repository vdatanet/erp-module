#nullable enable
using erp.Application.Dtos.TimeTracking.Requests;
using erp.Application.Dtos.TimeTracking.Responses;
using erp.Application.Interfaces.TimeTracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp.Blazor.Server.Controllers.TimeTracking;

[Authorize]
[ApiController]
[Route("api/v1/time")]
public class TimeController(ITime service) : ControllerBase
{
    [HttpPost("clock-toggle")]
    public async Task<ActionResult<ToggleResponse?>> Toggle([FromBody] ToggleRequest request)
    {
        var result = await service.Toggle(request);
        return result is null
            ? Problem(
                title: "TODO Title.", 
                detail: "TODO Detail.", 
                statusCode: StatusCodes.Status500InternalServerError)
            : Ok(result);
    }
}