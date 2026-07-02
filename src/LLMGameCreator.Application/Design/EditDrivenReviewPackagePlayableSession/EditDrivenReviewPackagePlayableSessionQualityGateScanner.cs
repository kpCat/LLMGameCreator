using System.Text;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;

public sealed class EditDrivenReviewPackagePlayableSessionQualityGateScanner
{
    private static readonly Regex TimestampLikePattern = new(
        @"\b20\d{2}-\d{2}-\d{2}[T ][0-2]\d:[0-5]\d:[0-5]\d",
        RegexOptions.Compiled);

    private static readonly IReadOnlyList<string> ScanDirectories =
    [
        "src/LLMGameCreator.Application/Design/EditDrivenReviewPackagePlayableSession",
        "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace",
        "tests/LLMGameCreator.Tests/Application/EditDrivenReviewPackagePlayableSession"
    ];

    private static readonly IReadOnlyList<string> ScanFiles =
    [
        "src/LLMGameCreator.WinForms/CompositionRoot.cs",
        "tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenReviewPackagePlayableSessionProductSmokeTests.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs"
    ];

    private static readonly IReadOnlyList<string> ForbiddenEvidencePrefixes =
    [
        "src/LLMGameCreator.Runtime/",
        "src/LLMGameCreator.Runtime.Abstractions/",
        "src/LLMGameCreator.GamePackage/",
        "src/LLMGameCreator.Infrastructure/",
        "generator-library/",
        "unity/"
    ];

