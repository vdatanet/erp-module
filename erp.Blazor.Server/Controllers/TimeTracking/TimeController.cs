#nullable enable
using erp.Application.Interfaces.TimeTracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp.Blazor.Server.Controllers.TimeTracking;

[Authorize]
[ApiController]
[Route("api/v1/time")]
public class TimeController(ITime service) : ControllerBase
{

}