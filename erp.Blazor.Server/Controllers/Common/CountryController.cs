using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.Xpo;
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
    [HttpGet]
    [SwaggerOperation("Returns all Countries")]
    public async Task<ActionResult<List<ListItem>>> GetCountries(int page = 1, int pageSize = 20)
    {
        var objectSpace = dataService.GetObjectSpace(typeof(Country));
        
        IQueryable<Country> query = objectSpace.GetObjectsQuery<Country>()
            .OrderBy(c => c.Name);
        
        var totalCount = await query.CountAsync();
        
        if (page > 0)
        {
            query = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }
        else
        {
            page = 0;
            pageSize = totalCount;
        }
        
        var result = await query
            .Select(country => new ListItem
            {
                Oid = country.Oid.ToString(),
                Name = country.Name,
                Description = null
            })
            .ToListAsync();

        return Ok(new
        { 
            Items = result, 
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize 
        });
    }
}