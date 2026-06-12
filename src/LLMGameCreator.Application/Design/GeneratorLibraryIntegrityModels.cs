namespace LLMGameCreator.Application.Design;

public sealed record GeneratorLibraryIntegrityReport(
    string? LibraryRoot,
    string? RepositoryRoot,
    GeneratorLibraryIntegritySummary Summary,
    IReadOnlyList<GeneratorLibraryIntegrityIssue> Issues)
{
    public bool HasErrors => Issues.Any(issue => issue.Severity == GeneratorLibraryIntegritySeverity.Error);
}

public sealed record GeneratorLibraryIntegritySummary(
    int ManifestCount,
    int ModuleCount,
    int CapabilityCount,
    int FileCount,
    int ErrorCount,
    int WarningCount,
    int InfoCount,
    int DuplicateCapabilityCount);

public sealed record GeneratorLibraryIntegrityIssue(
    GeneratorLibraryIntegritySeverity Severity,
    string Code,
    string Message,
    string Target,
    string? ManifestPath,
    string? SuggestedFix);

public enum GeneratorLibraryIntegritySeverity
{
    Info,
    Warning,
    Error
}
