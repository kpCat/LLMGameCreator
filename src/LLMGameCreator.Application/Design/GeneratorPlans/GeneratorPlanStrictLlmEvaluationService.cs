using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmEvaluationService
{
    private static readonly string[] DefaultContractIds =
    [
        "game_profile_v1",
        "scene_pack_v1",
        "quest_pack_v1",
        "mechanics_pack_v1"
    ];

    private readonly GeneratorPlanStrictLlmArtifactGenerationArtifactReader _auditReader;
    private readonly GeneratorPlanStrictLlmArtifactGenerationService _generationService;
    private readonly GeneratorPlanCapabilitySelectionArtifactReader _selectionReader;
    private readonly GeneratorPlanStrictLlmArtifactContractCatalog _contractCatalog;
    private readonly GeneratorPlanStrictLlmEvaluationMarkdownRenderer _markdownRenderer;
    private readonly GeneratorPlanStrictLlmEvaluationArtifactService _artifactService;

    public GeneratorPlanStrictLlmEvaluationService(
        GeneratorPlanStrictLlmArtifactGenerationArtifactReader auditReader,
        GeneratorPlanStrictLlmArtifactGenerationService generationService,
        GeneratorPlanCapabilitySelectionArtifactReader selectionReader,
        GeneratorPlanStrictLlmArtifactContractCatalog contractCatalog,
        GeneratorPlanStrictLlmEvaluationMarkdownRenderer markdownRenderer,
        GeneratorPlanStrictLlmEvaluationArtifactService artifactService)
    {
        _auditReader = auditReader;
        _generationService = generationService;
        _selectionReader = selectionReader;
        _contractCatalog = contractCatalog;
        _markdownRenderer = markdownRenderer;
        _artifactService = artifactService;
    }

    public async Task<GeneratorPlanStrictLlmEvaluationResult> EvaluateLatestAuditAsync(CancellationToken cancellationToken = default)
    {
        var latest = await _auditReader.ReadLatestAsync(cancellationToken).ConfigureAwait(false);
        if (!latest.Exists || latest.GenerationArtifact == null)
        {
            var missing = BuildInvalidResult(
                GeneratorPlanStrictLlmEvaluationMode.LatestAudit,
                Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanStrictLlmEvaluationDiagnosticCodes.MissingAudit,
                    "Latest strict LLM generation audit is missing.",
                    "strict_llm_generation_audit",
                    string.Empty));
            return await SaveAndReturnAsync(missing, cancellationToken).ConfigureAwait(false);
        }

        var selection = await ReadSelectionOrNullAsync(cancellationToken).ConfigureAwait(false);
        var result = BuildEvaluation(
            GeneratorPlanStrictLlmEvaluationMode.LatestAudit,
            latest.Result.SourceCapabilitySelectionId,
            latest.Result.RequestedContractIds,
            Math.Max(1, latest.Result.RequestedContractIds.Count),
            true,
            1,
            latest.Result.StagingResult != null,
            0,
            [latest.Result],
            BuildRunsFromLatestAudit(latest.Result),
            selection);

        return await SaveAndReturnAsync(result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GeneratorPlanStrictLlmEvaluationResult> RunEvaluationBatchAsync(
        GeneratorPlanStrictLlmEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestedContracts = NormalizeContracts(request.ContractIds.Count == 0 ? DefaultContractIds : request.ContractIds)
            .Take(4)
            .ToList();
        var iterations = Math.Clamp(request.IterationsPerContract, 1, 10);
        var repairAttempts = request.EnableRepairAttempt ? Math.Clamp(request.MaxRepairAttempts, 0, 2) : 0;
        var expectedCalls = ExpectedMaxLlmCalls(requestedContracts.Count, iterations, request.EnableRepairAttempt, repairAttempts);
        var validationDiagnostics = ValidateBatchRequest(request, requestedContracts);
        if (validationDiagnostics.Count > 0)
        {
            var invalid = BuildInvalidResult(
                GeneratorPlanStrictLlmEvaluationMode.Batch,
                validationDiagnostics,
                requestedContracts,
                iterations,
                request.EnableRepairAttempt,
                repairAttempts,
                request.StageValidArtifactsForReview,
                expectedCalls);
            return await SaveAndReturnAsync(invalid, cancellationToken).ConfigureAwait(false);
        }

        var latestSelection = await _selectionReader.ReadLatestAsync(cancellationToken).ConfigureAwait(false);
        if (!latestSelection.Exists || string.IsNullOrWhiteSpace(latestSelection.Selection.SelectionId))
        {
            var invalid = BuildInvalidResult(
                GeneratorPlanStrictLlmEvaluationMode.Batch,
                [Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanStrictLlmEvaluationDiagnosticCodes.MissingCapabilitySelection,
                    "Latest capability selection artifact is missing.",
                    "capability_selection",
                    string.Empty)],
                requestedContracts,
                iterations,
                request.EnableRepairAttempt,
                repairAttempts,
                request.StageValidArtifactsForReview,
                expectedCalls);
            return await SaveAndReturnAsync(invalid, cancellationToken).ConfigureAwait(false);
        }

        var generationResults = new List<GeneratorPlanStrictLlmArtifactGenerationResult>();
        var runs = new List<EvaluationRun>();
        foreach (var contractId in requestedContracts)
        {
            for (var index = 0; index < iterations; index++)
            {
                var generation = await _generationService.GenerateAsync(new GeneratorPlanStrictLlmArtifactGenerationRequest
                {
                    LlmProfileId = request.LlmProfileId,
                    ContractIds = [contractId],
                    UseLatestCapabilitySelection = true,
                    StageForReview = request.StageValidArtifactsForReview,
                    EnableRepairAttempt = request.EnableRepairAttempt,
                    MaxRepairAttempts = repairAttempts,
                    MaxTokens = request.MaxTokens,
                    Temperature = request.Temperature,
                    ExtraUserBrief = request.ExtraUserBrief
                }, cancellationToken).ConfigureAwait(false);

                generationResults.Add(generation);
                runs.Add(new EvaluationRun(contractId, generation));
            }
        }

        var result = BuildEvaluation(
            GeneratorPlanStrictLlmEvaluationMode.Batch,
            latestSelection.Selection.SelectionId,
            requestedContracts,
            iterations,
            request.EnableRepairAttempt,
            repairAttempts,
            request.StageValidArtifactsForReview,
            expectedCalls,
            generationResults,
            runs,
            latestSelection.Selection);

        return await SaveAndReturnAsync(result, cancellationToken).ConfigureAwait(false);
    }

    public static int ExpectedMaxLlmCalls(int contractCount, int iterationsPerContract, bool repairEnabled, int maxRepairAttempts)
    {
        var contracts = Math.Clamp(contractCount, 0, 4);
        var iterations = Math.Clamp(iterationsPerContract, 1, 10);
        var repairs = repairEnabled ? Math.Clamp(maxRepairAttempts, 0, 2) : 0;
        return contracts * iterations * (1 + repairs);
    }

    private async Task<GeneratorPlanStrictLlmEvaluationResult> SaveAndReturnAsync(
        GeneratorPlanStrictLlmEvaluationResult result,
        CancellationToken cancellationToken)
    {
        var withReport = result with { MarkdownReport = _markdownRenderer.Render(result) };
        await _artifactService.SaveAsync(withReport, cancellationToken).ConfigureAwait(false);
        return withReport;
    }

    private static IReadOnlyList<EvaluationRun> BuildRunsFromLatestAudit(GeneratorPlanStrictLlmArtifactGenerationResult result)
    {
        var contracts = result.RequestedContractIds.Count > 0
            ? result.RequestedContractIds
            : result.Attempts.Select(attempt => attempt.ContractId).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        return contracts.Select(contractId => new EvaluationRun(contractId, result)).ToList();
    }

    private IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> ValidateBatchRequest(
        GeneratorPlanStrictLlmEvaluationRequest request,
        IReadOnlyList<string> requestedContracts)
    {
        var diagnostics = new List<GeneratorPlanStrictLlmArtifactDiagnostic>();

        if (string.IsNullOrWhiteSpace(request.LlmProfileId))
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmEvaluationDiagnosticCodes.MissingLlmProfile, "LLM profile id is required for batch evaluation.", "llm_profile_id", string.Empty));
        }

        if (requestedContracts.Count == 0)
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmEvaluationDiagnosticCodes.MissingContracts, "At least one contract id is required.", "contract_ids", string.Empty));
        }

        foreach (var contractId in requestedContracts)
        {
            if (!_contractCatalog.TryGet(contractId, out _))
            {
                diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmEvaluationDiagnosticCodes.UnknownContract, $"Unknown contract id: {contractId}", "contract_ids", contractId));
            }
        }

        return diagnostics;
    }

    private async Task<GeneratorPlanCapabilitySelection?> ReadSelectionOrNullAsync(CancellationToken cancellationToken)
    {
        var latest = await _selectionReader.ReadLatestAsync(cancellationToken).ConfigureAwait(false);
        return latest.Exists && !string.IsNullOrWhiteSpace(latest.Selection.SelectionId)
            ? latest.Selection
            : null;
    }

    private static GeneratorPlanStrictLlmEvaluationResult BuildEvaluation(
        string mode,
        string selectionId,
        IReadOnlyList<string> requestedContracts,
        int iterations,
        bool repairEnabled,
        int maxRepairAttempts,
        bool stageForReview,
        int expectedCalls,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactGenerationResult> generationResults,
        IReadOnlyList<EvaluationRun> runs,
        GeneratorPlanCapabilitySelection? selection)
    {
        var evaluatedAtUtc = DateTimeOffset.UtcNow;
        var artifacts = generationResults.SelectMany(result => result.Artifacts).ToList();
        var attempts = generationResults.SelectMany(result => result.Attempts).ToList();
        var baseDiagnostics = generationResults.SelectMany(result => result.Diagnostics)
            .Concat(generationResults.SelectMany(result => result.Attempts).SelectMany(attempt => attempt.Diagnostics))
            .ToList();
        var qualityDiagnostics = BuildQualityDiagnostics(artifacts, selection);
        var diagnostics = baseDiagnostics
            .Concat(qualityDiagnostics)
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.ContractId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var distinctRequestedContracts = NormalizeContracts(requestedContracts);
        var totalRuns = runs.Count;
        var validArtifacts = artifacts.Where(artifact => artifact.Valid).ToList();
        var initialPassCount = validArtifacts.Count(artifact => !artifact.Repaired);
        var repairPassCount = validArtifacts.Count(artifact => artifact.Repaired);
        var failedCount = Math.Max(0, totalRuns - validArtifacts.Count);
        var initialFailureCount = Math.Max(0, totalRuns - initialPassCount);
        var summary = new GeneratorPlanStrictLlmEvaluationSummary
        {
            TotalContractsRequested = distinctRequestedContracts.Count,
            TotalGenerationRuns = totalRuns,
            TotalAttempts = attempts.Count,
            InitialPassCount = initialPassCount,
            RepairPassCount = repairPassCount,
            FailedCount = failedCount,
            ValidArtifactCount = validArtifacts.Count,
            StagedForReviewCount = generationResults.Sum(result => result.StagingResult?.Snapshot.Items.Count ?? 0),
            MarkdownFenceErrorCount = CountCode(diagnostics, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonMarkdownFence),
            JsonWrapperErrorCount = CountCode(diagnostics, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonTextWrapper),
            JsonInvalidCount = CountCode(diagnostics, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid),
            WrongArtifactKindCount = CountCode(diagnostics, GeneratorPlanStrictLlmArtifactDiagnosticCodes.WrongArtifactKind),
            ForbiddenFieldCount = CountCode(diagnostics, GeneratorPlanStrictLlmArtifactDiagnosticCodes.ForbiddenField),
            InvalidIdCount = CountCode(diagnostics, GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidId),
            MissingFieldCount = CountCode(diagnostics, GeneratorPlanStrictLlmArtifactDiagnosticCodes.MissingField),
            ExpectedMaxLlmCalls = expectedCalls,
            OverallPassRate = totalRuns == 0 ? 0 : Math.Round((double)validArtifacts.Count / totalRuns, 4),
            RepairRecoveryRate = initialFailureCount == 0 ? 0 : Math.Round((double)repairPassCount / initialFailureCount, 4)
        };

        return new GeneratorPlanStrictLlmEvaluationResult
        {
            Ok = true,
            Status = diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
                ? GeneratorPlanStrictLlmEvaluationStatus.EvaluatedWithWarnings
                : GeneratorPlanStrictLlmEvaluationStatus.Evaluated,
            Mode = mode,
            EvaluationId = "strict_llm_evaluation/" + StableHash(mode, selectionId, evaluatedAtUtc.ToString("O"))[..16],
            EvaluatedAtUtc = evaluatedAtUtc,
            SourceCapabilitySelectionId = selectionId,
            RequestedContractIds = distinctRequestedContracts,
            IterationsPerContract = iterations,
            RepairEnabled = repairEnabled,
            MaxRepairAttempts = maxRepairAttempts,
            StageValidArtifactsForReview = stageForReview,
            ExpectedMaxLlmCalls = expectedCalls,
            Summary = summary,
            ContractSummaries = BuildContractSummaries(distinctRequestedContracts, runs, artifacts, attempts, diagnostics),
            DiagnosticSummaries = BuildDiagnosticSummaries(diagnostics),
            Samples = BuildSamples(artifacts, diagnostics),
            Diagnostics = diagnostics
        };
    }

    private static GeneratorPlanStrictLlmEvaluationResult BuildInvalidResult(
        string mode,
        GeneratorPlanStrictLlmArtifactDiagnostic diagnostic)
    {
        return BuildInvalidResult(mode, [diagnostic], Array.Empty<string>(), 1, false, 0, false, 0);
    }

    private static GeneratorPlanStrictLlmEvaluationResult BuildInvalidResult(
        string mode,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics,
        IReadOnlyList<string>? requestedContracts = null,
        int iterations = 1,
        bool repairEnabled = false,
        int maxRepairAttempts = 0,
        bool stageForReview = false,
        int expectedCalls = 0)
    {
        var evaluatedAtUtc = DateTimeOffset.UtcNow;
        var normalizedContracts = NormalizeContracts(requestedContracts ?? Array.Empty<string>());
        return new GeneratorPlanStrictLlmEvaluationResult
        {
            Ok = false,
            Status = GeneratorPlanStrictLlmEvaluationStatus.Invalid,
            Mode = mode,
            EvaluationId = "strict_llm_evaluation/" + StableHash(mode, evaluatedAtUtc.ToString("O"))[..16],
            EvaluatedAtUtc = evaluatedAtUtc,
            RequestedContractIds = normalizedContracts,
            IterationsPerContract = iterations,
            RepairEnabled = repairEnabled,
            MaxRepairAttempts = maxRepairAttempts,
            StageValidArtifactsForReview = stageForReview,
            ExpectedMaxLlmCalls = expectedCalls,
            Summary = new GeneratorPlanStrictLlmEvaluationSummary
            {
                TotalContractsRequested = normalizedContracts.Count,
                ExpectedMaxLlmCalls = expectedCalls
            },
            DiagnosticSummaries = BuildDiagnosticSummaries(diagnostics),
            Diagnostics = diagnostics
        };
    }

    private static IReadOnlyList<GeneratorPlanStrictLlmEvaluationContractSummary> BuildContractSummaries(
        IReadOnlyList<string> requestedContracts,
        IReadOnlyList<EvaluationRun> runs,
        IReadOnlyList<GeneratorPlanStrictLlmGeneratedArtifact> artifacts,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactGenerationAttempt> attempts,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics)
    {
        return requestedContracts
            .Select(contractId =>
            {
                var contractRuns = runs.Where(run => string.Equals(run.ContractId, contractId, StringComparison.OrdinalIgnoreCase)).ToList();
                var contractArtifacts = artifacts.Where(artifact => string.Equals(artifact.ExpectedArtifactContract, contractId, StringComparison.OrdinalIgnoreCase)).ToList();
                var contractAttempts = attempts.Where(attempt => string.Equals(attempt.ContractId, contractId, StringComparison.OrdinalIgnoreCase)).ToList();
                var initialPass = contractArtifacts.Count(artifact => artifact.Valid && !artifact.Repaired);
                var repairPass = contractArtifacts.Count(artifact => artifact.Valid && artifact.Repaired);
                var runCount = contractRuns.Count;
                return new GeneratorPlanStrictLlmEvaluationContractSummary
                {
                    ContractId = contractId,
                    Runs = runCount,
                    InitialPass = initialPass,
                    RepairPass = repairPass,
                    Failed = Math.Max(0, runCount - contractArtifacts.Count(artifact => artifact.Valid)),
                    ValidArtifacts = contractArtifacts.Count(artifact => artifact.Valid),
                    AverageAttempts = runCount == 0 ? 0 : Math.Round((double)contractAttempts.Count / runCount, 2),
                    TopDiagnosticCodes = diagnostics
                        .Where(diagnostic => string.Equals(diagnostic.ContractId, contractId, StringComparison.OrdinalIgnoreCase))
                        .GroupBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                        .OrderByDescending(group => group.Count())
                        .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                        .Take(5)
                        .Select(group => group.Key)
                        .ToList()
                };
            })
            .ToList();
    }

    private static IReadOnlyList<GeneratorPlanStrictLlmEvaluationDiagnosticSummary> BuildDiagnosticSummaries(
        IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics)
    {
        return diagnostics
            .GroupBy(diagnostic => new
            {
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.ContractId,
                diagnostic.Target
            })
            .Select(group => new GeneratorPlanStrictLlmEvaluationDiagnosticSummary
            {
                Severity = group.Key.Severity,
                Code = group.Key.Code,
                ContractId = group.Key.ContractId,
                Target = group.Key.Target,
                Count = group.Count(),
                ExampleMessage = group.First().Message
            })
            .OrderBy(summary => SeverityOrder(summary.Severity))
            .ThenByDescending(summary => summary.Count)
            .ThenBy(summary => summary.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(summary => summary.ContractId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(summary => summary.Target, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<GeneratorPlanStrictLlmEvaluationSample> BuildSamples(
        IReadOnlyList<GeneratorPlanStrictLlmGeneratedArtifact> artifacts,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics)
    {
        return artifacts
            .OrderBy(artifact => artifact.ExpectedArtifactContract, StringComparer.OrdinalIgnoreCase)
            .ThenBy(artifact => artifact.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .Select(artifact => new GeneratorPlanStrictLlmEvaluationSample
            {
                ContractId = artifact.ExpectedArtifactContract,
                ArtifactId = artifact.ArtifactId,
                Valid = artifact.Valid,
                Repaired = artifact.Repaired,
                ContentExcerpt = Excerpt(artifact.ContentJson, 360),
                DiagnosticExcerpt = Excerpt(string.Join("; ", diagnostics
                    .Where(diagnostic => string.Equals(diagnostic.ContractId, artifact.ExpectedArtifactContract, StringComparison.OrdinalIgnoreCase))
                    .Select(diagnostic => diagnostic.Code + ": " + diagnostic.Message)), 240)
            })
            .ToList();
    }

    private static IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> BuildQualityDiagnostics(
        IReadOnlyList<GeneratorPlanStrictLlmGeneratedArtifact> artifacts,
        GeneratorPlanCapabilitySelection? selection)
    {
        var diagnostics = new List<GeneratorPlanStrictLlmArtifactDiagnostic>();
        foreach (var artifact in artifacts.Where(artifact => artifact.Valid))
        {
            AddArtifactQualityDiagnostics(artifact, selection, diagnostics);
        }

        foreach (var group in artifacts
                     .Where(artifact => artifact.Valid)
                     .SelectMany(ExtractTitles)
                     .GroupBy(item => new { item.ContractId, Title = item.Title.Trim().ToUpperInvariant() })
                     .Where(group => group.Count() > 1))
        {
            var title = group.First().Title.Trim();
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Warning,
                GeneratorPlanStrictLlmEvaluationDiagnosticCodes.RepeatedTitleWarning,
                $"Title is repeated across {group.Count()} samples: {title}",
                "title",
                group.Key.ContractId));
        }

        return diagnostics;
    }

    private static void AddArtifactQualityDiagnostics(
        GeneratorPlanStrictLlmGeneratedArtifact artifact,
        GeneratorPlanCapabilitySelection? selection,
        List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics)
    {
        if (!TryParseObject(artifact.ContentJson, out var document))
        {
            return;
        }

        using (document)
        {
            var root = document.RootElement;
            if (!root.TryGetProperty("source_context", out var sourceContext)
                || sourceContext.ValueKind != JsonValueKind.Object
                || !sourceContext.EnumerateObject().Any())
            {
                diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanStrictLlmEvaluationDiagnosticCodes.MissingSourceContextWarning, "source_context is missing or empty.", "source_context", artifact.ExpectedArtifactContract));
            }

            foreach (var text in EnumerateNamedStrings(root, string.Empty))
            {
                if (text.Name is "title" or "name" or "description" && ContainsGenericText(text.Value))
                {
                    diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanStrictLlmEvaluationDiagnosticCodes.GenericTextWarning, $"Generic placeholder text found in {text.Path}.", text.Path, artifact.ExpectedArtifactContract));
                }

                if (text.Name == "description" && text.Value.Trim().Length < 20)
                {
                    diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanStrictLlmEvaluationDiagnosticCodes.ShortDescriptionWarning, $"Description is shorter than 20 characters at {text.Path}.", text.Path, artifact.ExpectedArtifactContract));
                }
            }

            if (artifact.ExpectedArtifactContract == "mechanics_pack_v1" && root.TryGetProperty("mechanics", out var mechanics) && mechanics.ValueKind == JsonValueKind.Array)
            {
                var index = 0;
                foreach (var mechanic in mechanics.EnumerateArray())
                {
                    if (!mechanic.TryGetProperty("tags", out var tags) || tags.ValueKind != JsonValueKind.Array || tags.GetArrayLength() == 0)
                    {
                        diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Warning, GeneratorPlanStrictLlmEvaluationDiagnosticCodes.EmptyTagsWarning, "Mechanic tags are empty.", $"mechanics[{index}].tags", artifact.ExpectedArtifactContract));
                    }

                    index++;
                }
            }

            AddVariantMismatchDiagnostics(artifact, root, selection, diagnostics);
        }
    }

    private static void AddVariantMismatchDiagnostics(
        GeneratorPlanStrictLlmGeneratedArtifact artifact,
        JsonElement root,
        GeneratorPlanCapabilitySelection? selection,
        List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics)
    {
        if (selection == null || artifact.ExpectedArtifactContract != "game_profile_v1" || !root.TryGetProperty("game", out var game))
        {
            return;
        }

        AddMismatch("presentation_mode", selection.SelectedVariantIds.PresentationModeId);
        AddMismatch("world_topology", selection.SelectedVariantIds.WorldTopologyId);
        AddMismatch("actor_model", selection.SelectedVariantIds.ActorModelId);
        AddMismatch("combat_model", selection.SelectedVariantIds.CombatModelId);

        void AddMismatch(string field, string expected)
        {
            if (string.IsNullOrWhiteSpace(expected)
                || !game.TryGetProperty(field, out var property)
                || property.ValueKind != JsonValueKind.String)
            {
                return;
            }

            var actual = property.GetString() ?? string.Empty;
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    GeneratorPlanStrictLlmEvaluationDiagnosticCodes.VariantMismatchWarning,
                    $"{field} differs from latest capability selection. Expected '{expected}', got '{actual}'.",
                    "game." + field,
                    artifact.ExpectedArtifactContract));
            }
        }
    }

    private static IEnumerable<(string ContractId, string Title)> ExtractTitles(GeneratorPlanStrictLlmGeneratedArtifact artifact)
    {
        if (!TryParseObject(artifact.ContentJson, out var document))
        {
            yield break;
        }

        using (document)
        {
            foreach (var text in EnumerateNamedStrings(document.RootElement, string.Empty))
            {
                if (text.Name == "title" && !string.IsNullOrWhiteSpace(text.Value))
                {
                    yield return (artifact.ExpectedArtifactContract, text.Value);
                }
            }
        }
    }

    private static IEnumerable<(string Name, string Path, string Value)> EnumerateNamedStrings(JsonElement element, string path)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var nextPath = string.IsNullOrWhiteSpace(path) ? property.Name : path + "." + property.Name;
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    yield return (property.Name, nextPath, property.Value.GetString() ?? string.Empty);
                }

                foreach (var child in EnumerateNamedStrings(property.Value, nextPath))
                {
                    yield return child;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
            {
                foreach (var child in EnumerateNamedStrings(item, $"{path}[{index}]"))
                {
                    yield return child;
                }

                index++;
            }
        }
    }

    private static bool TryParseObject(string json, out JsonDocument document)
    {
        document = null!;
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool ContainsGenericText(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized.Contains("...", StringComparison.Ordinal)
            || normalized.Contains("tbd", StringComparison.Ordinal)
            || normalized == "test"
            || normalized.Contains(" test ", StringComparison.Ordinal)
            || normalized == "sample"
            || normalized.Contains("sample", StringComparison.Ordinal)
            || normalized == "example"
            || normalized.Contains("example", StringComparison.Ordinal);
    }

    private static int CountCode(IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics, string code)
    {
        return diagnostics.Count(diagnostic => string.Equals(diagnostic.Code, code, StringComparison.OrdinalIgnoreCase));
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

    private static string Excerpt(string text, int maxLength)
    {
        var normalized = (text ?? string.Empty)
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength] + "...";
    }

    private sealed record EvaluationRun(string ContractId, GeneratorPlanStrictLlmArtifactGenerationResult Result);
}
