#nullable enable

using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.Xpo;
using erp.Blazor.Server.DTOs.Common.Response;
using erp.Blazor.Server.Extensions;
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
    public async Task<ActionResult<PagedResponse<ListItem>>> GetCountries(string? search = null, int page = 1,
        int pageSize = 20)
    {
        var objectSpace = dataService.GetObjectSpace(typeof(Country));

        var query = objectSpace.GetObjectsQuery<Country>();

        if (!string.IsNullOrEmpty(search)) query = query.Where(c => c.Name.Contains(search));

        query = query.ApplyPaging(page, pageSize);

        var totalCount = await query.CountAsync();
        
        var result = await query
            .Select(country => new ListItem
            {
                Oid = country.Oid.ToString(),
                Name = country.Name,
                Description = null
            })
            .ToListAsync();

        return Ok(new PagedResponse<ListItem>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }
}