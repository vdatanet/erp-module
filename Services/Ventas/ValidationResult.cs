using System.Collections.Generic;
using System.Linq;

namespace erp.Module.Services.Ventas;

public record ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; init; } = new();

    public string ErrorMessage => string.Join("\n", Errors);

    public static ValidationResult Success() => new();

    public static ValidationResult Failure(string error) => new() { Errors = { error } };

    public static ValidationResult Failure(IEnumerable<string> errors) => new() { Errors = errors.ToList() };

    public void AddError(string error) => Errors.Add(error);
}
