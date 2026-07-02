using System.Text;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;

public sealed class EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner
{
    private const int MaxAllowedLineLength = 500;
    private const int MaxAllowedLineCount = 1000;
    private const int LargeSourceFileByteThreshold = 1_500;
    private const string WinFormsParentPath =
        "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs";
    private const string Goal082ApplicationSourcePrefix =
        "src/LLMGameCreator.Application/Design/EditDrivenUnityAlphaStreamingAssetsHandoff/";

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
        var rawByteScannedFileCount = files.Count;
        var logicalMaxLineLength = files.Count == 0 ? 0 : files.Max(file => file.LogicalMaxLineLength);
        var rawPhysicalMaxLineLength = files.Count == 0 ? 0 : files.Max(file => file.RawPhysicalMaxLineLength);
        var rawPhysicalLinesOver500 = files.Sum(file => file.RawPhysicalLinesOver500Count);
        var zeroLfSourceFileCount = files.Count(file => file.ZeroLfSource);
        var crOnlySourceFileCount = files.Count(file => file.CrOnlySource);
        var rawPhysicalOneLineSourceFileCount = files.Count(file => file.RawPhysicalOneLineSource);
        var filesWithTooFewLinesForSizeCount = files.Count(file => file.TooFewLinesForSizeSourceCandidate);
        var unityProbeIncludedInRawScan = files.Any(file => string.Equals(
            file.RelativePath,
            EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityProbeScriptPath,
            StringComparison.Ordinal));
        var winFormsParentIncludedInRawScan = files.Any(file => string.Equals(
            file.RelativePath,
            WinFormsParentPath,
            StringComparison.Ordinal));
        var goal082ApplicationFilesIncludedInRawScan = files.Any(file =>
            file.RelativePath.StartsWith(Goal082ApplicationSourcePrefix, StringComparison.Ordinal));

        foreach (var file in files.Where(file => file.LinesOver500Count > 0))
        {
            diagnostics.Add(Error(
                "goal082.quality.source_line_too_long",
                file.RelativePath,
                "Source line exceeds 500 characters."));
        }

        foreach (var file in files.Where(file => file.RawPhysicalLinesOver500Count > 0))
        {
            diagnostics.Add(Error(
                "goal082.quality.raw_physical_line_too_long",
                file.RelativePath,
                "Raw physical source line exceeds 500 characters."));
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

        foreach (var file in files.Where(file => file.MinifiedSourceCandidate || file.TooFewLinesForSizeSourceCandidate))
        {
            diagnostics.Add(Error(
                "goal082.quality.minified_source_shape",
                file.RelativePath,
                "Source file has a minified or too-few-lines-for-size shape."));
        }

        if (!unityProbeIncludedInRawScan)
        {
            diagnostics.Add(Error(
                "goal082.quality.unity_probe_missing_from_raw_scan",
                EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.UnityProbeScriptPath,
                "Raw-byte source scan must include the Unity handoff probe."));
        }

        if (!winFormsParentIncludedInRawScan)
        {
            diagnostics.Add(Error(
                "goal082.quality.winforms_parent_missing_from_raw_scan",
                WinFormsParentPath,
                "Raw-byte source scan must include the WinForms parent workspace page."));
        }

        if (!goal082ApplicationFilesIncludedInRawScan)
        {
            diagnostics.Add(Error(
                "goal082.quality.application_scope_missing_from_raw_scan",
                Goal082ApplicationSourcePrefix,
                "Raw-byte source scan must include the Goal082 Application seam."));
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
        var syntheticZeroLfOnePhysicalLineRejected = RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("public sealed class A { public string Value => \"" + new string('x', 520) + "\"; }"));
        if (!syntheticCrOnlyRejected || !syntheticZeroLfOnePhysicalLineRejected)
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
            RawByteScannedFileCount = rawByteScannedFileCount,
            MaxLineLength = logicalMaxLineLength,
            LogicalMaxLineLength = logicalMaxLineLength,
            RawPhysicalMaxLineLength = rawPhysicalMaxLineLength,
            LinesOver500Count = files.Sum(file => file.LinesOver500Count),
            RawPhysicalLinesOver500Count = rawPhysicalLinesOver500,
            FilesOver1000LinesCount = files.Count(file => file.LineCount > MaxAllowedLineCount),
            MinifiedSourceFileCount = files.Count(file => file.MinifiedSourceCandidate),
            RawPhysicalOneLineSourceCount = rawPhysicalOneLineSourceFileCount,
            RawPhysicalOneLineSourceFileCount = rawPhysicalOneLineSourceFileCount,
            ZeroLfSourceCount = zeroLfSourceFileCount,
            ZeroLfSourceFileCount = zeroLfSourceFileCount,
            CrOnlySourceCount = crOnlySourceFileCount,
            CrOnlySourceFileCount = crOnlySourceFileCount,
            FilesWithTooFewLinesForSizeCount = filesWithTooFewLinesForSizeCount,
            UnityProbeIncludedInRawScan = unityProbeIncludedInRawScan,
            WinFormsParentIncludedInRawScan = winFormsParentIncludedInRawScan,
            Goal082ApplicationFilesIncludedInRawScan = goal082ApplicationFilesIncludedInRawScan,
            SyntheticCrOnlySourceRejected = syntheticCrOnlyRejected,
            SyntheticZeroLfOneLineSourceRejected = syntheticZeroLfOnePhysicalLineRejected,
            SyntheticZeroLfOnePhysicalLineRejected = syntheticZeroLfOnePhysicalLineRejected,
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
        var scan = AnalyzeSourceBytes("synthetic.cs", bytes);
        return scan.ZeroLfSource
               || scan.CrOnlySource
               || scan.RawPhysicalOneLineSource
               || scan.RawPhysicalLinesOver500Count > 0
               || scan.MinifiedSourceCandidate
               || scan.TooFewLinesForSizeSourceCandidate;
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
        var relativePath = Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
        return AnalyzeSourceBytes(relativePath, bytes);
    }

