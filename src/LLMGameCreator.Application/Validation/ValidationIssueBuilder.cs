using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Validation;

internal static class ValidationIssueBuilder
{
    public static void Add(
        ValidationReport report,
        string code,
        ValidationSeverity severity,
        string message,
        string? targetId,
        string category,
        string? targetPath = null)
    {
        report.Issues.Add(new ValidationIssue
        {
            Code = code,
            Severity = severity,
            Message = message,
            TargetId = targetId,
            Category = category,
            TargetPath = targetPath,
            FilePath = targetPath
        });
    }

    public static void CheckDuplicates(
        ValidationReport report,
        IEnumerable<string> ids,
        string group,
        string category)
    {
        foreach (var duplicate in ids.Where(id => !string.IsNullOrWhiteSpace(id)).GroupBy(id => id).Where(g => g.Count() > 1))
        {
            Add(report, $"duplicate.{group}", ValidationSeverity.Error, $"Дублирующийся id в группе {group}: {duplicate.Key}", duplicate.Key, category);
        }
    }

    public static bool HasAnyText(IEnumerable<string> values)
    {
        return values.Any(value => !string.IsNullOrWhiteSpace(value));
    }
}
