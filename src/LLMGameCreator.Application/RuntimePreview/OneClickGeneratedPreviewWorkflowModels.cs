using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed record OneClickGeneratedPreviewWorkflowRequest
{
    public string ProjectRootPath { get; init; } = string.Empty;
    public string Seed { get; init; } = "one-click-generated-preview-workflow";
    public string Mode { get; init; } = ProceduralGameGenerationModes.SemiProceduralRegions;
    public IReadOnlyList<string> CompactStyleHintIds { get; init; } =
    [
        "theme/exploration",
        "theme/survival",
        "tone/mysterious",
        "quest_motif/faction_truce",
        "item_affordance/quest_item"
    ];
    public IReadOnlyList<string> SelectedVariantIds { get; init; } =
    [
        "world_topology/region_graph",
        "actor_model/single_player_character",
        "combat_model/turn_based",
        "inventory_model/list_inventory"
    ];
    public bool ReplaceCurrentPackage { get; init; } = true;
}

public sealed record OneClickGeneratedPreviewWorkflowResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public GamePackageDefinition GeneratedPackage { get; init; } = new();
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public string ProjectRootPath { get; init; } = string.Empty;
    public OneClickGeneratedPreviewWorkflowPaths Paths { get; init; } = new();
    public string StableSummary { get; init; } = string.Empty;
    public bool CurrentPackageReplaced { get; init; }
    public VisibleGeneratedPlayablePreviewResult VisiblePreviewResult { get; init; } = new();
    public IReadOnlyList<OneClickGeneratedPreviewWorkflowDiagnostic> Diagnostics { get; init; } = Array.Empty<OneClickGeneratedPreviewWorkflowDiagnostic>();
}

public sealed record OneClickGeneratedPreviewWorkflowPaths
{
    public string PlanJsonPath { get; init; } = string.Empty;
    public string RulePackJsonPath { get; init; } = string.Empty;
    public string TinyRuntimeLoopStateJsonPath { get; init; } = string.Empty;
    public string GeneratedPackageOutputDirectoryPath { get; init; } = string.Empty;
    public string GeneratedPackageJsonPath { get; init; } = string.Empty;
    public string VisiblePreviewOutputDirectoryPath { get; init; } = string.Empty;
    public string VisiblePreviewSnapshotJsonPath { get; init; } = string.Empty;
    public string VisiblePreviewReportJsonPath { get; init; } = string.Empty;
    public string VisiblePreviewReportMarkdownPath { get; init; } = string.Empty;
    public string ManualVerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record OneClickGeneratedPreviewWorkflowDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
