using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using LLMGameCreator.Application.Design.HybridDraftLuaExpansion;
using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;
using LLMGameCreator.Application.Design.LuaSandboxExecutionGate;
using LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;
using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;
using LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;
using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;

namespace LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;

public sealed class FullGeneratorWithoutMediaDryRunSourceLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public FullGeneratorSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var refs = new List<FullGeneratorSourceArtifactReference>();
        var artifactText = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var artifactHash = new SortedDictionary<string, string>(StringComparer.Ordinal);

        string Read(
            string relativeDirectory,
            string fileName,
            string sourceGoal,
            string artifactFamily,
            string summary)
        {
            var relativePath = NormalizeRelativePath(relativeDirectory, fileName);
            var path = Path.GetFullPath(Path.Combine(
                projectRoot,
                relativeDirectory.Replace('/', Path.DirectorySeparatorChar),
                fileName));
            EnsureContained(projectRoot, path);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Required Goal 047 source artifact was not found.", path);
            }

            var text = File.ReadAllText(path);
            var hash = ComputeHash(text);
            artifactText[relativePath] = text;
            artifactHash[relativePath] = hash;
            refs.Add(new FullGeneratorSourceArtifactReference
            {
                SourceGoal = sourceGoal,
                EvidenceRef = EvidenceRef(sourceGoal, artifactFamily, fileName),
                ArtifactFamily = artifactFamily,
                ArtifactFileName = fileName,
                ArtifactRelativePath = relativePath,
                ArtifactHash = hash,
                Summary = summary
            });

            return text;
        }

        ReadGoal034(Read);
        ReadGoal035(Read);
        ReadGoal036(Read);
        ReadGoal037(Read);
        ReadGoal038(Read);
        ReadGoal039(Read);
        var goal040 = ReadGoal040(Read);
        var goal043 = ReadGoal043(Read);

        return new FullGeneratorSourceBundle
        {
            Goal043Catalog = goal043.Catalog,
            Goal043SharedLifecycleContract = goal043.SharedLifecycleContract,
            Goal043PlansByFamilyId = goal043.PlansByFamilyId,
            Goal043ProofsByFamilyId = goal043.ProofsByFamilyId,
            Goal043PreviewExportMatrix = goal043.PreviewExportMatrix,
            Goal043RegressionMatrix = goal043.RegressionMatrix,
            Goal040ExportManifest = goal040.ExportManifest,
            Goal040RuntimePreviewConsumptionProof = goal040.RuntimePreviewConsumptionProof,
            Goal040PayloadsByScenario = goal040.PayloadsByScenario,
            SourceArtifactRefs = refs
                .OrderBy(item => SourceGoalOrder(item.SourceGoal))
                .ThenBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
                .ThenBy(item => item.EvidenceRef, StringComparer.Ordinal)
                .ToList(),
            ArtifactHashByRelativePath = artifactHash,
            ArtifactTextByRelativePath = artifactText
        };
    }

    private static void ReadGoal034(Func<string, string, string, string, string, string> read)
    {
        read(StrictLlmDraftArtifactLoopEvidenceService.RelativeOutputDirectory, StrictLlmDraftArtifactLoopEvidenceService.ContractSummaryJsonFileName, "Goal034", "strict_draft_contract_summary", "strict draft contracts and source limits");
        read(StrictLlmDraftArtifactLoopEvidenceService.RelativeOutputDirectory, StrictLlmDraftArtifactLoopEvidenceService.PromotionMatrixJsonFileName, "Goal034", "strict_draft_promotion_decisions", "quarantined draft promotion decision matrix");
        read(StrictLlmDraftArtifactLoopEvidenceService.RelativeOutputDirectory, StrictLlmDraftArtifactLoopEvidenceService.RepairMatrixJsonFileName, "Goal034", "strict_draft_repair_requests", "strict draft repair request summary");
        read(StrictLlmDraftArtifactLoopEvidenceService.RelativeOutputDirectory, StrictLlmDraftArtifactLoopEvidenceService.FrontierPlanJsonFileName, "Goal034", "strict_draft_plan", "frontier strict draft plan refs");
        read(StrictLlmDraftArtifactLoopEvidenceService.RelativeOutputDirectory, StrictLlmDraftArtifactLoopEvidenceService.GothicPlanJsonFileName, "Goal034", "strict_draft_plan", "gothic strict draft plan refs");
        read(StrictLlmDraftArtifactLoopEvidenceService.RelativeOutputDirectory, StrictLlmDraftArtifactLoopEvidenceService.MetamodulePlanJsonFileName, "Goal034", "strict_draft_plan", "metamodule strict draft plan refs");
    }

    private static void ReadGoal035(Func<string, string, string, string, string, string> read)
    {
        read(LuaModuleManifestEvidenceService.RelativeOutputDirectory, LuaModuleManifestEvidenceService.HostApiSurfacePolicyJsonFileName, "Goal035", "lua_host_api_surface_policy", "Lua host API denied surface policy");
        read(LuaModuleManifestEvidenceService.RelativeOutputDirectory, LuaModuleManifestEvidenceService.DependencyPlanJsonFileName, "Goal035", "lua_module_dependency_plan", "deterministic Lua manifest dependency order");
        read(LuaModuleManifestEvidenceService.RelativeOutputDirectory, LuaModuleManifestEvidenceService.FrontierSelectionJsonFileName, "Goal035", "lua_module_selection", "frontier Lua module selection refs");
        read(LuaModuleManifestEvidenceService.RelativeOutputDirectory, LuaModuleManifestEvidenceService.GothicSelectionJsonFileName, "Goal035", "lua_module_selection", "gothic Lua module selection refs");
        read(LuaModuleManifestEvidenceService.RelativeOutputDirectory, LuaModuleManifestEvidenceService.MetamoduleSelectionJsonFileName, "Goal035", "lua_module_selection", "metamodule Lua module selection refs");
    }

    private static void ReadGoal036(Func<string, string, string, string, string, string> read)
    {
        read(LuaSandboxExecutionGateEvidenceService.RelativeOutputDirectory, LuaSandboxExecutionGateEvidenceService.PolicySummaryJsonFileName, "Goal036", "lua_sandbox_policy_summary", "Lua sandbox policy summary");
        read(LuaSandboxExecutionGateEvidenceService.RelativeOutputDirectory, LuaSandboxExecutionGateEvidenceService.DryRunTraceMatrixJsonFileName, "Goal036", "lua_sandbox_dry_run_trace", "sandbox dry-run trace matrix");
        read(LuaSandboxExecutionGateEvidenceService.RelativeOutputDirectory, LuaSandboxExecutionGateEvidenceService.RepairPlanMatrixJsonFileName, "Goal036", "lua_sandbox_repair_plans", "sandbox repair plan matrix");
        read(LuaSandboxExecutionGateEvidenceService.RelativeOutputDirectory, LuaSandboxExecutionGateEvidenceService.FrontierDecisionJsonFileName, "Goal036", "lua_sandbox_decision", "frontier sandbox decision");
        read(LuaSandboxExecutionGateEvidenceService.RelativeOutputDirectory, LuaSandboxExecutionGateEvidenceService.GothicDecisionJsonFileName, "Goal036", "lua_sandbox_decision", "gothic sandbox decision");
        read(LuaSandboxExecutionGateEvidenceService.RelativeOutputDirectory, LuaSandboxExecutionGateEvidenceService.MetamoduleDecisionJsonFileName, "Goal036", "lua_sandbox_decision", "metamodule sandbox decision");
    }

    private static void ReadGoal037(Func<string, string, string, string, string, string> read)
    {
        read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.PipelineSummaryJsonFileName, "Goal037", "hybrid_pipeline_summary", "strict draft plus bounded Lua expansion pipeline summary");
        read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.DraftToLuaRequestMapJsonFileName, "Goal037", "draft_to_lua_request_map", "draft to Lua request map");
        read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.SandboxApprovedMatrixJsonFileName, "Goal037", "sandbox_approved_expansion_matrix", "sandbox-approved expansion matrix");
        read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.PromotionDecisionMatrixJsonFileName, "Goal037", "hybrid_promotion_decisions", "hybrid output promotion decision matrix");
        read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.FrontierOutputJsonFileName, "Goal037", "lua_expansion_output", "frontier expansion output summary");
        read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.GothicOutputJsonFileName, "Goal037", "lua_expansion_output", "gothic expansion output summary");
        read(HybridDraftLuaExpansionEvidenceService.RelativeOutputDirectory, HybridDraftLuaExpansionEvidenceService.MetamoduleOutputJsonFileName, "Goal037", "lua_expansion_output", "metamodule expansion output summary");
    }

    private static void ReadGoal038(Func<string, string, string, string, string, string> read)
    {
        read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, WorldScaleRegionMapEvidenceService.RegionGraphSummaryJsonFileName, "Goal038", "region_graph_summary", "region graph ids and counts");
        read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, WorldScaleRegionMapEvidenceService.ChunkedWorldConfigPreludeJsonFileName, "Goal038", "chunked_world_config_prelude", "chunk config and bounded window refs");
        read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, WorldScaleRegionMapEvidenceService.ReachabilityMatrixJsonFileName, "Goal038", "reachability_matrix", "reachability proof refs");
        read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, "finite-map-pack-frontier.json", "Goal038", "finite_map_pack", "frontier finite map pack summary");
        read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, "finite-map-pack-gothic.json", "Goal038", "finite_map_pack", "gothic finite map pack summary");
        read(WorldScaleRegionMapEvidenceService.RelativeOutputDirectory, "finite-map-pack-metamodule-kingdoms.json", "Goal038", "finite_map_pack", "metamodule finite map pack summary");
    }

    private static void ReadGoal039(Func<string, string, string, string, string, string> read)
    {
        read(RuntimeChunkDeltaEvidenceService.RelativeOutputDirectory, RuntimeChunkDeltaEvidenceService.FrontierPlanJsonFileName, "Goal039", "runtime_chunk_traversal_plan", "frontier runtime traversal plan");
        read(RuntimeChunkDeltaEvidenceService.RelativeOutputDirectory, RuntimeChunkDeltaEvidenceService.GothicPlanJsonFileName, "Goal039", "runtime_chunk_traversal_plan", "gothic runtime traversal plan");
        read(RuntimeChunkDeltaEvidenceService.RelativeOutputDirectory, RuntimeChunkDeltaEvidenceService.MetamodulePlanJsonFileName, "Goal039", "runtime_chunk_traversal_plan", "metamodule runtime traversal plan");
        read(RuntimeChunkDeltaEvidenceService.RelativeOutputDirectory, RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName, "Goal039", "runtime_save_load_roundtrip_proof", "runtime save-load proof");
        read(RuntimeChunkDeltaEvidenceService.RelativeOutputDirectory, RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName, "Goal039", "runtime_replay_determinism_proof", "runtime replay proof");
    }

    private static Goal040ReadResult ReadGoal040(Func<string, string, string, string, string, string> read)
    {
        var exportManifestText = read(ChunkedRuntimePreviewExportEvidenceService.RelativeOutputDirectory, ChunkedRuntimePreviewExportEvidenceService.ExportManifestJsonFileName, "Goal040", "chunked_export_manifest", "runtime preview/export manifest");
        var consumptionText = read(ChunkedRuntimePreviewExportEvidenceService.RelativeOutputDirectory, ChunkedRuntimePreviewExportEvidenceService.RuntimePreviewConsumptionProofJsonFileName, "Goal040", "runtime_preview_consumption_proof", "runtime preview consumption proof");
        read(ChunkedRuntimePreviewExportEvidenceService.RelativeOutputDirectory, ChunkedRuntimePreviewExportEvidenceService.CatalogSummaryJsonFileName, "Goal040", "chunked_consumer_catalog_summary", "Goal040 source catalog summary");
        read(ChunkedRuntimePreviewExportEvidenceService.RelativeOutputDirectory, ChunkedRuntimePreviewExportEvidenceService.MultiFamilyMatrixJsonFileName, "Goal040", "multi_family_world_scale_regression_matrix", "multi-family world-scale regression proof");

        var payloads = new SortedDictionary<string, ChunkedPreviewPayload>(StringComparer.Ordinal);
        foreach (var pair in FullGeneratorWithoutMediaDryRunVocabulary.ScenarioByFamilyId
                     .OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var fileName = ChunkedRuntimePreviewExportVocabulary.PayloadFileNamesByScenario[pair.Value];
            var text = read(ChunkedRuntimePreviewExportEvidenceService.RelativeOutputDirectory, fileName, "Goal040", "chunked_preview_payload", pair.Key + " preview/export payload summary");
            payloads[pair.Value] = ReadJson<ChunkedPreviewPayload>(text);
        }

        return new Goal040ReadResult(
            ReadJson<ChunkedExportManifest>(exportManifestText),
            ReadJson<RuntimePreviewConsumptionProof>(consumptionText),
            payloads);
    }

    private static Goal043ReadResult ReadGoal043(Func<string, string, string, string, string, string> read)
    {
        var catalogText = read(MultiFamilyGeneratedTemplateEvidenceService.RelativeOutputDirectory, MultiFamilyGeneratedTemplateEvidenceService.CatalogJsonFileName, "Goal043", "family_template_catalog", "Goal043 family catalog");
        var sharedText = read(MultiFamilyGeneratedTemplateEvidenceService.RelativeOutputDirectory, MultiFamilyGeneratedTemplateEvidenceService.SharedLifecycleContractJsonFileName, "Goal043", "shared_lifecycle_contract", "Goal043 shared lifecycle contract");
        var previewText = read(MultiFamilyGeneratedTemplateEvidenceService.RelativeOutputDirectory, MultiFamilyGeneratedTemplateEvidenceService.PreviewExportConsumptionMatrixJsonFileName, "Goal043", "preview_export_consumption_matrix", "Goal043 preview/export consumption matrix");
        var regressionText = read(MultiFamilyGeneratedTemplateEvidenceService.RelativeOutputDirectory, MultiFamilyGeneratedTemplateEvidenceService.RegressionMatrixJsonFileName, "Goal043", "multi_family_regression_matrix", "Goal043 multi-family regression matrix");
        read(MultiFamilyGeneratedTemplateEvidenceService.RelativeOutputDirectory, MultiFamilyGeneratedTemplateEvidenceService.InvalidMatrixJsonFileName, "Goal043", "invalid_family_diagnostics_matrix", "Goal043 invalid/fake/leak matrix");
        read(MultiFamilyGeneratedTemplateEvidenceService.RelativeOutputDirectory, MultiFamilyGeneratedTemplateEvidenceService.ReportMarkdownFileName, "Goal043", "multi_family_generated_template_report", "Goal043 produced-for-review report");

        var plans = new SortedDictionary<string, FamilyLifecyclePlan>(StringComparer.Ordinal);
        var proofs = new SortedDictionary<string, FamilySimulatableLoopProof>(StringComparer.Ordinal);
        foreach (var familyId in FullGeneratorWithoutMediaDryRunVocabulary.FamilyIds.OrderBy(item => item, StringComparer.Ordinal))
        {
            var planText = read(MultiFamilyGeneratedTemplateEvidenceService.RelativeOutputDirectory, MultiFamilyGeneratedTemplateEvidenceService.PlanFileName(familyId), "Goal043", "family_lifecycle_plan", familyId + " lifecycle plan");
            var proofText = read(MultiFamilyGeneratedTemplateEvidenceService.RelativeOutputDirectory, MultiFamilyGeneratedTemplateEvidenceService.LoopProofFileName(familyId), "Goal043", "family_simulatable_loop_proof", familyId + " simulatable loop proof");
            plans[familyId] = ReadJson<FamilyLifecyclePlan>(planText);
            proofs[familyId] = ReadJson<FamilySimulatableLoopProof>(proofText);
        }

        return new Goal043ReadResult(
            ReadJson<FamilyTemplateCatalog>(catalogText),
            ReadJson<SharedLifecycleContract>(sharedText),
            plans,
            proofs,
            ReadJson<PreviewExportConsumptionMatrix>(previewText),
            ReadJson<MultiFamilyRegressionMatrix>(regressionText));
    }

    private static T ReadJson<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, JsonOptions)
        ?? throw new InvalidOperationException("Artifact JSON could not be deserialized as " + typeof(T).Name + ".");

    private static string EvidenceRef(string sourceGoal, string artifactFamily, string fileName) =>
        $"{sourceGoal.ToLowerInvariant()}/{artifactFamily}/{Path.GetFileNameWithoutExtension(fileName)}";

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
            "Goal043" => 43,
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

    private sealed record Goal040ReadResult(
        ChunkedExportManifest ExportManifest,
        RuntimePreviewConsumptionProof RuntimePreviewConsumptionProof,
        IReadOnlyDictionary<string, ChunkedPreviewPayload> PayloadsByScenario);

    private sealed record Goal043ReadResult(
        FamilyTemplateCatalog Catalog,
        SharedLifecycleContract SharedLifecycleContract,
        IReadOnlyDictionary<string, FamilyLifecyclePlan> PlansByFamilyId,
        IReadOnlyDictionary<string, FamilySimulatableLoopProof> ProofsByFamilyId,
        PreviewExportConsumptionMatrix PreviewExportMatrix,
        MultiFamilyRegressionMatrix RegressionMatrix);
}
