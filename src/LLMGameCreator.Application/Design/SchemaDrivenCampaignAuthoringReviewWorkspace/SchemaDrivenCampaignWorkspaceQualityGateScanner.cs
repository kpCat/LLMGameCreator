using System.Text;

namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

public sealed class SchemaDrivenCampaignWorkspaceQualityGateScanner
{
    private static readonly IReadOnlyList<string> ScanDirectories =
    [
        "src/LLMGameCreator.Application/Design/SchemaDrivenCampaignAuthoringReviewWorkspace",
        "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace",
        "tests/LLMGameCreator.Tests/Application/SchemaDrivenCampaignAuthoringReviewWorkspace"
    ];

    private static readonly IReadOnlyList<string> ScanFiles =
    [
        "src/LLMGameCreator.WinForms/CompositionRoot.cs",
        "tests/LLMGameCreator.Tests/ProductSmoke/SchemaDrivenCampaignAuthoringReviewWorkspaceProductSmokeTests.cs"
    ];

    public QualityGateScan Scan(string projectRoot)
    {
        var files = EnumerateFiles(projectRoot)
            .OrderBy(path => Relative(projectRoot, path), StringComparer.Ordinal)
            .Select(path => ScanFile(projectRoot, path))
            .ToList();
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        var linesOver500 = files.Sum(file => file.LinesOver500Count);
        var filesOver900 = files.Count(file => file.LineCount > 900);
        var minified = files.Count(file => file.MinifiedSourceCandidate);
        var tooFewLinesForSize = files.Count(file => file.TooFewLinesForSize);
        var compositionRootScanned = files.Any(file =>
            file.RelativePath == "src/LLMGameCreator.WinForms/CompositionRoot.cs");
        var alphaRoute = files.Any(file => file.RelativePath.Contains("AlphaRuntimeBootstrap.cs", StringComparison.Ordinal));

        if (linesOver500 > 0)
        {
            diagnostics.Add(CampaignWorkspaceDiagnostic.Error(
                "goal074.quality.line_over_500",
                "qualityGateScan.linesOver500Count",
                "Changed source files must not add >500 character lines."));
        }

        if (filesOver900 > 0)
        {
            diagnostics.Add(CampaignWorkspaceDiagnostic.Error(
                "goal074.quality.file_over_900_lines",
                "qualityGateScan.filesOver900LinesCount",
                "Changed source files must remain bounded."));
        }

        if (minified > 0)
        {
            diagnostics.Add(CampaignWorkspaceDiagnostic.Error(
                "goal074.quality.minified_source",
                "qualityGateScan.minifiedSourceFileCount",
                "Changed source files must not be minified."));
        }

        if (tooFewLinesForSize > 0)
        {
            diagnostics.Add(CampaignWorkspaceDiagnostic.Error(
                "goal074.quality.too_few_lines_for_size",
                "qualityGateScan.filesWithTooFewLinesForSizeCount",
                "Changed source files must keep enough line breaks for their byte size."));
        }

        if (!compositionRootScanned)
        {
            diagnostics.Add(CampaignWorkspaceDiagnostic.Error(
                "goal074.quality.composition_root_missing",
                "src/LLMGameCreator.WinForms/CompositionRoot.cs",
                "Goal 074 quality scan must include CompositionRoot.cs."));
        }

        if (alphaRoute)
        {
            diagnostics.Add(CampaignWorkspaceDiagnostic.Error(
                "goal074.quality.alpha_runtime_route",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs",
                "Goal 074 must not add a new Unity Alpha runtime route."));
        }

        return new QualityGateScan
        {
            Passed = diagnostics.Count == 0,
            ScannedFileCount = files.Count,
            MaxLineLength = files.Count == 0 ? 0 : files.Max(file => file.MaxLineLength),
            LinesOver500Count = linesOver500,
            FilesOver900LinesCount = filesOver900,
            MinifiedSourceFileCount = minified,
            FilesWithTooFewLinesForSizeCount = tooFewLinesForSize,
            CompositionRootScanned = compositionRootScanned,
            NewAlphaRuntimeBootstrapRoute = alphaRoute,
            Files = files,
            Diagnostics = diagnostics
        };
    }

    private static IEnumerable<string> EnumerateFiles(string projectRoot)
    {
        foreach (var directory in ScanDirectories)
        {
            var full = Path.Combine(projectRoot, directory.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(full))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(full, "*.cs", SearchOption.AllDirectories))
            {
                yield return file;
            }
        }

        foreach (var file in ScanFiles)
        {
            var full = Path.Combine(projectRoot, file.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(full))
            {
                yield return full;
            }
        }
    }

    private static QualityGateFileScan ScanFile(string projectRoot, string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None);
        var lineLengths = lines.Select(line => line.Length).ToList();
        var byteCount = Encoding.UTF8.GetByteCount(text);
        var minimumExpectedLineCount = Math.Max(2, byteCount / 300);
        var tooFewLinesForSize = byteCount >= 1_500 && lines.Length < minimumExpectedLineCount;
        return new QualityGateFileScan
        {
            RelativePath = Relative(projectRoot, path),
            LineCount = lines.Length,
            ByteCount = byteCount,
            MinimumExpectedLineCount = minimumExpectedLineCount,
            MaxLineLength = lineLengths.Count == 0 ? 0 : lineLengths.Max(),
            LinesOver500Count = lineLengths.Count(length => length > 500),
            TooFewLinesForSize = tooFewLinesForSize,
            MinifiedSourceCandidate = lines.Length <= 1 || lineLengths.Any(length => length > 500) || tooFewLinesForSize
        };
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');
}
