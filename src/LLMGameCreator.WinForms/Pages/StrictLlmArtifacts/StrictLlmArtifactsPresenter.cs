using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Settings;

namespace LLMGameCreator.WinForms.Pages.StrictLlmArtifacts;

public sealed class StrictLlmArtifactsPresenter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public StrictLlmArtifactsViewState FromSettings(
        StrictLlmArtifactsViewState state,
        AppSettings settings,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactContractDefinition> contracts)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(contracts);

        var profiles = settings.LlmProfiles
            .Where(profile => !string.IsNullOrWhiteSpace(profile.Id))
            .Select(profile => new StrictLlmProfileOption
            {
                Id = profile.Id,
                Title = profile.Title,
                Model = profile.Model
            })
            .OrderBy(profile => profile.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(profile => profile.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var contractOptions = contracts
            .Select(contract => new StrictLlmContractOption
            {
                Id = contract.ContractId,
                Title = contract.ArtifactKind
            })
            .ToList();

        var selectedProfile = FirstExisting(state.SelectedProfileId, profiles.Select(profile => profile.Id))
            ?? FirstExisting(settings.DefaultLlmProfileId, profiles.Select(profile => profile.Id))
            ?? profiles.FirstOrDefault()?.Id
            ?? string.Empty;

        return state with
        {
            Profiles = profiles,
            SelectedProfileId = selectedProfile,
            Contracts = contractOptions,
            SelectedContractIds = state.SelectedContractIds.Count > 0
                ? state.SelectedContractIds
                : contractOptions.Select(contract => contract.Id).ToList(),
            Status = profiles.Count == 0 ? "No LLM profile configured." : state.Status
        };
    }

    public StrictLlmArtifactsViewState FromLatestSelection(
        StrictLlmArtifactsViewState state,
        GeneratorPlanCapabilitySelectionArtifactReadResult latest)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(latest);

        if (!latest.Exists || string.IsNullOrWhiteSpace(latest.Selection.SelectionId))
        {
            return state with
            {
                HasLatestSelection = false,
                SelectionId = string.Empty,
                SourceSummary = "No latest capability selection found.",
                Status = "Load Capability Picker selection before generation."
            };
        }

        var selection = latest.Selection;
        return state with
        {
            HasLatestSelection = true,
            SelectionId = selection.SelectionId,
            SourceSummary = BuildSelectionSummary(selection),
            Status = "Latest capability selection loaded."
        };
    }

    public GeneratorPlanStrictLlmArtifactGenerationRequest BuildRequest(StrictLlmArtifactsViewState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return new GeneratorPlanStrictLlmArtifactGenerationRequest
        {
            LlmProfileId = state.SelectedProfileId,
            ContractIds = state.SelectedContractIds,
            UseLatestCapabilitySelection = true,
            StageForReview = state.StageForReview,
            EnableRepairAttempt = state.EnableRepairAttempt,
            MaxRepairAttempts = state.EnableRepairAttempt ? 1 : 0,
            MaxTokens = state.MaxTokens,
            Temperature = state.Temperature,
            ExtraUserBrief = state.ExtraBrief
        };
    }

    public StrictLlmArtifactsViewState FromPreview(
        StrictLlmArtifactsViewState state,
        GeneratorPlanStrictLlmArtifactPromptPreviewResult preview)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(preview);

        return state with
        {
            Status = preview.Ok ? "Prompt preview ready." : preview.Status,
            PromptPreview = preview.PromptText,
            DiagnosticRows = DiagnosticRows(preview.Diagnostics)
        };
    }

    public StrictLlmArtifactsViewState FromGenerationResult(
        StrictLlmArtifactsViewState state,
        GeneratorPlanStrictLlmArtifactGenerationResult result)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(result);

        var status = result.Ok && result.StagingResult != null
            ? "Artifacts staged for review. Open Artifact Review to approve."
            : result.Status;

        return state with
        {
            Status = status,
            ResultJson = JsonSerializer.Serialize(result, JsonOptions),
            ArtifactRows = result.Artifacts
                .OrderBy(artifact => artifact.ExpectedArtifactContract, StringComparer.OrdinalIgnoreCase)
                .Select(artifact => new StrictLlmArtifactRow
                {
                    ArtifactId = artifact.ArtifactId,
                    Kind = artifact.ArtifactKind,
                    Contract = artifact.ExpectedArtifactContract,
                    Valid = artifact.Valid,
                    Repaired = artifact.Repaired,
                    RequiresApproval = artifact.RequiresHumanApproval
                })
                .ToList(),
            DiagnosticRows = DiagnosticRows(result.Diagnostics)
        };
    }

    public StrictLlmArtifactsViewState FromLatestAudit(
        StrictLlmArtifactsViewState state,
        GeneratorPlanStrictLlmArtifactGenerationArtifactReadResult latest)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(latest);

        if (!latest.Exists || latest.GenerationArtifact == null)
        {
            return state with
            {
                Status = "No strict LLM generation audit found.",
                ResultJson = string.Empty,
                ArtifactRows = Array.Empty<StrictLlmArtifactRow>(),
                DiagnosticRows = Array.Empty<StrictLlmDiagnosticRow>()
            };
        }

        return FromGenerationResult(state, latest.Result) with
        {
            Status = "Latest strict LLM generation audit loaded.",
            ResultJson = latest.GenerationArtifact.Json
        };
    }

    private static IReadOnlyList<StrictLlmDiagnosticRow> DiagnosticRows(IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics)
    {
        return diagnostics
            .Select(diagnostic => new StrictLlmDiagnosticRow
            {
                Severity = diagnostic.Severity,
                Code = diagnostic.Code,
                ContractId = diagnostic.ContractId,
                Target = diagnostic.Target,
                Message = diagnostic.Message
            })
            .OrderBy(row => SeverityOrder(row.Severity))
            .ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ContractId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Target, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string BuildSelectionSummary(GeneratorPlanCapabilitySelection selection)
    {
        return string.Join(Environment.NewLine, new[]
        {
            $"Selection id: {selection.SelectionId}",
            $"Title: {selection.Title}",
            $"Purpose: {selection.Purpose}",
            "Selected variant ids:",
            JsonSerializer.Serialize(selection.SelectedVariantIds, JsonOptions),
            $"Feature bundles: {selection.SelectedFeatureBundleIds.Count}",
            $"Warnings: {selection.Warnings.Count}",
            $"Errors: {selection.Errors.Count}"
        });
    }

    private static string? FirstExisting(string selectedId, IEnumerable<string> ids)
    {
        return ids.FirstOrDefault(id => string.Equals(id, selectedId, StringComparison.OrdinalIgnoreCase));
    }

    private static int SeverityOrder(string severity)
    {
        return severity switch
        {
            GeneratorPlanPreviewDiagnosticSeverity.Error => 0,
            GeneratorPlanPreviewDiagnosticSeverity.Warning => 1,
            GeneratorPlanPreviewDiagnosticSeverity.Info => 2,
            _ => 3
        };
    }
}
