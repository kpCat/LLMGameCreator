using System.Text;

namespace LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;

public sealed class EditDrivenPlayablePreviewRefreshQualityGateScanner
{
    private static readonly IReadOnlyList<string> ScanDirectories =
    [
        "src/LLMGameCreator.Application/Design/EditDrivenPlayablePreviewRefresh",
        "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace",
        "tests/LLMGameCreator.Tests/Application/EditDrivenPlayablePreviewRefresh"
    ];

    private static readonly IReadOnlyList<string> ScanFiles =
    [
        "src/LLMGameCreator.WinForms/CompositionRoot.cs",
        "tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenPlayablePreviewRefreshProductSmokeTests.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs"
    ];

    public EditDrivenQualityGateScan Scan(string projectRoot)
    {
        var files = EnumerateFiles(projectRoot)
            .OrderBy(path => Relative(projectRoot, path), StringComparer.Ordinal)
            .Select(path => ScanFile(projectRoot, path))
            .ToList();
        var diagnostics = new List<EditDrivenPlayablePreviewRefreshDiagnostic>();
        var linesOver500 = files.Sum(file => file.LinesOver500Count);
        var over1000 = files.Count(file =>
            file.LineCount > 1000
            && file.RelativePath != "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
        var minified = files.Count(file => file.MinifiedSourceCandidate);
        var binding = BuildWinFormsBindingInventory(projectRoot);
        var reportOnlySmoke = DetectReportOnlySmoke(projectRoot);
        var alphaLines = files
            .FirstOrDefault(file => file.RelativePath == "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs")
            ?.LineCount ?? 0;

        if (linesOver500 > 0)
        {
            diagnostics.Add(Error(
                "goal076.quality.line_over_500",
                "qualityGateScan.linesOver500Count",
                "Changed or scanned C# files must not have lines over 500 characters."));
        }

        if (over1000 > 0)
        {
            diagnostics.Add(Error(
                "goal076.quality.file_over_1000_lines",
                "qualityGateScan.filesOver1000LinesCount",
                "New Goal 076 source/test files must stay below 1000 lines."));
        }

        if (minified > 0)
        {
            diagnostics.Add(Error(
                "goal076.quality.minified_source",
                "qualityGateScan.minifiedSourceFileCount",
                "Goal 076 must not add one-line/minified source."));
        }

        if (!binding.Passed)
        {
            diagnostics.Add(Error(
                "goal076.quality.parent_ui_non_binding",
                "CampaignAuthoringReviewWorkspacePageControl",
                "Parent workspace must bind the Goal 076 playable refresh control through activation."));
        }

        if (reportOnlySmoke)
        {
            diagnostics.Add(Error(
                "goal076.quality.report_only_smoke",
                "EditDrivenPlayablePreviewRefreshProductSmokeTests",
                "Product smoke must verify behavior and negative handoff proof, not only report status."));
        }

        return new EditDrivenQualityGateScan
        {
            Passed = diagnostics.Count == 0,
            ScannedFileCount = files.Count,
            MaxLineLength = files.Count == 0 ? 0 : files.Max(file => file.MaxLineLength),
            LinesOver500Count = linesOver500,
            FilesOver1000LinesCount = over1000,
            MinifiedSourceFileCount = minified,
            AlphaRuntimeBootstrapLineCount = alphaLines,
            ParentUiBindingPassed = binding.Passed,
            ReportOnlySmokeDetected = reportOnlySmoke,
            Files = files,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public EditDrivenWinFormsBindingInventory BuildWinFormsBindingInventory(string projectRoot)
    {
        var group = new EditDrivenWinFormsBindingGroup
        {
            GroupId = "playable_refresh_status",
            ControlName = "CampaignPlayableRefreshControl",
            RelativePath = "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignPlayableRefreshControl.cs",
            SeparateUserControl = true,
            BindsGoal076Data = true
        };
        var pageDesigner = ReadOptional(
            projectRoot,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignAuthoringReviewWorkspacePageControl.Designer.cs");
        var pageCode = ReadOptional(
            projectRoot,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignAuthoringReviewWorkspacePageControl.cs");
        var compactPageCode = Compact(pageCode);
        var controlExists = File.Exists(Resolve(projectRoot, group.RelativePath));
        var tabDeclared = pageDesigner.Contains("_playableRefreshTabPage", StringComparison.Ordinal)
            && pageDesigner.Contains("_playableRefreshControl", StringComparison.Ordinal)
            && pageDesigner.Contains("CampaignPlayableRefreshControl", StringComparison.Ordinal);
        var serviceLoaded = pageCode.Contains("EditDrivenPlayablePreviewRefreshEvidenceService", StringComparison.Ordinal)
            && compactPageCode.Contains("_playableRefreshService.Build(root)", StringComparison.Ordinal);
        var controlBound = pageCode.Contains("EditDrivenPlayablePreviewRefreshBuildResult", StringComparison.Ordinal)
            && compactPageCode.Contains("_playableRefreshControl.Bind(refreshResult)", StringComparison.Ordinal);
        var activationBinds = tabDeclared && serviceLoaded && controlBound;
        var diagnostics = new List<EditDrivenPlayablePreviewRefreshDiagnostic>();

        if (!controlExists)
        {
            diagnostics.Add(Error("goal076.winforms.control_missing", group.RelativePath, "Required Goal 076 control is missing."));
        }

        if (!tabDeclared)
        {
            diagnostics.Add(Error(
                "goal076.winforms.refresh_tab_missing",
                "CampaignAuthoringReviewWorkspacePageControl.Designer.cs",
                "Parent workspace must declare a separate playable refresh tab/control."));
        }

        if (tabDeclared && !serviceLoaded)
        {
            diagnostics.Add(Error(
                "goal076.winforms.refresh_service_missing",
                "CampaignAuthoringReviewWorkspacePageControl.cs",
                "Parent workspace activation must load Goal 076 evidence service."));
        }

        if (tabDeclared && !controlBound)
        {
            diagnostics.Add(Error(
                "goal076.winforms.refresh_control_bind_missing",
                "CampaignAuthoringReviewWorkspacePageControl.cs",
                "Parent workspace must bind the Goal 076 result into CampaignPlayableRefreshControl."));
        }

        return new EditDrivenWinFormsBindingInventory
        {
            Passed = diagnostics.Count == 0,
            ParentPageRefreshTabDeclared = tabDeclared,
            ParentPageRefreshEvidenceServiceLoaded = serviceLoaded,
            ParentPageRefreshControlBound = controlBound,
            ParentPageActivationBindsGoal076Data = activationBinds,
            Groups = [group],
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<EditDrivenPlayablePreviewRefreshDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenPlayablePreviewRefreshDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

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

    private static EditDrivenQualityFileScan ScanFile(string projectRoot, string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None);
        var lineLengths = lines.Select(line => line.Length).ToList();
        return new EditDrivenQualityFileScan
        {
            RelativePath = Relative(projectRoot, path),
            LineCount = lines.Length,
            ByteCount = Encoding.UTF8.GetByteCount(text),
            MaxLineLength = lineLengths.Count == 0 ? 0 : lineLengths.Max(),
            LinesOver500Count = lineLengths.Count(length => length > 500),
            MinifiedSourceCandidate = lines.Length <= 1 || lineLengths.Any(length => length > 500)
        };
    }

    private static bool DetectReportOnlySmoke(string projectRoot)
    {
        var path = Resolve(projectRoot, "tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenPlayablePreviewRefreshProductSmokeTests.cs");
        if (!File.Exists(path))
        {
            return false;
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        return text.Contains("ImplementationStatus", StringComparison.Ordinal)
            && !text.Contains("ReadStagedPlayerHandoffManifest", StringComparison.Ordinal)
            && !text.Contains("TamperNegativeProof", StringComparison.Ordinal)
            && !text.Contains("GamePackageRefreshPlan", StringComparison.Ordinal)
            && !text.Contains("StateTransitionProof", StringComparison.Ordinal);
    }

    private static string ReadOptional(string projectRoot, string relativePath)
    {
        var path = Resolve(projectRoot, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Resolve(string projectRoot, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, path);
        return path;
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string Compact(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (!char.IsWhiteSpace(ch))
            {
                builder.Append(ch);
            }
        }

        return builder.ToString();
    }

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

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            _ => 2
        };

    private static EditDrivenPlayablePreviewRefreshDiagnostic Error(string code, string target, string message) =>
        EditDrivenPlayablePreviewRefreshDiagnostic.Error(code, target, message);
}
