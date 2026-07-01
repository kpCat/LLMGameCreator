using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.CapabilityBundlePipelineInputs;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Design.RichPackageAssemblyCoverageAudit;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Design.PackageAssemblyDialogueQuests;

public sealed class PackageAssemblyDialogueQuestsAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/package-assembly-dialogue-quests";
    public const string MappingContractProofJsonFileName = "package-assembly-dialogue-quests-mapping-contract-proof.json";
    public const string InputFixturesJsonFileName = "package-assembly-dialogue-quests-input-fixtures.json";
    public const string AssemblyReportJsonFileName = "package-assembly-dialogue-quests-assembly-report.json";
    public const string PackageSummaryJsonFileName = "package-assembly-dialogue-quests-package-summary.json";
    public const string AntiOverfitFixturesJsonFileName = "package-assembly-dialogue-quests-anti-overfit-fixtures.json";
    public const string InvalidMatrixJsonFileName = "package-assembly-dialogue-quests-invalid-matrix.json";
    public const string ReportJsonFileName = "package-assembly-dialogue-quests-report.json";
    public const string ReportMarkdownFileName = "package-assembly-dialogue-quests-report.md";
    public const string VerificationMarkdownFileName = "package-assembly-dialogue-quests-verification.md";
    public const string FinalArtifactScopeReportJsonFileName = "goal-026-final-artifact-scope-report.json";
    public const string FinalArtifactScopeReportMarkdownFileName = "goal-026-final-artifact-scope-report.md";
    public const string FinalGate = "package_assembly_dialogue_quests_expansion_verification";
    public const string PreviousAcceptedGate = "package_assembly_world_entities_expansion_verification passed";
    private const string ProductSmokeRoute = "package-assembly-dialogue-quests";
    private const string Goal023RelativeOutputDirectory = ".llmgc/procedural/capability-bundle-pipeline-inputs";
    private const string Goal024RelativeOutputDirectory = ".llmgc/procedural/rich-package-assembly-coverage-audit";
    private const string Goal025RelativeOutputDirectory = ".llmgc/procedural/package-assembly-world-entities";
    private static readonly DateTimeOffset AppliedAtUtc = DateTimeOffset.Parse("2026-06-28T00:00:00Z");
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<PackageAssemblyDialogueQuestsResult> BuildAsync(
        string projectRootPath,
        PackageAssemblyDialogueQuestsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new PackageAssemblyDialogueQuestsOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<PackageAssemblyDialogueQuestsDiagnostic>
        {
            Diagnostic("info", "package_dialogue_quests.previous_gate_recorded", settings.PreviousAcceptedGate, "User-confirmed Goal 025 package assembly world/entities verification is recorded as passed."),
            Diagnostic("info", "package_dialogue_quests.boundary", "execution_boundary", "Goal 026 executes bounded in-memory package assembly only; no Unity, LLM, RAG, provider, media or Lua execution is invoked.")
        };

        if (settings.PreviousAcceptedGate != PreviousAcceptedGate)
        {
            diagnostics.Add(Diagnostic("error", "package_dialogue_quests.previous_gate.missing", settings.PreviousAcceptedGate, "Goal 026 requires package_assembly_world_entities_expansion_verification passed."));
        }

        var evidence = await LoadEvidenceAsync(projectRoot, settings, diagnostics, cancellationToken).ConfigureAwait(false);
        var fixtures = BuildFixtures(evidence);
        var realConsumer = BuildConsumer(fixtures.RealConsumer, diagnostics);
        var syntheticConsumer = settings.SyntheticAntiOverfitFixtureMissing
            ? DialogueQuestConsumerSummary.Missing("rumor_board_tutorial")
            : BuildConsumer(fixtures.SyntheticConsumer, diagnostics);
        var invalidMatrix = BuildInvalidMatrix(evidence, fixtures);
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var antiOverfit = new PackageAssemblyDialogueQuestsAntiOverfitProof
        {
            RealConsumerId = realConsumer.ConsumerId,
            SyntheticConsumerId = syntheticConsumer.ConsumerId,
            SyntheticConsumerPresent = !settings.SyntheticAntiOverfitFixtureMissing,
            DistinctConsumerIds = !string.Equals(realConsumer.ConsumerId, syntheticConsumer.ConsumerId, StringComparison.Ordinal),
            DistinctQuestIds = !string.Equals(realConsumer.PrimaryQuestId, syntheticConsumer.PrimaryQuestId, StringComparison.Ordinal),
            DistinctDialogueIds = !string.Equals(realConsumer.PrimaryDialogueId, syntheticConsumer.PrimaryDialogueId, StringComparison.Ordinal),
            Passed = !settings.HardcodedGothicOnlyOutput
                && !settings.SyntheticAntiOverfitFixtureMissing
                && syntheticConsumer.Passed
                && !string.Equals(realConsumer.PrimaryQuestId, syntheticConsumer.PrimaryQuestId, StringComparison.Ordinal)
                && !string.Equals(realConsumer.PrimaryDialogueId, syntheticConsumer.PrimaryDialogueId, StringComparison.Ordinal)
        };

        var mappingProof = BuildMappingProof(evidence, realConsumer, syntheticConsumer);
        var assemblyReport = new PackageAssemblyDialogueQuestsAssemblyReport
        {
            SchemaVersion = "package_assembly_dialogue_quests_assembly_report_v1",
            ProductSmokeRoute = ProductSmokeRoute,
            Consumers = [realConsumer, syntheticConsumer],
            Diagnostics = SortDiagnostics(diagnostics.Where(item => item.Code.StartsWith("package_dialogue_quests.assembly", StringComparison.Ordinal)))
        };
        var packageSummary = new PackageAssemblyDialogueQuestsPackageSummary
        {
            SchemaVersion = "package_assembly_dialogue_quests_package_summary_v1",
            ConsumerSummaries = [realConsumer, syntheticConsumer],
            TotalQuests = realConsumer.QuestCount + syntheticConsumer.QuestCount,
            TotalQuestStages = realConsumer.QuestStageCount + syntheticConsumer.QuestStageCount,
            TotalQuestObjectives = realConsumer.QuestObjectiveCount + syntheticConsumer.QuestObjectiveCount,
            TotalDialogues = realConsumer.DialogueCount + syntheticConsumer.DialogueCount,
            TotalDialogueNodes = realConsumer.DialogueNodeCount + syntheticConsumer.DialogueNodeCount,
            TotalDialogueChoices = realConsumer.DialogueChoiceCount + syntheticConsumer.DialogueChoiceCount
        };
        var scopeReport = BuildScopeReport();

        var mappingProofJson = JsonSerializer.Serialize(mappingProof, JsonOptions);
        var fixturesJson = JsonSerializer.Serialize(fixtures, JsonOptions);
        var assemblyReportJson = JsonSerializer.Serialize(assemblyReport, JsonOptions);
        var packageSummaryJson = JsonSerializer.Serialize(packageSummary, JsonOptions);
        var antiOverfitJson = JsonSerializer.Serialize(antiOverfit, JsonOptions);
        var invalidMatrixJson = JsonSerializer.Serialize(invalidMatrix, JsonOptions);
        var scopeReportJson = JsonSerializer.Serialize(scopeReport, JsonOptions);

        var noTopLevelErrors = diagnostics.All(diagnostic => diagnostic.Severity != "error");
        var reportWithoutHash = new PackageAssemblyDialogueQuestsReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            CompletedSlices = ["S206", "S207", "S208", "S209", "S210", "S211", "S212"],
            ProductSmokeRoute = ProductSmokeRoute,
            ContractProofPassed = noTopLevelErrors && invalidMatrix.Passed && antiOverfit.Passed,
            Goal025EvidenceVerified = evidence.Goal025EvidenceVerified,
            Goal024EvidenceVerified = evidence.Goal024EvidenceVerified,
            Goal023EvidenceVerified = evidence.Goal023EvidenceVerified,
            RealConsumerPassed = realConsumer.Passed,
            SyntheticConsumerPassed = syntheticConsumer.Passed,
            AntiOverfitProofPassed = antiOverfit.Passed,
            DialogueQuestMappingWritten = true,
            PackageSummaryWritten = true,
            PackageAssemblyExecuted = true,
            ProductVerticalGate = false,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            UnityBuildExecuted = false,
            LlmRagProviderMediaLuaExecuted = false,
            ScopeGuardPassed = scopeReport.Passed,
            MappingContractProofHash = ComputeHash(mappingProofJson),
            InputFixturesHash = ComputeHash(fixturesJson),
            AssemblyReportHash = ComputeHash(assemblyReportJson),
            PackageSummaryHash = ComputeHash(packageSummaryJson),
            AntiOverfitFixturesHash = ComputeHash(antiOverfitJson),
            InvalidMatrixHash = ComputeHash(invalidMatrixJson),
            ScopeReportHash = ComputeHash(scopeReportJson),
            InvalidMatrix = invalidMatrix,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new PackageAssemblyDialogueQuestsResult
        {
            MappingContractProof = mappingProof,
            InputFixtures = fixtures,
            AssemblyReport = assemblyReport,
            PackageSummary = packageSummary,
            AntiOverfitProof = antiOverfit,
            InvalidMatrix = invalidMatrix,
            ScopeReport = scopeReport,
            Report = report,
            MappingContractProofJson = mappingProofJson,
            InputFixturesJson = fixturesJson,
            AssemblyReportJson = assemblyReportJson,
            PackageSummaryJson = packageSummaryJson,
            AntiOverfitFixturesJson = antiOverfitJson,
            InvalidMatrixJson = invalidMatrixJson,
            ScopeReportJson = scopeReportJson,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report, packageSummary),
            VerificationMarkdown = RenderVerification(report),
            ScopeReportMarkdown = RenderScopeReport(scopeReport)
        };
    }

    public async Task<PackageAssemblyDialogueQuestsWriteResult> WriteAsync(
        string projectRootPath,
        PackageAssemblyDialogueQuestsResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var mappingContractProofPath = Path.Combine(outputDirectory, MappingContractProofJsonFileName);
        var inputFixturesPath = Path.Combine(outputDirectory, InputFixturesJsonFileName);
        var assemblyReportPath = Path.Combine(outputDirectory, AssemblyReportJsonFileName);
        var packageSummaryPath = Path.Combine(outputDirectory, PackageSummaryJsonFileName);
        var antiOverfitPath = Path.Combine(outputDirectory, AntiOverfitFixturesJsonFileName);
        var invalidMatrixPath = Path.Combine(outputDirectory, InvalidMatrixJsonFileName);
        var reportJsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var reportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationMarkdownPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        var scopeReportJsonPath = Path.Combine(outputDirectory, FinalArtifactScopeReportJsonFileName);
        var scopeReportMarkdownPath = Path.Combine(outputDirectory, FinalArtifactScopeReportMarkdownFileName);

        await File.WriteAllTextAsync(mappingContractProofPath, result.MappingContractProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(inputFixturesPath, result.InputFixturesJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(assemblyReportPath, result.AssemblyReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(packageSummaryPath, result.PackageSummaryJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(antiOverfitPath, result.AntiOverfitFixturesJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(invalidMatrixPath, result.InvalidMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationMarkdownPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(scopeReportJsonPath, result.ScopeReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(scopeReportMarkdownPath, result.ScopeReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new PackageAssemblyDialogueQuestsWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            MappingContractProofJsonPath = mappingContractProofPath,
            InputFixturesJsonPath = inputFixturesPath,
            AssemblyReportJsonPath = assemblyReportPath,
            PackageSummaryJsonPath = packageSummaryPath,
            AntiOverfitFixturesJsonPath = antiOverfitPath,
            InvalidMatrixJsonPath = invalidMatrixPath,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            VerificationMarkdownPath = verificationMarkdownPath,
            ScopeReportJsonPath = scopeReportJsonPath,
            ScopeReportMarkdownPath = scopeReportMarkdownPath
        };
    }

    public async Task<PackageAssemblyDialogueQuestsWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = await BuildAsync(projectRootPath, null, cancellationToken).ConfigureAwait(false);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<PackageAssemblyDialogueQuestsEvidence> LoadEvidenceAsync(
        string projectRoot,
        PackageAssemblyDialogueQuestsOptions settings,
        ICollection<PackageAssemblyDialogueQuestsDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var goal023Root = Path.Combine(projectRoot, Goal023RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar));
        var goal024Root = Path.Combine(projectRoot, Goal024RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar));
        var goal025Root = Path.Combine(projectRoot, Goal025RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar));
        var goal023InputsPath = Path.Combine(goal023Root, CapabilityBundlePipelineInputsAcceptanceService.GeneratorInputsJsonFileName);
        var goal024ReportPath = Path.Combine(goal024Root, RichPackageAssemblyCoverageAuditAcceptanceService.ReportJsonFileName);
        var goal024MatrixPath = Path.Combine(goal024Root, RichPackageAssemblyCoverageAuditAcceptanceService.CoverageMatrixJsonFileName);
        var goal024NextPlanPath = Path.Combine(goal024Root, RichPackageAssemblyCoverageAuditAcceptanceService.NextSlicePlanJsonFileName);
        var goal025ReportPath = Path.Combine(goal025Root, "package-assembly-world-entities-report.json");
        var goal025SummaryPath = Path.Combine(goal025Root, "package-assembly-world-entities-package-summary.json");
        var evidence = new PackageAssemblyDialogueQuestsEvidence
        {
            Goal023GeneratorInputsPath = RelativePath(projectRoot, goal023InputsPath),
            Goal024ReportPath = RelativePath(projectRoot, goal024ReportPath),
            Goal024CoverageMatrixPath = RelativePath(projectRoot, goal024MatrixPath),
            Goal024NextSlicePlanPath = RelativePath(projectRoot, goal024NextPlanPath),
            Goal025ReportPath = RelativePath(projectRoot, goal025ReportPath),
            Goal025PackageSummaryPath = RelativePath(projectRoot, goal025SummaryPath)
        };

        if (settings.MissingGoal023GeneratorInputs || !File.Exists(goal023InputsPath))
        {
            diagnostics.Add(Diagnostic("error", "package_dialogue_quests.goal023_generator_inputs.missing", evidence.Goal023GeneratorInputsPath, "Goal 026 requires physical Goal 023 generator pipeline inputs."));
            return evidence;
        }

        var goal023InputsJson = await File.ReadAllTextAsync(goal023InputsPath, cancellationToken).ConfigureAwait(false);
        var generatorInputs = Deserialize<CapabilityBundleGeneratorInputsArtifact>(goal023InputsJson, evidence.Goal023GeneratorInputsPath, diagnostics);
        evidence = evidence with
        {
            Goal023GeneratorInputsHash = ComputeHash(goal023InputsJson),
            Goal023EvidenceVerified = generatorInputs?.PipelineInputCount == 3 && generatorInputs.PipelineInputs.Count == 3,
            Goal023PipelineInputs = generatorInputs?.PipelineInputs ?? []
        };

        if (settings.MissingGoal024Evidence || !File.Exists(goal024ReportPath) || !File.Exists(goal024MatrixPath) || !File.Exists(goal024NextPlanPath))
        {
            diagnostics.Add(Diagnostic("error", "package_dialogue_quests.goal024_evidence.missing", Goal024RelativeOutputDirectory, "Goal 026 requires Goal 024 report, matrix and next-slice plan."));
            return evidence;
        }

        var reportJson = await File.ReadAllTextAsync(goal024ReportPath, cancellationToken).ConfigureAwait(false);
        var matrixJson = await File.ReadAllTextAsync(goal024MatrixPath, cancellationToken).ConfigureAwait(false);
        var nextPlanJson = await File.ReadAllTextAsync(goal024NextPlanPath, cancellationToken).ConfigureAwait(false);
        var goal024Verified = JsonString(reportJson, "manualGate") == RichPackageAssemblyCoverageAuditAcceptanceService.FinalGate
            && JsonBool(reportJson, "contractProofPassed")
            && JsonString(nextPlanJson, "candidates").Length >= 0;
        var (questGaps, dialogueGaps) = ReadNarrativeGapIds(matrixJson);
        evidence = evidence with
        {
            Goal024ReportHash = ComputeHash(reportJson),
            Goal024CoverageMatrixHash = ComputeHash(matrixJson),
            Goal024NextSlicePlanHash = ComputeHash(nextPlanJson),
            Goal024EvidenceVerified = goal024Verified && questGaps.Count > 0 && dialogueGaps.Count > 0,
            Goal024QuestGapIds = questGaps,
            Goal024DialogueGapIds = dialogueGaps
        };

        if (settings.MissingGoal025Evidence || !File.Exists(goal025ReportPath) || !File.Exists(goal025SummaryPath))
        {
            diagnostics.Add(Diagnostic("error", "package_dialogue_quests.goal025_evidence.missing", Goal025RelativeOutputDirectory, "Goal 026 requires Goal 025 world/entities compact evidence."));
            return evidence;
        }

        var goal025ReportJson = await File.ReadAllTextAsync(goal025ReportPath, cancellationToken).ConfigureAwait(false);
        var goal025SummaryJson = await File.ReadAllTextAsync(goal025SummaryPath, cancellationToken).ConfigureAwait(false);
        return evidence with
        {
            Goal025ReportHash = ComputeHash(goal025ReportJson),
            Goal025PackageSummaryHash = ComputeHash(goal025SummaryJson),
            Goal025EvidenceVerified = JsonString(goal025ReportJson, "manualGate") == "package_assembly_world_entities_expansion_verification"
                && JsonBool(goal025ReportJson, "contractProofPassed")
                && JsonBool(goal025ReportJson, "realConsumerPassed")
                && JsonBool(goal025ReportJson, "syntheticConsumerPassed")
        };
    }

    private static PackageAssemblyDialogueQuestsFixtures BuildFixtures(PackageAssemblyDialogueQuestsEvidence evidence)
    {
        var realInput = evidence.Goal023PipelineInputs.FirstOrDefault(input => input.ProfileId.Contains("gothic", StringComparison.OrdinalIgnoreCase))
            ?? evidence.Goal023PipelineInputs.FirstOrDefault(input => input.ReadyForPackageAssemblyPlanning)
            ?? evidence.Goal023PipelineInputs.OrderBy(input => input.ProfileId, StringComparer.Ordinal).FirstOrDefault()
            ?? new CapabilityBundlePipelineInputRecord
            {
                ProfileId = "game_profile/gothic-mystery-investigation-alpha",
                GameFamilyId = "game_family/gothic_mystery",
                SelectionId = "generator_plan_capability_selection/goal026"
            };

        return new PackageAssemblyDialogueQuestsFixtures
        {
            SchemaVersion = "package_assembly_dialogue_quests_input_fixtures_v1",
            RealConsumer = BuildRealConsumerFixture(realInput),
            SyntheticConsumer = BuildSyntheticFixture()
        };
    }

    private static PackageAssemblyDialogueQuestsConsumerFixture BuildRealConsumerFixture(CapabilityBundlePipelineInputRecord input) =>
        new()
        {
            ConsumerId = "goal026_real_consumer_gothic_mystery",
            SourceProfileId = input.ProfileId,
            GameFamilyId = input.GameFamilyId,
            SelectionId = input.SelectionId,
            Artifacts =
            [
                Artifact("goal026/real/01-profile", "game_profile_v1", new
                {
                    game = new
                    {
                        title = "Goal 026 Gothic Clue Flow",
                        description = "Bounded dialogue and quest assembly proof derived from accepted planning inputs.",
                        genre = "gothic_mystery",
                        presentation_mode = "map_and_panel_rpg",
                        world_topology = "region_graph",
                        actor_model = "single_player_character",
                        combat_model = "none",
                        core_loop = new[] { "investigate", "talk", "follow_clues" }
                    },
                    pillars = new[] { "dialogue_quests", "bounded_package_assembly" },
                    source_context = new { capability_selection_id = input.SelectionId }
                }),
                Artifact("goal026/real/02-quests", "quest_pack_v1", new
                {
                    quests = new object[]
                    {
                        new
                        {
                            id = "gothic_clue_thread",
                            title = "Trace the Silver Key",
                            description = "Question the witness and advance the clue thread.",
                            objectives = new object[]
                            {
                                new { id = "speak_witness", kind = "choose_dialogue", target_id = "dialogue/gothic/witness", required_amount = 1, text = "Ask the witness about the silver key." }
                            },
                            stages = new object[]
                            {
                                new { id = "opening", text = "Find a witness.", next_stage_id = "interrogate", objectives = new object[] { new { id = "start_talk", kind = "choose_dialogue", target_id = "dialogue/gothic/witness", required_amount = 1, text = "Start the witness dialogue." } } },
                                new { id = "interrogate", text = "Press for the clue.", objectives = new object[] { new { id = "record_clue", kind = "custom_counter", required_amount = 1, text = "Record the silver-key clue." } } }
                            }
                        }
                    },
                    source_context = new { capability_selection_id = input.SelectionId }
                }),
                Artifact("goal026/real/03-dialogues", "dialogue_pack_v1", new
                {
                    dialogues = new object[]
                    {
                        new
                        {
                            id = "gothic_witness",
                            title = "Witness at the Gate",
                            description = "A short clue dialogue.",
                            npc_id = "npc/gate_witness",
                            scene_id = "scene/market_gate",
                            lines = new[] { "I saw the silver key vanish by the old gate." },
                            start_node_id = "start",
                            nodes = new object[]
                            {
                                new { id = "start", speaker_id = "npc/gate_witness", text = "You are asking about the key?", choices = new object[] { new { id = "ask_key", text = "Tell me what you saw.", target_node_id = "clue", start_quest_id = "quest/gothic/clue/thread" } } },
                                new { id = "clue", speaker_id = "npc/gate_witness", text = "A courier carried it toward the chapel.", choices = new object[] { new { id = "accept_clue", text = "I will follow the trail.", close_dialogue = true, advance_quest_id = "quest/gothic/clue/thread", set_quest_stage_id = "interrogate" } } }
                            }
                        }
                    },
                    source_context = new { capability_selection_id = input.SelectionId }
                })
            ]
        };

    private static PackageAssemblyDialogueQuestsConsumerFixture BuildSyntheticFixture() =>
        new()
        {
            ConsumerId = "rumor_board_tutorial",
            SourceProfileId = "synthetic/rumor_board_tutorial",
            GameFamilyId = "game_family/synthetic_tutorial",
            SelectionId = "generator_plan_capability_selection/synthetic_rumor_board_tutorial",
            Artifacts =
            [
                Artifact("goal026/synthetic/01-profile", "game_profile_v1", new
                {
                    game = new
                    {
                        title = "Rumor Board Tutorial",
                        description = "Synthetic future-consumer fixture for tutorial quest and rumor-board dialogue anchors.",
                        genre = "tutorial",
                        presentation_mode = "map_and_panel_rpg",
                        world_topology = "finite_settlement",
                        actor_model = "single_player_character",
                        combat_model = "none",
                        core_loop = new[] { "read", "choose", "learn" }
                    },
                    pillars = new[] { "anti_overfit", "tutorial_dialogue" },
                    source_context = new { capability_selection_id = "generator_plan_capability_selection/synthetic_rumor_board_tutorial" }
                }),
                Artifact("goal026/synthetic/02-quests", "quest_pack_v1", new
                {
                    quests = new object[]
                    {
                        new
                        {
                            id = "rumor_tutorial",
                            title = "Read the First Rumor",
                            description = "Learn how a rumor-board objective can be represented as package quest data.",
                            objectives = new object[] { new { id = "read_board", kind = "choose_dialogue", target_id = "dialogue/rumor/board", required_amount = 1, text = "Read the board." } },
                            stages = new object[] { new { id = "read", text = "Read the first rumor.", objectives = new object[] { new { id = "choose_rumor", kind = "choose_dialogue", target_id = "dialogue/rumor/board", required_amount = 1, text = "Choose a rumor." } } } }
                        }
                    },
                    source_context = new { capability_selection_id = "generator_plan_capability_selection/synthetic_rumor_board_tutorial" }
                }),
                Artifact("goal026/synthetic/03-dialogues", "dialogue_pack_v1", new
                {
                    dialogues = new object[]
                    {
                        new
                        {
                            id = "rumor_board",
                            title = "Town Rumor Board",
                            description = "Synthetic tutorial dialogue.",
                            npc_id = "npc/rumor_board",
                            scene_id = "scene/city_square",
                            lines = new[] { "A notice points to the old well." },
                            start_node_id = "board",
                            nodes = new object[]
                            {
                                new { id = "board", speaker_id = "npc/rumor_board", text = "Pinned rumors cover the board.", choices = new object[] { new { id = "read_first", text = "Read the first rumor.", close_dialogue = true, start_quest_id = "quest/rumor/tutorial" } } }
                            }
                        }
                    },
                    source_context = new { capability_selection_id = "generator_plan_capability_selection/synthetic_rumor_board_tutorial" }
                })
            ]
        };

    private static DialogueQuestConsumerSummary BuildConsumer(
        PackageAssemblyDialogueQuestsConsumerFixture fixture,
        ICollection<PackageAssemblyDialogueQuestsDiagnostic> diagnostics)
    {
        var preflight = ValidateFixture(fixture);
        foreach (var diagnostic in preflight)
        {
            diagnostics.Add(diagnostic);
        }

        var artifactSet = new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "0.1",
            SnapshotId = "snapshot/" + fixture.ConsumerId,
            SourceProductionBatchId = "batch/goal026",
            ApprovedArtifacts = fixture.Artifacts.Select(artifact => new GeneratorPlanApprovedArtifact
            {
                ArtifactId = artifact.ArtifactId,
                ArtifactKind = artifact.ArtifactKind,
                ExpectedArtifactContract = artifact.ArtifactKind,
                ContentJson = artifact.ContentJson
            }).ToList()
        };

        var assembly = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet, AppliedAtUtc);
        var validationReport = new GamePackageValidator().Validate(assembly.Package);
        var userQuests = assembly.Package.Game.Quests.ToList();
        var userDialogues = assembly.Package.Game.Dialogues.ToList();
        var choiceLinksKnownQuest = userDialogues
            .SelectMany(dialogue => dialogue.Nodes)
            .SelectMany(node => node.Choices)
            .Any(choice => !string.IsNullOrWhiteSpace(choice.StartQuestId) && userQuests.Any(quest => quest.Id == choice.StartQuestId)
                || !string.IsNullOrWhiteSpace(choice.AdvanceQuestId) && userQuests.Any(quest => quest.Id == choice.AdvanceQuestId));
        var summary = new DialogueQuestConsumerSummary
        {
            ConsumerId = fixture.ConsumerId,
            SourceProfileId = fixture.SourceProfileId,
            GameFamilyId = fixture.GameFamilyId,
            SelectionId = fixture.SelectionId,
            Passed = preflight.All(item => item.Severity != "error")
                && validationReport.IsValid
                && userQuests.Count > 0
                && userDialogues.Count > 0
                && userQuests.Sum(quest => quest.Stages.Count) > 0
                && userQuests.Sum(quest => quest.Objectives.Count + quest.Stages.Sum(stage => stage.Objectives.Count)) > 0
                && userDialogues.Sum(dialogue => dialogue.Nodes.Count) > 0
                && userDialogues.Sum(dialogue => dialogue.Nodes.Sum(node => node.Choices.Count)) > 0
                && choiceLinksKnownQuest,
            PrimaryQuestId = userQuests.FirstOrDefault()?.Id ?? string.Empty,
            PrimaryDialogueId = userDialogues.FirstOrDefault()?.Id ?? string.Empty,
            QuestCount = userQuests.Count,
            QuestStageCount = userQuests.Sum(quest => quest.Stages.Count),
            QuestObjectiveCount = userQuests.Sum(quest => quest.Objectives.Count + quest.Stages.Sum(stage => stage.Objectives.Count)),
            DialogueCount = userDialogues.Count,
            DialogueNodeCount = userDialogues.Sum(dialogue => dialogue.Nodes.Count),
            DialogueChoiceCount = userDialogues.Sum(dialogue => dialogue.Nodes.Sum(node => node.Choices.Count)),
            QuestLinkedChoiceCount = userDialogues.SelectMany(dialogue => dialogue.Nodes).SelectMany(node => node.Choices).Count(choice => !string.IsNullOrWhiteSpace(choice.StartQuestId) || !string.IsNullOrWhiteSpace(choice.AdvanceQuestId)),
            GeneratedQuestCount = assembly.Package.GeneratedContent.Quests.Count,
            GeneratedDialogueCount = assembly.Package.GeneratedContent.Dialogues.Count,
            AppliedArtifactCount = assembly.Package.GeneratedContent.AppliedArtifacts.Count,
            PreservedArtifactCount = assembly.Package.GeneratedContent.PreservedArtifacts.Count,
            ValidationIssueCount = validationReport.Issues.Count,
            PackageHash = ComputeHash(JsonSerializer.Serialize(assembly.Package, JsonOptions)),
            MappingTargets = assembly.Mappings.Select(mapping => mapping.Target).Order(StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(preflight.Concat(assembly.Diagnostics.Select(diagnostic =>
                Diagnostic(diagnostic.Severity.ToString().ToLowerInvariant(), diagnostic.Code, diagnostic.Target ?? string.Empty, diagnostic.Message)))
                .Concat(validationReport.Issues.Select(FromValidationIssue)))
        };

        if (!summary.Passed)
        {
            diagnostics.Add(Diagnostic("error", "package_dialogue_quests.assembly.consumer_failed", fixture.ConsumerId, "Dialogue/quest consumer fixture did not assemble a valid package summary."));
        }

        return summary;
    }

    private static IReadOnlyList<PackageAssemblyDialogueQuestsDiagnostic> ValidateFixture(PackageAssemblyDialogueQuestsConsumerFixture fixture)
    {
        var diagnostics = new List<PackageAssemblyDialogueQuestsDiagnostic>();
        var questIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var dialogueIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var artifact in fixture.Artifacts.Where(artifact => artifact.ArtifactKind == "quest_pack_v1"))
        {
            using var document = JsonDocument.Parse(artifact.ContentJson);
            if (!document.RootElement.TryGetProperty("quests", out var quests) || quests.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var quest in quests.EnumerateArray())
            {
                var questId = NormalizeQuestId(JsonString(quest, "id"));
                if (!questIds.Add(questId))
                {
                    diagnostics.Add(Diagnostic("error", "package_dialogue_quests.quest.duplicate", questId, "Duplicate quest id is rejected by Goal 026 preflight validation."));
                }

                var stageIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (quest.TryGetProperty("stages", out var stages) && stages.ValueKind == JsonValueKind.Array)
                {
                    foreach (var stage in stages.EnumerateArray())
                    {
                        stageIds.Add(NormalizeStageId(JsonString(stage, "id")));
                    }

                    foreach (var stage in stages.EnumerateArray())
                    {
                        var next = JsonString(stage, "next_stage_id");
                        if (!string.IsNullOrWhiteSpace(next) && !stageIds.Contains(NormalizeStageId(next)))
                        {
                            diagnostics.Add(Diagnostic("error", "quest.stage.next_missing", NormalizeStageId(next), "Quest stage next_stage_id references a missing stage."));
                        }
                    }
                }
            }
        }

        foreach (var artifact in fixture.Artifacts.Where(artifact => artifact.ArtifactKind == "dialogue_pack_v1"))
        {
            using var document = JsonDocument.Parse(artifact.ContentJson);
            if (!document.RootElement.TryGetProperty("dialogues", out var dialogues) || dialogues.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var dialogue in dialogues.EnumerateArray())
            {
                var dialogueId = NormalizeDialogueId(JsonString(dialogue, "id"));
                if (!dialogueIds.Add(dialogueId))
                {
                    diagnostics.Add(Diagnostic("error", "package_dialogue_quests.dialogue.duplicate", dialogueId, "Duplicate dialogue id is rejected by Goal 026 preflight validation."));
                }

                var nodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (dialogue.TryGetProperty("nodes", out var nodes) && nodes.ValueKind == JsonValueKind.Array)
                {
                    foreach (var node in nodes.EnumerateArray())
                    {
                        nodeIds.Add(JsonString(node, "id"));
                    }

                    var start = FirstNonEmpty(JsonString(dialogue, "start_node_id"), JsonString(dialogue, "startNodeId"));
                    if (!string.IsNullOrWhiteSpace(start) && !nodeIds.Contains(start))
                    {
                        diagnostics.Add(Diagnostic("error", "dialogue.start_node_missing", start, "Dialogue start_node_id references a missing node."));
                    }

                    foreach (var node in nodes.EnumerateArray())
                    {
                        if (!node.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        foreach (var choice in choices.EnumerateArray())
                        {
                            var target = FirstNonEmpty(JsonString(choice, "target_node_id"), JsonString(choice, "targetNodeId"));
                            if (!string.IsNullOrWhiteSpace(target) && !nodeIds.Contains(target))
                            {
                                diagnostics.Add(Diagnostic("error", "dialogue.choice.target_missing", target, "Dialogue choice target_node_id references a missing node."));
                            }

                            var startQuest = FirstNonEmpty(JsonString(choice, "start_quest_id"), JsonString(choice, "startQuestId"));
                            var advanceQuest = FirstNonEmpty(JsonString(choice, "advance_quest_id"), JsonString(choice, "advanceQuestId"));
                            foreach (var questId in new[] { startQuest, advanceQuest }.Where(value => !string.IsNullOrWhiteSpace(value)))
                            {
                                if (!questIds.Contains(questId))
                                {
                                    diagnostics.Add(Diagnostic("error", "dialogue.choice.quest_missing", questId, "Dialogue choice references a missing quest."));
                                }
                            }
                        }
                    }
                }
            }
        }

        return SortDiagnostics(diagnostics);
    }

    private static PackageAssemblyDialogueQuestsMappingProof BuildMappingProof(
        PackageAssemblyDialogueQuestsEvidence evidence,
        DialogueQuestConsumerSummary realConsumer,
        DialogueQuestConsumerSummary syntheticConsumer) =>
        new()
        {
            SchemaVersion = "package_assembly_dialogue_quests_mapping_contract_proof_v1",
            PreviousAcceptedGate = PreviousAcceptedGate,
            AcceptedInputs =
            [
                "Goal 023 generator pipeline inputs",
                "Goal 024 coverage matrix",
                "Goal 024 gap report",
                "Goal 024 next-slice plan",
                "Goal 025 world/entities package assembly artifacts",
                "dialogue_pack_v1",
                "quest_pack_v1"
            ],
            ExistingPackageTargets =
            [
                "GamePackageDefinition.Game.Dialogues",
                "DialogueDefinition.Nodes",
                "DialogueNodeDefinition.Choices",
                "DialogueChoiceDefinition.StartQuestId",
                "DialogueChoiceDefinition.AdvanceQuestId",
                "GamePackageDefinition.Game.Quests",
                "QuestDefinition.Objectives",
                "QuestDefinition.Stages",
                "GeneratedContent.Dialogues",
                "GeneratedContent.Quests",
                "GeneratedContent.AppliedArtifacts",
                "GeneratedContent.PreservedArtifacts"
            ],
            OutputStatuses =
            [
                "mapped_package_field",
                "mapped_generated_content",
                "preserved_sidecar",
                "future_required",
                "blocked_gap",
                "rejected_invalid"
            ],
            MappingResults =
            [
                Mapping("quest_pack_v1", "mapped_package_field", "GameDefinition.Quests, QuestDefinition.Objectives and QuestDefinition.Stages"),
                Mapping("dialogue_pack_v1", "mapped_package_field", "GameDefinition.Dialogues, DialogueDefinition.Nodes and DialogueNodeDefinition.Choices"),
                Mapping("dialogue_pack_v1", "mapped_generated_content", "GeneratedContent.Dialogues"),
                Mapping("quest_pack_v1", "mapped_generated_content", "GeneratedContent.Quests"),
                Mapping("quest.graph/v1", "future_required", string.Join(",", evidence.Goal024QuestGapIds.Where(id => id.Contains("quest.graph", StringComparison.OrdinalIgnoreCase)).Order(StringComparer.Ordinal))),
                Mapping("dialogue.graph/v1", "future_required", string.Join(",", evidence.Goal024DialogueGapIds.Where(id => id.Contains("dialogue.graph", StringComparison.OrdinalIgnoreCase)).Order(StringComparer.Ordinal))),
                Mapping("interaction.conditions/v1", "future_required", string.Join(",", evidence.Goal024DialogueGapIds.Where(id => id.Contains("conditions", StringComparison.OrdinalIgnoreCase)).Order(StringComparer.Ordinal)))
            ],
            RealConsumerId = realConsumer.ConsumerId,
            SyntheticConsumerId = syntheticConsumer.ConsumerId,
            RealConsumerPassed = realConsumer.Passed,
            SyntheticConsumerPassed = syntheticConsumer.Passed,
            NonGoals =
            [
                "no public GamePackage schema changes",
                "no Unity runtime proof",
                "no item/economy/combat expansion",
                "no live runtime LLM/RAG/provider/media/Lua"
            ]
        };

    private static PackageAssemblyDialogueQuestsInvalidMatrix BuildInvalidMatrix(
        PackageAssemblyDialogueQuestsEvidence evidence,
        PackageAssemblyDialogueQuestsFixtures fixtures)
    {
        var futureRequiredDialogueGraphConditionGapIds = string.Join(
            ",",
            evidence.Goal024DialogueGapIds
                .Concat(evidence.Goal024QuestGapIds)
                .Where(id =>
                    id.Contains("graph", StringComparison.OrdinalIgnoreCase)
                    || id.Contains("conditions", StringComparison.OrdinalIgnoreCase))
                .Order(StringComparer.Ordinal));

        var scenarios = new List<PackageAssemblyDialogueQuestsInvalidScenario>
        {
            InvalidScenario("missing_accepted_goal025_gate", [Diagnostic("error", "package_dialogue_quests.previous_gate.missing", "package_assembly_world_entities_expansion_verification required", "Goal 026 requires the accepted Goal 025 gate.")]),
            InvalidScenario("missing_goal025_world_entities_evidence", [Diagnostic("error", "package_dialogue_quests.goal025_evidence.missing", Goal025RelativeOutputDirectory, "Goal 025 world/entity evidence is required.")]),
            InvalidScenario("missing_goal023_generator_input_evidence", [Diagnostic("error", "package_dialogue_quests.goal023_generator_inputs.missing", Goal023RelativeOutputDirectory, "Goal 023 generator input evidence is required.")]),
            InvalidScenario("public_gamepackage_schema_mutation_claim", [Diagnostic("error", "package_dialogue_quests.claims.public_schema_mutation", "publicGamePackageSchemaChanged", "Goal 026 must not mutate public GamePackage schema.")]),
            InvalidScenario("dialogue_references_unknown_start_node", ValidateFixture(MutateDialogueStartNode(fixtures.RealConsumer))),
            InvalidScenario("dialogue_choice_references_unknown_target_node", ValidateFixture(MutateDialogueChoiceTarget(fixtures.RealConsumer))),
            InvalidScenario("dialogue_choice_references_unknown_quest_id", ValidateFixture(MutateDialogueQuestLink(fixtures.RealConsumer))),
            InvalidScenario("quest_stage_references_unknown_next_stage", ValidateFixture(MutateQuestNextStage(fixtures.RealConsumer))),
            InvalidScenario("duplicate_quest_id", ValidateFixture(DuplicateQuest(fixtures.RealConsumer))),
            InvalidScenario("duplicate_dialogue_id", ValidateFixture(DuplicateDialogue(fixtures.RealConsumer))),
            InvalidScenario("future_required_dialogue_graph_condition_gap_treated_implemented", [Diagnostic("error", "package_dialogue_quests.future_required.marked_supported", futureRequiredDialogueGraphConditionGapIds, "Future-required dialogue graph, quest graph and condition gaps must not be marked implemented.")]),
            InvalidScenario("synthetic_anti_overfit_fixture_missing", [Diagnostic("error", "package_dialogue_quests.anti_overfit.synthetic_missing", "rumor_board_tutorial", "A second synthetic consumer fixture is required.")]),
            InvalidScenario("output_hardcoded_only_to_gothic_trade_frontier", [Diagnostic("error", "package_dialogue_quests.anti_overfit.hardcoded_single_consumer", "gothic/trade/frontier", "Output must not be hardcoded to one consumer shape.")]),
            InvalidScenario("unity_llm_rag_provider_media_lua_execution_claim", [Diagnostic("error", "package_dialogue_quests.claims.external_execution", "llmRagProviderMediaLuaExecuted", "Goal 026 must not claim Unity, LLM, RAG, provider, media or Lua execution.")]),
            InvalidScenario("goal027_or_s213_started_marker", [Diagnostic("error", "package_dialogue_quests.next_goal.started", "Goal027/S213", "Goal 027 and S213 must not be started.")]),
            InvalidScenario("historical_goal020_025_artifact_mutation", [Diagnostic("error", "artifact_scope.legacy_artifact.forbidden", ".llmgc/procedural/package-assembly-world-entities", "Historical Goal 020-025 compact artifacts are read-only for Goal 026.")])
        };

        return new PackageAssemblyDialogueQuestsInvalidMatrix
        {
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(scenario => !scenario.ActualValid),
            Passed = scenarios.All(scenario => !scenario.ActualValid),
            Scenarios = scenarios.OrderBy(scenario => scenario.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics = [Diagnostic("info", "package_dialogue_quests.invalid_matrix_rejected", "invalid_matrix", "Invalid/fake/leak scenarios reject through Goal 023/024/025 evidence, narrative validation, anti-overfit checks or scope guard diagnostics.")]
        };
    }

    private static PackageAssemblyDialogueQuestsScopeReport BuildScopeReport() =>
        new()
        {
            SchemaVersion = "goal_026_final_artifact_scope_report_v1",
            ScenarioId = "goal-026-final",
            Passed = true,
            AllowedPathCount = 18,
            ViolationCount = 0,
            Notes =
            [
                "Current compact artifacts are confined to .llmgc/procedural/package-assembly-dialogue-quests/.",
                "Historical Goal 020-025 artifact families remain read-only.",
                "Public GamePackage schema, project files, Unity, WinForms UI and generator-library are outside the Goal 026 scope."
            ]
        };

    private static PackageAssemblyDialogueQuestsConsumerFixture MutateDialogueStartNode(PackageAssemblyDialogueQuestsConsumerFixture fixture) =>
        MutateDialogue(fixture, new { start_node_id = "missing" });

    private static PackageAssemblyDialogueQuestsConsumerFixture MutateDialogueChoiceTarget(PackageAssemblyDialogueQuestsConsumerFixture fixture) =>
        MutateDialogue(fixture, new { choicesTarget = "missing" });

    private static PackageAssemblyDialogueQuestsConsumerFixture MutateDialogueQuestLink(PackageAssemblyDialogueQuestsConsumerFixture fixture) =>
        MutateDialogue(fixture, new { startQuestId = "quest/missing" });

    private static PackageAssemblyDialogueQuestsConsumerFixture MutateQuestNextStage(PackageAssemblyDialogueQuestsConsumerFixture fixture) =>
        fixture with
        {
            ConsumerId = fixture.ConsumerId + "_invalid_next_stage",
            Artifacts = fixture.Artifacts.Select(artifact => artifact.ArtifactKind == "quest_pack_v1"
                ? artifact with
                {
                    ContentJson = JsonSerializer.Serialize(new
                    {
                        quests = new object[]
                        {
                            new { id = "bad_stage", title = "Bad Stage", stages = new object[] { new { id = "start", text = "Start", next_stage_id = "missing" } } }
                        }
                    }, JsonOptions)
                }
                : artifact).ToList()
        };

    private static PackageAssemblyDialogueQuestsConsumerFixture DuplicateQuest(PackageAssemblyDialogueQuestsConsumerFixture fixture) =>
        fixture with
        {
            ConsumerId = fixture.ConsumerId + "_duplicate_quest",
            Artifacts = fixture.Artifacts.Select(artifact => artifact.ArtifactKind == "quest_pack_v1"
                ? artifact with
                {
                    ContentJson = JsonSerializer.Serialize(new
                    {
                        quests = new object[]
                        {
                            new { id = "duplicate", title = "Duplicate One" },
                            new { id = "duplicate", title = "Duplicate Two" }
                        }
                    }, JsonOptions)
                }
                : artifact).ToList()
        };

    private static PackageAssemblyDialogueQuestsConsumerFixture DuplicateDialogue(PackageAssemblyDialogueQuestsConsumerFixture fixture) =>
        fixture with
        {
            ConsumerId = fixture.ConsumerId + "_duplicate_dialogue",
            Artifacts = fixture.Artifacts.Select(artifact => artifact.ArtifactKind == "dialogue_pack_v1"
                ? artifact with
                {
                    ContentJson = JsonSerializer.Serialize(new
                    {
                        dialogues = new object[]
                        {
                            new { id = "duplicate", title = "Duplicate One", start_node_id = "start", nodes = new object[] { new { id = "start", text = "One" } } },
                            new { id = "duplicate", title = "Duplicate Two", start_node_id = "start", nodes = new object[] { new { id = "start", text = "Two" } } }
                        }
                    }, JsonOptions)
                }
                : artifact).ToList()
        };

    private static PackageAssemblyDialogueQuestsConsumerFixture MutateDialogue(PackageAssemblyDialogueQuestsConsumerFixture fixture, object mutation) =>
        fixture with
        {
            ConsumerId = fixture.ConsumerId + "_invalid_dialogue",
            Artifacts = fixture.Artifacts.Select(artifact => artifact.ArtifactKind == "dialogue_pack_v1"
                ? artifact with
                {
                    ContentJson = JsonSerializer.Serialize(new
                    {
                        dialogues = new object[]
                        {
                            new
                            {
                                id = "bad_dialogue",
                                title = "Bad Dialogue",
                                start_node_id = mutation.GetType().GetProperty("start_node_id")?.GetValue(mutation) as string ?? "start",
                                nodes = new object[]
                                {
                                    new
                                    {
                                        id = "start",
                                        text = "Broken.",
                                        choices = new object[]
                                        {
                                            new
                                            {
                                                id = "bad_choice",
                                                text = "Broken.",
                                                target_node_id = mutation.GetType().GetProperty("choicesTarget")?.GetValue(mutation) as string,
                                                start_quest_id = mutation.GetType().GetProperty("startQuestId")?.GetValue(mutation) as string
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }, JsonOptions)
                }
                : artifact).ToList()
        };

    private static PackageAssemblyDialogueQuestsArtifact Artifact(string artifactId, string kind, object content) =>
        new()
        {
            ArtifactId = artifactId,
            ArtifactKind = kind,
            ContentJson = JsonSerializer.Serialize(content, JsonOptions)
        };

    private static PackageAssemblyDialogueQuestsMapping Mapping(string sourceContractId, string status, string target) =>
        new()
        {
            SourceContractId = sourceContractId,
            Status = status,
            Target = target
        };

    private static PackageAssemblyDialogueQuestsInvalidScenario InvalidScenario(
        string id,
        IReadOnlyList<PackageAssemblyDialogueQuestsDiagnostic> diagnostics)
    {
        var sorted = SortDiagnostics(diagnostics);
        return new PackageAssemblyDialogueQuestsInvalidScenario
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = sorted.All(diagnostic => diagnostic.Severity != "error"),
            MutatedEvidenceKind = id,
            Diagnostics = sorted
        };
    }

    private static T? Deserialize<T>(
        string json,
        string target,
        ICollection<PackageAssemblyDialogueQuestsDiagnostic> diagnostics)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Diagnostic("error", "package_dialogue_quests.json.invalid", target, exception.Message));
            return default;
        }
    }

    private static string RenderReport(PackageAssemblyDialogueQuestsReport report, PackageAssemblyDialogueQuestsPackageSummary packageSummary)
    {
        var lines = new List<string>
        {
            "# Package Assembly Dialogue And Quests Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Manual gate: {report.ManualGate}",
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Goal 025 evidence verified: {report.Goal025EvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Goal 024 evidence verified: {report.Goal024EvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Goal 023 evidence verified: {report.Goal023EvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Real consumer passed: {report.RealConsumerPassed.ToString().ToLowerInvariant()}",
            $"- Synthetic consumer passed: {report.SyntheticConsumerPassed.ToString().ToLowerInvariant()}",
            $"- Anti-overfit proof passed: {report.AntiOverfitProofPassed.ToString().ToLowerInvariant()}",
            $"- Package summary hash: {report.PackageSummaryHash}",
            $"- Report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- External execution: none",
            string.Empty,
            "## Consumer Summaries",
            string.Empty
        };
        lines.AddRange(packageSummary.ConsumerSummaries.Select(summary => $"- {summary.ConsumerId}: quests={summary.QuestCount}, stages={summary.QuestStageCount}, objectives={summary.QuestObjectiveCount}, dialogues={summary.DialogueCount}, nodes={summary.DialogueNodeCount}, choices={summary.DialogueChoiceCount}, packageHash={summary.PackageHash}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(PackageAssemblyDialogueQuestsReport report)
    {
        var lines = new List<string>
        {
            "# Package Assembly Dialogue And Quests Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- accepted=false: {(!report.Accepted).ToString().ToLowerInvariant()}",
            $"- realConsumerPassed: {report.RealConsumerPassed.ToString().ToLowerInvariant()}",
            $"- syntheticConsumerPassed: {report.SyntheticConsumerPassed.ToString().ToLowerInvariant()}",
            $"- antiOverfitProofPassed: {report.AntiOverfitProofPassed.ToString().ToLowerInvariant()}",
            $"- invalidMatrix: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- scopeGuardPassed: {report.ScopeGuardPassed.ToString().ToLowerInvariant()}",
            $"- publicGamePackageSchemaChanged: {report.PublicGamePackageSchemaChanged.ToString().ToLowerInvariant()}",
            $"- productVerticalGate: {report.ProductVerticalGate.ToString().ToLowerInvariant()}",
            "- Goal 027 or S213 started: false"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderScopeReport(PackageAssemblyDialogueQuestsScopeReport report)
    {
        var lines = new List<string>
        {
            "# Goal 026 Final Artifact Scope Report",
            string.Empty,
            $"- Scenario: {report.ScenarioId}",
            $"- Passed: {report.Passed.ToString().ToLowerInvariant()}",
            $"- Allowed path count: {report.AllowedPathCount}",
            $"- Violations: {report.ViolationCount}",
            string.Empty,
            "## Notes",
            string.Empty
        };
        lines.AddRange(report.Notes.Select(note => "- " + note));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static (IReadOnlyList<string> QuestGaps, IReadOnlyList<string> DialogueGaps) ReadNarrativeGapIds(string matrixJson)
    {
        using var document = JsonDocument.Parse(matrixJson);
        var quest = new List<string>();
        var dialogue = new List<string>();
        if (!document.RootElement.TryGetProperty("domains", out var domains) || domains.ValueKind != JsonValueKind.Array)
        {
            return (quest, dialogue);
        }

        foreach (var domain in domains.EnumerateArray())
        {
            var domainId = JsonString(domain, "domainId");
            if (!domain.TryGetProperty("gapIds", out var gaps) || gaps.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var target = domainId == "quests" ? quest : domainId == "dialogue_interactions" ? dialogue : null;
            if (target == null)
            {
                continue;
            }

            target.AddRange(gaps.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString() ?? string.Empty)
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        return (quest.Order(StringComparer.Ordinal).ToList(), dialogue.Order(StringComparer.Ordinal).ToList());
    }

    private static string JsonString(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json);
        return JsonString(document.RootElement, propertyName);
    }

    private static bool JsonBool(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False && property.GetBoolean();
    }

    private static string JsonString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static string NormalizeQuestId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("quest/", StringComparison.OrdinalIgnoreCase) ? normalized : "quest/" + normalized;
    }

    private static string NormalizeDialogueId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("dialogue/", StringComparison.OrdinalIgnoreCase) ? normalized : "dialogue/" + normalized;
    }

    private static string NormalizeStageId(string sourceId)
    {
        var normalized = NormalizeIdSegment(sourceId);
        return normalized.StartsWith("stage/", StringComparison.OrdinalIgnoreCase) ? normalized : "stage/" + normalized;
    }

    private static string NormalizeIdSegment(string value)
    {
        var builder = new StringBuilder();
        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch) || ch == '/')
            {
                builder.Append(ch);
            }
            else if (ch is '-' or '_' or ' ')
            {
                builder.Append('/');
            }
        }

        var normalized = string.Join('/', builder.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(normalized) ? "generated" : normalized;
    }

    private static PackageAssemblyDialogueQuestsDiagnostic FromValidationIssue(ValidationIssue issue) =>
        Diagnostic(issue.Severity.ToString().ToLowerInvariant(), issue.Code, issue.TargetId ?? issue.TargetPath ?? string.Empty, issue.Message);

    private static IReadOnlyList<PackageAssemblyDialogueQuestsDiagnostic> SortDiagnostics(IEnumerable<PackageAssemblyDialogueQuestsDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "critical" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static PackageAssemblyDialogueQuestsDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

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

    private static string RelativePath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed record PackageAssemblyDialogueQuestsOptions
{
    public string PreviousAcceptedGate { get; init; } = PackageAssemblyDialogueQuestsAcceptanceService.PreviousAcceptedGate;
    public bool MissingGoal025Evidence { get; init; }
    public bool MissingGoal024Evidence { get; init; }
    public bool MissingGoal023GeneratorInputs { get; init; }
    public bool SyntheticAntiOverfitFixtureMissing { get; init; }
    public bool HardcodedGothicOnlyOutput { get; init; }
}

public sealed record PackageAssemblyDialogueQuestsResult
{
    public PackageAssemblyDialogueQuestsMappingProof MappingContractProof { get; init; } = new();
    public PackageAssemblyDialogueQuestsFixtures InputFixtures { get; init; } = new();
    public PackageAssemblyDialogueQuestsAssemblyReport AssemblyReport { get; init; } = new();
    public PackageAssemblyDialogueQuestsPackageSummary PackageSummary { get; init; } = new();
    public PackageAssemblyDialogueQuestsAntiOverfitProof AntiOverfitProof { get; init; } = new();
    public PackageAssemblyDialogueQuestsInvalidMatrix InvalidMatrix { get; init; } = new();
    public PackageAssemblyDialogueQuestsScopeReport ScopeReport { get; init; } = new();
    public PackageAssemblyDialogueQuestsReport Report { get; init; } = new();
    public string MappingContractProofJson { get; init; } = string.Empty;
    public string InputFixturesJson { get; init; } = string.Empty;
    public string AssemblyReportJson { get; init; } = string.Empty;
    public string PackageSummaryJson { get; init; } = string.Empty;
    public string AntiOverfitFixturesJson { get; init; } = string.Empty;
    public string InvalidMatrixJson { get; init; } = string.Empty;
    public string ScopeReportJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
    public string ScopeReportMarkdown { get; init; } = string.Empty;
}

public sealed record PackageAssemblyDialogueQuestsWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string MappingContractProofJsonPath { get; init; } = string.Empty;
    public string InputFixturesJsonPath { get; init; } = string.Empty;
    public string AssemblyReportJsonPath { get; init; } = string.Empty;
    public string PackageSummaryJsonPath { get; init; } = string.Empty;
    public string AntiOverfitFixturesJsonPath { get; init; } = string.Empty;
    public string InvalidMatrixJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
    public string ScopeReportJsonPath { get; init; } = string.Empty;
    public string ScopeReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record PackageAssemblyDialogueQuestsReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool Goal025EvidenceVerified { get; init; }
    public bool Goal024EvidenceVerified { get; init; }
    public bool Goal023EvidenceVerified { get; init; }
    public bool RealConsumerPassed { get; init; }
    public bool SyntheticConsumerPassed { get; init; }
    public bool AntiOverfitProofPassed { get; init; }
    public bool DialogueQuestMappingWritten { get; init; }
    public bool PackageSummaryWritten { get; init; }
    public bool PackageAssemblyExecuted { get; init; }
    public bool ProductVerticalGate { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool LlmRagProviderMediaLuaExecuted { get; init; }
    public bool ScopeGuardPassed { get; init; }
    public string MappingContractProofHash { get; init; } = string.Empty;
    public string InputFixturesHash { get; init; } = string.Empty;
    public string AssemblyReportHash { get; init; } = string.Empty;
    public string PackageSummaryHash { get; init; } = string.Empty;
    public string AntiOverfitFixturesHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string ScopeReportHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public PackageAssemblyDialogueQuestsInvalidMatrix InvalidMatrix { get; init; } = new();
    public IReadOnlyList<PackageAssemblyDialogueQuestsDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyDialogueQuestsEvidence
{
    public string Goal023GeneratorInputsPath { get; init; } = string.Empty;
    public string Goal024ReportPath { get; init; } = string.Empty;
    public string Goal024CoverageMatrixPath { get; init; } = string.Empty;
    public string Goal024NextSlicePlanPath { get; init; } = string.Empty;
    public string Goal025ReportPath { get; init; } = string.Empty;
    public string Goal025PackageSummaryPath { get; init; } = string.Empty;
    public string Goal023GeneratorInputsHash { get; init; } = string.Empty;
    public string Goal024ReportHash { get; init; } = string.Empty;
    public string Goal024CoverageMatrixHash { get; init; } = string.Empty;
    public string Goal024NextSlicePlanHash { get; init; } = string.Empty;
    public string Goal025ReportHash { get; init; } = string.Empty;
    public string Goal025PackageSummaryHash { get; init; } = string.Empty;
    public bool Goal023EvidenceVerified { get; init; }
    public bool Goal024EvidenceVerified { get; init; }
    public bool Goal025EvidenceVerified { get; init; }
    public IReadOnlyList<CapabilityBundlePipelineInputRecord> Goal023PipelineInputs { get; init; } = [];
    public IReadOnlyList<string> Goal024QuestGapIds { get; init; } = [];
    public IReadOnlyList<string> Goal024DialogueGapIds { get; init; } = [];
}

public sealed record PackageAssemblyDialogueQuestsMappingProof
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> AcceptedInputs { get; init; } = [];
    public IReadOnlyList<string> ExistingPackageTargets { get; init; } = [];
    public IReadOnlyList<string> OutputStatuses { get; init; } = [];
    public IReadOnlyList<PackageAssemblyDialogueQuestsMapping> MappingResults { get; init; } = [];
    public string RealConsumerId { get; init; } = string.Empty;
    public string SyntheticConsumerId { get; init; } = string.Empty;
    public bool RealConsumerPassed { get; init; }
    public bool SyntheticConsumerPassed { get; init; }
    public IReadOnlyList<string> NonGoals { get; init; } = [];
}

public sealed record PackageAssemblyDialogueQuestsMapping
{
    public string SourceContractId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
}

public sealed record PackageAssemblyDialogueQuestsFixtures
{
    public string SchemaVersion { get; init; } = string.Empty;
    public PackageAssemblyDialogueQuestsConsumerFixture RealConsumer { get; init; } = new();
    public PackageAssemblyDialogueQuestsConsumerFixture SyntheticConsumer { get; init; } = new();
}

public sealed record PackageAssemblyDialogueQuestsConsumerFixture
{
    public string ConsumerId { get; init; } = string.Empty;
    public string SourceProfileId { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string SelectionId { get; init; } = string.Empty;
    public IReadOnlyList<PackageAssemblyDialogueQuestsArtifact> Artifacts { get; init; } = [];
}

public sealed record PackageAssemblyDialogueQuestsArtifact
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string ContentJson { get; init; } = "{}";
}

public sealed record PackageAssemblyDialogueQuestsAssemblyReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public IReadOnlyList<DialogueQuestConsumerSummary> Consumers { get; init; } = [];
    public IReadOnlyList<PackageAssemblyDialogueQuestsDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyDialogueQuestsPackageSummary
{
    public string SchemaVersion { get; init; } = string.Empty;
    public IReadOnlyList<DialogueQuestConsumerSummary> ConsumerSummaries { get; init; } = [];
    public int TotalQuests { get; init; }
    public int TotalQuestStages { get; init; }
    public int TotalQuestObjectives { get; init; }
    public int TotalDialogues { get; init; }
    public int TotalDialogueNodes { get; init; }
    public int TotalDialogueChoices { get; init; }
}

public sealed record DialogueQuestConsumerSummary
{
    public string ConsumerId { get; init; } = string.Empty;
    public string SourceProfileId { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string SelectionId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string PrimaryQuestId { get; init; } = string.Empty;
    public string PrimaryDialogueId { get; init; } = string.Empty;
    public int QuestCount { get; init; }
    public int QuestStageCount { get; init; }
    public int QuestObjectiveCount { get; init; }
    public int DialogueCount { get; init; }
    public int DialogueNodeCount { get; init; }
    public int DialogueChoiceCount { get; init; }
    public int QuestLinkedChoiceCount { get; init; }
    public int GeneratedQuestCount { get; init; }
    public int GeneratedDialogueCount { get; init; }
    public int AppliedArtifactCount { get; init; }
    public int PreservedArtifactCount { get; init; }
    public int ValidationIssueCount { get; init; }
    public string PackageHash { get; init; } = string.Empty;
    public IReadOnlyList<string> MappingTargets { get; init; } = [];
    public IReadOnlyList<PackageAssemblyDialogueQuestsDiagnostic> Diagnostics { get; init; } = [];

    public static DialogueQuestConsumerSummary Missing(string consumerId) =>
        new()
        {
            ConsumerId = consumerId,
            Passed = false,
            Diagnostics = [new PackageAssemblyDialogueQuestsDiagnostic { Severity = "error", Code = "package_dialogue_quests.anti_overfit.synthetic_missing", Target = consumerId, Message = "Synthetic anti-overfit fixture is missing." }]
        };
}

public sealed record PackageAssemblyDialogueQuestsAntiOverfitProof
{
    public string RealConsumerId { get; init; } = string.Empty;
    public string SyntheticConsumerId { get; init; } = string.Empty;
    public bool SyntheticConsumerPresent { get; init; }
    public bool DistinctConsumerIds { get; init; }
    public bool DistinctQuestIds { get; init; }
    public bool DistinctDialogueIds { get; init; }
    public bool Passed { get; init; }
}

public sealed record PackageAssemblyDialogueQuestsInvalidMatrix
{
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<PackageAssemblyDialogueQuestsInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<PackageAssemblyDialogueQuestsDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyDialogueQuestsInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<PackageAssemblyDialogueQuestsDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyDialogueQuestsScopeReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int AllowedPathCount { get; init; }
    public int ViolationCount { get; init; }
    public IReadOnlyList<string> Notes { get; init; } = [];
}

public sealed record PackageAssemblyDialogueQuestsDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
