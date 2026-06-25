using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.RuntimePreview;

namespace LLMGameCreator.Application.Design.Semantics;

public sealed class SemanticGuidedCompositionAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/semantic-guided-composition";
    public const string ReportJsonFileName = "semantic-guided-composition-report.json";
    public const string ReportMarkdownFileName = "semantic-guided-composition-report.md";
    public const string VerificationMarkdownFileName = "semantic-guided-composition-verification.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly SemanticLayerCompilerService _compiler;
    private readonly QuestDialogInteractionFamilyAcceptanceService _goal004Service;
    private readonly QuestDialogInteractionRulePackValidator _rulePackValidator;

    public SemanticGuidedCompositionAcceptanceService(
        SemanticLayerCompilerService? compiler = null,
        QuestDialogInteractionFamilyAcceptanceService? goal004Service = null,
        QuestDialogInteractionRulePackValidator? rulePackValidator = null)
    {
        _compiler = compiler ?? new SemanticLayerCompilerService();
        _goal004Service = goal004Service ?? new QuestDialogInteractionFamilyAcceptanceService();
        _rulePackValidator = rulePackValidator ?? new QuestDialogInteractionRulePackValidator();
    }

    public SemanticGuidedCompositionAcceptanceResult Build(
        string? projectRootPath = null,
        string? referencePackRootPath = null)
    {
        var loadResult = LoadReferencePacks(referencePackRootPath);
        var packs = loadResult.Packs;
        var proofPack = QuestDialogInteractionFamilyAcceptanceService.BuildProofPack();
        var validRulePack = _rulePackValidator.Validate(proofPack);
        var acceptedExternalIds = BuildAcceptedExternalIds(proofPack);
        var options = new SemanticLayerCompilerOptions
        {
            AcceptedExternalTargetIds = acceptedExternalIds
        };

        var scenarioRequests = new[]
        {
            Scenario("core_plus_wildland_frontier", "goal005-wildland-frontier", "genre/wildland_frontier"),
            Scenario("core_plus_gothic_mystery", "goal005-gothic-mystery", "genre/gothic_mystery"),
            Scenario("core_plus_trade_caravan", "goal005-trade-caravan", "genre/trade_caravan"),
            Scenario("core_genre_project_overlay", "goal005-project-overlay", "genre/wildland_frontier", "project/sky_lantern_outpost"),
            Scenario("candidate_quarantine", "goal005-candidate-quarantine", "genre/wildland_frontier", "imported_candidate/rumor_candidates", "llm_candidate/unused_suggestions"),
            Scenario("invalid_conflict_rejection", "goal005-invalid-conflict", "genre/gothic_mystery", "project/conflicting_overlay")
        };

        var scenarios = scenarioRequests
            .Select(request => BuildScenario(request, packs, options, proofPack, validRulePack))
            .ToList();
        var repeatedFirst = BuildScenario(scenarioRequests[0], packs, options, proofPack, validRulePack);
        var multiSeedChecks = new[]
        {
            Scenario("seed_check_a", "goal005-seed-a", "genre/wildland_frontier"),
            Scenario("seed_check_b", "goal005-seed-b", "genre/gothic_mystery"),
            Scenario("seed_check_c", "goal005-seed-c", "genre/trade_caravan")
        }
            .Select(request => BuildScenario(request, packs, options, proofPack, validRulePack))
            .ToList();

        var goal004 = _goal004Service.Build(projectRootPath);
        var validScenarios = scenarios.Where(item => item.ExpectedValid).ToList();
        var selectedSignatures = validScenarios
            .Select(item => string.Join("|", item.SelectedQuestPatternId, item.SelectedDialogueIntentId, item.SelectedInteractionPatternId, item.SelectedSemanticTermIds.FirstOrDefault() ?? string.Empty))
            .Distinct(StringComparer.Ordinal)
            .Count();
        var candidateScenario = scenarios.Single(item => item.ScenarioId == "candidate_quarantine");
        var invalidScenario = scenarios.Single(item => item.ScenarioId == "invalid_conflict_rejection");
        var replayStable = scenarios[0].ScenarioHash == repeatedFirst.ScenarioHash;
        var multiSeedStable = multiSeedChecks.All(item => item.Accepted && item.NoDanglingSemanticReferences);
        var loadSucceeded = !loadResult.Diagnostics.Any(item => item.Severity == SemanticDiagnosticSeverity.Error);
        var expectedValidSatisfied = scenarios.Where(item => item.ExpectedValid).All(item => item.Accepted);
        var expectedInvalidRejected = scenarios
            .Where(item => !item.ExpectedValid)
            .All(item => !item.Accepted && item.Diagnostics.Any(diagnostic => diagnostic.Severity == SemanticDiagnosticSeverity.Error));

        var diagnostics = new List<SemanticGuidedCompositionDiagnostic>
        {
            Diagnostic("info", "semantic_guided.no_external_execution", "harness", "No LLM, RAG, provider, Lua, Unity or media execution was invoked."),
            Diagnostic("info", "semantic_guided.compiler_boundary", "compiled_catalog", "Deterministic composition consumes validated compiled semantic catalogs only."),
            Diagnostic(replayStable ? "info" : "error", replayStable ? "semantic_guided.replay_stable" : "semantic_guided.replay_unstable", "core_plus_wildland_frontier", "Repeated identical run byte/hash stability check."),
            Diagnostic(multiSeedStable ? "info" : "error", multiSeedStable ? "semantic_guided.multi_seed_stable" : "semantic_guided.multi_seed_unstable", "multi_seed_matrix", "Bounded multiple-seed semantic-reference check."),
            Diagnostic(candidateScenario.CandidateLeakageDetected ? "error" : "info", candidateScenario.CandidateLeakageDetected ? "semantic_guided.candidate_leakage" : "semantic_guided.candidates_quarantined", candidateScenario.ScenarioId, "Candidate layers must not enter active generation."),
            Diagnostic(invalidScenario.Accepted ? "error" : "info", invalidScenario.Accepted ? "semantic_guided.invalid_not_rejected" : "semantic_guided.invalid_rejected", invalidScenario.ScenarioId, "Invalid/conflicting semantic input must be rejected."),
            Diagnostic(goal004.Report.Accepted ? "info" : "error", goal004.Report.Accepted ? "semantic_guided.goal004_preserved" : "semantic_guided.goal004_regressed", "goal_004", "Goal 004 runtime-backed progress, reward and completion evidence is preserved.")
        };
        diagnostics.AddRange(loadResult.Diagnostics.Select(item => Diagnostic(
            item.Severity,
            item.Code,
            item.Target,
            item.Message)));

        var reportWithoutHash = new SemanticGuidedCompositionReport
        {
            Accepted = loadSucceeded
                       && validRulePack.HasErrors == false
                       && goal004.Report.Accepted
                       && selectedSignatures >= 3
                       && replayStable
                       && multiSeedStable
                       && expectedValidSatisfied
                       && expectedInvalidRejected,
            ManualGate = "semantic_guided_composition_artifact_verification",
            ScenarioCount = scenarios.Count,
            MeaningfulValidVariantCount = selectedSignatures,
            Goal004RuntimeEvidencePreserved = goal004.Report.Accepted,
            Goal004ReportHash = goal004.Report.DeterministicHash,
            SemanticSelectedIdsExecutedInRuntime = false,
            RuntimeEvidenceSource = "semantic_selection_is_generator_level_goal004_runtime_evidence_is_independent_regression",
            ExpectedValidScenariosAccepted = expectedValidSatisfied,
            ExpectedInvalidScenariosRejectedByErrors = expectedInvalidRejected,
            RepeatedRunStable = replayStable,
            MultiSeedNoDanglingReferences = multiSeedStable,
            ExternalExecution = new SemanticGuidedExternalExecutionFlags(),
            Scenarios = scenarios,
            MultiSeedChecks = multiSeedChecks,
            Diagnostics = SortDiagnostics(diagnostics),
            WhatIsSemanticPackDriven =
            [
                "quest pattern selection from prefers_quest_pattern relations",
                "dialogue intent selection from prefers_dialogue_intent relations",
                "interaction pattern selection from prefers_interaction_family relations",
                "source-term trace for selected preference relations",
                "candidate quarantine from imported_candidate and llm_candidate layers"
            ],
            WhatStillRequiresCSharpPrimitive =
            [
                "new runtime command families",
                "new mutable runtime state containers",
                "new formula evaluator semantics",
                "new rendering or UI interaction modes",
                "new external providers or Lua execution",
                "new semantic relation kind semantics beyond the v1 allow-list"
            ]
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new SemanticGuidedCompositionAcceptanceResult
        {
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<SemanticGuidedCompositionWriteResult> WriteAsync(
        string projectRootPath,
        SemanticGuidedCompositionAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "semantic-guided-composition"));
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

        return new SemanticGuidedCompositionWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<SemanticGuidedCompositionWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        string? referencePackRootPath = null,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath, referencePackRootPath);
        var write = await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);

        var coreScenario = result.Report.Scenarios.FirstOrDefault(item => item.ScenarioId == "core_plus_wildland_frontier");
        if (coreScenario != null)
        {
            await _compiler.WriteAsync(projectRootPath, coreScenario.CompilerResult, cancellationToken).ConfigureAwait(false);
        }

        return write;
    }

    private SemanticGuidedCompositionScenario BuildScenario(
        SemanticGuidedScenarioRequest request,
        IReadOnlyList<SemanticLayerPack> packs,
        SemanticLayerCompilerOptions options,
        QuestDialogInteractionRulePack proofPack,
        QuestDialogInteractionRulePackValidationReport rulePackValidation)
    {
        var layerIds = new[] { "core/base" }
            .Concat(request.LayerIds)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var selectedLayers = layerIds
            .Select(layerId => packs.FirstOrDefault(pack => string.Equals(pack.LayerId, layerId, StringComparison.Ordinal)))
            .Where(item => item != null)
            .Cast<SemanticLayerPack>()
            .ToList();
        var missingLayers = layerIds.Except(selectedLayers.Select(item => item.LayerId), StringComparer.Ordinal).ToList();
        var compilerResult = _compiler.Compile(selectedLayers, options);
        var compositionDiagnostics = BuildCompositionDiagnostics(compilerResult)
            .Concat(missingLayers.Select(layerId => new SemanticCatalogDiagnostic
            {
                Severity = SemanticDiagnosticSeverity.Error,
                Code = "semantic_guided.missing_layer",
                SourceArtifactId = request.ScenarioId,
                Target = layerId,
                Message = "Semantic scenario references a layer that was not loaded."
            }))
            .ToList();

        var selectedQuest = SelectTarget(
            compilerResult.Catalog.Relations,
            SemanticRelationKinds.PrefersQuestPattern,
            proofPack.QuestPatterns.Select(item => item.PatternId).ToHashSet(StringComparer.Ordinal),
            request.Seed,
            proofPack.QuestPatterns.Select(item => item.PatternId).OrderBy(item => item, StringComparer.Ordinal).First());
        var selectedDialogue = SelectTarget(
            compilerResult.Catalog.Relations,
            SemanticRelationKinds.PrefersDialogueIntent,
            proofPack.DialogueIntents.Select(item => item.IntentId).ToHashSet(StringComparer.Ordinal),
            request.Seed,
            proofPack.DialogueIntents.Select(item => item.IntentId).OrderBy(item => item, StringComparer.Ordinal).First());
        var selectedInteraction = SelectTarget(
            compilerResult.Catalog.Relations,
            SemanticRelationKinds.PrefersInteractionFamily,
            proofPack.InteractionPatterns.Select(item => item.InteractionId).ToHashSet(StringComparer.Ordinal),
            request.Seed,
            proofPack.InteractionPatterns.Select(item => item.InteractionId).OrderBy(item => item, StringComparer.Ordinal).First());

        var knownIds = compilerResult.Catalog.Terms.Select(item => item.TermId).ToHashSet(StringComparer.Ordinal);
        var noDangling = missingLayers.Count == 0 &&
                         compilerResult.Catalog.Relations.All(item =>
                             knownIds.Contains(item.SourceTermId) &&
                             (knownIds.Contains(item.TargetTermId) || options.AcceptedExternalTargetIds.Contains(item.TargetTermId)));
        var trace = BuildTrace(compilerResult.Catalog.Relations, selectedQuest, selectedDialogue, selectedInteraction);
        var candidateLeakage = compilerResult.Catalog.Terms.Any(item => item.Status != SemanticTermStatuses.Known) ||
                               compilerResult.QuarantinedTerms.Any(item => trace.Any(traceItem =>
                                   string.Equals(traceItem.SourceTermId, item.TermId, StringComparison.Ordinal) ||
                                   string.Equals(traceItem.TargetId, item.TermId, StringComparison.Ordinal)));
        var expectedValid = request.ExpectedValid;
        var scenarioDiagnostics = SortScenarioDiagnostics(compilerResult.Catalog.Diagnostics.Concat(compositionDiagnostics));
        var hasErrorDiagnostics = scenarioDiagnostics.Any(item => item.Severity == SemanticDiagnosticSeverity.Error);
        var accepted = compilerResult.Accepted &&
                       missingLayers.Count == 0 &&
                       noDangling &&
                       !candidateLeakage &&
                       !rulePackValidation.HasErrors &&
                       !hasErrorDiagnostics;

        var scenarioWithoutHash = new SemanticGuidedCompositionScenario
        {
            ScenarioId = request.ScenarioId,
            Seed = request.Seed,
            ExpectedValid = expectedValid,
            Accepted = accepted,
            InputLayerIds = selectedLayers.Select(item => item.LayerId).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            InputLayerHashes = selectedLayers.ToDictionary(item => item.LayerId, item => ComputeHash(JsonSerializer.Serialize(item, JsonOptions)), StringComparer.Ordinal),
            CompiledCatalogHash = compilerResult.CompiledCatalogHash,
            ActiveTermCount = compilerResult.ActiveTermCount,
            QuarantinedTermCount = compilerResult.QuarantinedTermCount,
            DiagnosticCount = compilerResult.Catalog.Diagnostics.Count + compositionDiagnostics.Count,
            CandidateLeakageDetected = candidateLeakage,
            NoDanglingSemanticReferences = noDangling,
            ExpectationMatched = expectedValid ? accepted : !accepted && hasErrorDiagnostics,
            SelectedQuestPatternId = selectedQuest,
            SelectedDialogueIntentId = selectedDialogue,
            SelectedInteractionPatternId = selectedInteraction,
            SelectedSemanticTermIds = trace.Select(item => item.SourceTermId).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            Trace = trace,
            CompilerResult = compilerResult,
            Diagnostics = scenarioDiagnostics
        };

        return scenarioWithoutHash with
        {
            ScenarioHash = ComputeHash(JsonSerializer.Serialize(scenarioWithoutHash, JsonOptions))
        };
    }

    private static IReadOnlyList<SemanticCatalogDiagnostic> BuildCompositionDiagnostics(SemanticLayerCompilerResult compilerResult)
    {
        var diagnostics = new List<SemanticCatalogDiagnostic>();
        var active = compilerResult.Catalog.Terms.Select(item => item.TermId).ToHashSet(StringComparer.Ordinal);
        foreach (var relation in compilerResult.Catalog.Relations)
        {
            if (relation.RelationKind is SemanticRelationKinds.Excludes or SemanticRelationKinds.ForbiddenInTone &&
                active.Contains(relation.SourceTermId) &&
                active.Contains(relation.TargetTermId))
            {
                diagnostics.Add(new SemanticCatalogDiagnostic
                {
                    Severity = SemanticDiagnosticSeverity.Error,
                    Code = relation.RelationKind == SemanticRelationKinds.Excludes
                        ? "semantic_guided.excludes_conflict"
                        : "semantic_guided.forbidden_tone_conflict",
                    SourceArtifactId = string.Join(",", relation.LayerIds),
                    Target = relation.RelationId,
                    Message = "Active semantic composition contains mutually incompatible terms."
                });
            }

            if (relation.RelationKind == SemanticRelationKinds.Requires &&
                active.Contains(relation.SourceTermId) &&
                !active.Contains(relation.TargetTermId))
            {
                diagnostics.Add(new SemanticCatalogDiagnostic
                {
                    Severity = SemanticDiagnosticSeverity.Error,
                    Code = "semantic_guided.requires_unsatisfied",
                    SourceArtifactId = string.Join(",", relation.LayerIds),
                    Target = relation.RelationId,
                    Message = "Active semantic composition has an unsatisfied requires relation."
                });
            }
        }

        return diagnostics
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();
    }

    private static string SelectTarget(
        IEnumerable<SemanticCatalogRelation> relations,
        string relationKind,
        IReadOnlySet<string> validTargets,
        string seed,
        string fallback)
    {
        var candidates = relations
            .Where(item => item.RelationKind == relationKind && validTargets.Contains(item.TargetTermId))
            .OrderByDescending(RelationPriority)
            .ThenBy(item => StableRank(seed, item.RelationId + "|" + item.TargetTermId))
            .ThenBy(item => item.TargetTermId, StringComparer.Ordinal)
            .Select(item => item.TargetTermId)
            .ToList();

        return candidates.Count == 0 ? fallback : candidates[0];
    }

    private static int RelationPriority(SemanticCatalogRelation relation)
    {
        if (relation.LayerIds.Any(item => item.StartsWith("project/", StringComparison.Ordinal)))
        {
            return 3;
        }

        if (relation.LayerIds.Any(item => item.StartsWith("genre/", StringComparison.Ordinal)))
        {
            return 2;
        }

        if (relation.LayerIds.Any(item => item.StartsWith("core/", StringComparison.Ordinal)))
        {
            return 1;
        }

        return 0;
    }

    private static IReadOnlyList<SemanticGuidedSelectionTrace> BuildTrace(
        IEnumerable<SemanticCatalogRelation> relations,
        string selectedQuest,
        string selectedDialogue,
        string selectedInteraction)
    {
        var selected = new HashSet<string>([selectedQuest, selectedDialogue, selectedInteraction], StringComparer.Ordinal);
        return relations
            .Where(item => selected.Contains(item.TargetTermId))
            .OrderBy(item => item.RelationKind, StringComparer.Ordinal)
            .ThenBy(item => item.RelationId, StringComparer.Ordinal)
            .Select(item => new SemanticGuidedSelectionTrace
            {
                RelationId = item.RelationId,
                RelationKind = item.RelationKind,
                SourceTermId = item.SourceTermId,
                TargetId = item.TargetTermId,
                LayerIds = item.LayerIds
            })
            .ToList();
    }

    private SemanticLayerPackLoadResult LoadReferencePacks(string? referencePackRootPath)
    {
        var root = string.IsNullOrWhiteSpace(referencePackRootPath)
            ? Path.Combine(FindRepositoryRoot(), "generator-library", "semantic-packs")
            : referencePackRootPath;
        return _compiler.LoadPacksFromDirectory(root);
    }

    private static IReadOnlySet<string> BuildAcceptedExternalIds(QuestDialogInteractionRulePack proofPack) =>
        proofPack.QuestPatterns.Select(item => item.PatternId)
            .Concat(proofPack.DialogueIntents.Select(item => item.IntentId))
            .Concat(proofPack.InteractionPatterns.Select(item => item.InteractionId))
            .ToHashSet(StringComparer.Ordinal);

    private static SemanticGuidedScenarioRequest Scenario(
        string scenarioId,
        string seed,
        params string[] layerIds) => new()
        {
            ScenarioId = scenarioId,
            Seed = seed,
            ExpectedValid = scenarioId != "invalid_conflict_rejection",
            LayerIds = layerIds
        };

    private static long StableRank(string seed, string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed + "|" + value));
        return BitConverter.ToInt64(hash, 0) & long.MaxValue;
    }

    private static IReadOnlyList<SemanticGuidedCompositionDiagnostic> SortDiagnostics(IEnumerable<SemanticGuidedCompositionDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<SemanticCatalogDiagnostic> SortScenarioDiagnostics(IEnumerable<SemanticCatalogDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static SemanticGuidedCompositionDiagnostic Diagnostic(
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

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string RenderReport(SemanticGuidedCompositionReport report)
    {
        var lines = new List<string>
        {
            "# Semantic-Guided Composition Acceptance",
            string.Empty,
            "- Deterministic: true",
            "- External execution: none",
            $"- Accepted: `{report.Accepted.ToString().ToLowerInvariant()}`",
            $"- Report hash: `{report.DeterministicHash}`",
            $"- Manual gate: `{report.ManualGate}`",
            $"- Scenarios: `{report.ScenarioCount}`",
            $"- Meaningful valid variants: `{report.MeaningfulValidVariantCount}`",
            $"- Goal 004 preserved: `{report.Goal004RuntimeEvidencePreserved.ToString().ToLowerInvariant()}`",
            $"- Semantic-selected ids executed in runtime: `{report.SemanticSelectedIdsExecutedInRuntime.ToString().ToLowerInvariant()}`",
            $"- Runtime evidence source: `{report.RuntimeEvidenceSource}`",
            string.Empty,
            "## Scenarios",
            string.Empty
        };

        foreach (var scenario in report.Scenarios)
        {
            lines.Add($"### {scenario.ScenarioId}");
            lines.Add(string.Empty);
            lines.Add($"- Accepted: `{scenario.Accepted.ToString().ToLowerInvariant()}`");
            lines.Add($"- Layers: `{string.Join(", ", scenario.InputLayerIds)}`");
            lines.Add($"- Catalog hash: `{scenario.CompiledCatalogHash}`");
            lines.Add($"- Quest/dialogue/interaction: `{scenario.SelectedQuestPatternId}` / `{scenario.SelectedDialogueIntentId}` / `{scenario.SelectedInteractionPatternId}`");
            lines.Add($"- Active/quarantined terms: `{scenario.ActiveTermCount}` / `{scenario.QuarantinedTermCount}`");
            lines.Add($"- Candidate leakage: `{scenario.CandidateLeakageDetected.ToString().ToLowerInvariant()}`");
            lines.Add(string.Empty);
        }

        lines.Add("## Semantic-Pack Driven Choices");
        lines.Add(string.Empty);
        lines.AddRange(report.WhatIsSemanticPackDriven.Select(item => "- `" + item + "`"));
        lines.Add(string.Empty);
        lines.Add("## Requires C# Primitive");
        lines.Add(string.Empty);
        lines.AddRange(report.WhatStillRequiresCSharpPrimitive.Select(item => "- `" + item + "`"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- `{item.Severity}` `{item.Code}` target=`{item.Target}`: {item.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    private static string RenderVerification(SemanticGuidedCompositionReport report)
    {
        var lines = new List<string>
        {
            "# Semantic-Guided Composition Artifact Verification",
            string.Empty,
            "Review the generated report artifacts only. No WinForms launch, local LLM, RAG index, provider, Lua, Unity or media execution is required.",
            string.Empty,
            "1. Inspect `semantic-guided-composition-report.json`.",
            "2. Confirm the valid genre/project scenarios select different quest, dialogue and interaction ids.",
            "3. Confirm candidate scenarios keep quarantined terms out of active selections.",
            "4. Confirm invalid/conflict input is rejected with diagnostics.",
            "5. Confirm Goal 004 runtime-backed progress, reward and completion evidence remains preserved.",
            string.Empty,
            $"Headless acceptance status: `{report.Accepted.ToString().ToLowerInvariant()}`",
            $"Next state marker: `{report.ManualGate}`"
        };

        return string.Join("\n", lines) + "\n";
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln")))
        {
            directory = directory.Parent;
        }

        if (directory != null)
        {
            return directory.FullName;
        }

        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? Directory.GetCurrentDirectory();
    }

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Semantic-guided composition output path must stay under the project root.");
        }
    }
}

public sealed record SemanticGuidedScenarioRequest
{
    public string ScenarioId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; } = true;
    public IReadOnlyList<string> LayerIds { get; init; } = Array.Empty<string>();
}

public sealed record SemanticGuidedCompositionAcceptanceResult
{
    public SemanticGuidedCompositionReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record SemanticGuidedCompositionWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record SemanticGuidedCompositionReport
{
    public string SchemaVersion { get; init; } = "1";
    public string DeterministicHash { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public int ScenarioCount { get; init; }
    public int MeaningfulValidVariantCount { get; init; }
    public bool Goal004RuntimeEvidencePreserved { get; init; }
    public string Goal004ReportHash { get; init; } = string.Empty;
    public bool SemanticSelectedIdsExecutedInRuntime { get; init; }
    public string RuntimeEvidenceSource { get; init; } = string.Empty;
    public bool ExpectedValidScenariosAccepted { get; init; }
    public bool ExpectedInvalidScenariosRejectedByErrors { get; init; }
    public bool RepeatedRunStable { get; init; }
    public bool MultiSeedNoDanglingReferences { get; init; }
    public SemanticGuidedExternalExecutionFlags ExternalExecution { get; init; } = new();
    public IReadOnlyList<SemanticGuidedCompositionScenario> Scenarios { get; init; } = Array.Empty<SemanticGuidedCompositionScenario>();
    public IReadOnlyList<SemanticGuidedCompositionScenario> MultiSeedChecks { get; init; } = Array.Empty<SemanticGuidedCompositionScenario>();
    public IReadOnlyList<SemanticGuidedCompositionDiagnostic> Diagnostics { get; init; } = Array.Empty<SemanticGuidedCompositionDiagnostic>();
    public IReadOnlyList<string> WhatIsSemanticPackDriven { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> WhatStillRequiresCSharpPrimitive { get; init; } = Array.Empty<string>();
}

public sealed record SemanticGuidedCompositionScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool Accepted { get; init; }
    public string ScenarioHash { get; init; } = string.Empty;
    public IReadOnlyList<string> InputLayerIds { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> InputLayerHashes { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public string CompiledCatalogHash { get; init; } = string.Empty;
    public int ActiveTermCount { get; init; }
    public int QuarantinedTermCount { get; init; }
    public int DiagnosticCount { get; init; }
    public bool CandidateLeakageDetected { get; init; }
    public bool NoDanglingSemanticReferences { get; init; }
    public bool ExpectationMatched { get; init; }
    public string SelectedQuestPatternId { get; init; } = string.Empty;
    public string SelectedDialogueIntentId { get; init; } = string.Empty;
    public string SelectedInteractionPatternId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedSemanticTermIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<SemanticGuidedSelectionTrace> Trace { get; init; } = Array.Empty<SemanticGuidedSelectionTrace>();
    public SemanticLayerCompilerResult CompilerResult { get; init; } = new();
    public IReadOnlyList<SemanticCatalogDiagnostic> Diagnostics { get; init; } = Array.Empty<SemanticCatalogDiagnostic>();
}

public sealed record SemanticGuidedSelectionTrace
{
    public string RelationId { get; init; } = string.Empty;
    public string RelationKind { get; init; } = string.Empty;
    public string SourceTermId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public IReadOnlyList<string> LayerIds { get; init; } = Array.Empty<string>();
}

public sealed record SemanticGuidedExternalExecutionFlags
{
    public bool LlmExecuted { get; init; }
    public bool RagExecuted { get; init; }
    public bool ProviderExecuted { get; init; }
    public bool LuaExecuted { get; init; }
    public bool UnityExecuted { get; init; }
    public bool MediaExecuted { get; init; }
}

public sealed record SemanticGuidedCompositionDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
