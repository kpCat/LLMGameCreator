using System.Text;

namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignEditQualityGateScanner
{
    private static readonly IReadOnlyList<string> ScanDirectories =
    [
        "src/LLMGameCreator.Application/Design/SchemaDrivenCampaignEditValidateApplyLoop",
        "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace",
        "tests/LLMGameCreator.Tests/Application/SchemaDrivenCampaignEditValidateApplyLoop"
    ];

    private static readonly IReadOnlyList<string> ScanFiles =
    [
        "src/LLMGameCreator.WinForms/CompositionRoot.cs",
        "tests/LLMGameCreator.Tests/ProductSmoke/SchemaDrivenCampaignEditValidateApplyLoopProductSmokeTests.cs"
    ];

    public CampaignEditQualityGateScan Scan(string projectRoot)
    {
        var files = EnumerateFiles(projectRoot)
            .OrderBy(path => Relative(projectRoot, path), StringComparer.Ordinal)
            .Select(path => ScanFile(projectRoot, path))
            .ToList();
        var diagnostics = new List<CampaignEditDiagnostic>();
        var linesOver500 = files.Sum(file => file.LinesOver500Count);
        var over1000 = files.Count(file => file.LineCount > 1000);
        var minified = files.Count(file => file.MinifiedSourceCandidate);
        var compositionRootScanned = files.Any(file =>
            file.RelativePath == "src/LLMGameCreator.WinForms/CompositionRoot.cs");
        var goal074WinFormsScanned = files.Any(file =>
            file.RelativePath.StartsWith(
                "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/",
                StringComparison.Ordinal));
        var reportOnlyTestDetected = DetectReportOnlyTests(projectRoot);

        if (linesOver500 > 0)
        {
            diagnostics.Add(Error(
                "goal075.quality.line_over_500",
                "qualityGateScan.linesOver500Count",
                "Changed or new C# files must not have lines over 500 characters."));
        }

        if (over1000 > 0)
        {
            diagnostics.Add(Error(
                "goal075.quality.file_over_1000_lines",
                "qualityGateScan.filesOver1000LinesCount",
                "New Goal 075 files must stay below 1000 lines."));
        }

        if (minified > 0)
        {
            diagnostics.Add(Error(
                "goal075.quality.minified_source",
                "qualityGateScan.minifiedSourceFileCount",
                "Changed or new C# files must not be one-line/minified files."));
        }

        if (!compositionRootScanned)
        {
            diagnostics.Add(Error(
                "goal075.quality.composition_root_missing",
                "src/LLMGameCreator.WinForms/CompositionRoot.cs",
                "Goal 075 quality scan must include CompositionRoot.cs."));
        }

        if (!goal074WinFormsScanned)
        {
            diagnostics.Add(Error(
                "goal075.quality.goal074_winforms_missing",
                "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace",
                "Goal 075 quality scan must include the touched Goal 074 WinForms surface."));
        }

        if (reportOnlyTestDetected)
        {
            diagnostics.Add(Error(
                "goal075.quality.report_only_test_detected",
                "tests/LLMGameCreator.Tests",
                "Goal 075 tests must not only assert report passed flags."));
        }

        return new CampaignEditQualityGateScan
        {
            Passed = diagnostics.Count == 0,
            ScannedFileCount = files.Count,
            MaxLineLength = files.Count == 0 ? 0 : files.Max(file => file.MaxLineLength),
            LinesOver500Count = linesOver500,
            FilesOver1000LinesCount = over1000,
            MinifiedSourceFileCount = minified,
            CompositionRootScanned = compositionRootScanned,
            Goal074WinFormsFilesScanned = goal074WinFormsScanned,
            ReportOnlyTestDetected = reportOnlyTestDetected,
            Files = files,
            Diagnostics = diagnostics
        };
    }

    public WinFormsEditBindingInventory BuildWinFormsBindingInventory(string projectRoot)
    {
        var groups = new[]
        {
            Group("row_selector", "CampaignRowSelectorControl"),
            Group("editable_field_summary", "CampaignEditFieldSummaryControl"),
            Group("validation_diagnostics", "CampaignEditValidationControl"),
            Group("apply_rollback_summary", "CampaignEditApplyRollbackControl")
        };
        var compositionRoot = Resolve(projectRoot, "src/LLMGameCreator.WinForms/CompositionRoot.cs");
        var pageDesigner = Resolve(
            projectRoot,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignAuthoringReviewWorkspacePageControl.Designer.cs");
        var compositionText = File.Exists(compositionRoot)
            ? File.ReadAllText(compositionRoot, Encoding.UTF8)
            : string.Empty;
        var pageText = File.Exists(pageDesigner) ? File.ReadAllText(pageDesigner, Encoding.UTF8) : string.Empty;
        var navigationRegistered = compositionText.Contains(
            "CampaignAuthoringReviewWorkspacePageControl",
            StringComparison.Ordinal)
            && pageText.Contains("CampaignEditValidateApplyLoopControl", StringComparison.Ordinal);
        var diagnostics = new List<CampaignEditDiagnostic>();

        foreach (var group in groups)
        {
            var path = Resolve(projectRoot, group.RelativePath);
            if (!File.Exists(path))
            {
                diagnostics.Add(Error("goal075.winforms.control_missing", group.RelativePath, "Required control is missing."));
            }
        }

        if (!navigationRegistered)
        {
            diagnostics.Add(Error(
                "goal075.winforms.navigation_missing",
                "CampaignAuthoringReviewWorkspacePageControl",
                "Goal 075 edit loop must be reachable from the review workspace page."));
        }

        return new WinFormsEditBindingInventory
        {
            Passed = diagnostics.Count == 0,
            NavigationRegistered = navigationRegistered,
            Groups = groups,
            Diagnostics = diagnostics
        };
    }

    private static WinFormsEditBindingGroup Group(string groupId, string controlName) =>
        new()
        {
            GroupId = groupId,
            ControlName = controlName,
            RelativePath = "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/" + controlName + ".cs",
            SeparateUserControl = true,
            BindsGoal075Data = true
        };

    private static IEnumerable<string> EnumerateFiles(string projectRoot)
    {
        foreach (var directory in ScanDirectories)
        {
            var full = Resolve(projectRoot, directory);
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
            var full = Resolve(projectRoot, file);
            if (File.Exists(full))
            {
                yield return full;
            }
        }
    }

    private static CampaignEditQualityFileScan ScanFile(string projectRoot, string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None);
        var lineLengths = lines.Select(line => line.Length).ToList();
        return new CampaignEditQualityFileScan
        {
            RelativePath = Relative(projectRoot, path),
            LineCount = lines.Length,
            ByteCount = Encoding.UTF8.GetByteCount(text),
            MaxLineLength = lineLengths.Count == 0 ? 0 : lineLengths.Max(),
            LinesOver500Count = lineLengths.Count(length => length > 500),
            MinifiedSourceCandidate = lines.Length <= 1 || lineLengths.Any(length => length > 500)
        };
    }

    private static bool DetectReportOnlyTests(string projectRoot)
    {
        var testRoot = Resolve(projectRoot, "tests/LLMGameCreator.Tests/Application/SchemaDrivenCampaignEditValidateApplyLoop");
        if (!Directory.Exists(testRoot))
        {
            return false;
        }

        foreach (var file in Directory.EnumerateFiles(testRoot, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file, Encoding.UTF8);
            if (text.Contains("ImplementationStatus", StringComparison.Ordinal)
                && !text.Contains("ApplyRollbackLedger", StringComparison.Ordinal)
                && !text.Contains("DiffMatrix", StringComparison.Ordinal)
                && !text.Contains("InvalidMatrix", StringComparison.Ordinal)
                && !text.Contains("RequiredArtifactNames", StringComparison.Ordinal)
                && !text.Contains("SourceArtifacts", StringComparison.Ordinal)
                && !text.Contains("PreviewExportRefreshPayload", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string Resolve(string projectRoot, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, path);
        return path;
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static void EnsureContained(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + normalizedPath);
        }
    }

    private static CampaignEditDiagnostic Error(string code, string target, string message) =>
        CampaignEditDiagnostic.Error(code, target, message);
}
