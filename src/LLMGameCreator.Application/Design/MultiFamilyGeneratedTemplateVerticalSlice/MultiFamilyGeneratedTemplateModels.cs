using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;

namespace LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;

public static class MultiFamilyGeneratedTemplateVocabulary
{
    public const string GoalId = "goal_043_multi_family_generated_template_vertical_slice";
    public const string ProductSmokeRoute = "goal-043-multi-family-generated-template-vertical-slice";
    public const string FinalGate = "multi_family_generated_template_vertical_slice_verification";
    public const string SharedLifecycleContractId = "multi_family_generated_template_lifecycle_contract_v1";
    public const string CorePayloadSchemaId = ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId;

    public static readonly IReadOnlyList<string> FamilyIds =
    [
        "map_panel_rpg",
        "survival_sandbox",
        "first_person_grid_dungeon"
    ];

    public static readonly IReadOnlyDictionary<string, string> ScenarioByFamilyId =
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["first_person_grid_dungeon"] = "metamodule_kingdoms",
            ["map_panel_rpg"] = "gothic_intrigue",
            ["survival_sandbox"] = "frontier_survival"
        };

    public static readonly IReadOnlyList<string> SharedLifecyclePhases =
    [
        "family_profile",
        "semantic_intent_selection",
        "draft_lua_expansion_refs",
        "world_map_chunk_binding",
        "preview_export_consumer_binding",
        "family_loop_plan",
        "validation_trace",
        "simulatable_loop_proof",
        "manual_review_gate"
    ];
}

public sealed record FamilyTemplateBoundaryClaims
{
    public bool GamePackageSchemaMutation { get; init; }
    public bool RuntimeSourceMutation { get; init; }
    public bool RuntimeAbstractionsMutation { get; init; }
    public bool WinFormsUiMutation { get; init; }
    public bool UnitySourceMutation { get; init; }
    public bool ProviderLlmRagMedia { get; init; }
    public bool LuaSourceOrExecutor { get; init; }
    public bool GeneratorLibraryMutation { get; init; }
    public bool ExternalDependency { get; init; }
    public bool FilesystemNetworkProcessReflectionThreadTimeRandomNativeInterop { get; init; }

    [JsonIgnore]
    public bool AllFalse =>
        !GamePackageSchemaMutation &&
        !RuntimeSourceMutation &&
        !RuntimeAbstractionsMutation &&
        !WinFormsUiMutation &&
        !UnitySourceMutation &&
        !ProviderLlmRagMedia &&
        !LuaSourceOrExecutor &&
        !GeneratorLibraryMutation &&
        !ExternalDependency &&
        !FilesystemNetworkProcessReflectionThreadTimeRandomNativeInterop;
}

public sealed record MultiFamilySourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactFileName { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
}

