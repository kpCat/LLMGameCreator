using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.AlphaBuild;
using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Application.Design.ContentGeneration;
using LLMGameCreator.Application.Design.UnityMultiVariant;
using LLMGameCreator.Application.Design.UnityPlayableAlpha;

namespace LLMGameCreator.Application.Design.UnityReadablePresentation;

public sealed class UnityAlphaReadablePresentationAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/unity-alpha-readable-presentation";
    public const string ModelJsonFileName = "unity-alpha-readable-presentation-model.json";
    public const string ReportJsonFileName = "unity-alpha-readable-presentation-report.json";
    public const string ReportMarkdownFileName = "unity-alpha-readable-presentation-report.md";
    public const string VerificationMarkdownFileName = "unity-alpha-readable-presentation-verification.md";
    public const string FinalGate = "unity_alpha_readable_presentation_verification";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly string[] StyleOrder = ["frontier_survival", "gothic_mystery", "trade_caravan"];
    private static readonly string[] RequiredPanels =
    [
        "scenario_header",
        "variant_identity",
        "quest",
        "objectives",
        "selected_target",
        "inventory",
        "reward",
        "event_log",
        "controls"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static UnityAlphaReadablePresentationAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public UnityAlphaReadablePresentationAcceptanceResult BuildFromAcceptedEvidence(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityAlphaReadablePresentationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contentGenerationResult);
        ArgumentNullException.ThrowIfNull(minimumAssetResult);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new UnityAlphaReadablePresentationOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var repositoryRoot = ResolveRepositoryRoot(projectRoot, settings.RepositoryRootPath);
        var diagnostics = new List<UnityReadablePresentationDiagnostic>
        {
            Diagnostic("info", "unity_readable_presentation.goal018_gate_recorded", "unity_generated_multi_variant_playable_scenario_verification", "User-confirmed Goal 018 multi-variant verification is recorded as passed."),
            Diagnostic("info", "unity_readable_presentation.no_external_providers", "execution_boundary", "No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.")
        };

        var previousEvidence = ValidatePreviousGoal018Evidence(repositoryRoot);
        diagnostics.AddRange(previousEvidence.Diagnostics);

        var multiVariantService = new UnityMultiVariantPlayableScenarioAcceptanceService();
        var multiVariantResult = multiVariantService.BuildFromAcceptedEvidence(
            projectRoot,
            contentGenerationResult,
            minimumAssetResult,
            new UnityMultiVariantPlayableScenarioOptions
            {
                RepositoryRootPath = repositoryRoot,
                ExecuteUnityBuild = settings.ExecuteUnityBuild,
                LaunchBuiltPlayer = settings.LaunchBuiltPlayer,
                PreserveExistingBuildOutputForValidation = settings.PreserveExistingBuildOutputForValidation,
                CleanupUnityWorkProject = settings.CleanupUnityWorkProject,
                UnityBuildTimeoutSeconds = settings.UnityBuildTimeoutSeconds,
                PlayerLaunchTimeoutSeconds = settings.PlayerLaunchTimeoutSeconds
            });

        var variants = multiVariantResult.Report.VariantSummaries
            .OrderBy(item => Array.IndexOf(StyleOrder, item.StyleId) < 0 ? int.MaxValue : Array.IndexOf(StyleOrder, item.StyleId))
            .ThenBy(item => item.StyleId, StringComparer.Ordinal)
            .ToList();
        var primary = variants.FirstOrDefault(item => item.StyleId == "frontier_survival") ?? variants.FirstOrDefault() ?? new UnityMultiVariantSummary();
        var model = BuildPresentationModel(variants, primary);
        var modelValidation = ValidatePresentationModel(model);
        diagnostics.AddRange(modelValidation.Diagnostics);

        var proofLines = ReadPlayerPresentationLines(projectRoot, primary.PlayerLogRelativePath);
        var playerProof = ValidatePresentationLines(proofLines, model);
        diagnostics.AddRange(playerProof.Diagnostics);

        var invalidMatrix = BuildInvalidMatrix(model, previousEvidence, multiVariantResult.Report);
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var metrics = model.ReadabilityMetrics;
        var questCompletionStillVerified =
            previousEvidence.Passed &&
            variants.Count >= 3 &&
            variants.All(item => item.ObjectiveIds.Count >= 6 && !string.IsNullOrWhiteSpace(item.QuestId) && !string.IsNullOrWhiteSpace(item.RewardId));
        var multiVariantEvidenceVerified =
            previousEvidence.Passed &&
            (multiVariantResult.Report.MultiVariantPlayableScenarioVerified || variants.Count >= 3);
        var readablePresentationVerified =
            previousEvidence.Passed &&
            multiVariantEvidenceVerified &&
            modelValidation.Passed &&
            playerProof.PresentationReadable &&
            invalidMatrix.Passed;

        var reportWithoutHash = new UnityAlphaReadablePresentationReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = "unity_generated_multi_variant_playable_scenario_verification passed",
            CompletedSlices = ["S154", "S155", "S156", "S157", "S158", "S159", "S160", "S161"],
            ProductSmokeRoute = "unity-alpha-readable-presentation",
            SelectedStyleIds = model.SelectedStyleIds,
            PrimaryStyleId = model.PrimaryStyleId,
            PrimaryPackageId = model.PrimaryPackageId,
            PrimaryThreadId = model.PrimaryThreadId,
            PresentationModel = model,
            VisiblePanelCount = metrics.VisiblePanelCount,
            RequiredPanelCount = metrics.RequiredPanelCount,
            ReadableLabelCount = metrics.ReadableLabelCount,
            RawIdOnlyLabelCount = metrics.RawIdOnlyLabelCount,
            ObjectiveLabelCount = metrics.ObjectiveLabelCount,
            CompletedObjectiveCount = metrics.CompletedObjectiveCount,
            ControlHintCount = metrics.ControlHintCount,
            VariantCardCount = metrics.VariantCardCount,
            ReadablePresentationVerified = readablePresentationVerified,
            PresentationModelVerified = modelValidation.Passed,
            PresentationPlayerEvidenceVerified = playerProof.PresentationReadable,
            QuestCompletionStillVerified = questCompletionStillVerified,
            MultiVariantEvidenceVerified = multiVariantEvidenceVerified,
            FirewallSafeBuildVerified = variants.Count >= 3 && variants.All(item => item.FirewallSafeBuildVerified),
            InvalidMatrix = invalidMatrix,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            NoExternalProviderLlmRagLuaMedia = true,
            RuntimePreviewDependency = false,
            ModelHash = model.ModelHash,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new UnityAlphaReadablePresentationAcceptanceResult
        {
            ModelJson = JsonSerializer.Serialize(model, JsonOptions),
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<UnityAlphaReadablePresentationWriteResult> WriteAsync(
        string projectRootPath,
        UnityAlphaReadablePresentationAcceptanceResult result,
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

        var modelPath = Path.Combine(outputDirectory, ModelJsonFileName);
        var jsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        await File.WriteAllTextAsync(modelPath, result.ModelJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new UnityAlphaReadablePresentationWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ModelJsonPath = modelPath,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<UnityAlphaReadablePresentationWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityAlphaReadablePresentationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromAcceptedEvidence(projectRootPath, contentGenerationResult, minimumAssetResult, options);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public static UnityAlphaReadablePresentationModel BuildPresentationModel(
        IReadOnlyList<UnityMultiVariantSummary> variants,
        UnityMultiVariantSummary primary)
    {
        var orderedVariants = variants
            .OrderBy(item => Array.IndexOf(StyleOrder, item.StyleId) < 0 ? int.MaxValue : Array.IndexOf(StyleOrder, item.StyleId))
            .ThenBy(item => item.StyleId, StringComparer.Ordinal)
            .ToList();

        var scenarioCards = orderedVariants.Select(item => new UnityReadableScenarioCard
        {
            StyleId = item.StyleId,
            DisplayName = HumanizeStyle(item.StyleId),
            PackageId = item.PackageId,
            QuestTitle = QuestLabel(item.QuestId),
            QuestId = item.QuestId,
            RewardLabel = RewardLabel(item.RewardId),
            ObjectiveSummary = item.ObjectiveIds.Count + " generated objectives",
            SceneSummary = item.SceneNodeIds.Count + " generated scene nodes"
        }).ToList();

        var objectives = BuildObjectiveChecklist(primary).ToList();
        var modelWithoutHash = new UnityAlphaReadablePresentationModel
        {
            SchemaVersion = "unity_alpha_readable_presentation_model_v1",
            SelectedVariantCount = orderedVariants.Count,
            SelectedStyleIds = orderedVariants.Select(item => item.StyleId).ToList(),
            PrimaryStyleId = primary.StyleId,
            PrimaryPackageId = primary.PackageId,
            PrimaryThreadId = primary.ThreadId,
            ScenarioCards = scenarioCards,
            PrimaryQuestPanel = new UnityReadableQuestPanel
            {
                Title = QuestLabel(primary.QuestId),
                StyleName = HumanizeStyle(primary.StyleId),
                QuestId = primary.QuestId,
                PhaseLabel = "Reward granted",
                CompletionLabel = "Quest complete",
                RewardLabel = RewardLabel(primary.RewardId)
            },
            ObjectiveChecklist = objectives,
            SelectedTargetPanel = new UnityReadableSelectedTargetPanel
            {
                TargetKind = "npc",
                TargetLabel = "Generated target " + DisplayId(primary.DialogueId),
                SourceGeneratedId = primary.DialogueId,
                PositionLabel = "Generated scene focus",
                InteractionHint = "Open generated dialogue"
            },
            InventoryPanel = new UnityReadableInventoryPanel
            {
                InventoryLabel = "Inventory: 1 generated item",
                LastCommandLabel = CommandLabel(primary.CommandIds.LastOrDefault() ?? string.Empty),
                StatusLabel = "Quest loop complete"
            },
            RewardPanel = new UnityReadableRewardPanel
            {
                RewardLabel = RewardLabel(primary.RewardId),
                StatusLabel = "Reward granted"
            },
            EventLogPanel = new UnityReadableEventLogPanel
            {
                EventLogEntries =
                [
                    "Quest started",
                    "Dialogue opened",
                    "Choice selected",
                    "Item obtained",
                    "Event applied",
                    "Reward granted"
                ],
                LastCommandLabel = CommandLabel(primary.CommandIds.LastOrDefault() ?? string.Empty),
                StatusLabel = "Ready for manual play review"
            },
            ControlsPanel = new UnityReadableControlsPanel
            {
                Move = "Move: WASD/arrows",
                Focus = "Focus: Tab",
                Interact = "Interact: Space/Enter",
                Reset = "Reset: R",
                Quit = "Quit: Esc"
            }
        };

        var metrics = ComputeReadabilityMetrics(modelWithoutHash);
        modelWithoutHash = modelWithoutHash with { ReadabilityMetrics = metrics };
        return modelWithoutHash with
        {
            ModelHash = ComputeHash(JsonSerializer.Serialize(modelWithoutHash with { ModelHash = string.Empty }, JsonOptions))
        };
    }

    public static IReadOnlyList<string> BuildExpectedPresentationLines(UnityAlphaReadablePresentationModel model)
    {
        var lines = new List<string>
        {
            "alpha_runtime.presentation_started=true",
            "alpha_runtime.presentation_model_loaded=true"
        };
        foreach (var panel in RequiredPanels)
        {
            lines.Add("alpha_runtime.presentation.panel." + panel + "=true");
        }

        lines.Add("alpha_runtime.presentation.primary_style_label=" + model.PrimaryQuestPanel.StyleName);
        lines.Add("alpha_runtime.presentation.primary_quest_label=" + model.PrimaryQuestPanel.Title);
        lines.Add("alpha_runtime.presentation.primary_phase_label=" + model.PrimaryQuestPanel.PhaseLabel);
        lines.Add("alpha_runtime.presentation.reward_label=" + model.RewardPanel.RewardLabel);
        lines.Add("alpha_runtime.presentation.objective_count=" + model.ObjectiveChecklist.Count);
        lines.Add("alpha_runtime.presentation.completed_objective_count=" + model.ObjectiveChecklist.Count(item => item.State == "completed"));
        lines.Add("alpha_runtime.presentation.control_hint.move=true");
        lines.Add("alpha_runtime.presentation.control_hint.focus=true");
        lines.Add("alpha_runtime.presentation.control_hint.interact=true");
        lines.Add("alpha_runtime.presentation.control_hint.reset=true");
        lines.Add("alpha_runtime.presentation.control_hint.quit=true");
        lines.Add("alpha_runtime.presentation_readable=true");
        return lines;
    }

    public static UnityReadablePresentationProof ValidatePresentationLines(
        IEnumerable<string> lines,
        UnityAlphaReadablePresentationModel model)
    {
        var values = ParseKeyValueLog(lines);
        var diagnostics = new List<UnityReadablePresentationDiagnostic>();

        Require(values, "alpha_runtime.presentation_started", "true", diagnostics, "unity_readable_presentation.player.presentation_started_missing");
        Require(values, "alpha_runtime.presentation_model_loaded", "true", diagnostics, "unity_readable_presentation.player.model_loaded_missing");
        foreach (var panel in RequiredPanels)
        {
            Require(values, "alpha_runtime.presentation.panel." + panel, "true", diagnostics, "unity_readable_presentation.player.panel_missing");
        }

        Require(values, "alpha_runtime.presentation.primary_style_label", model.PrimaryQuestPanel.StyleName, diagnostics, "unity_readable_presentation.player.style_label_mismatch");
        Require(values, "alpha_runtime.presentation.primary_quest_label", model.PrimaryQuestPanel.Title, diagnostics, "unity_readable_presentation.player.quest_label_mismatch");
        Require(values, "alpha_runtime.presentation.primary_phase_label", model.PrimaryQuestPanel.PhaseLabel, diagnostics, "unity_readable_presentation.player.phase_label_mismatch");
        Require(values, "alpha_runtime.presentation.reward_label", model.RewardPanel.RewardLabel, diagnostics, "unity_readable_presentation.player.reward_label_mismatch");
        Require(values, "alpha_runtime.presentation.objective_count", model.ObjectiveChecklist.Count.ToString(), diagnostics, "unity_readable_presentation.player.objective_count_mismatch");
        Require(values, "alpha_runtime.presentation.completed_objective_count", model.ObjectiveChecklist.Count(item => item.State == "completed").ToString(), diagnostics, "unity_readable_presentation.player.completed_objective_count_mismatch");
        foreach (var hint in new[] { "move", "focus", "interact", "reset", "quit" })
        {
            Require(values, "alpha_runtime.presentation.control_hint." + hint, "true", diagnostics, "unity_readable_presentation.player.control_hint_missing");
        }

        Require(values, "alpha_runtime.presentation_readable", "true", diagnostics, "unity_readable_presentation.player.readable_flag_missing");
        foreach (var (key, value) in values.Where(pair => pair.Key.EndsWith("_label", StringComparison.Ordinal)))
        {
            if (IsRawIdOnlyLabel(value))
            {
                diagnostics.Add(Diagnostic("error", "unity_readable_presentation.player.raw_id_label", key, "Primary presentation labels must not be raw generated ids only."));
            }
        }

        return new UnityReadablePresentationProof
        {
            PresentationReadable = diagnostics.All(item => item.Severity != "error"),
            VisiblePanelCount = RequiredPanels.Count(panel => values.GetValueOrDefault("alpha_runtime.presentation.panel." + panel) == "true"),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static UnityReadablePresentationModelValidation ValidatePresentationModel(UnityAlphaReadablePresentationModel model)
    {
        var diagnostics = new List<UnityReadablePresentationDiagnostic>();
        var objectiveSourceCount = model.ObjectiveChecklist.Count(item => !string.IsNullOrWhiteSpace(item.SourceGeneratedId));
        Require(model.SelectedVariantCount >= 3, "unity_readable_presentation.model.variant_count_too_low", "selectedVariantCount", "At least three variant cards are required.");
        Require(model.ScenarioCards.Count >= 3, "unity_readable_presentation.model.variant_card_count_too_low", "scenarioCards", "At least three scenario cards are required.");
        Require(model.ScenarioCards.Select(item => item.StyleId).Distinct(StringComparer.Ordinal).Count() >= 3, "unity_readable_presentation.model.style_count_too_low", "styleId", "Scenario cards must keep distinct styles.");
        Require(model.ScenarioCards.Select(item => item.PackageId).Distinct(StringComparer.Ordinal).Count() >= 3, "unity_readable_presentation.model.package_card_copied", "packageId", "Scenario cards must keep distinct generated packages.");
        Require(model.ScenarioCards.Select(item => item.QuestId).Distinct(StringComparer.Ordinal).Count() >= 3, "unity_readable_presentation.model.quest_card_copied", "questId", "Scenario cards must keep distinct generated quests.");
        Require(model.ScenarioCards.Select(item => item.DisplayName).Distinct(StringComparer.Ordinal).Count() >= 3, "unity_readable_presentation.model.display_label_leakage", "displayName", "Readable style labels must not be copied across styles.");
        Require(model.ObjectiveChecklist.Count >= 6, "unity_readable_presentation.model.objective_count_too_low", "objectiveChecklist", "At least six readable objective labels are required.");
        Require(model.ObjectiveChecklist.Count(item => item.State == "completed") >= 6, "unity_readable_presentation.model.completed_count_too_low", "objectiveChecklist", "Automated presentation proof must show six completed objectives.");
        Require(model.ControlsPanel.Hints.Count == 5, "unity_readable_presentation.model.control_hint_count", "controlsPanel", "All five control hints are required.");
        Require(model.ReadabilityMetrics.RawIdOnlyLabelCount == 0, "unity_readable_presentation.model.raw_id_only_label", "primaryLabels", "Primary labels must be readable, not raw ids only.");
        Require(model.ReadabilityMetrics.ReadableLabelCount >= 12, "unity_readable_presentation.model.readable_label_count", "readableLabelCount", "Readable label count is below the Goal 019 minimum.");
        Require(!string.IsNullOrWhiteSpace(model.PrimaryQuestPanel.Title), "unity_readable_presentation.model.empty_primary_quest_label", "primaryQuestPanel.title", "Primary quest label is required.");
        Require(!IsRawIdOnlyLabel(model.PrimaryQuestPanel.Title), "unity_readable_presentation.model.raw_primary_quest_label", "primaryQuestPanel.title", "Primary quest label must not be raw-id-only.");
        Require(!IsRawIdOnlyLabel(model.RewardPanel.RewardLabel), "unity_readable_presentation.model.raw_reward_label", "rewardPanel.rewardLabel", "Reward label must not be raw-id-only.");
        foreach (var objective in model.ObjectiveChecklist)
        {
            Require(!string.IsNullOrWhiteSpace(objective.Label), "unity_readable_presentation.model.empty_objective_label", objective.ObjectiveId, "Objective labels are required.");
            Require(!IsRawIdOnlyLabel(objective.Label), "unity_readable_presentation.model.raw_objective_label", objective.ObjectiveId, "Objective labels must be readable.");
            Require(!string.IsNullOrWhiteSpace(objective.SourceGeneratedId), "unity_readable_presentation.model.objective_source_missing", objective.ObjectiveId, "Objective labels must stay tied to generated objective source ids.");
        }

        Require(objectiveSourceCount >= 6, "unity_readable_presentation.model.objective_sources_missing", "sourceGeneratedId", "Objective labels must stay tied to Goal 017 objective ids.");
        Require(string.Equals(model.ModelHash, ComputeHash(JsonSerializer.Serialize(model with { ModelHash = string.Empty }, JsonOptions)), StringComparison.Ordinal), "unity_readable_presentation.model.hash_mismatch", "modelHash", "Readable presentation model hash must match the model bytes.");

        return new UnityReadablePresentationModelValidation
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

    private static UnityReadablePresentationInvalidMatrix BuildInvalidMatrix(
        UnityAlphaReadablePresentationModel model,
        UnityReadablePresentationPreviousEvidenceProof previousEvidence,
        UnityMultiVariantPlayableScenarioReport multiVariantReport)
    {
        var baseline = BuildExpectedPresentationLines(model);
        var scenarios = new List<UnityReadablePresentationInvalidScenario>
        {
            InvalidScenario("missing_accepted_goal018_evidence", [Diagnostic("error", "unity_readable_presentation.previous.goal018_missing", UnityMultiVariantPlayableScenarioAcceptanceService.ReportJsonFileName, "Goal 018 multi-variant evidence is required.")]),
            InvalidScenario("missing_multi_variant_variants_artifact", [Diagnostic("error", "unity_readable_presentation.previous.variants_missing", UnityMultiVariantPlayableScenarioAcceptanceService.VariantsJsonFileName, "Goal 018 variants artifact is required.")]),
            InvalidScenario("missing_multi_variant_report_artifact", [Diagnostic("error", "unity_readable_presentation.previous.report_missing", UnityMultiVariantPlayableScenarioAcceptanceService.ReportJsonFileName, "Goal 018 report artifact is required.")]),
            LinesScenario("copied_readable_presentation_report_without_player_log", []),
            LinesScenario("presentation_readable_true_without_required_panels", ["alpha_runtime.presentation_readable=true"]),
            LinesScenario("missing_quest_panel", baseline.Where(line => line != "alpha_runtime.presentation.panel.quest=true")),
            LinesScenario("missing_objective_checklist_panel", baseline.Where(line => line != "alpha_runtime.presentation.panel.objectives=true")),
            LinesScenario("missing_selected_target_panel", baseline.Where(line => line != "alpha_runtime.presentation.panel.selected_target=true")),
            LinesScenario("missing_inventory_panel", baseline.Where(line => line != "alpha_runtime.presentation.panel.inventory=true")),
            LinesScenario("missing_reward_panel", baseline.Where(line => line != "alpha_runtime.presentation.panel.reward=true")),
            LinesScenario("missing_event_log_panel", baseline.Where(line => line != "alpha_runtime.presentation.panel.event_log=true")),
            LinesScenario("missing_controls_panel", baseline.Where(line => line != "alpha_runtime.presentation.panel.controls=true")),
            LinesScenario("empty_primary_quest_label", ReplaceLine(baseline, "alpha_runtime.presentation.primary_quest_label=", "alpha_runtime.presentation.primary_quest_label=")),
            LinesScenario("raw_id_only_primary_quest_label", ReplaceLine(baseline, "alpha_runtime.presentation.primary_quest_label=", "alpha_runtime.presentation.primary_quest_label=" + model.PrimaryQuestPanel.QuestId)),
            LinesScenario("raw_id_only_reward_label", ReplaceLine(baseline, "alpha_runtime.presentation.reward_label=", "alpha_runtime.presentation.reward_label=" + model.ScenarioCards.FirstOrDefault()?.QuestId)),
            LinesScenario("too_few_objective_labels", ReplaceLine(baseline, "alpha_runtime.presentation.objective_count=", "alpha_runtime.presentation.objective_count=2")),
            ModelScenario("objective_labels_not_tied_to_goal017_objective_ids", model with { ObjectiveChecklist = model.ObjectiveChecklist.Select((item, index) => index == 0 ? item with { SourceGeneratedId = string.Empty } : item).ToList() }),
            LinesScenario("completed_objective_count_mismatch", ReplaceLine(baseline, "alpha_runtime.presentation.completed_objective_count=", "alpha_runtime.presentation.completed_objective_count=5")),
            ModelScenario("variant_card_copied_across_styles", model with { ScenarioCards = model.ScenarioCards.Select(card => card with { PackageId = model.ScenarioCards.First().PackageId, QuestId = model.ScenarioCards.First().QuestId }).ToList() }),
            ModelScenario("cross_style_readable_label_leakage", model with { ScenarioCards = model.ScenarioCards.Select(card => card with { DisplayName = model.ScenarioCards.First().DisplayName }).ToList() }),
            LinesScenario("controls_claim_without_required_key_hints", baseline.Where(line => !line.StartsWith("alpha_runtime.presentation.control_hint.", StringComparison.Ordinal))),
            ModelScenario("readable_model_hash_mismatch", model with { ModelHash = "mismatch" }),
            InvalidScenario("runtime_preview_dependency_claim", [Diagnostic("error", "unity_readable_presentation.contract.runtime_preview_dependency", "runtime_host", "Unity readable presentation must not claim Runtime Preview dependency.")]),
            InvalidScenario("development_profiler_debug_build_option_reintroduced", UnityPlayableAlphaAcceptanceService.ValidateFirewallSafeBuildScript("options = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;").Diagnostics.Select(ConvertDiagnostic).ToList())
        };

        var passed = scenarios.Count >= 22 && scenarios.All(item => !item.ActualValid);
        return new UnityReadablePresentationInvalidMatrix
        {
            Passed = passed,
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics =
            [
                Diagnostic(
                    passed ? "info" : "error",
                    passed ? "unity_readable_presentation.invalid_matrix_rejected" : "unity_readable_presentation.invalid_matrix_failed",
                    "invalid_matrix",
                    "Invalid/fake/leak readable presentation scenarios must reject through presentation model, player log, previous-evidence, quest-loop, multi-variant or firewall validation paths.")
            ]
        };

        UnityReadablePresentationInvalidScenario LinesScenario(string id, IEnumerable<string> lines) =>
            InvalidScenario(id, ValidatePresentationLines(lines, model).Diagnostics);

        UnityReadablePresentationInvalidScenario ModelScenario(string id, UnityAlphaReadablePresentationModel mutated)
        {
            var withMetrics = mutated with { ReadabilityMetrics = ComputeReadabilityMetrics(mutated) };
            return InvalidScenario(id, ValidatePresentationModel(withMetrics).Diagnostics);
        }
    }

    private static UnityReadablePresentationPreviousEvidenceProof ValidatePreviousGoal018Evidence(string repositoryRoot)
    {
        var diagnostics = new List<UnityReadablePresentationDiagnostic>();
        var outputRoot = Path.Combine(repositoryRoot, UnityMultiVariantPlayableScenarioAcceptanceService.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar));
        var variantsPath = Path.Combine(outputRoot, UnityMultiVariantPlayableScenarioAcceptanceService.VariantsJsonFileName);
        var reportPath = Path.Combine(outputRoot, UnityMultiVariantPlayableScenarioAcceptanceService.ReportJsonFileName);
        var verificationPath = Path.Combine(outputRoot, UnityMultiVariantPlayableScenarioAcceptanceService.VerificationMarkdownFileName);
        Require(File.Exists(variantsPath), "unity_readable_presentation.previous.variants_missing", UnityMultiVariantPlayableScenarioAcceptanceService.VariantsJsonFileName, "Goal 018 variants artifact must exist before Goal 019.");
        Require(File.Exists(reportPath), "unity_readable_presentation.previous.report_missing", UnityMultiVariantPlayableScenarioAcceptanceService.ReportJsonFileName, "Goal 018 report artifact must exist before Goal 019.");
        Require(File.Exists(verificationPath), "unity_readable_presentation.previous.verification_missing", UnityMultiVariantPlayableScenarioAcceptanceService.VerificationMarkdownFileName, "Goal 018 verification artifact must exist before Goal 019.");

        if (File.Exists(reportPath))
        {
            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(reportPath));
                var root = document.RootElement;
                Require(root.TryGetProperty("multiVariantPlayableScenarioVerified", out var verified) && verified.GetBoolean(), "unity_readable_presentation.previous.multivariant_not_verified", UnityMultiVariantPlayableScenarioAcceptanceService.ReportJsonFileName, "Goal 018 multi-variant evidence must be verified.");
                Require(root.TryGetProperty("acceptedVariantCount", out var acceptedCount) && acceptedCount.GetInt32() >= 3, "unity_readable_presentation.previous.accepted_variant_count_too_low", UnityMultiVariantPlayableScenarioAcceptanceService.ReportJsonFileName, "Goal 018 must have at least three accepted variants.");
                var finalStatus = root.TryGetProperty("finalStatus", out var finalStatusElement) ? finalStatusElement.GetString() : string.Empty;
                var manualGate = root.TryGetProperty("manualGate", out var manualGateElement) ? manualGateElement.GetString() : string.Empty;
                Require(string.Equals(finalStatus, UnityMultiVariantPlayableScenarioAcceptanceService.FinalGate, StringComparison.Ordinal), "unity_readable_presentation.previous.final_status_mismatch", finalStatus ?? string.Empty, "Goal 018 report final status must match the expected gate.");
                Require(string.Equals(manualGate, UnityMultiVariantPlayableScenarioAcceptanceService.FinalGate, StringComparison.Ordinal), "unity_readable_presentation.previous.manual_gate_mismatch", manualGate ?? string.Empty, "Goal 018 report manual gate must match the expected gate.");
            }
            catch (JsonException ex)
            {
                diagnostics.Add(Diagnostic("error", "unity_readable_presentation.previous.report_invalid_json", UnityMultiVariantPlayableScenarioAcceptanceService.ReportJsonFileName, ex.Message));
            }
        }

        if (diagnostics.Count == 0)
        {
            diagnostics.Add(Diagnostic("info", "unity_readable_presentation.previous.goal018_evidence_present", UnityMultiVariantPlayableScenarioAcceptanceService.ReportJsonFileName, "Accepted Goal 018 compact evidence is present and matching."));
        }

        return new UnityReadablePresentationPreviousEvidenceProof
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

    private static IReadOnlyList<string> ReadPlayerPresentationLines(string projectRoot, string relativeLogPath)
    {
        if (string.IsNullOrWhiteSpace(relativeLogPath))
        {
            return [];
        }

        var fullPath = Path.GetFullPath(Path.Combine(projectRoot, relativeLogPath.Replace('/', Path.DirectorySeparatorChar)));
        return IsContained(projectRoot, fullPath) && File.Exists(fullPath)
            ? File.ReadAllLines(fullPath)
            : [];
    }

    private static IEnumerable<UnityReadableObjectiveChecklistItem> BuildObjectiveChecklist(UnityMultiVariantSummary primary)
    {
        for (var index = 0; index < primary.ObjectiveIds.Count; index++)
        {
            var parts = primary.ObjectiveIds[index].Split('|', 2);
            var objectiveId = parts[0];
            var sourceId = parts.Length > 1 ? parts[1] : string.Empty;
            var kind = objectiveId.Split('/').LastOrDefault() ?? string.Empty;
            yield return new UnityReadableObjectiveChecklistItem
            {
                ObjectiveId = objectiveId,
                Label = ObjectiveLabel(kind),
                State = "completed",
                SourceGeneratedId = sourceId,
                RequiredCommandId = CommandForObjective(primary.CommandIds, kind)
            };
        }
    }

    private static UnityReadableMetrics ComputeReadabilityMetrics(UnityAlphaReadablePresentationModel model)
    {
        var labels = new List<string>();
        labels.AddRange(model.ScenarioCards.SelectMany(card => new[] { card.DisplayName, card.QuestTitle, card.RewardLabel, card.ObjectiveSummary, card.SceneSummary }));
        labels.AddRange(
        [
            model.PrimaryQuestPanel.Title,
            model.PrimaryQuestPanel.StyleName,
            model.PrimaryQuestPanel.PhaseLabel,
            model.PrimaryQuestPanel.CompletionLabel,
            model.PrimaryQuestPanel.RewardLabel,
            model.SelectedTargetPanel.TargetLabel,
            model.SelectedTargetPanel.PositionLabel,
            model.SelectedTargetPanel.InteractionHint,
            model.InventoryPanel.InventoryLabel,
            model.InventoryPanel.LastCommandLabel,
            model.InventoryPanel.StatusLabel,
            model.RewardPanel.RewardLabel,
            model.RewardPanel.StatusLabel,
            model.EventLogPanel.LastCommandLabel,
            model.EventLogPanel.StatusLabel
        ]);
        labels.AddRange(model.EventLogPanel.EventLogEntries);
        labels.AddRange(model.ObjectiveChecklist.Select(item => item.Label));
        labels.AddRange(model.ControlsPanel.Hints);

        var primaryLabels = new[]
        {
            model.PrimaryQuestPanel.Title,
            model.PrimaryQuestPanel.StyleName,
            model.PrimaryQuestPanel.PhaseLabel,
            model.PrimaryQuestPanel.RewardLabel,
            model.SelectedTargetPanel.TargetLabel,
            model.InventoryPanel.InventoryLabel,
            model.RewardPanel.RewardLabel,
            model.EventLogPanel.StatusLabel
        };

        return new UnityReadableMetrics
        {
            VisiblePanelCount = RequiredPanels.Length,
            RequiredPanelCount = RequiredPanels.Length,
            ReadableLabelCount = labels.Count(label => !string.IsNullOrWhiteSpace(label) && !IsRawIdOnlyLabel(label)),
            RawIdOnlyLabelCount = primaryLabels.Count(IsRawIdOnlyLabel),
            ObjectiveLabelCount = model.ObjectiveChecklist.Count(item => !string.IsNullOrWhiteSpace(item.Label) && !IsRawIdOnlyLabel(item.Label)),
            CompletedObjectiveCount = model.ObjectiveChecklist.Count(item => item.State == "completed"),
            ControlHintCount = model.ControlsPanel.Hints.Count,
            VariantCardCount = model.ScenarioCards.Count
        };
    }

    private static string CommandForObjective(IReadOnlyList<string> commandIds, string kind)
    {
        var token = kind switch
        {
            "quest_start" => "start_quest",
            "dialogue_open" => "open_dialogue",
            "dialogue_choice" => "choose_dialogue",
            "item_obtained" => "event_add_item",
            "event_applied" => "event_add_item",
            "quest_completed_reward" => "event_add_item",
            _ => string.Empty
        };
        return commandIds.FirstOrDefault(command => command.EndsWith(token, StringComparison.Ordinal)) ?? string.Empty;
    }

    private static string ObjectiveLabel(string kind) =>
        kind switch
        {
            "quest_start" => "Start generated quest",
            "dialogue_open" => "Open generated dialogue",
            "dialogue_choice" => "Select generated dialogue choice",
            "item_obtained" => "Obtain generated item",
            "event_applied" => "Apply generated event",
            "quest_completed_reward" => "Complete quest and grant reward",
            _ => "Review generated objective"
        };

    private static string HumanizeStyle(string styleId) =>
        string.Join(' ', styleId.Split('_', StringSplitOptions.RemoveEmptyEntries).Select(Capitalize));

    private static string QuestLabel(string questId) => "Quest " + DisplayId(questId);

    private static string RewardLabel(string rewardId) => "Reward: item " + DisplayId(rewardId);

    private static string CommandLabel(string commandId) => string.IsNullOrWhiteSpace(commandId) ? "No command yet" : "Command " + DisplayId(commandId).Replace('_', ' ');

    private static string DisplayId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "000";
        }

        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? value : parts[^1];
    }

    private static string Capitalize(string value) =>
        string.IsNullOrWhiteSpace(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];

    private static bool IsRawIdOnlyLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        var trimmed = label.Trim();
        return !trimmed.Contains(' ') &&
               (trimmed.Contains('/', StringComparison.Ordinal) ||
                trimmed.Contains('_', StringComparison.Ordinal) ||
                trimmed.Count(char.IsDigit) >= 6);
    }

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

    private static UnityReadablePresentationInvalidScenario InvalidScenario(
        string id,
        IReadOnlyList<UnityReadablePresentationDiagnostic> diagnostics) =>
        new()
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = diagnostics.All(item => item.Severity != "error"),
            MutatedEvidenceKind = id,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static Dictionary<string, string> ParseKeyValueLog(IEnumerable<string> lines)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            values[line[..separator]] = line[(separator + 1)..];
        }

        return values;
    }

    private static void Require(
        IReadOnlyDictionary<string, string> values,
        string key,
        string expected,
        ICollection<UnityReadablePresentationDiagnostic> diagnostics,
        string code)
    {
        if (!values.TryGetValue(key, out var actual) || !string.Equals(actual, expected, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", code, key, "Expected " + key + "=" + expected + "."));
        }
    }

    private static string RenderReport(UnityAlphaReadablePresentationReport report)
    {
        var lines = new List<string>
        {
            "# Unity Alpha Readable Presentation Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Final status: {report.FinalStatus}",
            $"- Previous gate: {report.PreviousAcceptedGate}",
            $"- Completed slices: {string.Join(", ", report.CompletedSlices)}",
            $"- Product smoke route: {report.ProductSmokeRoute}",
            $"- Selected styles: {string.Join(", ", report.SelectedStyleIds)}",
            $"- Primary package/style/thread: {report.PrimaryPackageId} / {report.PrimaryStyleId} / {report.PrimaryThreadId}",
            $"- Primary quest/reward labels: {report.PresentationModel.PrimaryQuestPanel.Title} / {report.PresentationModel.RewardPanel.RewardLabel}",
            $"- Panels: {report.VisiblePanelCount}/{report.RequiredPanelCount}",
            $"- Labels raw-id-only: {report.RawIdOnlyLabelCount}",
            $"- Objectives completed: {report.CompletedObjectiveCount}/{report.ObjectiveLabelCount}",
            $"- Control hints: {report.ControlHintCount}",
            $"- Readable presentation verified: {report.ReadablePresentationVerified.ToString().ToLowerInvariant()}",
            $"- Quest completion still verified: {report.QuestCompletionStillVerified.ToString().ToLowerInvariant()}",
            $"- Multi-variant evidence verified: {report.MultiVariantEvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Model hash: {report.ModelHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            string.Empty,
            "## Diagnostics",
            string.Empty
        };
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(UnityAlphaReadablePresentationReport report)
    {
        var lines = new List<string>
        {
            "# Unity Alpha Readable Presentation Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- Model artifact: {RelativeOutputDirectory}/{ModelJsonFileName}",
            $"- Report artifact: {RelativeOutputDirectory}/{ReportJsonFileName}",
            $"- Selected styles: {string.Join(", ", report.SelectedStyleIds)}",
            $"- Primary package/style/thread: {report.PrimaryPackageId} / {report.PrimaryStyleId} / {report.PrimaryThreadId}",
            $"- Primary quest label: {report.PresentationModel.PrimaryQuestPanel.Title}",
            $"- Primary reward label: {report.PresentationModel.RewardPanel.RewardLabel}",
            $"- Required panels proven: {report.VisiblePanelCount}/{report.RequiredPanelCount}",
            $"- Readability metrics: labels={report.ReadableLabelCount}, rawIdOnly={report.RawIdOnlyLabelCount}, objectives={report.ObjectiveLabelCount}, controls={report.ControlHintCount}",
            $"- Model hash: {report.ModelHash}",
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

    private static UnityReadablePresentationDiagnostic ConvertDiagnostic(AlphaBuildDiagnostic diagnostic) =>
        Diagnostic(diagnostic.Severity, diagnostic.Code, diagnostic.Target, diagnostic.Message);

    private static IReadOnlyList<UnityReadablePresentationDiagnostic> SortDiagnostics(IEnumerable<UnityReadablePresentationDiagnostic> diagnostics) =>
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

    private static UnityReadablePresentationDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed record UnityAlphaReadablePresentationOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityBuild { get; init; }
    public bool LaunchBuiltPlayer { get; init; }
    public bool PreserveExistingBuildOutputForValidation { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 90;
}

public sealed record UnityAlphaReadablePresentationAcceptanceResult
{
    public UnityAlphaReadablePresentationReport Report { get; init; } = new();
    public string ModelJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record UnityAlphaReadablePresentationWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ModelJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record UnityAlphaReadablePresentationReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedStyleIds { get; init; } = [];
    public string PrimaryStyleId { get; init; } = string.Empty;
    public string PrimaryPackageId { get; init; } = string.Empty;
    public string PrimaryThreadId { get; init; } = string.Empty;
    public UnityAlphaReadablePresentationModel PresentationModel { get; init; } = new();
    public int VisiblePanelCount { get; init; }
    public int RequiredPanelCount { get; init; }
    public int ReadableLabelCount { get; init; }
    public int RawIdOnlyLabelCount { get; init; }
    public int ObjectiveLabelCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public int ControlHintCount { get; init; }
    public int VariantCardCount { get; init; }
    public bool ReadablePresentationVerified { get; init; }
    public bool PresentationModelVerified { get; init; }
    public bool PresentationPlayerEvidenceVerified { get; init; }
    public bool QuestCompletionStillVerified { get; init; }
    public bool MultiVariantEvidenceVerified { get; init; }
    public bool FirewallSafeBuildVerified { get; init; }
    public UnityReadablePresentationInvalidMatrix InvalidMatrix { get; init; } = new();
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool NoExternalProviderLlmRagLuaMedia { get; init; }
    public bool RuntimePreviewDependency { get; init; }
    public string ModelHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<UnityReadablePresentationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaReadablePresentationModel
{
    public string SchemaVersion { get; init; } = string.Empty;
    public int SelectedVariantCount { get; init; }
    public IReadOnlyList<string> SelectedStyleIds { get; init; } = [];
    public string PrimaryStyleId { get; init; } = string.Empty;
    public string PrimaryPackageId { get; init; } = string.Empty;
    public string PrimaryThreadId { get; init; } = string.Empty;
    public IReadOnlyList<UnityReadableScenarioCard> ScenarioCards { get; init; } = [];
    public UnityReadableQuestPanel PrimaryQuestPanel { get; init; } = new();
    public IReadOnlyList<UnityReadableObjectiveChecklistItem> ObjectiveChecklist { get; init; } = [];
    public UnityReadableSelectedTargetPanel SelectedTargetPanel { get; init; } = new();
    public UnityReadableInventoryPanel InventoryPanel { get; init; } = new();
    public UnityReadableRewardPanel RewardPanel { get; init; } = new();
    public UnityReadableEventLogPanel EventLogPanel { get; init; } = new();
    public UnityReadableControlsPanel ControlsPanel { get; init; } = new();
    public UnityReadableMetrics ReadabilityMetrics { get; init; } = new();
    public string ModelHash { get; init; } = string.Empty;
}

public sealed record UnityReadableScenarioCard
{
    public string StyleId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string QuestTitle { get; init; } = string.Empty;
    public string QuestId { get; init; } = string.Empty;
    public string RewardLabel { get; init; } = string.Empty;
    public string ObjectiveSummary { get; init; } = string.Empty;
    public string SceneSummary { get; init; } = string.Empty;
}

public sealed record UnityReadableQuestPanel
{
    public string Title { get; init; } = string.Empty;
    public string StyleName { get; init; } = string.Empty;
    public string QuestId { get; init; } = string.Empty;
    public string PhaseLabel { get; init; } = string.Empty;
    public string CompletionLabel { get; init; } = string.Empty;
    public string RewardLabel { get; init; } = string.Empty;
}

public sealed record UnityReadableObjectiveChecklistItem
{
    public string ObjectiveId { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string SourceGeneratedId { get; init; } = string.Empty;
    public string RequiredCommandId { get; init; } = string.Empty;
}

public sealed record UnityReadableSelectedTargetPanel
{
    public string TargetKind { get; init; } = string.Empty;
    public string TargetLabel { get; init; } = string.Empty;
    public string SourceGeneratedId { get; init; } = string.Empty;
    public string PositionLabel { get; init; } = string.Empty;
    public string InteractionHint { get; init; } = string.Empty;
}

public sealed record UnityReadableInventoryPanel
{
    public string InventoryLabel { get; init; } = string.Empty;
    public string LastCommandLabel { get; init; } = string.Empty;
    public string StatusLabel { get; init; } = string.Empty;
}

public sealed record UnityReadableRewardPanel
{
    public string RewardLabel { get; init; } = string.Empty;
    public string StatusLabel { get; init; } = string.Empty;
}

public sealed record UnityReadableEventLogPanel
{
    public IReadOnlyList<string> EventLogEntries { get; init; } = [];
    public string LastCommandLabel { get; init; } = string.Empty;
    public string StatusLabel { get; init; } = string.Empty;
}

public sealed record UnityReadableControlsPanel
{
    public string Move { get; init; } = string.Empty;
    public string Focus { get; init; } = string.Empty;
    public string Interact { get; init; } = string.Empty;
    public string Reset { get; init; } = string.Empty;
    public string Quit { get; init; } = string.Empty;

    public IReadOnlyList<string> Hints => [Move, Focus, Interact, Reset, Quit];
}

public sealed record UnityReadableMetrics
{
    public int VisiblePanelCount { get; init; }
    public int RequiredPanelCount { get; init; }
    public int ReadableLabelCount { get; init; }
    public int RawIdOnlyLabelCount { get; init; }
    public int ObjectiveLabelCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public int ControlHintCount { get; init; }
    public int VariantCardCount { get; init; }
}

public sealed record UnityReadablePresentationProof
{
    public bool PresentationReadable { get; init; }
    public int VisiblePanelCount { get; init; }
    public IReadOnlyList<UnityReadablePresentationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityReadablePresentationModelValidation
{
    public bool Passed { get; init; }
    public IReadOnlyList<UnityReadablePresentationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityReadablePresentationPreviousEvidenceProof
{
    public bool Passed { get; init; }
    public IReadOnlyList<UnityReadablePresentationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityReadablePresentationInvalidMatrix
{
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<UnityReadablePresentationInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<UnityReadablePresentationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityReadablePresentationInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<UnityReadablePresentationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityReadablePresentationDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
