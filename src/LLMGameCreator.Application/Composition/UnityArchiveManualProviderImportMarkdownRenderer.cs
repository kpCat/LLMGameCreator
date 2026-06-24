namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveManualProviderImportMarkdownRenderer
{
    public string Render(UnityArchiveManualProviderImportResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var lines = new List<string>
        {
            "# Unity Archive Manual Provider Import v1",
            string.Empty,
            $"- Readiness: `{result.Readiness}`",
            $"- Imported: `{result.ImportedCount}`",
            $"- Already imported: `{result.SkippedCount}`",
            $"- Conflicts: `{result.ConflictCount}`",
            $"- Invalid or failed: `{result.InvalidCount}`",
            string.Empty,
            "## Entries",
            string.Empty
        };

        if (result.Entries.Count == 0)
        {
            lines.Add("- None");
        }
        else
        {
            lines.AddRange(result.Entries.Select(entry =>
                $"- `{entry.SlotId}`: `{entry.Status}`; provider=`{entry.ProviderKind}`; source=`{entry.SourceRelativePath}`; target=`{entry.ExpectedOutputRelativePath}`; bytes=`{entry.FileSizeBytes}`; sha256=`{entry.ContentSha256}`"));
        }

        lines.AddRange([string.Empty, "## Diagnostics", string.Empty]);
        if (result.Diagnostics.Count == 0)
        {
            lines.Add("- None");
        }
        else
        {
            lines.AddRange(result.Diagnostics.Select(diagnostic =>
                $"- `{diagnostic.Severity}` `{diagnostic.Code}` slot=`{diagnostic.SlotId}`: {diagnostic.Message}"));
        }

        lines.AddRange([string.Empty, "## Written archive-relative paths", string.Empty]);
        if (result.WrittenRelativePaths.Count == 0)
        {
            lines.Add("- None");
        }
        else
        {
            lines.AddRange(result.WrittenRelativePaths.Select(path => $"- `{path}`"));
        }

        return string.Join("\n", lines) + "\n";
    }
}