    public EditDrivenReviewPackagePlayableSessionQualityGateScan Scan(
        string projectRoot,
        string expectedAlphaRuntimeBootstrapHash,
        IReadOnlyDictionary<string, string> evidencePayloads)
    {
        var files = EnumerateFiles(projectRoot)
            .OrderBy(path => Relative(projectRoot, path), StringComparer.Ordinal)
            .Select(path => ScanFile(projectRoot, path))
            .ToList();
        var diagnostics = new List<EditDrivenReviewPackagePlayableSessionDiagnostic>();
        var linesOver500 = files.Sum(file => file.LinesOver500Count);
        var over1000 = files.Count(file =>
            file.LineCount > 1000
            && file.RelativePath != "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
        var minified = files.Count(file => file.MinifiedSourceCandidate);
        var binding = BuildWinFormsBindingInventory(projectRoot);
        var reportOnlySmoke = DetectReportOnlySmoke(projectRoot);
        var alphaPath = Resolve(projectRoot, "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
        var alpha = files.FirstOrDefault(file =>
            file.RelativePath == "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
        var alphaHash = File.Exists(alphaPath)
            ? EditDrivenReviewPackagePlayableSessionHash.Sha256(File.ReadAllBytes(alphaPath))
            : string.Empty;
        var alphaUnchanged = string.IsNullOrWhiteSpace(expectedAlphaRuntimeBootstrapHash)
            || string.Equals(alphaHash, expectedAlphaRuntimeBootstrapHash, StringComparison.OrdinalIgnoreCase);
        var evidenceScan = ScanEvidencePayloads(evidencePayloads);

        if (linesOver500 > 0)
        {
            diagnostics.Add(Error(
                "goal078.quality.line_over_500",
                "qualityGateScan.linesOver500Count",
                "Changed or scanned C# files must not have lines over 500 characters."));
        }

        if (over1000 > 0)
        {
            diagnostics.Add(Error(
                "goal078.quality.file_over_1000_lines",
                "qualityGateScan.filesOver1000LinesCount",
                "New or touched Goal 078 C# files must stay below 1000 lines."));
        }

        if (minified > 0)
        {
            diagnostics.Add(Error(
                "goal078.quality.minified_source",
                "qualityGateScan.minifiedSourceFileCount",
                "Goal 078 must not add one-line/minified source files."));
        }

        if (!binding.Passed)
        {
            diagnostics.Add(Error(
                "goal078.quality.parent_ui_non_binding",
                "CampaignAuthoringReviewWorkspacePageControl",
                "Parent workspace must bind the Goal 078 play-session control through activation."));
        }

        if (reportOnlySmoke)
        {
            diagnostics.Add(Error(
                "goal078.quality.report_only_smoke",
                "EditDrivenReviewPackagePlayableSessionProductSmokeTests",
                "Product smoke must read package/session artifacts and replay proof, not only report status."));
        }

        if (!alphaUnchanged)
        {
            diagnostics.Add(Error(
                "goal078.quality.alpha_runtime_bootstrap_changed",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs",
                "Goal 078 must not edit AlphaRuntimeBootstrap.cs."));
        }

        diagnostics.AddRange(evidenceScan.Diagnostics);

        return new EditDrivenReviewPackagePlayableSessionQualityGateScan
        {
            Passed = diagnostics.Count == 0,
            ScannedFileCount = files.Count,
            MaxLineLength = files.Count == 0 ? 0 : files.Max(file => file.MaxLineLength),
            LinesOver500Count = linesOver500,
            FilesOver1000LinesCount = over1000,
            MinifiedSourceFileCount = minified,
            ParentUiBindingPassed = binding.Passed,
            ReportOnlySmokeDetected = reportOnlySmoke,
            AlphaRuntimeBootstrapLineCount = alpha?.LineCount ?? 0,
            AlphaRuntimeBootstrapHash = alphaHash,
            AlphaRuntimeBootstrapExpectedHash = expectedAlphaRuntimeBootstrapHash,
            AlphaRuntimeBootstrapUnchanged = alphaUnchanged,
            EvidenceContainsAbsoluteLocalPaths = evidenceScan.ContainsAbsoluteLocalPaths,
            EvidenceContainsTimestampLikeValues = evidenceScan.ContainsTimestampLikeValues,
            EvidenceContainsHeavyLogs = evidenceScan.ContainsHeavyLogs,
            EvidenceContainsScratchTamperFiles = evidenceScan.ContainsScratchTamperFiles,
            ForbiddenAreaEvidenceDetected = evidenceScan.ContainsForbiddenAreaEvidence,
            Files = files,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public EditDrivenReviewPackagePlayableSessionWinFormsBindingInventory BuildWinFormsBindingInventory(string projectRoot)
    {
        var group = new EditDrivenReviewPackagePlayableSessionWinFormsBindingGroup
        {
            GroupId = "review_package_play_session_status",
            ControlName = "CampaignReviewPackagePlaySessionControl",
            RelativePath = "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignReviewPackagePlaySessionControl.cs",
            SeparateUserControl = true,
            BindsGoal078Data = true
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
        var tabDeclared = pageDesigner.Contains("_playSessionTabPage", StringComparison.Ordinal)
            && pageDesigner.Contains("_playSessionControl", StringComparison.Ordinal)
            && pageDesigner.Contains("CampaignReviewPackagePlaySessionControl", StringComparison.Ordinal);
        var serviceLoaded = pageCode.Contains(
                "EditDrivenReviewPackagePlayableSessionEvidenceService",
                StringComparison.Ordinal)
            && compactPageCode.Contains("_playSessionService.Build(root)", StringComparison.Ordinal);
        var controlBound = pageCode.Contains(
                "EditDrivenReviewPackagePlayableSessionBuildResult",
                StringComparison.Ordinal)
            && compactPageCode.Contains("_playSessionControl.Bind(playSessionResult)", StringComparison.Ordinal);
        var activationBinds = tabDeclared && serviceLoaded && controlBound;
        var diagnostics = new List<EditDrivenReviewPackagePlayableSessionDiagnostic>();

        if (!controlExists)
        {
            diagnostics.Add(Error("goal078.winforms.control_missing", group.RelativePath, "Required Goal 078 control is missing."));
        }

        if (!tabDeclared)
        {
            diagnostics.Add(Error(
                "goal078.winforms.play_session_tab_missing",
                "CampaignAuthoringReviewWorkspacePageControl.Designer.cs",
                "Parent workspace must declare a separate play-session tab/control."));
        }

        if (tabDeclared && !serviceLoaded)
        {
            diagnostics.Add(Error(
                "goal078.winforms.play_session_service_missing",
                "CampaignAuthoringReviewWorkspacePageControl.cs",
                "Parent workspace activation must load Goal 078 evidence service."));
        }

        if (tabDeclared && !controlBound)
        {
            diagnostics.Add(Error(
                "goal078.winforms.play_session_control_bind_missing",
                "CampaignAuthoringReviewWorkspacePageControl.cs",
                "Parent workspace must bind the Goal 078 result into CampaignReviewPackagePlaySessionControl."));
        }

        return new EditDrivenReviewPackagePlayableSessionWinFormsBindingInventory
        {
            Passed = diagnostics.Count == 0,
            ParentPagePlaySessionTabDeclared = tabDeclared,
            ParentPagePlaySessionEvidenceServiceLoaded = serviceLoaded,
            ParentPagePlaySessionControlBound = controlBound,
            ParentPageActivationBindsGoal078Data = activationBinds,
            Groups = [group],
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenReviewPackagePlayableSessionDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static EvidencePayloadScan ScanEvidencePayloads(IReadOnlyDictionary<string, string> evidencePayloads)
    {
        var diagnostics = new List<EditDrivenReviewPackagePlayableSessionDiagnostic>();
        var containsAbsolutePath = false;
        var containsTimestamp = false;
        var containsHeavyLogs = false;
        var containsTamper = false;
        var containsForbidden = false;

        foreach (var pair in evidencePayloads.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            if (pair.Value.Contains(@"C:\", StringComparison.OrdinalIgnoreCase)
                || pair.Value.Contains("/Users/", StringComparison.OrdinalIgnoreCase))
            {
                containsAbsolutePath = true;
                diagnostics.Add(Error(
                    "goal078.evidence.absolute_path",
                    pair.Key,
                    "Tracked Goal 078 evidence must not contain absolute local paths."));
            }

            if (TimestampLikePattern.IsMatch(pair.Value))
            {
                containsTimestamp = true;
                diagnostics.Add(Error(
                    "goal078.evidence.timestamp_like_value",
                    pair.Key,
                    "Tracked Goal 078 evidence must not contain timestamp-like values."));
            }

            if (pair.Key.Contains("/logs/", StringComparison.OrdinalIgnoreCase)
                || pair.Key.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            {
                containsHeavyLogs = true;
                diagnostics.Add(Error(
                    "goal078.evidence.heavy_log",
                    pair.Key,
                    "Tracked Goal 078 evidence must not contain heavy logs."));
            }

            if (pair.Key.Contains("tamper", StringComparison.OrdinalIgnoreCase)
                && pair.Key.StartsWith("review-package/", StringComparison.OrdinalIgnoreCase))
            {
                containsTamper = true;
                diagnostics.Add(Error(
                    "goal078.evidence.scratch_tamper_file",
                    pair.Key,
                    "Negative proof must not leave scratch tamper files inside review package evidence."));
            }

            if (ForbiddenEvidencePrefixes.Any(prefix => pair.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                containsForbidden = true;
                diagnostics.Add(Error(
                    "goal078.evidence.forbidden_area",
                    pair.Key,
                    "Goal 078 evidence must not materialize forbidden area mutations."));
            }
        }

        return new EvidencePayloadScan(
            containsAbsolutePath,
            containsTimestamp,
            containsHeavyLogs,
            containsTamper,
            containsForbidden,
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

    private static EditDrivenReviewPackagePlayableSessionQualityFileScan ScanFile(string projectRoot, string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None);
        var lineLengths = lines.Select(line => line.Length).ToList();
        return new EditDrivenReviewPackagePlayableSessionQualityFileScan
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
            "tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenReviewPackagePlayableSessionProductSmokeTests.cs");
        if (!File.Exists(path))
        {
            return false;
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        return text.Contains("ImplementationStatus", StringComparison.Ordinal)
            && !text.Contains("package-read-proof.json", StringComparison.Ordinal)
            && !text.Contains("playable-session-action-log.json", StringComparison.Ordinal)
            && !text.Contains("playable-session-replay-proof.json", StringComparison.Ordinal)
            && !text.Contains("TargetPayloadRead", StringComparison.Ordinal);
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

    private static EditDrivenReviewPackagePlayableSessionDiagnostic Error(string code, string target, string message) =>
        EditDrivenReviewPackagePlayableSessionDiagnostic.Error(code, target, message);

    private sealed record EvidencePayloadScan(
        bool ContainsAbsoluteLocalPaths,
        bool ContainsTimestampLikeValues,
        bool ContainsHeavyLogs,
        bool ContainsScratchTamperFiles,
        bool ContainsForbiddenAreaEvidence,
        IReadOnlyList<EditDrivenReviewPackagePlayableSessionDiagnostic> Diagnostics);
}
