using System.Text;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup Group(
        string groupId,
        string displayName,
        string sourceGoalId,
        string sourceRoot,
        List<VisualWorldPreviewArtifactEntry> entries,
        IReadOnlyList<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var hasError = diagnostics.Any(item => item.Severity == "error");
        return new VisualWorldPreviewArtifactGroup
        {
            GroupId = groupId,
            DisplayName = displayName,
            SourceGoalId = sourceGoalId,
            SourceRootRelativePath = sourceRoot,
            Status = hasError || entries.Count == 0
                ? VisualWorldPreviewArtifactStatus.Failed
                : VisualWorldPreviewArtifactStatus.Passed,
            EntryCount = entries.Count,
            SvgEntryCount = entries.Count(item => !string.IsNullOrWhiteSpace(item.TextSvgPreviewPath)),
            Entries = entries.OrderBy(item => item.Id, StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics.OrderBy(item => item.Code, StringComparer.Ordinal).ToList()
        };
    }

    private static List<VisualWorldPreviewArtifactEntry> BuildCoreEntries(
        string projectRoot,
        string sourceRoot,
        string sourceGoalId,
        IReadOnlyList<(string FileName, string Kind)> files,
        IReadOnlyDictionary<string, string> ledger,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var entries = new List<VisualWorldPreviewArtifactEntry>();
        foreach (var file in files)
        {
            var relativePath = sourceRoot + "/" + file.FileName;
            var fullPath = Resolve(projectRoot, relativePath);
            var exists = File.Exists(fullPath);
            if (!exists)
            {
                diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                    "goal092.artifact.missing",
                    relativePath,
                    "Required visual world source artifact was not found."));
            }

            entries.Add(new VisualWorldPreviewArtifactEntry
            {
                Id = sourceGoalId + "." + Path.GetFileNameWithoutExtension(file.FileName),
                RelativePath = relativePath,
                ArtifactKind = file.Kind,
                SourceGoalId = sourceGoalId,
                Sha256 = exists ? HashFor(projectRoot, relativePath, ledger) : string.Empty,
                Status = exists
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = exists ? "artifact exists" : "artifact missing",
                SafeRatingMetadataSummary = "sourceArtifact=true"
            });
        }

        return entries;
    }

    private static void AddSvgEntry(
        string projectRoot,
        List<VisualWorldPreviewArtifactEntry> entries,
        List<VisualWorldPreviewSvgEntry> svgEntries,
        string sourceGoalId,
        string id,
        string relativePath,
        string artifactKind,
        string metadataSummary,
        IReadOnlyDictionary<string, string> ledger,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(relativePath))
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.svg.invalid_catalog_entry",
                sourceGoalId,
                "SVG catalog entry must have an id and relative path."));
            return;
        }

        var fullPath = Resolve(projectRoot, relativePath);
        var exists = File.Exists(fullPath);
        var text = exists ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        var safe = exists && IsSafeSvg(text);
        if (!exists)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.svg.missing",
                relativePath,
                "Text SVG preview declared by source catalog is missing."));
        }
        else if (!safe)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.svg.unsafe",
                relativePath,
                "Text SVG preview contains unsafe content for text display."));
        }

        var sha = exists ? HashFor(projectRoot, relativePath, ledger) : string.Empty;
        var entryId = sourceGoalId + "." + id;
        var preview = exists ? TruncatePreview(text) : string.Empty;
        entries.Add(new VisualWorldPreviewArtifactEntry
        {
            Id = entryId,
            RelativePath = relativePath,
            ArtifactKind = artifactKind,
            SourceGoalId = sourceGoalId,
            Sha256 = sha,
            Status = safe
                ? VisualWorldPreviewArtifactStatus.Passed
                : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = safe ? "safe text SVG preview" : "missing or unsafe text SVG",
            TextSvgPreviewPath = relativePath,
            SafeRatingMetadataSummary = metadataSummary,
            TextPreview = preview
        });
        svgEntries.Add(new VisualWorldPreviewSvgEntry
        {
            EntryId = entryId,
            SourceGoalId = sourceGoalId,
            RelativePath = relativePath,
            Sha256 = sha,
            ByteLength = exists ? Encoding.UTF8.GetByteCount(text) : 0,
            SafeToDisplayAsText = safe,
            SafetySummary = safe
                ? "text SVG contains no script, external URL or base64 payload"
                : "missing or unsafe SVG",
            PreviewText = preview
        });
    }
}
