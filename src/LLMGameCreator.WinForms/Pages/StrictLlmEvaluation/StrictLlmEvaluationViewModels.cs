namespace LLMGameCreator.WinForms.Pages.StrictLlmEvaluation;

public sealed record StrictLlmEvaluationViewState
{
    public bool LatestAuditOnly { get; init; } = true;
    public IReadOnlyList<StrictLlmEvaluationProfileOption> Profiles { get; init; } = Array.Empty<StrictLlmEvaluationProfileOption>();
    public string SelectedProfileId { get; init; } = string.Empty;
    public IReadOnlyList<StrictLlmEvaluationContractOption> Contracts { get; init; } = Array.Empty<StrictLlmEvaluationContractOption>();
    public IReadOnlyList<string> SelectedContractIds { get; init; } = Array.Empty<string>();
    public int IterationsPerContract { get; init; } = 1;
    public int MaxTokens { get; init; } = 4000;
    public double Temperature { get; init; } = 0.2;
    public bool EnableRepairAttempt { get; init; } = true;
    public int MaxRepairAttempts { get; init; } = 1;
    public bool StageValidArtifactsForReview { get; init; }
    public string ExtraBrief { get; init; } = string.Empty;
    public string Status { get; init; } = "Not loaded.";
    public string LatestAuditSummary { get; init; } = "No latest audit loaded.";
    public string SummaryText { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string EvaluationJson { get; init; } = string.Empty;
    public IReadOnlyList<StrictLlmEvaluationContractRow> ContractRows { get; init; } = Array.Empty<StrictLlmEvaluationContractRow>();
    public IReadOnlyList<StrictLlmEvaluationDiagnosticRow> DiagnosticRows { get; init; } = Array.Empty<StrictLlmEvaluationDiagnosticRow>();
    public IReadOnlyList<StrictLlmEvaluationSampleRow> SampleRows { get; init; } = Array.Empty<StrictLlmEvaluationSampleRow>();
    public int ExpectedMaxLlmCalls => ContractsForBatch * Math.Clamp(IterationsPerContract, 1, 10) * (1 + (EnableRepairAttempt ? Math.Clamp(MaxRepairAttempts, 0, 2) : 0));
    public int ContractsForBatch => Math.Clamp(SelectedContractIds.Count, 0, 4);
    public bool CanRunBatch => !LatestAuditOnly && Profiles.Count > 0 && !string.IsNullOrWhiteSpace(SelectedProfileId) && SelectedContractIds.Count > 0;
}

public sealed record StrictLlmEvaluationProfileOption
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string DisplayName => string.IsNullOrWhiteSpace(Title) ? $"{Id} ({Model})" : $"{Title} | {Id} | {Model}";
}

public sealed record StrictLlmEvaluationContractOption
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string DisplayName => string.IsNullOrWhiteSpace(Title) ? Id : $"{Title} ({Id})";
}

public sealed record StrictLlmEvaluationContractRow
{
    public string ContractId { get; init; } = string.Empty;
    public int Runs { get; init; }
    public int InitialPass { get; init; }
    public int RepairPass { get; init; }
    public int Failed { get; init; }
    public int ValidArtifacts { get; init; }
    public double AverageAttempts { get; init; }
    public string TopDiagnosticCodes { get; init; } = string.Empty;
}

public sealed record StrictLlmEvaluationDiagnosticRow
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public int Count { get; init; }
    public string ExampleMessage { get; init; } = string.Empty;
}

public sealed record StrictLlmEvaluationSampleRow
{
    public string ContractId { get; init; } = string.Empty;
    public string ArtifactId { get; init; } = string.Empty;
    public bool Valid { get; init; }
    public bool Repaired { get; init; }
    public string ContentExcerpt { get; init; } = string.Empty;
    public string DiagnosticExcerpt { get; init; } = string.Empty;
}
