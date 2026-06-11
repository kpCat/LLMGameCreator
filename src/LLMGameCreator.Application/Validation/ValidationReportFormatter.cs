using System.Text;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Validation;

public sealed class ValidationReportFormatter
{
    public string Format(ValidationReport report)
    {
        if (report.Issues.Count == 0)
        {
            return "Ошибок не найдено.";
        }

        var builder = new StringBuilder();
        foreach (var severityGroup in report.Issues
            .OrderBy(issue => GetSeverityOrder(issue.Severity))
            .ThenBy(issue => issue.Severity.ToString(), StringComparer.Ordinal)
            .GroupBy(issue => issue.Severity))
        {
            AppendLine(builder, severityGroup.Key.ToString());

            foreach (var categoryGroup in severityGroup
                .OrderBy(issue => GetCategory(issue), StringComparer.Ordinal)
                .ThenBy(issue => issue.Code, StringComparer.Ordinal)
                .ThenBy(issue => issue.TargetId ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(issue => issue.TargetPath ?? issue.FilePath ?? string.Empty, StringComparer.Ordinal)
                .GroupBy(GetCategory))
            {
                AppendLine(builder, $"  {categoryGroup.Key}");

                foreach (var issue in categoryGroup)
                {
                    AppendLine(builder, $"    {FormatIssue(issue)}");
                }
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatIssue(ValidationIssue issue)
    {
        var target = FormatTarget(issue);
        if (target.Length == 0)
        {
            return $"{issue.Code}: {issue.Message}";
        }

        return $"{issue.Code} [{target}]: {issue.Message}";
    }

    private static string FormatTarget(ValidationIssue issue)
    {
        var path = issue.TargetPath ?? issue.FilePath;
        if (!string.IsNullOrWhiteSpace(issue.TargetId) && !string.IsNullOrWhiteSpace(path))
        {
            return $"{issue.TargetId}; {path}";
        }

        if (!string.IsNullOrWhiteSpace(issue.TargetId))
        {
            return issue.TargetId;
        }

        return string.IsNullOrWhiteSpace(path) ? string.Empty : path;
    }

    private static string GetCategory(ValidationIssue issue)
    {
        return string.IsNullOrWhiteSpace(issue.Category) ? "General" : issue.Category;
    }

    private static int GetSeverityOrder(ValidationSeverity severity)
    {
        return severity switch
        {
            ValidationSeverity.Critical => 0,
            ValidationSeverity.Error => 1,
            ValidationSeverity.Warning => 2,
            ValidationSeverity.Info => 3,
            _ => 4
        };
    }

    private static void AppendLine(StringBuilder builder, string text)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }

        builder.Append(text);
    }
}
