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
    public ActionResult Toggle([FromBody] ToggleRequest request)
    {
        //var os = dataService.GetObjectSpace(typeof(ApplicationUser));
        //var session = ((XPObjectSpace)os).Session;

        //var user = os.GetObjectByKey<ApplicationUser>(req.EmployeeOid);
        //if (user is null) return NotFound("Empleado no encontrado");

        //Project? project = null;
        //ProjectActivity? activity = null;
        //if (req.ProjectOid.HasValue) project = os.GetObjectByKey<Project>(req.ProjectOid.Value);
        //if (req.ActivityOid.HasValue) activity = os.GetObjectByKey<ProjectActivity>(req.ActivityOid.Value);

        //var now = req.Timestamp ?? DateTime.Now;

        //var result = TimesheetHelper.ToggleClock(session, user, now, project, activity, req.Prefix);
        //os.CommitChanges();

        //return Ok(new
        //{
            //action = result.Type.ToString(),           // ClockIn | ClockOut
            //entryId = result.Entry.Oid,
            //startOn = result.Entry.StartOn,
            //endOn = result.Entry.EndOn,
            //dailyId = result.Daily.Oid,
            //date = result.Daily.Date
        //});
        
        return Ok(new ToggleResponse(
            "Hello there!"
            ));
    }
}