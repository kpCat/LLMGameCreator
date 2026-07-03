using System.Text.Json;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal095SourceGoalId =
        "goal_095_visual_chunk_cache_unity_streamingassets_handoff";
    private const string Goal095SourceRoot =
        ".llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff";
    private const string Goal095StreamingAssetsRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/VisualChunkCacheGoal095";
    private const string Goal095ProbeSourcePath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/VisualChunkCacheHandoffProbe.cs";
    private const string Goal095AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";

    private static VisualWorldPreviewArtifactGroup BuildUnityHandoffGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadUnityHandoffSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                Goal095SourceRoot,
                Goal095SourceGoalId,
                [
                    ("visual-chunk-cache-unity-handoff-report.md", "unity_handoff_report"),
                    ("visual-chunk-cache-unity-handoff-manifest.json", "unity_handoff_manifest"),
                    ("visual-chunk-cache-unity-streamingassets-ledger.json", "streamingassets_payload_ledger"),
                    ("visual-chunk-cache-unity-probe-source-inventory.json", "unity_probe_source_inventory"),
                    ("visual-chunk-cache-unity-simulated-read-proof.json", "simulated_unity_read_proof"),
                    ("visual-chunk-cache-unity-negative-proof.json", "unity_handoff_negative_proof"),
                    ("visual-chunk-cache-unity-quality-gate-scan.json", "unity_handoff_quality_gate")
                ],
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithUnityHandoffSummary(entry, summary))
            .ToList();

        AddUnityPayloadRootEntry(entries, summary);
        foreach (var file in summary.PayloadFiles)
        {
            entries.Add(BuildUnityPayloadFileEntry(file, summary));
        }

        entries.Add(BuildAlphaRuntimeBootstrapEntry(projectRoot, summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "unity_handoff",
            "Goal 095 Unity Handoff",
            Goal095SourceGoalId,
            Goal095SourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry WithUnityHandoffSummary(
        VisualWorldPreviewArtifactEntry entry,
        UnityHandoffProofSummary summary) =>
        entry with
        {
            PayloadFileCount = summary.PayloadFileCount,
            PackageCount = summary.PackageCount,
            ExportRecordCount = summary.ExportRecordCount,
            StreamWindowCount = summary.StreamWindowCount,
            UniqueChunkKeyCount = summary.UniqueChunkKeyCount,
            SimulatedUnityReadProofPassed = summary.SimulatedReadProofPassed,
            NegativeProofPassed = summary.NegativeProofPassed,
            ProbeSourceInventoryPassed = summary.ProbeSourceInventoryPassed,
            AlphaRuntimeBootstrapUnchanged = summary.AlphaRuntimeBootstrapUnchanged,
            ForbiddenUnityAreasUnchanged = summary.ForbiddenUnityAreasUnchanged,
            MetadataOnly = summary.MetadataOnly,
            PayloadHashesMatchGoal095Ledger = summary.PayloadHashesMatchGoal095Ledger,
            NoUnityFilesChangedByGoal096 = summary.NoUnityFilesChangedByGoal096
        };

    private static void AddUnityPayloadRootEntry(
        List<VisualWorldPreviewArtifactEntry> entries,
        UnityHandoffProofSummary summary)
    {
        entries.Add(WithUnityHandoffSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal095SourceGoalId + ".streamingassets_payload_root",
                RelativePath = Goal095StreamingAssetsRoot,
                ArtifactKind = "streamingassets_payload_root",
                SourceGoalId = Goal095SourceGoalId,
                Status = summary.PayloadRootExists
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "payloadFiles=" + summary.PayloadFileCount
                    + "; unityProbeRoot=LLMGameCreator/VisualChunkCacheGoal095",
                SafeRatingMetadataSummary = "metadataOnly="
                    + summary.MetadataOnly.ToString().ToLowerInvariant()
            },
            summary));
    }

    private static VisualWorldPreviewArtifactEntry BuildUnityPayloadFileEntry(
        UnityHandoffPayloadFile file,
        UnityHandoffProofSummary summary) =>
        WithUnityHandoffSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal095SourceGoalId + ".payload." + file.Role,
                RelativePath = file.RepositoryRelativePath,
                ArtifactKind = "streamingassets_payload_" + file.Role,
                SourceGoalId = Goal095SourceGoalId,
                Sha256 = string.IsNullOrWhiteSpace(file.ActualSha256)
                    ? file.DeclaredSha256
                    : file.ActualSha256,
                Status = file.Exists && file.HashMatches
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "role=" + file.Role
                    + "; byteCount=" + file.ByteCount
                    + "; hashMatchesLedger=" + file.HashMatches.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary = "streamingAssetsPayload=true; metadataOnly="
                    + summary.MetadataOnly.ToString().ToLowerInvariant()
            },
            summary);

    private static VisualWorldPreviewArtifactEntry BuildAlphaRuntimeBootstrapEntry(
        string projectRoot,
        UnityHandoffProofSummary summary) =>
        WithUnityHandoffSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal095SourceGoalId + ".alpha_runtime_bootstrap_unchanged",
                RelativePath = Goal095AlphaRuntimeBootstrapPath,
                ArtifactKind = "alpha_runtime_bootstrap_unchanged_status",
                SourceGoalId = Goal095SourceGoalId,
                Sha256 = File.Exists(Resolve(projectRoot, Goal095AlphaRuntimeBootstrapPath))
                    ? HashFor(
                        projectRoot,
                        Goal095AlphaRuntimeBootstrapPath,
                        new Dictionary<string, string>(StringComparer.Ordinal))
                    : string.Empty,
                Status = summary.AlphaRuntimeBootstrapUnchanged
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "unchanged="
                    + summary.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary = "statusOnly=true; unityFileUnchangedByGoal096="
                    + summary.NoUnityFilesChangedByGoal096.ToString().ToLowerInvariant()
            },
            summary);

    private static UnityHandoffProofSummary LoadUnityHandoffSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var manifest = TryReadJson(
            projectRoot,
            Goal095SourceRoot + "/visual-chunk-cache-unity-handoff-manifest.json",
            diagnostics);
        using var ledger = TryReadJson(
            projectRoot,
            Goal095SourceRoot + "/visual-chunk-cache-unity-streamingassets-ledger.json",
            diagnostics);
        using var probe = TryReadJson(
            projectRoot,
            Goal095SourceRoot + "/visual-chunk-cache-unity-probe-source-inventory.json",
            diagnostics);
        using var simulatedRead = TryReadJson(
            projectRoot,
            Goal095SourceRoot + "/visual-chunk-cache-unity-simulated-read-proof.json",
            diagnostics);
        using var negative = TryReadJson(
            projectRoot,
            Goal095SourceRoot + "/visual-chunk-cache-unity-negative-proof.json",
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            Goal095SourceRoot + "/visual-chunk-cache-unity-quality-gate-scan.json",
            diagnostics);

        var payloadRoot = Resolve(projectRoot, Goal095StreamingAssetsRoot);
        var payloadFiles = ReadUnityPayloadFiles(projectRoot, ledger?.RootElement, diagnostics);
        var payloadHashesMatch = payloadFiles.Count == 5
            && payloadFiles.All(file => file.Exists && file.HashMatches);
        var probeSourceMatches = ProbeSourceMatchesRecordedInventory(projectRoot, probe?.RootElement);
        var metadataOnly = manifest is not null
            && TryGetBool(manifest.RootElement, "runtimeHandoffSidecarMetadataOnly")
            && !TryGetBool(manifest.RootElement, "containsRuntimeExecution")
            && !TryGetBool(manifest.RootElement, "containsProviderCalls")
            && !TryGetBool(manifest.RootElement, "containsUnityGameplayImplementation");
        var alphaUnchanged = quality is not null
            && TryGetBool(quality.RootElement, "alphaRuntimeBootstrapUnchanged");
        var forbiddenUnityUnchanged = quality is not null
            && TryGetBool(quality.RootElement, "noForbiddenUnityAreasChanged");
        var noUnityFilesChanged = payloadHashesMatch && probeSourceMatches && alphaUnchanged;
        var relativePaths = payloadFiles.All(file => IsSafeRelativePath(file.RepositoryRelativePath))
            && IsSafeRelativePath(Goal095SourceRoot)
            && IsSafeRelativePath(Goal095ProbeSourcePath)
            && IsSafeRelativePath(Goal095AlphaRuntimeBootstrapPath);

        var summary = new UnityHandoffProofSummary(
            PayloadRootExists: Directory.Exists(payloadRoot),
            PayloadFileCount: manifest is null ? 0 : ReadInt(manifest.RootElement, "payloadFileCount"),
            PackageCount: manifest is null ? 0 : ReadInt(manifest.RootElement, "packageCount"),
            ExportRecordCount: manifest is null ? 0 : ReadInt(manifest.RootElement, "exportRecordCount"),
            StreamWindowCount: manifest is null ? 0 : ReadInt(manifest.RootElement, "streamWindowCount"),
            UniqueChunkKeyCount: manifest is null ? 0 : ReadInt(manifest.RootElement, "uniqueChunkKeyCount"),
            SimulatedReadProofPassed: simulatedRead is not null
                && TryGetBool(simulatedRead.RootElement, "passed"),
            NegativeProofPassed: negative is not null && TryGetBool(negative.RootElement, "passed"),
            ProbeSourceInventoryVisible: probe is not null,
            ProbeSourceInventoryPassed: probe is not null
                && TryGetBool(probe.RootElement, "passed")
                && probeSourceMatches,
            AlphaRuntimeBootstrapUnchanged: alphaUnchanged,
            ForbiddenUnityAreasUnchanged: forbiddenUnityUnchanged,
            MetadataOnly: metadataOnly,
            PayloadHashesMatchGoal095Ledger: payloadHashesMatch,
            Goal095FilesDiscoveredByRelativePaths: relativePaths,
            NoUnityFilesChangedByGoal096: noUnityFilesChanged,
            PayloadFiles: payloadFiles);

        AddUnitySummaryDiagnostics(summary, diagnostics);
        return summary;
    }

    private static IReadOnlyList<UnityHandoffPayloadFile> ReadUnityPayloadFiles(
        string projectRoot,
        JsonElement? ledgerRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (ledgerRoot is null || !TryGetArray(ledgerRoot.Value, "files", out var files))
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal096.unity_handoff.ledger_missing_files",
                Goal095SourceRoot + "/visual-chunk-cache-unity-streamingassets-ledger.json",
                "Goal095 StreamingAssets ledger must expose payload files."));
            return [];
        }

        return files
            .OrderBy(file => TryGetString(file, "role"), StringComparer.Ordinal)
            .Select(file => BuildPayloadFile(projectRoot, file))
            .ToList();
    }

    private static UnityHandoffPayloadFile BuildPayloadFile(string projectRoot, JsonElement file)
    {
        var relativeName = TryGetString(file, "relativePath");
        var repositoryRelativePath = NormalizePath(Goal095StreamingAssetsRoot + "/" + relativeName);
        var declaredHash = TryGetString(file, "sha256");
        var fullPath = Resolve(projectRoot, repositoryRelativePath);
        var exists = File.Exists(fullPath);
        var actualHash = exists
            ? HashFor(projectRoot, repositoryRelativePath, new Dictionary<string, string>(StringComparer.Ordinal))
            : string.Empty;
        return new UnityHandoffPayloadFile(
            RelativePath: relativeName,
            RepositoryRelativePath: repositoryRelativePath,
            Role: TryGetString(file, "role"),
            DeclaredSha256: declaredHash,
            ActualSha256: actualHash,
            ByteCount: ReadInt(file, "byteCount"),
            Exists: exists,
            HashMatches: exists && string.Equals(actualHash, declaredHash, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ProbeSourceMatchesRecordedInventory(
        string projectRoot,
        JsonElement? probeRoot)
    {
        if (probeRoot is null)
        {
            return false;
        }

        var fullPath = Resolve(projectRoot, Goal095ProbeSourcePath);
        var recordedHash = TryGetString(probeRoot.Value, "probeSha256");
        var actualHash = File.Exists(fullPath)
            ? HashFor(projectRoot, Goal095ProbeSourcePath, new Dictionary<string, string>(StringComparer.Ordinal))
            : string.Empty;
        return string.Equals(actualHash, recordedHash, StringComparison.OrdinalIgnoreCase);
    }

    private static void AddUnitySummaryDiagnostics(
        UnityHandoffProofSummary summary,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(
            summary.PayloadRootExists,
            "goal096.unity_handoff.payload_root_missing",
            Goal095StreamingAssetsRoot,
            diagnostics);
        AddIfFalse(summary.PayloadFileCount == 5, "goal096.unity_handoff.payload_count", "manifest", diagnostics);
        AddIfFalse(summary.PackageCount == 4, "goal096.unity_handoff.package_count", "manifest", diagnostics);
        AddIfFalse(summary.ExportRecordCount == 93, "goal096.unity_handoff.record_count", "manifest", diagnostics);
        AddIfFalse(summary.StreamWindowCount == 5, "goal096.unity_handoff.window_count", "manifest", diagnostics);
        AddIfFalse(summary.UniqueChunkKeyCount == 93, "goal096.unity_handoff.chunk_key_count", "manifest", diagnostics);
        AddIfFalse(
            summary.PayloadHashesMatchGoal095Ledger,
            "goal096.unity_handoff.payload_hashes",
            "StreamingAssets",
            diagnostics);
        AddIfFalse(
            summary.SimulatedReadProofPassed,
            "goal096.unity_handoff.simulated_read",
            "proofStatus",
            diagnostics);
        AddIfFalse(summary.NegativeProofPassed, "goal096.unity_handoff.negative", "proofStatus", diagnostics);
        AddIfFalse(
            summary.ProbeSourceInventoryPassed,
            "goal096.unity_handoff.probe_source",
            Goal095ProbeSourcePath,
            diagnostics);
        AddIfFalse(
            summary.AlphaRuntimeBootstrapUnchanged,
            "goal096.unity_handoff.alpha_bootstrap",
            Goal095AlphaRuntimeBootstrapPath,
            diagnostics);
        AddIfFalse(
            summary.ForbiddenUnityAreasUnchanged,
            "goal096.unity_handoff.forbidden_unity",
            "qualityGate",
            diagnostics);
        AddIfFalse(summary.MetadataOnly, "goal096.unity_handoff.metadata_only", "manifest", diagnostics);
        AddIfFalse(
            summary.Goal095FilesDiscoveredByRelativePaths,
            "goal096.unity_handoff.relative_paths",
            "unity_handoff",
            diagnostics);
        AddIfFalse(
            summary.NoUnityFilesChangedByGoal096,
            "goal096.unity_handoff.unity_files_changed",
            "unity_handoff",
            diagnostics);
    }

    private static int ReadInt(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private sealed record UnityHandoffPayloadFile(
        string RelativePath,
        string RepositoryRelativePath,
        string Role,
        string DeclaredSha256,
        string ActualSha256,
        int ByteCount,
        bool Exists,
        bool HashMatches);

    private sealed record UnityHandoffProofSummary(
        bool PayloadRootExists,
        int PayloadFileCount,
        int PackageCount,
        int ExportRecordCount,
        int StreamWindowCount,
        int UniqueChunkKeyCount,
        bool SimulatedReadProofPassed,
        bool NegativeProofPassed,
        bool ProbeSourceInventoryVisible,
        bool ProbeSourceInventoryPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool ForbiddenUnityAreasUnchanged,
        bool MetadataOnly,
        bool PayloadHashesMatchGoal095Ledger,
        bool Goal095FilesDiscoveredByRelativePaths,
        bool NoUnityFilesChangedByGoal096,
        IReadOnlyList<UnityHandoffPayloadFile> PayloadFiles);
}
