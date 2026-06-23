namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveReviewComparisonMarkdownRenderer
{
    public string Render(UnityArchiveReviewComparisonReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var lines = new List<string>
        {
            "# Unity Archive Review Comparison v1",
            string.Empty,
            $"- Current snapshot: `{report.CurrentSnapshotId}`",
            $"- Previous snapshot: `{report.PreviousSnapshotId}`",
            $"- Readiness: `{report.Readiness}`",
            string.Empty,
            "## Summary",
            string.Empty,
            $"- Source file count delta: `{report.Summary.SourceFileCountDelta}`",
            $"- Diagnostic count delta: `{report.Summary.DiagnosticCountDelta}`",
            $"- Error count delta: `{report.Summary.ErrorCountDelta}`",
            $"- Warning count delta: `{report.Summary.WarningCountDelta}`",
            $"- Info count delta: `{report.Summary.InfoCountDelta}`",
            $"- Invalid output count delta: `{report.Summary.InvalidOutputCountDelta}`",
            string.Empty,
            "### Provider counts",
            string.Empty,
            $"- Asset slots: `{report.Summary.AssetSlotCount}`",
            $"- Audio slots: `{report.Summary.AudioSlotCount}`",
            $"- Lua module slots: `{report.Summary.LuaModuleSlotCount}`",
            $"- Provider jobs: `{report.Summary.ProviderJobCount}`",
            string.Empty,
            "### Request counts",
            string.Empty,
            $"- Asset requests: `{report.Summary.AssetRequestCount}`",
            $"- Audio requests: `{report.Summary.AudioRequestCount}`",
            $"- Lua module requests: `{report.Summary.LuaModuleRequestCount}`"
        };

        if (report.Deltas.Count > 0)
        {
            lines.AddRange(
            [
                string.Empty,
                "## Readiness deltas",
                string.Empty
            ]);
            lines.AddRange(report.Deltas.Select(d => $"- `{d.Dimension}`: `{d.Previous}` -> `{d.Current}`"));
        }

        if (report.DiagnosticChanges.Count > 0)
        {
            lines.AddRange(
            [
                string.Empty,
                "## Diagnostic changes",
                string.Empty
            ]);
            lines.AddRange(report.DiagnosticChanges.Select(d =>
                $"- `{d.Change}` `{d.Severity}` `{d.Code}` in `{d.SourceFile}` target=`{d.TargetId}`: {d.Message}"));
        }

        if (report.SourceFileChanges.Count > 0)
        {
            lines.AddRange(
            [
                string.Empty,
                "## Source file changes",
                string.Empty
            ]);
            lines.AddRange(report.SourceFileChanges.Select(f => $"- `{f.Change}` `{f.RelativePath}` ({f.Kind})"));
        }

        if (report.InvalidReasonChanges.Count > 0)
        {
            lines.AddRange(
            [
                string.Empty,
                "## Invalid reason changes",
                string.Empty
            ]);
            lines.AddRange(report.InvalidReasonChanges.Select(r => $"- `{r.Reason}`: `{r.PreviousCount}` -> `{r.CurrentCount}`"));
        }

        return string.Join("\n", lines) + "\n";
    }
}