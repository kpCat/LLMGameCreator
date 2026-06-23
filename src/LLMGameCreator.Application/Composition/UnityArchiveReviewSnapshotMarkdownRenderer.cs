namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveReviewSnapshotMarkdownRenderer
{
    public string Render(UnityArchiveReviewSnapshotReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var lines = new List<string>
        {
            "# Unity Archive Review Snapshot v1",
            string.Empty,
            $"- Readiness: `{report.Readiness}`",
            $"- Diagnostics: `{report.DiagnosticCount}` total, `{report.ErrorCount}` errors, `{report.WarningCount}` warnings, `{report.InfoCount}` info",
            $"- Source files: `{report.SourceFileCount}`",
            string.Empty,
            "## Validation",
            string.Empty,
            $"- export-validation.json present: `{report.Validation.ExportValidationPresent}`",
            $"- materialization readiness: `{report.Validation.MaterializationReadiness}`",
            $"- dry-run readiness: `{report.Validation.DryRunReadiness}`",
            $"- materialized files listed: `{report.Validation.MaterializedFileCount}`",
            string.Empty,
            "## Providers",
            string.Empty,
            $"- readiness-report.json present: `{report.Providers.ReadinessReportPresent}`",
            $"- provider plan readiness: `{report.Providers.Readiness}`",
            $"- asset slots: `{report.Providers.AssetSlotCount}`",
            $"- audio slots: `{report.Providers.AudioSlotCount}`",
            $"- Lua module slots: `{report.Providers.LuaModuleSlotCount}`",
            $"- provider jobs: `{report.Providers.ProviderJobCount}`",
            string.Empty,
            "### Provider batches",
            string.Empty
        };

        if (report.Providers.Batches.Count == 0)
        {
            lines.Add("- none");
        }
        else
        {
            lines.AddRange(report.Providers.Batches.Select(batch =>
                $"- `{batch.ProviderKind}`: `{batch.JobCount}` jobs, executionEnabled=`{batch.ExecutionEnabled}`"));
        }

        lines.AddRange(
        [
            string.Empty,
            "## Fulfillment",
            string.Empty,
            $"- fulfillment-state.json present: `{report.Fulfillment.FulfillmentStatePresent}`",
            $"- invalid-outputs.json present: `{report.Fulfillment.InvalidOutputsPresent}`",
            $"- total slots: `{report.Fulfillment.TotalSlotCount}`",
            $"- missing: `{report.Fulfillment.MissingCount}`",
            $"- available: `{report.Fulfillment.AvailableCount}`",
            $"- invalid: `{report.Fulfillment.InvalidCount}`",
            $"- invalid outputs: `{report.Fulfillment.InvalidOutputCount}`",
            string.Empty,
            "### Invalid reasons",
            string.Empty
        ]);

        if (report.Fulfillment.InvalidReasons.Count == 0)
        {
            lines.Add("- none");
        }
        else
        {
            lines.AddRange(report.Fulfillment.InvalidReasons.Select(reason =>
                $"- `{reason.Reason}`: `{reason.Count}`"));
        }

        lines.AddRange(
        [
            string.Empty,
            "## Requests",
            string.Empty,
            $"- asset requests: `{report.Requests.AssetRequestCount}`",
            $"- audio requests: `{report.Requests.AudioRequestCount}`",
            $"- Lua module requests: `{report.Requests.LuaModuleRequestCount}`",
            string.Empty,
            "## Diagnostics",
            string.Empty
        ]);

        if (report.Diagnostics.Count == 0)
        {
            lines.Add("- none");
        }
        else
        {
            lines.AddRange(report.Diagnostics.Select(diagnostic =>
                $"- `{diagnostic.Severity}` `{diagnostic.Code}` in `{diagnostic.SourceFile}` target=`{diagnostic.TargetId}`: {diagnostic.Message}"));
        }

        lines.AddRange(
        [
            string.Empty,
            "## Source files",
            string.Empty
        ]);

        if (report.SourceFiles.Count == 0)
        {
            lines.Add("- none");
        }
        else
        {
            lines.AddRange(report.SourceFiles.Select(file =>
                $"- `{file.RelativePath}` ({file.Kind})"));
        }

        return string.Join("\n", lines) + "\n";
    }
}
