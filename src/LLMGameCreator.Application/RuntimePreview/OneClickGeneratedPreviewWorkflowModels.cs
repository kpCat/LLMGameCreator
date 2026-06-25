using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed record OneClickGeneratedPreviewWorkflowRequest
{
    public string ProjectRootPath { get; init; } = string.Empty;
    public string Seed { get; init; } = GenerationPresetOptionsService.DefaultSeed;
    public string Mode { get; init; } = ProceduralGameGenerationModes.SemiProceduralRegions;
    public string PresetId { get; init; } = GenerationPresetOptionsService.DefaultPresetId;
    public IReadOnlyList<string> CompactStyleHintIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedVariantIds { get; init; } = Array.Empty<string>();
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
    public GenerationPresetOptions GenerationOptions { get; init; } = new();
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
    public string MicrogameAcceptanceOutputDirectoryPath { get; init; } = string.Empty;
    public string MicrogameAcceptanceSnapshotJsonPath { get; init; } = string.Empty;
    public string MicrogameAcceptanceReportMarkdownPath { get; init; } = string.Empty;
    public string MicrogameManualVerificationMarkdownPath { get; init; } = string.Empty;
    public string RuntimeBackedMicrogameStateOutputDirectoryPath { get; init; } = string.Empty;
    public string RuntimeBackedMicrogameStateSnapshotJsonPath { get; init; } = string.Empty;
    public string RuntimeBackedMicrogameStateReportMarkdownPath { get; init; } = string.Empty;
    public string RuntimeBackedMicrogameManualVerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record OneClickGeneratedPreviewWorkflowDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
