#nullable enable
using erp.Application.Dtos.Common;
using erp.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace erp.Blazor.Server.Controllers.Common;

[Authorize]
[ApiController]
[Route("api/api/countries")]
public class HeroesController(ICountryService service) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation("Returns all Countries")]
    public async Task<ActionResult<List<CountryDto>>> GetAll(string? search)
    {
        var countries = await service.GetAll(search);
        return Ok(countries);
    }

    // [HttpPost]
    // [SwaggerOperation("Creates a new Country")]
    // public async Task<ActionResult<ListItem>> NewCountry(NewCountryRequest req)
    // {
    //     var country = await _objectSpace
    //         .GetObjectsQuery<Country>()
    //         .FirstOrDefaultAsync(x => x.Name == req.Name);
    //
    //     if (country != null)
    //         return BadRequest($"Country with name '{req.Name}' already exists");
    //
    //     var newCountry = _objectSpace.CreateObject<Country>();
    //     newCountry.Name = req.Name;
    //     _objectSpace.CommitChanges();
    //
    //     return CreatedAtAction(nameof(GetByKey), new { id = newCountry.Oid.ToString() }, new ListItem
    //     {
    //         Oid = newCountry.Oid.ToString(),
    //         Name = newCountry.Name,
    //         Description = null
    //     });
    // }

    // [HttpGet]
    // [SwaggerOperation("Returns all Countries")]
    // public async Task<ActionResult<PagedResponse<ListItem>>> GetAll(string? search, int? page, int? pageSize)
    // {
    //     //if (pageSize <= 0) pageSize = 20;
    //     //if (pageSize > 1000) pageSize = 1000;
    //     //if (page <= 0) page = 1;
    //
    //     var query = _objectSpace.GetObjectsQuery<Country>();
    //
    //     query = query.ApplySearch(search, c => c.Name)
    //         .OrderBy(c => c.Name);
    //
    //     var totalCount = await query.CountAsync();
    //
    //     if (totalCount < 1) return NotFound();
    //
    //     query = query.ApplyPaging(page, pageSize);
    //
    //     var result = await query
    //         .Select(country => new ListItem
    //         {
    //             Oid = country.Oid.ToString(),
    //             Name = country.Name,
    //             Description = null
    //         })
    //         .ToListAsync();
    //
    //     return Ok(new PagedResponse<ListItem>
    //     {
    //         Items = result,
    //         TotalCount = totalCount,
    //         Page = page,
    //         PageSize = pageSize
    //     });
    // }

    // [HttpGet("{id}")]
    // [SwaggerOperation("Returns a Country by Key")]
    // public async Task<ActionResult<ListItem>> GetByKey(string id)
    // {
    //     var country = await _objectSpace.GetObjectsQuery<Country>()
    //         .Where(x => x.Oid.ToString() == id)
    //         .Select(hero => new ListItem
    //         {
    //             Oid = hero.Oid.ToString(),
    //             Name = hero.Name,
    //             Description = null
    //         })
    //         .FirstOrDefaultAsync();
    //
    //     if (country == null) return NotFound();
    //     return Ok(country);
    // }
    
    // [HttpPut]
    // [SwaggerOperation("Updates a Country")]
    // public async Task<IActionResult> UpdateTodoItem(UpdateCountryRequest req)
    // {
    //     var result = await _objectSpace.GetObjectsQuery<Country>()
    //         .Where(x => x.Oid.ToString() == req.Oid)
    //         .FirstOrDefaultAsync();
    //
    //     if (result == null) return NotFound();
    //
    //     result.Name = req.Name;
    //     _objectSpace.CommitChanges();
    //
    //     return NoContent();
    // }
    
    // [HttpDelete("{id}")]
    // [SwaggerOperation("Deletes a Country by Key")]
    // public async Task<IActionResult> DeleteByKey(string id)
    // {
    //     var result = await _objectSpace.GetObjectsQuery<Country>()
    //         .Where(x => x.Oid.ToString() == id)
    //         .FirstOrDefaultAsync();
    //     if (result == null) return NotFound();
    //
    //     _objectSpace.Delete(result);
    //     _objectSpace.CommitChanges();
    //
    //     return NoContent();
    // }
}