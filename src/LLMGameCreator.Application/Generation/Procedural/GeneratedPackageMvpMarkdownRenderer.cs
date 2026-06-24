namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedPackageMvpMarkdownRenderer
{
    public string RenderReport(GeneratedPackageMvpReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var lines = new List<string>
        {
            "# Generated Package MVP v1",
            string.Empty,
            "This deterministic package MVP does not call an LLM, provider, Lua, Unity, media generator, or external runtime.",
            string.Empty,
            "## Metadata",
            string.Empty,
            $"- Package id: `{report.PackageId}`",
            $"- Package title: `{report.PackageTitle}`",
            $"- Package hash: `{report.PackageHash}`",
            $"- Stable summary: `{report.StableSummary}`",
            $"- Plan id: `{report.Source.PlanId}`",
            $"- Plan hash: `{report.Source.PlanHash}`",
            $"- Rule pack id: `{report.Source.RulePackId}`",
            $"- Rule pack hash: `{report.Source.RulePackHash}`",
            $"- Tiny loop state hash: `{report.Source.TinyLoopStateHash}`",
            $"- Seed: `{report.Source.Seed}`",
            $"- Mode: `{report.Source.Mode}`",
            string.Empty,
            "## Mapped Records",
            string.Empty
        };

        lines.AddRange(report.MappedRecords.Count == 0
            ? ["- None"]
            : report.MappedRecords.Select(record =>
                $"- `{record.SourceKind}` `{record.SourceId}` -> `{record.PackageKind}` `{record.PackageId}`: {record.MappingNote}"));

        lines.AddRange([string.Empty, "## Validation", string.Empty]);
        lines.AddRange(report.ValidationIssues.Count == 0
            ? ["- Package validator: clean"]
            : report.ValidationIssues.Select(issue =>
                $"- `{issue.Severity}` `{issue.Code}` target=`{issue.TargetId}` category=`{issue.Category}`: {issue.Message}"));

        lines.AddRange([string.Empty, "## Runtime Bootstrap", string.Empty]);
        lines.AddRange(FormatRuntime(report.RuntimeBootstrap));

        lines.AddRange([string.Empty, "## Diagnostics", string.Empty]);
        lines.AddRange(report.Diagnostics.Count == 0
            ? ["- None"]
            : report.Diagnostics.Select(diagnostic =>
                $"- `{diagnostic.Severity}` `{diagnostic.Code}` target=`{diagnostic.Target}`: {diagnostic.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    public string RenderRuntimeBootstrapReport(GeneratedPackageRuntimeBootstrapReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var lines = new List<string>
        {
            "# Generated Package Runtime Bootstrap Report v1",
            string.Empty
        };
        lines.AddRange(FormatRuntime(report));
        lines.AddRange([string.Empty, "## Diagnostics", string.Empty]);
        lines.AddRange(report.Diagnostics.Count == 0
            ? ["- None"]
            : report.Diagnostics.Select(diagnostic =>
                $"- `{diagnostic.Severity}` `{diagnostic.Code}` target=`{diagnostic.Target}`: {diagnostic.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    private static IReadOnlyList<string> FormatRuntime(GeneratedPackageRuntimeBootstrapReport report) =>
    [
        $"- Validation passed: `{report.ValidationPassed.ToString().ToLowerInvariant()}`",
        $"- Initial state created: `{report.InitialStateCreated.ToString().ToLowerInvariant()}`",
        $"- Map runtime started: `{report.MapRuntimeStarted.ToString().ToLowerInvariant()}`",
        $"- Move command succeeded: `{report.MoveCommandSucceeded.ToString().ToLowerInvariant()}`",
        $"- Interact command observed: `{report.InteractCommandObserved.ToString().ToLowerInvariant()}`",
        $"- Start map id: `{report.StartMapId}`",
        $"- Current map id: `{report.CurrentMapId}`",
        $"- Player entity id: `{report.PlayerEntityId}`",
        $"- Runtime summary: `{report.RuntimeSummary}`",
        $"- Event types: `{(report.EventTypes.Count == 0 ? "none" : string.Join(",", report.EventTypes))}`"
    ];
}
