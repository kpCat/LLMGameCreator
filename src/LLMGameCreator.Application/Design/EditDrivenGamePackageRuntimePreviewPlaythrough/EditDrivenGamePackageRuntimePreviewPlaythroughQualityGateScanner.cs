using System.Text;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;

public sealed class EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScanner
{
    private const int MaxAllowedLineLength = 500;
    private const int MaxAllowedLineCount = 1000;
    private const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    private const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";

    private static readonly Regex TimestampLikePattern = new(
        @"\b20\d{2}-\d{2}-\d{2}[T ][0-2]\d:[0-5]\d:[0-5]\d",
        RegexOptions.Compiled);

    public EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScan Scan(
        string repositoryRootPath,
        EditDrivenGamePackageRuntimePreviewPlaythroughWinFormsBindingInventory bindingInventory,
        IReadOnlyDictionary<string, string> evidencePayloads)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var files = CandidateFiles(root)
            .Where(File.Exists)
            .Select(path => ScanFile(root, path))
            .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToList();
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic>();
        var evidenceScan = ScanEvidencePayloads(evidencePayloads);
        var reportOnlySmoke = DetectReportOnlySmoke(root);
        var alphaPath = Path.Combine(root, Normalize(AlphaRuntimeBootstrapPath));
        var alphaBytes = File.Exists(alphaPath) ? File.ReadAllBytes(alphaPath) : [];
        var alphaLineCount = alphaBytes.Length == 0 ? 0 : Encoding.UTF8.GetString(alphaBytes).Split('\n').Length;
        var alphaHash = alphaBytes.Length == 0
            ? string.Empty
            : EditDrivenGamePackageRuntimePreviewPlaythroughHash.Sha256Bytes(alphaBytes);
        var alphaUnchanged = string.Equals(
            alphaHash,
            AlphaRuntimeBootstrapExpectedHash,
            StringComparison.OrdinalIgnoreCase);

        if (!bindingInventory.Passed)
        {
            diagnostics.Add(Error(
                "goal081.quality.winforms_binding_failed",
                "winforms-binding-inventory.json",
                "Goal081 parent WinForms binding inventory did not pass."));
        }

        foreach (var file in files.Where(file => file.LinesOver500Count > 0))
        {
            diagnostics.Add(Error("goal081.quality.source_line_too_long", file.RelativePath, "Source line exceeds 500 characters."));
        }

        foreach (var file in files.Where(file => file.LineCount > MaxAllowedLineCount))
        {
            diagnostics.Add(Error("goal081.quality.source_file_too_long", file.RelativePath, "Goal081 source file exceeds 1000 logical lines."));
        }

        foreach (var file in files.Where(file => file.ZeroLfSource || file.CrOnlySource || file.RawPhysicalOneLineSource))
        {
            diagnostics.Add(Error(
                "goal081.quality.raw_source_format_rejected",
                file.RelativePath,
                "Source file has zero-LF, CR-only, or raw one-physical-line shape."));
        }

        if (reportOnlySmoke)
        {
            diagnostics.Add(Error(
                "goal081.quality.report_only_smoke",
                "EditDrivenGamePackageRuntimePreviewPlaythroughProductSmokeTests.cs",
                "Product smoke must read package, command, transcript and negative proof artifacts."));
        }

        if (!alphaUnchanged)
        {
            diagnostics.Add(Error(
                "goal081.quality.alpha_runtime_bootstrap_changed",
                AlphaRuntimeBootstrapPath,
                "Goal081 must keep AlphaRuntimeBootstrap.cs read-only and unchanged."));
        }