public sealed record MultiFamilySourceBundle
{
    public IReadOnlyDictionary<string, ChunkedPreviewPayload> Goal040PayloadsByScenario { get; init; } = new SortedDictionary<string, ChunkedPreviewPayload>(StringComparer.Ordinal);
    public ChunkedConsumerCatalogSummary Goal040Catalog { get; init; } = new();
    public ChunkedExportManifest Goal040ExportManifest { get; init; } = new();
    public MultiFamilyWorldScaleRegressionMatrix Goal040FamilyRegressionMatrix { get; init; } = new();
    public RuntimePreviewConsumptionProof Goal040ConsumptionProof { get; init; } = new();
    public IReadOnlyDictionary<string, RuntimeChunkTraversalPlan> Goal039PlansByScenario { get; init; } = new SortedDictionary<string, RuntimeChunkTraversalPlan>(StringComparer.Ordinal);
    public IReadOnlyList<MultiFamilySourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyDictionary<string, string> ArtifactTextByRelativePath { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ArtifactHashByRelativePath { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record FamilyTemplateCatalog
{
    public string SchemaVersion { get; init; } = "family_template_catalog_v1";
    public string GoalId { get; init; } = MultiFamilyGeneratedTemplateVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = MultiFamilyGeneratedTemplateVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = MultiFamilyGeneratedTemplateVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal040AcceptedByUserHandoff { get; init; }
    public string Goal040AcceptedGate { get; init; } = "chunked_runtime_preview_export_multifamily_smoke_verification passed";
    public int FamilyCount { get; init; }
    public bool SourceGoal037HybridExpansionConsumed { get; init; }
    public bool SourceGoal038WorldMapConsumed { get; init; }
    public bool SourceGoal039RuntimeTraversalConsumed { get; init; }
    public bool SourceGoal040PreviewExportConsumed { get; init; }
    public IReadOnlyList<FamilyTemplateCatalogEntry> Families { get; init; } = [];
    public IReadOnlyList<MultiFamilySourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FamilyTemplateCatalogEntry
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string FamilyExtensionSchemaId { get; init; } = string.Empty;
    public string DeterministicOrderingKey { get; init; } = string.Empty;
    public string LifecyclePlanFileName { get; init; } = string.Empty;
    public string LoopProofFileName { get; init; } = string.Empty;
    public string SourceGoal040PayloadFileName { get; init; } = string.Empty;
    public string SourceGoal040PayloadHash { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedFeatureRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedIntentionRefs { get; init; } = [];
    public IReadOnlyList<string> RequiredFamilyMarkers { get; init; } = [];
}

public sealed record FamilyLifecyclePlan
{
    public string SchemaVersion { get; init; } = "family_lifecycle_plan_v1";
    public string GoalId { get; init; } = MultiFamilyGeneratedTemplateVocabulary.GoalId;
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string SharedLifecycleContractId { get; init; } = MultiFamilyGeneratedTemplateVocabulary.SharedLifecycleContractId;
    public string DeterministicOrderingKey { get; init; } = string.Empty;
    public IReadOnlyList<string> LifecyclePhases { get; init; } = [];
    public IReadOnlyList<string> SelectedFeatureRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedIntentionRefs { get; init; } = [];
    public IReadOnlyList<MultiFamilySourceArtifactReference> DraftLuaExpansionSourceRefs { get; init; } = [];
    public IReadOnlyList<MultiFamilySourceArtifactReference> RegionChunkTraversalSourceRefs { get; init; } = [];
    public IReadOnlyList<MultiFamilySourceArtifactReference> PreviewExportConsumerRefs { get; init; } = [];
    public IReadOnlyList<MultiFamilySourceArtifactReference> SourceReferences { get; init; } = [];
    public FamilySpecificExtension FamilyExtension { get; init; } = new();
    public IReadOnlyList<FamilyLoopCommand> LoopCommands { get; init; } = [];
    public IReadOnlyList<FamilyValidationTraceEntry> ValidationTrace { get; init; } = [];
    public FamilyTemplateBoundaryClaims BoundaryClaims { get; init; } = new();
    public bool ArchitectureForkAttempted { get; init; }
    public bool FinalProsePromotedAsPlayableContent { get; init; }
    public IReadOnlyList<string> UnscopedFamilySpecificFields { get; init; } = [];
}

public sealed record FamilySpecificExtension
{
    public string FamilyId { get; init; } = string.Empty;
    public string ExtensionSchemaId { get; init; } = string.Empty;
    public IReadOnlyList<string> PresentationMarkers { get; init; } = [];
    public IReadOnlyList<string> LoopMarkers { get; init; } = [];
    public IReadOnlyDictionary<string, string> Values { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record FamilyLoopCommand
{
    public int Order { get; init; }
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string FamilyMarker { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "applied";
}

public sealed record FamilyLoopEvent
{
    public int Order { get; init; }
    public string EventId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string StateKey { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
}

public sealed record FamilyLoopState
{
    public IReadOnlyDictionary<string, string> Values { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record BlockedInvalidAction
{
    public bool Blocked { get; init; }
    public string CommandId { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public sealed record FamilySimulatableLoopProof
{
    public string SchemaVersion { get; init; } = "family_simulatable_loop_proof_v1";
    public string GoalId { get; init; } = MultiFamilyGeneratedTemplateVocabulary.GoalId;
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public FamilyLoopState InitialState { get; init; } = new();
    public IReadOnlyList<FamilyLoopCommand> OrderedCommands { get; init; } = [];
    public IReadOnlyList<FamilyLoopEvent> Events { get; init; } = [];
    public FamilyLoopState AfterState { get; init; } = new();
    public IReadOnlyList<string> ChangedMarkers { get; init; } = [];
    public BlockedInvalidAction BlockedInvalidAction { get; init; } = new();
    public bool StateChanged { get; init; }
    public bool FamilySpecificMinimumsPassed { get; init; }
    public string ReplayDeterminismHash { get; init; } = string.Empty;
    public string ReplayedDeterminismHash { get; init; } = string.Empty;
    public IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FamilyValidationTraceEntry
{
    public int Order { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public string Status { get; init; } = "passed";
    public string Message { get; init; } = string.Empty;
}

public sealed record SharedLifecycleContract
{
    public string SchemaVersion { get; init; } = "shared_lifecycle_contract_v1";
    public string ContractId { get; init; } = MultiFamilyGeneratedTemplateVocabulary.SharedLifecycleContractId;
    public IReadOnlyList<string> SharedPhaseIds { get; init; } = [];
    public int FamilyCount { get; init; }
    public bool Passed { get; init; }
    public string SharedPhaseHash { get; init; } = string.Empty;
    public IReadOnlyList<FamilyLifecycleContractRow> Families { get; init; } = [];
    public IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FamilyLifecycleContractRow
{
    public string FamilyId { get; init; } = string.Empty;
    public string FamilyExtensionSchemaId { get; init; } = string.Empty;
    public IReadOnlyList<string> PhaseIds { get; init; } = [];
    public bool OnlyFamilyExtensionDiffers { get; init; }
    public bool ArchitectureForked { get; init; }
}

public sealed record MultiFamilyRegressionMatrix
{
    public string SchemaVersion { get; init; } = "multi_family_generated_template_regression_matrix_v1";
    public int FamilyCount { get; init; }
    public int LifecyclePlanCount { get; init; }
    public int SimulatableLoopProofCount { get; init; }
    public bool SharedLifecycleContractPassed { get; init; }
    public bool FamilySpecificMinimumsPassed { get; init; }
    public bool PreviewExportConsumptionPassed { get; init; }
    public bool NoArchitectureForks { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<MultiFamilyRegressionRow> Rows { get; init; } = [];
}

public sealed record MultiFamilyRegressionRow
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public bool UsesSharedLifecycleContract { get; init; }
    public bool UsesFamilyScopedExtensionOnly { get; init; }
    public bool SimulatableLoopProofPassed { get; init; }
    public bool SourceGoal040PreviewExportConsumed { get; init; }
}

public sealed record PreviewExportConsumptionMatrix
{
    public string SchemaVersion { get; init; } = "preview_export_consumption_matrix_v1";
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public bool SourceGoal040PreviewExportConsumed { get; init; }
    public IReadOnlyList<PreviewExportConsumptionRow> Rows { get; init; } = [];
}

public sealed record PreviewExportConsumptionRow
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string Goal040PayloadFileName { get; init; } = string.Empty;
    public string Goal040PayloadHash { get; init; } = string.Empty;
    public string CorePayloadSchemaId { get; init; } = string.Empty;
    public bool FamilyLensFound { get; init; }
    public bool TransformedIntoLifecyclePlan { get; init; }
    public bool PayloadCopiedWithoutTransformation { get; init; }
}

public sealed record InvalidFamilyDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = "invalid_family_diagnostics_matrix_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public int BlockedCount { get; init; }
    public IReadOnlyList<InvalidFamilyDiagnosticsScenario> Scenarios { get; init; } = [];
}

public sealed record InvalidFamilyDiagnosticsScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MultiFamilyGeneratedTemplateReport
{
    public string SchemaVersion { get; init; } = "multi_family_generated_template_vertical_slice_report_v1";
    public string GoalId { get; init; } = MultiFamilyGeneratedTemplateVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = MultiFamilyGeneratedTemplateVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = MultiFamilyGeneratedTemplateVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Goal040AcceptedByUserHandoff { get; init; }
    public string Goal040AcceptedGate { get; init; } = "chunked_runtime_preview_export_multifamily_smoke_verification passed";
    public int FamilyCount { get; init; }
    public int SimulatableLoopProofCount { get; init; }
    public bool SourceGoal037HybridExpansionConsumed { get; init; }
    public bool SourceGoal038WorldMapConsumed { get; init; }
    public bool SourceGoal039RuntimeTraversalConsumed { get; init; }
    public bool SourceGoal040PreviewExportConsumed { get; init; }
    public bool SharedLifecycleContractPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool PreviewExportConsumptionMatrixPassed { get; init; }
    public bool MultiFamilyRegressionPassed { get; init; }
    public string CatalogHash { get; init; } = string.Empty;
    public string SharedLifecycleContractHash { get; init; } = string.Empty;
    public string RegressionMatrixHash { get; init; } = string.Empty;
    public string PreviewExportConsumptionMatrixHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MultiFamilyGeneratedTemplateEvidenceResult
{
    public FamilyTemplateCatalog Catalog { get; init; } = new();
    public SharedLifecycleContract SharedLifecycleContract { get; init; } = new();
    public IReadOnlyList<FamilyLifecyclePlan> Plans { get; init; } = [];
    public IReadOnlyList<FamilySimulatableLoopProof> LoopProofs { get; init; } = [];
    public MultiFamilyRegressionMatrix RegressionMatrix { get; init; } = new();
    public PreviewExportConsumptionMatrix PreviewExportConsumptionMatrix { get; init; } = new();
    public InvalidFamilyDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public MultiFamilyGeneratedTemplateReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record MultiFamilyGeneratedTemplateWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record MultiFamilyGeneratedTemplateDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static MultiFamilyGeneratedTemplateDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static MultiFamilyGeneratedTemplateDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}
