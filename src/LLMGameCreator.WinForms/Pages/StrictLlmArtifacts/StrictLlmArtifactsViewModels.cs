using LLMGameCreator.Application.Design.GeneratorPlans;

namespace LLMGameCreator.WinForms.Pages.StrictLlmArtifacts;

public sealed record StrictLlmArtifactsViewState
{
    public IReadOnlyList<StrictLlmProfileOption> Profiles { get; init; } = Array.Empty<StrictLlmProfileOption>();
    public string SelectedProfileId { get; init; } = string.Empty;
    public IReadOnlyList<StrictLlmContractOption> Contracts { get; init; } = Array.Empty<StrictLlmContractOption>();
    public IReadOnlyList<string> SelectedContractIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<StrictLlmBatchPresetOption> BatchPresets { get; init; } = Array.Empty<StrictLlmBatchPresetOption>();
    public string SelectedBatchPresetId { get; init; } = string.Empty;
    public int MaxTokens { get; init; } = 4000;
    public double Temperature { get; init; } = 0.2;
    public bool EnableRepairAttempt { get; init; } = true;
    public bool StageForReview { get; init; } = true;
    public string ExtraBrief { get; init; } = string.Empty;
    public bool HasLatestSelection { get; init; }
    public string SelectionId { get; init; } = string.Empty;
    public string SourceSummary { get; init; } = "No capability selection loaded.";
    public string Status { get; init; } = "Not loaded.";
    public string PromptPreview { get; init; } = string.Empty;
    public string ResultJson { get; init; } = string.Empty;
    public IReadOnlyList<StrictLlmArtifactRow> ArtifactRows { get; init; } = Array.Empty<StrictLlmArtifactRow>();
    public IReadOnlyList<StrictLlmDiagnosticRow> DiagnosticRows { get; init; } = Array.Empty<StrictLlmDiagnosticRow>();
    public bool CanGenerate => Profiles.Count > 0 && HasLatestSelection && SelectedContractIds.Count > 0;
}

public sealed record StrictLlmProfileOption
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string DisplayName => string.IsNullOrWhiteSpace(Title) ? $"{Id} ({Model})" : $"{Title} | {Id} | {Model}";
}

public sealed record StrictLlmContractOption
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string DisplayName => string.IsNullOrWhiteSpace(Title) ? Id : $"{Title} ({Id})";
}

public sealed record StrictLlmBatchPresetOption
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string DisplayName => string.IsNullOrWhiteSpace(Id)
        ? Title
        : string.IsNullOrWhiteSpace(Title) ? Id : $"{Title} ({Id})";
}

public sealed record StrictLlmArtifactRow
{
    public string ArtifactId { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Contract { get; init; } = string.Empty;
    public bool Valid { get; init; }
    public bool Repaired { get; init; }
    public bool RequiresApproval { get; init; }
}

public sealed record StrictLlmDiagnosticRow
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
