using System.Text;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;

public sealed class EditDrivenPlayableReviewPackageMaterializationQualityGateScanner
{
    private static readonly Regex TimestampLikePattern = new(
        @"\b20\d{2}-\d{2}-\d{2}[T ][0-2]\d:[0-5]\d:[0-5]\d",
        RegexOptions.Compiled);

    private static readonly IReadOnlyList<string> ScanDirectories =
    [
        "src/LLMGameCreator.Application/Design/EditDrivenPlayableReviewPackageMaterialization",
        "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace",
        "tests/LLMGameCreator.Tests/Application/EditDrivenPlayableReviewPackageMaterialization"
    ];

    private static readonly IReadOnlyList<string> ScanFiles =
    [
        "src/LLMGameCreator.WinForms/CompositionRoot.cs",
        "tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenPlayableReviewPackageMaterializationProductSmokeTests.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs"
    ];

    public EditDrivenReviewPackageQualityGateScan Scan(
        string projectRoot,
        int reviewPackageTargetFileCount,
        IReadOnlyDictionary<string, string> evidencePayloads)
    {
        var files = EnumerateFiles(projectRoot)
            .OrderBy(path => Relative(projectRoot, path), StringComparer.Ordinal)
            .Select(path => ScanFile(projectRoot, path))
            .ToList();
        var diagnostics = new List<EditDrivenPlayableReviewPackageDiagnostic>();
        var linesOver500 = files.Sum(file => file.LinesOver500Count);
        var over1000 = files.Count(file =>
            file.LineCount > 1000
            && file.RelativePath != "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
        var minified = files.Count(file => file.MinifiedSourceCandidate);
        var binding = BuildWinFormsBindingInventory(projectRoot);
        var reportOnlySmoke = DetectReportOnlySmoke(projectRoot);
        var alpha = files.FirstOrDefault(file =>
            file.RelativePath == "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
        var alphaPath = Resolve(projectRoot, "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
        var alphaHash = File.Exists(alphaPath)
            ? EditDrivenPlayableReviewPackageMaterializationHash.Sha256(File.ReadAllBytes(alphaPath))
            : string.Empty;
        var evidenceScan = ScanEvidencePayloads(evidencePayloads);

        if (reviewPackageTargetFileCount <= 0)
        {
            diagnostics.Add(Error(
                "goal077.quality.review_package_targets_missing",
                "review-package/targets",
                "A GREEN Goal 077 report requires concrete review package target files."));
        }

        if (linesOver500 > 0)
        {
            diagnostics.Add(Error(
                "goal077.quality.line_over_500",
                "qualityGateScan.linesOver500Count",
                "Changed or scanned C# files must not have lines over 500 characters."));
        }

        if (over1000 > 0)
        {
            diagnostics.Add(Error(
                "goal077.quality.file_over_1000_lines",
                "qualityGateScan.filesOver1000LinesCount",
                "New Goal 077 source/test files must stay below 1000 lines."));
        }

        if (minified > 0)
        {
            diagnostics.Add(Error(
                "goal077.quality.minified_source",
                "qualityGateScan.minifiedSourceFileCount",
                "Goal 077 must not add one-line/minified source."));
        }

        if (!binding.Passed)
        {
            diagnostics.Add(Error(
                "goal077.quality.parent_ui_non_binding",
                "CampaignAuthoringReviewWorkspacePageControl",
                "Parent workspace must bind the Goal 077 review package control through activation."));
        }

        if (reportOnlySmoke)
        {
            diagnostics.Add(Error(
                "goal077.quality.report_only_smoke",
                "EditDrivenPlayableReviewPackageMaterializationProductSmokeTests",
                "Product smoke must read package files from disk and validate hashes, not only report status."));
        }

        diagnostics.AddRange(evidenceScan.Diagnostics);

        return new EditDrivenReviewPackageQualityGateScan
        {
            Passed = diagnostics.Count == 0,
            ScannedFileCount = files.Count,
            MaxLineLength = files.Count == 0 ? 0 : files.Max(file => file.MaxLineLength),
            LinesOver500Count = linesOver500,
            FilesOver1000LinesCount = over1000,
            MinifiedSourceFileCount = minified,
            ReviewPackageTargetFileCount = reviewPackageTargetFileCount,
            ParentUiBindingPassed = binding.Passed,
            ReportOnlySmokeDetected = reportOnlySmoke,
            AlphaRuntimeBootstrapLineCount = alpha?.LineCount ?? 0,
            AlphaRuntimeBootstrapHash = alphaHash,
            AlphaRuntimeBootstrapNoChangeStatus = "read_only_hash_recorded_no_goal077_write_path",
            EvidenceContainsAbsoluteLocalPaths = evidenceScan.ContainsAbsoluteLocalPaths,
            EvidenceContainsTimestampLikeValues = evidenceScan.ContainsTimestampLikeValues,
            EvidenceContainsHeavyLogs = evidenceScan.ContainsHeavyLogs,
            EvidenceContainsScratchTamperFiles = evidenceScan.ContainsScratchTamperFiles,
            Files = files,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public EditDrivenReviewPackageWinFormsBindingInventory BuildWinFormsBindingInventory(string projectRoot)
    {
        var group = new EditDrivenReviewPackageWinFormsBindingGroup
        {
            GroupId = "review_package_status",
            ControlName = "CampaignReviewPackageControl",
            RelativePath = "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignReviewPackageControl.cs",
            SeparateUserControl = true,
            BindsGoal077Data = true
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
        var tabDeclared = pageDesigner.Contains("_reviewPackageTabPage", StringComparison.Ordinal)
            && pageDesigner.Contains("_reviewPackageControl", StringComparison.Ordinal)
            && pageDesigner.Contains("CampaignReviewPackageControl", StringComparison.Ordinal);
        var serviceLoaded = pageCode.Contains(
                "EditDrivenPlayableReviewPackageMaterializationEvidenceService",
                StringComparison.Ordinal)
            && compactPageCode.Contains("_reviewPackageService.Build(root)", StringComparison.Ordinal);
        var controlBound = pageCode.Contains(
                "EditDrivenPlayableReviewPackageMaterializationBuildResult",
                StringComparison.Ordinal)
            && compactPageCode.Contains("_reviewPackageControl.Bind(reviewPackageResult)", StringComparison.Ordinal);
        var activationBinds = tabDeclared && serviceLoaded && controlBound;
        var diagnostics = new List<EditDrivenPlayableReviewPackageDiagnostic>();

        if (!controlExists)
        {
            diagnostics.Add(Error("goal077.winforms.control_missing", group.RelativePath, "Required Goal 077 control is missing."));
        }

        if (!tabDeclared)
        {
            diagnostics.Add(Error(
                "goal077.winforms.review_package_tab_missing",
                "CampaignAuthoringReviewWorkspacePageControl.Designer.cs",
                "Parent workspace must declare a separate review package tab/control."));
        }

        if (tabDeclared && !serviceLoaded)
        {
            diagnostics.Add(Error(
                "goal077.winforms.review_package_service_missing",
                "CampaignAuthoringReviewWorkspacePageControl.cs",
                "Parent workspace activation must load Goal 077 evidence service."));
        }

        if (tabDeclared && !controlBound)
        {
            diagnostics.Add(Error(
                "goal077.winforms.review_package_control_bind_missing",
                "CampaignAuthoringReviewWorkspacePageControl.cs",
                "Parent workspace must bind the Goal 077 result into CampaignReviewPackageControl."));
        }

        return new EditDrivenReviewPackageWinFormsBindingInventory
        {
            Passed = diagnostics.Count == 0,
            ParentPageReviewPackageTabDeclared = tabDeclared,
            ParentPageReviewPackageEvidenceServiceLoaded = serviceLoaded,
            ParentPageReviewPackageControlBound = controlBound,
            ParentPageActivationBindsGoal077Data = activationBinds,
            Groups = [group],
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenPlayableReviewPackageDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static EvidencePayloadScan ScanEvidencePayloads(IReadOnlyDictionary<string, string> evidencePayloads)
    {
        var diagnostics = new List<EditDrivenPlayableReviewPackageDiagnostic>();
        var containsAbsolutePath = false;
        var containsTimestamp = false;
        var containsHeavyLogs = false;
        var containsTamper = false;

        foreach (var pair in evidencePayloads.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            if (pair.Value.Contains(@"C:\", StringComparison.OrdinalIgnoreCase)
                || pair.Value.Contains("/Users/", StringComparison.OrdinalIgnoreCase))
            {
                containsAbsolutePath = true;
                diagnostics.Add(Error(
                    "goal077.evidence.absolute_path",
                    pair.Key,
                    "Tracked Goal 077 evidence must not contain absolute local paths."));
            }

            if (TimestampLikePattern.IsMatch(pair.Value))
            {
                containsTimestamp = true;
                diagnostics.Add(Error(
                    "goal077.evidence.timestamp_like_value",
                    pair.Key,
                    "Tracked Goal 077 evidence must not contain timestamp-like values."));
            }

            if (pair.Key.Contains("/logs/", StringComparison.OrdinalIgnoreCase)
                || pair.Key.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            {
                containsHeavyLogs = true;
                diagnostics.Add(Error(
                    "goal077.evidence.heavy_log",
                    pair.Key,
                    "Tracked Goal 077 evidence must not contain heavy logs."));
            }

            if (pair.Key.Contains("tamper", StringComparison.OrdinalIgnoreCase)
                && pair.Key.StartsWith("review-package/", StringComparison.OrdinalIgnoreCase))
            {
                containsTamper = true;
                diagnostics.Add(Error(
                    "goal077.evidence.scratch_tamper_file",
                    pair.Key,
                    "Negative proof must not leave scratch tamper files inside the review package."));
            }
        }

        return new EvidencePayloadScan(
            containsAbsolutePath,
            containsTimestamp,
            containsHeavyLogs,
            containsTamper,
            SortDiagnostics(diagnostics));
    }

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

    private static EditDrivenReviewPackageQualityFileScan ScanFile(string projectRoot, string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None);
        var lineLengths = lines.Select(line => line.Length).ToList();
        return new EditDrivenReviewPackageQualityFileScan
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
        var path = Resolve(
            projectRoot,
            "tests/LLMGameCreator.Tests/ProductSmoke/"
                + "EditDrivenPlayableReviewPackageMaterializationProductSmokeTests.cs");
        if (!File.Exists(path))
        {
            return false;
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        return text.Contains("ImplementationStatus", StringComparison.Ordinal)
            && !text.Contains("ReadStagedReviewPackage", StringComparison.Ordinal)
            && !text.Contains("PackageFileLedger", StringComparison.Ordinal)
            && !text.Contains("ReviewPackageFiles", StringComparison.Ordinal)
            && !text.Contains("TargetFile", StringComparison.Ordinal);
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

    private static EditDrivenPlayableReviewPackageDiagnostic Error(string code, string target, string message) =>
        EditDrivenPlayableReviewPackageDiagnostic.Error(code, target, message);

    private sealed record EvidencePayloadScan(
        bool ContainsAbsoluteLocalPaths,
        bool ContainsTimestampLikeValues,
        bool ContainsHeavyLogs,
        bool ContainsScratchTamperFiles,
        IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> Diagnostics);
}
