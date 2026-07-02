using System.Text;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;

public sealed class EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner
{
    private const int MaxAllowedLineLength = 500;
    private const int MaxAllowedLineCount = 1000;
    private static readonly Regex TimestampLikePattern = new(
        @"\b20\d{2}-\d{2}-\d{2}[T ][0-2]\d:[0-5]\d:[0-5]\d",
        RegexOptions.Compiled);

    public EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScan Scan(
        string repositoryRootPath,
        EditDrivenUnityAlphaStreamingAssetsHandoffWinFormsBindingInventory bindingInventory,
        IReadOnlyDictionary<string, string> evidencePayloads,
        IReadOnlyDictionary<string, string> streamingPayloads)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var files = CandidateFiles(root)
            .Where(File.Exists)
            .Select(path => ScanFile(root, path))
            .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToList();
        var diagnostics = new List<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic>();
        var evidenceScan = ScanEvidencePayloads(evidencePayloads.Concat(streamingPayloads).ToDictionary(
            item => item.Key,
            item => item.Value,
            StringComparer.Ordinal));

        foreach (var file in files.Where(file => file.LinesOver500Count > 0))
        {
            diagnostics.Add(Error(
                "goal082.quality.source_line_too_long",
                file.RelativePath,
                "Source line exceeds 500 characters."));
        }

        foreach (var file in files.Where(file => file.LineCount > MaxAllowedLineCount))
        {
            diagnostics.Add(Error(
                "goal082.quality.source_file_too_long",
                file.RelativePath,
                "Goal082 source file exceeds 1000 logical lines."));
        }

        foreach (var file in files.Where(file => file.ZeroLfSource || file.CrOnlySource || file.RawPhysicalOneLineSource))
        {
            diagnostics.Add(Error(
                "goal082.quality.raw_source_format_rejected",
                file.RelativePath,
                "Source file has zero-LF, CR-only, or raw one-physical-line shape."));
        }

        if (!bindingInventory.Passed)
        {
            diagnostics.Add(Error(
                "goal082.quality.winforms_binding_failed",
                "winforms-binding-inventory.json",
                "Goal082 parent WinForms binding inventory did not pass."));
        }

        var alphaPath = Path.Combine(
            root,
            Normalize(EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapPath));
        var alphaBytes = File.Exists(alphaPath) ? File.ReadAllBytes(alphaPath) : [];
        var alphaText = alphaBytes.Length == 0 ? string.Empty : Encoding.UTF8.GetString(alphaBytes);
        var alphaLineCount = AlphaRuntimeBootstrapLineCount(alphaBytes);
        var alphaHash = alphaBytes.Length == 0
            ? string.Empty
            : EditDrivenUnityAlphaStreamingAssetsHandoffHash.Sha256Bytes(alphaBytes);
        var alphaUnchanged = AlphaRuntimeBootstrapMatchesBaseline(alphaBytes);
        if (!alphaUnchanged)
        {
            diagnostics.Add(Error(
                "goal082.quality.alpha_runtime_bootstrap_changed",
                EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapPath,
                "Goal082 must keep AlphaRuntimeBootstrap.cs read-only and unchanged."));
        }

