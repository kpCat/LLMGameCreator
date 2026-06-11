namespace LLMGameCreator.Domain.Validation;

public enum ValidationSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}

public sealed class ValidationIssue
{
    public string Code { get; set; } = string.Empty;
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
    public string Message { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public string? FilePath { get; set; }

    public override string ToString()
    {
        var target = string.IsNullOrWhiteSpace(TargetId) ? string.Empty : $" [{TargetId}]";
        return $"{Severity}: {Code}{target} - {Message}";
    }
}

public sealed class ValidationReport
{
    public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
    public bool IsValid => Issues.All(i => i.Severity != ValidationSeverity.Error && i.Severity != ValidationSeverity.Critical);
}
