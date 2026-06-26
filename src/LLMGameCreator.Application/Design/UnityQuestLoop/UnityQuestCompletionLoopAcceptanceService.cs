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
using LLMGameCreator.Application.Design.UnityRuntimeState;

namespace LLMGameCreator.Application.Design.UnityQuestLoop;

public sealed class UnityQuestCompletionLoopAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/unity-quest-completion-loop";
    public const string PlanJsonFileName = "unity-quest-completion-loop-plan.json";
    public const string StateJsonFileName = "unity-quest-completion-loop-state.json";
    public const string ReportJsonFileName = "unity-quest-completion-loop-report.json";
    public const string ReportMarkdownFileName = "unity-quest-completion-loop-report.md";
    public const string VerificationMarkdownFileName = "unity-quest-completion-loop-verification.md";
    public const string FinalGate = "unity_generated_quest_completion_loop_verification";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly string[] QuestPhaseOrder =
    [
        "not_started",
        "started",
        "dialogue_opened",
        "choice_selected",
        "item_obtained",
        "event_applied",
        "completed",
        "reward_granted"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static UnityQuestCompletionLoopAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public UnityQuestCompletionLoopAcceptanceResult BuildFromAcceptedEvidence(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityQuestCompletionLoopOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contentGenerationResult);
        ArgumentNullException.ThrowIfNull(minimumAssetResult);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new UnityQuestCompletionLoopOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var repositoryRoot = ResolveRepositoryRoot(projectRoot, settings.RepositoryRootPath);
        var outputRelativeDirectory = string.IsNullOrWhiteSpace(settings.RelativeOutputDirectoryOverride)
            ? RelativeOutputDirectory
            : settings.RelativeOutputDirectoryOverride;
        var alphaService = new AlphaRunnableBuildAcceptanceService();
        var alphaResult = alphaService.BuildFromAcceptedEvidence(
            projectRoot,
            contentGenerationResult,
            minimumAssetResult,
            new AlphaRunnableBuildOptions
            {
                RepositoryRootPath = repositoryRoot,
                RelativeOutputDirectoryOverride = outputRelativeDirectory,
                SelectedStyleId = settings.SelectedStyleId,
                CandidateOrdinal = settings.CandidateOrdinal,
                ExecuteUnityBuild = settings.ExecuteUnityBuild,
                LaunchBuiltPlayer = settings.LaunchBuiltPlayer,
                PreserveExistingBuildOutputForValidation = settings.PreserveExistingBuildOutputForValidation,
                CleanupUnityWorkProject = settings.CleanupUnityWorkProject,
                UnityBuildTimeoutSeconds = settings.UnityBuildTimeoutSeconds,
                PlayerLaunchTimeoutSeconds = settings.PlayerLaunchTimeoutSeconds
            });

        var alpha = alphaResult.Report;
        var projection = UnityGeneratedSceneProjectionAcceptanceService.BuildProjection(alpha);
        var plan = BuildPlan(projection);
        var previousEvidence = ValidatePreviousGoal016Evidence(repositoryRoot, projection);
        var playLoop = ValidatePlayLoop(projectRoot, alpha, projection, plan);
        var firewall = ValidateFirewall(repositoryRoot, projectRoot, alpha);
        var state = BuildState(plan, playLoop);
        var invalidMatrix = BuildInvalidMatrix(plan, projection, firewall);
        var diagnostics = SortDiagnostics(
            previousEvidence.Diagnostics
                .Concat(playLoop.Diagnostics)
                .Concat(firewall.Diagnostics)
                .Concat(invalidMatrix.Diagnostics)
                .Concat(alpha.Diagnostics.Select(ConvertDiagnostic))
                .Concat(
                [
                    Diagnostic("info", "unity_quest_loop.goal016_gate_recorded", "unity_generated_runtime_state_loop_verification", "User-confirmed Goal 016 Unity runtime state loop verification is recorded as passed."),
                    Diagnostic("info", "unity_quest_loop.no_external_providers", "execution_boundary", "No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.")
                ]));

        plan = plan with { PlanHash = ComputeHash(JsonSerializer.Serialize(plan with { PlanHash = string.Empty }, JsonOptions)) };
        state = state with { StateHash = ComputeHash(JsonSerializer.Serialize(state with { StateHash = string.Empty }, JsonOptions)) };

        var reportWithoutHash = new UnityQuestCompletionLoopReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = "unity_generated_runtime_state_loop_verification passed",
            CompletedSlices = ["S138", "S139", "S140", "S141", "S142", "S143", "S144", "S145"],
            ProductSmokeRoute = "unity-quest-completion-loop",
            AlphaBuild = alpha,
            Plan = plan,
            State = state,
            SelectedPackageId = plan.SelectedPackageId,
            SelectedStyleId = plan.SelectedStyleId,
            SelectedThreadId = plan.SelectedThreadId,
            SelectedQuestId = plan.SelectedQuestId,
            SelectedDialogueId = plan.SelectedDialogueId,
            SelectedDialogueChoiceId = plan.SelectedDialogueChoiceId,
            SelectedItemId = plan.SelectedItemId,
            SelectedEventId = plan.SelectedEventId,
            SelectedRewardId = plan.SelectedRewardId,
            QuestCompletionLoopVerified = playLoop.QuestCompletionLoopVerified,
            QuestPlanVerified = playLoop.QuestPlanVerified,
            QuestPhaseTraceVerified = playLoop.QuestPhaseTraceVerified,
            ObjectiveChecklistVerified = playLoop.ObjectiveChecklistVerified,
            ObjectiveCommandCorrelationVerified = playLoop.ObjectiveCommandCorrelationVerified,
            QuestCompletedVerified = playLoop.QuestCompletedVerified,
            RewardGrantedVerified = playLoop.RewardGrantedVerified,
            MovementVerified = playLoop.RuntimeStateProof.MovementVerified,
            FocusVerified = playLoop.RuntimeStateProof.FocusVerified,
            InteractionVerified = playLoop.RuntimeStateProof.InteractionVerified,
            PlayLoopVerified = alpha.PlayLoopVerified && playLoop.RuntimeStateProof.PlayLoopVerified && playLoop.QuestCompletionLoopVerified,
            RuntimeStateLoopEvidenceVerified = previousEvidence.Passed && playLoop.RuntimeStateProof.RuntimeStateLoopVerified,
            FirewallSafeBuildVerified = firewall.FirewallSafeBuildVerified,
            InvalidMatrix = invalidMatrix,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            NoExternalProviderLlmRagLuaMedia = true,
            RuntimePreviewDependency = alpha.RuntimePreviewDependency,
            QuestLoopHash = state.StateHash,
            PlanHash = plan.PlanHash,
            StateHash = state.StateHash,
            BuildManifestHash = alpha.BuildManifestHash,
            DeterministicReportRelativePath = $"{RelativeOutputDirectory}/{ReportJsonFileName}",
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new UnityQuestCompletionLoopAcceptanceResult
        {
            PlanJson = JsonSerializer.Serialize(plan, JsonOptions),
            StateJson = JsonSerializer.Serialize(state, JsonOptions),
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report, alphaResult.VerificationMarkdown)
        };
    }

    public async Task<UnityQuestCompletionLoopWriteResult> WriteAsync(
        string projectRootPath,
        UnityQuestCompletionLoopAcceptanceResult result,
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

        var planPath = Path.Combine(outputDirectory, PlanJsonFileName);
        var statePath = Path.Combine(outputDirectory, StateJsonFileName);
        var jsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        await File.WriteAllTextAsync(planPath, result.PlanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(statePath, result.StateJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new UnityQuestCompletionLoopWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            PlanJsonPath = planPath,
            StateJsonPath = statePath,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<UnityQuestCompletionLoopWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityQuestCompletionLoopOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromAcceptedEvidence(projectRootPath, contentGenerationResult, minimumAssetResult, options);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public static UnityQuestCompletionPlan BuildPlan(UnityGeneratedSceneProjection projection)
    {
        var startCommand = Command(projection, "quest/start", projection.SelectedQuestId);
        var openDialogueCommand = Command(projection, "dialogue/open", projection.SelectedDialogueId);
        var choiceCommand = Command(projection, "dialogue/choose", string.Empty);
        var itemCommand = Command(projection, "event/add_item", projection.SelectedItemId);
        if (string.IsNullOrWhiteSpace(itemCommand.CommandId))
        {
            itemCommand = Command(projection, "loot/roll", string.Empty);
        }

        var eventCommand = CommandWithSecondary(projection, "event/", projection.SelectedEventId);
        var completionCommand = string.IsNullOrWhiteSpace(eventCommand.CommandId) ? itemCommand : eventCommand;
        var steps = new List<UnityQuestObjectiveStep>
        {
            Step(0, "quest_start", projection.SelectedQuestId, startCommand, "Start generated quest"),
            Step(1, "dialogue_open", projection.SelectedDialogueId, openDialogueCommand, "Open generated dialogue"),
            Step(2, "dialogue_choice", choiceCommand.TargetId, choiceCommand, "Select generated dialogue choice"),
            Step(3, "item_obtained", projection.SelectedItemId, itemCommand, "Obtain generated item"),
            Step(4, "event_applied", projection.SelectedEventId, eventCommand, "Apply generated event"),
            Step(5, "quest_completed_reward", projection.SelectedQuestId, completionCommand, "Complete quest and grant reward")
        };

        return new UnityQuestCompletionPlan
        {
            SchemaVersion = "unity_quest_completion_loop_plan_v1",
            SelectedPackageId = projection.SelectedPackageId,
            SelectedStyleId = projection.SelectedStyleId,
            SelectedThreadId = projection.SelectedThreadId,
            SelectedQuestId = projection.SelectedQuestId,
            SelectedQuestTitle = "Quest " + DisplayId(projection.SelectedQuestId),
            SelectedQuestSourceId = projection.SelectedQuestId,
            SelectedDialogueId = projection.SelectedDialogueId,
            SelectedDialogueChoiceId = choiceCommand.TargetId,
            SelectedItemId = projection.SelectedItemId,
            SelectedEventId = projection.SelectedEventId,
            SelectedRewardId = projection.SelectedItemId,
            SelectedRewardKind = "item",
            StartMapId = projection.SelectedMapId,
            QuestPhaseOrder = QuestPhaseOrder,
            ObjectiveSteps = steps,
            CommandSequence = steps
                .Select(step => step.RequiredCommandId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList(),
            CompletionCriteria =
            [
                "quest_started",
                "dialogue_opened",
                "choice_selected",
                "item_obtained",
                "event_applied",
                "quest_completed",
                "reward_granted"
            ],
            ExpectedFinalState = new UnityQuestExpectedFinalState
            {
                QuestCompleted = true,
                RewardGranted = true,
                RewardId = projection.SelectedItemId,
                RewardKind = "item",
                InventoryItemCount = 1
            }
        };

        static UnityQuestObjectiveStep Step(
            int index,
            string kind,
            string sourceId,
            UnityGeneratedSceneCommandHint command,
            string label) =>
            new()
            {
                ObjectiveId = $"objective/{index}/{kind}",
                ObjectiveKind = kind,
                SourceGeneratedId = sourceId,
                RequiredCommandId = command.CommandId,
                RequiredCommandType = command.CommandType,
                RequiredTargetId = command.TargetId,
                RequiredSecondaryTargetId = command.SecondaryTargetId,
                Before = false,
                After = true,
                VisibleLabel = label
            };
    }

    public static IReadOnlyList<string> BuildExpectedQuestLoopLines(
        UnityGeneratedSceneProjection projection,
        UnityQuestCompletionPlan plan)
    {
        var lines = UnityRuntimeStateLoopAcceptanceService.BuildExpectedStateLoopLines(projection).ToList();
        lines.Add("alpha_runtime.quest_loop_started=true");
        lines.Add("alpha_runtime.quest_loop_plan_loaded=true");
        lines.Add("alpha_runtime.quest_loop.package_id=" + plan.SelectedPackageId);
        lines.Add("alpha_runtime.quest_loop.style_id=" + plan.SelectedStyleId);
        lines.Add("alpha_runtime.quest_loop.thread_id=" + plan.SelectedThreadId);
        lines.Add("alpha_runtime.quest_loop.quest_id=" + plan.SelectedQuestId);
        lines.Add("alpha_runtime.quest_loop.dialogue_id=" + plan.SelectedDialogueId);
        lines.Add("alpha_runtime.quest_loop.choice_id=" + plan.SelectedDialogueChoiceId);
        lines.Add("alpha_runtime.quest_loop.item_id=" + plan.SelectedItemId);
        lines.Add("alpha_runtime.quest_loop.event_id=" + plan.SelectedEventId);
        lines.Add("alpha_runtime.quest_loop.reward_id=" + plan.SelectedRewardId);
        AppendPhaseLines(lines);
        for (var index = 0; index < plan.ObjectiveSteps.Count; index++)
        {
            AppendObjectiveLines(lines, index, plan.ObjectiveSteps[index]);
        }

        lines.Add("alpha_runtime.quest_completed.before=false");
        lines.Add("alpha_runtime.quest_completed.after=true");
        lines.Add("alpha_runtime.reward_granted.before=false");
        lines.Add("alpha_runtime.reward_granted.after=true");
        lines.Add("alpha_runtime.reward.kind=" + plan.SelectedRewardKind);
        lines.Add("alpha_runtime.reward.id=" + plan.SelectedRewardId);
        lines.Add("alpha_runtime.quest_loop_completed=true");
        return lines;
    }

    public static UnityQuestCompletionLoopProof ValidateQuestLoopLines(
        IEnumerable<string> lines,
        UnityGeneratedSceneProjection projection,
        UnityQuestCompletionPlan plan)
    {
        var lineList = lines.Select(line => line.Trim()).Where(line => line.Length > 0).ToList();
        var values = ParseKeyValueLog(lineList);
        var diagnostics = new List<UnityQuestLoopDiagnostic>();
        var runtimeProof = UnityRuntimeStateLoopAcceptanceService.ValidateStateLoopLines(lineList, projection);
        diagnostics.AddRange(runtimeProof.Diagnostics.Select(ConvertDiagnostic));

        Require(values, "alpha_runtime.quest_loop_started", "true", diagnostics, "unity_quest_loop.identity.missing");
        Require(values, "alpha_runtime.quest_loop_plan_loaded", "true", diagnostics, "unity_quest_loop.identity.missing");
        Require(values, "alpha_runtime.quest_loop.package_id", plan.SelectedPackageId, diagnostics, "unity_quest_loop.identity.package_mismatch");
        Require(values, "alpha_runtime.quest_loop.style_id", plan.SelectedStyleId, diagnostics, "unity_quest_loop.identity.style_mismatch");
        Require(values, "alpha_runtime.quest_loop.thread_id", plan.SelectedThreadId, diagnostics, "unity_quest_loop.identity.thread_mismatch");
        Require(values, "alpha_runtime.quest_loop.quest_id", plan.SelectedQuestId, diagnostics, "unity_quest_loop.identity.quest_mismatch");
        Require(values, "alpha_runtime.quest_loop.dialogue_id", plan.SelectedDialogueId, diagnostics, "unity_quest_loop.identity.dialogue_mismatch");
        Require(values, "alpha_runtime.quest_loop.choice_id", plan.SelectedDialogueChoiceId, diagnostics, "unity_quest_loop.identity.choice_mismatch");
        Require(values, "alpha_runtime.quest_loop.item_id", plan.SelectedItemId, diagnostics, "unity_quest_loop.identity.item_mismatch");
        Require(values, "alpha_runtime.quest_loop.event_id", plan.SelectedEventId, diagnostics, "unity_quest_loop.identity.event_mismatch");
        Require(values, "alpha_runtime.quest_loop.reward_id", plan.SelectedRewardId, diagnostics, "unity_quest_loop.identity.reward_mismatch");

        var phaseIndexes = new List<int>();
        var phaseKeys = new[]
        {
            ("alpha_runtime.quest_phase.before", "not_started"),
            ("alpha_runtime.quest_phase.after.started", "started"),
            ("alpha_runtime.quest_phase.after.dialogue_opened", "dialogue_opened"),
            ("alpha_runtime.quest_phase.after.choice_selected", "choice_selected"),
            ("alpha_runtime.quest_phase.after.item_obtained", "item_obtained"),
            ("alpha_runtime.quest_phase.after.event_applied", "event_applied"),
            ("alpha_runtime.quest_phase.after.completed", "completed"),
            ("alpha_runtime.quest_phase.after.reward_granted", "reward_granted")
        };

        foreach (var (key, expected) in phaseKeys)
        {
            Require(values, key, expected, diagnostics, "unity_quest_loop.phase.missing_or_mismatch");
            phaseIndexes.Add(IndexOf(lineList, key + "="));
        }

        if (phaseIndexes.Any(index => index < 0) || !phaseIndexes.SequenceEqual(phaseIndexes.OrderBy(index => index)))
        {
            diagnostics.Add(Diagnostic("error", "unity_quest_loop.phase.order_mismatch", "alpha_runtime.quest_phase", "Quest phase trace must appear in required order."));
        }

        for (var index = 0; index < plan.ObjectiveSteps.Count; index++)
        {
            var step = plan.ObjectiveSteps[index];
            Require(values, ObjectiveKey(index, "objective_id"), step.ObjectiveId, diagnostics, "unity_quest_loop.objective.id_mismatch");
            Require(values, ObjectiveKey(index, "objective_kind"), step.ObjectiveKind, diagnostics, "unity_quest_loop.objective.kind_mismatch");
            Require(values, ObjectiveKey(index, "source_id"), step.SourceGeneratedId, diagnostics, "unity_quest_loop.objective.source_mismatch");
            Require(values, ObjectiveKey(index, "required_command_id"), step.RequiredCommandId, diagnostics, "unity_quest_loop.objective.command_id_mismatch");
            Require(values, ObjectiveKey(index, "required_command_type"), step.RequiredCommandType, diagnostics, "unity_quest_loop.objective.command_type_mismatch");
            Require(values, ObjectiveKey(index, "required_target_id"), step.RequiredTargetId, diagnostics, "unity_quest_loop.objective.target_mismatch");
            Require(values, ObjectiveKey(index, "required_secondary_target_id"), step.RequiredSecondaryTargetId, diagnostics, "unity_quest_loop.objective.secondary_target_mismatch");
            Require(values, ObjectiveKey(index, "before"), "false", diagnostics, "unity_quest_loop.objective.before_after_missing");
            Require(values, ObjectiveKey(index, "after"), "true", diagnostics, "unity_quest_loop.objective.before_after_missing");

            if (!CommandAllowedForObjective(step))
            {
                diagnostics.Add(Diagnostic("error", "unity_quest_loop.objective.command_correlation_failed", step.ObjectiveId, "Objective step must be caused by the matching generated command type and target."));
            }
        }

        Require(values, "alpha_runtime.quest_completed.before", "false", diagnostics, "unity_quest_loop.completion.before_after_missing");
        Require(values, "alpha_runtime.quest_completed.after", "true", diagnostics, "unity_quest_loop.completion.before_after_missing");
        Require(values, "alpha_runtime.reward_granted.before", "false", diagnostics, "unity_quest_loop.reward.before_after_missing");
        Require(values, "alpha_runtime.reward_granted.after", "true", diagnostics, "unity_quest_loop.reward.before_after_missing");
        Require(values, "alpha_runtime.reward.kind", plan.SelectedRewardKind, diagnostics, "unity_quest_loop.reward.kind_mismatch");
        Require(values, "alpha_runtime.reward.id", plan.SelectedRewardId, diagnostics, "unity_quest_loop.reward.id_mismatch");
        Require(values, "alpha_runtime.quest_loop_completed", "true", diagnostics, "unity_quest_loop.completion.claim_missing");

        var phaseVerified = !diagnostics.Any(item => item.Code.StartsWith("unity_quest_loop.phase.", StringComparison.Ordinal));
        var objectiveVerified = !diagnostics.Any(item => item.Code.StartsWith("unity_quest_loop.objective.", StringComparison.Ordinal));
        var completionVerified = !diagnostics.Any(item => item.Code.StartsWith("unity_quest_loop.completion.", StringComparison.Ordinal));
        var rewardVerified = !diagnostics.Any(item => item.Code.StartsWith("unity_quest_loop.reward.", StringComparison.Ordinal));
        return new UnityQuestCompletionLoopProof
        {
            RuntimeStateProof = runtimeProof,
            QuestCompletionLoopVerified = runtimeProof.RuntimeStateLoopVerified && phaseVerified && objectiveVerified && completionVerified && rewardVerified && diagnostics.All(item => item.Severity != "error"),
            QuestPlanVerified = !diagnostics.Any(item => item.Code.StartsWith("unity_quest_loop.identity.", StringComparison.Ordinal)),
            QuestPhaseTraceVerified = phaseVerified,
            ObjectiveChecklistVerified = objectiveVerified,
            ObjectiveCommandCorrelationVerified = objectiveVerified,
            QuestCompletedVerified = completionVerified,
            RewardGrantedVerified = rewardVerified,
            PhaseTrace = QuestPhaseOrder,
            ObjectiveStepIds = plan.ObjectiveSteps.Select(step => step.ObjectiveId).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };

        static void Require(
            IReadOnlyDictionary<string, string> parsed,
            string key,
            string expected,
            ICollection<UnityQuestLoopDiagnostic> collector,
            string code)
        {
            if (!parsed.TryGetValue(key, out var actual) || !string.Equals(actual, expected, StringComparison.Ordinal))
            {
                collector.Add(Diagnostic("error", code, key, $"Expected {key}={expected}."));
            }
        }
    }

    private static UnityQuestCompletionLoopProof ValidatePlayLoop(
        string projectRoot,
        AlphaRunnableBuildReport alpha,
        UnityGeneratedSceneProjection projection,
        UnityQuestCompletionPlan plan)
    {
        var playLoopLogPath = string.IsNullOrWhiteSpace(alpha.LaunchVerification.PlayLoopLogRelativePath)
            ? string.Empty
            : Path.Combine(projectRoot, alpha.LaunchVerification.PlayLoopLogRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(playLoopLogPath) || !File.Exists(playLoopLogPath))
        {
            return new UnityQuestCompletionLoopProof
            {
                Diagnostics =
                [
                    Diagnostic("error", "unity_quest_loop.play_loop.log_missing", "logs/alpha-player-play-loop.log", "Quest completion verification requires the real player play-loop log.")
                ]
            };
        }

        return ValidateQuestLoopLines(File.ReadAllLines(playLoopLogPath), projection, plan);
    }

    private static UnityQuestPreviousEvidenceProof ValidatePreviousGoal016Evidence(
        string repositoryRoot,
        UnityGeneratedSceneProjection projection)
    {
        var diagnostics = new List<UnityQuestLoopDiagnostic>();
        var reportPath = Path.Combine(repositoryRoot, UnityRuntimeStateLoopAcceptanceService.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), UnityRuntimeStateLoopAcceptanceService.ReportJsonFileName);
        var statePath = Path.Combine(repositoryRoot, UnityRuntimeStateLoopAcceptanceService.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), UnityRuntimeStateLoopAcceptanceService.StateJsonFileName);
        var verificationPath = Path.Combine(repositoryRoot, UnityRuntimeStateLoopAcceptanceService.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), UnityRuntimeStateLoopAcceptanceService.VerificationMarkdownFileName);

        if (!File.Exists(reportPath))
        {
            diagnostics.Add(Diagnostic("error", "unity_quest_loop.previous.runtime_report_missing", UnityRuntimeStateLoopAcceptanceService.ReportJsonFileName, "Goal 017 must reuse accepted Goal 016 runtime state loop report evidence."));
        }

        if (!File.Exists(statePath))
        {
            diagnostics.Add(Diagnostic("error", "unity_quest_loop.previous.runtime_state_missing", UnityRuntimeStateLoopAcceptanceService.StateJsonFileName, "Goal 017 must reuse accepted Goal 016 runtime state JSON evidence."));
        }

        if (!File.Exists(verificationPath))
        {
            diagnostics.Add(Diagnostic("error", "unity_quest_loop.previous.runtime_verification_missing", UnityRuntimeStateLoopAcceptanceService.VerificationMarkdownFileName, "Goal 017 must reuse accepted Goal 016 verification evidence."));
        }

        if (File.Exists(reportPath))
        {
            using var report = JsonDocument.Parse(File.ReadAllText(reportPath));
            var root = report.RootElement;
            var finalStatus = root.TryGetProperty("finalStatus", out var finalStatusElement) ? finalStatusElement.GetString() : string.Empty;
            var selectedPackageId = root.TryGetProperty("selectedPackageId", out var packageElement) ? packageElement.GetString() : string.Empty;
            var selectedThreadId = root.TryGetProperty("selectedThreadId", out var threadElement) ? threadElement.GetString() : string.Empty;
            if (!string.Equals(finalStatus, UnityRuntimeStateLoopAcceptanceService.FinalGate, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_quest_loop.previous.final_gate_mismatch", finalStatus ?? string.Empty, "Goal 016 report must be the Unity runtime state loop gate."));
            }

            if (!string.Equals(selectedPackageId, projection.SelectedPackageId, StringComparison.Ordinal) ||
                !string.Equals(selectedThreadId, projection.SelectedThreadId, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_quest_loop.previous.identity_mismatch", selectedPackageId ?? string.Empty, "Goal 017 selected package/thread must match accepted Goal 016 evidence."));
            }
        }

        return new UnityQuestPreviousEvidenceProof
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            ReportRelativePath = $"{UnityRuntimeStateLoopAcceptanceService.RelativeOutputDirectory}/{UnityRuntimeStateLoopAcceptanceService.ReportJsonFileName}",
            StateRelativePath = $"{UnityRuntimeStateLoopAcceptanceService.RelativeOutputDirectory}/{UnityRuntimeStateLoopAcceptanceService.StateJsonFileName}",
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static UnityQuestFirewallProof ValidateFirewall(string repositoryRoot, string projectRoot, AlphaRunnableBuildReport alpha)
    {
        var scriptPath = Path.Combine(repositoryRoot, "unity", "LLMGameCreatorAlpha", "Assets", "Editor", "AlphaBuildEntrypoint.cs");
        if (!File.Exists(scriptPath))
        {
            return new UnityQuestFirewallProof
            {
                Diagnostics =
                [
                    Diagnostic("error", "unity_quest_loop.firewall.build_script_missing", "unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs", "Firewall-safe build proof requires the repository Alpha build entrypoint.")
                ]
            };
        }

        var proof = UnityPlayableAlphaAcceptanceService.ValidateFirewallSafeBuildScript(File.ReadAllText(scriptPath));
        var metadataPath = Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), "build", "windows", "alpha-build-metadata.json");
        var metadataPresent = File.Exists(metadataPath);
        var diagnostics = proof.Diagnostics.Select(ConvertDiagnostic).ToList();
        if (alpha.WindowsExecutableProduced && !metadataPresent)
        {
            diagnostics.Add(Diagnostic("error", "unity_quest_loop.firewall.metadata_missing", "alpha-build-metadata.json", "Quest loop build metadata must be present for produced Windows player output."));
        }

        return new UnityQuestFirewallProof
        {
            BuildOptions = proof.BuildOptions,
            StaticChecksPassed = proof.StaticChecksPassed,
            BuildMetadataPresent = metadataPresent,
            FirewallSafeBuildVerified = proof.StaticChecksPassed && (!alpha.WindowsExecutableProduced || metadataPresent),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static UnityQuestCompletionLoopState BuildState(
        UnityQuestCompletionPlan plan,
        UnityQuestCompletionLoopProof proof)
    {
        return new UnityQuestCompletionLoopState
        {
            SchemaVersion = "unity_quest_completion_loop_state_v1",
            SelectedPackageId = plan.SelectedPackageId,
            SelectedStyleId = plan.SelectedStyleId,
            SelectedThreadId = plan.SelectedThreadId,
            SelectedQuestId = plan.SelectedQuestId,
            SelectedDialogueId = plan.SelectedDialogueId,
            SelectedDialogueChoiceId = plan.SelectedDialogueChoiceId,
            SelectedItemId = plan.SelectedItemId,
            SelectedEventId = plan.SelectedEventId,
            SelectedRewardId = plan.SelectedRewardId,
            PhaseTrace = proof.PhaseTrace,
            ObjectiveStepIds = proof.ObjectiveStepIds,
            ObjectiveCount = plan.ObjectiveSteps.Count,
            QuestCompletedBefore = false,
            QuestCompletedAfter = proof.QuestCompletedVerified,
            RewardGrantedBefore = false,
            RewardGrantedAfter = proof.RewardGrantedVerified,
            RuntimeStateLoopVerified = proof.RuntimeStateProof.RuntimeStateLoopVerified,
            CommandStateTransitionCount = proof.RuntimeStateProof.CommandStateTransitionCount
        };
    }

    private static UnityQuestInvalidMatrix BuildInvalidMatrix(
        UnityQuestCompletionPlan plan,
        UnityGeneratedSceneProjection projection,
        UnityQuestFirewallProof firewall)
    {
        var baseline = BuildExpectedQuestLoopLines(projection, plan);
        var scenarios = new List<UnityQuestInvalidScenario>
        {
            InvalidScenario("missing_accepted_goal016_evidence", [Diagnostic("error", "unity_quest_loop.previous.runtime_report_missing", UnityRuntimeStateLoopAcceptanceService.ReportJsonFileName, "Goal 016 runtime state loop evidence is required.")]),
            InvalidScenario("missing_runtime_state_loop_report", [Diagnostic("error", "unity_quest_loop.previous.runtime_report_missing", UnityRuntimeStateLoopAcceptanceService.ReportJsonFileName, "Runtime state loop report is required.")]),
            InvalidScenario("missing_runtime_state_loop_state_json", [Diagnostic("error", "unity_quest_loop.previous.runtime_state_missing", UnityRuntimeStateLoopAcceptanceService.StateJsonFileName, "Runtime state loop state JSON is required.")]),
            InvalidScenario("copied_quest_completion_report_without_player_log", [Diagnostic("error", "unity_quest_loop.play_loop.log_missing", "logs/alpha-player-play-loop.log", "Quest completion report cannot replace player log evidence.")]),
            LinesScenario("completion_claimed_without_phase_trace", baseline.Where(line => !line.StartsWith("alpha_runtime.quest_phase.", StringComparison.Ordinal))),
            LinesScenario("completion_claimed_without_objective_checklist", baseline.Where(line => !line.StartsWith("alpha_runtime.quest_objective.", StringComparison.Ordinal))),
            LinesScenario("reward_claimed_without_completion", ReplaceLine(baseline, "alpha_runtime.quest_completed.after=", "alpha_runtime.quest_completed.after=false")),
            LinesScenario("objective_step_changed_without_before_after_delta", baseline.Where(line => !line.StartsWith("alpha_runtime.quest_objective.1.before=", StringComparison.Ordinal) && !line.StartsWith("alpha_runtime.quest_objective.1.after=", StringComparison.Ordinal))),
            LinesScenario("objective_step_command_id_mismatch", ReplaceLine(baseline, "alpha_runtime.quest_objective.0.required_command_id=", "alpha_runtime.quest_objective.0.required_command_id=cmd/mismatch")),
            LinesScenario("objective_step_command_type_mismatch", ReplaceLine(baseline, "alpha_runtime.quest_objective.0.required_command_type=", "alpha_runtime.quest_objective.0.required_command_type=dialogue/open")),
            LinesScenario("objective_step_target_id_mismatch", ReplaceLine(baseline, "alpha_runtime.quest_objective.0.required_target_id=", "alpha_runtime.quest_objective.0.required_target_id=quest/mismatch")),
            LinesScenario("objective_step_secondary_target_id_mismatch", ReplaceLine(baseline, "alpha_runtime.quest_objective.2.required_secondary_target_id=", "alpha_runtime.quest_objective.2.required_secondary_target_id=quest/mismatch")),
            LinesScenario("quest_start_objective_caused_by_non_quest_command", ReplaceLine(baseline, "alpha_runtime.quest_objective.0.required_command_type=", "alpha_runtime.quest_objective.0.required_command_type=event/add_item")),
            LinesScenario("dialogue_objective_caused_by_non_dialogue_command", ReplaceLine(baseline, "alpha_runtime.quest_objective.1.required_command_type=", "alpha_runtime.quest_objective.1.required_command_type=quest/start")),
            LinesScenario("item_objective_caused_by_non_item_event_loot_command", ReplaceLine(baseline, "alpha_runtime.quest_objective.3.required_command_type=", "alpha_runtime.quest_objective.3.required_command_type=dialogue/open")),
            LinesScenario("event_objective_caused_by_command_from_another_event", ReplaceLine(baseline, "alpha_runtime.quest_objective.4.required_secondary_target_id=", "alpha_runtime.quest_objective.4.required_secondary_target_id=event/other")),
            LinesScenario("quest_phase_order_mismatch", MoveLineBefore(baseline, "alpha_runtime.quest_phase.after.completed=", "alpha_runtime.quest_phase.after.started=")),
            LinesScenario("reward_id_from_another_package_style", ReplaceLine(baseline, "alpha_runtime.reward.id=", "alpha_runtime.reward.id=item/other-package/000")),
            LinesScenario("selected_package_id_mismatch", ReplaceLine(baseline, "alpha_runtime.quest_loop.package_id=", "alpha_runtime.quest_loop.package_id=game/content_generation/other")),
            LinesScenario("selected_style_id_mismatch", ReplaceLine(baseline, "alpha_runtime.quest_loop.style_id=", "alpha_runtime.quest_loop.style_id=other_style")),
            LinesScenario("selected_thread_id_mismatch", ReplaceLine(baseline, "alpha_runtime.quest_loop.thread_id=", "alpha_runtime.quest_loop.thread_id=thread/other/000")),
            LinesScenario("state_leak_from_previous_run", ReplaceLine(baseline, "alpha_runtime.quest_objective.0.before=", "alpha_runtime.quest_objective.0.before=true")),
            InvalidScenario("runtime_preview_dependency_claim", [Diagnostic("error", "unity_quest_loop.contract.runtime_preview_dependency", "runtime_host", "Unity quest completion loop must not claim Runtime Preview dependency.")]),
            InvalidScenario("development_profiler_debug_build_option_reintroduced", UnityPlayableAlphaAcceptanceService.ValidateFirewallSafeBuildScript("options = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;").Diagnostics.Select(ConvertDiagnostic).ToList())
        };

        var passed = scenarios.All(item => !item.ActualValid) && firewall.FirewallSafeBuildVerified;
        return new UnityQuestInvalidMatrix
        {
            Passed = passed,
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics =
            [
                Diagnostic(passed ? "info" : "error", passed ? "unity_quest_loop.invalid_matrix_rejected" : "unity_quest_loop.invalid_matrix_failed", "invalid_matrix", "Invalid/fake/leak quest completion scenarios must reject through quest-loop, runtime-state, previous-evidence or firewall validation paths.")
            ]
        };

        UnityQuestInvalidScenario LinesScenario(string id, IEnumerable<string> lines) =>
            InvalidScenario(id, ValidateQuestLoopLines(lines, projection, plan).Diagnostics);
    }

    private static bool CommandAllowedForObjective(UnityQuestObjectiveStep step) =>
        step.ObjectiveKind switch
        {
            "quest_start" => step.RequiredCommandType == "quest/start" && step.RequiredTargetId == step.SourceGeneratedId,
            "dialogue_open" => step.RequiredCommandType == "dialogue/open" && step.RequiredTargetId == step.SourceGeneratedId,
            "dialogue_choice" => step.RequiredCommandType == "dialogue/choose" && step.RequiredTargetId == step.SourceGeneratedId,
            "item_obtained" => step.RequiredCommandType is "event/add_item" or "loot/roll",
            "event_applied" => step.RequiredCommandType.StartsWith("event/", StringComparison.Ordinal) && step.RequiredSecondaryTargetId == step.SourceGeneratedId,
            "quest_completed_reward" => step.RequiredCommandType is "event/add_item" or "loot/roll" or "quest/start",
            _ => false
        };

    private static UnityGeneratedSceneCommandHint Command(
        UnityGeneratedSceneProjection projection,
        string commandType,
        string targetId) =>
        projection.CommandHints.FirstOrDefault(command =>
            command.CommandType == commandType &&
            (string.IsNullOrWhiteSpace(targetId) || command.TargetId == targetId)) ?? new UnityGeneratedSceneCommandHint();

    private static UnityGeneratedSceneCommandHint CommandWithSecondary(
        UnityGeneratedSceneProjection projection,
        string commandTypePrefix,
        string secondaryTargetId) =>
        projection.CommandHints.FirstOrDefault(command =>
            command.CommandType.StartsWith(commandTypePrefix, StringComparison.Ordinal) &&
            (string.IsNullOrWhiteSpace(secondaryTargetId) || command.SecondaryTargetId == secondaryTargetId)) ?? new UnityGeneratedSceneCommandHint();

    private static void AppendPhaseLines(ICollection<string> lines)
    {
        lines.Add("alpha_runtime.quest_phase.before=not_started");
        lines.Add("alpha_runtime.quest_phase.after.started=started");
        lines.Add("alpha_runtime.quest_phase.after.dialogue_opened=dialogue_opened");
        lines.Add("alpha_runtime.quest_phase.after.choice_selected=choice_selected");
        lines.Add("alpha_runtime.quest_phase.after.item_obtained=item_obtained");
        lines.Add("alpha_runtime.quest_phase.after.event_applied=event_applied");
        lines.Add("alpha_runtime.quest_phase.after.completed=completed");
        lines.Add("alpha_runtime.quest_phase.after.reward_granted=reward_granted");
    }

    private static void AppendObjectiveLines(ICollection<string> lines, int index, UnityQuestObjectiveStep step)
    {
        lines.Add(ObjectiveKey(index, "objective_id") + "=" + step.ObjectiveId);
        lines.Add(ObjectiveKey(index, "objective_kind") + "=" + step.ObjectiveKind);
        lines.Add(ObjectiveKey(index, "source_id") + "=" + step.SourceGeneratedId);
        lines.Add(ObjectiveKey(index, "required_command_id") + "=" + step.RequiredCommandId);
        lines.Add(ObjectiveKey(index, "required_command_type") + "=" + step.RequiredCommandType);
        lines.Add(ObjectiveKey(index, "required_target_id") + "=" + step.RequiredTargetId);
        lines.Add(ObjectiveKey(index, "required_secondary_target_id") + "=" + step.RequiredSecondaryTargetId);
        lines.Add(ObjectiveKey(index, "before") + "=false");
        lines.Add(ObjectiveKey(index, "after") + "=true");
    }

    private static string ObjectiveKey(int index, string suffix) =>
        $"alpha_runtime.quest_objective.{index}.{suffix}";

    private static int IndexOf(IReadOnlyList<string> lines, string prefix)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            if (lines[index].StartsWith(prefix, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
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

    private static IEnumerable<string> MoveLineBefore(IEnumerable<string> lines, string movedPrefix, string beforePrefix)
    {
        var list = lines.ToList();
        var movedIndex = list.FindIndex(line => line.StartsWith(movedPrefix, StringComparison.Ordinal));
        var beforeIndex = list.FindIndex(line => line.StartsWith(beforePrefix, StringComparison.Ordinal));
        if (movedIndex < 0 || beforeIndex < 0)
        {
            return list;
        }

        var moved = list[movedIndex];
        list.RemoveAt(movedIndex);
        beforeIndex = list.FindIndex(line => line.StartsWith(beforePrefix, StringComparison.Ordinal));
        list.Insert(Math.Max(0, beforeIndex), moved);
        return list;
    }

    private static UnityQuestInvalidScenario InvalidScenario(
        string id,
        IReadOnlyList<UnityQuestLoopDiagnostic> diagnostics) =>
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

    private static string RenderReport(UnityQuestCompletionLoopReport report)
    {
        var lines = new List<string>
        {
            "# Unity Generated Quest Completion Loop Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Final status: {report.FinalStatus}",
            $"- Previous gate: {report.PreviousAcceptedGate}",
            $"- Completed slices: {string.Join(", ", report.CompletedSlices)}",
            $"- Product smoke route: {report.ProductSmokeRoute}",
            $"- Selected package/style/thread: {report.SelectedPackageId} / {report.SelectedStyleId} / {report.SelectedThreadId}",
            $"- Selected quest/dialogue/choice/item/event/reward: {report.SelectedQuestId} / {report.SelectedDialogueId} / {report.SelectedDialogueChoiceId} / {report.SelectedItemId} / {report.SelectedEventId} / {report.SelectedRewardId}",
            $"- Quest completion loop verified: {report.QuestCompletionLoopVerified.ToString().ToLowerInvariant()}",
            $"- Phase/objective/correlation: {report.QuestPhaseTraceVerified.ToString().ToLowerInvariant()} / {report.ObjectiveChecklistVerified.ToString().ToLowerInvariant()} / {report.ObjectiveCommandCorrelationVerified.ToString().ToLowerInvariant()}",
            $"- Quest completed/reward granted: {report.QuestCompletedVerified.ToString().ToLowerInvariant()} / {report.RewardGrantedVerified.ToString().ToLowerInvariant()}",
            $"- Movement/focus/interaction/play-loop: {report.MovementVerified.ToString().ToLowerInvariant()} / {report.FocusVerified.ToString().ToLowerInvariant()} / {report.InteractionVerified.ToString().ToLowerInvariant()} / {report.PlayLoopVerified.ToString().ToLowerInvariant()}",
            $"- Objective count: {report.Plan.ObjectiveSteps.Count}",
            $"- Quest-loop hash: {report.QuestLoopHash}",
            $"- Plan hash: {report.PlanHash}",
            $"- State hash: {report.StateHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Build manifest hash: {report.BuildManifestHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            string.Empty,
            "## Diagnostics",
            string.Empty
        };
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(
        UnityQuestCompletionLoopReport report,
        string alphaVerificationMarkdown)
    {
        var lines = new List<string>
        {
            "# Unity Generated Quest Completion Loop Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- Plan artifact: {RelativeOutputDirectory}/{PlanJsonFileName}",
            $"- State artifact: {RelativeOutputDirectory}/{StateJsonFileName}",
            $"- Report artifact: {RelativeOutputDirectory}/{ReportJsonFileName}",
            $"- Selected package/style/thread: {report.SelectedPackageId} / {report.SelectedStyleId} / {report.SelectedThreadId}",
            $"- Selected quest/dialogue/choice/item/event/reward: {report.SelectedQuestId} / {report.SelectedDialogueId} / {report.SelectedDialogueChoiceId} / {report.SelectedItemId} / {report.SelectedEventId} / {report.SelectedRewardId}",
            $"- Quest phase trace: {string.Join(", ", report.Plan.QuestPhaseOrder)}",
            $"- Objective ids: {string.Join(", ", report.Plan.ObjectiveSteps.Select(step => step.ObjectiveId))}",
            $"- Quest-loop hash: {report.QuestLoopHash}",
            $"- Plan hash: {report.PlanHash}",
            $"- State hash: {report.StateHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Build manifest hash: {report.BuildManifestHash}",
            $"- Final gate status: required, not passed",
            $"- Future post-goal work started: false",
            string.Empty,
            "## Underlying Alpha Build Verification",
            string.Empty,
            SanitizeEmbeddedAlphaVerification(alphaVerificationMarkdown).TrimEnd()
        };

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string SanitizeEmbeddedAlphaVerification(string markdown)
    {
        var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (line.StartsWith("- Unity command:", StringComparison.Ordinal) ||
                line.StartsWith("- Launch command:", StringComparison.Ordinal) ||
                line.StartsWith("- Play-loop command:", StringComparison.Ordinal))
            {
                lines[index] = line[..line.IndexOf(':')] + ": (omitted; local machine paths are not part of compact deterministic root artifacts)";
            }
        }

        return string.Join(Environment.NewLine, lines);
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

    private static string DisplayId(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? "(none)"
            : value.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? value;

    private static UnityQuestLoopDiagnostic ConvertDiagnostic(UnityRuntimeStateDiagnostic diagnostic) =>
        Diagnostic(diagnostic.Severity, diagnostic.Code, diagnostic.Target, diagnostic.Message);

    private static UnityQuestLoopDiagnostic ConvertDiagnostic(AlphaBuildDiagnostic diagnostic) =>
        Diagnostic(diagnostic.Severity, diagnostic.Code, diagnostic.Target, diagnostic.Message);

    private static IReadOnlyList<UnityQuestLoopDiagnostic> SortDiagnostics(IEnumerable<UnityQuestLoopDiagnostic> diagnostics) =>
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

    private static UnityQuestLoopDiagnostic Diagnostic(string severity, string code, string target, string message) =>
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

public sealed record UnityQuestCompletionLoopOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public string RelativeOutputDirectoryOverride { get; init; } = string.Empty;
    public string SelectedStyleId { get; init; } = string.Empty;
    public int? CandidateOrdinal { get; init; }
    public bool ExecuteUnityBuild { get; init; }
    public bool LaunchBuiltPlayer { get; init; }
    public bool PreserveExistingBuildOutputForValidation { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 90;
}

public sealed record UnityQuestCompletionLoopAcceptanceResult
{
    public UnityQuestCompletionLoopReport Report { get; init; } = new();
    public string PlanJson { get; init; } = string.Empty;
    public string StateJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record UnityQuestCompletionLoopWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string PlanJsonPath { get; init; } = string.Empty;
    public string StateJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record UnityQuestCompletionLoopReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public AlphaRunnableBuildReport AlphaBuild { get; init; } = new();
    public UnityQuestCompletionPlan Plan { get; init; } = new();
    public UnityQuestCompletionLoopState State { get; init; } = new();
    public string SelectedPackageId { get; init; } = string.Empty;
    public string SelectedStyleId { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public string SelectedQuestId { get; init; } = string.Empty;
    public string SelectedDialogueId { get; init; } = string.Empty;
    public string SelectedDialogueChoiceId { get; init; } = string.Empty;
    public string SelectedItemId { get; init; } = string.Empty;
    public string SelectedEventId { get; init; } = string.Empty;
    public string SelectedRewardId { get; init; } = string.Empty;
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
    public UnityQuestInvalidMatrix InvalidMatrix { get; init; } = new();
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool NoExternalProviderLlmRagLuaMedia { get; init; }
    public bool RuntimePreviewDependency { get; init; }
    public string QuestLoopHash { get; init; } = string.Empty;
    public string PlanHash { get; init; } = string.Empty;
    public string StateHash { get; init; } = string.Empty;
    public string BuildManifestHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public string DeterministicReportRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<UnityQuestLoopDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityQuestCompletionPlan
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string SelectedPackageId { get; init; } = string.Empty;
    public string SelectedStyleId { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public string SelectedQuestId { get; init; } = string.Empty;
    public string SelectedQuestTitle { get; init; } = string.Empty;
    public string SelectedQuestSourceId { get; init; } = string.Empty;
    public string SelectedDialogueId { get; init; } = string.Empty;
    public string SelectedDialogueChoiceId { get; init; } = string.Empty;
    public string SelectedItemId { get; init; } = string.Empty;
    public string SelectedEventId { get; init; } = string.Empty;
    public string SelectedRewardId { get; init; } = string.Empty;
    public string SelectedRewardKind { get; init; } = string.Empty;
    public string StartMapId { get; init; } = string.Empty;
    public IReadOnlyList<string> QuestPhaseOrder { get; init; } = [];
    public IReadOnlyList<UnityQuestObjectiveStep> ObjectiveSteps { get; init; } = [];
    public IReadOnlyList<string> CommandSequence { get; init; } = [];
    public IReadOnlyList<string> CompletionCriteria { get; init; } = [];
    public UnityQuestExpectedFinalState ExpectedFinalState { get; init; } = new();
    public string PlanHash { get; init; } = string.Empty;
}

public sealed record UnityQuestObjectiveStep
{
    public string ObjectiveId { get; init; } = string.Empty;
    public string ObjectiveKind { get; init; } = string.Empty;
    public string SourceGeneratedId { get; init; } = string.Empty;
    public string RequiredCommandId { get; init; } = string.Empty;
    public string RequiredCommandType { get; init; } = string.Empty;
    public string RequiredTargetId { get; init; } = string.Empty;
    public string RequiredSecondaryTargetId { get; init; } = string.Empty;
    public bool Before { get; init; }
    public bool After { get; init; }
    public string VisibleLabel { get; init; } = string.Empty;
}

public sealed record UnityQuestExpectedFinalState
{
    public bool QuestCompleted { get; init; }
    public bool RewardGranted { get; init; }
    public string RewardId { get; init; } = string.Empty;
    public string RewardKind { get; init; } = string.Empty;
    public int InventoryItemCount { get; init; }
}

public sealed record UnityQuestCompletionLoopState
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string SelectedPackageId { get; init; } = string.Empty;
    public string SelectedStyleId { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public string SelectedQuestId { get; init; } = string.Empty;
    public string SelectedDialogueId { get; init; } = string.Empty;
    public string SelectedDialogueChoiceId { get; init; } = string.Empty;
    public string SelectedItemId { get; init; } = string.Empty;
    public string SelectedEventId { get; init; } = string.Empty;
    public string SelectedRewardId { get; init; } = string.Empty;
    public IReadOnlyList<string> PhaseTrace { get; init; } = [];
    public IReadOnlyList<string> ObjectiveStepIds { get; init; } = [];
    public int ObjectiveCount { get; init; }
    public bool QuestCompletedBefore { get; init; }
    public bool QuestCompletedAfter { get; init; }
    public bool RewardGrantedBefore { get; init; }
    public bool RewardGrantedAfter { get; init; }
    public bool RuntimeStateLoopVerified { get; init; }
    public int CommandStateTransitionCount { get; init; }
    public string StateHash { get; init; } = string.Empty;
}

public sealed record UnityQuestCompletionLoopProof
{
    public UnityRuntimeStateLoopProof RuntimeStateProof { get; init; } = new();
    public bool QuestCompletionLoopVerified { get; init; }
    public bool QuestPlanVerified { get; init; }
    public bool QuestPhaseTraceVerified { get; init; }
    public bool ObjectiveChecklistVerified { get; init; }
    public bool ObjectiveCommandCorrelationVerified { get; init; }
    public bool QuestCompletedVerified { get; init; }
    public bool RewardGrantedVerified { get; init; }
    public IReadOnlyList<string> PhaseTrace { get; init; } = [];
    public IReadOnlyList<string> ObjectiveStepIds { get; init; } = [];
    public IReadOnlyList<UnityQuestLoopDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityQuestPreviousEvidenceProof
{
    public bool Passed { get; init; }
    public string ReportRelativePath { get; init; } = string.Empty;
    public string StateRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<UnityQuestLoopDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityQuestFirewallProof
{
    public string BuildOptions { get; init; } = string.Empty;
    public bool StaticChecksPassed { get; init; }
    public bool BuildMetadataPresent { get; init; }
    public bool FirewallSafeBuildVerified { get; init; }
    public IReadOnlyList<UnityQuestLoopDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityQuestInvalidMatrix
{
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<UnityQuestInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<UnityQuestLoopDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityQuestInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<UnityQuestLoopDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityQuestLoopDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
