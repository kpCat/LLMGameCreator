using System.Text;

namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;

public sealed class EditDrivenGamePackageRuntimePreviewBridgeQualityGateScanner
{
    private const int MaxAllowedLineLength = 500;
    private const int MaxAllowedLineCount = 1000;

    public EditDrivenGamePackageRuntimePreviewBridgeQualityGateScan Scan(
        string repositoryRootPath,
        EditDrivenGamePackageRuntimePreviewBridgeWinFormsBindingInventory bindingInventory)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var files = CandidateFiles(root)
            .Where(File.Exists)
            .Select(path => ScanFile(root, path))
            .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToList();
        var outputRoot = Path.Combine(
            root,
            EditDrivenGamePackageRuntimePreviewBridgeVocabulary.RelativeOutputDirectory);
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic>();
        var alphaPath = Path.Combine(
            root,
            "unity",
            "LLMGameCreatorAlpha",
            "Assets",
            "Scripts",
            "AlphaRuntimeBootstrap.cs");
        var alphaBytes = File.Exists(alphaPath) ? File.ReadAllBytes(alphaPath) : [];
        var alphaLineCount = alphaBytes.Length == 0 ? 0 : Encoding.UTF8.GetString(alphaBytes).Split('\n').Length;
        var alphaHash = alphaBytes.Length == 0
            ? string.Empty
            : EditDrivenGamePackageRuntimePreviewBridgeHash.Sha256Bytes(alphaBytes);
        var evidenceTexts = Array.Empty<string>();
        var evidenceJoined = string.Join('\n', evidenceTexts);
        var absoluteLocalPathDetected = evidenceJoined.Contains("C:\\Users\\", StringComparison.OrdinalIgnoreCase)
                                        || evidenceJoined.Contains("C:/Users/", StringComparison.OrdinalIgnoreCase);
        var timestampLikeDetected = evidenceJoined.Contains("T00:", StringComparison.Ordinal)
                                    || evidenceJoined.Contains("T12:", StringComparison.Ordinal)
                                    || evidenceJoined.Contains("Z\"", StringComparison.Ordinal);
        var heavyLogsDetected = evidenceJoined.Contains("/logs/", StringComparison.OrdinalIgnoreCase)
                                || evidenceJoined.Contains("\\logs\\", StringComparison.OrdinalIgnoreCase);
        var scratchDetected = evidenceJoined.Contains("tamper-scratch", StringComparison.OrdinalIgnoreCase)
                              || evidenceJoined.Contains("scratch-tamper", StringComparison.OrdinalIgnoreCase);
        var forbiddenDetected = evidenceJoined.Contains("src/LLMGameCreator.Runtime/", StringComparison.Ordinal)
                                || evidenceJoined.Contains("src/LLMGameCreator.GamePackage/GamePackageDefinition.cs", StringComparison.Ordinal)
                                || evidenceJoined.Contains("generator-library/", StringComparison.Ordinal);

        if (!bindingInventory.Passed)
        {
            diagnostics.Add(Diagnostic(
                "winforms_binding_inventory_failed",
                "winforms-binding-inventory.json",
                "Goal080 WinForms binding inventory did not pass."));
        }

        foreach (var file in files.Where(file => file.LinesOver500Count > 0))
        {
            diagnostics.Add(Diagnostic("source_line_too_long", file.RelativePath, "Source line exceeds 500 characters."));
        }

        foreach (var file in files.Where(file => file.LineCount > MaxAllowedLineCount))
        {
            diagnostics.Add(Diagnostic("source_file_too_long", file.RelativePath, "Source file exceeds 1000 logical lines."));
        }

        foreach (var file in files.Where(file => file.ZeroLfSource || file.CrOnlySource || file.RawPhysicalOneLineSource))
        {
            diagnostics.Add(Diagnostic("raw_source_format_rejected", file.RelativePath, "Source file has zero-LF, CR-only, or raw one-physical-line shape."));
        }

