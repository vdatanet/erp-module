#nullable enable
using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.Xpo;
using erp.Blazor.Server.DTOs.Common.Response;
using erp.Blazor.Server.Helpers;
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
    [HttpGet($"/")]
    [SwaggerOperation("Returns all Countries")]
    public async Task<ActionResult<List<ListItem>>> GetCountries(
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        var objectSpace = dataService.GetObjectSpace(typeof(Country));

        var query = objectSpace.GetObjectsQuery<Country>();
        
        query = query.ApplySearchFilter(search,
            c => c.Name
        );
         
        var result = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(country => new ListItem
            {
                Oid = country.Oid.ToString(),
                Name = country.Name,
                Description = null
            })
            .ToListAsync();

        return Ok(result);
    }
    
}