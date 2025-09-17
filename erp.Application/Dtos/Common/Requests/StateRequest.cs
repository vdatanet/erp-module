using System.ComponentModel.DataAnnotations;

namespace erp.Application.Dtos.Common.Requests;

public class StateRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}