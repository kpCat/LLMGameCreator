using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.Semantics;

public sealed class SemanticSelectedRuntimeCompositionAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/semantic-runtime-composition";
    public const string ReportJsonFileName = "semantic-runtime-composition-report.json";
    public const string ReportMarkdownFileName = "semantic-runtime-composition-report.md";
    public const string VerificationMarkdownFileName = "semantic-runtime-composition-verification.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly SemanticGuidedCompositionAcceptanceService _semanticGuidedService;
    private readonly QuestDialogInteractionRulePackValidator _rulePackValidator;
    private readonly IGamePackageValidator _packageValidator;
    private readonly ISemanticSelectedRuntimeCompositionRuntimeAdapter _runtimeAdapter;
    private readonly Func<GamePackageDefinition, SemanticSelectedCompositionPlan, GamePackageDefinition> _packageMutator;

    public SemanticSelectedRuntimeCompositionAcceptanceService(
        SemanticGuidedCompositionAcceptanceService? semanticGuidedService = null,
        QuestDialogInteractionRulePackValidator? rulePackValidator = null,
        IGamePackageValidator? packageValidator = null,
        ISemanticSelectedRuntimeCompositionRuntimeAdapter? runtimeAdapter = null,
        Func<GamePackageDefinition, SemanticSelectedCompositionPlan, GamePackageDefinition>? packageMutator = null)
    {
        _semanticGuidedService = semanticGuidedService ?? new SemanticGuidedCompositionAcceptanceService();
        _rulePackValidator = rulePackValidator ?? new QuestDialogInteractionRulePackValidator();
        _packageValidator = packageValidator ?? new GamePackageValidator();
        _runtimeAdapter = runtimeAdapter ?? new RuntimeAdapterUnavailable();
        _packageMutator = packageMutator ?? ((package, _) => package);
    }

    public SemanticSelectedRuntimeCompositionAcceptanceResult Build(
        string? projectRootPath = null,
        string? referencePackRootPath = null)
    {
        var semanticGuided = _semanticGuidedService.Build(projectRootPath, referencePackRootPath);
        var proofPack = QuestDialogInteractionFamilyAcceptanceService.BuildProofPack();
        var proofValidation = _rulePackValidator.Validate(proofPack);
        var scenarios = BuildRequiredScenarios(semanticGuided.Report.Scenarios, proofPack, proofValidation).ToList();
        var repeatedFirst = BuildRequiredScenario(
            semanticGuided.Report.Scenarios.First(item => item.ScenarioId == "core_plus_wildland_frontier"),
            proofPack,
            proofValidation);

        var validScenarios = scenarios.Where(item => item.ExpectedValid).ToList();
        var isolationDiagnostics = ValidateCrossVariantIsolation(validScenarios);
        var invalidScenario = scenarios.Single(item => item.ScenarioId == "invalid_conflict_rejection");
        var deterministicReplayPassed =
            validScenarios.Count > 0 &&
            validScenarios[0].CompositionPlanHash == repeatedFirst.CompositionPlanHash &&
            validScenarios[0].GeneratedPackageHash == repeatedFirst.GeneratedPackageHash &&
            validScenarios[0].RuntimeEvidence.RuntimeEvidenceHash == repeatedFirst.RuntimeEvidence.RuntimeEvidenceHash;
        var validPackageClean = validScenarios.All(item => item.PackageValidationPassed);
        var validRuntimeExecuted = validScenarios.All(item => item.SemanticSelectedIdsExecutedInRuntime);
        var invalidRejected = !invalidScenario.ActualValid &&
                              invalidScenario.Diagnostics.Any(item => item.Severity == "error") &&
                              invalidScenario.Diagnostics.Any(item => item.Code == "semantic_guided.excludes_conflict") &&
                              !invalidScenario.RuntimeEvidence.RuntimeAttempted &&
                              string.IsNullOrWhiteSpace(invalidScenario.GeneratedPackageHash);
        var saveLoadPassed = validScenarios.All(item => item.RuntimeEvidence.SaveLoadRoundtripPassed);
        var noLeakage = validScenarios.All(item => !item.CandidateOrConflictLeakageDetected);
        var variantIsolationPassed = HasDistinctValues(validScenarios.Select(item => item.CompositionPlanHash)) &&
                                     HasDistinctValues(validScenarios.Select(item => item.GeneratedPackageHash)) &&
                                     HasDistinctValues(validScenarios.Select(item => item.RuntimeEvidence.RuntimeStateHash)) &&
                                     isolationDiagnostics.All(item => item.Severity != "error");
        var multiSeedPassed = semanticGuided.Report.MultiSeedChecks.All(item => item.NoDanglingSemanticReferences);
        var goal004Goal005RegressionsPassed =
            semanticGuided.Report.Goal004RuntimeEvidencePreserved &&
            semanticGuided.Report.ExpectedValidScenariosAccepted &&
            semanticGuided.Report.ExpectedInvalidScenariosRejectedByErrors;

        var diagnostics = new List<SemanticSelectedRuntimeCompositionDiagnostic>
        {
            Diagnostic("info", "semantic_runtime.goal005_gate_recorded", "semantic_guided_composition_artifact_verification", "User-confirmed Goal 005 semantic-guided composition artifact verification is recorded as passed."),
            Diagnostic("info", "semantic_runtime.no_external_execution", "harness", "No LLM, RAG, provider, Lua, Unity or media execution was invoked."),
            Diagnostic(validPackageClean ? "info" : "error", validPackageClean ? "semantic_runtime.packages_validator_clean" : "semantic_runtime.package_validation_failed", "package", "All valid semantic-selected packages must pass package validation."),
            Diagnostic(validRuntimeExecuted ? "info" : "error", validRuntimeExecuted ? "semantic_runtime.selected_ids_executed" : "semantic_runtime.selected_ids_not_executed", "runtime", "Runtime evidence must reference the same selected ids and package hash."),
            Diagnostic(invalidRejected ? "info" : "error", invalidRejected ? "semantic_runtime.invalid_rejected" : "semantic_runtime.invalid_not_rejected", invalidScenario.ScenarioId, "Invalid semantic/rule binding must be rejected before a runnable package is created."),
            Diagnostic(deterministicReplayPassed ? "info" : "error", deterministicReplayPassed ? "semantic_runtime.replay_stable" : "semantic_runtime.replay_unstable", "core_plus_wildland_frontier", "Repeated composition, package and runtime evidence hashes must match."),
            Diagnostic(saveLoadPassed ? "info" : "error", saveLoadPassed ? "semantic_runtime.save_load_roundtrip_passed" : "semantic_runtime.save_load_roundtrip_failed", "runtime_state", "Runtime session/state serialization roundtrip must preserve selected evidence."),
            Diagnostic(variantIsolationPassed ? "info" : "error", variantIsolationPassed ? "semantic_runtime.variant_isolation_passed" : "semantic_runtime.variant_isolation_failed", "variant_matrix", "Semantic variants must not leak selected ids, package hashes or runtime state hashes."),
            Diagnostic(multiSeedPassed ? "info" : "error", multiSeedPassed ? "semantic_runtime.multi_seed_no_dangling_refs" : "semantic_runtime.multi_seed_dangling_refs", "multi_seed_matrix", "Bounded multi-seed semantic reference check must have no dangling refs."),
            Diagnostic(goal004Goal005RegressionsPassed ? "info" : "error", goal004Goal005RegressionsPassed ? "semantic_runtime.goal004_goal005_regressions_preserved" : "semantic_runtime.goal004_goal005_regression", "regression", "Goal 004 and Goal 005 semantic-guided regression evidence remains preserved.")
        };
        diagnostics.AddRange(scenarios.SelectMany(item => item.Diagnostics));
        diagnostics.AddRange(isolationDiagnostics);

        var accepted = semanticGuided.Report.Accepted &&
                       !proofValidation.HasErrors &&
                       validScenarios.Count == 4 &&
                       validPackageClean &&
                       validRuntimeExecuted &&
                       invalidRejected &&
                       deterministicReplayPassed &&
                       saveLoadPassed &&
                       variantIsolationPassed &&
                       noLeakage &&
                       multiSeedPassed &&
                       goal004Goal005RegressionsPassed;

        var reportWithoutHash = new SemanticSelectedRuntimeCompositionReport
        {
            Accepted = accepted,
            ManualGate = "semantic_selected_runtime_composition_artifact_verification",
            Goal005GateRecorded = true,
            Goal005SourceReportHash = semanticGuided.Report.DeterministicHash,
            ProofRulePackId = proofPack.Metadata.RulePackId,
            ProofRulePackHasErrors = proofValidation.HasErrors,
            ScenarioCount = scenarios.Count,
            ValidScenarioCount = validScenarios.Count,
            InvalidScenarioRejected = invalidRejected,
            SemanticSelectedIdsExecutedInRuntime = validRuntimeExecuted,
            DeterministicReplayPassed = deterministicReplayPassed,
            SaveLoadRoundtripPassed = saveLoadPassed,
            CrossVariantIsolationPassed = variantIsolationPassed,
            CandidateConflictLeakageDetected = !noLeakage,
            MultiSeedNoDanglingReferences = multiSeedPassed,
            PackageValidationPassed = validPackageClean,
            Goal004Goal005RegressionPassed = goal004Goal005RegressionsPassed,
            ExternalExecution = new SemanticSelectedRuntimeCompositionExternalExecutionFlags(),
            Scenarios = scenarios,
            Diagnostics = SortDiagnostics(diagnostics),
            RemainingCSharpPrimitiveLimits =
            [
                "new runtime command families",
                "new mutable runtime state containers",
                "new formula evaluator semantics",
                "new rendering or UI interaction modes",
                "new external providers or Lua execution",
                "regional world navigation and richer encounter execution"
            ]
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new SemanticSelectedRuntimeCompositionAcceptanceResult
        {
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<SemanticSelectedRuntimeCompositionWriteResult> WriteAsync(
        string projectRootPath,
        SemanticSelectedRuntimeCompositionAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "semantic-runtime-composition"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportJsonFileName));
        var markdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportMarkdownFileName));
        var verificationPath = Path.GetFullPath(Path.Combine(outputDirectory, VerificationMarkdownFileName));
        EnsureContained(outputDirectory, jsonPath);
        EnsureContained(outputDirectory, markdownPath);
        EnsureContained(outputDirectory, verificationPath);

        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new SemanticSelectedRuntimeCompositionWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<SemanticSelectedRuntimeCompositionWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        string? referencePackRootPath = null,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath, referencePackRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private IEnumerable<SemanticSelectedRuntimeCompositionScenario> BuildRequiredScenarios(
        IReadOnlyList<SemanticGuidedCompositionScenario> semanticScenarios,
        QuestDialogInteractionRulePack proofPack,
        QuestDialogInteractionRulePackValidationReport proofValidation)
    {
        var requiredIds = new[]
        {
            "core_plus_wildland_frontier",
            "core_plus_gothic_mystery",
            "core_plus_trade_caravan",
            "core_genre_project_overlay",
            "invalid_conflict_rejection"
        };

        foreach (var scenarioId in requiredIds)
        {
            var source = semanticScenarios.First(item => item.ScenarioId == scenarioId);
            yield return BuildRequiredScenario(source, proofPack, proofValidation);
        }
    }

    private SemanticSelectedRuntimeCompositionScenario BuildRequiredScenario(
        SemanticGuidedCompositionScenario source,
        QuestDialogInteractionRulePack proofPack,
        QuestDialogInteractionRulePackValidationReport proofValidation)
    {
        var diagnostics = new List<SemanticSelectedRuntimeCompositionDiagnostic>();
        var selectedQuest = proofPack.QuestPatterns.FirstOrDefault(item => item.PatternId == source.SelectedQuestPatternId);
        var selectedDialogue = proofPack.DialogueIntents.FirstOrDefault(item => item.IntentId == source.SelectedDialogueIntentId);
        var selectedInteraction = proofPack.InteractionPatterns.FirstOrDefault(item => item.InteractionId == source.SelectedInteractionPatternId);

        if (proofValidation.HasErrors)
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.rule_pack_invalid", proofValidation.RulePackId, "Goal 004 proof rule pack validation has errors."));
        }

        if (selectedQuest == null)
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.quest_declaration_missing", source.SelectedQuestPatternId, "Selected quest pattern id was not found in the validated rule pack."));
        }

        if (selectedDialogue == null)
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.dialogue_declaration_missing", source.SelectedDialogueIntentId, "Selected dialogue intent id was not found in the validated rule pack."));
        }

        if (selectedInteraction == null)
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.interaction_declaration_missing", source.SelectedInteractionPatternId, "Selected interaction pattern id was not found in the validated rule pack."));
        }

        diagnostics.AddRange(source.Diagnostics
            .Where(item => item.Severity == SemanticDiagnosticSeverity.Error)
            .Select(item => Diagnostic("error", item.Code, item.Target, item.Message)));

        var plan = selectedQuest == null || selectedDialogue == null || selectedInteraction == null
            ? new SemanticSelectedCompositionPlan()
            : BuildPlan(source, selectedQuest, selectedDialogue, selectedInteraction, proofPack, diagnostics);
        var candidateOrConflictLeakage = source.CandidateLeakageDetected ||
                                         source.Diagnostics.Any(item => item.Severity == SemanticDiagnosticSeverity.Error);
        var semanticValid = source.Accepted &&
                            source.ExpectedValid &&
                            !candidateOrConflictLeakage &&
                            diagnostics.All(item => item.Severity != "error");
        var actualValid = semanticValid;
        GamePackageDefinition? package = null;
        string packageJson = string.Empty;
        string packageHash = string.Empty;
        var packageValidationPassed = false;
        var validationIssues = new List<SemanticSelectedPackageValidationIssue>();
        var packageAuditPassed = false;
        SemanticSelectedRuntimeCompositionRuntimeEvidence runtimeEvidence;

        if (semanticValid)
        {
            package = _packageMutator(BuildPackage(plan), plan);
            packageJson = JsonSerializer.Serialize(package, JsonOptions);
            packageHash = ComputeHash(packageJson);
            var validation = _packageValidator.Validate(package);
            packageValidationPassed = validation.IsValid;
            validationIssues = validation.Issues
                .OrderBy(item => item.Severity)
                .ThenBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.TargetId, StringComparer.Ordinal)
                .Select(ToValidationIssue)
                .ToList();
            foreach (var issue in validationIssues.Where(item => item.Severity is nameof(ValidationSeverity.Error) or nameof(ValidationSeverity.Critical)))
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.package_validation_failed", issue.TargetId, issue.Code + ": " + issue.Message));
            }

            var packageAuditDiagnostics = AuditMaterializedPackage(package, plan);
            packageAuditPassed = packageAuditDiagnostics.All(item => item.Severity != "error");
            diagnostics.AddRange(packageAuditDiagnostics);
            actualValid = semanticValid && packageValidationPassed && packageAuditPassed;

            if (packageValidationPassed && packageAuditPassed)
            {
                runtimeEvidence = _runtimeAdapter.Run(new SemanticSelectedRuntimeCompositionRuntimeRequest
                {
                    ScenarioId = source.ScenarioId,
                    Plan = plan,
                    Package = package,
                    PackageHash = packageHash,
                    PackageJson = packageJson
                });
                var runtimeDiagnostics = ValidateRuntimeEvidence(source.ScenarioId, plan, package, packageHash, runtimeEvidence);
                diagnostics.AddRange(runtimeDiagnostics);
                if (runtimeDiagnostics.Any(item => item.Severity == "error"))
                {
                    runtimeEvidence = runtimeEvidence with
                    {
                        SemanticSelectedIdsExecutedInRuntime = false,
                        Diagnostics = SortDiagnostics(runtimeEvidence.Diagnostics.Concat(runtimeDiagnostics))
                    };
                }
            }
            else
            {
                runtimeEvidence = new SemanticSelectedRuntimeCompositionRuntimeEvidence
                {
                    RuntimeAttempted = false,
                    PackageHash = packageHash,
                    Diagnostics =
                    [
                        Diagnostic("error", "semantic_runtime.runtime_blocked_by_materialized_audit", source.ScenarioId, "Package validation or materialized-binding audit failed before runtime execution.")
                    ]
                };
            }
            diagnostics.AddRange(runtimeEvidence.Diagnostics);
        }
        else
        {
            runtimeEvidence = new SemanticSelectedRuntimeCompositionRuntimeEvidence
            {
                RuntimeAttempted = false,
                Diagnostics =
                [
                    Diagnostic("info", "semantic_runtime.invalid_not_runnable", source.ScenarioId, "Invalid semantic/rule binding was rejected before package materialization.")
                ]
            };
            diagnostics.AddRange(runtimeEvidence.Diagnostics);
        }

        var selectedIdsExecuted = actualValid &&
                                  packageValidationPassed &&
                                  packageAuditPassed &&
                                  runtimeEvidence.SemanticSelectedIdsExecutedInRuntime &&
                                  runtimeEvidence.ExecutedQuestPatternId == plan.SelectedQuestPatternId &&
                                  runtimeEvidence.ExecutedDialogueIntentId == plan.SelectedDialogueIntentId &&
                                  runtimeEvidence.ExecutedInteractionPatternId == plan.SelectedInteractionPatternId &&
                                  runtimeEvidence.PackageHash == packageHash;

        return new SemanticSelectedRuntimeCompositionScenario
        {
            ScenarioId = source.ScenarioId,
            Seed = source.Seed,
            ExpectedValid = source.ExpectedValid,
            ActualValid = actualValid,
            InputLayerIds = source.InputLayerIds,
            InputLayerHashes = source.InputLayerHashes,
            CompiledCatalogHash = source.CompiledCatalogHash,
            SemanticTrace = source.Trace.Select(item => new SemanticSelectedTraceLink
            {
                RelationId = item.RelationId,
                RelationKind = item.RelationKind,
                SourceTermId = item.SourceTermId,
                TargetId = item.TargetId,
                LayerIds = item.LayerIds
            }).ToList(),
            CandidateOrConflictLeakageDetected = candidateOrConflictLeakage,
            SelectedQuestPatternId = source.SelectedQuestPatternId,
            SelectedDialogueIntentId = source.SelectedDialogueIntentId,
            SelectedInteractionPatternId = source.SelectedInteractionPatternId,
            CompositionPlan = plan,
            CompositionPlanHash = plan.PlanHash,
            GeneratedPackageId = package?.Manifest.PackageId ?? string.Empty,
            GeneratedPackageHash = packageHash,
            GeneratedPackageJsonHash = string.IsNullOrWhiteSpace(packageJson) ? string.Empty : ComputeHash(packageJson),
            PackageValidationPassed = packageValidationPassed,
            ValidationIssues = validationIssues,
            RuntimeEvidence = runtimeEvidence,
            SemanticSelectedIdsExecutedInRuntime = selectedIdsExecuted,
            TraceChain = BuildTraceChain(source, plan, packageHash, runtimeEvidence),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static SemanticSelectedCompositionPlan BuildPlan(
        SemanticGuidedCompositionScenario source,
        QuestPatternDefinition quest,
        DialogueIntentPatternDefinition dialogue,
        InteractionPatternDefinition interaction,
        QuestDialogInteractionRulePack proofPack,
        ICollection<SemanticSelectedRuntimeCompositionDiagnostic> diagnostics)
    {
        foreach (var targetRef in quest.Objectives.Select(item => item.TargetRef)
                     .Concat(proofPack.InteractionPatterns.Select(item => item.TargetRef))
                     .Concat(proofPack.InteractionPatterns
                         .Where(item => !string.IsNullOrWhiteSpace(item.RequiredItemRef))
                         .Select(item => item.RequiredItemRef))
                     .OrderBy(item => item, StringComparer.Ordinal))
        {
            if (!CanResolvePackageTargetId(targetRef))
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.content_binding_missing", targetRef, "Selected declaration target ref cannot be represented by generated package content."));
            }
        }

        var packageQuestId = PackageQuestId(source.ScenarioId);
        var packageDialogueId = PackageDialogueId(source.ScenarioId);
        var packageNpcId = "entity/generated_contact";
        var packageEncounterId = "encounter/generated_challenge";
        var requiredInteractionIds = quest.Objectives
            .SelectMany(item => item.RequiredInteractionPatternIds)
            .Append(interaction.InteractionId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var interactionBindings = new List<SemanticSelectedInteractionBindingPlan>();
        foreach (var interactionId in requiredInteractionIds)
        {
            var declaration = proofPack.InteractionPatterns.FirstOrDefault(item => item.InteractionId == interactionId);
            if (declaration == null)
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.required_interaction_declaration_missing", interactionId, "Selected quest objective references an interaction declaration that is absent from the validated rule pack."));
                continue;
            }

            interactionBindings.Add(new SemanticSelectedInteractionBindingPlan
            {
                InteractionPatternId = declaration.InteractionId,
                Family = declaration.Family,
                TargetRef = declaration.TargetRef,
                ResultActionId = declaration.ResultActionId,
                RequiredItemRef = declaration.RequiredItemRef,
                PackageInteractionId = declaration.InteractionId,
                PackageTargetId = ResolvePackageTargetId(declaration.TargetRef, packageQuestId, packageDialogueId, packageNpcId, packageEncounterId),
                PackageRequiredItemId = string.IsNullOrWhiteSpace(declaration.RequiredItemRef)
                    ? string.Empty
                    : ResolvePackageTargetId(declaration.RequiredItemRef, packageQuestId, packageDialogueId, packageNpcId, packageEncounterId),
                CorrelatedObjectiveIds = quest.Objectives
                    .Where(objective => objective.RequiredInteractionPatternIds.Contains(declaration.InteractionId, StringComparer.Ordinal))
                    .Select(objective => PackageObjectiveId(objective.ObjectiveId))
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToList()
            });
        }

        var planWithoutHash = new SemanticSelectedCompositionPlan
        {
            PlanId = "semantic_runtime_plan/" + source.ScenarioId,
            Seed = source.Seed,
            InputLayerIds = source.InputLayerIds,
            InputLayerHashes = source.InputLayerHashes,
            CompiledCatalogHash = source.CompiledCatalogHash,
            SelectedSemanticTermIds = source.SelectedSemanticTermIds,
            Trace = source.Trace.Select(item => new SemanticSelectedTraceLink
            {
                RelationId = item.RelationId,
                RelationKind = item.RelationKind,
                SourceTermId = item.SourceTermId,
                TargetId = item.TargetId,
                LayerIds = item.LayerIds
            }).ToList(),
            SelectedQuestPatternId = quest.PatternId,
            SelectedQuestPatternType = quest.PatternType,
            SelectedQuestObjectives = quest.Objectives.Select(item => new SemanticSelectedQuestObjectivePlan
            {
                ObjectiveId = item.ObjectiveId,
                ObjectiveKind = item.ObjectiveKind,
                TargetRef = item.TargetRef,
                PackageObjectiveId = PackageObjectiveId(item.ObjectiveId),
                PackageTargetId = ResolvePackageTargetId(item.TargetRef, packageQuestId, packageDialogueId, packageNpcId, packageEncounterId),
                RequiredInteractionPatternIds = item.RequiredInteractionPatternIds
            }).ToList(),
            SelectedDialogueIntentId = dialogue.IntentId,
            SelectedDialogueIntentType = dialogue.IntentType,
            SelectedDialogueTemplateId = dialogue.IntentId,
            BoundSemanticSlots = BuildSlots(source),
            SelectedInteractionPatternId = interaction.InteractionId,
            SelectedInteractionFamily = interaction.Family,
            SelectedInteractionTargetRef = interaction.TargetRef,
            SelectedInteractionResultActionId = interaction.ResultActionId,
            SelectedInteractionRequiredItemRef = interaction.RequiredItemRef,
            MaterializedInteractions = interactionBindings,
            PackageQuestId = packageQuestId,
            PackageDialogueId = packageDialogueId,
            PackageInteractionId = interaction.InteractionId,
            PackageNpcId = packageNpcId,
            PackageItemIds = ["item/generated_cache", "item/generated_reward", "item/generated_recovered_item"],
            PackageEncounterId = packageEncounterId,
            PackageMapId = "map/semantic_runtime_start",
            Diagnostics = diagnostics.OrderBy(item => item.Code, StringComparer.Ordinal).ThenBy(item => item.Target, StringComparer.Ordinal).ToList(),
            Provenance = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceScenarioId"] = source.ScenarioId,
                ["compiledCatalogHash"] = source.CompiledCatalogHash,
                ["selectedQuestPatternId"] = quest.PatternId,
                ["selectedDialogueIntentId"] = dialogue.IntentId,
                ["selectedInteractionPatternId"] = interaction.InteractionId
            }
        };

        return planWithoutHash with
        {
            PlanHash = ComputeHash(JsonSerializer.Serialize(planWithoutHash, JsonOptions))
        };
    }

    private static GamePackageDefinition BuildPackage(SemanticSelectedCompositionPlan plan)
    {
        var quest = BuildQuest(plan);
        var dialogue = BuildDialogue(plan);
        var interactions = plan.MaterializedInteractions.Select(item => BuildInteraction(plan, item)).ToList();
        var package = new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = "game/semantic_runtime_" + ShortHash(plan.PlanHash),
                Title = "Semantic Runtime " + plan.ScenarioLabel,
                Version = "0.1.0",
                FormatVersion = "0.1",
                StartMapId = plan.PackageMapId,
                Description = "Deterministic semantic-selected runtime composition package."
            },
            Game = new GameDefinition
            {
                TilePrototypes =
                [
                    new TilePrototypeDefinition { Id = "tile/semantic_floor", Name = "Semantic Floor", Walkable = true, MovementCost = 1 },
                    new TilePrototypeDefinition { Id = "tile/semantic_blocker", Name = "Semantic Blocker", Walkable = false, MovementCost = 1 }
                ],
                EntityPrototypes =
                [
                    new EntityPrototypeDefinition
                    {
                        Id = "entity_prototype/generated_contact",
                        Name = "Generated Contact",
                        Components =
                        [
                            new ComponentDefinition
                            {
                                Type = "interactable",
                                Args = SortedArgs(("dialogueId", plan.PackageDialogueId), ("interactionId", plan.PackageInteractionId))
                            }
                        ]
                    }
                ],
                Maps =
                [
                    new MapDefinition
                    {
                        Id = plan.PackageMapId,
                        Name = "Semantic Runtime Start",
                        Width = 5,
                        Height = 5,
                        DefaultTileId = "tile/semantic_floor",
                        StartPosition = new Position2D(1, 1),
                        Tiles = [new TileOverrideDefinition { X = 4, Y = 4, TileId = "tile/semantic_blocker" }],
                        Entities =
                        [
                            new EntityInstanceDefinition
                            {
                                Id = plan.PackageNpcId,
                                PrototypeId = "entity_prototype/generated_contact",
                                Position = new Position2D(3, 1)
                            }
                        ]
                    }
                ],
                Items =
                [
                    Item("item/generated_cache", "Generated Cache", plan),
                    Item("item/generated_reward", "Generated Reward", plan, useFlagId: FlagId(plan)),
                    Item("item/generated_recovered_item", "Generated Recovered Item", plan)
                ],
                Inventories =
                [
                    new InventoryDefinition
                    {
                        Id = "inventory/player",
                        OwnerKind = "player",
                        OwnerId = "player",
                        Slots = 8,
                        Stacks =
                        [
                            new ItemStackDefinition { ItemId = "item/generated_reward", Amount = 1, QuestItem = true }
                        ],
                        Metadata = Metadata(plan, ("semanticRuntimeRole", "runtime_start_inventory"))
                    }
                ],
                Encounters =
                [
                    new EncounterDefinition
                    {
                        Id = plan.PackageEncounterId,
                        Name = "Generated Challenge",
                        Kind = "semantic_selected_challenge",
                        Participants =
                        [
                            new EncounterParticipantDefinition
                            {
                                Id = "participant/player",
                                Name = "Player",
                                Kind = "player",
                                Team = "player"
                            },
                            new EncounterParticipantDefinition
                            {
                                Id = "participant/generated_challenge",
                                Name = "Generated Challenge",
                                Kind = "generated_challenge",
                                Team = "challenge"
                            }
                        ],
                        Rewards = [new OutputDefinition { Kind = "item", Id = "item/generated_recovered_item", Amount = 1 }],
                        Tags = ["semantic_selected"],
                        Metadata = Metadata(plan, ("selectedRuleInteractionId", plan.SelectedInteractionPatternId))
                    }
                ],
                Quests = [quest],
                Dialogues = [dialogue],
                Interactions = interactions,
                Factions =
                [
                    new FactionDefinition
                    {
                        Id = "faction/generated_contact",
                        Name = "Generated Contact",
                        Description = "Semantic-selected contact faction.",
                        Kind = "generated_faction",
                        DefaultReputation = 0,
                        MinReputation = -100,
                        MaxReputation = 100,
                        Metadata = Metadata(plan)
                    }
                ]
            },
            GeneratedContent = new GeneratedContentDefinition
            {
                Profile = new GeneratedGameProfileDefinition
                {
                    Title = "Semantic Runtime " + plan.ScenarioLabel,
                    Description = "Semantic-selected composition package profile.",
                    Genre = string.Join(",", plan.InputLayerIds.Where(item => item.StartsWith("genre/", StringComparison.Ordinal)).OrderBy(item => item, StringComparer.Ordinal)),
                    Tone = string.Join(",", plan.BoundSemanticSlots.Where(item => item.Key.Contains("tone", StringComparison.Ordinal)).Select(item => item.Value).OrderBy(item => item, StringComparer.Ordinal)),
                    PresentationMode = "presentation_mode/top_down_2d",
                    WorldTopology = "world_topology/single_map",
                    ActorModel = "actor_model/single_player_character",
                    CombatModel = "combat_model/runtime_primitives",
                    CoreLoop = ["move", "interact", "dialogue", "quest_progress", "reward"],
                    Pillars = ["semantic_selected_declarations", "validator_clean_package", "headless_runtime_evidence"],
                    SourceContextJson = JsonSerializer.Serialize(plan.Provenance, JsonOptions)
                },
                Scenes =
                [
                    new GeneratedSceneDefinition
                    {
                        SourceId = plan.PackageMapId,
                        PackageMapId = plan.PackageMapId,
                        Title = "Semantic Runtime Start",
                        Description = "Start scene for semantic-selected runtime composition.",
                        Purpose = "semantic_runtime_composition"
                    }
                ],
                Regions =
                [
                    new GeneratedRegionDefinition
                    {
                        SourceId = "region/semantic_runtime",
                        Title = "Semantic Runtime Region",
                        Description = "Region derived from selected semantic layers.",
                        SceneIds = [plan.PackageMapId]
                    }
                ],
                Npcs =
                [
                    new GeneratedNpcDefinition
                    {
                        SourceId = plan.PackageNpcId,
                        Name = "Generated Contact",
                        Description = "NPC bound to selected dialogue and interaction declarations.",
                        RegionId = "region/semantic_runtime",
                        SceneId = plan.PackageMapId
                    }
                ],
                Items = plan.PackageItemIds.Select(item => new GeneratedItemDefinition
                {
                    SourceId = item,
                    Name = TitleFromId(item),
                    Description = "Item bound by semantic-selected composition."
                }).ToList(),
                Dialogues =
                [
                    new GeneratedDialogueDefinition
                    {
                        SourceId = plan.SelectedDialogueIntentId,
                        Title = dialogue.Title,
                        Description = "Dialogue generated from selected semantic intent.",
                        NpcId = plan.PackageNpcId,
                        SceneId = plan.PackageMapId,
                        Lines = dialogue.Nodes.Select(item => item.Text).ToList()
                    }
                ],
                Encounters =
                [
                    new GeneratedEncounterDefinition
                    {
                        SourceId = plan.PackageEncounterId,
                        Title = "Generated Challenge",
                        Description = "Encounter target for semantic-selected interaction/runtime evidence.",
                        RegionId = "region/semantic_runtime",
                        SceneId = plan.PackageMapId,
                        NpcIds = [plan.PackageNpcId]
                    }
                ],
                Quests =
                [
                    new GeneratedQuestSeedDefinition
                    {
                        SourceId = plan.SelectedQuestPatternId,
                        PackageQuestId = plan.PackageQuestId,
                        Title = quest.Title,
                        Description = quest.Description,
                        Steps = plan.SelectedQuestObjectives.Select(item => item.ObjectiveKind).ToList(),
                        Objectives = plan.SelectedQuestObjectives.Select(item => item.PackageTargetId).ToList()
                    }
                ],
                Mechanics = plan.MaterializedInteractions.Select(item => new GeneratedMechanicDefinition
                    {
                        SourceId = item.InteractionPatternId,
                        PackageAbilityId = item.PackageInteractionId,
                        Name = TitleFromId(item.InteractionPatternId),
                        Description = "Selected or objective-required interaction pattern represented by existing runtime primitives.",
                        Tags = [item.Family, item.ResultActionId]
                    }).ToList(),
                AppliedArtifacts =
                [
                    new GeneratedContentArtifactProvenance
                    {
                        ArtifactId = "semantic_runtime_composition/" + ShortHash(plan.PlanHash),
                        ContractId = "semantic_selected_runtime_composition_v1",
                        ArtifactKind = "semantic_runtime_composition",
                        CapabilitySelectionId = "semantic_runtime_composition",
                        GeneratedAt = string.Empty,
                        AuditId = plan.PlanId,
                        AppliedAt = string.Empty,
                        ContentHash = plan.PlanHash,
                        MappingResult = "semantic_selected_declarations_materialized_to_existing_package_contracts"
                    }
                ],
                PreservedArtifacts =
                [
                    new PreservedGeneratedArtifactDefinition
                    {
                        ArtifactId = plan.PlanId,
                        ContractId = "semantic_selected_composition_plan_v1",
                        ArtifactKind = "semantic_selected_composition_plan",
                        Reason = "semantic_to_package_runtime_trace",
                        RawJson = JsonSerializer.Serialize(plan, JsonOptions)
                    }
                ]
            }
        };

        return package;
    }

    private static QuestDefinition BuildQuest(SemanticSelectedCompositionPlan plan) => new()
    {
        Id = plan.PackageQuestId,
        Title = TitleFromId(plan.SelectedQuestPatternId),
        Description = "Quest materialized from selected semantic rule declaration.",
        Kind = "semantic_selected_quest",
        AutoStart = false,
        Objectives = plan.SelectedQuestObjectives.Select(objective => ToPackageObjective(objective, plan)).ToList(),
        Rewards =
        [
            new OutputDefinition { Kind = "item", Id = "item/generated_reward", Amount = 1 },
            new OutputDefinition { Kind = "flag", Id = CompletionFlagId(plan), Amount = 1, Mode = "completed" }
        ],
        Tags = ["semantic_selected", plan.SelectedQuestPatternType],
        Metadata = Metadata(plan, ("selectedRuleQuestPatternId", plan.SelectedQuestPatternId))
    };

    private static QuestObjectiveDefinition ToPackageObjective(SemanticSelectedQuestObjectivePlan objective, SemanticSelectedCompositionPlan plan)
    {
        var kind = objective.ObjectiveKind switch
        {
            "objective/fetch_item" => "has_item",
            "objective/deliver_item" => "has_item",
            "objective/recover_item" => "complete_encounter",
            "objective/interact" when objective.TargetRef.StartsWith("npc/", StringComparison.Ordinal) => "talk_to",
            "objective/interact" => "custom_counter",
            _ => "custom_counter"
        };

        return new QuestObjectiveDefinition
        {
            Id = objective.PackageObjectiveId,
            Kind = kind,
            TargetId = objective.PackageTargetId,
            RequiredAmount = 1,
            Metadata = SortedArgs(
                ("selectedRuleObjectiveId", objective.ObjectiveId),
                ("selectedRuleObjectiveKind", objective.ObjectiveKind),
                ("selectedRuleTargetRef", objective.TargetRef),
                ("requiredInteractionPatternIds", string.Join(",", objective.RequiredInteractionPatternIds.OrderBy(item => item, StringComparer.Ordinal))),
                ("dialogue_id", objective.TargetRef.StartsWith("npc/", StringComparison.Ordinal) ? plan.PackageDialogueId : string.Empty))
        };
    }

    private static DialogueDefinition BuildDialogue(SemanticSelectedCompositionPlan plan)
    {
        var line = ReplaceSlots(DialogueLineTemplate(plan), plan.BoundSemanticSlots);
        return new DialogueDefinition
        {
            Id = plan.PackageDialogueId,
            Title = TitleFromId(plan.SelectedDialogueIntentId),
            StartNodeId = "start",
            Tags = ["semantic_selected", plan.SelectedDialogueIntentType],
            Metadata = Metadata(plan, ("selectedRuleDialogueIntentId", plan.SelectedDialogueIntentId)),
            Nodes =
            [
                new DialogueNodeDefinition
                {
                    Id = "start",
                    SpeakerId = plan.PackageNpcId,
                    Text = line,
                    Metadata = Metadata(plan, ("selectedRuleDialogueTemplateId", plan.SelectedDialogueTemplateId)),
                    Choices =
                    [
                        new DialogueChoiceDefinition
                        {
                            Id = "advance",
                            Text = "Continue.",
                            Metadata = SortedArgs(
                                ("quest_id", plan.PackageQuestId),
                                ("objective_id", plan.SelectedQuestObjectives.First().PackageObjectiveId)),
                            CloseDialogue = true
                        }
                    ]
                }
            ]
        };
    }

    private static InteractionDefinition BuildInteraction(SemanticSelectedCompositionPlan plan, SemanticSelectedInteractionBindingPlan binding)
    {
        var kind = binding.Family switch
        {
            "interaction/talk" => "talk",
            "interaction/use_item_on_target" => "use_item_on_target",
            "interaction/resolve_challenge" => "fight",
            _ => "inspect"
        };
        var effects = new List<EffectDefinition>();
        if (kind == "inspect")
        {
            effects.Add(new EffectDefinition
            {
                Type = binding.ResultActionId == "action/grant_item" ? "add_item" : "set_flag",
                Args = SortedArgs(
                    ("id", binding.ResultActionId == "action/grant_item" ? binding.PackageTargetId : FlagId(plan)),
                    ("amount", "1"),
                    ("value", "true"))
            });
        }

        return new InteractionDefinition
        {
            Id = binding.PackageInteractionId,
            Kind = kind,
            Effects = effects,
            Metadata = Metadata(
                plan,
                ("selectedRuleInteractionPatternId", binding.InteractionPatternId),
                ("selectedRuleInteractionFamily", binding.Family),
                ("selectedRuleInteractionTargetRef", binding.TargetRef),
                ("selectedRuleInteractionResultActionId", binding.ResultActionId),
                ("target_id", binding.PackageTargetId),
                ("dialogue_id", kind == "talk" ? plan.PackageDialogueId : string.Empty),
                ("item_id", kind == "use_item_on_target" ? binding.PackageRequiredItemId : string.Empty),
                ("encounter_id", kind == "fight" ? plan.PackageEncounterId : string.Empty),
                ("correlatedObjectiveIds", string.Join(",", binding.CorrelatedObjectiveIds)),
                ("seed", StableInt(plan.PlanHash).ToString(System.Globalization.CultureInfo.InvariantCulture)))
        };
    }

    private static ItemDefinition Item(string itemId, string name, SemanticSelectedCompositionPlan plan, string useFlagId = "") => new()
    {
        Id = itemId,
        Name = name,
        Description = "Semantic-selected runtime composition item.",
        Kind = "semantic_selected_item",
        MaxStack = 20,
        QuestItem = true,
        Tags = ["semantic_selected"],
        Metadata = Metadata(plan),
        UseEffects = string.IsNullOrWhiteSpace(useFlagId)
            ? []
            :
            [
                new EffectDefinition
                {
                    Type = "set_flag",
                    Args = SortedArgs(("id", useFlagId), ("value", "true"))
                }
            ]
    };

    private static bool CanResolvePackageTargetId(string targetRef) =>
        targetRef is "encounter/generated_challenge"
            or "item/generated_cache"
            or "item/generated_recovered_item"
            or "item/generated_reward"
            or "npc/generated_contact"
            or "object/generated_marker"
            or "quest/generated_goal";

    private static SortedDictionary<string, string> BuildSlots(SemanticGuidedCompositionScenario source) =>
        new(StringComparer.Ordinal)
        {
            ["challenge_title"] = TitleFromId("encounter/generated_challenge"),
            ["objective_target"] = TitleFromId(source.SelectedQuestPatternId),
            ["player"] = "player",
            ["quest_title"] = TitleFromId(source.SelectedQuestPatternId),
            ["reward_title"] = TitleFromId("item/generated_reward"),
            ["semantic_relation"] = source.Trace.FirstOrDefault()?.RelationId ?? source.ScenarioId
        };

    private static string DialogueLineTemplate(SemanticSelectedCompositionPlan plan) =>
        "{quest_title}: {objective_target}. {reward_title} tracks " + plan.SelectedDialogueIntentId + ".";

    private static string ReplaceSlots(string template, IReadOnlyDictionary<string, string> slots)
    {
        var result = template;
        foreach (var pair in slots.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            result = result.Replace("{" + pair.Key + "}", pair.Value, StringComparison.Ordinal);
        }

        return result;
    }

    private static string ResolvePackageTargetId(
        string targetRef,
        string packageQuestId,
        string packageDialogueId,
        string packageNpcId,
        string packageEncounterId) =>
        targetRef switch
        {
            "encounter/generated_challenge" => packageEncounterId,
            "item/generated_cache" => "item/generated_cache",
            "item/generated_recovered_item" => "item/generated_recovered_item",
            "item/generated_reward" => "item/generated_reward",
            "npc/generated_contact" => packageNpcId,
            "object/generated_marker" => packageNpcId,
            "quest/generated_goal" => packageQuestId,
            "dialogue/generated_contact" => packageDialogueId,
            _ => targetRef
        };

    private static string PackageQuestId(string scenarioId) => "quest/semantic_selected_" + IdSegment(scenarioId);
    private static string PackageDialogueId(string scenarioId) => "dialogue/semantic_selected_" + IdSegment(scenarioId);
    private static string PackageObjectiveId(string objectiveId) => "objective/" + IdSegment(objectiveId);
    private static string FlagId(SemanticSelectedCompositionPlan plan) => "flag/semantic_selected_" + ShortHash(plan.PlanHash);
    private static string CompletionFlagId(SemanticSelectedCompositionPlan plan) => "flag/semantic_completed_" + ShortHash(plan.PlanHash);

    private static IReadOnlyList<string> BuildTraceChain(
        SemanticGuidedCompositionScenario source,
        SemanticSelectedCompositionPlan plan,
        string packageHash,
        SemanticSelectedRuntimeCompositionRuntimeEvidence runtimeEvidence) =>
        [
            "semantic layers: " + string.Join(",", source.InputLayerIds),
            "compiled catalog hash: " + source.CompiledCatalogHash,
            "selected semantic relation ids: " + string.Join(",", source.Trace.Select(item => item.RelationId).OrderBy(item => item, StringComparer.Ordinal)),
            "selected rule declarations: " + plan.SelectedQuestPatternId + " / " + plan.SelectedDialogueIntentId + " / " + plan.SelectedInteractionPatternId,
            "composition plan hash: " + plan.PlanHash,
            "generated package hash: " + packageHash,
            "runtime commands: " + string.Join(",", runtimeEvidence.Commands.Select(item => item.CommandType).OrderBy(item => item, StringComparer.Ordinal)),
            "runtime evidence hash: " + runtimeEvidence.RuntimeEvidenceHash
        ];

    public static IReadOnlyList<SemanticSelectedRuntimeCompositionDiagnostic> AuditMaterializedPackage(
        GamePackageDefinition package,
        SemanticSelectedCompositionPlan plan)
    {
        var diagnostics = new List<SemanticSelectedRuntimeCompositionDiagnostic>();
        var quest = package.Game.Quests.FirstOrDefault(item => string.Equals(item.Id, plan.PackageQuestId, StringComparison.Ordinal));
        var dialogue = package.Game.Dialogues.FirstOrDefault(item => string.Equals(item.Id, plan.PackageDialogueId, StringComparison.Ordinal));
        var interactions = package.Game.Interactions.ToDictionary(item => item.Id, item => item, StringComparer.Ordinal);

        if (quest == null)
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.quest_missing", plan.PackageQuestId, "Selected package quest is missing."));
        }

        if (dialogue == null)
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.dialogue_missing", plan.PackageDialogueId, "Selected package dialogue is missing."));
        }

        if (!interactions.ContainsKey(plan.PackageInteractionId))
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.selected_interaction_missing", plan.PackageInteractionId, "Semantic-selected interaction is missing from package interactions."));
        }

        foreach (var objective in plan.SelectedQuestObjectives)
        {
            var packageObjective = quest?.Objectives.FirstOrDefault(item => string.Equals(item.Id, objective.PackageObjectiveId, StringComparison.Ordinal));
            if (packageObjective == null)
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.objective_missing", objective.PackageObjectiveId, "Selected quest objective is missing from the package quest."));
                continue;
            }

            if (!string.Equals(packageObjective.TargetId, objective.PackageTargetId, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.objective_target_mismatch", objective.PackageObjectiveId, "Selected quest objective target does not match the composition plan."));
            }

            if (!PackageTargetExists(package, objective.TargetRef, objective.PackageTargetId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.objective_target_missing", objective.PackageTargetId, "Selected quest objective target does not resolve to materialized package content."));
            }

            if (objective.TargetRef.StartsWith("npc/", StringComparison.Ordinal) &&
                (!packageObjective.Metadata.TryGetValue("dialogue_id", out var dialogueId) || !string.Equals(dialogueId, plan.PackageDialogueId, StringComparison.Ordinal)))
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.objective_dialogue_binding_missing", objective.PackageObjectiveId, "NPC objective must bind to the exact scenario dialogue id in metadata."));
            }

            foreach (var requiredInteractionId in objective.RequiredInteractionPatternIds.OrderBy(item => item, StringComparer.Ordinal))
            {
                if (!plan.MaterializedInteractions.Any(item => string.Equals(item.InteractionPatternId, requiredInteractionId, StringComparison.Ordinal)) ||
                    !interactions.ContainsKey(requiredInteractionId))
                {
                    diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.required_interaction_missing", requiredInteractionId, "Objective-required interaction is not materialized in the package."));
                }
            }
        }

        foreach (var binding in plan.MaterializedInteractions)
        {
            if (!interactions.TryGetValue(binding.PackageInteractionId, out var interaction))
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.materialized_interaction_missing", binding.PackageInteractionId, "Materialized interaction binding is missing from package interactions."));
                continue;
            }

            CheckMetadata(diagnostics, interaction.Metadata, "selectedRuleInteractionPatternId", binding.InteractionPatternId, interaction.Id);
            CheckMetadata(diagnostics, interaction.Metadata, "target_id", binding.PackageTargetId, interaction.Id);
            if (!PackageTargetExists(package, binding.TargetRef, binding.PackageTargetId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.interaction_target_missing", binding.PackageTargetId, "Materialized interaction target does not resolve to package content."));
            }

            if (binding.Family == "interaction/talk")
            {
                CheckMetadata(diagnostics, interaction.Metadata, "dialogue_id", plan.PackageDialogueId, interaction.Id);
            }

            if (binding.Family == "interaction/resolve_challenge")
            {
                CheckMetadata(diagnostics, interaction.Metadata, "encounter_id", plan.PackageEncounterId, interaction.Id);
            }

            if (binding.Family == "interaction/use_item_on_target")
            {
                CheckMetadata(diagnostics, interaction.Metadata, "item_id", binding.PackageRequiredItemId, interaction.Id);
                if (!package.Game.Items.Any(item => string.Equals(item.Id, binding.PackageRequiredItemId, StringComparison.Ordinal)))
                {
                    diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.required_item_missing", binding.PackageRequiredItemId, "Interaction required item does not resolve to package content."));
                }
            }
        }

        if (dialogue != null)
        {
            CheckMetadata(diagnostics, dialogue.Metadata, "selectedRuleDialogueIntentId", plan.SelectedDialogueIntentId, dialogue.Id);
            var startNode = dialogue.Nodes.FirstOrDefault(item => string.Equals(item.Id, dialogue.StartNodeId, StringComparison.Ordinal));
            if (startNode == null)
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.dialogue_start_missing", dialogue.Id, "Selected dialogue start node is missing."));
            }
            else
            {
                if (!string.Equals(startNode.SpeakerId, plan.PackageNpcId, StringComparison.Ordinal) ||
                    !PackageTargetExists(package, "npc/generated_contact", startNode.SpeakerId))
                {
                    diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.dialogue_speaker_missing", startNode.SpeakerId, "Selected dialogue speaker does not resolve to the package NPC entity."));
                }

                var advance = startNode.Choices.FirstOrDefault(item => string.Equals(item.Id, "advance", StringComparison.Ordinal));
                if (advance == null ||
                    !advance.Metadata.TryGetValue("quest_id", out var questId) ||
                    !string.Equals(questId, plan.PackageQuestId, StringComparison.Ordinal) ||
                    !advance.Metadata.TryGetValue("objective_id", out var objectiveId) ||
                    !plan.SelectedQuestObjectives.Any(item => string.Equals(item.PackageObjectiveId, objectiveId, StringComparison.Ordinal)))
                {
                    diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.dialogue_quest_binding_missing", dialogue.Id, "Selected dialogue choice must advance the exact package quest and objective."));
                }
            }
        }

        if (package.GeneratedContent.Profile.SourceContextJson is { Length: > 0 } sourceContextJson)
        {
            try
            {
                var provenance = JsonSerializer.Deserialize<Dictionary<string, string>>(sourceContextJson, JsonOptions) ?? [];
                CheckProvenance(diagnostics, provenance, "selectedQuestPatternId", plan.SelectedQuestPatternId);
                CheckProvenance(diagnostics, provenance, "selectedDialogueIntentId", plan.SelectedDialogueIntentId);
                CheckProvenance(diagnostics, provenance, "selectedInteractionPatternId", plan.SelectedInteractionPatternId);
            }
            catch (JsonException)
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.provenance_unreadable", plan.PlanId, "Generated package provenance SourceContextJson is not deterministic JSON."));
            }
        }

        var placeholderValues = package.Game.Quests.SelectMany(questItem => questItem.Objectives.Select(objective => objective.TargetId ?? string.Empty))
            .Concat(package.Game.Quests.SelectMany(questItem => questItem.Objectives.SelectMany(objective => objective.Metadata.Values)))
            .Concat(package.Game.Interactions.SelectMany(interaction => interaction.Metadata.Values))
            .Concat(package.Game.Dialogues.Select(dialogueItem => dialogueItem.Id))
            .Where(value => string.Equals(value, "dialogue/semantic_selected", StringComparison.Ordinal))
            .ToList();
        if (placeholderValues.Count > 0)
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.suffixless_dialogue_placeholder", "dialogue/semantic_selected", "Suffixless placeholder dialogue id is not an exact materialized package id."));
        }

        return SortDiagnostics(diagnostics.Count == 0
            ? [Diagnostic("info", "semantic_runtime.audit.materialized_bindings_passed", plan.PlanId, "Selected quest/dialogue/interaction bindings resolve to exact materialized package ids.")]
            : diagnostics);
    }

    private static IReadOnlyList<SemanticSelectedRuntimeCompositionDiagnostic> ValidateRuntimeEvidence(
        string scenarioId,
        SemanticSelectedCompositionPlan plan,
        GamePackageDefinition package,
        string packageHash,
        SemanticSelectedRuntimeCompositionRuntimeEvidence evidence)
    {
        var diagnostics = new List<SemanticSelectedRuntimeCompositionDiagnostic>();
        if (!evidence.RuntimeAttempted)
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.runtime_not_attempted", scenarioId, "Runtime evidence was not attempted for a valid audited package."));
        }

        if (!evidence.RuntimeStartSucceeded)
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.runtime_start_failed", scenarioId, "Runtime start did not succeed."));
        }

        if (!string.Equals(evidence.PackageHash, packageHash, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.package_hash_mismatch", scenarioId, "Runtime evidence package hash does not match the generated package hash."));
        }

        if (!EvidenceEquals(evidence, "packageId", package.Manifest.PackageId) ||
            !EvidenceEquals(evidence, "currentMapId", plan.PackageMapId))
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.package_identity_missing", scenarioId, "Runtime state evidence must observe the exact package id and map id."));
        }

        if (evidence.ExecutedQuestPatternId != plan.SelectedQuestPatternId ||
            evidence.ExecutedDialogueIntentId != plan.SelectedDialogueIntentId ||
            evidence.ExecutedInteractionPatternId != plan.SelectedInteractionPatternId ||
            evidence.ExecutedPackageQuestId != plan.PackageQuestId ||
            evidence.ExecutedPackageDialogueId != plan.PackageDialogueId ||
            evidence.ExecutedPackageInteractionId != plan.PackageInteractionId)
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.selected_ids_mismatch", scenarioId, "Runtime evidence selected declaration/package ids do not match the composition plan."));
        }

        foreach (var command in evidence.Commands)
        {
            if (!command.Succeeded)
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.required_command_failed", command.CommandId, "Every command required for semantic runtime acceptance must succeed."));
            }
        }

        RequireCommand(diagnostics, evidence, "gameplay/start_quest", plan.PackageQuestId);
        RequireCommand(diagnostics, evidence, "gameplay/open_dialogue", plan.PackageDialogueId);
        RequireCommand(diagnostics, evidence, "gameplay/choose_dialogue_option", "advance");
        foreach (var binding in plan.MaterializedInteractions)
        {
            RequireCommand(diagnostics, evidence, "gameplay/execute_interaction", binding.PackageInteractionId);
        }

        foreach (var objective in plan.SelectedQuestObjectives)
        {
            var objectiveEvidence = evidence.ObjectiveEvidence.FirstOrDefault(item => string.Equals(item.PackageObjectiveId, objective.PackageObjectiveId, StringComparison.Ordinal));
            if (objectiveEvidence == null)
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.objective_missing", objective.PackageObjectiveId, "Runtime-owned objective evidence is missing."));
                continue;
            }

            if (!objectiveEvidence.Completed ||
                !objectiveEvidence.RuntimeOwnedProgressEvidence ||
                objectiveEvidence.AfterAmount < objectiveEvidence.RequiredAmount ||
                !string.Equals(objectiveEvidence.TargetId, objective.PackageTargetId, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.objective_not_completed", objective.PackageObjectiveId, "Runtime-owned objective evidence must prove completion/progress for the exact package target."));
            }

            foreach (var requiredInteractionId in objective.RequiredInteractionPatternIds)
            {
                var correlation = evidence.ObjectiveInteractionCorrelations.FirstOrDefault(item =>
                    string.Equals(item.PackageObjectiveId, objective.PackageObjectiveId, StringComparison.Ordinal) &&
                    string.Equals(item.InteractionPatternId, requiredInteractionId, StringComparison.Ordinal));
                if (correlation == null || !correlation.InteractionSucceeded || !correlation.ObjectiveAdvanceSucceeded)
                {
                    diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.objective_interaction_correlation_missing", objective.PackageObjectiveId + "|" + requiredInteractionId, "Objective-required interaction must succeed before correlated objective advancement is counted."));
                }
            }
        }

        if (!evidence.StateDelta.DialogueOpened ||
            !evidence.StateDelta.DialogueClosedAfterChoice ||
            !string.Equals(evidence.StateDelta.ActiveDialogueIdAfterOpen, plan.PackageDialogueId, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.dialogue_state_missing", plan.PackageDialogueId, "Selected dialogue must be opened and its relevant option executed successfully."));
        }

        if (evidence.StateDelta.RewardAmountAfter <= evidence.StateDelta.RewardAmountBefore ||
            evidence.StateDelta.CompletionFlagBefore == evidence.StateDelta.CompletionFlagAfter ||
            !string.Equals(evidence.StateDelta.CompletionFlagAfter, "completed", StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.reward_completion_delta_missing", plan.PackageQuestId, "Reward/completion evidence must be an unambiguous runtime-owned delta, not pre-seeded inventory."));
        }

        if (!evidence.SaveLoadRoundtripPassed ||
            !string.Equals(evidence.RuntimeStateHash, evidence.RestoredRuntimeStateHash, StringComparison.Ordinal) ||
            !DictionaryEquals(evidence.StateEvidence, evidence.RestoredStateEvidence))
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.save_load_mismatch", scenarioId, "Save/load must restore the exact required runtime evidence."));
        }

        if (!evidence.SemanticSelectedIdsExecutedInRuntime && diagnostics.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.selected_execution_flag_false", scenarioId, "Adapter did not claim semantic-selected execution after providing otherwise valid evidence."));
        }

        return SortDiagnostics(diagnostics);
    }

    private static IReadOnlyList<SemanticSelectedRuntimeCompositionDiagnostic> ValidateCrossVariantIsolation(
        IReadOnlyList<SemanticSelectedRuntimeCompositionScenario> scenarios)
    {
        var diagnostics = new List<SemanticSelectedRuntimeCompositionDiagnostic>();
        foreach (var scenario in scenarios)
        {
            var ownKeys = ExpectedIsolationKeys(scenario).ToHashSet(StringComparer.Ordinal);
            if (!scenario.RuntimeEvidence.IsolationKeys.Any() ||
                !ownKeys.All(key => scenario.RuntimeEvidence.IsolationKeys.Contains(key, StringComparer.Ordinal)))
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.isolation.own_keys_missing", scenario.ScenarioId, "Runtime evidence must contain all scenario-specific package, declaration and state isolation keys."));
            }

            if (!EvidenceEquals(scenario.RuntimeEvidence, "scenarioId", scenario.ScenarioId))
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.isolation.scenario_id_missing", scenario.ScenarioId, "Runtime evidence scenario id must match the scenario being executed."));
            }

            foreach (var other in scenarios.Where(item => !string.Equals(item.ScenarioId, scenario.ScenarioId, StringComparison.Ordinal)))
            {
                var foreignKeys = ExpectedIsolationKeys(other).Except(ownKeys, StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
                if (scenario.RuntimeEvidence.IsolationKeys.Any(key => foreignKeys.Contains(key)) ||
                    scenario.RuntimeEvidence.StateEvidence.Values.Any(value => foreignKeys.Contains(value)))
                {
                    diagnostics.Add(Diagnostic("error", "semantic_runtime.isolation.foreign_state_leak", scenario.ScenarioId, "Runtime evidence contains package/declaration/state ids from another variant."));
                }
            }
        }

        return SortDiagnostics(diagnostics.Count == 0
            ? [Diagnostic("info", "semantic_runtime.isolation.variant_state_passed", "variant_matrix", "Sequential scenario evidence contains only its own package, selected declaration and state ids.")]
            : diagnostics);
    }

    private static string RenderReport(SemanticSelectedRuntimeCompositionReport report)
    {
        var lines = new List<string>
        {
            "# Semantic-Selected Runtime Composition Acceptance",
            string.Empty,
            "- Deterministic: true",
            "- External execution: none",
            $"- Accepted: `{report.Accepted.ToString().ToLowerInvariant()}`",
            $"- Report hash: `{report.DeterministicHash}`",
            $"- Manual gate: `{report.ManualGate}`",
            $"- Goal 005 gate recorded: `{report.Goal005GateRecorded.ToString().ToLowerInvariant()}`",
            $"- Semantic-selected ids executed in runtime: `{report.SemanticSelectedIdsExecutedInRuntime.ToString().ToLowerInvariant()}`",
            $"- Invalid scenario rejected: `{report.InvalidScenarioRejected.ToString().ToLowerInvariant()}`",
            $"- Replay/save-load/isolation: `{report.DeterministicReplayPassed.ToString().ToLowerInvariant()}` / `{report.SaveLoadRoundtripPassed.ToString().ToLowerInvariant()}` / `{report.CrossVariantIsolationPassed.ToString().ToLowerInvariant()}`",
            string.Empty,
            "## Scenarios",
            string.Empty
        };

        foreach (var scenario in report.Scenarios)
        {
            lines.Add($"### {scenario.ScenarioId}");
            lines.Add(string.Empty);
            lines.Add($"- Expected/actual valid: `{scenario.ExpectedValid.ToString().ToLowerInvariant()}` / `{scenario.ActualValid.ToString().ToLowerInvariant()}`");
            lines.Add($"- Selected ids: `{scenario.SelectedQuestPatternId}` / `{scenario.SelectedDialogueIntentId}` / `{scenario.SelectedInteractionPatternId}`");
            lines.Add($"- Plan hash: `{scenario.CompositionPlanHash}`");
            lines.Add($"- Package hash: `{scenario.GeneratedPackageHash}`");
            lines.Add($"- Runtime executed: `{scenario.SemanticSelectedIdsExecutedInRuntime.ToString().ToLowerInvariant()}`");
            lines.Add(string.Empty);
        }

        lines.Add("## Remaining C# Primitive Limits");
        lines.Add(string.Empty);
        lines.AddRange(report.RemainingCSharpPrimitiveLimits.Select(item => "- `" + item + "`"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- `{item.Severity}` `{item.Code}` target=`{item.Target}`: {item.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    private static string RenderVerification(SemanticSelectedRuntimeCompositionReport report)
    {
        var lines = new List<string>
        {
            "# Semantic-Selected Runtime Composition Verification",
            string.Empty,
            "Headless acceptance artifacts for Goal 006. No WinForms launch, local LLM, RAG index, provider, Lua, Unity or media execution is required.",
            string.Empty,
            $"- Headless acceptance status: `{report.Accepted.ToString().ToLowerInvariant()}`",
            $"- Next state marker: `{report.ManualGate}`",
            $"- Valid variants: `{report.ValidScenarioCount}`",
            $"- Invalid variant rejected: `{report.InvalidScenarioRejected.ToString().ToLowerInvariant()}`",
            $"- Runtime selected-id execution: `{report.SemanticSelectedIdsExecutedInRuntime.ToString().ToLowerInvariant()}`"
        };

        return string.Join("\n", lines) + "\n";
    }

    private static SemanticSelectedPackageValidationIssue ToValidationIssue(ValidationIssue issue) => new()
    {
        Severity = issue.Severity.ToString(),
        Code = issue.Code,
        Message = issue.Message,
        TargetId = issue.TargetId ?? string.Empty,
        Category = issue.Category ?? string.Empty
    };

    private static bool PackageTargetExists(GamePackageDefinition package, string sourceRef, string packageId)
    {
        if (sourceRef.StartsWith("item/", StringComparison.Ordinal))
        {
            return package.Game.Items.Any(item => string.Equals(item.Id, packageId, StringComparison.Ordinal));
        }

        if (sourceRef.StartsWith("encounter/", StringComparison.Ordinal))
        {
            return package.Game.Encounters.Any(item => string.Equals(item.Id, packageId, StringComparison.Ordinal));
        }

        if (sourceRef.StartsWith("quest/", StringComparison.Ordinal))
        {
            return package.Game.Quests.Any(item => string.Equals(item.Id, packageId, StringComparison.Ordinal));
        }

        if (sourceRef.StartsWith("dialogue/", StringComparison.Ordinal))
        {
            return package.Game.Dialogues.Any(item => string.Equals(item.Id, packageId, StringComparison.Ordinal));
        }

        if (sourceRef.StartsWith("npc/", StringComparison.Ordinal) || sourceRef.StartsWith("object/", StringComparison.Ordinal))
        {
            return package.Game.Maps.SelectMany(item => item.Entities).Any(item => string.Equals(item.Id, packageId, StringComparison.Ordinal));
        }

        return false;
    }

    private static void CheckMetadata(
        ICollection<SemanticSelectedRuntimeCompositionDiagnostic> diagnostics,
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string expected,
        string target)
    {
        if (!metadata.TryGetValue(key, out var actual) || !string.Equals(actual, expected, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.metadata_mismatch", target + "|" + key, "Materialized package metadata does not match the exact composition plan id."));
        }
    }

    private static void CheckProvenance(
        ICollection<SemanticSelectedRuntimeCompositionDiagnostic> diagnostics,
        IReadOnlyDictionary<string, string> provenance,
        string key,
        string expected)
    {
        if (!provenance.TryGetValue(key, out var actual) || !string.Equals(actual, expected, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.audit.provenance_mismatch", key, "Generated package provenance does not match the composition plan id."));
        }
    }

    private static bool EvidenceEquals(
        SemanticSelectedRuntimeCompositionRuntimeEvidence evidence,
        string key,
        string expected) =>
        evidence.StateEvidence.TryGetValue(key, out var actual) && string.Equals(actual, expected, StringComparison.Ordinal);

    private static void RequireCommand(
        ICollection<SemanticSelectedRuntimeCompositionDiagnostic> diagnostics,
        SemanticSelectedRuntimeCompositionRuntimeEvidence evidence,
        string commandType,
        string targetId)
    {
        if (!evidence.Commands.Any(item =>
                string.Equals(item.CommandType, commandType, StringComparison.Ordinal) &&
                string.Equals(item.TargetId, targetId, StringComparison.Ordinal) &&
                item.Succeeded))
        {
            diagnostics.Add(Diagnostic("error", "semantic_runtime.evidence.required_command_missing", commandType + "|" + targetId, "Required runtime command success evidence is missing."));
        }
    }

    private static bool DictionaryEquals(
        IReadOnlyDictionary<string, string> left,
        IReadOnlyDictionary<string, string> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        foreach (var pair in left)
        {
            if (!right.TryGetValue(pair.Key, out var value) || !string.Equals(value, pair.Value, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<string> ExpectedIsolationKeys(SemanticSelectedRuntimeCompositionScenario scenario)
    {
        yield return "scenario:" + scenario.ScenarioId;
        yield return "packageId:" + scenario.GeneratedPackageId;
        yield return "packageHash:" + scenario.GeneratedPackageHash;
        yield return "quest:" + scenario.CompositionPlan.PackageQuestId;
        yield return "dialogue:" + scenario.CompositionPlan.PackageDialogueId;
        yield return "selectedQuest:" + scenario.SelectedQuestPatternId;
        yield return "selectedDialogue:" + scenario.SelectedDialogueIntentId;
        yield return "selectedInteraction:" + scenario.SelectedInteractionPatternId;
        foreach (var objective in scenario.CompositionPlan.SelectedQuestObjectives)
        {
            yield return "objective:" + objective.PackageObjectiveId;
        }

        foreach (var interaction in scenario.CompositionPlan.MaterializedInteractions)
        {
            yield return "interaction:" + interaction.PackageInteractionId;
        }
    }

    private static bool HasDistinctValues(IEnumerable<string> values)
    {
        var list = values.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        return list.Count > 1 && list.Distinct(StringComparer.Ordinal).Count() == list.Count;
    }

    private static Dictionary<string, string> Metadata(SemanticSelectedCompositionPlan plan, params (string Key, string Value)[] extra) =>
        SortedArgs(
            [
                ("semanticRuntimePlanId", plan.PlanId),
                ("semanticRuntimePlanHash", plan.PlanHash),
                ("compiledCatalogHash", plan.CompiledCatalogHash),
                ("selectedQuestPatternId", plan.SelectedQuestPatternId),
                ("selectedDialogueIntentId", plan.SelectedDialogueIntentId),
                ("selectedInteractionPatternId", plan.SelectedInteractionPatternId),
                .. extra
            ]);

    private static Dictionary<string, string> SortedArgs(params (string Key, string Value)[] values) =>
        values
            .Where(item => !string.IsNullOrWhiteSpace(item.Value))
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);

    private static IReadOnlyList<SemanticSelectedRuntimeCompositionDiagnostic> SortDiagnostics(IEnumerable<SemanticSelectedRuntimeCompositionDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static SemanticSelectedRuntimeCompositionDiagnostic Diagnostic(
        string severity,
        string code,
        string target,
        string message) => new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static string IdSegment(string id)
    {
        var normalized = id.Replace('/', '_').Trim('_').ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            builder.Append(character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-' ? character : '_');
        }

        var segment = builder.ToString();
        while (segment.Contains("__", StringComparison.Ordinal))
        {
            segment = segment.Replace("__", "_", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(segment) ? "generated" : segment;
    }

    private static string TitleFromId(string id)
    {
        var segment = IdSegment(id);
        var words = segment.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word is not "quest" and not "pattern" and not "dialogue" and not "interaction" and not "objective" and not "generated" and not "item")
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..]);
        var title = string.Join(" ", words);
        return string.IsNullOrWhiteSpace(title) ? "Generated" : title;
    }

    private static int StableInt(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return BitConverter.ToInt32(hash, 0) & 0x7FFFFFFF;
    }

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ShortHash(string hash) => hash.Length <= 12 ? hash : hash[..12];

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Semantic-runtime composition output path must stay under the project root.");
        }
    }

    private sealed class RuntimeAdapterUnavailable : ISemanticSelectedRuntimeCompositionRuntimeAdapter
    {
        public SemanticSelectedRuntimeCompositionRuntimeEvidence Run(SemanticSelectedRuntimeCompositionRuntimeRequest request) => new()
        {
            RuntimeAttempted = false,
            PackageHash = request.PackageHash,
            Diagnostics =
            [
                Diagnostic("warning", "semantic_runtime.runtime_adapter_unavailable", request.ScenarioId, "Application layer has no LLMGameCreator.Runtime project dependency; focused tests and product smoke supply a headless runtime adapter.")
            ]
        };
    }
}

public interface ISemanticSelectedRuntimeCompositionRuntimeAdapter
{
    SemanticSelectedRuntimeCompositionRuntimeEvidence Run(SemanticSelectedRuntimeCompositionRuntimeRequest request);
}

public sealed record SemanticSelectedRuntimeCompositionRuntimeRequest
{
    public string ScenarioId { get; init; } = string.Empty;
    public SemanticSelectedCompositionPlan Plan { get; init; } = new();
    public GamePackageDefinition Package { get; init; } = new();
    public string PackageHash { get; init; } = string.Empty;
    public string PackageJson { get; init; } = string.Empty;
}

public sealed record SemanticSelectedRuntimeCompositionAcceptanceResult
{
    public SemanticSelectedRuntimeCompositionReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record SemanticSelectedRuntimeCompositionWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record SemanticSelectedRuntimeCompositionReport
{
    public string SchemaVersion { get; init; } = "1";
    public string DeterministicHash { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public bool Goal005GateRecorded { get; init; }
    public string Goal005SourceReportHash { get; init; } = string.Empty;
    public string ProofRulePackId { get; init; } = string.Empty;
    public bool ProofRulePackHasErrors { get; init; }
    public int ScenarioCount { get; init; }
    public int ValidScenarioCount { get; init; }
    public bool InvalidScenarioRejected { get; init; }
    public bool SemanticSelectedIdsExecutedInRuntime { get; init; }
    public bool DeterministicReplayPassed { get; init; }
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool CrossVariantIsolationPassed { get; init; }
    public bool CandidateConflictLeakageDetected { get; init; }
    public bool MultiSeedNoDanglingReferences { get; init; }
    public bool PackageValidationPassed { get; init; }
    public bool Goal004Goal005RegressionPassed { get; init; }
    public SemanticSelectedRuntimeCompositionExternalExecutionFlags ExternalExecution { get; init; } = new();
    public IReadOnlyList<SemanticSelectedRuntimeCompositionScenario> Scenarios { get; init; } = Array.Empty<SemanticSelectedRuntimeCompositionScenario>();
    public IReadOnlyList<SemanticSelectedRuntimeCompositionDiagnostic> Diagnostics { get; init; } = Array.Empty<SemanticSelectedRuntimeCompositionDiagnostic>();
    public IReadOnlyList<string> RemainingCSharpPrimitiveLimits { get; init; } = Array.Empty<string>();
}

public sealed record SemanticSelectedRuntimeCompositionScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<string> InputLayerIds { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> InputLayerHashes { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public string CompiledCatalogHash { get; init; } = string.Empty;
    public IReadOnlyList<SemanticSelectedTraceLink> SemanticTrace { get; init; } = Array.Empty<SemanticSelectedTraceLink>();
    public bool CandidateOrConflictLeakageDetected { get; init; }
    public string SelectedQuestPatternId { get; init; } = string.Empty;
    public string SelectedDialogueIntentId { get; init; } = string.Empty;
    public string SelectedInteractionPatternId { get; init; } = string.Empty;
    public SemanticSelectedCompositionPlan CompositionPlan { get; init; } = new();
    public string CompositionPlanHash { get; init; } = string.Empty;
    public string GeneratedPackageId { get; init; } = string.Empty;
    public string GeneratedPackageHash { get; init; } = string.Empty;
    public string GeneratedPackageJsonHash { get; init; } = string.Empty;
    public bool PackageValidationPassed { get; init; }
    public IReadOnlyList<SemanticSelectedPackageValidationIssue> ValidationIssues { get; init; } = Array.Empty<SemanticSelectedPackageValidationIssue>();
    public SemanticSelectedRuntimeCompositionRuntimeEvidence RuntimeEvidence { get; init; } = new();
    public bool SemanticSelectedIdsExecutedInRuntime { get; init; }
    public IReadOnlyList<string> TraceChain { get; init; } = Array.Empty<string>();
    public IReadOnlyList<SemanticSelectedRuntimeCompositionDiagnostic> Diagnostics { get; init; } = Array.Empty<SemanticSelectedRuntimeCompositionDiagnostic>();
}

public sealed record SemanticSelectedCompositionPlan
{
    public string SchemaVersion { get; init; } = "1";
    public string PlanId { get; init; } = string.Empty;
    public string PlanHash { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public IReadOnlyList<string> InputLayerIds { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> InputLayerHashes { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public string CompiledCatalogHash { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedSemanticTermIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<SemanticSelectedTraceLink> Trace { get; init; } = Array.Empty<SemanticSelectedTraceLink>();
    public string SelectedQuestPatternId { get; init; } = string.Empty;
    public string SelectedQuestPatternType { get; init; } = string.Empty;
    public IReadOnlyList<SemanticSelectedQuestObjectivePlan> SelectedQuestObjectives { get; init; } = Array.Empty<SemanticSelectedQuestObjectivePlan>();
    public string SelectedDialogueIntentId { get; init; } = string.Empty;
    public string SelectedDialogueIntentType { get; init; } = string.Empty;
    public string SelectedDialogueTemplateId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> BoundSemanticSlots { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public string SelectedInteractionPatternId { get; init; } = string.Empty;
    public string SelectedInteractionFamily { get; init; } = string.Empty;
    public string SelectedInteractionTargetRef { get; init; } = string.Empty;
    public string SelectedInteractionResultActionId { get; init; } = string.Empty;
    public string SelectedInteractionRequiredItemRef { get; init; } = string.Empty;
    public IReadOnlyList<SemanticSelectedInteractionBindingPlan> MaterializedInteractions { get; init; } = Array.Empty<SemanticSelectedInteractionBindingPlan>();
    public string PackageQuestId { get; init; } = string.Empty;
    public string PackageDialogueId { get; init; } = string.Empty;
    public string PackageInteractionId { get; init; } = string.Empty;
    public string PackageNpcId { get; init; } = string.Empty;
    public IReadOnlyList<string> PackageItemIds { get; init; } = Array.Empty<string>();
    public string PackageEncounterId { get; init; } = string.Empty;
    public string PackageMapId { get; init; } = string.Empty;
    public IReadOnlyList<SemanticSelectedRuntimeCompositionDiagnostic> Diagnostics { get; init; } = Array.Empty<SemanticSelectedRuntimeCompositionDiagnostic>();
    public IReadOnlyDictionary<string, string> Provenance { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    public string ScenarioLabel => PlanId.Contains('/', StringComparison.Ordinal) ? PlanId[(PlanId.LastIndexOf('/') + 1)..] : PlanId;
}

public sealed record SemanticSelectedQuestObjectivePlan
{
    public string ObjectiveId { get; init; } = string.Empty;
    public string ObjectiveKind { get; init; } = string.Empty;
    public string TargetRef { get; init; } = string.Empty;
    public string PackageObjectiveId { get; init; } = string.Empty;
    public string PackageTargetId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredInteractionPatternIds { get; init; } = Array.Empty<string>();
}

public sealed record SemanticSelectedInteractionBindingPlan
{
    public string InteractionPatternId { get; init; } = string.Empty;
    public string Family { get; init; } = string.Empty;
    public string TargetRef { get; init; } = string.Empty;
    public string ResultActionId { get; init; } = string.Empty;
    public string RequiredItemRef { get; init; } = string.Empty;
    public string PackageInteractionId { get; init; } = string.Empty;
    public string PackageTargetId { get; init; } = string.Empty;
    public string PackageRequiredItemId { get; init; } = string.Empty;
    public IReadOnlyList<string> CorrelatedObjectiveIds { get; init; } = Array.Empty<string>();
}

public sealed record SemanticSelectedTraceLink
{
    public string RelationId { get; init; } = string.Empty;
    public string RelationKind { get; init; } = string.Empty;
    public string SourceTermId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public IReadOnlyList<string> LayerIds { get; init; } = Array.Empty<string>();
}

public sealed record SemanticSelectedRuntimeCompositionRuntimeEvidence
{
    public bool RuntimeAttempted { get; init; }
    public bool RuntimeStartSucceeded { get; init; }
    public bool SemanticSelectedIdsExecutedInRuntime { get; init; }
    public string PackageHash { get; init; } = string.Empty;
    public string ExecutedQuestPatternId { get; init; } = string.Empty;
    public string ExecutedDialogueIntentId { get; init; } = string.Empty;
    public string ExecutedInteractionPatternId { get; init; } = string.Empty;
    public string ExecutedPackageQuestId { get; init; } = string.Empty;
    public string ExecutedPackageDialogueId { get; init; } = string.Empty;
    public string ExecutedPackageInteractionId { get; init; } = string.Empty;
    public IReadOnlyList<SemanticSelectedRuntimeCommandEvidence> Commands { get; init; } = Array.Empty<SemanticSelectedRuntimeCommandEvidence>();
    public IReadOnlyList<string> RuntimeEventTypes { get; init; } = Array.Empty<string>();
    public string RuntimeStateHash { get; init; } = string.Empty;
    public string RestoredRuntimeStateHash { get; init; } = string.Empty;
    public bool SaveLoadRoundtripPassed { get; init; }
    public string RuntimeEvidenceHash { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> StateEvidence { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> RestoredStateEvidence { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<SemanticSelectedRuntimeObjectiveEvidence> ObjectiveEvidence { get; init; } = Array.Empty<SemanticSelectedRuntimeObjectiveEvidence>();
    public IReadOnlyList<SemanticSelectedRuntimeInteractionObjectiveCorrelation> ObjectiveInteractionCorrelations { get; init; } = Array.Empty<SemanticSelectedRuntimeInteractionObjectiveCorrelation>();
    public SemanticSelectedRuntimeStateDelta StateDelta { get; init; } = new();
    public IReadOnlyList<string> IsolationKeys { get; init; } = Array.Empty<string>();
    public IReadOnlyList<SemanticSelectedRuntimeCompositionDiagnostic> Diagnostics { get; init; } = Array.Empty<SemanticSelectedRuntimeCompositionDiagnostic>();
}

public sealed record SemanticSelectedRuntimeCommandEvidence
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public bool Succeeded { get; init; }
    public string RuleInteractionPatternId { get; init; } = string.Empty;
    public string CorrelatedObjectiveId { get; init; } = string.Empty;
    public IReadOnlyList<string> EventTypes { get; init; } = Array.Empty<string>();
}

public sealed record SemanticSelectedRuntimeObjectiveEvidence
{
    public string PackageObjectiveId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public double BeforeAmount { get; init; }
    public double AfterAmount { get; init; }
    public double RequiredAmount { get; init; }
    public bool Completed { get; init; }
    public bool RuntimeOwnedProgressEvidence { get; init; }
    public IReadOnlyList<string> RequiredInteractionPatternIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CorrelatedInteractionPatternIds { get; init; } = Array.Empty<string>();
}

public sealed record SemanticSelectedRuntimeInteractionObjectiveCorrelation
{
    public string PackageObjectiveId { get; init; } = string.Empty;
    public string InteractionPatternId { get; init; } = string.Empty;
    public string PackageInteractionId { get; init; } = string.Empty;
    public string InteractionCommandId { get; init; } = string.Empty;
    public bool InteractionSucceeded { get; init; }
    public string ObjectiveAdvanceCommandId { get; init; } = string.Empty;
    public bool ObjectiveAdvanceSucceeded { get; init; }
}

public sealed record SemanticSelectedRuntimeStateDelta
{
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string MapIdBefore { get; init; } = string.Empty;
    public string MapIdAfter { get; init; } = string.Empty;
    public string QuestStateBefore { get; init; } = string.Empty;
    public string QuestStateAfter { get; init; } = string.Empty;
    public string RewardItemId { get; init; } = string.Empty;
    public double RewardAmountBefore { get; init; }
    public double RewardAmountAfter { get; init; }
    public string CompletionFlagId { get; init; } = string.Empty;
    public string CompletionFlagBefore { get; init; } = string.Empty;
    public string CompletionFlagAfter { get; init; } = string.Empty;
    public string ActiveDialogueIdAfterOpen { get; init; } = string.Empty;
    public bool DialogueOpened { get; init; }
    public bool DialogueClosedAfterChoice { get; init; }
    public string EncounterIdBefore { get; init; } = string.Empty;
    public string EncounterIdAfter { get; init; } = string.Empty;
}

public sealed record SemanticSelectedRuntimeCompositionExternalExecutionFlags
{
    public bool LlmExecuted { get; init; }
    public bool RagExecuted { get; init; }
    public bool ProviderExecuted { get; init; }
    public bool LuaExecuted { get; init; }
    public bool UnityExecuted { get; init; }
    public bool MediaExecuted { get; init; }
}

public sealed record SemanticSelectedRuntimeCompositionDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record SemanticSelectedPackageValidationIssue
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
}

public sealed record SemanticSelectedContentRef
{
    public string SourceRef { get; init; } = string.Empty;
    public string PackageKind { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
}
