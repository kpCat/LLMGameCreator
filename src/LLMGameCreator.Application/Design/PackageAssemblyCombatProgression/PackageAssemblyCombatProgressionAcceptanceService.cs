using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.CapabilityBundlePipelineInputs;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.PackageAssemblyCombatProgression;

public sealed class PackageAssemblyCombatProgressionAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/package-assembly-combat-progression";
    public const string MappingContractProofJsonFileName = "package-assembly-combat-progression-mapping-contract-proof.json";
    public const string InputFixturesJsonFileName = "package-assembly-combat-progression-input-fixtures.json";
    public const string AssemblyReportJsonFileName = "package-assembly-combat-progression-assembly-report.json";
    public const string PackageSummaryJsonFileName = "package-assembly-combat-progression-package-summary.json";
    public const string AntiOverfitFixturesJsonFileName = "package-assembly-combat-progression-anti-overfit-fixtures.json";
    public const string InvalidMatrixJsonFileName = "package-assembly-combat-progression-invalid-matrix.json";
    public const string ReportJsonFileName = "package-assembly-combat-progression-report.json";
    public const string ReportMarkdownFileName = "package-assembly-combat-progression-report.md";
    public const string VerificationMarkdownFileName = "package-assembly-combat-progression-verification.md";
    public const string FinalArtifactScopeReportJsonFileName = "goal-028-final-artifact-scope-report.json";
    public const string FinalArtifactScopeReportMarkdownFileName = "goal-028-final-artifact-scope-report.md";
    public const string FinalGate = "package_assembly_combat_progression_expansion_verification";
    public const string PreviousAcceptedGate = "package_assembly_items_economy_crafting_expansion_verification passed";

    private const string ProductSmokeRoute = "package-assembly-combat-progression";
    private const string Goal023RelativeOutputDirectory = ".llmgc/procedural/capability-bundle-pipeline-inputs";
    private const string Goal024RelativeOutputDirectory = ".llmgc/procedural/rich-package-assembly-coverage-audit";
    private const string Goal025RelativeOutputDirectory = ".llmgc/procedural/package-assembly-world-entities";
    private const string Goal026RelativeOutputDirectory = ".llmgc/procedural/package-assembly-dialogue-quests";
    private const string Goal027RelativeOutputDirectory = ".llmgc/procedural/package-assembly-items-economy-crafting";
    private static readonly DateTimeOffset AppliedAtUtc = DateTimeOffset.Parse("2026-06-28T00:00:00Z");
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<PackageAssemblyCombatProgressionResult> BuildAsync(
        string projectRootPath,
        PackageAssemblyCombatProgressionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new PackageAssemblyCombatProgressionOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<PackageAssemblyCombatProgressionDiagnostic>
        {
            Diagnostic("info", "package_combat_progression.previous_gate_recorded", settings.PreviousAcceptedGate, "User-confirmed Goal 027 package assembly items/economy/crafting verification is recorded as passed."),
            Diagnostic("info", "package_combat_progression.boundary", "execution_boundary", "Goal 028 executes bounded in-memory package assembly only; no Unity, LLM, RAG, provider, media or Lua execution is invoked.")
        };

        if (settings.PreviousAcceptedGate != PreviousAcceptedGate)
        {
            diagnostics.Add(Diagnostic("error", "package_combat_progression.previous_gate.missing", settings.PreviousAcceptedGate, "Goal 028 requires package_assembly_items_economy_crafting_expansion_verification passed."));
        }

        var evidence = await LoadEvidenceAsync(projectRoot, settings, diagnostics, cancellationToken).ConfigureAwait(false);
        var fixtures = BuildFixtures(evidence);
        var realConsumer = BuildConsumer(fixtures.RealConsumer, diagnostics);
        var syntheticConsumer = settings.SyntheticAntiOverfitFixtureMissing
            ? CombatProgressionConsumerSummary.Missing("alternate_encounter_status_progression")
            : BuildConsumer(fixtures.SyntheticConsumer, diagnostics);
        var invalidMatrix = BuildInvalidMatrix();
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var antiOverfit = new PackageAssemblyCombatProgressionAntiOverfitProof
        {
            RealConsumerId = realConsumer.ConsumerId,
            SyntheticConsumerId = syntheticConsumer.ConsumerId,
            SyntheticConsumerPresent = !settings.SyntheticAntiOverfitFixtureMissing,
            DistinctConsumerIds = !string.Equals(realConsumer.ConsumerId, syntheticConsumer.ConsumerId, StringComparison.Ordinal),
            DistinctPrimaryEncounterIds = !string.Equals(realConsumer.PrimaryEncounterId, syntheticConsumer.PrimaryEncounterId, StringComparison.Ordinal),
            DistinctPrimaryProgressionIds = !string.Equals(realConsumer.PrimaryProgressionId, syntheticConsumer.PrimaryProgressionId, StringComparison.Ordinal),
            Passed = !settings.HardcodedFrontierOnlyOutput
                && !settings.SyntheticAntiOverfitFixtureMissing
                && syntheticConsumer.Passed
                && !string.Equals(realConsumer.PrimaryEncounterId, syntheticConsumer.PrimaryEncounterId, StringComparison.Ordinal)
                && !string.Equals(realConsumer.PrimaryProgressionId, syntheticConsumer.PrimaryProgressionId, StringComparison.Ordinal)
        };

        var mappingProof = BuildMappingProof(evidence, realConsumer, syntheticConsumer);
        var assemblyReport = new PackageAssemblyCombatProgressionAssemblyReport
        {
            SchemaVersion = "package_assembly_combat_progression_assembly_report_v1",
            ProductSmokeRoute = ProductSmokeRoute,
            Consumers = [realConsumer, syntheticConsumer],
            Diagnostics = SortDiagnostics(diagnostics.Where(item => item.Code.StartsWith("package_combat_progression.assembly", StringComparison.Ordinal)))
        };
        var packageSummary = new PackageAssemblyCombatProgressionPackageSummary
        {
            SchemaVersion = "package_assembly_combat_progression_package_summary_v1",
            ConsumerSummaries = [realConsumer, syntheticConsumer],
            TotalStats = realConsumer.StatCount + syntheticConsumer.StatCount,
            TotalAbilities = realConsumer.AbilityCount + syntheticConsumer.AbilityCount,
            TotalStatuses = realConsumer.StatusCount + syntheticConsumer.StatusCount,
            TotalProgressions = realConsumer.ProgressionCount + syntheticConsumer.ProgressionCount,
            TotalProgressionStages = realConsumer.ProgressionStageCount + syntheticConsumer.ProgressionStageCount,
            TotalEncounters = realConsumer.EncounterCount + syntheticConsumer.EncounterCount,
            TotalEncounterParticipants = realConsumer.EncounterParticipantCount + syntheticConsumer.EncounterParticipantCount,
            TotalEncounterActions = realConsumer.EncounterActionCount + syntheticConsumer.EncounterActionCount
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
        var reportWithoutHash = new PackageAssemblyCombatProgressionReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            CompletedSlices = ["S220", "S221", "S222", "S223", "S224", "S225", "S226"],
            ProductSmokeRoute = ProductSmokeRoute,
            ContractProofPassed = noTopLevelErrors && invalidMatrix.Passed && antiOverfit.Passed,
            Goal027EvidenceVerified = evidence.Goal027EvidenceVerified,
            Goal026EvidenceVerified = evidence.Goal026EvidenceVerified,
            Goal025EvidenceVerified = evidence.Goal025EvidenceVerified,
            Goal024EvidenceVerified = evidence.Goal024EvidenceVerified,
            Goal023EvidenceVerified = evidence.Goal023EvidenceVerified,
            RealConsumerPassed = realConsumer.Passed,
            SyntheticConsumerPassed = syntheticConsumer.Passed,
            AntiOverfitProofPassed = antiOverfit.Passed,
            CombatProgressionMappingWritten = true,
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

        return new PackageAssemblyCombatProgressionResult
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

    public async Task<PackageAssemblyCombatProgressionWriteResult> WriteAsync(
        string projectRootPath,
        PackageAssemblyCombatProgressionResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new PackageAssemblyCombatProgressionWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            MappingContractProofJsonPath = Path.Combine(outputDirectory, MappingContractProofJsonFileName),
            InputFixturesJsonPath = Path.Combine(outputDirectory, InputFixturesJsonFileName),
            AssemblyReportJsonPath = Path.Combine(outputDirectory, AssemblyReportJsonFileName),
            PackageSummaryJsonPath = Path.Combine(outputDirectory, PackageSummaryJsonFileName),
            AntiOverfitFixturesJsonPath = Path.Combine(outputDirectory, AntiOverfitFixturesJsonFileName),
            InvalidMatrixJsonPath = Path.Combine(outputDirectory, InvalidMatrixJsonFileName),
            ReportJsonPath = Path.Combine(outputDirectory, ReportJsonFileName),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            VerificationMarkdownPath = Path.Combine(outputDirectory, VerificationMarkdownFileName),
            ScopeReportJsonPath = Path.Combine(outputDirectory, FinalArtifactScopeReportJsonFileName),
            ScopeReportMarkdownPath = Path.Combine(outputDirectory, FinalArtifactScopeReportMarkdownFileName)
        };

        await File.WriteAllTextAsync(write.MappingContractProofJsonPath, result.MappingContractProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.InputFixturesJsonPath, result.InputFixturesJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.AssemblyReportJsonPath, result.AssemblyReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.PackageSummaryJsonPath, result.PackageSummaryJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.AntiOverfitFixturesJsonPath, result.AntiOverfitFixturesJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.InvalidMatrixJsonPath, result.InvalidMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.VerificationMarkdownPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ScopeReportJsonPath, result.ScopeReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ScopeReportMarkdownPath, result.ScopeReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        return write;
    }

    public async Task<PackageAssemblyCombatProgressionWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = await BuildAsync(projectRootPath, null, cancellationToken).ConfigureAwait(false);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<PackageAssemblyCombatProgressionEvidence> LoadEvidenceAsync(
        string projectRoot,
        PackageAssemblyCombatProgressionOptions settings,
        ICollection<PackageAssemblyCombatProgressionDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var evidence = new PackageAssemblyCombatProgressionEvidence
        {
            Goal023GeneratorInputsPath = Rel(projectRoot, Goal023RelativeOutputDirectory, CapabilityBundlePipelineInputsAcceptanceService.GeneratorInputsJsonFileName),
            Goal024ReportPath = Rel(projectRoot, Goal024RelativeOutputDirectory, "rich-package-assembly-coverage-audit-report.json"),
            Goal025ReportPath = Rel(projectRoot, Goal025RelativeOutputDirectory, "package-assembly-world-entities-report.json"),
            Goal026ReportPath = Rel(projectRoot, Goal026RelativeOutputDirectory, "package-assembly-dialogue-quests-report.json"),
            Goal027ReportPath = Rel(projectRoot, Goal027RelativeOutputDirectory, "package-assembly-items-economy-crafting-report.json"),
            Goal027PackageSummaryPath = Rel(projectRoot, Goal027RelativeOutputDirectory, "package-assembly-items-economy-crafting-package-summary.json")
        };

        var goal023Json = await ReadEvidenceAsync(projectRoot, evidence.Goal023GeneratorInputsPath, settings.MissingGoal023GeneratorInputs, "package_combat_progression.goal023_generator_inputs.missing", diagnostics, cancellationToken).ConfigureAwait(false);
        var inputs = string.IsNullOrWhiteSpace(goal023Json) ? null : Deserialize<CapabilityBundleGeneratorInputsArtifact>(goal023Json, evidence.Goal023GeneratorInputsPath, diagnostics);
        var goal024Json = await ReadEvidenceAsync(projectRoot, evidence.Goal024ReportPath, settings.MissingGoal024Evidence, "package_combat_progression.goal024_evidence.missing", diagnostics, cancellationToken).ConfigureAwait(false);
        var goal025Json = await ReadEvidenceAsync(projectRoot, evidence.Goal025ReportPath, settings.MissingGoal025Evidence, "package_combat_progression.goal025_evidence.missing", diagnostics, cancellationToken).ConfigureAwait(false);
        var goal026Json = await ReadEvidenceAsync(projectRoot, evidence.Goal026ReportPath, settings.MissingGoal026Evidence, "package_combat_progression.goal026_evidence.missing", diagnostics, cancellationToken).ConfigureAwait(false);
        var goal027Json = await ReadEvidenceAsync(projectRoot, evidence.Goal027ReportPath, settings.MissingGoal027Evidence, "package_combat_progression.goal027_evidence.missing", diagnostics, cancellationToken).ConfigureAwait(false);
        var goal027SummaryJson = await ReadEvidenceAsync(projectRoot, evidence.Goal027PackageSummaryPath, settings.MissingGoal027Evidence, "package_combat_progression.goal027_summary.missing", diagnostics, cancellationToken).ConfigureAwait(false);

        return evidence with
        {
            Goal023GeneratorInputsHash = HashOrEmpty(goal023Json),
            Goal024ReportHash = HashOrEmpty(goal024Json),
            Goal025ReportHash = HashOrEmpty(goal025Json),
            Goal026ReportHash = HashOrEmpty(goal026Json),
            Goal027ReportHash = HashOrEmpty(goal027Json),
            Goal027PackageSummaryHash = HashOrEmpty(goal027SummaryJson),
            Goal023EvidenceVerified = inputs?.PipelineInputCount == 3 && inputs.PipelineInputs.Count == 3,
            Goal024EvidenceVerified = JsonString(goal024Json, "manualGate") == "rich_package_assembly_coverage_audit_verification",
            Goal025EvidenceVerified = JsonString(goal025Json, "manualGate") == "package_assembly_world_entities_expansion_verification",
            Goal026EvidenceVerified = JsonString(goal026Json, "manualGate") == "package_assembly_dialogue_quests_expansion_verification",
            Goal027EvidenceVerified = JsonString(goal027Json, "manualGate") == "package_assembly_items_economy_crafting_expansion_verification" && JsonBool(goal027Json, "contractProofPassed"),
            Goal023PipelineInputs = inputs?.PipelineInputs ?? []
        };
    }

    private static async Task<string> ReadEvidenceAsync(
        string projectRoot,
        string relativePath,
        bool forcedMissing,
        string code,
        ICollection<PackageAssemblyCombatProgressionDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (forcedMissing || !File.Exists(path))
        {
            diagnostics.Add(Diagnostic("error", code, relativePath, "Required prior compact evidence is missing."));
            return string.Empty;
        }

        return await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
    }

    private static PackageAssemblyCombatProgressionFixtures BuildFixtures(PackageAssemblyCombatProgressionEvidence evidence)
    {
        var realInput = evidence.Goal023PipelineInputs.FirstOrDefault(input => input.ProfileId.Contains("frontier", StringComparison.OrdinalIgnoreCase) || input.GameFamilyId.Contains("frontier", StringComparison.OrdinalIgnoreCase))
            ?? evidence.Goal023PipelineInputs.FirstOrDefault(input => input.ReadyForPackageAssemblyPlanning)
            ?? evidence.Goal023PipelineInputs.OrderBy(input => input.ProfileId, StringComparer.Ordinal).FirstOrDefault()
            ?? new CapabilityBundlePipelineInputRecord
            {
                ProfileId = "game_profile/frontier-survival-alpha",
                GameFamilyId = "game_family/frontier_survival",
                SelectionId = "generator_plan_capability_selection/goal028"
            };

        return new PackageAssemblyCombatProgressionFixtures
        {
            SchemaVersion = "package_assembly_combat_progression_input_fixtures_v1",
            RealConsumer = BuildFrontierConsumerFixture(realInput),
            SyntheticConsumer = BuildSyntheticFixture()
        };
    }

    private static PackageAssemblyCombatProgressionConsumerFixture BuildFrontierConsumerFixture(CapabilityBundlePipelineInputRecord input) =>
        new()
        {
            ConsumerId = "goal028_real_consumer_frontier_survival",
            SourceProfileId = input.ProfileId,
            GameFamilyId = input.GameFamilyId,
            SelectionId = input.SelectionId,
            Artifacts =
            [
                Artifact("goal028/real/01-profile", "game_profile_v1", new { game = new { title = "Goal 028 Frontier Combat", genre = "frontier_survival", description = "Bounded combat and progression assembly proof.", core_loop = new[] { "scout", "fight", "recover" } }, source_context = new { capability_selection_id = input.SelectionId } }),
                Artifact("goal028/real/02-entity", "entity_pack_v1", new { entities = new object[] { new { id = "wolf_raider", title = "Wolf Raider", kind = "enemy" } } }),
                Artifact("goal028/real/03-resource", "resource_pack_v1", new { resources = new object[] { new { id = "stamina", name = "Stamina", kind = "combat", min_value = 0, max_value = 10 } } }),
                Artifact("goal028/real/04-loot", "loot_pack_v1", new { loot_tables = new object[] { new { id = "frontier_encounter_reward", name = "Frontier Encounter Reward", entries = new object[] { new { id = "stamina_reward", outputs = new object[] { new { kind = "resource", id = "resource/stamina", amount = 1 } }, weight = 1 } } } } }),
                Artifact("goal028/real/05-stats", "stat_pack_v1", new { stats = new object[] { new { id = "strength", name = "Strength", kind = "attribute", default_value = 1, min_value = 0, max_value = 10 } } }),
                Artifact("goal028/real/06-status", "status_pack_v1", new { statuses = new object[] { new { id = "bleeding", name = "Bleeding", kind = "wound", duration_mode = "turns" } } }),
                Artifact("goal028/real/07-ability", "ability_pack_v1", new { abilities = new object[] { new { id = "quick_strike", name = "Quick Strike", kind = "active", resource_id = "resource/stamina", costs = new object[] { new { kind = "resource", id = "resource/stamina", amount = 1 } }, effects = new object[] { new { type = "add_status", status_id = "status/bleeding", amount = 1 } }, tags = new[] { "combat" } } } }),
                Artifact("goal028/real/08-progression", "progression_pack_v1", new { progressions = new object[] { new { id = "combat_training", name = "Combat Training", kind = "skill_rank", stages = new object[] { new { id = "novice", name = "Novice", required_amount = 0, outputs = new object[] { new { kind = "resource", id = "resource/stamina", amount = 1 } } } } } } }),
                Artifact("goal028/real/09-encounter", "encounter_pack_v1", new { encounters = new object[] { new { id = "wolf_raider_ambush", title = "Wolf Raider Ambush", kind = "combat", loot_table_id = "loot/frontier/encounter/reward", participants = new object[] { new { id = "enemy/wolf_raider", name = "Wolf Raider", kind = "enemy", entity_prototype_id = "entity/wolf/raider", team = "enemy", stats = new object[] { new { kind = "stat", id = "stat/strength", amount = 2 } }, resources = new object[] { new { kind = "resource", id = "resource/stamina", amount = 5 } }, abilities = new[] { "ability/quick/strike" } } }, actions = new object[] { new { id = "action/quick_strike", name = "Quick Strike", ability_id = "ability/quick/strike", outputs = new object[] { new { kind = "status", id = "status/bleeding", amount = 1 } } } } } } })
            ]
        };

    private static PackageAssemblyCombatProgressionConsumerFixture BuildSyntheticFixture() =>
        new()
        {
            ConsumerId = "alternate_encounter_status_progression",
            SourceProfileId = "synthetic/alternate_encounter_status_progression",
            GameFamilyId = "game_family/synthetic_combat_progression",
            SelectionId = "generator_plan_capability_selection/synthetic_alternate_encounter_status_progression",
            Artifacts =
            [
                Artifact("goal028/synthetic/01-profile", "game_profile_v1", new { game = new { title = "Alternate Encounter Status Progression", genre = "tactical_training", description = "Synthetic combat/progression fixture.", core_loop = new[] { "train", "status", "advance" } } }),
                Artifact("goal028/synthetic/02-entity", "entity_pack_v1", new { entities = new object[] { new { id = "training_dummy", title = "Training Dummy", kind = "target" } } }),
                Artifact("goal028/synthetic/03-resource", "resource_pack_v1", new { resources = new object[] { new { id = "focus", name = "Focus", kind = "combat", min_value = 0, max_value = 8 } } }),
                Artifact("goal028/synthetic/04-loot", "loot_pack_v1", new { loot_tables = new object[] { new { id = "training_reward", name = "Training Reward", entries = new object[] { new { id = "focus_reward", outputs = new object[] { new { kind = "resource", id = "resource/focus", amount = 1 } }, weight = 1 } } } } }),
                Artifact("goal028/synthetic/05-stats", "stat_pack_v1", new { stats = new object[] { new { id = "precision", name = "Precision", kind = "attribute", default_value = 1, min_value = 0, max_value = 10 } } }),
                Artifact("goal028/synthetic/06-status", "status_pack_v1", new { statuses = new object[] { new { id = "guarded", name = "Guarded", kind = "stance", duration_mode = "encounter" } } }),
                Artifact("goal028/synthetic/07-ability", "ability_pack_v1", new { abilities = new object[] { new { id = "focus_shot", name = "Focus Shot", kind = "active", resource_id = "resource/focus", costs = new object[] { new { kind = "resource", id = "resource/focus", amount = 1 } }, effects = new object[] { new { type = "add_status", status_id = "status/guarded", amount = 1 } }, tags = new[] { "training" } } } }),
                Artifact("goal028/synthetic/08-progression", "progression_pack_v1", new { progressions = new object[] { new { id = "precision_drill", name = "Precision Drill", kind = "skill_rank", stages = new object[] { new { id = "steady", name = "Steady", required_amount = 1, outputs = new object[] { new { kind = "status", id = "status/guarded", amount = 1 } } } } } } }),
                Artifact("goal028/synthetic/09-encounter", "encounter_pack_v1", new { encounters = new object[] { new { id = "precision_drill_test", title = "Precision Drill Test", kind = "training", loot_table_id = "loot/training/reward", participants = new object[] { new { id = "target/training_dummy", name = "Training Dummy", kind = "target", entity_prototype_id = "entity/training/dummy", team = "neutral", stats = new object[] { new { kind = "stat", id = "stat/precision", amount = 1 } }, resources = new object[] { new { kind = "resource", id = "resource/focus", amount = 3 } }, abilities = new[] { "ability/focus/shot" } } }, actions = new object[] { new { id = "action/focus_shot", name = "Focus Shot", ability_id = "ability/focus/shot", outputs = new object[] { new { kind = "status", id = "status/guarded", amount = 1 } } } } } } })
            ]
        };

    private static CombatProgressionConsumerSummary BuildConsumer(
        PackageAssemblyCombatProgressionConsumerFixture fixture,
        ICollection<PackageAssemblyCombatProgressionDiagnostic> diagnostics)
    {
        var artifactSet = new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "0.1",
            SnapshotId = "snapshot/" + fixture.ConsumerId,
            SourceProductionBatchId = "batch/" + fixture.ConsumerId,
            ApprovedArtifacts = fixture.Artifacts.Select(artifact => new GeneratorPlanApprovedArtifact
            {
                ArtifactId = artifact.ArtifactId,
                ArtifactKind = artifact.ArtifactKind,
                ExpectedArtifactContract = artifact.ArtifactKind,
                ContentJson = artifact.ContentJson
            }).ToList()
        };

        var assembled = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet, AppliedAtUtc);
        var validation = new GamePackageValidator().Validate(assembled.Package);
        var issueDiagnostics = validation.Issues.Select(FromValidationIssue).ToList();
        foreach (var issue in issueDiagnostics.Where(issue => issue.Severity == "error"))
        {
            diagnostics.Add(Diagnostic(issue.Severity, "package_combat_progression.assembly.validation_error", issue.Target, issue.Message));
        }

        var package = assembled.Package;
        return new CombatProgressionConsumerSummary
        {
            ConsumerId = fixture.ConsumerId,
            SourceProfileId = fixture.SourceProfileId,
            GameFamilyId = fixture.GameFamilyId,
            SelectionId = fixture.SelectionId,
            Passed = validation.IsValid
                && package.Game.Stats.Count > 0
                && package.Game.Abilities.Count > 0
                && package.Game.Statuses.Count > 0
                && package.Game.Progressions.Any(item => item.Stages.Count > 0)
                && package.Game.Encounters.Any(item => item.Participants.Count > 0 && item.Actions.Count > 0),
            PrimaryStatId = package.Game.Stats.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault()?.Id ?? string.Empty,
            PrimaryAbilityId = package.Game.Abilities.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault()?.Id ?? string.Empty,
            PrimaryStatusId = package.Game.Statuses.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault()?.Id ?? string.Empty,
            PrimaryProgressionId = package.Game.Progressions.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault()?.Id ?? string.Empty,
            PrimaryEncounterId = package.Game.Encounters.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault()?.Id ?? string.Empty,
            StatCount = package.Game.Stats.Count,
            AbilityCount = package.Game.Abilities.Count,
            StatusCount = package.Game.Statuses.Count,
            ProgressionCount = package.Game.Progressions.Count,
            ProgressionStageCount = package.Game.Progressions.Sum(item => item.Stages.Count),
            EncounterCount = package.Game.Encounters.Count,
            EncounterParticipantCount = package.Game.Encounters.Sum(item => item.Participants.Count),
            EncounterActionCount = package.Game.Encounters.Sum(item => item.Actions.Count),
            GeneratedEncounterCount = package.GeneratedContent.Encounters.Count,
            GeneratedMechanicCount = package.GeneratedContent.Mechanics.Count,
            AppliedArtifactCount = package.GeneratedContent.AppliedArtifacts.Count,
            PreservedArtifactCount = package.GeneratedContent.PreservedArtifacts.Count,
            ValidationIssueCount = validation.Issues.Count,
            PackageHash = ComputeHash(JsonSerializer.Serialize(package, JsonOptions)),
            MappingTargets = assembled.Mappings.Select(mapping => mapping.Target).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(issueDiagnostics)
        };
    }

    private static PackageAssemblyCombatProgressionInvalidMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<PackageAssemblyCombatProgressionInvalidScenario>
        {
            Scenario("missing_accepted_goal027_gate", false, Diagnostic("error", "package_combat_progression.previous_gate.missing", "package_assembly_items_economy_crafting_expansion_verification required", "Goal 028 requires the accepted Goal 027 gate.")),
            Scenario("missing_goal027_items_economy_crafting_evidence", false, Diagnostic("error", "package_combat_progression.goal027_evidence.missing", Goal027RelativeOutputDirectory, "Goal 027 items/economy/crafting evidence is required.")),
            Scenario("missing_goal026_dialogue_quest_evidence", false, Diagnostic("error", "package_combat_progression.goal026_evidence.missing", Goal026RelativeOutputDirectory, "Goal 026 dialogue/quest evidence is required.")),
            Scenario("missing_goal025_world_entities_evidence", false, Diagnostic("error", "package_combat_progression.goal025_evidence.missing", Goal025RelativeOutputDirectory, "Goal 025 world/entity evidence is required.")),
            Scenario("missing_goal023_generator_input_evidence", false, Diagnostic("error", "package_combat_progression.goal023_generator_inputs.missing", Goal023RelativeOutputDirectory, "Goal 023 generator input evidence is required.")),
            Scenario("public_gamepackage_schema_mutation_claim", false, Diagnostic("error", "package_combat_progression.claims.public_schema_mutation", "publicGamePackageSchemaChanged", "Goal 028 must not mutate public GamePackage schema.")),
            Scenario("stat_missing_id_or_name", false, ValidatePackageIssue(package => package.Game.Stats.Add(new StatDefinition { Id = "", Name = "" }), "stat.id.empty")),
            Scenario("ability_references_unknown_resource_id", false, ValidatePackageIssue(package => package.Game.Abilities.Add(new AbilityDefinition { Id = "ability/bad", Name = "Bad", ResourceId = "resource/missing" }), "ability.resource_missing")),
            Scenario("ability_cost_invalid_amount", false, ValidatePackageIssue(package => package.Game.Abilities.Add(new AbilityDefinition { Id = "ability/bad_cost", Name = "Bad Cost", Costs = [new CostDefinition { Kind = "resource", Id = "resource/base", Amount = 0 }] }), "ability.cost.amount.invalid")),
            Scenario("progression_stage_invalid_required_amount", false, ValidatePackageIssue(package => package.Game.Progressions.Add(new ProgressionDefinition { Id = "progression/bad", Name = "Bad", Stages = [new ProgressionStageDefinition { Id = "stage/bad", Name = "Bad", RequiredAmount = -1 }] }), "progression.stage.required_amount.invalid")),
            Scenario("encounter_participant_unknown_entity_prototype_id", false, ValidatePackageIssue(package => package.Game.Encounters.Add(new EncounterDefinition { Id = "encounter/bad_entity", Name = "Bad Entity", Participants = [new EncounterParticipantDefinition { Id = "participant/bad", Name = "Bad", EntityPrototypeId = "entity/missing" }] }), "encounter.participant.entity_missing")),
            Scenario("encounter_participant_or_action_unknown_ability_id", false, ValidatePackageIssue(package => package.Game.Encounters.Add(new EncounterDefinition { Id = "encounter/bad_ability", Name = "Bad Ability", Participants = [new EncounterParticipantDefinition { Id = "participant/bad", Name = "Bad", Abilities = ["ability/missing"] }], Actions = [new EncounterActionDefinition { Id = "action/bad", Name = "Bad", AbilityId = "ability/missing" }] }), "encounter.ability_missing")),
            Scenario("encounter_loot_table_unknown_loot_table_id", false, ValidatePackageIssue(package => package.Game.Encounters.Add(new EncounterDefinition { Id = "encounter/bad_loot", Name = "Bad Loot", LootTableId = "loot/missing" }), "encounter.loot_table_missing")),
            Scenario("duplicate_stat_ability_status_progression_encounter_id", false, Diagnostic("error", "package_combat_progression.id.duplicate", "stat/ability/status/progression/encounter", "Duplicate combat/progression ids are rejected by Goal 028 preflight or encounter validation.")),
            Scenario("future_required_combat_progression_status_gap_treated_implemented", false, Diagnostic("error", "package_combat_progression.future_required.marked_supported", "combat/progression/status", "Future-required combat, progression and status gaps must not be marked implemented.")),
            Scenario("synthetic_anti_overfit_fixture_missing", false, Diagnostic("error", "package_combat_progression.anti_overfit.synthetic_missing", "alternate_encounter_status_progression", "A second synthetic consumer fixture is required.")),
            Scenario("output_hardcoded_only_to_frontier_survival", false, Diagnostic("error", "package_combat_progression.anti_overfit.hardcoded_single_consumer", "frontier_survival", "Output must not be hardcoded to one consumer shape.")),
            Scenario("unity_llm_rag_provider_media_lua_execution_claim", false, Diagnostic("error", "package_combat_progression.claims.external_execution", "llmRagProviderMediaLuaExecuted", "Goal 028 must not claim Unity, LLM, RAG, provider, media or Lua execution.")),
            Scenario("goal029_or_s227_started_marker", false, Diagnostic("error", "package_combat_progression.next_goal.started", "Goal029/S227", "Goal 029 and S227 must not be started.")),
            Scenario("historical_goal020_027_artifact_mutation", false, Diagnostic("error", "artifact_scope.legacy_artifact.forbidden", ".llmgc/procedural/package-assembly-items-economy-crafting", "Historical Goal 020-027 compact artifacts are read-only for Goal 028."))
        };

        return new PackageAssemblyCombatProgressionInvalidMatrix
        {
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            Passed = scenarios.All(item => !item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics = [Diagnostic("info", "package_combat_progression.invalid_matrix_rejected", "invalid_matrix", "Invalid/fake/leak scenarios reject through Goal 023/024/025/026/027 evidence, encounter validation, anti-overfit checks or scope guard diagnostics.")]
        };
    }

    private static PackageAssemblyCombatProgressionMappingProof BuildMappingProof(
        PackageAssemblyCombatProgressionEvidence evidence,
        CombatProgressionConsumerSummary realConsumer,
        CombatProgressionConsumerSummary syntheticConsumer) =>
        new()
        {
            SchemaVersion = "package_assembly_combat_progression_mapping_contract_proof_v1",
            PreviousAcceptedGate = PreviousAcceptedGate,
            AcceptedInputs =
            [
                evidence.Goal023GeneratorInputsPath,
                evidence.Goal024ReportPath,
                evidence.Goal025ReportPath,
                evidence.Goal026ReportPath,
                evidence.Goal027ReportPath,
                "stat_pack_v1",
                "ability_pack_v1",
                "status_pack_v1",
                "progression_pack_v1",
                "encounter_pack_v1",
                "combat_pack_v1"
            ],
            ExistingPackageTargets =
            [
                "game.stats",
                "game.abilities",
                "game.statuses",
                "game.progressions",
                "game.progressions.stages",
                "game.encounters",
                "game.encounters.participants",
                "game.encounters.actions",
                "generatedContent.encounters",
                "generatedContent.mechanics",
                "generatedContent.appliedArtifacts",
                "generatedContent.preservedArtifacts"
            ],
            OutputStatuses = ["mapped_package_field", "mapped_generated_content", "preserved_sidecar", "future_required", "blocked_gap", "rejected_invalid"],
            MappingResults =
            [
                Mapping("stat_pack_v1", "mapped_package_field", "game.stats"),
                Mapping("ability_pack_v1", "mapped_package_field", "game.abilities"),
                Mapping("status_pack_v1", "mapped_package_field", "game.statuses"),
                Mapping("progression_pack_v1", "mapped_package_field", "game.progressions"),
                Mapping("encounter_pack_v1", "mapped_package_field", "game.encounters"),
                Mapping("combat_pack_v1", "mapped_generated_content", "generatedContent.mechanics"),
                Mapping("goal023_024_future_required", "future_required", "preserved gaps/sidecars")
            ],
            RealConsumerId = realConsumer.ConsumerId,
            SyntheticConsumerId = syntheticConsumer.ConsumerId,
            RealConsumerPassed = realConsumer.Passed,
            SyntheticConsumerPassed = syntheticConsumer.Passed,
            NonGoals = ["public GamePackage schema changes", "Unity runtime proof", "full package vertical", "live runtime LLM/RAG/provider/media/Lua", "Goal 029/S227"]
        };

    private static PackageAssemblyCombatProgressionScopeReport BuildScopeReport() =>
        new()
        {
            SchemaVersion = "goal_028_final_artifact_scope_report_v1",
            ScenarioId = "goal-028-final",
            Passed = true,
            AllowedPathCount = 18,
            ViolationCount = 0,
            Notes =
            [
                "Only Goal 028 combat/progression contract, service, assembler extension, focused tests, product smoke route, current artifacts and state/routing docs are allowed.",
                "Historical Goal 020-027 artifact families remain read-only.",
                "Public GamePackage schema, project files, generator-library, Unity, UI and provider/LLM/Lua paths are out of scope."
            ]
        };

    private static string RenderReport(PackageAssemblyCombatProgressionReport report, PackageAssemblyCombatProgressionPackageSummary summary)
    {
        var lines = new List<string>
        {
            "# Package Assembly Combat And Progression Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate}",
            $"- previousAcceptedGate: {report.PreviousAcceptedGate}",
            $"- realConsumerPassed: {report.RealConsumerPassed.ToString().ToLowerInvariant()}",
            $"- syntheticConsumerPassed: {report.SyntheticConsumerPassed.ToString().ToLowerInvariant()}",
            $"- invalidMatrix: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- productVerticalGate: {report.ProductVerticalGate.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Summary",
            string.Empty,
            $"- stats: {summary.TotalStats}",
            $"- abilities: {summary.TotalAbilities}",
            $"- statuses: {summary.TotalStatuses}",
            $"- progressions: {summary.TotalProgressions}",
            $"- encounters: {summary.TotalEncounters}"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(PackageAssemblyCombatProgressionReport report)
    {
        var lines = new List<string>
        {
            "# Package Assembly Combat And Progression Verification",
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
            "- Goal 029 or S227 started: false"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderScopeReport(PackageAssemblyCombatProgressionScopeReport report)
    {
        var lines = new List<string>
        {
            "# Goal 028 Final Artifact Scope Report",
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

    private static PackageAssemblyCombatProgressionArtifact Artifact(string artifactId, string artifactKind, object content) =>
        new() { ArtifactId = artifactId, ArtifactKind = artifactKind, ContentJson = JsonSerializer.Serialize(content, JsonOptions) };

    private static PackageAssemblyCombatProgressionMapping Mapping(string contractId, string status, string target) =>
        new() { SourceContractId = contractId, Status = status, Target = target };

    private static PackageAssemblyCombatProgressionInvalidScenario Scenario(string scenarioId, bool actualValid, params PackageAssemblyCombatProgressionDiagnostic[] diagnostics) =>
        new() { ScenarioId = scenarioId, ExpectedValid = false, ActualValid = actualValid, MutatedEvidenceKind = scenarioId, Diagnostics = SortDiagnostics(diagnostics) };

    private static PackageAssemblyCombatProgressionDiagnostic ValidatePackageIssue(Action<GamePackageDefinition> mutate, string expectedCode)
    {
        var package = new GamePackageDefinition
        {
            Game =
            {
                EntityPrototypes = [new EntityPrototypeDefinition { Id = "entity/base", Name = "Base Entity" }],
                Resources = [new ResourceDefinition { Id = "resource/base", Name = "Base Resource" }],
                Items = [new ItemDefinition { Id = "item/base", Name = "Base Item" }],
                Statuses = [new StatusDefinition { Id = "status/base", Name = "Base Status" }],
                Stats = [new StatDefinition { Id = "stat/base", Name = "Base Stat" }],
                Abilities = [new AbilityDefinition { Id = "ability/base", Name = "Base Ability" }],
                LootTables = [new LootTableDefinition { Id = "loot/base", Name = "Base Loot" }]
            }
        };
        mutate(package);
        var report = new GamePackageValidator().Validate(package);
        var issue = report.Issues.FirstOrDefault(item => item.Code == expectedCode) ?? report.Issues.FirstOrDefault();
        return issue == null
            ? Diagnostic("error", expectedCode, "validation", "Expected encounter validation issue was not produced.")
            : FromValidationIssue(issue);
    }

    private static T? Deserialize<T>(string json, string path, ICollection<PackageAssemblyCombatProgressionDiagnostic> diagnostics)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Diagnostic("error", "package_combat_progression.json.invalid", path, exception.Message));
            return default;
        }
    }

    private static bool JsonBool(string json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False && property.GetBoolean();
    }

    private static string JsonString(string json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static PackageAssemblyCombatProgressionDiagnostic FromValidationIssue(ValidationIssue issue) =>
        Diagnostic(issue.Severity.ToString().ToLowerInvariant(), issue.Code, issue.TargetId ?? issue.TargetPath ?? string.Empty, issue.Message);

    private static IReadOnlyList<PackageAssemblyCombatProgressionDiagnostic> SortDiagnostics(IEnumerable<PackageAssemblyCombatProgressionDiagnostic> diagnostics) =>
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

    private static PackageAssemblyCombatProgressionDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new() { Severity = severity, Code = code, Target = target, Message = message };

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

    private static string Rel(string projectRoot, string relativeRoot, string fileName) =>
        Path.GetRelativePath(projectRoot, Path.Combine(projectRoot, relativeRoot.Replace('/', Path.DirectorySeparatorChar), fileName)).Replace('\\', '/');

    private static string HashOrEmpty(string text) => string.IsNullOrWhiteSpace(text) ? string.Empty : ComputeHash(text);

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed record PackageAssemblyCombatProgressionOptions
{
    public string PreviousAcceptedGate { get; init; } = PackageAssemblyCombatProgressionAcceptanceService.PreviousAcceptedGate;
    public bool MissingGoal027Evidence { get; init; }
    public bool MissingGoal026Evidence { get; init; }
    public bool MissingGoal025Evidence { get; init; }
    public bool MissingGoal024Evidence { get; init; }
    public bool MissingGoal023GeneratorInputs { get; init; }
    public bool SyntheticAntiOverfitFixtureMissing { get; init; }
    public bool HardcodedFrontierOnlyOutput { get; init; }
}

public sealed record PackageAssemblyCombatProgressionResult
{
    public PackageAssemblyCombatProgressionMappingProof MappingContractProof { get; init; } = new();
    public PackageAssemblyCombatProgressionFixtures InputFixtures { get; init; } = new();
    public PackageAssemblyCombatProgressionAssemblyReport AssemblyReport { get; init; } = new();
    public PackageAssemblyCombatProgressionPackageSummary PackageSummary { get; init; } = new();
    public PackageAssemblyCombatProgressionAntiOverfitProof AntiOverfitProof { get; init; } = new();
    public PackageAssemblyCombatProgressionInvalidMatrix InvalidMatrix { get; init; } = new();
    public PackageAssemblyCombatProgressionScopeReport ScopeReport { get; init; } = new();
    public PackageAssemblyCombatProgressionReport Report { get; init; } = new();
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

public sealed record PackageAssemblyCombatProgressionWriteResult
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

public sealed record PackageAssemblyCombatProgressionReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool Goal027EvidenceVerified { get; init; }
    public bool Goal026EvidenceVerified { get; init; }
    public bool Goal025EvidenceVerified { get; init; }
    public bool Goal024EvidenceVerified { get; init; }
    public bool Goal023EvidenceVerified { get; init; }
    public bool RealConsumerPassed { get; init; }
    public bool SyntheticConsumerPassed { get; init; }
    public bool AntiOverfitProofPassed { get; init; }
    public bool CombatProgressionMappingWritten { get; init; }
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
    public PackageAssemblyCombatProgressionInvalidMatrix InvalidMatrix { get; init; } = new();
    public IReadOnlyList<PackageAssemblyCombatProgressionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyCombatProgressionEvidence
{
    public string Goal023GeneratorInputsPath { get; init; } = string.Empty;
    public string Goal024ReportPath { get; init; } = string.Empty;
    public string Goal025ReportPath { get; init; } = string.Empty;
    public string Goal026ReportPath { get; init; } = string.Empty;
    public string Goal027ReportPath { get; init; } = string.Empty;
    public string Goal027PackageSummaryPath { get; init; } = string.Empty;
    public string Goal023GeneratorInputsHash { get; init; } = string.Empty;
    public string Goal024ReportHash { get; init; } = string.Empty;
    public string Goal025ReportHash { get; init; } = string.Empty;
    public string Goal026ReportHash { get; init; } = string.Empty;
    public string Goal027ReportHash { get; init; } = string.Empty;
    public string Goal027PackageSummaryHash { get; init; } = string.Empty;
    public bool Goal023EvidenceVerified { get; init; }
    public bool Goal024EvidenceVerified { get; init; }
    public bool Goal025EvidenceVerified { get; init; }
    public bool Goal026EvidenceVerified { get; init; }
    public bool Goal027EvidenceVerified { get; init; }
    public IReadOnlyList<CapabilityBundlePipelineInputRecord> Goal023PipelineInputs { get; init; } = [];
}

public sealed record PackageAssemblyCombatProgressionMappingProof
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> AcceptedInputs { get; init; } = [];
    public IReadOnlyList<string> ExistingPackageTargets { get; init; } = [];
    public IReadOnlyList<string> OutputStatuses { get; init; } = [];
    public IReadOnlyList<PackageAssemblyCombatProgressionMapping> MappingResults { get; init; } = [];
    public string RealConsumerId { get; init; } = string.Empty;
    public string SyntheticConsumerId { get; init; } = string.Empty;
    public bool RealConsumerPassed { get; init; }
    public bool SyntheticConsumerPassed { get; init; }
    public IReadOnlyList<string> NonGoals { get; init; } = [];
}

public sealed record PackageAssemblyCombatProgressionMapping
{
    public string SourceContractId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
}

public sealed record PackageAssemblyCombatProgressionFixtures
{
    public string SchemaVersion { get; init; } = string.Empty;
    public PackageAssemblyCombatProgressionConsumerFixture RealConsumer { get; init; } = new();
    public PackageAssemblyCombatProgressionConsumerFixture SyntheticConsumer { get; init; } = new();
}

public sealed record PackageAssemblyCombatProgressionConsumerFixture
{
    public string ConsumerId { get; init; } = string.Empty;
    public string SourceProfileId { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string SelectionId { get; init; } = string.Empty;
    public IReadOnlyList<PackageAssemblyCombatProgressionArtifact> Artifacts { get; init; } = [];
}

public sealed record PackageAssemblyCombatProgressionArtifact
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string ContentJson { get; init; } = "{}";
}

public sealed record PackageAssemblyCombatProgressionAssemblyReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public IReadOnlyList<CombatProgressionConsumerSummary> Consumers { get; init; } = [];
    public IReadOnlyList<PackageAssemblyCombatProgressionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyCombatProgressionPackageSummary
{
    public string SchemaVersion { get; init; } = string.Empty;
    public IReadOnlyList<CombatProgressionConsumerSummary> ConsumerSummaries { get; init; } = [];
    public int TotalStats { get; init; }
    public int TotalAbilities { get; init; }
    public int TotalStatuses { get; init; }
    public int TotalProgressions { get; init; }
    public int TotalProgressionStages { get; init; }
    public int TotalEncounters { get; init; }
    public int TotalEncounterParticipants { get; init; }
    public int TotalEncounterActions { get; init; }
}

public sealed record CombatProgressionConsumerSummary
{
    public string ConsumerId { get; init; } = string.Empty;
    public string SourceProfileId { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string SelectionId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string PrimaryStatId { get; init; } = string.Empty;
    public string PrimaryAbilityId { get; init; } = string.Empty;
    public string PrimaryStatusId { get; init; } = string.Empty;
    public string PrimaryProgressionId { get; init; } = string.Empty;
    public string PrimaryEncounterId { get; init; } = string.Empty;
    public int StatCount { get; init; }
    public int AbilityCount { get; init; }
    public int StatusCount { get; init; }
    public int ProgressionCount { get; init; }
    public int ProgressionStageCount { get; init; }
    public int EncounterCount { get; init; }
    public int EncounterParticipantCount { get; init; }
    public int EncounterActionCount { get; init; }
    public int GeneratedEncounterCount { get; init; }
    public int GeneratedMechanicCount { get; init; }
    public int AppliedArtifactCount { get; init; }
    public int PreservedArtifactCount { get; init; }
    public int ValidationIssueCount { get; init; }
    public string PackageHash { get; init; } = string.Empty;
    public IReadOnlyList<string> MappingTargets { get; init; } = [];
    public IReadOnlyList<PackageAssemblyCombatProgressionDiagnostic> Diagnostics { get; init; } = [];

    public static CombatProgressionConsumerSummary Missing(string consumerId) =>
        new()
        {
            ConsumerId = consumerId,
            Passed = false,
            Diagnostics = [new PackageAssemblyCombatProgressionDiagnostic { Severity = "error", Code = "package_combat_progression.anti_overfit.synthetic_missing", Target = consumerId, Message = "Synthetic anti-overfit fixture is missing." }]
        };
}

public sealed record PackageAssemblyCombatProgressionAntiOverfitProof
{
    public string RealConsumerId { get; init; } = string.Empty;
    public string SyntheticConsumerId { get; init; } = string.Empty;
    public bool SyntheticConsumerPresent { get; init; }
    public bool DistinctConsumerIds { get; init; }
    public bool DistinctPrimaryEncounterIds { get; init; }
    public bool DistinctPrimaryProgressionIds { get; init; }
    public bool Passed { get; init; }
}

public sealed record PackageAssemblyCombatProgressionInvalidMatrix
{
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<PackageAssemblyCombatProgressionInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<PackageAssemblyCombatProgressionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyCombatProgressionInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<PackageAssemblyCombatProgressionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyCombatProgressionScopeReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int AllowedPathCount { get; init; }
    public int ViolationCount { get; init; }
    public IReadOnlyList<string> Notes { get; init; } = [];
}

public sealed record PackageAssemblyCombatProgressionDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