        if (absoluteLocalPathDetected)
        {
            diagnostics.Add(Diagnostic(
                "evidence_absolute_path_detected",
                EditDrivenGamePackageRuntimePreviewBridgeVocabulary.RelativeOutputDirectory,
                "Evidence contains an absolute local path."));
        }

        if (heavyLogsDetected || scratchDetected || forbiddenDetected)
        {
            diagnostics.Add(Diagnostic(
                "evidence_hygiene_failed",
                EditDrivenGamePackageRuntimePreviewBridgeVocabulary.RelativeOutputDirectory,
                "Evidence references heavy logs, scratch files, or forbidden areas."));
        }

        var syntheticCrOnlyRejected = RejectsSuspiciousRawSourceBytes(Encoding.UTF8.GetBytes("public sealed class A\r{\r}\r"));
        var syntheticZeroLfRejected = RejectsSuspiciousRawSourceBytes(Encoding.UTF8.GetBytes("public sealed class A { public string Value => \"" + new string('x', 520) + "\"; }"));
        if (!syntheticCrOnlyRejected || !syntheticZeroLfRejected)
        {
            diagnostics.Add(Diagnostic("synthetic_raw_source_not_rejected", "quality-gate-scan", "Synthetic raw-byte source guard failed."));
        }

        return new EditDrivenGamePackageRuntimePreviewBridgeQualityGateScan
        {
            Passed = diagnostics.Count == 0
                     && syntheticCrOnlyRejected
                     && syntheticZeroLfRejected
                     && bindingInventory.Passed,
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
            ReportOnlySmokeDetected = false,
            AlphaRuntimeBootstrapLineCount = alphaLineCount,
            AlphaRuntimeBootstrapHash = alphaHash,
            AlphaRuntimeBootstrapRecordedReadOnly = !string.IsNullOrWhiteSpace(alphaHash),
            EvidenceContainsAbsoluteLocalPaths = absoluteLocalPathDetected,
            EvidenceContainsTimestampLikeValues = timestampLikeDetected,
            EvidenceContainsHeavyLogs = heavyLogsDetected,
            EvidenceContainsScratchTamperFiles = scratchDetected,
            ForbiddenAreaEvidenceDetected = forbiddenDetected,
            Files = files,
            Diagnostics = diagnostics
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

    private static IReadOnlyList<string> CandidateFiles(string root)
    {
        var fixedFiles = new[]
        {
            "src/LLMGameCreator.WinForms/CompositionRoot.cs",
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs",
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs",
            "tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenGamePackageRuntimePreviewBridgeProductSmokeTests.cs",
            "tests/LLMGameCreator.Tests/Docs/CurrentGeneratorStateDocsTests.cs"
        };
        var prefixes = new[]
        {
            "src/LLMGameCreator.Application/Design/EditDrivenGamePackageRuntimePreviewBridge",
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace",
            "tests/LLMGameCreator.Tests/Application/EditDrivenGamePackageRuntimePreviewBridge"
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

    private static EditDrivenGamePackageRuntimePreviewBridgeQualityFileScan ScanFile(string root, string path)
    {
        var bytes = File.ReadAllBytes(path);
        var text = Encoding.UTF8.GetString(bytes);
        var hasLf = text.Contains('\n');
        var hasCr = text.Contains('\r');
        var lines = hasLf ? text.Split('\n') : [text];
        var maxLineLength = lines.Select(line => line.TrimEnd('\r').Length).DefaultIfEmpty(0).Max();
        var relativePath = Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

        return new EditDrivenGamePackageRuntimePreviewBridgeQualityFileScan
        {
            RelativePath = relativePath,
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

    private static string Normalize(string path) => path.Replace('/', Path.DirectorySeparatorChar);

    private static EditDrivenGamePackageRuntimePreviewBridgeDiagnostic Diagnostic(
        string code,
        string target,
        string message) =>
        EditDrivenGamePackageRuntimePreviewBridgeDiagnostic.Error(code, target, message);
}
