namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    public async Task<VisualWorldStreamPreviewWorkspaceWriteResult> WriteAsync(
        string projectRootPath,
        VisualWorldStreamPreviewWorkspaceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            VisualWorldStreamPreviewWorkspaceVocabulary.RelativeOutputDirectory
                .Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new VisualWorldStreamPreviewWorkspaceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            CatalogJsonPath = Path.Combine(outputDirectory, CatalogJsonFileName),
            ProofStatusJsonPath = Path.Combine(outputDirectory, ProofStatusJsonFileName),
            WinFormsBindingInventoryJsonPath =
                Path.Combine(outputDirectory, WinFormsBindingInventoryJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName),
            Result = result
        };

        await File.WriteAllTextAsync(
            write.ReportMarkdownPath,
            result.ReportMarkdown,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.CatalogJsonPath,
            result.CatalogJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.ProofStatusJsonPath,
            result.ProofStatusJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.WinFormsBindingInventoryJsonPath,
            result.WinFormsBindingInventoryJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.QualityGateScanJsonPath,
            result.QualityGateScanJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);

        return write;
    }

    private static VisualWorldPreviewWorkspaceQualityGate BuildQualityGate(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewSvgEntry> svgEntries,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs,
        VisualWorldPreviewWinFormsBindingInventory binding,
        VisualWorldStreamPreviewSourceHealthScan sourceHealth,
        IReadOnlyList<VisualWorldPreviewDiagnostic> sourceDiagnostics)
    {
        var diagnostics = new List<VisualWorldPreviewDiagnostic>();
        var groupCount = groups.Count;
        var entryCount = groups.Sum(group => group.EntryCount);
        var goal091StreamEntries = groups
            .Where(group => group.GroupId == "chunk_stream_windows")
            .SelectMany(group => group.Entries)
            .Count(entry => entry.ArtifactKind == "text_svg_chunk_stream_window_overview");
        var requiredGroups = new[]
        {
            "microtiles",
            "map_patches",
            "region_composer",
            "world_profiles",
            "chunk_stream_windows"
        };
        var requiredArtifactGroupsPresent = requiredGroups.All(required =>
            groups.Any(group => group.GroupId == required && group.EntryCount > 0));
        var noAbsolutePaths = groups.SelectMany(group => group.Entries)
            .Select(entry => entry.RelativePath)
            .Concat(svgEntries.Select(entry => entry.RelativePath))
            .All(IsSafeRelativePath);
        var noBinaryMedia = groups.SelectMany(group => group.Entries)
            .Select(entry => entry.RelativePath)
            .Concat(svgEntries.Select(entry => entry.RelativePath))
            .All(path => !IsBinaryOrRasterMedia(path));
        var proofStatusPassed = proofs.Count >= 7 && proofs.All(item => item.Passed);

        AddIfFalse(requiredArtifactGroupsPresent, "goal092.quality.groups_missing", "catalog", diagnostics);
        AddIfFalse(svgEntries.Count >= 4, "goal092.quality.svg_count", "catalog.svgEntries", diagnostics);
        AddIfFalse(goal091StreamEntries >= 4, "goal092.quality.goal091_missing", "chunk_stream_windows", diagnostics);
        AddIfFalse(proofStatusPassed, "goal092.quality.proofs_failed", "proofStatus", diagnostics);
        AddIfFalse(noAbsolutePaths, "goal092.quality.absolute_path", "catalog", diagnostics);
        AddIfFalse(noBinaryMedia, "goal092.quality.binary_media", "catalog", diagnostics);
        AddIfFalse(binding.Passed, "goal092.quality.winforms_binding", "winformsBinding", diagnostics);
        AddIfFalse(sourceHealth.Passed, "goal092.quality.source_health", "sourceHealth", diagnostics);
        foreach (var diagnostic in sourceDiagnostics
                     .Concat(sourceHealth.Diagnostics)
                     .Where(item => item.Severity == "error"))
        {
            diagnostics.Add(diagnostic);
        }

        var passed = diagnostics.All(item => item.Severity != "error");
        return new VisualWorldPreviewWorkspaceQualityGate
        {
            Accepted = false,
            Passed = passed,
            GroupCount = groupCount,
            EntryCount = entryCount,
            SvgTextPreviewCount = svgEntries.Count,
            Goal091StreamWindowEntryCount = goal091StreamEntries,
            RequiredArtifactGroupsPresent = requiredArtifactGroupsPresent,
            Goal091StreamWindowsVisible = goal091StreamEntries >= 4,
            ProofStatusPassed = proofStatusPassed,
            NoAbsolutePaths = noAbsolutePaths,
            NoBinaryOrRasterMediaAdded = noBinaryMedia,
            WinFormsBindingReal = binding.Passed,
            SourceHealthPassed = sourceHealth.Passed,
            ScannedCSharpFileCount = sourceHealth.ScannedCSharpFileCount,
            MaxLogicalLineCount = sourceHealth.MaxLogicalLineCount,
            MaxPhysicalLineLength = sourceHealth.MaxPhysicalLineLength,
            FilesOver1000LogicalLinesCount = sourceHealth.FilesOver1000LogicalLinesCount,
            FilesOver700LogicalLinesInGoal092NamespaceCount =
                sourceHealth.FilesOver700LogicalLinesInGoal092NamespaceCount,
            ZeroLfSourceCount = sourceHealth.ZeroLfSourceCount,
            CrOnlySourceCount = sourceHealth.CrOnlySourceCount,
            RawPhysicalOneLineSourceCount = sourceHealth.RawPhysicalOneLineSourceCount,
            MinifiedSourceCount = sourceHealth.MinifiedSourceCount,
            WorkspaceServiceLogicalLineCount = sourceHealth.WorkspaceServiceLogicalLineCount,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                ".llmgc/procedural/goal-092-visual-world-stream-preview-workspace/",
                ".llmgc/procedural/goal-092a-visual-world-preview-service-split-source-health/",
                "docs/agent-tasks/goal-092-visual-world-stream-preview-workspace/",
                "docs/agent-tasks/goal-092a-visual-world-preview-service-split-source-health/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = diagnostics
                .GroupBy(item => item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.Target, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static VisualWorldStreamPreviewWorkspaceReport BuildReport(
        VisualWorldStreamPreviewCatalog catalog,
        VisualWorldStreamPreviewProofStatusDocument proofStatus,
        VisualWorldPreviewWinFormsBindingInventory binding,
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        string catalogJson,
        string proofStatusJson,
        string bindingJson,
        string qualityJson) =>
        new()
        {
            Accepted = false,
            GroupCount = catalog.GroupCount,
            EntryCount = catalog.EntryCount,
            SvgTextPreviewCount = catalog.SvgTextPreviewCount,
            Goal091StreamWindowEntryCount = qualityGate.Goal091StreamWindowEntryCount,
            ProofStatusPassed = proofStatus.Passed,
            WinFormsBindingPassed = binding.Passed,
            QualityGatePassed = qualityGate.Passed,
            SourceHealthPassed = qualityGate.SourceHealthPassed,
            WorkspaceServiceLogicalLineCount = qualityGate.WorkspaceServiceLogicalLineCount,
            MaxLogicalLineCount = qualityGate.MaxLogicalLineCount,
            FilesOver1000LogicalLinesCount = qualityGate.FilesOver1000LogicalLinesCount,
            FilesOver700LogicalLinesInGoal092NamespaceCount =
                qualityGate.FilesOver700LogicalLinesInGoal092NamespaceCount,
            CatalogHash = Sha256Text(catalogJson),
            ProofStatusHash = Sha256Text(proofStatusJson),
            WinFormsBindingInventoryHash = Sha256Text(bindingJson),
            QualityGateHash = Sha256Text(qualityJson)
        };

    private static string RenderReport(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldStreamPreviewCatalog catalog,
        VisualWorldStreamPreviewProofStatusDocument proofStatus,
        VisualWorldPreviewWinFormsBindingInventory binding,
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 092 Visual World Stream Preview Workspace Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 092 adds a BCL-only Application review seam and WinForms workspace over the deterministic visual world artifacts from Goals 086-091. It loads existing JSON/text-SVG evidence by repository-relative path and does not add runtime, Unity, provider, schema, project-file, dependency, binary media or raster media changes.",
            string.Empty,
            "## Catalog",
            string.Empty,
            $"- groupCount: {catalog.GroupCount}",
            $"- entryCount: {catalog.EntryCount}",
            $"- svgTextPreviewCount: {catalog.SvgTextPreviewCount}",
            $"- goal091StreamWindowEntryCount: {qualityGate.Goal091StreamWindowEntryCount}",
            string.Empty
        };
        lines.AddRange(catalog.Groups.Select(group =>
            "- " + group.GroupId + ": entries=" + group.EntryCount
            + ", svgEntries=" + group.SvgEntryCount
            + ", sourceGoal=" + group.SourceGoalId
            + ", status=" + group.Status));
        lines.AddRange(
        [
            string.Empty,
            "## Proof Status",
            string.Empty,
            $"- proofStatusPassed: {proofStatus.Passed.ToString().ToLowerInvariant()}",
            $"- proofCount: {proofStatus.ProofCount}"
        ]);
        lines.AddRange(proofStatus.Proofs.Select(proof =>
            "- " + proof.ProofId + ": passed=" + proof.Passed.ToString().ToLowerInvariant()
            + ", path=" + proof.RelativePath));
        lines.AddRange(
        [
            string.Empty,
            "## WinForms Binding",
            string.Empty,
            $"- bindingPassed: {binding.Passed.ToString().ToLowerInvariant()}",
            $"- pageControlExists: {binding.PageControlExists.ToString().ToLowerInvariant()}",
            $"- designerExists: {binding.DesignerExists.ToString().ToLowerInvariant()}",
            $"- compositionRootRegistersService: {binding.CompositionRootRegistersService.ToString().ToLowerInvariant()}",
            $"- compositionRootRegistersPage: {binding.CompositionRootRegistersPage.ToString().ToLowerInvariant()}",
            $"- editorRegistryIncludesPage: {binding.EditorRegistryIncludesPage.ToString().ToLowerInvariant()}",
            $"- pageActivationLoadsApplicationResult: {binding.PageActivationLoadsApplicationResult.ToString().ToLowerInvariant()}",
            $"- pageBindDisplaysGroupsEntriesProofs: {binding.PageBindDisplaysGroupsEntriesProofs.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Source Health",
            string.Empty,
            $"- sourceHealthPassed: {qualityGate.SourceHealthPassed.ToString().ToLowerInvariant()}",
            $"- scannedCSharpFileCount: {qualityGate.ScannedCSharpFileCount}",
            $"- workspaceServiceLogicalLineCount: {qualityGate.WorkspaceServiceLogicalLineCount}",
            $"- maxLogicalLineCount: {qualityGate.MaxLogicalLineCount}",
            $"- maxPhysicalLineLength: {qualityGate.MaxPhysicalLineLength}",
            $"- filesOver1000LogicalLinesCount: {qualityGate.FilesOver1000LogicalLinesCount}",
            $"- filesOver700LogicalLinesInGoal092NamespaceCount: {qualityGate.FilesOver700LogicalLinesInGoal092NamespaceCount}",
            $"- zeroLfSourceCount: {qualityGate.ZeroLfSourceCount}",
            $"- crOnlySourceCount: {qualityGate.CrOnlySourceCount}",
            $"- rawPhysicalOneLineSourceCount: {qualityGate.RawPhysicalOneLineSourceCount}",
            $"- minifiedSourceCount: {qualityGate.MinifiedSourceCount}",
            string.Empty,
            "## Quality Gate",
            string.Empty,
            $"- qualityGatePassed: {qualityGate.Passed.ToString().ToLowerInvariant()}",
            $"- requiredArtifactGroupsPresent: {qualityGate.RequiredArtifactGroupsPresent.ToString().ToLowerInvariant()}",
            $"- goal091StreamWindowsVisible: {qualityGate.Goal091StreamWindowsVisible.ToString().ToLowerInvariant()}",
            $"- noAbsolutePaths: {qualityGate.NoAbsolutePaths.ToString().ToLowerInvariant()}",
            $"- noBinaryOrRasterMediaAdded: {qualityGate.NoBinaryOrRasterMediaAdded.ToString().ToLowerInvariant()}",
            $"- noRuntimeUnityProviderSchemaProjectDependencyChanges: {qualityGate.NoRuntimeUnityProviderSchemaProjectDependencyChanges.ToString().ToLowerInvariant()}",
            $"- noPromptDumps: {qualityGate.NoPromptDumps.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- catalogHash: {report.CatalogHash}",
            $"- proofStatusHash: {report.ProofStatusHash}",
            $"- winFormsBindingInventoryHash: {report.WinFormsBindingInventoryHash}",
            $"- qualityGateHash: {report.QualityGateHash}"
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
