using DevExpress.ExpressApp.WebApi.Services;
using erp.Module.BusinessObjects.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace erp.Blazor.Server.Controllers.Common;

[Authorize]
[ApiController]
[Route("api/custom/[controller]")]
public class CountryController(IDataService dataService) : ControllerBase
{
    [HttpGet("GetCountries")]
    [SwaggerOperation("Returns all Countries")]
    public List<Country> GetCountries()
    {
        var objectSpace = dataService.GetObjectSpace(typeof(Country));
        var result = objectSpace.GetObjects<Country>().ToList();
        return result;
    }
}