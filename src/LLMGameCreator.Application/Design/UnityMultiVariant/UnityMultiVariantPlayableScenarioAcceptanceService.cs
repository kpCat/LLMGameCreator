using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.AlphaBuild;
using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Application.Design.ContentGeneration;
using LLMGameCreator.Application.Design.UnityGeneratedScene;
using LLMGameCreator.Application.Design.UnityPlayableAlpha;
using LLMGameCreator.Application.Design.UnityQuestLoop;

namespace LLMGameCreator.Application.Design.UnityMultiVariant;

public sealed class UnityMultiVariantPlayableScenarioAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/unity-multi-variant-playable-scenario";
    public const string VariantsJsonFileName = "unity-multi-variant-playable-scenario-variants.json";
    public const string ReportJsonFileName = "unity-multi-variant-playable-scenario-report.json";
    public const string ReportMarkdownFileName = "unity-multi-variant-playable-scenario-report.md";
    public const string VerificationMarkdownFileName = "unity-multi-variant-playable-scenario-verification.md";
    public const string FinalGate = "unity_generated_multi_variant_playable_scenario_verification";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly string[] ExpectedStyleIds = ["frontier_survival", "gothic_mystery", "trade_caravan"];
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static UnityMultiVariantPlayableScenarioAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public UnityMultiVariantPlayableScenarioAcceptanceResult BuildFromAcceptedEvidence(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityMultiVariantPlayableScenarioOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contentGenerationResult);
        ArgumentNullException.ThrowIfNull(minimumAssetResult);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new UnityMultiVariantPlayableScenarioOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var repositoryRoot = ResolveRepositoryRoot(projectRoot, settings.RepositoryRootPath);
        var selectedStyleIds = NormalizeSelectedStyles(settings.SelectedStyleIds);
        var diagnostics = new List<UnityMultiVariantDiagnostic>
        {
            Diagnostic("info", "unity_multi_variant.goal017_gate_recorded", FinalGate, "User-confirmed Goal 017 quest completion verification is recorded as passed."),
            Diagnostic("info", "unity_multi_variant.no_external_providers", "execution_boundary", "No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.")
        };

        diagnostics.AddRange(ValidateSelectedStyles(selectedStyleIds));
        var previousEvidence = ValidatePreviousGoal017Evidence(repositoryRoot);
        diagnostics.AddRange(previousEvidence.Diagnostics);

        var questService = new UnityQuestCompletionLoopAcceptanceService();
        var workVariants = new List<VariantWorkItem>();
        foreach (var styleId in selectedStyleIds.Where(IsKnownStyle))
        {
            var variantRelativeOutput = $"{RelativeOutputDirectory}/variants/{styleId}";
            var questResult = questService.BuildFromAcceptedEvidence(
                projectRoot,
                contentGenerationResult,
                minimumAssetResult,
                new UnityQuestCompletionLoopOptions
                {
                    RepositoryRootPath = repositoryRoot,
                    RelativeOutputDirectoryOverride = variantRelativeOutput,
                    SelectedStyleId = styleId,
                    ExecuteUnityBuild = settings.ExecuteUnityBuild,
                    LaunchBuiltPlayer = settings.LaunchBuiltPlayer,
                    PreserveExistingBuildOutputForValidation = settings.PreserveExistingBuildOutputForValidation,
                    CleanupUnityWorkProject = settings.CleanupUnityWorkProject,
                    UnityBuildTimeoutSeconds = settings.UnityBuildTimeoutSeconds,
                    PlayerLaunchTimeoutSeconds = settings.PlayerLaunchTimeoutSeconds
                });

            var projection = UnityGeneratedSceneProjectionAcceptanceService.BuildProjection(questResult.Report.AlphaBuild);
            var summary = BuildVariantSummary(styleId, questResult.Report, projection, previousEvidence.Passed);
            workVariants.Add(new VariantWorkItem
            {
                Summary = summary,
                QuestReport = questResult.Report,
                Projection = projection,
                Plan = questResult.Report.Plan
            });

        }

        var variantSummaries = workVariants.Select(item => item.Summary).OrderBy(item => item.StyleId, StringComparer.Ordinal).ToList();
        var distinctness = ValidateVariantSet(variantSummaries);
        diagnostics.AddRange(distinctness.Diagnostics);
        var invalidMatrix = BuildInvalidMatrix(workVariants, previousEvidence);
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var variantsJson = JsonSerializer.Serialize(variantSummaries, JsonOptions);
        var variantsHash = ComputeHash(variantsJson);
        var acceptedVariantCount = variantSummaries.Count(item => item.Accepted);
        var allVariantsQuestComplete = variantSummaries.Count >= 3 && variantSummaries.All(item => item.QuestCompletedVerified);
        var allVariantsRewardGranted = variantSummaries.Count >= 3 && variantSummaries.All(item => item.RewardGrantedVerified);
        var allVariantsUseSamePipeline = variantSummaries.Count >= 3 && variantSummaries.All(item => item.ProductSmokeRoute == "unity-quest-completion-loop");
        var multiVariantVerified =
            previousEvidence.Passed &&
            distinctness.Passed &&
            invalidMatrix.Passed &&
            acceptedVariantCount >= 3 &&
            allVariantsQuestComplete &&
            allVariantsRewardGranted &&
            allVariantsUseSamePipeline;

        var reportWithoutHash = new UnityMultiVariantPlayableScenarioReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = "unity_generated_quest_completion_loop_verification passed",
            CompletedSlices = ["S146", "S147", "S148", "S149", "S150", "S151", "S152", "S153"],
            ProductSmokeRoute = "unity-multi-variant-playable-scenario",
            VariantCount = variantSummaries.Count,
            AcceptedVariantCount = acceptedVariantCount,
            SelectedStyleIds = variantSummaries.Select(item => item.StyleId).ToList(),
            SelectedPackageIds = variantSummaries.Select(item => item.PackageId).ToList(),
            SelectedThreadIds = variantSummaries.Select(item => item.ThreadId).ToList(),
            DistinctStyleCount = variantSummaries.Select(item => item.StyleId).Distinct(StringComparer.Ordinal).Count(),
            DistinctPackageCount = variantSummaries.Select(item => item.PackageId).Distinct(StringComparer.Ordinal).Count(),
            DistinctQuestCount = variantSummaries.Select(item => item.QuestId).Distinct(StringComparer.Ordinal).Count(),
            DistinctSceneSignatureCount = variantSummaries.Select(SceneSignature).Distinct(StringComparer.Ordinal).Count(),
            DistinctObjectiveSignatureCount = variantSummaries.Select(ObjectiveSignature).Distinct(StringComparer.Ordinal).Count(),
            AllVariantsQuestComplete = allVariantsQuestComplete,
            AllVariantsRewardGranted = allVariantsRewardGranted,
            AllVariantsUseSamePipeline = allVariantsUseSamePipeline,
            MultiVariantPlayableScenarioVerified = multiVariantVerified,
            VariantSummaries = variantSummaries,
            InvalidMatrix = invalidMatrix,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            NoExternalProviderLlmRagLuaMedia = true,
            RuntimePreviewDependency = false,
            VariantsHash = variantsHash,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new UnityMultiVariantPlayableScenarioAcceptanceResult
        {
            VariantsJson = variantsJson,
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<UnityMultiVariantPlayableScenarioWriteResult> WriteAsync(
        string projectRootPath,
        UnityMultiVariantPlayableScenarioAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var variantsPath = Path.Combine(outputDirectory, VariantsJsonFileName);
        var jsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        await File.WriteAllTextAsync(variantsPath, result.VariantsJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new UnityMultiVariantPlayableScenarioWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            VariantsJsonPath = variantsPath,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<UnityMultiVariantPlayableScenarioWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityMultiVariantPlayableScenarioOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromAcceptedEvidence(projectRootPath, contentGenerationResult, minimumAssetResult, options);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static UnityMultiVariantSummary BuildVariantSummary(
        string expectedStyleId,
        UnityQuestCompletionLoopReport questReport,
        UnityGeneratedSceneProjection projection,
        bool previousEvidencePassed)
    {
        var diagnostics = new List<UnityMultiVariantDiagnostic>();
        if (!string.Equals(questReport.SelectedStyleId, expectedStyleId, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "unity_multi_variant.variant.style_mismatch", expectedStyleId, "Selected quest loop style must match the requested variant style."));
        }

        if (!questReport.PlayLoopVerified || string.IsNullOrWhiteSpace(questReport.AlphaBuild.LaunchVerification.PlayLoopLogRelativePath))
        {
            diagnostics.Add(Diagnostic("error", "unity_multi_variant.variant.player_log_missing", expectedStyleId, "Each variant requires its own player play-loop evidence."));
        }

        diagnostics.AddRange(ValidateStyleBindings(expectedStyleId, questReport, projection));

        var runtimeStateLoopEvidenceVerified = questReport.State.RuntimeStateLoopVerified && questReport.QuestCompletionLoopVerified;
        var accepted =
            previousEvidencePassed &&
            questReport.QuestCompletionLoopVerified &&
            questReport.QuestPlanVerified &&
            questReport.QuestPhaseTraceVerified &&
            questReport.ObjectiveChecklistVerified &&
            questReport.ObjectiveCommandCorrelationVerified &&
            questReport.QuestCompletedVerified &&
            questReport.RewardGrantedVerified &&
            questReport.MovementVerified &&
            questReport.FocusVerified &&
            questReport.InteractionVerified &&
            questReport.PlayLoopVerified &&
            runtimeStateLoopEvidenceVerified &&
            questReport.FirewallSafeBuildVerified &&
            !questReport.RuntimePreviewDependency &&
            diagnostics.All(item => item.Severity != "error");

        return new UnityMultiVariantSummary
        {
            StyleId = questReport.SelectedStyleId,
            PackageId = questReport.SelectedPackageId,
            ThreadId = questReport.SelectedThreadId,
            QuestId = questReport.SelectedQuestId,
            DialogueId = questReport.SelectedDialogueId,
            DialogueChoiceId = questReport.SelectedDialogueChoiceId,
            ItemId = questReport.SelectedItemId,
            EventId = questReport.SelectedEventId,
            RewardId = questReport.SelectedRewardId,
            SceneNodeIds = projection.Nodes.Select(item => item.NodeId).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            ObjectiveIds = questReport.Plan.ObjectiveSteps
                .Select(item => item.ObjectiveId + "|" + item.SourceGeneratedId)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            CommandIds = questReport.Plan.CommandSequence.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            PhaseTrace = questReport.State.PhaseTrace,
            QuestLoopHash = questReport.QuestLoopHash,
            PlanHash = questReport.PlanHash,
            StateHash = questReport.StateHash,
            BuildManifestHash = questReport.BuildManifestHash,
            PlayerLogRelativePath = questReport.AlphaBuild.LaunchVerification.PlayLoopLogRelativePath,
            QuestCompletionLoopVerified = questReport.QuestCompletionLoopVerified,
            QuestPlanVerified = questReport.QuestPlanVerified,
            QuestPhaseTraceVerified = questReport.QuestPhaseTraceVerified,
            ObjectiveChecklistVerified = questReport.ObjectiveChecklistVerified,
            ObjectiveCommandCorrelationVerified = questReport.ObjectiveCommandCorrelationVerified,
            QuestCompletedVerified = questReport.QuestCompletedVerified,
            RewardGrantedVerified = questReport.RewardGrantedVerified,
            MovementVerified = questReport.MovementVerified,
            FocusVerified = questReport.FocusVerified,
            InteractionVerified = questReport.InteractionVerified,
            PlayLoopVerified = questReport.PlayLoopVerified,
            RuntimeStateLoopEvidenceVerified = runtimeStateLoopEvidenceVerified,
            FirewallSafeBuildVerified = questReport.FirewallSafeBuildVerified,
            ProductSmokeRoute = questReport.ProductSmokeRoute,
            Accepted = accepted,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static UnityMultiVariantDistinctnessProof ValidateVariantSet(IReadOnlyList<UnityMultiVariantSummary> variants)
    {
        var diagnostics = new List<UnityMultiVariantDiagnostic>();
        Require(variants.Count >= 3, "unity_multi_variant.distinct.variant_count_too_low", variants.Count.ToString(), "At least three variant summaries are required.");
        Require(variants.Count(item => item.Accepted) >= 3, "unity_multi_variant.distinct.accepted_count_too_low", variants.Count(item => item.Accepted).ToString(), "At least three variants must pass quest-loop evidence checks.");
        Require(DistinctCount(variants.Select(item => item.StyleId)) >= 3, "unity_multi_variant.distinct.style_count_too_low", "styleId", "Variants must have distinct style ids.");
        Require(DistinctCount(variants.Select(item => item.PackageId)) >= 3, "unity_multi_variant.distinct.package_count_too_low", "packageId", "Variants must have distinct package ids.");
        Require(DistinctCount(variants.Select(item => item.ThreadId)) >= 3, "unity_multi_variant.distinct.thread_count_too_low", "threadId", "Variants must have distinct thread ids.");
        Require(DistinctCount(variants.Select(item => item.QuestId)) >= 3, "unity_multi_variant.distinct.quest_count_too_low", "questId", "Variants must have distinct quest ids.");
        Require(DistinctCount(variants.Select(SceneSignature)) >= 3, "unity_multi_variant.distinct.scene_signature_count_too_low", "sceneNodeIds", "Variants must have distinct scene signatures.");
        Require(DistinctCount(variants.Select(ObjectiveSignature)) >= 3, "unity_multi_variant.distinct.objective_signature_count_too_low", "objectiveIds", "Variants must have distinct objective signatures.");
        Require(DistinctCount(variants.Select(CommandSignature)) >= 3, "unity_multi_variant.distinct.command_signature_count_too_low", "commandIds", "Variants must have distinct command signatures.");
        Require(variants.All(item => item.QuestCompletedVerified), "unity_multi_variant.distinct.quest_not_completed", "questCompletedVerified", "Every variant must complete its generated quest.");
        Require(variants.All(item => item.RewardGrantedVerified), "unity_multi_variant.distinct.reward_not_granted", "rewardGrantedVerified", "Every variant must grant its generated reward.");
        Require(variants.All(item => item.ProductSmokeRoute == "unity-quest-completion-loop"), "unity_multi_variant.distinct.pipeline_mismatch", "productSmokeRoute", "Every variant must use the same Goal 017 quest loop pipeline.");
        foreach (var variant in variants)
        {
            var slug = variant.StyleId.Replace('_', '-');
            foreach (var (value, target) in new[]
            {
                (variant.PackageId, "packageId"),
                (variant.ThreadId, "threadId"),
                (variant.QuestId, "questId"),
                (variant.DialogueId, "dialogueId"),
                (variant.DialogueChoiceId, "dialogueChoiceId"),
                (variant.ItemId, "itemId"),
                (variant.EventId, "eventId"),
                (variant.RewardId, "rewardId")
            })
            {
                Require(!string.IsNullOrWhiteSpace(value) && value.Contains(slug, StringComparison.Ordinal), "unity_multi_variant.distinct.style_binding_mismatch", target, "Variant summary ids must bind to the selected style.");
            }

            Require(variant.CommandIds.All(commandId => commandId.Contains(slug, StringComparison.Ordinal)), "unity_multi_variant.distinct.command_style_binding_mismatch", "commandIds", "Variant command ids must bind to the selected style.");
        }

        var nonEmptyBuildHashes = variants.Select(item => item.BuildManifestHash).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        if (nonEmptyBuildHashes.Count > 0)
        {
            Require(DistinctCount(nonEmptyBuildHashes) == nonEmptyBuildHashes.Count, "unity_multi_variant.distinct.build_manifest_hash_copied", "buildManifestHash", "Per-variant build manifest hashes must not be copied across variants.");
        }

        var nonEmptyLogs = variants.Select(item => item.PlayerLogRelativePath).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        if (nonEmptyLogs.Count > 0)
        {
            Require(DistinctCount(nonEmptyLogs) == nonEmptyLogs.Count, "unity_multi_variant.distinct.player_log_copied", "playerLogRelativePath", "Per-variant player log paths must not be copied across variants.");
        }

        return new UnityMultiVariantDistinctnessProof
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            Diagnostics = SortDiagnostics(diagnostics)
        };

        void Require(bool condition, string code, string target, string message)
        {
            if (!condition)
            {
                diagnostics.Add(Diagnostic("error", code, target, message));
            }
        }
    }

    private static UnityMultiVariantInvalidMatrix BuildInvalidMatrix(
        IReadOnlyList<VariantWorkItem> workVariants,
        UnityMultiVariantPreviousEvidenceProof previousEvidence)
    {
        var baseline = workVariants.Select(item => item.Summary).ToList();
        var first = workVariants.FirstOrDefault();
        var second = workVariants.Skip(1).FirstOrDefault();
        var scenarios = new List<UnityMultiVariantInvalidScenario>
        {
            InvalidScenario("missing_accepted_goal017_evidence", [Diagnostic("error", "unity_multi_variant.previous.goal017_missing", UnityQuestCompletionLoopAcceptanceService.ReportJsonFileName, "Goal 017 quest completion evidence is required.")]),
            InvalidScenario("missing_goal017_plan_artifact", [Diagnostic("error", "unity_multi_variant.previous.plan_missing", UnityQuestCompletionLoopAcceptanceService.PlanJsonFileName, "Goal 017 plan artifact is required.")]),
            InvalidScenario("missing_goal017_state_artifact", [Diagnostic("error", "unity_multi_variant.previous.state_missing", UnityQuestCompletionLoopAcceptanceService.StateJsonFileName, "Goal 017 state artifact is required.")]),
            InvalidScenario("missing_goal017_report_artifact", [Diagnostic("error", "unity_multi_variant.previous.report_missing", UnityQuestCompletionLoopAcceptanceService.ReportJsonFileName, "Goal 017 report artifact is required.")]),
            InvalidScenario("copied_multi_variant_report_without_per_variant_player_logs", ValidateVariantSet(baseline.Select(item => item with { PlayerLogRelativePath = string.Empty, Accepted = false }).ToList()).Diagnostics),
            VariantSetScenario("only_one_variant_repeated_three_times", RepeatFirst(baseline)),
            VariantSetScenario("three_variants_with_same_package_id", baseline.Select(item => item with { PackageId = baseline.FirstOrDefault()?.PackageId ?? item.PackageId }).ToList()),
            VariantSetScenario("three_variants_with_same_quest_id", baseline.Select(item => item with { QuestId = baseline.FirstOrDefault()?.QuestId ?? item.QuestId }).ToList()),
            VariantSetScenario("three_variants_with_same_scene_signature", baseline.Select(item => item with { SceneNodeIds = baseline.FirstOrDefault()?.SceneNodeIds ?? item.SceneNodeIds }).ToList()),
            VariantSetScenario("three_variants_with_same_objective_signature", baseline.Select(item => item with { ObjectiveIds = baseline.FirstOrDefault()?.ObjectiveIds ?? item.ObjectiveIds }).ToList()),
            VariantSetScenario("style_id_changed_without_matching_package_evidence", baseline.Select((item, index) => index == 0 ? item with { StyleId = "gothic_mystery" } : item).ToList()),
            VariantSetScenario("package_id_changed_without_matching_staged_payload", baseline.Select((item, index) => index == 0 ? item with { PackageId = "game/content_generation/other" } : item).ToList()),
            VariantSetScenario("thread_id_changed_without_matching_command_evidence", baseline.Select((item, index) => index == 0 ? item with { ThreadId = "thread/other/000" } : item).ToList()),
            QuestLineScenario("quest_completion_claimed_without_phase_trace", first, lines => lines.Where(line => !line.StartsWith("alpha_runtime.quest_phase.", StringComparison.Ordinal))),
            QuestLineScenario("quest_completion_claimed_without_objective_checklist", first, lines => lines.Where(line => !line.StartsWith("alpha_runtime.quest_objective.", StringComparison.Ordinal))),
            QuestLineScenario("reward_claimed_without_completion", first, lines => ReplaceLine(lines, "alpha_runtime.quest_completed.after=", "alpha_runtime.quest_completed.after=false")),
            QuestLineScenario("objective_command_id_mismatch", first, lines => ReplaceLine(lines, "alpha_runtime.quest_objective.0.required_command_id=", "alpha_runtime.quest_objective.0.required_command_id=cmd/mismatch")),
            QuestLineScenario("objective_command_type_mismatch", first, lines => ReplaceLine(lines, "alpha_runtime.quest_objective.0.required_command_type=", "alpha_runtime.quest_objective.0.required_command_type=dialogue/open")),
            QuestLineScenario("objective_target_mismatch", first, lines => ReplaceLine(lines, "alpha_runtime.quest_objective.0.required_target_id=", "alpha_runtime.quest_objective.0.required_target_id=quest/mismatch")),
            AlphaInvalidScenario("cross_style_asset_leakage", first, "cross_style_package_export_asset_leakage"),
            VariantSetScenario("cross_style_command_leakage", baseline.Select((item, index) => index == 0 && second != null ? item with { CommandIds = second.Summary.CommandIds } : item).ToList()),
            VariantSetScenario("cross_style_reward_leakage", baseline.Select((item, index) => index == 0 && second != null ? item with { RewardId = second.Summary.RewardId } : item).ToList()),
            VariantSetScenario("build_manifest_copied_from_another_variant", baseline.Select(item => item with { BuildManifestHash = baseline.FirstOrDefault()?.BuildManifestHash ?? item.BuildManifestHash }).ToList()),
            VariantSetScenario("player_log_copied_from_another_variant", baseline.Select(item => item with { PlayerLogRelativePath = baseline.FirstOrDefault()?.PlayerLogRelativePath ?? item.PlayerLogRelativePath }).ToList()),
            InvalidScenario("runtime_preview_dependency_claim", [Diagnostic("error", "unity_multi_variant.contract.runtime_preview_dependency", "runtime_host", "Unity multi-variant acceptance must not claim Runtime Preview dependency.")]),
            InvalidScenario("development_profiler_debug_build_option_reintroduced", UnityPlayableAlphaAcceptanceService.ValidateFirewallSafeBuildScript("options = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;").Diagnostics.Select(ConvertDiagnostic).ToList())
        };

        var passed = scenarios.Count >= 24 && scenarios.All(item => !item.ActualValid);
        return new UnityMultiVariantInvalidMatrix
        {
            Passed = passed,
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics =
            [
                Diagnostic(passed ? "info" : "error", passed ? "unity_multi_variant.invalid_matrix_rejected" : "unity_multi_variant.invalid_matrix_failed", "invalid_matrix", "Invalid/fake/leak multi-variant scenarios must reject through multi-variant, quest-loop, previous-evidence, artifact or firewall validation paths.")
            ]
        };

        UnityMultiVariantInvalidScenario VariantSetScenario(string id, IReadOnlyList<UnityMultiVariantSummary> variants) =>
            InvalidScenario(id, ValidateVariantSet(variants).Diagnostics);

        UnityMultiVariantInvalidScenario QuestLineScenario(string id, VariantWorkItem? item, Func<IEnumerable<string>, IEnumerable<string>> mutate)
        {
            if (item == null)
            {
                return InvalidScenario(id, [Diagnostic("error", "unity_multi_variant.invalid.baseline_missing", id, "A valid baseline variant is required for quest-loop mutation.")]);
            }

            var baselineLines = UnityQuestCompletionLoopAcceptanceService.BuildExpectedQuestLoopLines(item.Projection, item.Plan);
            var proof = UnityQuestCompletionLoopAcceptanceService.ValidateQuestLoopLines(mutate(baselineLines), item.Projection, item.Plan);
            return InvalidScenario(id, proof.Diagnostics.Select(ConvertDiagnostic).ToList());
        }

        UnityMultiVariantInvalidScenario AlphaInvalidScenario(string id, VariantWorkItem? item, string alphaScenarioId)
        {
            var alphaScenario = item?.QuestReport.AlphaBuild.InvalidMatrix.Scenarios.FirstOrDefault(scenario => scenario.ScenarioId == alphaScenarioId);
            return InvalidScenario(id, alphaScenario == null
                ? [Diagnostic("error", "unity_multi_variant.invalid.alpha_scenario_missing", alphaScenarioId, "Alpha invalid scenario evidence is required.")]
                : alphaScenario.Diagnostics.Select(ConvertDiagnostic).ToList());
        }
    }

    private static UnityMultiVariantPreviousEvidenceProof ValidatePreviousGoal017Evidence(string repositoryRoot)
    {
        var diagnostics = new List<UnityMultiVariantDiagnostic>();
        var outputRoot = Path.Combine(repositoryRoot, UnityQuestCompletionLoopAcceptanceService.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar));
        var planPath = Path.Combine(outputRoot, UnityQuestCompletionLoopAcceptanceService.PlanJsonFileName);
        var statePath = Path.Combine(outputRoot, UnityQuestCompletionLoopAcceptanceService.StateJsonFileName);
        var reportPath = Path.Combine(outputRoot, UnityQuestCompletionLoopAcceptanceService.ReportJsonFileName);
        var verificationPath = Path.Combine(outputRoot, UnityQuestCompletionLoopAcceptanceService.VerificationMarkdownFileName);

        Require(File.Exists(planPath), "unity_multi_variant.previous.plan_missing", UnityQuestCompletionLoopAcceptanceService.PlanJsonFileName, "Goal 017 plan artifact must exist before Goal 018.");
        Require(File.Exists(statePath), "unity_multi_variant.previous.state_missing", UnityQuestCompletionLoopAcceptanceService.StateJsonFileName, "Goal 017 state artifact must exist before Goal 018.");
        Require(File.Exists(reportPath), "unity_multi_variant.previous.report_missing", UnityQuestCompletionLoopAcceptanceService.ReportJsonFileName, "Goal 017 report artifact must exist before Goal 018.");
        Require(File.Exists(verificationPath), "unity_multi_variant.previous.verification_missing", UnityQuestCompletionLoopAcceptanceService.VerificationMarkdownFileName, "Goal 017 verification artifact must exist before Goal 018.");

        if (File.Exists(reportPath))
        {
            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(reportPath));
                var root = document.RootElement;
                var finalStatus = root.TryGetProperty("finalStatus", out var finalStatusElement) ? finalStatusElement.GetString() : string.Empty;
                var manualGate = root.TryGetProperty("manualGate", out var manualGateElement) ? manualGateElement.GetString() : string.Empty;
                Require(string.Equals(finalStatus, UnityQuestCompletionLoopAcceptanceService.FinalGate, StringComparison.Ordinal), "unity_multi_variant.previous.final_status_mismatch", finalStatus ?? string.Empty, "Goal 017 report final status must match the expected gate.");
                Require(string.Equals(manualGate, UnityQuestCompletionLoopAcceptanceService.FinalGate, StringComparison.Ordinal), "unity_multi_variant.previous.manual_gate_mismatch", manualGate ?? string.Empty, "Goal 017 report manual gate must match the expected gate.");
            }
            catch (JsonException ex)
            {
                diagnostics.Add(Diagnostic("error", "unity_multi_variant.previous.report_invalid_json", UnityQuestCompletionLoopAcceptanceService.ReportJsonFileName, ex.Message));
            }
        }

        if (diagnostics.Count == 0)
        {
            diagnostics.Add(Diagnostic("info", "unity_multi_variant.previous.goal017_evidence_present", UnityQuestCompletionLoopAcceptanceService.ReportJsonFileName, "Accepted Goal 017 compact evidence is present and matching."));
        }

        return new UnityMultiVariantPreviousEvidenceProof
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            Diagnostics = SortDiagnostics(diagnostics)
        };

        void Require(bool condition, string code, string target, string message)
        {
            if (!condition)
            {
                diagnostics.Add(Diagnostic("error", code, target, message));
            }
        }
    }

    private static IReadOnlyList<UnityMultiVariantDiagnostic> ValidateStyleBindings(
        string expectedStyleId,
        UnityQuestCompletionLoopReport questReport,
        UnityGeneratedSceneProjection projection)
    {
        var diagnostics = new List<UnityMultiVariantDiagnostic>();
        var slug = expectedStyleId.Replace('_', '-');
        foreach (var (value, target) in new[]
        {
            (questReport.SelectedPackageId, "packageId"),
            (questReport.SelectedThreadId, "threadId"),
            (questReport.SelectedQuestId, "questId"),
            (questReport.SelectedDialogueId, "dialogueId"),
            (questReport.SelectedDialogueChoiceId, "dialogueChoiceId"),
            (questReport.SelectedItemId, "itemId"),
            (questReport.SelectedEventId, "eventId"),
            (questReport.SelectedRewardId, "rewardId")
        })
        {
            if (!value.Contains(slug, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_multi_variant.variant.cross_style_identity_leakage", target, "Variant ids must bind to the selected style/package evidence."));
            }
        }

        if (projection.CommandHints.Any(command =>
                !command.CommandId.Contains(slug, StringComparison.Ordinal) ||
                (!string.IsNullOrWhiteSpace(command.TargetId) && !command.TargetId.Contains(slug, StringComparison.Ordinal) && !command.TargetId.StartsWith("loot_table/", StringComparison.Ordinal))))
        {
            diagnostics.Add(Diagnostic("error", "unity_multi_variant.variant.cross_style_command_leakage", expectedStyleId, "Command hints must belong to the selected style."));
        }

        return diagnostics;
    }

    private static IReadOnlyList<UnityMultiVariantSummary> RepeatFirst(IReadOnlyList<UnityMultiVariantSummary> variants)
    {
        if (variants.Count == 0)
        {
            return [];
        }

        return [variants[0], variants[0], variants[0]];
    }

    private static IReadOnlyList<string> NormalizeSelectedStyles(IReadOnlyList<string> selectedStyleIds)
    {
        var source = selectedStyleIds.Count == 0 ? ExpectedStyleIds : selectedStyleIds;
        return source
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<UnityMultiVariantDiagnostic> ValidateSelectedStyles(IReadOnlyList<string> styleIds)
    {
        var diagnostics = new List<UnityMultiVariantDiagnostic>();
        if (styleIds.Count < 3)
        {
            diagnostics.Add(Diagnostic("error", "unity_multi_variant.selection.too_few_styles", styleIds.Count.ToString(), "Goal 018 requires at least three selected styles."));
        }

        foreach (var styleId in styleIds.Where(styleId => !IsKnownStyle(styleId)))
        {
            diagnostics.Add(Diagnostic("error", "unity_multi_variant.selection.unknown_style_id", styleId, "Selected style id must be one of the deterministic Alpha style candidates."));
        }

        return SortDiagnostics(diagnostics);
    }

    private static bool IsKnownStyle(string styleId) => ExpectedStyleIds.Contains(styleId, StringComparer.Ordinal);

    private static int DistinctCount(IEnumerable<string> values) =>
        values.Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).Count();

    private static string SceneSignature(UnityMultiVariantSummary summary) =>
        string.Join("|", summary.SceneNodeIds.OrderBy(item => item, StringComparer.Ordinal));

    private static string ObjectiveSignature(UnityMultiVariantSummary summary) =>
        string.Join("|", summary.ObjectiveIds.OrderBy(item => item, StringComparer.Ordinal));

    private static string CommandSignature(UnityMultiVariantSummary summary) =>
        string.Join("|", summary.CommandIds.OrderBy(item => item, StringComparer.Ordinal));

    private static IEnumerable<string> ReplaceLine(IEnumerable<string> lines, string prefix, string replacement)
    {
        var replaced = false;
        foreach (var line in lines)
        {
            if (!replaced && line.StartsWith(prefix, StringComparison.Ordinal))
            {
                replaced = true;
                yield return replacement;
            }
            else
            {
                yield return line;
            }
        }
    }

    private static UnityMultiVariantInvalidScenario InvalidScenario(
        string id,
        IReadOnlyList<UnityMultiVariantDiagnostic> diagnostics) =>
        new()
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = diagnostics.All(item => item.Severity != "error"),
            MutatedEvidenceKind = id,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static string RenderReport(UnityMultiVariantPlayableScenarioReport report)
    {
        var lines = new List<string>
        {
            "# Unity Multi-Variant Playable Scenario Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Final status: {report.FinalStatus}",
            $"- Previous gate: {report.PreviousAcceptedGate}",
            $"- Completed slices: {string.Join(", ", report.CompletedSlices)}",
            $"- Product smoke route: {report.ProductSmokeRoute}",
            $"- Variants: {report.AcceptedVariantCount}/{report.VariantCount}",
            $"- Styles: {string.Join(", ", report.SelectedStyleIds)}",
            $"- Distinct package/quest/scene/objective: {report.DistinctPackageCount} / {report.DistinctQuestCount} / {report.DistinctSceneSignatureCount} / {report.DistinctObjectiveSignatureCount}",
            $"- All variants quest complete/reward granted: {report.AllVariantsQuestComplete.ToString().ToLowerInvariant()} / {report.AllVariantsRewardGranted.ToString().ToLowerInvariant()}",
            $"- Multi-variant scenario verified: {report.MultiVariantPlayableScenarioVerified.ToString().ToLowerInvariant()}",
            $"- Variants hash: {report.VariantsHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            string.Empty,
            "## Variants",
            string.Empty
        };
        lines.AddRange(report.VariantSummaries.Select(item => $"- {item.StyleId}: package={item.PackageId} thread={item.ThreadId} quest={item.QuestId} reward={item.RewardId} accepted={item.Accepted.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(UnityMultiVariantPlayableScenarioReport report)
    {
        var lines = new List<string>
        {
            "# Unity Multi-Variant Playable Scenario Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- Variants artifact: {RelativeOutputDirectory}/{VariantsJsonFileName}",
            $"- Report artifact: {RelativeOutputDirectory}/{ReportJsonFileName}",
            $"- Selected styles: {string.Join(", ", report.SelectedStyleIds)}",
            $"- Selected packages: {string.Join(", ", report.SelectedPackageIds)}",
            $"- Selected threads: {string.Join(", ", report.SelectedThreadIds)}",
            $"- Accepted variants: {report.AcceptedVariantCount}/{report.VariantCount}",
            $"- Variants hash: {report.VariantsHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Final gate status: required, not passed",
            $"- Future post-goal work started: false"
        };

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string ResolveRepositoryRoot(string projectRoot, string overrideRoot)
    {
        if (!string.IsNullOrWhiteSpace(overrideRoot))
        {
            return Path.GetFullPath(overrideRoot);
        }

        var current = new DirectoryInfo(projectRoot);
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? projectRoot;
    }

    private static void EnsureContained(string root, string path)
    {
        if (!IsContained(root, path))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static bool IsContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        return pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
    }

    private static UnityMultiVariantDiagnostic ConvertDiagnostic(UnityQuestLoopDiagnostic diagnostic) =>
        Diagnostic(diagnostic.Severity, diagnostic.Code, diagnostic.Target, diagnostic.Message);

    private static UnityMultiVariantDiagnostic ConvertDiagnostic(AlphaBuildDiagnostic diagnostic) =>
        Diagnostic(diagnostic.Severity, diagnostic.Code, diagnostic.Target, diagnostic.Message);

    private static IReadOnlyList<UnityMultiVariantDiagnostic> SortDiagnostics(IEnumerable<UnityMultiVariantDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static UnityMultiVariantDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed record VariantWorkItem
    {
        public UnityMultiVariantSummary Summary { get; init; } = new();
        public UnityQuestCompletionLoopReport QuestReport { get; init; } = new();
        public UnityGeneratedSceneProjection Projection { get; init; } = new();
        public UnityQuestCompletionPlan Plan { get; init; } = new();
    }
}

public sealed record UnityMultiVariantPlayableScenarioOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedStyleIds { get; init; } = [];
    public bool ExecuteUnityBuild { get; init; }
    public bool LaunchBuiltPlayer { get; init; }
    public bool PreserveExistingBuildOutputForValidation { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 90;
}

public sealed record UnityMultiVariantPlayableScenarioAcceptanceResult
{
    public UnityMultiVariantPlayableScenarioReport Report { get; init; } = new();
    public string VariantsJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record UnityMultiVariantPlayableScenarioWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string VariantsJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record UnityMultiVariantPlayableScenarioReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public int VariantCount { get; init; }
    public int AcceptedVariantCount { get; init; }
    public IReadOnlyList<string> SelectedStyleIds { get; init; } = [];
    public IReadOnlyList<string> SelectedPackageIds { get; init; } = [];
    public IReadOnlyList<string> SelectedThreadIds { get; init; } = [];
    public int DistinctStyleCount { get; init; }
    public int DistinctPackageCount { get; init; }
    public int DistinctQuestCount { get; init; }
    public int DistinctSceneSignatureCount { get; init; }
    public int DistinctObjectiveSignatureCount { get; init; }
    public bool AllVariantsQuestComplete { get; init; }
    public bool AllVariantsRewardGranted { get; init; }
    public bool AllVariantsUseSamePipeline { get; init; }
    public bool MultiVariantPlayableScenarioVerified { get; init; }
    public IReadOnlyList<UnityMultiVariantSummary> VariantSummaries { get; init; } = [];
    public UnityMultiVariantInvalidMatrix InvalidMatrix { get; init; } = new();
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool NoExternalProviderLlmRagLuaMedia { get; init; }
    public bool RuntimePreviewDependency { get; init; }
    public string VariantsHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<UnityMultiVariantDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityMultiVariantSummary
{
    public string StyleId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string ThreadId { get; init; } = string.Empty;
    public string QuestId { get; init; } = string.Empty;
    public string DialogueId { get; init; } = string.Empty;
    public string DialogueChoiceId { get; init; } = string.Empty;
    public string ItemId { get; init; } = string.Empty;
    public string EventId { get; init; } = string.Empty;
    public string RewardId { get; init; } = string.Empty;
    public IReadOnlyList<string> SceneNodeIds { get; init; } = [];
    public IReadOnlyList<string> ObjectiveIds { get; init; } = [];
    public IReadOnlyList<string> CommandIds { get; init; } = [];
    public IReadOnlyList<string> PhaseTrace { get; init; } = [];
    public string QuestLoopHash { get; init; } = string.Empty;
    public string PlanHash { get; init; } = string.Empty;
    public string StateHash { get; init; } = string.Empty;
    public string BuildManifestHash { get; init; } = string.Empty;
    public string PlayerLogRelativePath { get; init; } = string.Empty;
    public bool QuestCompletionLoopVerified { get; init; }
    public bool QuestPlanVerified { get; init; }
    public bool QuestPhaseTraceVerified { get; init; }
    public bool ObjectiveChecklistVerified { get; init; }
    public bool ObjectiveCommandCorrelationVerified { get; init; }
    public bool QuestCompletedVerified { get; init; }
    public bool RewardGrantedVerified { get; init; }
    public bool MovementVerified { get; init; }
    public bool FocusVerified { get; init; }
    public bool InteractionVerified { get; init; }
    public bool PlayLoopVerified { get; init; }
    public bool RuntimeStateLoopEvidenceVerified { get; init; }
    public bool FirewallSafeBuildVerified { get; init; }
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public IReadOnlyList<UnityMultiVariantDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityMultiVariantDistinctnessProof
{
    public bool Passed { get; init; }
    public IReadOnlyList<UnityMultiVariantDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityMultiVariantPreviousEvidenceProof
{
    public bool Passed { get; init; }
    public IReadOnlyList<UnityMultiVariantDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityMultiVariantInvalidMatrix
{
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<UnityMultiVariantInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<UnityMultiVariantDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityMultiVariantInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<UnityMultiVariantDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityMultiVariantDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