        var parentWorkspacePath = Path.Combine(
            root,
            Normalize("src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs"));
        var parentWorkspaceLineCount = File.Exists(parentWorkspacePath)
            ? File.ReadAllText(parentWorkspacePath).Split('\n').Length
            : 0;
        var probePath = Path.Combine(
            root,
            Normalize(EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityProbeScriptPath));
        var probeText = File.Exists(probePath) ? File.ReadAllText(probePath) : string.Empty;
        var probeLineCount = string.IsNullOrEmpty(probeText) ? 0 : probeText.Split('\n').Length;
        var probeBelow300 = probeLineCount > 0 && probeLineCount < 300;
        var probeUsesStreamingAssets = probeText.Contains("Application.streamingAssetsPath", StringComparison.Ordinal)
                                       && probeText.Contains(
                                           EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityStreamingAssetsProbeRoot,
                                           StringComparison.Ordinal);
        var probeHasForbiddenDependency = probeText.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal)
                                          || probeText.Contains("LLMProvider", StringComparison.OrdinalIgnoreCase)
                                          || probeText.Contains("Comfy", StringComparison.OrdinalIgnoreCase)
                                          || probeText.Contains("Fooocus", StringComparison.OrdinalIgnoreCase)
                                          || probeText.Contains("LLMGameCreator.Runtime", StringComparison.Ordinal)
                                          || probeText.Contains("MediaProvider", StringComparison.OrdinalIgnoreCase);
        if (!probeBelow300)
        {
            diagnostics.Add(Error(
                "goal082.quality.unity_probe_too_long",
                EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityProbeScriptPath,
                "Unity probe source must stay below 300 lines."));
        }

        if (!probeUsesStreamingAssets)
        {
            diagnostics.Add(Error(
                "goal082.quality.unity_probe_streamingassets_missing",
                EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityProbeScriptPath,
                "Unity probe must use Application.streamingAssetsPath and the Goal082 payload root."));
        }

        if (probeHasForbiddenDependency)
        {
            diagnostics.Add(Error(
                "goal082.quality.unity_probe_forbidden_dependency",
                EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityProbeScriptPath,
                "Unity probe must not depend on runtime/provider/LLM/media systems or AlphaRuntimeBootstrap."));
        }

        var syntheticCrOnlyRejected = RejectsSuspiciousRawSourceBytes(Encoding.UTF8.GetBytes("public sealed class A\r{\r}\r"));
        var syntheticZeroLfRejected = RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("public sealed class A { public string Value => \"" + new string('x', 520) + "\"; }"));
        if (!syntheticCrOnlyRejected || !syntheticZeroLfRejected)
        {
            diagnostics.Add(Error(
                "goal082.quality.synthetic_raw_source_not_rejected",
                "quality-gate-scan",
                "Synthetic raw-byte source guard failed."));
        }

        diagnostics.AddRange(evidenceScan.Diagnostics);

        return new EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScan
        {
            Passed = diagnostics.Count == 0,
            ScannedFileCount = files.Count,
            MaxLineLength = files.Count == 0 ? 0 : files.Max(file => file.MaxLineLength),
            LinesOver500Count = files.Sum(file => file.LinesOver500Count),
            FilesOver1000LinesCount = files.Count(file => file.LineCount > MaxAllowedLineCount),
            MinifiedSourceFileCount = files.Count(file => file.MinifiedSourceCandidate),
            RawPhysicalOneLineSourceCount = files.Count(file => file.RawPhysicalOneLineSource),
            ZeroLfSourceCount = files.Count(file => file.ZeroLfSource),
            CrOnlySourceCount = files.Count(file => file.CrOnlySource),
            SyntheticCrOnlySourceRejected = syntheticCrOnlyRejected,
            SyntheticZeroLfOneLineSourceRejected = syntheticZeroLfRejected,
            ParentWorkspaceLineCount = parentWorkspaceLineCount,
            AlphaRuntimeBootstrapBaselineLineCount = 3672,
            AlphaRuntimeBootstrapBaselineHash = EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapExpectedHash,
            AlphaRuntimeBootstrapAfterLineCount = alphaLineCount,
            AlphaRuntimeBootstrapAfterHash = alphaHash,
            AlphaRuntimeBootstrapUnchanged = alphaUnchanged,
            UnityProbeLineCount = probeLineCount,
            UnityProbeBelow300Lines = probeBelow300,
            UnityProbeUsesStreamingAssetsPath = probeUsesStreamingAssets,
            UnityProbeNoRuntimeProviderLlmMediaDependency = !probeHasForbiddenDependency,
            ParentUiBindingPassed = bindingInventory.Passed,
            EvidenceContainsAbsoluteLocalPaths = evidenceScan.ContainsAbsoluteLocalPaths,
            EvidenceContainsTimestampLikeValues = evidenceScan.ContainsTimestampLikeValues,
            EvidenceContainsHeavyLogs = evidenceScan.ContainsHeavyLogs,
            EvidenceContainsScratchTamperFiles = evidenceScan.ContainsScratchTamperFiles,
            Files = files,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static EditDrivenUnityAlphaStreamingAssetsHandoffWinFormsBindingInventory BuildWinFormsBindingInventory(
        string repositoryRootPath)
    {
        const string parentCs =
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs";
        const string parentDesigner =
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs";
        const string childCs =
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignUnityAlphaStreamingAssetsHandoffControl.cs";
        var root = Path.GetFullPath(repositoryRootPath);
        var parentCode = SafeRead(root, parentCs);
        var compactParent = Compact(parentCode);
        var designerCode = SafeRead(root, parentDesigner);
        var childCode = SafeRead(root, childCs);
        var diagnostics = new List<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic>();
        var tabDeclared = designerCode.Contains("_unityAlphaStreamingAssetsHandoffTabPage", StringComparison.Ordinal)
                          && designerCode.Contains("_unityAlphaStreamingAssetsHandoffControl", StringComparison.Ordinal)
                          && designerCode.Contains("CampaignUnityAlphaStreamingAssetsHandoffControl", StringComparison.Ordinal);
        var serviceLoaded = parentCode.Contains(
                                "EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService",
                                StringComparison.Ordinal)
                            && compactParent.Contains(
                                "_unityAlphaStreamingAssetsHandoffService.BuildAndWriteAsync(root).GetAwaiter().GetResult().Result",
                                StringComparison.Ordinal);
        var controlBound = parentCode.Contains(
                               "EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult",
                               StringComparison.Ordinal)
                           && compactParent.Contains(
                               "_unityAlphaStreamingAssetsHandoffControl.Bind(unityAlphaStreamingAssetsHandoffResult)",
                               StringComparison.Ordinal)
                           && childCode.Contains(
                               "Bind(EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult result)",
                               StringComparison.Ordinal);
        var activationBinds = tabDeclared && serviceLoaded && controlBound;

        if (!File.Exists(Path.Combine(root, Normalize(childCs))))
        {
            diagnostics.Add(Error("goal082.winforms.control_missing", childCs, "Required Goal082 control is missing."));
        }

        if (!tabDeclared)
        {
            diagnostics.Add(Error(
                "goal082.winforms.tab_missing",
                parentDesigner,
                "Parent workspace must declare a separate Goal082 StreamingAssets handoff tab/control."));
        }

        if (!serviceLoaded)
        {
            diagnostics.Add(Error(
                "goal082.winforms.service_missing",
                parentCs,
                "Parent workspace activation must load Goal082 evidence service."));
        }

        if (!controlBound)
        {
            diagnostics.Add(Error(
                "goal082.winforms.control_bind_missing",
                parentCs,
                "Parent workspace must bind Goal082 result into CampaignUnityAlphaStreamingAssetsHandoffControl."));
        }

        return new EditDrivenUnityAlphaStreamingAssetsHandoffWinFormsBindingInventory
        {
            Passed = diagnostics.Count == 0,
            ParentPageHandoffTabDeclared = tabDeclared,
            ParentPageHandoffServiceLoaded = serviceLoaded,
            ParentPageHandoffControlBound = controlBound,
            ParentPageActivationBindsGoal082Data = activationBinds,
            Groups =
            [
                new EditDrivenUnityAlphaStreamingAssetsHandoffWinFormsBindingGroup
                {
                    GroupId = "goal082_unity_alpha_streamingassets_handoff_tab",
                    ControlName = "CampaignUnityAlphaStreamingAssetsHandoffControl",
                    RelativePath = childCs,
                    SeparateUserControl = childCode.Contains(": UserControl", StringComparison.Ordinal),
                    BindsGoal082Data = childCode.Contains(
                        "Bind(EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult result)",
                        StringComparison.Ordinal)
                }
            ],
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static bool RejectsSuspiciousRawSourceBytes(byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes);
        var hasLf = text.Contains('\n');
        var hasCr = text.Contains('\r');
        if (!hasLf)
        {
            return true;
        }

        if (hasCr && !hasLf)
        {
            return true;
        }

        var lines = text.Split('\n');
        var maxLineLength = lines.Select(line => line.TrimEnd('\r').Length).DefaultIfEmpty(0).Max();
        return lines.Length == 1 && maxLineLength > MaxAllowedLineLength;
    }

    public static bool AlphaRuntimeBootstrapMatchesBaseline(byte[] bytes) =>
        bytes.Length > 0
        && AlphaRuntimeBootstrapLineCount(bytes) == 3672
        && string.Equals(
            EditDrivenUnityAlphaStreamingAssetsHandoffHash.Sha256Bytes(bytes),
            EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapExpectedHash,
            StringComparison.OrdinalIgnoreCase);

    public static int AlphaRuntimeBootstrapLineCount(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return 0;
        }

        return Encoding.UTF8.GetString(bytes).Split('\n').Length;
    }

    public static IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(
                item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message,
                StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Severity == "error" ? 0 : 1)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<string> CandidateFiles(string root)
    {
        var fixedFiles = new[]
        {
            "src/LLMGameCreator.WinForms/CompositionRoot.cs",
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs",
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs",
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignUnityAlphaStreamingAssetsHandoffControl.cs",
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignUnityAlphaStreamingAssetsHandoffControl.Designer.cs",
            "tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenUnityAlphaStreamingAssetsHandoffProductSmokeTests.cs",
            "tests/LLMGameCreator.Tests/Docs/CurrentGeneratorStateDocsTests.cs",
            EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityProbeScriptPath
        };
        var prefixes = new[]
        {
            "src/LLMGameCreator.Application/Design/EditDrivenUnityAlphaStreamingAssetsHandoff",
            "tests/LLMGameCreator.Tests/Application/EditDrivenUnityAlphaStreamingAssetsHandoff"
        };
        var files = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var fixedFile in fixedFiles)
        {
            files.Add(Path.Combine(root, Normalize(fixedFile)));
        }

        foreach (var prefix in prefixes)
        {
            var fullPrefix = Path.Combine(root, Normalize(prefix));
            if (!Directory.Exists(fullPrefix))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(fullPrefix, "*.cs", SearchOption.AllDirectories))
            {
                files.Add(file);
            }
        }

        return files.ToList();
    }

    private static EditDrivenUnityAlphaStreamingAssetsHandoffQualityFileScan ScanFile(string root, string path)
    {
        var bytes = File.ReadAllBytes(path);
        var text = Encoding.UTF8.GetString(bytes);
        var hasLf = text.Contains('\n');
        var hasCr = text.Contains('\r');
        var lines = hasLf ? text.Split('\n') : [text];
        var maxLineLength = lines.Select(line => line.TrimEnd('\r').Length).DefaultIfEmpty(0).Max();

        return new EditDrivenUnityAlphaStreamingAssetsHandoffQualityFileScan
        {
            RelativePath = Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'),
            LineCount = lines.Length,
            ByteCount = bytes.Length,
            MaxLineLength = maxLineLength,
            LinesOver500Count = lines.Count(line => line.TrimEnd('\r').Length > MaxAllowedLineLength),
            RawPhysicalOneLineSource = lines.Length == 1 && bytes.Length > 200,
            ZeroLfSource = !hasLf,
            CrOnlySource = hasCr && !hasLf,
            MinifiedSourceCandidate = lines.Length <= 2 && maxLineLength > MaxAllowedLineLength
        };
    }

    private static EvidencePayloadScan ScanEvidencePayloads(IReadOnlyDictionary<string, string> evidencePayloads)
    {
        var diagnostics = new List<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic>();
        var containsAbsolutePath = false;
        var containsTimestamp = false;
        var containsHeavyLogs = false;
        var containsScratch = false;

        foreach (var pair in evidencePayloads.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            if (pair.Value.Contains(@"C:\", StringComparison.OrdinalIgnoreCase)
                || pair.Value.Contains("C:/", StringComparison.OrdinalIgnoreCase)
                || pair.Value.Contains("/Users/", StringComparison.OrdinalIgnoreCase))
            {
                containsAbsolutePath = true;
                diagnostics.Add(Error(
                    "goal082.evidence.absolute_path",
                    pair.Key,
                    "Tracked Goal082 evidence must not contain absolute local paths."));
            }

            if (TimestampLikePattern.IsMatch(pair.Value))
            {
                containsTimestamp = true;
                diagnostics.Add(Error(
                    "goal082.evidence.timestamp_like_value",
                    pair.Key,
                    "Tracked Goal082 evidence must not contain timestamp-like values."));
            }

            if (pair.Key.Contains("/logs/", StringComparison.OrdinalIgnoreCase)
                || pair.Key.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            {
                containsHeavyLogs = true;
                diagnostics.Add(Error(
                    "goal082.evidence.heavy_log",
                    pair.Key,
                    "Tracked Goal082 evidence must not contain heavy logs."));
            }

            if (pair.Value.Contains("tamper-scratch", StringComparison.OrdinalIgnoreCase)
                || pair.Value.Contains("scratch-tamper", StringComparison.OrdinalIgnoreCase))
            {
                containsScratch = true;
                diagnostics.Add(Error(
                    "goal082.evidence.scratch_tamper_file",
                    pair.Key,
                    "Negative proof must not leave scratch tamper files in tracked evidence."));
            }
        }

        return new EvidencePayloadScan(
            containsAbsolutePath,
            containsTimestamp,
            containsHeavyLogs,
            containsScratch,
            SortDiagnostics(diagnostics));
    }

    private static string SafeRead(string root, string relativePath)
    {
        var path = Path.Combine(root, Normalize(relativePath));
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

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

    private static string Normalize(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);

    private static EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic Error(
        string code,
        string target,
        string message) =>
        EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic.Error(code, target, message);

    private sealed record EvidencePayloadScan(
        bool ContainsAbsoluteLocalPaths,
        bool ContainsTimestampLikeValues,
        bool ContainsHeavyLogs,
        bool ContainsScratchTamperFiles,
        IReadOnlyList<EditDrivenUnityAlphaStreamingAssetsHandoffDiagnostic> Diagnostics);
}
