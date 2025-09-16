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
    [SwaggerOperation("Returns all Countries")]
    public async Task<ActionResult<List<CountryDto>>> GetAll(string? search)
    {
        var countries = await service.GetAll(search);
        return Ok(countries);
    }
}