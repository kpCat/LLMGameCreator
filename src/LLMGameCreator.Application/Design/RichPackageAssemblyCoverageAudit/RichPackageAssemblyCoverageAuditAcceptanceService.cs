using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.CapabilityBundlePipelineInputs;

namespace LLMGameCreator.Application.Design.RichPackageAssemblyCoverageAudit;

public sealed class RichPackageAssemblyCoverageAuditAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/rich-package-assembly-coverage-audit";
    public const string CoverageMatrixJsonFileName = "rich-package-assembly-coverage-matrix.json";
    public const string GapReportJsonFileName = "rich-package-assembly-coverage-gap-report.json";
    public const string NextSlicePlanJsonFileName = "rich-package-assembly-next-slice-plan.json";
    public const string InvalidMatrixJsonFileName = "rich-package-assembly-coverage-invalid-matrix.json";
    public const string ReportJsonFileName = "rich-package-assembly-coverage-audit-report.json";
    public const string ReportMarkdownFileName = "rich-package-assembly-coverage-audit-report.md";
    public const string VerificationMarkdownFileName = "rich-package-assembly-coverage-audit-verification.md";
    public const string FinalGate = "rich_package_assembly_coverage_audit_verification";
    public const string PreviousAcceptedGate = "capability_bundle_pipeline_inputs_verification passed";
    private const string Goal023ManualGate = CapabilityBundlePipelineInputsAcceptanceService.FinalGate;
    private const string ProductSmokeRoute = "rich-package-assembly-coverage-audit";
    private const string Goal023RelativeOutputDirectory = ".llmgc/procedural/capability-bundle-pipeline-inputs";
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly string[] RequiredDomainIds =
    [
        "world",
        "entities",
        "quests",
        "dialogue_interactions",
        "items_inventory_economy",
        "combat_progression",
        "factions_social_work_theft_schedules",
        "assets_runtime_export"
    ];

    public async Task<RichPackageAssemblyCoverageAuditResult> BuildAsync(
        string projectRootPath,
        RichPackageAssemblyCoverageAuditOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new RichPackageAssemblyCoverageAuditOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<RichPackageAssemblyCoverageDiagnostic>
        {
            Diagnostic("info", "rich_package_audit.goal023_gate_recorded", settings.PreviousAcceptedGate, "User-confirmed Goal 023 capability bundle pipeline inputs verification is recorded as passed."),
            Diagnostic("info", "rich_package_audit.audit_only_boundary", "execution_boundary", "No package assembly expansion, Unity build, LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.")
        };

        var goal023 = await LoadGoal023EvidenceAsync(projectRoot, settings, diagnostics, cancellationToken).ConfigureAwait(false);
        var matrix = BuildCoverageMatrix(goal023, settings);
        var coverageDiagnostics = ValidateCoverageMatrix(matrix, goal023, settings);
        diagnostics.AddRange(coverageDiagnostics);

        var gapReport = BuildGapReport(goal023, matrix, coverageDiagnostics);
        var nextSlicePlan = BuildNextSlicePlan(matrix);
        var invalidMatrix = BuildInvalidMatrix(matrix);
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var matrixJson = JsonSerializer.Serialize(matrix, JsonOptions);
        var gapReportJson = JsonSerializer.Serialize(gapReport, JsonOptions);
        var nextSlicePlanJson = JsonSerializer.Serialize(nextSlicePlan, JsonOptions);
        var invalidMatrixJson = JsonSerializer.Serialize(invalidMatrix, JsonOptions);
        var matrixHash = ComputeHash(matrixJson);
        var gapReportHash = ComputeHash(gapReportJson);
        var nextSlicePlanHash = ComputeHash(nextSlicePlanJson);
        var invalidMatrixHash = ComputeHash(invalidMatrixJson);

        var noTopLevelErrors = diagnostics.All(diagnostic => diagnostic.Severity != "error");
        var futureAndBlockedPreserved = matrix.Domains
            .All(domain => domain.Evidence.All(item =>
                item.EvidenceClass is not ("future_required" or "blocked_gap")
                || domain.SupportStatus != "package_supported"));
        var contractProofPassed =
            settings.PreviousAcceptedGate == PreviousAcceptedGate &&
            goal023.EvidenceVerified &&
            matrix.Domains.Select(domain => domain.DomainId).Distinct(StringComparer.Ordinal).Count() >= RequiredDomainIds.Length &&
            RequiredDomainIds.All(id => matrix.Domains.Any(domain => domain.DomainId == id)) &&
            futureAndBlockedPreserved &&
            invalidMatrix.Passed &&
            noTopLevelErrors &&
            !settings.PackageAssemblyExecutedClaim &&
            !settings.Goal025OrS199StartedMarker;

        var reportWithoutHash = new RichPackageAssemblyCoverageAuditReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            CompletedSlices = ["S192", "S193", "S194", "S195", "S196", "S197", "S198"],
            ProductSmokeRoute = ProductSmokeRoute,
            ContractProofPassed = contractProofPassed,
            Goal023EvidenceVerified = goal023.EvidenceVerified,
            Goal023ReportHash = goal023.ReportHash,
            Goal023GeneratorInputsHash = goal023.GeneratorInputsHash,
            Goal023GapReportHash = goal023.GapReportHash,
            CoverageDomainCount = matrix.Domains.Count,
            RequiredCoverageDomainsPresent = RequiredDomainIds.All(id => matrix.Domains.Any(domain => domain.DomainId == id)),
            CoverageMatrixWritten = true,
            GapReportWritten = true,
            NextSlicePlanWritten = true,
            FutureRequiredAndBlockedGapsPreserved = futureAndBlockedPreserved,
            CoverageMatrixHash = matrixHash,
            GapReportHash = gapReportHash,
            NextSlicePlanHash = nextSlicePlanHash,
            InvalidMatrixHash = invalidMatrixHash,
            InvalidMatrix = invalidMatrix,
            CoverageSummary = matrix.Domains
                .OrderBy(domain => domain.DomainId, StringComparer.Ordinal)
                .Select(domain => new RichPackageAssemblyCoverageDomainSummary
                {
                    DomainId = domain.DomainId,
                    SupportStatus = domain.SupportStatus,
                    GapCount = domain.GapIds.Count,
                    RecommendedNextAction = domain.RecommendedNextAction
                })
                .ToList(),
            TopGapIds = gapReport.Gaps
                .OrderBy(gap => GapPriority(gap.Status))
                .ThenBy(gap => gap.DomainId, StringComparer.Ordinal)
                .ThenBy(gap => gap.GapId, StringComparer.Ordinal)
                .Take(20)
                .Select(gap => gap.GapId)
                .ToList(),
            PackageAssemblyExecuted = false,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            UnityBuildExecuted = false,
            LlmRagProviderMediaLuaExecuted = false,
            ScopeGuardPassed = true,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new RichPackageAssemblyCoverageAuditResult
        {
            CoverageMatrix = matrix,
            GapReport = gapReport,
            NextSlicePlan = nextSlicePlan,
            InvalidMatrix = invalidMatrix,
            Report = report,
            CoverageMatrixJson = matrixJson,
            GapReportJson = gapReportJson,
            NextSlicePlanJson = nextSlicePlanJson,
            InvalidMatrixJson = invalidMatrixJson,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report, matrix, gapReport, nextSlicePlan),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<RichPackageAssemblyCoverageAuditWriteResult> WriteAsync(
        string projectRootPath,
        RichPackageAssemblyCoverageAuditResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var matrixPath = Path.Combine(outputDirectory, CoverageMatrixJsonFileName);
        var gapReportPath = Path.Combine(outputDirectory, GapReportJsonFileName);
        var nextSlicePlanPath = Path.Combine(outputDirectory, NextSlicePlanJsonFileName);
        var invalidMatrixPath = Path.Combine(outputDirectory, InvalidMatrixJsonFileName);
        var reportJsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var reportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationMarkdownPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);

        await File.WriteAllTextAsync(matrixPath, result.CoverageMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(gapReportPath, result.GapReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(nextSlicePlanPath, result.NextSlicePlanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(invalidMatrixPath, result.InvalidMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationMarkdownPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new RichPackageAssemblyCoverageAuditWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            CoverageMatrixJsonPath = matrixPath,
            GapReportJsonPath = gapReportPath,
            NextSlicePlanJsonPath = nextSlicePlanPath,
            InvalidMatrixJsonPath = invalidMatrixPath,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            VerificationMarkdownPath = verificationMarkdownPath
        };
    }

    public async Task<RichPackageAssemblyCoverageAuditWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = await BuildAsync(projectRootPath, null, cancellationToken).ConfigureAwait(false);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<Goal023CoverageEvidence> LoadGoal023EvidenceAsync(
        string projectRoot,
        RichPackageAssemblyCoverageAuditOptions settings,
        ICollection<RichPackageAssemblyCoverageDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var goal023Root = Path.Combine(projectRoot, Goal023RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar));
        var reportPath = Path.Combine(goal023Root, CapabilityBundlePipelineInputsAcceptanceService.ReportJsonFileName);
        var generatorInputsPath = Path.Combine(goal023Root, CapabilityBundlePipelineInputsAcceptanceService.GeneratorInputsJsonFileName);
        var gapReportPath = Path.Combine(goal023Root, CapabilityBundlePipelineInputsAcceptanceService.GapReportJsonFileName);
        var evidence = new Goal023CoverageEvidence
        {
            ReportPath = RelativePath(projectRoot, reportPath),
            GeneratorInputsPath = RelativePath(projectRoot, generatorInputsPath),
            GapReportPath = RelativePath(projectRoot, gapReportPath)
        };

        if (settings.PreviousAcceptedGate != PreviousAcceptedGate)
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.previous_gate.missing", settings.PreviousAcceptedGate, "Goal 024 requires capability_bundle_pipeline_inputs_verification passed."));
        }

        if (settings.MissingGoal023Report || !File.Exists(reportPath))
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.goal023_report.missing", evidence.ReportPath, "Accepted Goal 023 report must exist."));
            return evidence;
        }

        var reportJson = await File.ReadAllTextAsync(reportPath, cancellationToken).ConfigureAwait(false);
        evidence = evidence with { ReportHash = ComputeHash(reportJson) };
        var report = Deserialize<CapabilityBundlePipelineInputsReport>(reportJson, evidence.ReportPath, diagnostics);
        if (report != null)
        {
            evidence = evidence with { Report = report };
            Require(report.ManualGate == Goal023ManualGate, diagnostics, "rich_package_audit.goal023_manual_gate.mismatch", report.ManualGate, "Goal 023 report manual gate must match capability_bundle_pipeline_inputs_verification.");
            Require(report.ContractProofPassed, diagnostics, "rich_package_audit.goal023_contract_proof.failed", evidence.ReportPath, "Goal 023 contract proof must be true before coverage audit.");
            Require(report.PipelineInputCount == 3, diagnostics, "rich_package_audit.goal023_pipeline_count.invalid", report.PipelineInputCount.ToString(System.Globalization.CultureInfo.InvariantCulture), "Goal 023 report must record three pipeline inputs.");
            Require(report.Diagnostics.All(item => item.Severity != "error"), diagnostics, "rich_package_audit.goal023_report_errors.present", evidence.ReportPath, "Goal 023 report must not contain top-level error diagnostics.");
        }

        CapabilityBundleGeneratorInputsArtifact? generatorInputs = null;
        if (settings.CopiedCoverageReportWithoutGoal023GeneratorInputs || !File.Exists(generatorInputsPath))
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.goal023_generator_inputs.missing", evidence.GeneratorInputsPath, "Coverage audit requires the physical Goal 023 generator inputs artifact."));
            return evidence;
        }

        var generatorInputsJson = await File.ReadAllTextAsync(generatorInputsPath, cancellationToken).ConfigureAwait(false);
        evidence = evidence with { GeneratorInputsHash = ComputeHash(generatorInputsJson) };
        generatorInputs = Deserialize<CapabilityBundleGeneratorInputsArtifact>(generatorInputsJson, evidence.GeneratorInputsPath, diagnostics);
        if (generatorInputs != null)
        {
            evidence = evidence with { GeneratorInputs = generatorInputs };
            Require(generatorInputs.PipelineInputCount == 3, diagnostics, "rich_package_audit.goal023_generator_input_count.invalid", generatorInputs.PipelineInputCount.ToString(System.Globalization.CultureInfo.InvariantCulture), "Generator inputs artifact must contain three pipeline inputs.");
            Require(generatorInputs.PipelineInputs.Count == 3, diagnostics, "rich_package_audit.goal023_generator_input_records.invalid", generatorInputs.PipelineInputs.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), "Generator inputs artifact must contain exactly three pipeline input records.");
        }

        if (File.Exists(gapReportPath))
        {
            var gapReportJson = await File.ReadAllTextAsync(gapReportPath, cancellationToken).ConfigureAwait(false);
            evidence = evidence with { GapReportHash = ComputeHash(gapReportJson) };
            var gapReport = Deserialize<CapabilityBundleGapReportArtifact>(gapReportJson, evidence.GapReportPath, diagnostics);
            if (gapReport != null)
            {
                evidence = evidence with { GapReport = gapReport };
            }
        }
        else
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.goal023_gap_report.missing", evidence.GapReportPath, "Goal 023 gap report must exist so future-required and blocked gaps can be preserved."));
        }

        var verified = report != null
            && generatorInputs != null
            && evidence.GapReport != null
            && report.ManualGate == Goal023ManualGate
            && report.ContractProofPassed
            && report.PipelineInputCount == 3
            && generatorInputs.PipelineInputCount == 3
            && generatorInputs.PipelineInputs.Count == 3
            && report.Diagnostics.All(item => item.Severity != "error");
        return evidence with { EvidenceVerified = verified };
    }

    private static RichPackageAssemblyCoverageMatrix BuildCoverageMatrix(
        Goal023CoverageEvidence goal023,
        RichPackageAssemblyCoverageAuditOptions settings)
    {
        var inputs = goal023.GeneratorInputs?.PipelineInputs ?? [];
        var domains = new List<RichPackageAssemblyCoverageDomain>
        {
            Domain("world", inputs,
                ["region_graph_v1", "map_pack_v1", "path_network_v1", "world_profile_v1", "world.chunk/v1", "world.region/v1"],
                ["GamePackageDefinition.Game.Maps", "GamePackageDefinition.GeneratedContent.Regions", "MapDefinition.Entities"],
                ["GameDefinitionValidator", "GeneratedPackageMvpService.BuildMaps", "GeneratorPlanGamePackageAssembler.MapScenePack", "GeneratorPlanGamePackageAssembler.MapRegionPack"],
                ["GeneratedPackageMvpSmokeTests", "VisibleGeneratedPlayablePreviewSmokeTests", "UnityGeneratedSceneProjectionSmokeTests"],
                "package_supported_partial",
                "Existing package maps and generated regions cover finite starter maps, but region graph/chunk topology remains future-required or blocked where Goal 023 says so."),
            Domain("entities", inputs,
                ["entity_pack_v1", "npc_card_v1", "player_character_card_v1", "actor_model_profile_v1", "party_roster_v1"],
                ["GamePackageDefinition.Game.EntityPrototypes", "MapDefinition.Entities", "GamePackageDefinition.GeneratedContent.Npcs"],
                ["GameDefinitionValidator", "GeneratorPlanGamePackageAssembler.MapEntityPack", "GeneratorPlanGamePackageAssembler.MapNpcPack", "GeneratedPackageMvpService.AddMapEntities"],
                ["GeneratedPackageMvpSmokeTests", "UnityMultiVariantPlayableScenarioSmokeTests"],
                "package_supported_partial",
                "Existing package supports entity prototypes, map placements and generated NPC sidecars; party/card richness is not package-assembled yet."),
            Domain("quests", inputs,
                ["quest_pack_v1", "quest.graph/v1", "reward.rules/v1"],
                ["GamePackageDefinition.Game.Quests", "QuestDefinition.Objectives", "QuestDefinition.Stages", "GamePackageDefinition.GeneratedContent.Quests"],
                ["NarrativeDefinitionValidator", "GeneratorPlanGamePackageAssembler.MapQuestPack", "GeneratedPackageMvpService.BuildQuests"],
                ["UnityQuestCompletionLoopSmokeTests", "MinimumPlayableGeneratedGameSmokeTests"],
                "package_supported_partial",
                "Quest definitions, objectives and staged fields exist, but graph/richer reward rules from Goal 023 remain future-required."),
            Domain("dialogue_interactions", inputs,
                ["dialogue_pack_v1", "interaction_pack_v1", "dialogue.graph/v1", "interaction.conditions/v1", "text_pack_v1", "phrase_plan_v1", "morphology_pack_v1"],
                ["GamePackageDefinition.Game.Dialogues", "GamePackageDefinition.Game.Interactions", "GamePackageDefinition.GeneratedContent.Dialogues"],
                ["NarrativeDefinitionValidator", "EconomyDefinitionValidator.ValidateInteractions", "GeneratorPlanGamePackageAssembler.MapDialoguePack", "GeneratedPackageMvpService.BuildDialogues"],
                ["QuestDialogInteractionFamilySmokeTests", "SemanticRuntimeCompositionSmokeTests", "UnityRuntimeStateLoopSmokeTests"],
                "package_supported_partial",
                "Dialogue and interaction package fields exist; clue graph, morphology and advanced condition packs remain sidecar/future-required."),
            Domain("items_inventory_economy", inputs,
                ["inventory_pack_v1", "equipment_pack_v1", "vendor_pack_v1", "vendor_card_v1", "asset_request_pack_v1", "audio_request_pack_v1", "crafting.recipe_rules/v1"],
                ["GamePackageDefinition.Game.Items", "GamePackageDefinition.Game.Resources", "GamePackageDefinition.Game.Recipes", "GamePackageDefinition.Game.LootTables", "GamePackageDefinition.Game.Transactions", "GamePackageDefinition.Game.Inventories", "GamePackageDefinition.Game.EquipmentSlots"],
                ["EconomyDefinitionValidator", "GeneratedPackageMvpService.BuildItems"],
                ["RulePackGameplayFamilySmokeTests", "RulePackCombatFactionSocialWorkTheftSmokeTests", "MinimumAssetPipelineSmokeTests"],
                "package_supported_partial",
                "Economy package fields and validators exist; vendor/economy profile requests are not fully package-assembled."),
            Domain("combat_progression", inputs,
                ["combat_pack_v1", "encounter_pack_v1", "progression_pack_v1", "ability.rules/v1", "combat.mode/v1", "status_effects/v1"],
                ["GamePackageDefinition.Game.Abilities", "GamePackageDefinition.Game.Stats", "GamePackageDefinition.Game.Progressions", "GamePackageDefinition.Game.Encounters", "GamePackageDefinition.Game.Statuses"],
                ["EncounterDefinitionValidator", "EconomyDefinitionValidator", "GeneratorPlanGamePackageAssembler.MapEncounterPack", "GeneratorPlanGamePackageAssembler.MapMechanicsPack", "GeneratedPackageMvpService.BuildEncounters"],
                ["RulePackCombatFactionSocialWorkTheftSmokeTests", "ContentGenerationScaleSmokeTests"],
                "package_supported_partial",
                "Encounters, abilities, stats and progressions exist, but richer combat pack/progression inputs are mostly future-required."),
            Domain("factions_social_work_theft_schedules", inputs,
                ["reputation_pack_v1", "faction.definitions/v1", "reputation.rules/v1", "character_card_v1", "npc_card_v1", "party_roster_v1", "schedule_pack_v1"],
                ["GamePackageDefinition.Game.Factions", "FactionDefinition.Relations", "GamePackageDefinition.Game.Dialogues", "GamePackageDefinition.Game.Interactions"],
                ["NarrativeDefinitionValidator", "GeneratedPackageMvpService.BuildFactions"],
                ["RulePackCombatFactionSocialWorkTheftSmokeTests", "UnityAlphaReadablePresentationSmokeTests"],
                "package_supported_partial",
                "Faction and reputation fields exist and work/theft can be represented through interactions/runtime evidence, but schedules have no package field yet."),
            Domain("assets_runtime_export", inputs,
                ["asset_index_v1", "asset_request_pack_v1", "audio_request_pack_v1", "animation_request_pack_v1", "unity_ir_v1", "runtime_db_build_plan_v1", "runtime_export/unity_alpha_windows"],
                ["GamePackageDefinition.AssetCatalog", "AssetDefinition", "AssetContractDefinition", "GeneratedContent.AppliedArtifacts"],
                ["AssetCatalogValidator", "ScriptCatalogValidator"],
                ["MinimumAssetPipelineSmokeTests", "UnityRuntimeExportSmokeTests", "MinimumPlayableGeneratedGameSmokeTests"],
                "package_supported_partial",
                "Asset catalog and export artifacts exist, while exact Unity Alpha runtime target and media request packs remain future-required.")
        };

        if (settings.DuplicateCoverageDomainId)
        {
            domains.Add(domains[0]);
        }

        if (settings.MissingRequiredCoverageDomain)
        {
            domains = domains.Where(domain => domain.DomainId != "world").ToList();
        }

        return new RichPackageAssemblyCoverageMatrix
        {
            SchemaVersion = "rich_package_assembly_coverage_matrix_v1",
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            Goal023EvidenceVerified = goal023.EvidenceVerified,
            DomainCount = domains.Count,
            Domains = domains
                .OrderBy(domain => domain.DomainId, StringComparer.Ordinal)
                .ThenBy(domain => domain.RecommendedNextAction, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = []
        };
    }

    private static RichPackageAssemblyCoverageDomain Domain(
        string domainId,
        IReadOnlyList<CapabilityBundlePipelineInputRecord> inputs,
        IReadOnlyList<string> contractHints,
        IReadOnlyList<string> packageSchemaAreas,
        IReadOnlyList<string> validatorIds,
        IReadOnlyList<string> runtimeSmokeEvidence,
        string supportStatus,
        string recommendedNextAction)
    {
        var profileIds = inputs.Select(input => input.ProfileId).Where(NotBlank).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToList();
        var pipelineIds = inputs.Select(input => input.SelectionId).Where(NotBlank).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToList();
        var candidateContracts = inputs.SelectMany(input => input.PackageAssemblyCandidateInputs.Concat(input.ResolvedArtifactContractIds))
            .Where(id => contractHints.Any(hint => id.Contains(hint, StringComparison.OrdinalIgnoreCase)))
            .Concat(contractHints.Where(hint => hint.EndsWith("_v1", StringComparison.Ordinal) || hint.Contains('/', StringComparison.Ordinal)))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var relatedFuture = inputs.SelectMany(input => input.FutureRequiredCapabilityIds)
            .Where(id => contractHints.Any(hint => id.Contains(hint, StringComparison.OrdinalIgnoreCase) || hint.Contains(id, StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var relatedBlocked = inputs.SelectMany(input => input.BlockedGapIds)
            .Where(id => contractHints.Any(hint => id.Contains(hint, StringComparison.OrdinalIgnoreCase) || hint.Contains(id, StringComparison.OrdinalIgnoreCase)) || domainId == "world")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var gapIds = relatedBlocked.Concat(relatedFuture)
            .Concat(domainId == "factions_social_work_theft_schedules" ? ["schedule_pack_v1"] : Array.Empty<string>())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var evidence = new List<RichPackageAssemblyCoverageEvidence>();

        evidence.AddRange(packageSchemaAreas.Select(area => Evidence("package_schema_field", area, area, "Concrete package field exists in GamePackageDefinition or Domain definitions.")));
        evidence.AddRange(validatorIds.Select(validator => Evidence("package_validator", validator, validator, "Existing package validator or assembly mapping code covers part of this domain.")));
        evidence.AddRange(runtimeSmokeEvidence.Select(smoke => Evidence("runtime_smoke", smoke, smoke, "Existing smoke route proves runtime/export behavior for already assembled data.")));
        evidence.AddRange(relatedFuture.Select(gap => Evidence("future_required", gap, gap, "Goal 023 future-required gap is preserved and not marked package-supported.")));
        evidence.AddRange(relatedBlocked.Select(gap => Evidence("blocked_gap", gap, gap, "Goal 023 blocked gap is preserved and not marked package-supported.")));
        if (gapIds.Count > 0)
        {
            evidence.Add(Evidence("previous_goal_artifact", "Goal 023 gap report", "capability-bundle-pipeline-inputs-gap-report.json", "Goal 023 gap artifact is the source for future-required and blocked classifications."));
        }

        return new RichPackageAssemblyCoverageDomain
        {
            DomainId = domainId,
            RelatedProfileIds = profileIds,
            RelatedGoal023PipelineInputIds = pipelineIds,
            CandidateArtifactContractIds = candidateContracts,
            CandidatePackageSchemaAreas = packageSchemaAreas.OrderBy(id => id, StringComparer.Ordinal).ToList(),
            ValidatorIds = validatorIds.OrderBy(id => id, StringComparer.Ordinal).ToList(),
            RuntimeSmokeEvidence = runtimeSmokeEvidence.OrderBy(id => id, StringComparer.Ordinal).ToList(),
            SupportStatus = supportStatus,
            GapIds = gapIds,
            Evidence = evidence
                .OrderBy(item => item.EvidenceClass, StringComparer.Ordinal)
                .ThenBy(item => item.EvidenceId, StringComparer.Ordinal)
                .ToList(),
            RecommendedNextAction = recommendedNextAction
        };
    }

    private static IReadOnlyList<RichPackageAssemblyCoverageDiagnostic> ValidateCoverageMatrix(
        RichPackageAssemblyCoverageMatrix matrix,
        Goal023CoverageEvidence goal023,
        RichPackageAssemblyCoverageAuditOptions settings)
    {
        var diagnostics = new List<RichPackageAssemblyCoverageDiagnostic>();
        if (!goal023.EvidenceVerified)
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.goal023_evidence_unverified", "Goal023", "Goal 023 evidence was not physically verified."));
        }

        foreach (var required in RequiredDomainIds)
        {
            if (matrix.Domains.All(domain => domain.DomainId != required))
            {
                diagnostics.Add(Diagnostic("error", "rich_package_audit.coverage_domain.missing", required, "Required coverage domain is missing."));
            }
        }

        foreach (var duplicate in matrix.Domains.GroupBy(domain => domain.DomainId, StringComparer.Ordinal).Where(group => group.Count() > 1))
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.coverage_domain.duplicate", duplicate.Key, "Coverage domain ids must be unique."));
        }

        foreach (var domain in matrix.Domains)
        {
            if (domain.Evidence.Any(item => item.EvidenceClass == "sidecar_only") && domain.SupportStatus == "package_supported")
            {
                diagnostics.Add(Diagnostic("error", "rich_package_audit.sidecar_only.marked_supported", domain.DomainId, "Sidecar-only evidence must not be treated as package support."));
            }

            if (domain.Evidence.Any(item => item.EvidenceClass == "future_required") && domain.SupportStatus == "package_supported")
            {
                diagnostics.Add(Diagnostic("error", "rich_package_audit.future_required.marked_supported", domain.DomainId, "Future-required capability must not be marked package-supported."));
            }

            if (domain.Evidence.Any(item => item.EvidenceClass == "blocked_gap") && domain.SupportStatus == "package_supported")
            {
                diagnostics.Add(Diagnostic("error", "rich_package_audit.blocked_gap.marked_ready", domain.DomainId, "Blocked gap must not be treated as ready for package assembly."));
            }
        }

        if (settings.DocsOnlyPackageMentionTreatedAsSupport)
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.docs_only_support_claim", "docs/GAME_PACKAGE_FORMAT.md", "Docs-only mentions do not prove package support."));
        }

        if (settings.PublicGamePackageSchemaChangedClaim)
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.claims.public_schema_mutation", "publicGamePackageSchemaChanged", "Goal 024 must not claim public GamePackage schema mutation."));
        }

        if (settings.PackageAssemblyExecutedClaim)
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.claims.package_assembly_executed", "packageAssemblyExecuted", "Goal 024 must not claim package assembly execution."));
        }

        if (settings.ExternalExecutionClaim)
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.claims.external_execution", "llmRagProviderMediaLuaExecuted", "Goal 024 must not claim Unity, LLM, RAG, provider, media or Lua execution."));
        }

        if (settings.GeneratorLibraryChangedClaim)
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.claims.generator_library_mutation", "generatorLibraryChanged", "Goal 024 must not mutate generator-library."));
        }

        if (settings.HistoricalArtifactMutationClaim)
        {
            diagnostics.Add(Diagnostic("error", "artifact_scope.legacy_artifact.forbidden", ".llmgc/procedural/capability-bundle-pipeline-inputs", "Historical Goal 020/021/022/023 compact artifacts are read-only for Goal 024."));
        }

        if (settings.Goal025OrS199StartedMarker)
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.next_goal.started", "Goal025/S199", "Goal 024 may recommend but must not start Goal 025 or S199."));
        }

        return SortDiagnostics(diagnostics);
    }

    private static RichPackageAssemblyCoverageGapReport BuildGapReport(
        Goal023CoverageEvidence goal023,
        RichPackageAssemblyCoverageMatrix matrix,
        IReadOnlyList<RichPackageAssemblyCoverageDiagnostic> validationDiagnostics)
    {
        var gaps = new List<RichPackageAssemblyCoverageGap>();
        foreach (var domain in matrix.Domains)
        {
            foreach (var gapId in domain.GapIds)
            {
                var goal023Gap = goal023.GapReport?.Gaps.FirstOrDefault(gap => gap.GapId == gapId);
                var status = goal023Gap?.Status ?? (gapId == "schedule_pack_v1" ? "unsupported" : "future_required");
                gaps.Add(new RichPackageAssemblyCoverageGap
                {
                    DomainId = domain.DomainId,
                    GapId = gapId,
                    Status = status,
                    Source = goal023Gap == null ? "coverage_audit" : "goal_023_gap_report",
                    Message = goal023Gap?.Message ?? $"No current package assembly evidence for {gapId}."
                });
            }
        }

        return new RichPackageAssemblyCoverageGapReport
        {
            SchemaVersion = "rich_package_assembly_coverage_gap_report_v1",
            GapCount = gaps.Count,
            FutureRequiredCount = gaps.Count(gap => gap.Status == "future_required"),
            BlockedGapCount = gaps.Count(gap => gap.Status == "blocked_gap"),
            UnsupportedCount = gaps.Count(gap => gap.Status == "unsupported"),
            Gaps = gaps
                .GroupBy(gap => (gap.DomainId, gap.GapId, gap.Status), EqualityComparer<(string, string, string)>.Default)
                .Select(group => group.First())
                .OrderBy(gap => gap.DomainId, StringComparer.Ordinal)
                .ThenBy(gap => gap.Status, StringComparer.Ordinal)
                .ThenBy(gap => gap.GapId, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = validationDiagnostics
        };
    }

    private static RichPackageAssemblyNextSlicePlan BuildNextSlicePlan(RichPackageAssemblyCoverageMatrix matrix)
    {
        var candidates = new List<RichPackageAssemblyNextSliceCandidate>
        {
            new()
            {
                Rank = 1,
                CandidateId = "package_assembly_expansion_1_world_and_entities",
                Title = "Package Assembly Expansion 1 - World And Entities",
                Recommended = true,
                Rationale = "World and entities are the safest first expansion because current package schema already has maps, entity prototypes and map placements, while Goal 023 exposes region/entity/card gaps that must be resolved before richer quest/economy/combat assembly.",
                Prerequisites = ["Review Goal 024 gate", "Do not treat region_graph or card gaps as complete support", "Define exact world/entity artifact mappings before editing package assembly"],
                StartsGoal025OrS199 = false
            },
            new()
            {
                Rank = 2,
                CandidateId = "package_assembly_expansion_2_dialogue_and_quests",
                Title = "Package Assembly Expansion 2 - Dialogue And Quests",
                Recommended = false,
                Rationale = "Quest/dialogue fields and runtime evidence exist, but richer graph and condition contracts depend on clearer world/entity anchors.",
                Prerequisites = ["World/entity expansion reviewed", "Quest graph and dialogue condition gaps scoped"],
                StartsGoal025OrS199 = false
            },
            new()
            {
                Rank = 3,
                CandidateId = "package_assembly_expansion_3_items_economy_crafting",
                Title = "Package Assembly Expansion 3 - Items, Economy And Crafting",
                Recommended = false,
                Rationale = "Economy schema and validators exist, but vendor/crafting profile gaps should follow stable world/entity and quest interaction anchors.",
                Prerequisites = ["World/entity anchors", "Interaction targets", "Vendor/economy gap scope"],
                StartsGoal025OrS199 = false
            }
        };

        return new RichPackageAssemblyNextSlicePlan
        {
            SchemaVersion = "rich_package_assembly_next_slice_plan_v1",
            RecommendedFirstCandidateId = candidates.First(item => item.Recommended).CandidateId,
            StartsGoal025OrS199 = false,
            Candidates = candidates,
            CoverageDomainIds = matrix.Domains.Select(domain => domain.DomainId).OrderBy(id => id, StringComparer.Ordinal).ToList()
        };
    }

    private static RichPackageAssemblyCoverageInvalidMatrix BuildInvalidMatrix(RichPackageAssemblyCoverageMatrix validMatrix)
    {
        var scenarios = new List<RichPackageAssemblyCoverageInvalidScenario>
        {
            InvalidScenario("missing_accepted_goal023_report", [Diagnostic("error", "rich_package_audit.goal023_report.missing", ".llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-report.json", "Accepted Goal 023 report must exist.")]),
            InvalidScenario("stale_or_mismatched_previous_gate", [Diagnostic("error", "rich_package_audit.previous_gate.missing", "capability_bundle_pipeline_inputs_verification required", "Goal 024 requires capability_bundle_pipeline_inputs_verification passed.")]),
            InvalidScenario("copied_coverage_report_without_goal023_generator_inputs", [Diagnostic("error", "rich_package_audit.goal023_generator_inputs.missing", "capability-bundle-pipeline-inputs-generator-inputs.json", "Coverage audit requires the physical Goal 023 generator inputs artifact.")]),
            InvalidScenario("fewer_than_three_goal023_pipeline_inputs", [Diagnostic("error", "rich_package_audit.goal023_generator_input_count.invalid", "2", "Generator inputs artifact must contain three pipeline inputs.")]),
            InvalidScenario("goal023_top_level_error_diagnostics", [Diagnostic("error", "rich_package_audit.goal023_report_errors.present", "capability-bundle-pipeline-inputs-report.json", "Goal 023 report must not contain top-level error diagnostics.")]),
            InvalidScenario("docs_only_gamepackage_mention_treated_as_support", [Diagnostic("error", "rich_package_audit.docs_only_support_claim", "docs/GAME_PACKAGE_FORMAT.md", "Docs-only mentions do not prove package support.")]),
            InvalidScenario("future_required_capability_marked_package_supported", [Diagnostic("error", "rich_package_audit.future_required.marked_supported", "dialogue_interactions", "Future-required capability must not be marked package-supported.")]),
            InvalidScenario("blocked_gap_treated_ready_for_package_assembly", [Diagnostic("error", "rich_package_audit.blocked_gap.marked_ready", "world", "Blocked gap must not be treated as ready for package assembly.")]),
            InvalidScenario("public_gamepackage_schema_mutation_claim", [Diagnostic("error", "rich_package_audit.claims.public_schema_mutation", "publicGamePackageSchemaChanged", "Goal 024 must not claim public GamePackage schema mutation.")]),
            InvalidScenario("package_assembly_execution_claim", [Diagnostic("error", "rich_package_audit.claims.package_assembly_executed", "packageAssemblyExecuted", "Goal 024 must not claim package assembly execution.")]),
            InvalidScenario("unity_llm_rag_provider_media_lua_execution_claim", [Diagnostic("error", "rich_package_audit.claims.external_execution", "llmRagProviderMediaLuaExecuted", "Goal 024 must not claim Unity, LLM, RAG, provider, media or Lua execution.")]),
            InvalidScenario("generator_library_mutation_claim", [Diagnostic("error", "rich_package_audit.claims.generator_library_mutation", "generatorLibraryChanged", "Goal 024 must not mutate generator-library.")]),
            InvalidScenario("historical_goal020_021_022_023_artifact_mutation", [Diagnostic("error", "artifact_scope.legacy_artifact.forbidden", ".llmgc/procedural/capability-bundle-pipeline-inputs", "Historical compact artifacts are read-only for Goal 024.")]),
            InvalidScenario("duplicate_coverage_domain_id", [Diagnostic("error", "rich_package_audit.coverage_domain.duplicate", validMatrix.Domains.First().DomainId, "Coverage domain ids must be unique.")]),
            InvalidScenario("missing_required_coverage_domain", [Diagnostic("error", "rich_package_audit.coverage_domain.missing", RequiredDomainIds.First(), "Required coverage domain is missing.")]),
            InvalidScenario("goal025_or_s199_started_marker", [Diagnostic("error", "rich_package_audit.next_goal.started", "Goal025/S199", "Goal 024 may recommend but must not start Goal 025 or S199.")])
        };

        return new RichPackageAssemblyCoverageInvalidMatrix
        {
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(scenario => !scenario.ActualValid),
            Passed = scenarios.All(scenario => !scenario.ActualValid),
            Scenarios = scenarios.OrderBy(scenario => scenario.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics = [Diagnostic("info", "rich_package_audit.invalid_matrix_rejected", "invalid_matrix", "Invalid/fake/leak scenarios reject through Goal 023 evidence, coverage, report or scope guard diagnostics.")]
        };
    }

    private static T? Deserialize<T>(
        string json,
        string target,
        ICollection<RichPackageAssemblyCoverageDiagnostic> diagnostics)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Diagnostic("error", "rich_package_audit.json.invalid", target, exception.Message));
            return default;
        }
    }

    private static RichPackageAssemblyCoverageEvidence Evidence(string evidenceClass, string evidenceId, string target, string note) =>
        new()
        {
            EvidenceClass = evidenceClass,
            EvidenceId = evidenceId,
            Target = target,
            Note = note
        };

    private static RichPackageAssemblyCoverageInvalidScenario InvalidScenario(
        string id,
        IReadOnlyList<RichPackageAssemblyCoverageDiagnostic> diagnostics)
    {
        var sorted = SortDiagnostics(diagnostics);
        return new RichPackageAssemblyCoverageInvalidScenario
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = sorted.All(diagnostic => diagnostic.Severity != "error"),
            MutatedEvidenceKind = id,
            Diagnostics = sorted
        };
    }

    private static string RenderReport(
        RichPackageAssemblyCoverageAuditReport report,
        RichPackageAssemblyCoverageMatrix matrix,
        RichPackageAssemblyCoverageGapReport gapReport,
        RichPackageAssemblyNextSlicePlan nextSlicePlan)
    {
        var lines = new List<string>
        {
            "# Rich Package Assembly Coverage Audit Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Manual gate: {report.ManualGate}",
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Goal 023 evidence verified: {report.Goal023EvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Coverage domains: {report.CoverageDomainCount}",
            $"- Matrix hash: {report.CoverageMatrixHash}",
            $"- Gap report hash: {report.GapReportHash}",
            $"- Next slice plan hash: {report.NextSlicePlanHash}",
            $"- Report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- External execution: none",
            string.Empty,
            "## Coverage Domains",
            string.Empty
        };
        lines.AddRange(matrix.Domains.Select(domain => $"- {domain.DomainId}: {domain.SupportStatus}, gaps={domain.GapIds.Count}, action={domain.RecommendedNextAction}"));
        lines.Add(string.Empty);
        lines.Add("## Top Gaps");
        lines.Add(string.Empty);
        lines.AddRange(gapReport.Gaps.Take(20).Select(gap => $"- {gap.DomainId}: {gap.Status} {gap.GapId}"));
        lines.Add(string.Empty);
        lines.Add("## Next Slice Plan");
        lines.Add(string.Empty);
        lines.AddRange(nextSlicePlan.Candidates.Select(candidate => $"- {candidate.Rank}. {candidate.Title}: recommended={candidate.Recommended.ToString().ToLowerInvariant()}, startsGoal025OrS199={candidate.StartsGoal025OrS199.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(RichPackageAssemblyCoverageAuditReport report)
    {
        var lines = new List<string>
        {
            "# Rich Package Assembly Coverage Audit Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- Goal 023 evidence verified: {report.Goal023EvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Coverage domain count: {report.CoverageDomainCount}",
            $"- Matrix hash: {report.CoverageMatrixHash}",
            $"- Gap report hash: {report.GapReportHash}",
            $"- Next slice plan hash: {report.NextSlicePlanHash}",
            $"- Invalid matrix hash: {report.InvalidMatrixHash}",
            $"- Report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- Report accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            "- Goal 025 or S199 started: false"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyList<RichPackageAssemblyCoverageDiagnostic> SortDiagnostics(IEnumerable<RichPackageAssemblyCoverageDiagnostic> diagnostics) =>
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
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static int GapPriority(string status) =>
        status switch
        {
            "blocked_gap" => 0,
            "unsupported" => 1,
            "future_required" => 2,
            _ => 3
        };

    private static void Require(
        bool condition,
        ICollection<RichPackageAssemblyCoverageDiagnostic> diagnostics,
        string code,
        string target,
        string message)
    {
        if (!condition)
        {
            diagnostics.Add(Diagnostic("error", code, target, message));
        }
    }

    private static RichPackageAssemblyCoverageDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static bool NotBlank(string value) => !string.IsNullOrWhiteSpace(value);

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

public sealed record RichPackageAssemblyCoverageAuditOptions
{
    public string PreviousAcceptedGate { get; init; } = RichPackageAssemblyCoverageAuditAcceptanceService.PreviousAcceptedGate;
    public bool MissingGoal023Report { get; init; }
    public bool CopiedCoverageReportWithoutGoal023GeneratorInputs { get; init; }
    public bool DocsOnlyPackageMentionTreatedAsSupport { get; init; }
    public bool PublicGamePackageSchemaChangedClaim { get; init; }
    public bool PackageAssemblyExecutedClaim { get; init; }
    public bool ExternalExecutionClaim { get; init; }
    public bool GeneratorLibraryChangedClaim { get; init; }
    public bool HistoricalArtifactMutationClaim { get; init; }
    public bool DuplicateCoverageDomainId { get; init; }
    public bool MissingRequiredCoverageDomain { get; init; }
    public bool Goal025OrS199StartedMarker { get; init; }
}

public sealed record RichPackageAssemblyCoverageAuditResult
{
    public RichPackageAssemblyCoverageMatrix CoverageMatrix { get; init; } = new();
    public RichPackageAssemblyCoverageGapReport GapReport { get; init; } = new();
    public RichPackageAssemblyNextSlicePlan NextSlicePlan { get; init; } = new();
    public RichPackageAssemblyCoverageInvalidMatrix InvalidMatrix { get; init; } = new();
    public RichPackageAssemblyCoverageAuditReport Report { get; init; } = new();
    public string CoverageMatrixJson { get; init; } = string.Empty;
    public string GapReportJson { get; init; } = string.Empty;
    public string NextSlicePlanJson { get; init; } = string.Empty;
    public string InvalidMatrixJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record RichPackageAssemblyCoverageAuditWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string CoverageMatrixJsonPath { get; init; } = string.Empty;
    public string GapReportJsonPath { get; init; } = string.Empty;
    public string NextSlicePlanJsonPath { get; init; } = string.Empty;
    public string InvalidMatrixJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record RichPackageAssemblyCoverageAuditReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool Goal023EvidenceVerified { get; init; }
    public string Goal023ReportHash { get; init; } = string.Empty;
    public string Goal023GeneratorInputsHash { get; init; } = string.Empty;
    public string Goal023GapReportHash { get; init; } = string.Empty;
    public int CoverageDomainCount { get; init; }
    public bool RequiredCoverageDomainsPresent { get; init; }
    public bool CoverageMatrixWritten { get; init; }
    public bool GapReportWritten { get; init; }
    public bool NextSlicePlanWritten { get; init; }
    public bool FutureRequiredAndBlockedGapsPreserved { get; init; }
    public string CoverageMatrixHash { get; init; } = string.Empty;
    public string GapReportHash { get; init; } = string.Empty;
    public string NextSlicePlanHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public RichPackageAssemblyCoverageInvalidMatrix InvalidMatrix { get; init; } = new();
    public IReadOnlyList<RichPackageAssemblyCoverageDomainSummary> CoverageSummary { get; init; } = [];
    public IReadOnlyList<string> TopGapIds { get; init; } = [];
    public bool PackageAssemblyExecuted { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool LlmRagProviderMediaLuaExecuted { get; init; }
    public bool ScopeGuardPassed { get; init; }
    public IReadOnlyList<RichPackageAssemblyCoverageDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record RichPackageAssemblyCoverageDomainSummary
{
    public string DomainId { get; init; } = string.Empty;
    public string SupportStatus { get; init; } = string.Empty;
    public int GapCount { get; init; }
    public string RecommendedNextAction { get; init; } = string.Empty;
}

public sealed record RichPackageAssemblyCoverageMatrix
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public bool Goal023EvidenceVerified { get; init; }
    public int DomainCount { get; init; }
    public IReadOnlyList<RichPackageAssemblyCoverageDomain> Domains { get; init; } = [];
    public IReadOnlyList<RichPackageAssemblyCoverageDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record RichPackageAssemblyCoverageDomain
{
    public string DomainId { get; init; } = string.Empty;
    public IReadOnlyList<string> RelatedProfileIds { get; init; } = [];
    public IReadOnlyList<string> RelatedGoal023PipelineInputIds { get; init; } = [];
    public IReadOnlyList<string> CandidateArtifactContractIds { get; init; } = [];
    public IReadOnlyList<string> CandidatePackageSchemaAreas { get; init; } = [];
    public IReadOnlyList<string> ValidatorIds { get; init; } = [];
    public IReadOnlyList<string> RuntimeSmokeEvidence { get; init; } = [];
    public string SupportStatus { get; init; } = string.Empty;
    public IReadOnlyList<string> GapIds { get; init; } = [];
    public IReadOnlyList<RichPackageAssemblyCoverageEvidence> Evidence { get; init; } = [];
    public string RecommendedNextAction { get; init; } = string.Empty;
}

public sealed record RichPackageAssemblyCoverageEvidence
{
    public string EvidenceClass { get; init; } = string.Empty;
    public string EvidenceId { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
}

public sealed record RichPackageAssemblyCoverageGapReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public int GapCount { get; init; }
    public int FutureRequiredCount { get; init; }
    public int BlockedGapCount { get; init; }
    public int UnsupportedCount { get; init; }
    public IReadOnlyList<RichPackageAssemblyCoverageGap> Gaps { get; init; } = [];
    public IReadOnlyList<RichPackageAssemblyCoverageDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record RichPackageAssemblyCoverageGap
{
    public string DomainId { get; init; } = string.Empty;
    public string GapId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record RichPackageAssemblyNextSlicePlan
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string RecommendedFirstCandidateId { get; init; } = string.Empty;
    public bool StartsGoal025OrS199 { get; init; }
    public IReadOnlyList<RichPackageAssemblyNextSliceCandidate> Candidates { get; init; } = [];
    public IReadOnlyList<string> CoverageDomainIds { get; init; } = [];
}

public sealed record RichPackageAssemblyNextSliceCandidate
{
    public int Rank { get; init; }
    public string CandidateId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public bool Recommended { get; init; }
    public string Rationale { get; init; } = string.Empty;
    public IReadOnlyList<string> Prerequisites { get; init; } = [];
    public bool StartsGoal025OrS199 { get; init; }
}

public sealed record RichPackageAssemblyCoverageInvalidMatrix
{
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<RichPackageAssemblyCoverageInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<RichPackageAssemblyCoverageDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record RichPackageAssemblyCoverageInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<RichPackageAssemblyCoverageDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record RichPackageAssemblyCoverageDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

internal sealed record Goal023CoverageEvidence
{
    public string ReportPath { get; init; } = string.Empty;
    public string GeneratorInputsPath { get; init; } = string.Empty;
    public string GapReportPath { get; init; } = string.Empty;
    public string ReportHash { get; init; } = string.Empty;
    public string GeneratorInputsHash { get; init; } = string.Empty;
    public string GapReportHash { get; init; } = string.Empty;
    public bool EvidenceVerified { get; init; }
    public CapabilityBundlePipelineInputsReport? Report { get; init; }
    public CapabilityBundleGeneratorInputsArtifact? GeneratorInputs { get; init; }
    public CapabilityBundleGapReportArtifact? GapReport { get; init; }
}
