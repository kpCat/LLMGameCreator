using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using LLMGameCreator.Application.Design.HybridDraftLuaExpansion;
using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;
using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;

namespace LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class MultiFamilyGeneratedTemplateSourceLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public MultiFamilySourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var refs = new List<MultiFamilySourceArtifactReference>();
        var artifactText = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var artifactHash = new SortedDictionary<string, string>(StringComparer.Ordinal);

        string Read(
            string relativeDirectory,
            string fileName,
            string sourceGoal,
            string artifactFamily,
            string? evidenceRef = null)
        {
            var relativePath = NormalizeRelativePath(relativeDirectory, fileName);
            var path = Path.GetFullPath(Path.Combine(
                projectRoot,
                relativeDirectory.Replace('/', Path.DirectorySeparatorChar),
                fileName));
            EnsureContained(projectRoot, path);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Required source artifact was not found.", path);
            }

            var text = File.ReadAllText(path);
            var hash = ComputeHash(text);
            artifactText[relativePath] = text;
            artifactHash[relativePath] = hash;
            refs.Add(new MultiFamilySourceArtifactReference
            {
                SourceGoal = sourceGoal,
                EvidenceRef = evidenceRef ?? fileName,
                ArtifactFamily = artifactFamily,
                ArtifactFileName = fileName,
                ArtifactRelativePath = relativePath,
                ArtifactHash = hash
            });
            return text;
        }

        var goal037Pipeline = Read(
            HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory,
            HybridDraftLuaExpansionEvidenceService.PipelineSummaryJsonFileName,
            "Goal037",
            "hybrid_pipeline_summary");
        var goal037PipelineRelativePath = NormalizeRelativePath(
            HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory,
            HybridDraftLuaExpansionEvidenceService.PipelineSummaryJsonFileName);
        refs.AddRange(BuildEmbeddedGoal037UpstreamRefs(goal037PipelineRelativePath, artifactHash[goal037PipelineRelativePath]));
        _ = goal037Pipeline;
        Read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.DraftToLuaRequestMapJsonFileName, "Goal037", "draft_to_lua_request_map");
        Read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.SandboxApprovedMatrixJsonFileName, "Goal037", "sandbox_approved_expansion_matrix");
        Read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.FrontierOutputJsonFileName, "Goal037", "lua_expansion_output");
        Read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.GothicOutputJsonFileName, "Goal037", "lua_expansion_output");
        Read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.MetamoduleOutputJsonFileName, "Goal037", "lua_expansion_output");

        Read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, WorldScaleRegionMapEvidenceService.RegionGraphSummaryJsonFileName, "Goal038", "region_graph_summary");
        Read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, WorldScaleRegionMapEvidenceService.ChunkedWorldConfigPreludeJsonFileName, "Goal038", "chunked_world_config_prelude");
        Read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, WorldScaleRegionMapEvidenceService.ReachabilityMatrixJsonFileName, "Goal038", "reachability_matrix");
        Read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, "finite-map-pack-frontier.json", "Goal038", "finite_map_pack");
        Read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, "finite-map-pack-gothic.json", "Goal038", "finite_map_pack");
        Read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, "finite-map-pack-metamodule-kingdoms.json", "Goal038", "finite_map_pack");

        var planFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["frontier_survival"] = RuntimeChunkDeltaEvidenceService.FrontierPlanJsonFileName,
            ["gothic_intrigue"] = RuntimeChunkDeltaEvidenceService.GothicPlanJsonFileName,
            ["metamodule_kingdoms"] = RuntimeChunkDeltaEvidenceService.MetamodulePlanJsonFileName
        };
        var plans = new SortedDictionary<string, RuntimeChunkTraversalPlan>(StringComparer.Ordinal);
        foreach (var pair in planFiles)
        {
            var text = Read(RuntimeChunkDeltaEvidenceService.RelativeOutputDirectory, pair.Value, "Goal039", "runtime_chunk_traversal_plan");
            plans[pair.Key] = ReadJson<RuntimeChunkTraversalPlan>(text);
        }

        Read(RuntimeChunkDeltaEvidenceService.RelativeOutputDirectory, RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName, "Goal039", "runtime_save_load_roundtrip_proof");
        Read(RuntimeChunkDeltaEvidenceService.RelativeOutputDirectory, RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName, "Goal039", "chunk_replay_determinism_proof");

        var catalogText = Read(ChunkedRuntimePreviewExportEvidenceService.RelativeOutputDirectory, ChunkedRuntimePreviewExportEvidenceService.CatalogSummaryJsonFileName, "Goal040", "chunked_consumer_catalog_summary");
        var manifestText = Read(ChunkedRuntimePreviewExportEvidenceService.RelativeOutputDirectory, ChunkedRuntimePreviewExportEvidenceService.ExportManifestJsonFileName, "Goal040", "chunked_export_manifest");
        var matrixText = Read(ChunkedRuntimePreviewExportEvidenceService.RelativeOutputDirectory, ChunkedRuntimePreviewExportEvidenceService.MultiFamilyMatrixJsonFileName, "Goal040", "multi_family_world_scale_regression_matrix");
        var consumptionText = Read(ChunkedRuntimePreviewExportEvidenceService.RelativeOutputDirectory, ChunkedRuntimePreviewExportEvidenceService.RuntimePreviewConsumptionProofJsonFileName, "Goal040", "runtime_preview_consumption_proof");

        var payloads = new SortedDictionary<string, ChunkedPreviewPayload>(StringComparer.Ordinal);
        foreach (var pair in ChunkedRuntimePreviewExportVocabulary.PayloadFileNamesByScenario
                     .Where(pair => MultiFamilyGeneratedTemplateVocabulary.ScenarioByFamilyId.Values.Contains(pair.Key, StringComparer.Ordinal))
                     .OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            var text = Read(ChunkedRuntimePreviewExportEvidenceService.RelativeOutputDirectory, pair.Value, "Goal040", "chunked_preview_export_payload");
            payloads[pair.Key] = ReadJson<ChunkedPreviewPayload>(text);
        }

        return new MultiFamilySourceBundle
        {
            Goal040PayloadsByScenario = payloads,
            Goal040Catalog = ReadJson<ChunkedConsumerCatalogSummary>(catalogText),
            Goal040ExportManifest = ReadJson<ChunkedExportManifest>(manifestText),
            Goal040FamilyRegressionMatrix = ReadJson<MultiFamilyWorldScaleRegressionMatrix>(matrixText),
            Goal040ConsumptionProof = ReadJson<RuntimePreviewConsumptionProof>(consumptionText),
            Goal039PlansByScenario = plans,
            SourceArtifactRefs = refs
                .OrderBy(item => SourceGoalOrder(item.SourceGoal))
                .ThenBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
                .ThenBy(item => item.EvidenceRef, StringComparer.Ordinal)
                .ToList(),
            ArtifactTextByRelativePath = artifactText,
            ArtifactHashByRelativePath = artifactHash
        };
    }

    private static IReadOnlyList<MultiFamilySourceArtifactReference> BuildEmbeddedGoal037UpstreamRefs(
        string artifactRelativePath,
        string artifactHash) =>
    [
        Embedded("Goal034", "goal034_draft_request_candidate", "strict_llm_draft_request_candidate", artifactRelativePath, artifactHash),
        Embedded("Goal035", "goal035_lua_manifest_selection", "lua_module_manifest_selection", artifactRelativePath, artifactHash),
        Embedded("Goal036", "goal036_sandbox_gate_decision", "lua_sandbox_execution_gate_decision", artifactRelativePath, artifactHash)
    ];

    private static MultiFamilySourceArtifactReference Embedded(
        string sourceGoal,
        string evidenceRef,
        string artifactFamily,
        string artifactRelativePath,
        string artifactHash) =>
        new()
        {
            SourceGoal = sourceGoal,
            EvidenceRef = evidenceRef,
            ArtifactFamily = artifactFamily,
            ArtifactFileName = HybridDraftLuaExpansionEvidenceService.PipelineSummaryJsonFileName,
            ArtifactRelativePath = artifactRelativePath,
            ArtifactHash = artifactHash
        };

    private static T ReadJson<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, JsonOptions)
        ?? throw new InvalidOperationException("Artifact JSON could not be deserialized as " + typeof(T).Name + ".");

    private static string NormalizeRelativePath(string relativeDirectory, string fileName) =>
        (relativeDirectory.TrimEnd('/', '\\') + "/" + fileName).Replace('\\', '/');

    private static int SourceGoalOrder(string sourceGoal) =>
        sourceGoal switch
        {
            "Goal034" => 34,
            "Goal035" => 35,
            "Goal036" => 36,
            "Goal037" => 37,
            "Goal038" => 38,
            "Goal039" => 39,
            "Goal040" => 40,
            _ => 999
        };

    private static string ComputeHash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}
