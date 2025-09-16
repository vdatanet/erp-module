#nullable enable
using erp.Application.Dtos.Common;
using erp.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace erp.Blazor.Server.Controllers.Common;

[Authorize]
[ApiController]
[Route("api/v1/countries")]
public class CountriesController(ICountryService service) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation("Returns all countries with search option")]
    public async Task<ActionResult<List<CountryDto>>> GetAll(string? search)
    {
        var countries = await service.GetAll(search);
        return Ok(countries);
    }
    
    [HttpGet("paged")]
    [SwaggerOperation("Returns all countries with search option and pagination")]
    public async Task<ActionResult<PagedResponse<CountryDto>>> GetPaged(string? search, int? page, int? pageSize)
    {
        var countries = await service.GetPaged(search, page, pageSize);
        return Ok(countries);
    }
    
    [HttpGet("{oid:guid}")]
    [SwaggerOperation("Returns a country by its Oid")]
    public async Task<ActionResult<CountryDto>> GetByOid(Guid oid)
    {
        var country = await service.GetByOid(oid);
        return Ok(country);
    }
    
    [HttpPost]
    [SwaggerOperation("Creates a new country")]
    public async Task<ActionResult<CountryDto>> Add(CountryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var country = await service.Add(request);
        return CreatedAtAction(nameof(GetByOid), new { oid = country.Oid }, country);
    }
    
    [HttpPut("{oid:guid}")]
    [SwaggerOperation("Updates a country")]
    public async Task<ActionResult<CountryDto>> Update(Guid oid, CountryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var country = await service.Update(oid, request);
        return country == null ? NotFound("Country not found") : Ok(country);
    }
    
    [HttpDelete("{oid:guid}")]
    [SwaggerOperation("Deletes a country")]
    public async Task<ActionResult<bool>> Delete(Guid oid)
    {
        var country = await service.Delete(oid);
        return country ? Ok() : NotFound("Country not found");
    }
}