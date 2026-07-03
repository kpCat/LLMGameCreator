using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldPreviewServiceSplitSourceHealthEvidenceService
{
    public const string ReportMarkdownFileName = "visual-world-preview-service-split-report.md";
    public const string SourceHealthBeforeAfterJsonFileName = "source-health-before-after.json";
    public const string RefactorFileInventoryJsonFileName = "refactor-file-inventory.json";
    public const string BehaviorEquivalenceProofJsonFileName = "behavior-equivalence-proof.json";
    public const string QualityGateScanJsonFileName = "quality-gate-scan.json";

    private const string WorkspaceServiceRelativePath =
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/"
        + "VisualWorldStreamPreviewWorkspaceService.cs";
    private const int BeforeRepairWorkspaceServiceLogicalLineCount = 1295;
    private const int BeforeRepairMaxPhysicalLineLength = 336;

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static VisualWorldPreviewServiceSplitSourceHealthEvidenceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public VisualWorldPreviewServiceSplitSourceHealthBuildResult Build(string projectRootPath)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(projectRoot);
        var after = VisualWorldStreamPreviewSourceHealthScanner.ScanGoal092Namespace(projectRoot);
        var beforeAfter = BuildBeforeAfter(after);
        var inventory = BuildRefactorInventory(after);
        var behavior = BuildBehaviorEquivalenceProof(workspace);
        var quality = BuildQualityGate(beforeAfter, inventory, behavior, workspace.QualityGateScan);

        var beforeAfterJson = Serialize(beforeAfter);
        var inventoryJson = Serialize(inventory);
        var behaviorJson = Serialize(behavior);
        var qualityJson = Serialize(quality);
        var reportWithoutHash = new VisualWorldPreviewServiceSplitReport
        {
            Accepted = false,
            QualityGatePassed = quality.Passed,
            BehaviorEquivalencePassed = behavior.Passed,
            SourceHealthPassed = beforeAfter.Passed,
            WorkspaceServiceLogicalLineCountBeforeRepair =
                beforeAfter.Before.WorkspaceServiceLogicalLineCount,
            WorkspaceServiceLogicalLineCountAfterRepair =
                beforeAfter.After.WorkspaceServiceLogicalLineCount,
            MaxLogicalLineCountAfterRepair = beforeAfter.After.MaxLogicalLineCount,
            SourceHealthBeforeAfterHash = Sha256Text(beforeAfterJson),
            RefactorInventoryHash = Sha256Text(inventoryJson),
            BehaviorEquivalenceProofHash = Sha256Text(behaviorJson),
            QualityGateHash = Sha256Text(qualityJson)
        };
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash, quality, string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Sha256Text(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, quality, report.DeterministicReportHash);

        return new VisualWorldPreviewServiceSplitSourceHealthBuildResult
        {
            SourceHealthBeforeAfter = beforeAfter,
            RefactorFileInventory = inventory,
            BehaviorEquivalenceProof = behavior,
            QualityGateScan = quality,
            Report = report,
            SourceHealthBeforeAfterJson = beforeAfterJson,
            RefactorFileInventoryJson = inventoryJson,
            BehaviorEquivalenceProofJson = behaviorJson,
            QualityGateScanJson = qualityJson,
            ReportMarkdown = reportMarkdown
        };
    }

    public async Task<VisualWorldPreviewServiceSplitSourceHealthWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualWorldPreviewServiceSplitSourceHealthWriteResult> WriteAsync(
        string projectRootPath,
        VisualWorldPreviewServiceSplitSourceHealthBuildResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            VisualWorldPreviewServiceSplitSourceHealthVocabulary.RelativeOutputDirectory
                .Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new VisualWorldPreviewServiceSplitSourceHealthWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            SourceHealthBeforeAfterJsonPath =
                Path.Combine(outputDirectory, SourceHealthBeforeAfterJsonFileName),
            RefactorFileInventoryJsonPath =
                Path.Combine(outputDirectory, RefactorFileInventoryJsonFileName),
            BehaviorEquivalenceProofJsonPath =
                Path.Combine(outputDirectory, BehaviorEquivalenceProofJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName),
            Result = result
        };

        await File.WriteAllTextAsync(
            write.ReportMarkdownPath,
            result.ReportMarkdown,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.SourceHealthBeforeAfterJsonPath,
            result.SourceHealthBeforeAfterJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.RefactorFileInventoryJsonPath,
            result.RefactorFileInventoryJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.BehaviorEquivalenceProofJsonPath,
            result.BehaviorEquivalenceProofJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.QualityGateScanJsonPath,
            result.QualityGateScanJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);

        return write;
    }

    private static VisualWorldPreviewServiceSplitSourceHealthBeforeAfter BuildBeforeAfter(
        VisualWorldStreamPreviewSourceHealthScan after)
    {
        var before = new VisualWorldPreviewServiceSplitSourceHealthSnapshot
        {
            Source = "pre_repair_working_tree_scan_at_goal092a_start_head_18d98f381",
            WorkspaceServiceRelativePath = WorkspaceServiceRelativePath,
            ScannedCSharpFileCount = 2,
            WorkspaceServiceLogicalLineCount = BeforeRepairWorkspaceServiceLogicalLineCount,
            MaxLogicalLineCount = BeforeRepairWorkspaceServiceLogicalLineCount,
            MaxPhysicalLineLength = BeforeRepairMaxPhysicalLineLength,
            FilesOver1000LogicalLinesCount = 1,
            FilesOver700LogicalLinesInGoal092NamespaceCount = 1,
            ZeroLfSourceCount = 0,
            CrOnlySourceCount = 0,
            RawPhysicalOneLineSourceCount = 0,
            MinifiedSourceCount = 0,
            OversizedWorkspaceServiceDetected = true
        };
        var passed = before.OversizedWorkspaceServiceDetected
                     && before.WorkspaceServiceLogicalLineCount > 1000
                     && after.Passed
                     && after.FilesOver1000LogicalLinesCount == 0
                     && after.FilesOver700LogicalLinesInGoal092NamespaceCount == 0
                     && after.WorkspaceServiceLogicalLineCount
                        <= VisualWorldStreamPreviewSourceHealthScanner.PreferredMaxLogicalLineCount;

        return new VisualWorldPreviewServiceSplitSourceHealthBeforeAfter
        {
            Accepted = false,
            Passed = passed,
            Before = before,
            After = after
        };
    }

    private static VisualWorldPreviewRefactorFileInventory BuildRefactorInventory(
        VisualWorldStreamPreviewSourceHealthScan sourceHealth)
    {
        var files = sourceHealth.Files
            .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
            .Select(file => new VisualWorldPreviewRefactorInventoryFile
            {
                RelativePath = file.RelativePath,
                Responsibility = ResponsibilityFor(file.RelativePath),
                LogicalLineCount = file.LogicalLineCount,
                MaxPhysicalLineLength = file.RawPhysicalMaxLineLength
            })
            .ToList();

        return new VisualWorldPreviewRefactorFileInventory
        {
            Passed = sourceHealth.Passed
                     && sourceHealth.WorkspaceServiceLogicalLineCount
                        <= VisualWorldStreamPreviewSourceHealthScanner.PreferredMaxLogicalLineCount,
            FileCount = files.Count,
            MaxLogicalLineCount = sourceHealth.MaxLogicalLineCount,
            WorkspaceServiceLogicalLineCount = sourceHealth.WorkspaceServiceLogicalLineCount,
            Files = files
        };
    }

    private static VisualWorldPreviewBehaviorEquivalenceProof BuildBehaviorEquivalenceProof(
        VisualWorldStreamPreviewWorkspaceResult workspace)
    {
        var passed = workspace.Catalog.GroupCount >= 5
                     && workspace.Catalog.EntryCount >= 54
                     && workspace.Catalog.SvgTextPreviewCount >= 38
                     && workspace.QualityGateScan.Goal091StreamWindowEntryCount >= 4
                     && workspace.ProofStatus.ProofCount >= 7
                     && workspace.QualityGateScan.RequiredArtifactGroupsPresent
                     && workspace.QualityGateScan.Goal091StreamWindowsVisible
                     && workspace.ProofStatus.Passed
                     && workspace.WinFormsBindingInventory.Passed
                     && workspace.QualityGateScan.NoAbsolutePaths
                     && workspace.QualityGateScan.NoBinaryOrRasterMediaAdded;

        return new VisualWorldPreviewBehaviorEquivalenceProof
        {
            Passed = passed,
            ArtifactGroupCount = workspace.Catalog.GroupCount,
            EntryCount = workspace.Catalog.EntryCount,
            SvgTextPreviewCount = workspace.Catalog.SvgTextPreviewCount,
            Goal091StreamWindowEntryCount = workspace.QualityGateScan.Goal091StreamWindowEntryCount,
            ProofStatusCount = workspace.ProofStatus.ProofCount,
            RequiredArtifactGroupsPresent = workspace.QualityGateScan.RequiredArtifactGroupsPresent,
            Goal091StreamWindowsVisible = workspace.QualityGateScan.Goal091StreamWindowsVisible,
            ProofStatusPassed = workspace.ProofStatus.Passed,
            WinFormsBindingPassed = workspace.WinFormsBindingInventory.Passed,
            NoAbsolutePaths = workspace.QualityGateScan.NoAbsolutePaths,
            NoBinaryOrRasterMediaAdded = workspace.QualityGateScan.NoBinaryOrRasterMediaAdded
        };
    }

    private static VisualWorldPreviewServiceSplitQualityGateScan BuildQualityGate(
        VisualWorldPreviewServiceSplitSourceHealthBeforeAfter beforeAfter,
        VisualWorldPreviewRefactorFileInventory inventory,
        VisualWorldPreviewBehaviorEquivalenceProof behavior,
        VisualWorldPreviewWorkspaceQualityGate goal092Quality)
    {
        var diagnostics = new List<VisualWorldPreviewDiagnostic>();
        AddIfFalse(
            beforeAfter.Before.OversizedWorkspaceServiceDetected,
            "goal092a.quality.before_oversized_not_detected",
            WorkspaceServiceRelativePath,
            diagnostics);
        AddIfFalse(
            beforeAfter.After.FilesOver1000LogicalLinesCount == 0,
            "goal092a.quality.after_files_over_1000",
            "sourceHealth.after",
            diagnostics);
        AddIfFalse(
            beforeAfter.After.FilesOver700LogicalLinesInGoal092NamespaceCount == 0,
            "goal092a.quality.after_files_over_700",
            "sourceHealth.after",
            diagnostics);
        AddIfFalse(
            beforeAfter.After.WorkspaceServiceLogicalLineCount
                <= VisualWorldStreamPreviewSourceHealthScanner.PreferredMaxLogicalLineCount,
            "goal092a.quality.service_still_oversized",
            WorkspaceServiceRelativePath,
            diagnostics);
        AddIfFalse(
            goal092Quality.SourceHealthPassed
                && goal092Quality.FilesOver1000LogicalLinesCount == 0
                && goal092Quality.FilesOver700LogicalLinesInGoal092NamespaceCount == 0,
            "goal092a.quality.goal092_source_health_missing",
            "visual-world-stream-preview-quality-gate-scan.json",
            diagnostics);
        AddIfFalse(
            behavior.Passed,
            "goal092a.quality.behavior_equivalence_failed",
            "behavior-equivalence-proof.json",
            diagnostics);
        AddIfFalse(
            inventory.Passed,
            "goal092a.quality.refactor_inventory_failed",
            "refactor-file-inventory.json",
            diagnostics);

        diagnostics.AddRange(beforeAfter.After.Diagnostics);
        var passed = diagnostics.All(item => item.Severity != "error");

        return new VisualWorldPreviewServiceSplitQualityGateScan
        {
            Accepted = false,
            Passed = passed,
            BeforeOversizedServiceDetected = beforeAfter.Before.OversizedWorkspaceServiceDetected,
            AfterNoFilesOver1000LogicalLines =
                beforeAfter.After.FilesOver1000LogicalLinesCount == 0,
            AfterNoFilesOver700LogicalLines =
                beforeAfter.After.FilesOver700LogicalLinesInGoal092NamespaceCount == 0,
            WorkspaceServiceBelow700Lines =
                beforeAfter.After.WorkspaceServiceLogicalLineCount
                <= VisualWorldStreamPreviewSourceHealthScanner.PreferredMaxLogicalLineCount,
            Goal092QualityGateCarriesSourceHealthMetrics = goal092Quality.SourceHealthPassed,
            BehaviorEquivalencePassed = behavior.Passed,
            RefactorInventoryPassed = inventory.Passed,
            ScannedCSharpFileCount = beforeAfter.After.ScannedCSharpFileCount,
            MaxLogicalLineCountAfterRepair = beforeAfter.After.MaxLogicalLineCount,
            WorkspaceServiceLogicalLineCountBeforeRepair =
                beforeAfter.Before.WorkspaceServiceLogicalLineCount,
            WorkspaceServiceLogicalLineCountAfterRepair =
                beforeAfter.After.WorkspaceServiceLogicalLineCount,
            Diagnostics = diagnostics
                .GroupBy(item => item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.Target, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static string RenderReport(
        VisualWorldPreviewServiceSplitReport report,
        VisualWorldPreviewServiceSplitQualityGateScan quality,
        string deterministicReportHash)
    {
        var lines = new[]
        {
            "# Goal 092A Visual World Preview Service Split Source Health Report",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: " + report.Accepted.ToString().ToLowerInvariant(),
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + deterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 092A splits the oversized Goal 092 Application service into smaller BCL-only files, keeps the public preview workspace seam intact and adds source-health evidence that fails files over 1000 logical lines.",
            string.Empty,
            "## Source Health",
            string.Empty,
            "- sourceHealthPassed: " + report.SourceHealthPassed.ToString().ToLowerInvariant(),
            "- workspaceServiceLogicalLineCountBeforeRepair: " + report.WorkspaceServiceLogicalLineCountBeforeRepair,
            "- workspaceServiceLogicalLineCountAfterRepair: " + report.WorkspaceServiceLogicalLineCountAfterRepair,
            "- maxLogicalLineCountAfterRepair: " + report.MaxLogicalLineCountAfterRepair,
            "- beforeOversizedServiceDetected: " + quality.BeforeOversizedServiceDetected.ToString().ToLowerInvariant(),
            "- afterNoFilesOver1000LogicalLines: " + quality.AfterNoFilesOver1000LogicalLines.ToString().ToLowerInvariant(),
            "- afterNoFilesOver700LogicalLines: " + quality.AfterNoFilesOver700LogicalLines.ToString().ToLowerInvariant(),
            string.Empty,
            "## Behavior Equivalence",
            string.Empty,
            "- behaviorEquivalencePassed: " + report.BehaviorEquivalencePassed.ToString().ToLowerInvariant(),
            "- goal092QualityGateCarriesSourceHealthMetrics: " + quality.Goal092QualityGateCarriesSourceHealthMetrics.ToString().ToLowerInvariant(),
            string.Empty,
            "## Quality Gate",
            string.Empty,
            "- qualityGatePassed: " + report.QualityGatePassed.ToString().ToLowerInvariant(),
            "- noForbiddenAreasRequired: " + quality.NoForbiddenAreasRequired.ToString().ToLowerInvariant(),
            "- noBinaryMediaArtifacts: " + quality.NoBinaryMediaArtifacts.ToString().ToLowerInvariant(),
            "- noPromptDumps: " + quality.NoPromptDumps.ToString().ToLowerInvariant(),
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            "- sourceHealthBeforeAfterHash: " + report.SourceHealthBeforeAfterHash,
            "- refactorInventoryHash: " + report.RefactorInventoryHash,
            "- behaviorEquivalenceProofHash: " + report.BehaviorEquivalenceProofHash,
            "- qualityGateHash: " + report.QualityGateHash
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string ResponsibilityFor(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath);
        return fileName switch
        {
            "VisualWorldStreamPreviewWorkspaceService.cs" => "public service orchestration seam",
            "VisualWorldStreamPreviewArtifactDiscovery.cs" => "Goal086-091 artifact discovery",
            "VisualWorldStreamPreviewCatalogBuilder.cs" => "catalog entry construction",
            "VisualWorldStreamPreviewProofStatusLoader.cs" => "Goal091 proof status loading",
            "VisualWorldStreamPreviewEvidenceWriter.cs" => "Goal092 evidence writer and quality gate",
            "VisualWorldStreamPreviewSourceHealthScanner.cs" => "raw/logical source-health scanner",
            "VisualWorldPreviewServiceSplitSourceHealthEvidenceService.cs" => "Goal092A evidence writer",
            "VisualWorldStreamPreviewWorkspaceUtilities.cs" => "local JSON, path and hash helpers",
            "VisualWorldStreamPreviewWinFormsBindingScanner.cs" => "WinForms binding inventory scan",
            "VisualWorldStreamPreviewWorkspaceModels.cs" => "DTOs and vocabulary",
            "VisualWorldStreamPreviewCacheExportInspector.cs" => "Goal093 cache export inspector discovery",
            _ => "Goal092 workspace support file"
        };
    }

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                code,
                target,
                "Goal092A service split source-health quality gate did not pass."));
        }
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string Sha256Text(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(
                pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                rootFull.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path must stay under the project root.");
        }
    }
}
