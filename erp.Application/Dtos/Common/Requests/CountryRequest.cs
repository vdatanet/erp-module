using System.ComponentModel.DataAnnotations;

namespace erp.Application.Dtos.Common.Requests;

public class CountryRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}