        var syntheticCrOnlyRejected = RejectsSuspiciousRawSourceBytes(Encoding.UTF8.GetBytes("public sealed class A\r{\r}\r"));
        var syntheticZeroLfRejected = RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("public sealed class A { public string Value => \"" + new string('x', 520) + "\"; }"));
        if (!syntheticCrOnlyRejected || !syntheticZeroLfRejected)
        {
            diagnostics.Add(Error(
                "goal081.quality.synthetic_raw_source_not_rejected",
                "quality-gate-scan",
                "Synthetic raw-byte source guard failed."));
        }

        diagnostics.AddRange(evidenceScan.Diagnostics);

        return new EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScan
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
            ParentUiBindingPassed = bindingInventory.Passed,
            ReportOnlySmokeDetected = reportOnlySmoke,
            AlphaRuntimeBootstrapLineCount = alphaLineCount,
            AlphaRuntimeBootstrapHash = alphaHash,
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

    public static EditDrivenGamePackageRuntimePreviewPlaythroughWinFormsBindingInventory BuildWinFormsBindingInventory(
        string repositoryRootPath)
    {
        const string parentCs =
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs";
        const string parentDesigner =
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs";
        const string childCs =
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignGamePackageRuntimePreviewPlaythroughControl.cs";
        var root = Path.GetFullPath(repositoryRootPath);
        var parentCode = SafeRead(root, parentCs);
        var compactParent = Compact(parentCode);
        var designerCode = SafeRead(root, parentDesigner);
        var childCode = SafeRead(root, childCs);
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic>();
        var tabDeclared = designerCode.Contains("_runtimePreviewPlaythroughTabPage", StringComparison.Ordinal)
                          && designerCode.Contains("_runtimePreviewPlaythroughControl", StringComparison.Ordinal)
                          && designerCode.Contains("CampaignGamePackageRuntimePreviewPlaythroughControl", StringComparison.Ordinal);
        var serviceLoaded = parentCode.Contains(
                                "EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService",
                                StringComparison.Ordinal)
                            && compactParent.Contains(
                                "_runtimePreviewPlaythroughService.BuildAndWriteAsync(root).GetAwaiter().GetResult().Result",
                                StringComparison.Ordinal);
        var controlBound = parentCode.Contains(
                               "EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult",
                               StringComparison.Ordinal)
                           && compactParent.Contains(
                               "_runtimePreviewPlaythroughControl.Bind(runtimePreviewPlaythroughResult)",
                               StringComparison.Ordinal)
                           && childCode.Contains(
                               "Bind(EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult result)",
                               StringComparison.Ordinal);
        var activationBinds = tabDeclared && serviceLoaded && controlBound;

        if (!File.Exists(Path.Combine(root, Normalize(childCs))))
        {
            diagnostics.Add(Error("goal081.winforms.control_missing", childCs, "Required Goal081 control is missing."));
        }

        if (!tabDeclared)
        {
            diagnostics.Add(Error(
                "goal081.winforms.tab_missing",
                parentDesigner,
                "Parent workspace must declare a separate Goal081 playthrough tab/control."));
        }

        if (!serviceLoaded)
        {
            diagnostics.Add(Error(
                "goal081.winforms.service_missing",
                parentCs,
                "Parent workspace activation must load Goal081 evidence service."));
        }

        if (!controlBound)
        {
            diagnostics.Add(Error(
                "goal081.winforms.control_bind_missing",
                parentCs,
                "Parent workspace must bind the Goal081 result into CampaignGamePackageRuntimePreviewPlaythroughControl."));
        }

        return new EditDrivenGamePackageRuntimePreviewPlaythroughWinFormsBindingInventory
        {
            Passed = diagnostics.Count == 0,
            ParentPagePlaythroughTabDeclared = tabDeclared,
            ParentPagePlaythroughServiceLoaded = serviceLoaded,
            ParentPagePlaythroughControlBound = controlBound,
            ParentPageActivationBindsGoal081Data = activationBinds,
            Groups =
            [
                new EditDrivenGamePackageRuntimePreviewPlaythroughWinFormsBindingGroup
                {
                    GroupId = "goal081_runtime_preview_playthrough_tab",
                    ControlName = "CampaignGamePackageRuntimePreviewPlaythroughControl",
                    RelativePath = childCs,
                    SeparateUserControl = childCode.Contains(": UserControl", StringComparison.Ordinal),
                    BindsGoal081Data = childCode.Contains(
                        "Bind(EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult result)",
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

    public static IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity == "error" ? 0 : 1)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<string> CandidateFiles(string root)
    {
        var fixedFiles = new[]
        {
            "src/LLMGameCreator.WinForms/CompositionRoot.cs",
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs",
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs",
            "tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenGamePackageRuntimePreviewPlaythroughProductSmokeTests.cs",
            "tests/LLMGameCreator.Tests/Docs/CurrentGeneratorStateDocsTests.cs"
        };
        var prefixes = new[]
        {
            "src/LLMGameCreator.Application/Design/EditDrivenGamePackageRuntimePreviewPlaythrough",
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace",
            "tests/LLMGameCreator.Tests/Application/EditDrivenGamePackageRuntimePreviewPlaythrough"
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

    private static EditDrivenGamePackageRuntimePreviewPlaythroughQualityFileScan ScanFile(string root, string path)
    {
        var bytes = File.ReadAllBytes(path);
        var text = Encoding.UTF8.GetString(bytes);
        var hasLf = text.Contains('\n');
        var hasCr = text.Contains('\r');
        var lines = hasLf ? text.Split('\n') : [text];
        var maxLineLength = lines.Select(line => line.TrimEnd('\r').Length).DefaultIfEmpty(0).Max();

        return new EditDrivenGamePackageRuntimePreviewPlaythroughQualityFileScan
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
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic>();
        var containsAbsolutePath = false;
        var containsTimestamp = false;
        var containsHeavyLogs = false;
        var containsScratch = false;
        var containsForbidden = false;

        foreach (var pair in evidencePayloads.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            if (pair.Value.Contains(@"C:\", StringComparison.OrdinalIgnoreCase)
                || pair.Value.Contains("C:/", StringComparison.OrdinalIgnoreCase)
                || pair.Value.Contains("/Users/", StringComparison.OrdinalIgnoreCase))
            {
                containsAbsolutePath = true;
                diagnostics.Add(Error(
                    "goal081.evidence.absolute_path",
                    pair.Key,
                    "Tracked Goal081 evidence must not contain absolute local paths."));
            }

            if (TimestampLikePattern.IsMatch(pair.Value))
            {
                containsTimestamp = true;
                diagnostics.Add(Error(
                    "goal081.evidence.timestamp_like_value",
                    pair.Key,
                    "Tracked Goal081 evidence must not contain timestamp-like values."));
            }

            if (pair.Key.Contains("/logs/", StringComparison.OrdinalIgnoreCase)
                || pair.Key.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            {
                containsHeavyLogs = true;
                diagnostics.Add(Error(
                    "goal081.evidence.heavy_log",
                    pair.Key,
                    "Tracked Goal081 evidence must not contain heavy logs."));
            }

            if (pair.Key.Contains("tamper", StringComparison.OrdinalIgnoreCase)
                || pair.Value.Contains("tamper-scratch", StringComparison.OrdinalIgnoreCase)
                || pair.Value.Contains("scratch-tamper", StringComparison.OrdinalIgnoreCase))
            {
                containsScratch = true;
                diagnostics.Add(Error(
                    "goal081.evidence.scratch_tamper_file",
                    pair.Key,
                    "Negative proof must not leave scratch tamper files in tracked evidence."));
            }

            if (pair.Value.Contains("src/LLMGameCreator.Runtime/", StringComparison.Ordinal)
                || pair.Value.Contains("src/LLMGameCreator.GamePackage/GamePackageDefinition.cs", StringComparison.Ordinal)
                || pair.Value.Contains("generator-library/", StringComparison.Ordinal))
            {
                containsForbidden = true;
                diagnostics.Add(Error(
                    "goal081.evidence.forbidden_area",
                    pair.Key,
                    "Goal081 evidence must not materialize forbidden area mutations."));
            }
        }

        return new EvidencePayloadScan(
            containsAbsolutePath,
            containsTimestamp,
            containsHeavyLogs,
            containsScratch,
            containsForbidden,
            SortDiagnostics(diagnostics));
    }

    private static bool DetectReportOnlySmoke(string root)
    {
        var path = Path.Combine(
            root,
            Normalize("tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenGamePackageRuntimePreviewPlaythroughProductSmokeTests.cs"));
        if (!File.Exists(path))
        {
            return false;
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        return text.Contains("ImplementationStatus", StringComparison.Ordinal)
               && !text.Contains("package-read-proof.json", StringComparison.Ordinal)
               && !text.Contains("playthrough-command-script.json", StringComparison.Ordinal)
               && !text.Contains("playthrough-transcript.json", StringComparison.Ordinal)
               && !text.Contains("playthrough-negative-proof.json", StringComparison.Ordinal);
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

    private static EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic Error(
        string code,
        string target,
        string message) =>
        EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic.Error(code, target, message);

    private sealed record EvidencePayloadScan(
        bool ContainsAbsoluteLocalPaths,
        bool ContainsTimestampLikeValues,
        bool ContainsHeavyLogs,
        bool ContainsScratchTamperFiles,
        bool ContainsForbiddenAreaEvidence,
        IReadOnlyList<EditDrivenGamePackageRuntimePreviewPlaythroughDiagnostic> Diagnostics);
}
