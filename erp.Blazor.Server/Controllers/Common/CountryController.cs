using DevExpress.ExpressApp.WebApi.Services;
using erp.Blazor.Server.DTOs.Common.Response;
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
    public List<ListItem> GetCountries()
    {
        var objectSpace = dataService.GetObjectSpace(typeof(Country));
        
        var result = objectSpace.GetObjects<Country>()
            .Select(country => new ListItem 
            {
                Oid = country.Oid.ToString(),
                Name = country.Name,
                Description = null
            })
            .ToList();
        
        return result;
    }
}