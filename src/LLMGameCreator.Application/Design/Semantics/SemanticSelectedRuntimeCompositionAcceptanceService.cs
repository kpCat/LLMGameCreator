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

    public SemanticSelectedRuntimeCompositionAcceptanceService(
        SemanticGuidedCompositionAcceptanceService? semanticGuidedService = null,
        QuestDialogInteractionRulePackValidator? rulePackValidator = null,
        IGamePackageValidator? packageValidator = null,
        ISemanticSelectedRuntimeCompositionRuntimeAdapter? runtimeAdapter = null)
    {
        _semanticGuidedService = semanticGuidedService ?? new SemanticGuidedCompositionAcceptanceService();
        _rulePackValidator = rulePackValidator ?? new QuestDialogInteractionRulePackValidator();
        _packageValidator = packageValidator ?? new GamePackageValidator();
        _runtimeAdapter = runtimeAdapter ?? new RuntimeAdapterUnavailable();
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
                              string.IsNullOrWhiteSpace(invalidScenario.GeneratedPackageHash);
        var saveLoadPassed = validScenarios.All(item => item.RuntimeEvidence.SaveLoadRoundtripPassed);
        var noLeakage = validScenarios.All(item => !item.CandidateOrConflictLeakageDetected);
        var variantIsolationPassed = HasDistinctValues(validScenarios.Select(item => item.CompositionPlanHash)) &&
                                     HasDistinctValues(validScenarios.Select(item => item.GeneratedPackageHash)) &&
                                     HasDistinctValues(validScenarios.Select(item => item.RuntimeEvidence.RuntimeStateHash));
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
            : BuildPlan(source, selectedQuest, selectedDialogue, selectedInteraction, diagnostics);
        var candidateOrConflictLeakage = source.CandidateLeakageDetected ||
                                         source.Diagnostics.Any(item => item.Severity == SemanticDiagnosticSeverity.Error);
        var actualValid = source.Accepted &&
                          source.ExpectedValid &&
                          !candidateOrConflictLeakage &&
                          diagnostics.All(item => item.Severity != "error");
        GamePackageDefinition? package = null;
        string packageJson = string.Empty;
        string packageHash = string.Empty;
        var packageValidationPassed = false;
        var validationIssues = new List<SemanticSelectedPackageValidationIssue>();
        SemanticSelectedRuntimeCompositionRuntimeEvidence runtimeEvidence;

        if (actualValid)
        {
            package = BuildPackage(plan);
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

            runtimeEvidence = packageValidationPassed
                ? _runtimeAdapter.Run(new SemanticSelectedRuntimeCompositionRuntimeRequest
                {
                    ScenarioId = source.ScenarioId,
                    Plan = plan,
                    Package = package,
                    PackageHash = packageHash,
                    PackageJson = packageJson
                })
                : new SemanticSelectedRuntimeCompositionRuntimeEvidence
                {
                    Diagnostics =
                    [
                        Diagnostic("error", "semantic_runtime.runtime_blocked_by_validation", source.ScenarioId, "Package validation failed before runtime execution.")
                    ]
                };
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
        ICollection<SemanticSelectedRuntimeCompositionDiagnostic> diagnostics)
    {
        var contentRefs = BuildContentRefs();
        foreach (var targetRef in quest.Objectives.Select(item => item.TargetRef)
                     .Concat([interaction.TargetRef])
                     .Concat(string.IsNullOrWhiteSpace(interaction.RequiredItemRef) ? Array.Empty<string>() : [interaction.RequiredItemRef])
                     .OrderBy(item => item, StringComparer.Ordinal))
        {
            if (!contentRefs.ContainsKey(targetRef))
            {
                diagnostics.Add(Diagnostic("error", "semantic_runtime.content_binding_missing", targetRef, "Selected declaration target ref cannot be represented by generated package content."));
            }
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
                PackageTargetId = ResolvePackageTargetId(item.TargetRef),
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
            PackageQuestId = PackageQuestId(source.ScenarioId),
            PackageDialogueId = PackageDialogueId(source.ScenarioId),
            PackageInteractionId = interaction.InteractionId,
            PackageNpcId = "entity/generated_contact",
            PackageItemIds = ["item/generated_cache", "item/generated_reward", "item/generated_recovered_item"],
            PackageEncounterId = "encounter/generated_challenge",
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
        var interaction = BuildInteraction(plan);
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
                            new ItemStackDefinition { ItemId = "item/generated_reward", Amount = 1, QuestItem = true },
                            new ItemStackDefinition { ItemId = "item/generated_cache", Amount = 1, QuestItem = true }
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
                Interactions = [interaction],
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
                Mechanics =
                [
                    new GeneratedMechanicDefinition
                    {
                        SourceId = plan.SelectedInteractionPatternId,
                        PackageAbilityId = plan.PackageInteractionId,
                        Name = TitleFromId(plan.SelectedInteractionPatternId),
                        Description = "Selected interaction pattern represented by existing runtime primitives.",
                        Tags = [plan.SelectedInteractionFamily, plan.SelectedInteractionResultActionId]
                    }
                ],
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
        Objectives = plan.SelectedQuestObjectives.Select(ToPackageObjective).ToList(),
        Rewards =
        [
            new OutputDefinition { Kind = "item", Id = "item/generated_reward", Amount = 1 },
            new OutputDefinition { Kind = "flag", Id = CompletionFlagId(plan), Amount = 1, Mode = "completed" }
        ],
        Tags = ["semantic_selected", plan.SelectedQuestPatternType],
        Metadata = Metadata(plan, ("selectedRuleQuestPatternId", plan.SelectedQuestPatternId))
    };

    private static QuestObjectiveDefinition ToPackageObjective(SemanticSelectedQuestObjectivePlan objective)
    {
        var kind = objective.ObjectiveKind switch
        {
            "objective/fetch_item" => "has_item",
            "objective/deliver_item" => "has_item",
            "objective/recover_item" => "complete_encounter",
            "objective/interact" when objective.PackageTargetId.StartsWith("dialogue/", StringComparison.Ordinal) => "talk_to",
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
                ("dialogue_id", objective.PackageTargetId.StartsWith("dialogue/", StringComparison.Ordinal) ? objective.PackageTargetId : string.Empty))
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
                            AdvanceQuestId = plan.PackageQuestId,
                            Metadata = SortedArgs(("objective_id", plan.SelectedQuestObjectives.First().PackageObjectiveId)),
                            CloseDialogue = true
                        }
                    ]
                }
            ]
        };
    }

    private static InteractionDefinition BuildInteraction(SemanticSelectedCompositionPlan plan)
    {
        var kind = plan.SelectedInteractionFamily switch
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
                Type = plan.SelectedInteractionResultActionId == "action/grant_item" ? "add_item" : "set_flag",
                Args = SortedArgs(
                    ("id", plan.SelectedInteractionResultActionId == "action/grant_item" ? "item/generated_cache" : FlagId(plan)),
                    ("amount", "1"),
                    ("value", "true"))
            });
        }

        return new InteractionDefinition
        {
            Id = plan.PackageInteractionId,
            Kind = kind,
            Effects = effects,
            Metadata = Metadata(
                plan,
                ("selectedRuleInteractionPatternId", plan.SelectedInteractionPatternId),
                ("selectedRuleInteractionFamily", plan.SelectedInteractionFamily),
                ("selectedRuleInteractionTargetRef", plan.SelectedInteractionTargetRef),
                ("dialogue_id", kind == "talk" ? plan.PackageDialogueId : string.Empty),
                ("item_id", kind == "use_item_on_target" ? ResolvePackageTargetId(plan.SelectedInteractionRequiredItemRef) : string.Empty),
                ("encounter_id", kind == "fight" ? plan.PackageEncounterId : string.Empty),
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

    private static IReadOnlyDictionary<string, SemanticSelectedContentRef> BuildContentRefs() =>
        new SortedDictionary<string, SemanticSelectedContentRef>(StringComparer.Ordinal)
        {
            ["encounter/generated_challenge"] = new() { SourceRef = "encounter/generated_challenge", PackageKind = "encounter", PackageId = "encounter/generated_challenge" },
            ["item/generated_cache"] = new() { SourceRef = "item/generated_cache", PackageKind = "item", PackageId = "item/generated_cache" },
            ["item/generated_recovered_item"] = new() { SourceRef = "item/generated_recovered_item", PackageKind = "item", PackageId = "item/generated_recovered_item" },
            ["item/generated_reward"] = new() { SourceRef = "item/generated_reward", PackageKind = "item", PackageId = "item/generated_reward" },
            ["npc/generated_contact"] = new() { SourceRef = "npc/generated_contact", PackageKind = "dialogue", PackageId = "dialogue/semantic_selected" },
            ["object/generated_marker"] = new() { SourceRef = "object/generated_marker", PackageKind = "map_entity", PackageId = "entity/generated_contact" },
            ["quest/generated_goal"] = new() { SourceRef = "quest/generated_goal", PackageKind = "quest", PackageId = "quest/semantic_selected" }
        };

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

    private static string ResolvePackageTargetId(string targetRef) =>
        targetRef switch
        {
            "encounter/generated_challenge" => "encounter/generated_challenge",
            "item/generated_cache" => "item/generated_cache",
            "item/generated_recovered_item" => "item/generated_recovered_item",
            "item/generated_reward" => "item/generated_reward",
            "npc/generated_contact" => "dialogue/semantic_selected",
            "object/generated_marker" => "entity/generated_contact",
            "quest/generated_goal" => "quest/semantic_selected",
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
    public IReadOnlyList<SemanticSelectedRuntimeCompositionDiagnostic> Diagnostics { get; init; } = Array.Empty<SemanticSelectedRuntimeCompositionDiagnostic>();
}

public sealed record SemanticSelectedRuntimeCommandEvidence
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public bool Succeeded { get; init; }
    public IReadOnlyList<string> EventTypes { get; init; } = Array.Empty<string>();
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
