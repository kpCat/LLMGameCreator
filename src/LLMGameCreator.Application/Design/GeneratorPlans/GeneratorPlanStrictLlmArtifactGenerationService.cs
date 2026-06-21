using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Generation;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.Application.Validation;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmArtifactGenerationService
{
    private readonly IAppSettingsRepository _settingsRepository;
    private readonly ILlmChatClient _llmChatClient;
    private readonly GeneratorPlanCapabilitySelectionArtifactReader _selectionReader;
    private readonly GeneratorPlanStrictLlmArtifactContractCatalog _contractCatalog;
    private readonly GeneratorPlanStrictLlmArtifactPromptBuilder _promptBuilder;
    private readonly GeneratorPlanStrictJsonResponseParser _parser;
    private readonly GeneratorPlanStrictLlmArtifactValidator _validator;
    private readonly GeneratorPlanStrictLlmArtifactRepairPromptBuilder _repairPromptBuilder;
    private readonly GeneratorPlanDraftArtifactApprovalService _approvalService;
    private readonly GeneratorPlanDraftArtifactApprovalArtifactService _approvalArtifactService;
    private readonly GeneratorPlanStrictLlmArtifactGenerationArtifactService _artifactService;
    private readonly ContentLanguageDiagnosticService _languageDiagnosticService;

    public GeneratorPlanStrictLlmArtifactGenerationService(
        IAppSettingsRepository settingsRepository,
        ILlmChatClient llmChatClient,
        GeneratorPlanCapabilitySelectionArtifactReader selectionReader,
        GeneratorPlanStrictLlmArtifactContractCatalog contractCatalog,
        GeneratorPlanStrictLlmArtifactPromptBuilder promptBuilder,
        GeneratorPlanStrictJsonResponseParser parser,
        GeneratorPlanStrictLlmArtifactValidator validator,
        GeneratorPlanStrictLlmArtifactRepairPromptBuilder repairPromptBuilder,
        GeneratorPlanDraftArtifactApprovalService approvalService,
        GeneratorPlanDraftArtifactApprovalArtifactService approvalArtifactService,
        GeneratorPlanStrictLlmArtifactGenerationArtifactService artifactService,
        ContentLanguageDiagnosticService? languageDiagnosticService = null)
    {
        _settingsRepository = settingsRepository;
        _llmChatClient = llmChatClient;
        _selectionReader = selectionReader;
        _contractCatalog = contractCatalog;
        _promptBuilder = promptBuilder;
        _parser = parser;
        _validator = validator;
        _repairPromptBuilder = repairPromptBuilder;
        _approvalService = approvalService;
        _approvalArtifactService = approvalArtifactService;
        _artifactService = artifactService;
        _languageDiagnosticService = languageDiagnosticService ?? new ContentLanguageDiagnosticService();
    }

    public async Task<GeneratorPlanStrictLlmArtifactPromptPreviewResult> PreviewPromptAsync(
        GeneratorPlanStrictLlmArtifactGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var diagnostics = new List<GeneratorPlanStrictLlmArtifactDiagnostic>();
        var contractId = NormalizeContracts(request.ContractIds).FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(contractId) || !_contractCatalog.TryGet(contractId, out var contract))
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.UnknownContract, $"Unknown or missing contract id: {contractId}", "contract_ids", contractId));
            return new GeneratorPlanStrictLlmArtifactPromptPreviewResult { Ok = false, Status = GeneratorPlanStrictLlmArtifactGenerationStatus.Invalid, Diagnostics = diagnostics };
        }

        var selection = await LoadSelectionAsync(diagnostics, cancellationToken).ConfigureAwait(false);
        if (selection == null)
        {
            return new GeneratorPlanStrictLlmArtifactPromptPreviewResult { Ok = false, Status = GeneratorPlanStrictLlmArtifactGenerationStatus.Invalid, ContractId = contractId, Diagnostics = diagnostics };
        }

        var prompt = _promptBuilder.Build(contract, selection, request);
        return new GeneratorPlanStrictLlmArtifactPromptPreviewResult
        {
            Ok = true,
            Status = "preview_ready",
            ContractId = contractId,
            PromptText = prompt.CombinedText,
            Diagnostics = diagnostics
        };
    }

    public async Task<GeneratorPlanStrictLlmArtifactGenerationResult> GenerateAsync(
        GeneratorPlanStrictLlmArtifactGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var diagnostics = new List<GeneratorPlanStrictLlmArtifactDiagnostic>();
        var artifacts = new List<GeneratorPlanStrictLlmGeneratedArtifact>();
        var attempts = new List<GeneratorPlanStrictLlmArtifactGenerationAttempt>();
        var requestedContracts = NormalizeContracts(request.ContractIds);
        var settings = await _settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
        var savePromptText = settings.Generation.SaveEveryRequest;
        var profile = ResolveProfile(settings, request.LlmProfileId);

        if (profile == null)
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.MissingLlmProfile, "LLM profile is missing or incomplete.", "llm_profile_id", string.Empty));
            return await SaveAndReturnAsync(BuildResult(false, generatedAtUtc, string.Empty, requestedContracts, artifacts, attempts, diagnostics, null), cancellationToken).ConfigureAwait(false);
        }

        if (requestedContracts.Count == 0)
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.MissingContracts, "At least one contract id is required.", "contract_ids", string.Empty));
            return await SaveAndReturnAsync(BuildResult(false, generatedAtUtc, string.Empty, requestedContracts, artifacts, attempts, diagnostics, null), cancellationToken).ConfigureAwait(false);
        }

        var selection = await LoadSelectionAsync(diagnostics, cancellationToken).ConfigureAwait(false);
        if (selection == null)
        {
            return await SaveAndReturnAsync(BuildResult(false, generatedAtUtc, string.Empty, requestedContracts, artifacts, attempts, diagnostics, null), cancellationToken).ConfigureAwait(false);
        }

        foreach (var contractId in requestedContracts)
        {
            if (!_contractCatalog.TryGet(contractId, out var contract))
            {
                diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.UnknownContract, $"Unknown contract id: {contractId}", "contract_ids", contractId));
                continue;
            }

            var prompt = _promptBuilder.Build(contract, selection, request);
            var response = await CompleteAsync(profile, prompt, request, diagnostics, cancellationToken).ConfigureAwait(false);
            if (response == null)
            {
                continue;
            }

            var artifact = EvaluateAttempt(contract, prompt, response.Content, request.ContentLanguage, 0, false, savePromptText, attempts);
            if (artifact.Valid)
            {
                artifacts.Add(artifact);
                AddLanguageWarnings(diagnostics, attempts, contract.ContractId);
                continue;
            }

            if (!request.EnableRepairAttempt)
            {
                diagnostics.AddRange(LatestAttemptDiagnostics(attempts, contract.ContractId));
                continue;
            }

            var maxRepairAttempts = Math.Clamp(request.MaxRepairAttempts, 0, 2);
            var repaired = false;
            for (var repairIndex = 1; repairIndex <= maxRepairAttempts; repairIndex++)
            {
                var latestDiagnostics = LatestAttemptDiagnostics(attempts, contract.ContractId);
                var repairPrompt = _repairPromptBuilder.BuildRepairPrompt(contract, prompt, response.Content, latestDiagnostics, repairIndex, request.ContentLanguage);
                var repairResponse = await CompleteAsync(profile, repairPrompt, request, diagnostics, cancellationToken).ConfigureAwait(false);
                if (repairResponse == null)
                {
                    break;
                }

                var repairedArtifact = EvaluateAttempt(contract, repairPrompt, repairResponse.Content, request.ContentLanguage, repairIndex, true, savePromptText, attempts);
                if (repairedArtifact.Valid)
                {
                    artifacts.Add(repairedArtifact);
                    AddLanguageWarnings(diagnostics, attempts, contract.ContractId);
                    repaired = true;
                    break;
                }

                response = repairResponse;
            }

            if (!repaired)
            {
                diagnostics.AddRange(LatestAttemptDiagnostics(attempts, contract.ContractId));
            }
        }

        GeneratorPlanDraftArtifactApprovalResult? stagingResult = null;
        if (request.StageForReview && artifacts.Any(artifact => artifact.Valid))
        {
            stagingResult = await StageForReviewAsync(artifacts, generatedAtUtc, selection.SelectionId, cancellationToken).ConfigureAwait(false);
        }

        var ok = diagnostics.All(diagnostic => diagnostic.Severity != GeneratorPlanPreviewDiagnosticSeverity.Error)
            && artifacts.Count > 0;
        var result = BuildResult(ok, generatedAtUtc, selection.SelectionId, requestedContracts, artifacts, attempts, diagnostics, stagingResult);
        return await SaveAndReturnAsync(result, cancellationToken).ConfigureAwait(false);
    }

    private async Task<GeneratorPlanStrictLlmArtifactGenerationResult> SaveAndReturnAsync(
        GeneratorPlanStrictLlmArtifactGenerationResult result,
        CancellationToken cancellationToken)
    {
        await _artifactService.SaveAsync(result, cancellationToken).ConfigureAwait(false);
        return result;
    }

    private async Task<GeneratorPlanCapabilitySelection?> LoadSelectionAsync(
        List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var latest = await _selectionReader.ReadLatestAsync(cancellationToken).ConfigureAwait(false);
        if (!latest.Exists || string.IsNullOrWhiteSpace(latest.Selection.SelectionId))
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.MissingCapabilitySelection, "Latest capability selection artifact is missing.", "capability_selection", string.Empty));
            return null;
        }

        return latest.Selection;
    }

    private async Task<LlmChatResponse?> CompleteAsync(
        LlmEndpointSettings profile,
        GeneratorPlanStrictLlmArtifactPrompt prompt,
        GeneratorPlanStrictLlmArtifactGenerationRequest request,
        List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _llmChatClient.CompleteAsync(profile, new LlmChatRequest
            {
                SystemPrompt = prompt.SystemPrompt,
                UserPrompt = prompt.UserPrompt,
                Temperature = Math.Clamp(request.Temperature, 0.0, 1.0),
                MaxTokens = Math.Clamp(request.MaxTokens, 256, 12000)
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.LlmCallFailed, ex.Message, "llm", prompt.ContractId));
            return null;
        }
    }

    private GeneratorPlanStrictLlmGeneratedArtifact EvaluateAttempt(
        GeneratorPlanStrictLlmArtifactContractDefinition contract,
        GeneratorPlanStrictLlmArtifactPrompt prompt,
        string responseContent,
        string contentLanguage,
        int attemptIndex,
        bool isRepairAttempt,
        bool savePromptText,
        List<GeneratorPlanStrictLlmArtifactGenerationAttempt> attempts)
    {
        var parse = _parser.Parse(responseContent, contract.ContractId);
        var validationDiagnostics = parse.Ok
            ? _validator.Validate(parse.Json, contract).Concat(
                _languageDiagnosticService.Inspect(parse.Json, contentLanguage).Select(diagnostic => Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    GeneratorPlanStrictLlmArtifactDiagnosticCodes.ContentLanguageWarning,
                    diagnostic.Message,
                    diagnostic.Target,
                    contract.ContractId))).ToList()
            : parse.Diagnostics;
        var validationOk = validationDiagnostics.All(diagnostic => diagnostic.Severity != GeneratorPlanPreviewDiagnosticSeverity.Error);

        attempts.Add(new GeneratorPlanStrictLlmArtifactGenerationAttempt
        {
            ContractId = contract.ContractId,
            AttemptIndex = attemptIndex,
            IsRepairAttempt = isRepairAttempt,
            PromptHash = StableHash(prompt.CombinedText),
            ResponseHash = StableHash(responseContent),
            ParsedOk = parse.Ok,
            ValidationOk = validationOk,
            Diagnostics = validationDiagnostics,
            PromptText = savePromptText ? prompt.CombinedText : string.Empty,
            ResponseText = savePromptText ? responseContent : string.Empty
        });

        return new GeneratorPlanStrictLlmGeneratedArtifact
        {
            ArtifactId = $"artifact/strict_llm/{contract.ContractId}",
            ArtifactKind = contract.ArtifactKind,
            ExpectedArtifactContract = contract.ContractId,
            ContentJson = parse.Ok ? parse.Json : "{}",
            Valid = validationOk,
            Repaired = isRepairAttempt && validationOk,
            RequiresHumanApproval = true
        };
    }

    private static IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> LatestAttemptDiagnostics(
        IReadOnlyList<GeneratorPlanStrictLlmArtifactGenerationAttempt> attempts,
        string contractId)
    {
        return attempts
            .LastOrDefault(attempt => string.Equals(attempt.ContractId, contractId, StringComparison.OrdinalIgnoreCase))
            ?.Diagnostics
            ?? Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();
    }

    private static void AddLanguageWarnings(
        List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactGenerationAttempt> attempts,
        string contractId)
    {
        diagnostics.AddRange(LatestAttemptDiagnostics(attempts, contractId)
            .Where(diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.ContentLanguageWarning));
    }

    private async Task<GeneratorPlanDraftArtifactApprovalResult> StageForReviewAsync(
        IReadOnlyList<GeneratorPlanStrictLlmGeneratedArtifact> artifacts,
        DateTimeOffset generatedAtUtc,
        string selectionId,
        CancellationToken cancellationToken)
    {
        var batchId = "strict_llm/" + StableHash(selectionId, generatedAtUtc.ToString("O"))[..16];
        var productionArtifacts = artifacts
            .Where(artifact => artifact.Valid)
            .Select(artifact => new GeneratorPlanProducedDraftArtifact
            {
                Id = artifact.ArtifactId,
                QueueItemId = $"strict_llm/{artifact.ExpectedArtifactContract}",
                SourceExecutionStepId = $"strict_llm/{artifact.ExpectedArtifactContract}",
                ArtifactId = artifact.ArtifactId,
                ArtifactKind = artifact.ArtifactKind,
                ExpectedArtifactContract = artifact.ExpectedArtifactContract,
                State = GeneratorPlanProducedDraftArtifactState.ReadyForApproval,
                ContentJson = artifact.ContentJson,
                ValidationGates = ["strict_llm_contract_validator"],
                RequiresHumanApproval = true
            })
            .ToList();

        var productionResult = new GeneratorPlanDraftArtifactProductionResult
        {
            Ok = true,
            Status = GeneratorPlanDraftArtifactProductionStatus.ReadyForApproval,
            GeneratedAtUtc = generatedAtUtc,
            Batch = new GeneratorPlanDraftArtifactProductionBatch
            {
                Id = batchId,
                SourceQueueId = batchId,
                SourceDraftExecutionPlanId = "strict_llm",
                SourcePreviewExampleId = selectionId,
                SourcePath = GeneratorPlanStrictLlmArtifactGenerationArtifactIds.GenerationArtifactPath,
                Status = GeneratorPlanDraftArtifactProductionStatus.ReadyForApproval,
                Artifacts = productionArtifacts,
                Summary = new GeneratorPlanDraftArtifactProductionSummary
                {
                    ArtifactCount = productionArtifacts.Count,
                    ReadyForApprovalCount = productionArtifacts.Count
                }
            }
        };

        var approvalResult = _approvalService.CreateSnapshot(productionResult, new GeneratorPlanDraftArtifactApprovalRequest
        {
            AutoApproveValidArtifacts = false,
            RenderMarkdown = true
        });

        await _approvalArtifactService.SaveAsync(approvalResult, new GeneratorPlanDraftArtifactApprovalArtifactSaveRequest(), cancellationToken).ConfigureAwait(false);
        return approvalResult;
    }

    private static GeneratorPlanStrictLlmArtifactGenerationResult BuildResult(
        bool ok,
        DateTimeOffset generatedAtUtc,
        string selectionId,
        IReadOnlyList<string> requestedContracts,
        IReadOnlyList<GeneratorPlanStrictLlmGeneratedArtifact> artifacts,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactGenerationAttempt> attempts,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics,
        GeneratorPlanDraftArtifactApprovalResult? stagingResult)
    {
        var hasErrors = diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        return new GeneratorPlanStrictLlmArtifactGenerationResult
        {
            Ok = ok,
            Status = hasErrors
                ? GeneratorPlanStrictLlmArtifactGenerationStatus.Invalid
                : diagnostics.Count > 0
                    ? GeneratorPlanStrictLlmArtifactGenerationStatus.GeneratedWithDiagnostics
                    : GeneratorPlanStrictLlmArtifactGenerationStatus.Generated,
            GeneratedAtUtc = generatedAtUtc,
            SourceCapabilitySelectionId = selectionId,
            RequestedContractIds = requestedContracts,
            Artifacts = artifacts,
            Attempts = attempts,
            Diagnostics = diagnostics
                .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
                .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                .ThenBy(diagnostic => diagnostic.ContractId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            StagingResult = stagingResult
        };
    }

    private static LlmEndpointSettings? ResolveProfile(AppSettings settings, string requestedProfileId)
    {
        var profile = settings.LlmProfiles.FirstOrDefault(item => string.Equals(item.Id, requestedProfileId, StringComparison.Ordinal))
            ?? (!string.IsNullOrWhiteSpace(settings.DefaultLlmProfileId)
                ? settings.LlmProfiles.FirstOrDefault(item => string.Equals(item.Id, settings.DefaultLlmProfileId, StringComparison.Ordinal))
                : null)
            ?? settings.LlmProfiles.FirstOrDefault();

        return profile == null || string.IsNullOrWhiteSpace(profile.Endpoint) || string.IsNullOrWhiteSpace(profile.Model)
            ? null
            : profile;
    }

    private static IReadOnlyList<string> NormalizeContracts(IEnumerable<string> contractIds)
    {
        return contractIds
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static GeneratorPlanStrictLlmArtifactDiagnostic Diagnostic(string severity, string code, string message, string target, string contractId)
    {
        return new GeneratorPlanStrictLlmArtifactDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            Target = target,
            ContractId = contractId
        };
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

    private static string StableHash(params string[] values)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", values)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
