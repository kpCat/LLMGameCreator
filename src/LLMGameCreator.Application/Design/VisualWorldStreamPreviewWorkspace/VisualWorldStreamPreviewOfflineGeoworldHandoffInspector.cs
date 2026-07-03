using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldVisualCacheUnityHandoff;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal100SourceGoalId =
        "goal_100_offline_geoworld_visual_cache_unity_handoff";
    private const string Goal100SourceRoot =
        ".llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff";
    private const string Goal100StreamingAssetsRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal100";
    private const string Goal100ProbeSourcePath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldHandoffProbe.cs";

    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldHandoffGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldHandoffSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                Goal100SourceRoot,
                Goal100SourceGoalId,
                [
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.ReportMarkdownFileName,
                        "offline_geoworld_handoff_report"),
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.VisualCacheCatalogFileName,
                        "offline_geoworld_visual_cache_catalog"),
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.VisualCachePackageIndexFileName,
                        "offline_geoworld_visual_cache_package_index"),
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.FeatureChunkLedgerFileName,
                        "offline_geoworld_feature_chunk_ledger"),
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.HandoffManifestFileName,
                        "offline_geoworld_unity_handoff_manifest"),
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityStreamingAssetsLedgerFileName,
                        "offline_geoworld_streamingassets_ledger"),
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeSourceInventoryFileName,
                        "offline_geoworld_probe_source_inventory"),
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnitySimulatedReadProofFileName,
                        "offline_geoworld_simulated_unity_read_proof"),
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.NegativeProofFileName,
                        "offline_geoworld_negative_proof"),
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.WorkspaceBindingInventoryFileName,
                        "offline_geoworld_workspace_binding_inventory"),
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.SourceLineageFileName,
                        "offline_geoworld_source_lineage"),
                    (OfflineGeoworldVisualCacheUnityHandoffVocabulary.QualityGateScanFileName,
                        "offline_geoworld_quality_gate")
                ],
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldHandoffSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredPayloadFileNames)
        {
            var relativePath = Goal100StreamingAssetsRoot + "/" + fileName;
            var exists = File.Exists(Resolve(projectRoot, relativePath));
            entries.Add(WithOfflineGeoworldHandoffSummary(
                new VisualWorldPreviewArtifactEntry
                {
                    Id = Goal100SourceGoalId + ".payload." + Path.GetFileNameWithoutExtension(fileName),
                    RelativePath = relativePath,
                    ArtifactKind = "offline_geoworld_streamingassets_payload",
                    SourceGoalId = Goal100SourceGoalId,
                    Sha256 = exists
                        ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                        : string.Empty,
                    Status = exists
                        ? VisualWorldPreviewArtifactStatus.Passed
                        : VisualWorldPreviewArtifactStatus.Failed,
                    DiagnosticSummary = exists ? "mirrored payload exists" : "mirrored payload missing",
                    SafeRatingMetadataSummary = "metadataOnly=true; relativePath=true"
                },
                summary));
        }

        entries.Add(WithOfflineGeoworldHandoffSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal100SourceGoalId + ".summary",
                RelativePath = Goal100SourceRoot + "/"
                    + OfflineGeoworldVisualCacheUnityHandoffVocabulary.QualityGateScanFileName,
                ArtifactKind = "offline_geoworld_handoff_workspace_summary",
                SourceGoalId = Goal100SourceGoalId,
                Sha256 = HashFor(
                    projectRoot,
                    Goal100SourceRoot + "/"
                    + OfflineGeoworldVisualCacheUnityHandoffVocabulary.QualityGateScanFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "packages=" + summary.PackageCount
                    + "; features=" + summary.FeatureCount
                    + "; records=" + summary.VisualCacheRecordCount
                    + "; chunks=" + summary.SourceChunkCount
                    + "; windows=" + summary.StreamWindowChunkCount,
                SafeRatingMetadataSummary = summary.FeatureKindCountsSummary
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_handoff",
            "Goal 100 Offline Geoworld Handoff",
            Goal100SourceGoalId,
            Goal100SourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldHandoffSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldHandoffWorkspaceSummary summary) =>
        entry with
        {
            PackageCount = summary.PackageCount,
            PayloadFileCount = summary.PayloadFileCount,
            GeoworldNormalizedFeatureCount = summary.FeatureCount,
            GeoworldWorldSourceGraphChunkCount = summary.SourceChunkCount,
            GeoworldStreamWindowChunkCount = summary.StreamWindowChunkCount,
            GeoworldVisualCacheRecordCount = summary.VisualCacheRecordCount,
            OfflineGeoworldHandoffFeatureKindCountsSummary = summary.FeatureKindCountsSummary,
            SimulatedUnityReadProofPassed = summary.SimulatedReadProofPassed,
            NegativeProofPassed = summary.NegativeProofPassed,
            AlphaRuntimeBootstrapUnchanged = summary.AlphaRuntimeBootstrapUnchanged,
            MetadataOnly = summary.MetadataOnly,
            NoRawFullWorldDump = summary.NoRawGeodataDump,
            OfflineGeoworldHandoffQualityGatePassed = summary.QualityGatePassed
        };

    private static OfflineGeoworldHandoffWorkspaceSummary LoadOfflineGeoworldHandoffSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var manifest = TryReadJson(
            projectRoot,
            Goal100SourceRoot + "/" + OfflineGeoworldVisualCacheUnityHandoffVocabulary.HandoffManifestFileName,
            diagnostics);
        using var catalog = TryReadJson(
            projectRoot,
            Goal100SourceRoot + "/" + OfflineGeoworldVisualCacheUnityHandoffVocabulary.VisualCacheCatalogFileName,
            diagnostics);
        using var streamingLedger = TryReadJson(
            projectRoot,
            Goal100SourceRoot + "/"
            + OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityStreamingAssetsLedgerFileName,
            diagnostics);
        using var readProof = TryReadJson(
            projectRoot,
            Goal100SourceRoot + "/"
            + OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnitySimulatedReadProofFileName,
            diagnostics);
        using var negative = TryReadJson(
            projectRoot,
            Goal100SourceRoot + "/" + OfflineGeoworldVisualCacheUnityHandoffVocabulary.NegativeProofFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            Goal100SourceRoot + "/" + OfflineGeoworldVisualCacheUnityHandoffVocabulary.QualityGateScanFileName,
            diagnostics);

        var packageCount = manifest is null ? 0 : ReadGoal100Int(manifest.RootElement, "packageCount");
        var featureCount = manifest is null ? 0 : ReadGoal100Int(manifest.RootElement, "featureCount");
        var recordCount = manifest is null ? 0 : ReadGoal100Int(manifest.RootElement, "visualCacheRecordCount");
        var sourceChunkCount = manifest is null ? 0 : ReadGoal100Int(manifest.RootElement, "sourceChunkCount");
        var streamChunkCount = manifest is null ? 0 : ReadGoal100Int(manifest.RootElement, "streamWindowChunkCount");
        var payloadFileCount = streamingLedger is null
            ? 0
            : ReadGoal100Int(streamingLedger.RootElement, "payloadFileCount");
        var kindSummary = catalog is null
            ? string.Empty
            : ReadFeatureKindCountsSummary(catalog.RootElement);
        var metadataOnly = manifest is not null && TryGetBool(manifest.RootElement, "metadataOnly");
        var noRaw = manifest is not null && TryGetBool(manifest.RootElement, "noRawGeodata");
        var readPassed = readProof is not null && TryGetBool(readProof.RootElement, "passed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var alphaUnchanged = quality is not null
            && TryGetBool(quality.RootElement, "alphaRuntimeBootstrapUnchanged");
        var relativePaths = IsSafeRelativePath(Goal100SourceRoot)
                            && IsSafeRelativePath(Goal100StreamingAssetsRoot)
                            && IsSafeRelativePath(Goal100ProbeSourcePath);
        var passed = packageCount == 3
                     && featureCount == 10
                     && recordCount == 18
                     && sourceChunkCount == 5
                     && streamChunkCount == 9
                     && payloadFileCount == 5
                     && !string.IsNullOrWhiteSpace(kindSummary)
                     && metadataOnly
                     && noRaw
                     && readPassed
                     && negativePassed
                     && alphaUnchanged
                     && qualityPassed
                     && relativePaths;
        AddIfFalse(
            passed,
            "goal100.workspace.summary_failed",
            "offline_geoworld_handoff",
            diagnostics);
        return new OfflineGeoworldHandoffWorkspaceSummary(
            Passed: passed,
            PackageCount: packageCount,
            FeatureCount: featureCount,
            VisualCacheRecordCount: recordCount,
            SourceChunkCount: sourceChunkCount,
            StreamWindowChunkCount: streamChunkCount,
            PayloadFileCount: payloadFileCount,
            FeatureKindCountsSummary: kindSummary,
            MetadataOnly: metadataOnly,
            NoRawGeodataDump: noRaw,
            SimulatedReadProofPassed: readPassed,
            NegativeProofPassed: negativePassed,
            AlphaRuntimeBootstrapUnchanged: alphaUnchanged,
            QualityGatePassed: qualityPassed);
    }

    private static string ReadFeatureKindCountsSummary(JsonElement element)
    {
        if (!element.TryGetProperty("featureCountByKind", out var counts)
            || counts.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        return string.Join(
            "; ",
            counts.EnumerateObject()
                .OrderBy(item => item.Name, StringComparer.Ordinal)
                .Select(item => item.Name + "=" + item.Value.GetInt32()));
    }

    private static int ReadGoal100Int(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private sealed record OfflineGeoworldHandoffWorkspaceSummary(
        bool Passed,
        int PackageCount,
        int FeatureCount,
        int VisualCacheRecordCount,
        int SourceChunkCount,
        int StreamWindowChunkCount,
        int PayloadFileCount,
        string FeatureKindCountsSummary,
        bool MetadataOnly,
        bool NoRawGeodataDump,
        bool SimulatedReadProofPassed,
        bool NegativeProofPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed);
}
