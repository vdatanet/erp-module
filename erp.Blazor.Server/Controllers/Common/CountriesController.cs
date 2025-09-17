#nullable enable
using erp.Application.Dtos.Common.Requests;
using erp.Application.Dtos.Common.Responses;
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
    public async Task<ActionResult<ItemsResponse<CountryDto>>> GetAll(string? search)
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
    [SwaggerOperation("Returns a country by its oid")]
    public async Task<ActionResult<CountryDto>> GetByOid(Guid oid)
    {
        if (oid == Guid.Empty)
        {
            return BadRequest("Invalid country oid");
        }

        var country = await service.GetByOid(oid);

        return country == null ? NotFound("Country not found") : Ok(country);
    }

    [HttpPost]
    [SwaggerOperation("Creates a new country")]
    public ActionResult<CountryDto> Add(CountryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var country = service.Add(request);
        return CreatedAtAction(nameof(GetByOid), new { oid = country.Oid }, country);
    }

    [HttpPut("{oid:guid}")]
    [SwaggerOperation("Updates a country")]
    public async Task<ActionResult<CountryDto>> Update(Guid oid, CountryRequest request)
    {
        if (oid == Guid.Empty)
        {
            return BadRequest("Invalid country oid");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var country = await service.Update(oid, request);
        return country == null ? NotFound("Country not found") : Ok(country);
    }

    [HttpDelete("{oid:guid}")]
    [SwaggerOperation("Deletes a country")]
    public async Task<ActionResult> Delete(Guid oid)
    {
        var country = await service.Delete(oid);
        return country ? Ok() : NotFound("Country not found");
    }
}