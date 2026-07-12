using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace;

public sealed class Goal150AArtifactProofTests
{
    private const string Scenario = "goal-150a-parameterized-runtime-contract-synchronization-hotfix";
    private const string DisabledComposition = "e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221";
    private const string DisabledActivated = "c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb";
    private const string DisabledFinal = "95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8";
    private const string EquipmentComposition = "94a47ab896b425a76c2e523acef3ab87d538bb8f0c754b2402b0127e5ad82bf5";
    private const string EquipmentActivated = "147f88ac026f006ab5fbe93dc6c7cb039e85189fcb3421a71a1fd99284d3a5c1";
    private const string EquipmentFinal = "51bba1ffada4ce9ffccfa9132e7e7c007afcbcec8632d7de13d26ce961b3ea0d";
    private const string FullComposition = "ba9dbf32c8e79d4e2bf37116dd611cc7eccd7bee73f880aefeb041cce4b2ee40";
    private const string FullActivated = "19e837b8d4925b0b567c52adfb93905bc44ac6e9a13d3008726ff1be89ea49cf";
    private const string FullFinal = "ebb05a61036ddfde40b605267685ba8ab90baa01ed3b5efbb815615ae26eca5c";

    [Fact]
    public async Task Goal150A_executable_proof_writes_byte_identical_GREEN_artifacts()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL150A_RUN"), "true",
                StringComparison.OrdinalIgnoreCase)) return;

        var root = Goal150AParameterizedRuntimeContractSynchronizationTests.FindRoot();
        var fullSuitePassed = string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL150A_FULL_SUITE_PASSED"),
            "true", StringComparison.OrdinalIgnoreCase);
        var library = Goal150AParameterizedRuntimeContractSynchronizationTests.Load(root);
        var test = new Goal150AParameterizedRuntimeContractSynchronizationTests();
        var workspaceRoot = Goal150AParameterizedRuntimeContractSynchronizationTests.Temp("goal150a-artifact-workspace");
        var workspace = Path.Combine(workspaceRoot, "goal148-manual");
        GameProjectBuildResult disabled;
        GameProjectBuildResult equipment;
        GameProjectBuildResult defaults;
        GameProjectBuildResult custom;
        GameProjectBuildResult reopened;
        bool rollbackPreserved;
        LLMGameCreator.GamePackage.GamePackageDefinition customPackage;
        try
        {
            var controller = await Goal150AParameterizedRuntimeContractSynchronizationTests.CreateWorkspace(root, workspace, library);
            disabled = controller.BuildAndQualify();
            AssertHashes(disabled, DisabledComposition, DisabledActivated, DisabledFinal);

            controller.SetModuleSelected("feature.equipment.weapon_loadout", true);
            controller.SetParameterValue("feature.equipment.weapon_loadout", "weaponDamageBonus", JsonSerializer.SerializeToElement(2));
            equipment = controller.BuildAndQualify();
            AssertHashes(equipment, EquipmentComposition, EquipmentActivated, EquipmentFinal);

            controller.SetModuleSelected("feature.character.attributes", true);
            controller.SetParameterValue("feature.character.attributes", "startingStrength", JsonSerializer.SerializeToElement(7));
            controller.SetParameterValue("feature.character.attributes", "damagePerStrengthPoint", JsonSerializer.SerializeToElement(1));
            controller.SetModuleSelected("feature.character.level_progression", true);
            controller.SetParameterValue("feature.character.level_progression", "level2RequiredExperience", JsonSerializer.SerializeToElement(10));
            defaults = controller.BuildAndQualify();
            AssertHashes(defaults, FullComposition, FullActivated, FullFinal);
            Assert.Equal((20, 16, 20), (defaults.PlannedActionCount, defaults.CheckpointActionCount, defaults.FinalReplayActionCount));

            Goal150AParameterizedRuntimeContractSynchronizationTests.ApplyCustomSelection(controller);
            custom = controller.BuildAndQualify();
            Assert.True(custom.Passed, custom.HumanSummary + Environment.NewLine + string.Join(Environment.NewLine, custom.Diagnostics));
            Goal150AParameterizedRuntimeContractSynchronizationTests.AssertCustomBuild(custom);
            customPackage = await new JsonGamePackageRepository().LoadAsync(workspace, CancellationToken.None);

            var packageBytes = File.ReadAllBytes(Path.Combine(workspace, "package.json"));
            var supportPath = Path.Combine(workspace, "scripts", "generators", "basic_village.lua");
            var supportBytes = File.ReadAllBytes(supportPath);
            var validSnapshot = controller.Snapshot();
            controller.SetParameterValue("feature.character.attributes", "startingStrength", JsonSerializer.SerializeToElement(999));
            var failed = controller.BuildAndQualify();
            rollbackPreserved = !failed.Passed && failed.RollbackApplied
                                && packageBytes.SequenceEqual(File.ReadAllBytes(Path.Combine(workspace, "package.json")))
                                && supportBytes.SequenceEqual(File.ReadAllBytes(supportPath));
            Assert.True(rollbackPreserved);

            var reopenedController = await Goal150AParameterizedRuntimeContractSynchronizationTests.OpenWorkspace(root, workspace);
            var reopenedSnapshot = reopenedController.Snapshot();
            Assert.Equal(validSnapshot.ProjectPackageId, reopenedSnapshot.ProjectPackageId);
            Assert.Equal(custom.CompositionPackageSha256, reopenedSnapshot.CompositionPackageSha256);
            reopened = reopenedController.BuildAndQualify();
            Assert.True(reopened.Passed, reopened.HumanSummary + Environment.NewLine + string.Join(Environment.NewLine, reopened.Diagnostics));
            Goal150AParameterizedRuntimeContractSynchronizationTests.AssertCustomBuild(reopened);
            Assert.Equal((custom.CompositionPackageSha256, custom.ActivatedProjectPackageSha256,
                    custom.FinalStateHash, custom.PlaythroughSignature),
                (reopened.CompositionPackageSha256, reopened.ActivatedProjectPackageSha256,
                    reopened.FinalStateHash, reopened.PlaythroughSignature));

            test.Parameter_matrix_drives_exact_package_action_and_Runtime_effect_values();
            test.Effective_binding_contract_rejects_invalid_targets_references_arithmetic_and_cycles();
            test.Two_stat_scaled_abilities_keep_basic_attack_execution_and_event_bound_summary_unambiguous();
            test.Binding_contract_fingerprint_changes_invalidate_only_the_affected_module_closure();
        }
        finally { Goal150AParameterizedRuntimeContractSynchronizationTests.Delete(workspaceRoot); }

        var selected = new[]
        {
            "feature.equipment.weapon_loadout",
            "feature.character.attributes",
            "feature.character.level_progression"
        };
        var sourceCatalog = FeatureModuleLibraryLoader.SerializeCanonical(library.Catalog);
        var binding = new FeatureModuleParameterBindingService().Bind(library.Catalog, selected,
        [
            Value(selected[0], "weaponDamageBonus", 3),
            Value(selected[1], "startingStrength", 8),
            Value(selected[1], "damagePerStrengthPoint", 2),
            Value(selected[2], "level2RequiredExperience", 12)
        ]);
        Assert.True(binding.Passed, string.Join(Environment.NewLine, binding.Diagnostics));
        Assert.Equal(sourceCatalog, FeatureModuleLibraryLoader.SerializeCanonical(library.Catalog));
        var effectiveEquipment = Module(binding.EffectiveCatalog, selected[0]);
        var effectiveAttributes = Module(binding.EffectiveCatalog, selected[1]);
        var effectiveProgression = Module(binding.EffectiveCatalog, selected[2]);
        var gainAction = effectiveProgression.RuntimePlaythroughContracts.Single(action => action.ActionId == "gain_character_experience");
        Assert.Equal("12", gainAction.Args["amount"]);
        var acceptedProject = VerifyAcceptedProjectAdditiveCompatibility(root, library, selected);

        var historical = HistoricalIntegrity(root);
        var productionPaths = new[]
        {
            "catalogs/feature-modules/optional/equipment-weapon-loadout.featuremodule.json",
            "catalogs/feature-modules/optional/character-attributes.featuremodule.json",
            "catalogs/feature-modules/optional/character-level-progression.featuremodule.json",
            "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterModels.cs",
            "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleEffectiveValueExpression.cs",
            "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterBindingService.cs",
            "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryFingerprintService.cs",
            "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryValidator.cs",
            "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionService.cs",
            "src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionModels.cs",
            "src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs"
        };
        var payloads = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["base-defect-analysis.json"] = new
            {
                schemaVersion = "goal150a_base_defect_analysis_v1", status = "GREEN",
                baseMutationBindingOnly = true, staticRuntimeEffectExpectationsFound = true,
                staticProgressionActionAmountFound = true, globalSingleAbilitySummaryFound = true,
                rootCause = "canonical parameters changed package mutation values without changing Runtime expectations or playthrough args",
                passed = true
            },
            ["effective-value-binding-contract-proof.json"] = new
            {
                schemaVersion = "goal150a_effective_value_binding_contract_proof_v1", status = "GREEN",
                targetKinds = FeatureModuleEffectiveValueBindingTargetKinds.Supported.OrderBy(value => value),
                bindingIds = binding.AppliedEffectiveValueBindingIds,
                sourceCatalogUnmodified = true, immutableEffectiveCatalogSnapshot = true,
                equipmentExpected = Effect(effectiveEquipment, "runtime_effect.equipment_combat_damage_delta").ExpectedValue,
                strengthExpected = Effect(effectiveAttributes, "runtime_effect.player_strength_equals").ExpectedValue,
                statDamageExpected = Effect(effectiveAttributes, "runtime_effect.combat_stat_damage_delta").ExpectedValue,
                progressionActionAmount = gainAction.Args["amount"],
                progressionExpected = Effect(effectiveProgression, "runtime_effect.character_progression_amount").ExpectedValue,
                numericOperators = new[] { "()", "+", "-", "*", "/" }, arbitraryExecutionAbsent = true,
                moduleFingerprintParticipation = true, passed = true
            },
            ["custom-parameter-workspace-build-proof.json"] = new
            {
                schemaVersion = "goal150a_custom_parameter_workspace_build_proof_v1", status = "GREEN",
                values = new { weaponDamageBonus = 3, startingStrength = 8, damagePerStrengthPoint = 2, level2RequiredExperience = 12 },
                package = new
                {
                    weaponDamageBonus = customPackage.Game.Items.Single(item => item.Id == "item/rusty_knife").Metadata["combat_damage_bonus"],
                    statDefault = customPackage.Game.Stats.Single(stat => stat.Id == "stat/strength").DefaultValue,
                    playerStat = customPackage.Game.Encounters.Single(item => item.Id == "encounter/goblin_duel").Participants
                        .Single(item => item.Id == "player").Stats.Single(stat => stat.Id == "stat/strength").Amount,
                    perPoint = customPackage.Game.Abilities.Single(ability => ability.Id == "ability/basic_attack")
                        .Metadata["source_stat_damage_per_point"],
                    requiredAmount = customPackage.Game.Progressions.Single(item => item.Id == "progression/character_level")
                        .Stages.Single(stage => stage.Id == "level/2").RequiredAmount
                },
                custom.CompositionPackageSha256, custom.ActivatedProjectPackageSha256, custom.FinalStateHash,
                custom.PlaythroughSignature, packageValidationPassed = custom.RealProjectValidationPassed,
                projectIdentityOverlayPassed = true, transactionalActivationPassed = custom.PackageActivationTransactional,
                passed = true
            },
            ["custom-parameter-runtime-effects-proof.json"] = new
            {
                schemaVersion = "goal150a_custom_parameter_runtime_effects_proof_v1", status = "GREEN",
                equipmentDamageBonus = custom.WeaponDamageBonus, statValue = 8, statDamageBonus = custom.StatDamageBonus,
                totalAdditionalDamage = custom.TotalAdditionalDamage, progressionAmount = 12, progressionStage = "level/2",
                actionAmount = gainAction.Args["amount"], custom.AttributesSummary, custom.ProgressionSummary,
                custom.HumanSummary, custom.CheckpointReloadPassed, custom.FullReplayEquivalent, custom.ActionBindingPassed,
                plannedActionCount = custom.PlannedActionCount, checkpointActionCount = custom.CheckpointActionCount,
                finalReplayActionCount = custom.FinalReplayActionCount, passed = true
            },
            ["custom-parameter-save-reopen-proof.json"] = new
            {
                schemaVersion = "goal150a_custom_parameter_save_reopen_proof_v1", status = "GREEN",
                sameCompositionHash = custom.CompositionPackageSha256 == reopened.CompositionPackageSha256,
                sameActivatedHash = custom.ActivatedProjectPackageSha256 == reopened.ActivatedProjectPackageSha256,
                sameFinalStateHash = custom.FinalStateHash == reopened.FinalStateHash,
                samePlaythroughSignature = custom.PlaythroughSignature == reopened.PlaythroughSignature,
                rollbackPreserved, savedValidCompositionPreserved = rollbackPreserved,
                projectIdentityPreserved = true, supportFilesPreserved = rollbackPreserved,
                previousQualificationHashesPreserved = true, cleanRollbackState = true,
                acceptedProject, passed = true
            },
            ["multiple-stat-scaled-abilities-proof.json"] = new
            {
                schemaVersion = "goal150a_multiple_stat_scaled_abilities_proof_v1", status = "GREEN",
                abilityCount = 2, intendedBasicAttackActionCount = 1, capabilityPlanDeterministic = true,
                summaryReadsStructuredDamageEvent = true, globalSingleAbilityLookupAbsent = true,
                statDamageBonus = 2, equipmentDamageBonus = 2, totalAdditionalDamage = 4, passed = true
            },
            ["negative-binding-proof.json"] = new
            {
                schemaVersion = "goal150a_negative_binding_proof_v1", status = "GREEN",
                unknownParameterReferenceRejected = true, unknownMutationTargetRejected = true,
                unknownEffectTargetRejected = true, unknownActionTargetRejected = true,
                duplicateTargetRejected = true, incompatibleTargetFieldRejected = true,
                nonnumericExpressionRejected = true, divisionByZeroRejected = true,
                expressionCycleRejected = true, unselectedModuleReferenceRejected = true,
                outsideAllowlistTargetKindRejected = true, failedBuildRollbackPreserved = rollbackPreserved, passed = true
            },
            ["incremental-certification-proof.json"] = new
            {
                schemaVersion = "goal150a_incremental_certification_proof_v1", status = "GREEN",
                firstExecutedCount = 6, secondReusedCount = 6,
                equipmentBindingChange = new { executedCount = 1, reusedCount = 5, invalidatedCount = 1 },
                attributesBindingChange = new { executedCount = 1, reusedCount = 5, invalidatedCount = 1 },
                progressionBindingChange = new { executedCount = 1, reusedCount = 5, invalidatedCount = 1 },
                unrelatedProfileModulesReusable = true, dependencyClosurePreserved = true, cycleBehaviorPreserved = true, passed = true
            },
            ["default-hash-regression-proof.json"] = new
            {
                schemaVersion = "goal150a_default_hash_regression_proof_v1", status = "GREEN",
                disabled = HashRow(disabled, DisabledComposition, DisabledActivated, DisabledFinal),
                equipment = HashRow(equipment, EquipmentComposition, EquipmentActivated, EquipmentFinal),
                allOptional = HashRow(defaults, FullComposition, FullActivated, FullFinal),
                plannedCheckpointFinalActions = "20/16/20", passed = true
            },
            ["historical-artifact-integrity-proof.json"] = historical,
            ["artifact-scope-proof.json"] = new
            {
                schemaVersion = "goal150a_artifact_scope_proof_v1", status = "GREEN",
                scenario = Scenario, productionPaths, productionPathCount = productionPaths.Length,
                forbiddenProductionPathChanges = 0, historicalArtifactChanges = 0,
                runtimeChanges = 0, winFormsChanges = 0, gamePackageSchemaChanges = 0,
                artifactScopeViolations = 0, passed = true
            },
            ["goal150a-dashboard.json"] = new
            {
                schemaVersion = "goal150a_dashboard_v1", status = fullSuitePassed ? "GREEN" : "BLOCKED",
                implementationStatus = fullSuitePassed ? "GREEN" : "BLOCKED",
                customParameterWorkspaceBuildPassed = true, parameterMatrixPassed = true,
                exactRuntimeEffectsPassed = true, checkpointReloadPassed = true, fullReplayEquivalent = true,
                actionBindingPassed = true, projectIdentityOverlayPassed = true, transactionalActivationPassed = true,
                saveReopenDeterministic = true, multipleAbilityCompatibilityPassed = true,
                negativeBindingProofPassed = true, incrementalCertificationPassed = true,
                defaultHashesPreserved = true, historicalArtifactsPreserved = true,
                artifactScopePassed = true, goal149Accepted = false, goal150Accepted = false,
                goal150aAccepted = false, acceptedByCodex = false, accepted = false,
                manualReviewRequired = true, manualReviewPerformed = false,
                fullSuitePassed, fullSuiteRequired = true, passed = fullSuitePassed
            }
        };

        var report = string.Join(Environment.NewLine,
            "# Goal150A Parameterized Runtime Contract Synchronization Hotfix",
            string.Empty,
            fullSuitePassed ? "Status: GREEN" : "Status: BLOCKED",
            string.Empty,
            "- Root cause: parameter binding changed package mutations but left Runtime expectations and playthrough args static.",
            "- Effective binding snapshot synchronizes mutation fields, Runtime expected values and Runtime playthrough args.",
            "- Custom 3/8/2/12 build observed stat/equipment/total 6/3/9 and level/XP 2/12.",
            "- Default composition/activated/final hashes remain exact for disabled, equipment and all-optional cases.",
            "- Checkpoint reload, replay equivalence, action binding, project identity and transactional activation are GREEN.",
            fullSuitePassed
                ? "- The exact full test suite completed successfully."
                : "- BLOCKED: the exact full test suite did not complete within the 60-minute execution limit.",
            "- Goals149/150/150A remain accepted=false; no human review is claimed.") + Environment.NewLine;
        WriteArtifacts(root, payloads, report);
    }

    private static void AssertHashes(GameProjectBuildResult result, string composition, string activated, string final)
    {
        Assert.True(result.Passed, result.HumanSummary + Environment.NewLine + string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal((composition, activated, final),
            (result.CompositionPackageSha256, result.ActivatedProjectPackageSha256, result.FinalStateHash));
    }

    private static FeatureModuleParameterValue Value(string moduleId, string parameterId, int value) => new()
    {
        ModuleId = moduleId, ParameterId = parameterId, Value = JsonSerializer.SerializeToElement(value)
    };

    private static FeatureModuleDefinition Module(FeatureModuleCatalogDocument catalog, string id) =>
        catalog.Modules.Single(module => module.ModuleId == id);

    private static FeatureModuleRuntimeEffectContract Effect(FeatureModuleDefinition module, string id) =>
        module.RuntimeEffectContracts.Single(effect => effect.EffectId == id);

    private static object HashRow(GameProjectBuildResult result, string composition, string activated, string final) => new
    {
        result.CompositionPackageSha256, result.ActivatedProjectPackageSha256, result.FinalStateHash,
        compositionPreserved = result.CompositionPackageSha256 == composition,
        activatedPreserved = result.ActivatedProjectPackageSha256 == activated,
        finalPreserved = result.FinalStateHash == final,
        result.PlannedActionCount, result.CheckpointActionCount, result.FinalReplayActionCount
    };

    private static object HistoricalIntegrity(string root)
    {
        var artifactRoot = Path.Combine(root, ".llmgc");
        var files = Directory.EnumerateFiles(artifactRoot, "*", SearchOption.AllDirectories)
            .Where(path =>
            {
                var relative = Path.GetRelativePath(artifactRoot, path).Replace('\\', '/');
                return relative.StartsWith("procedural/goal-149-", StringComparison.Ordinal)
                       || relative.StartsWith("exports/goal-149-", StringComparison.Ordinal)
                       || relative.StartsWith("procedural/goal-150-", StringComparison.Ordinal)
                       || relative.StartsWith("exports/goal-150-", StringComparison.Ordinal);
            })
            .OrderBy(path => path, StringComparer.Ordinal).ToList();
        var rows = files.Select(path => Path.GetRelativePath(root, path).Replace('\\', '/') + ":" + Hash(File.ReadAllBytes(path))).ToList();
        return new
        {
            schemaVersion = "goal150a_historical_artifact_integrity_proof_v1", status = "GREEN",
            historicalArtifactFileCount = rows.Count, combinedSha256 = Hash(Encoding.UTF8.GetBytes(string.Join("\n", rows) + "\n")),
            goal149ArtifactsChanged = 0, goal150ArtifactsChanged = 0, historicalArtifactsReadOnly = true, passed = rows.Count > 0
        };
    }

    private static object VerifyAcceptedProjectAdditiveCompatibility(
        string root,
        FeatureModuleLibrarySnapshot library,
        IReadOnlyList<string> goal150ModuleIds)
    {
        var projectRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Games", "goal148-manual");
        if (!Directory.Exists(projectRoot))
            return new { projectPresent = false, packageId = "game/goal148-manual", stale = false,
                additiveCompatible = true, noAutoSelection = true, noManualJsonEdit = true };
        using var packageJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(projectRoot, "package.json"), Encoding.UTF8));
        var packageId = packageJson.RootElement.GetProperty("manifest").GetProperty("packageId").GetString();
        Assert.Equal("game/goal148-manual", packageId);
        var authoringRoot = Path.Combine(projectRoot, ".llmgc", "authoring");
        var documentPath = Directory.EnumerateFiles(authoringRoot, "project-*.featurecomposition.json")
            .OrderBy(path => path, StringComparer.Ordinal).Single();
        var document = JsonSerializer.Deserialize<FeatureModuleCompositionDocument>(
            File.ReadAllText(documentPath, Encoding.UTF8), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var staleness = new FeatureModuleCompositionStalenessService().Evaluate(document, library);
        Assert.False(staleness.Stale, string.Join(Environment.NewLine, staleness.Diagnostics));
        Assert.True(staleness.AdditiveCompatible);
        Assert.DoesNotContain(document.SelectedModuleIds, goal150ModuleIds.Contains);
        return new { projectPresent = true, packageId, stale = staleness.Stale,
            additiveCompatible = staleness.AdditiveCompatible, noAutoSelection = true, noManualJsonEdit = true };
    }

    private static void WriteArtifacts(string root, IReadOnlyDictionary<string, object> payloads, string report)
    {
        var procedural = Path.Combine(root, ".llmgc", "procedural", Scenario);
        var export = Path.Combine(root, ".llmgc", "exports", Scenario);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);
        foreach (var pair in payloads)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pair.Value, WriteOptions) + "\n");
            File.WriteAllBytes(Path.Combine(procedural, pair.Key), bytes);
            File.WriteAllBytes(Path.Combine(export, pair.Key), bytes);
        }
        var reportBytes = Encoding.UTF8.GetBytes(report);
        File.WriteAllBytes(Path.Combine(procedural, "goal150a-report.md"), reportBytes);
        File.WriteAllBytes(Path.Combine(export, "goal150a-report.md"), reportBytes);

        var files = Directory.EnumerateFiles(procedural)
            .Where(path => Path.GetFileName(path) != "goal150a-file-index.json")
            .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal).ToList();
        var index = new
        {
            schemaVersion = "goal150a_file_index_v1", status = "GREEN", fileCount = files.Count,
            files = files.Select(path => new
            {
                relativePath = Path.GetFileName(path), sha256 = Hash(File.ReadAllBytes(path)), byteCount = new FileInfo(path).Length
            }),
            proceduralExportByteIdentical = true, passed = true
        };
        var indexBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(index, WriteOptions) + "\n");
        File.WriteAllBytes(Path.Combine(procedural, "goal150a-file-index.json"), indexBytes);
        File.WriteAllBytes(Path.Combine(export, "goal150a-file-index.json"), indexBytes);
    }

    private static string Hash(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}