    private static EditDrivenUnityAlphaStreamingAssetsHandoffQualityFileScan AnalyzeSourceBytes(
        string relativePath,
        byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes);
        var raw = AnalyzeRawPhysicalLines(bytes);
        var logicalLines = Regex.Split(text, "\r\n|\n|\r");
        var logicalLengths = logicalLines.Select(line => line.Length).ToList();
        var logicalMaxLineLength = logicalLengths.Count == 0 ? 0 : logicalLengths.Max();
        var logicalLinesOver500 = logicalLengths.Count(length => length > MaxAllowedLineLength);
        var looksLikeCSharp = LooksLikeCSharpSource(text);
        var substantialZeroLfSource = looksLikeCSharp
                                      && raw.LfByteCount == 0
                                      && (bytes.Length >= LargeSourceFileByteThreshold
                                          || raw.RawPhysicalMaxLineLength > MaxAllowedLineLength
                                          || raw.ContainsCrOnlyLineEndings);
        var crOnlySource = looksLikeCSharp
                           && raw.LfByteCount == 0
                           && raw.ContainsCrOnlyLineEndings;
        var rawPhysicalOneLine = looksLikeCSharp
                                 && raw.RawPhysicalLineCount <= 1
                                 && (bytes.Length >= LargeSourceFileByteThreshold
                                     || raw.RawPhysicalMaxLineLength > MaxAllowedLineLength);
        var tooFewLinesForSize = looksLikeCSharp
                                 && bytes.Length >= LargeSourceFileByteThreshold
                                 && (raw.RawPhysicalLineCount <= 3 || logicalLines.Length <= 3);
        var minified = logicalMaxLineLength > MaxAllowedLineLength
                       || raw.RawPhysicalMaxLineLength > MaxAllowedLineLength
                       || rawPhysicalOneLine
                       || tooFewLinesForSize;

        return new EditDrivenUnityAlphaStreamingAssetsHandoffQualityFileScan
        {
            RelativePath = relativePath,
            LineCount = logicalLines.Length,
            LogicalLineCount = logicalLines.Length,
            ByteCount = bytes.Length,
            MaxLineLength = logicalMaxLineLength,
            LogicalMaxLineLength = logicalMaxLineLength,
            LfByteCount = raw.LfByteCount,
            CrByteCount = raw.CrByteCount,
            RawPhysicalLineCount = raw.RawPhysicalLineCount,
            RawPhysicalMaxLineLength = raw.RawPhysicalMaxLineLength,
            LinesOver500Count = logicalLinesOver500,
            RawPhysicalLinesOver500Count = raw.RawPhysicalLinesOver500Count,
            RawPhysicalOneLineSource = rawPhysicalOneLine,
            ZeroLfSource = substantialZeroLfSource,
            CrOnlySource = crOnlySource,
            ContainsCrOnlyLineEndings = raw.ContainsCrOnlyLineEndings,
            MinifiedSourceCandidate = minified,
            TooFewLinesForSizeSourceCandidate = tooFewLinesForSize
        };
    }

    private static RawSourceLineMetrics AnalyzeRawPhysicalLines(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return new RawSourceLineMetrics(0, 0, 0, 0, 0, false);
        }

        var lfCount = 0;
        var crCount = 0;
        var currentLength = 0;
        var maxLength = 0;
        var over500Count = 0;
        var containsCrOnlyLineEndings = false;

        for (var index = 0; index < bytes.Length; index++)
        {
            var value = bytes[index];
            if (value == '\n')
            {
                lfCount++;
                if (currentLength > maxLength)
                {
                    maxLength = currentLength;
                }

                if (currentLength > MaxAllowedLineLength)
                {
                    over500Count++;
                }

                currentLength = 0;
                continue;
            }

            currentLength++;
            if (value != '\r')
            {
                continue;
            }

            crCount++;
            if (index + 1 >= bytes.Length || bytes[index + 1] != '\n')
            {
                containsCrOnlyLineEndings = true;
            }
        }

        if (currentLength > maxLength)
        {
            maxLength = currentLength;
        }

        if (currentLength > MaxAllowedLineLength)
        {
            over500Count++;
        }

        return new RawSourceLineMetrics(
            lfCount,
            crCount,
            lfCount + 1,
            maxLength,
            over500Count,
            containsCrOnlyLineEndings);
    }

    private static bool LooksLikeCSharpSource(string text) =>
        text.Contains("class ", StringComparison.Ordinal)
        || text.Contains("namespace ", StringComparison.Ordinal)
        || text.Contains("using ", StringComparison.Ordinal)
        || text.Contains("public ", StringComparison.Ordinal)
        || text.Contains("internal ", StringComparison.Ordinal)
        || text.Contains("private ", StringComparison.Ordinal);

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

    private sealed record RawSourceLineMetrics(
        int LfByteCount,
        int CrByteCount,
        int RawPhysicalLineCount,
        int RawPhysicalMaxLineLength,
        int RawPhysicalLinesOver500Count,
        bool ContainsCrOnlyLineEndings);
}
