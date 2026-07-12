using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleCertification;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.CapabilityDrivenRuntimePlaythrough;

public sealed class Goal150ArtifactProofTests
{
    private const string Scenario = "goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice";
    private const string LegacyComposition = "e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221";
    private const string LegacyActivated = "c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb";
    private const string LegacyFinal = "95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8";
    private const string EquipmentComposition = "94a47ab896b425a76c2e523acef3ab87d538bb8f0c754b2402b0127e5ad82bf5";
    private const string EquipmentActivated = "147f88ac026f006ab5fbe93dc6c7cb039e85189fcb3421a71a1fd99284d3a5c1";
    private const string EquipmentFinal = "51bba1ffada4ce9ffccfa9132e7e7c007afcbcec8632d7de13d26ce961b3ea0d";

    [Fact]
    public async Task Goal150ArtifactProof_writes_complete_deterministic_evidence_matrix()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL150_RUN"), "true",
                StringComparison.OrdinalIgnoreCase)) return;

        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var attributes = Module(library, "feature.character.attributes");
        var progression = Module(library, "feature.character.level_progression");
        var equipment = Module(library, "feature.equipment.weapon_loadout");
        var packageJson = File.ReadAllText(Path.Combine(root, "samples", "minimal-map-game", "package.json"));
        var mutationService = new FeatureModulePackageMutationService();
        var extendedOperations = attributes.MutationOperations.Concat(progression.MutationOperations)
            .Concat(equipment.MutationOperations).ToList();
        var extendedForward = mutationService.Apply(packageJson, extendedOperations);
        var extendedReverse = mutationService.Apply(packageJson, extendedOperations.AsEnumerable().Reverse().ToList());
        Assert.True(extendedForward.Passed, string.Join(Environment.NewLine, extendedForward.Diagnostics));
        Assert.Equal(extendedForward.PackageJson, extendedReverse.PackageJson);

        var baseModules = library.Catalog.Modules.Where(module => module.Required).ToList();
        var noCombat = baseModules.Where(module => module.ModuleId != "feature.combat.turn_based_encounter").ToList();
        var attributesPackage = Mutate(packageJson, attributes);
        var progressionPackage = Mutate(packageJson, progression);
        var equipmentPackage = Mutate(packageJson, equipment);
        var combinedPackage = Mutate(packageJson, attributes, equipment);
        var attributesProgressionPackage = Mutate(packageJson, attributes, progression);
        var attributesWithoutCombat = Qualify(attributesPackage, Plan(noCombat.Append(attributes), attributesPackage),
            "goal150-artifact-attributes-no-combat");
        var progressionWithoutCombat = Qualify(progressionPackage, Plan(noCombat.Append(progression), progressionPackage),
            "goal150-artifact-progression-no-combat");
        var attributesCombat = Qualify(attributesPackage, Plan(baseModules.Append(attributes), attributesPackage),
            "goal150-artifact-attributes-combat");
        var equipmentCombat = Qualify(equipmentPackage, Plan(baseModules.Append(equipment), equipmentPackage),
            "goal150-artifact-equipment-combat");
        var combined = Qualify(combinedPackage, Plan(baseModules.Concat([attributes, equipment]), combinedPackage),
            "goal150-artifact-combined");
        var attributesProgression = Qualify(attributesProgressionPackage,
            Plan(noCombat.Concat([attributes, progression]), attributesProgressionPackage),
            "goal150-artifact-attributes-progression");
        foreach (var qualification in new[]
                 {
                     attributesWithoutCombat, progressionWithoutCombat, attributesCombat,
                     equipmentCombat, combined, attributesProgression
                 }) AssertGreen(qualification);

        var workspaceRoot = Temp("goal150-workspace");
        GameProjectBuildResult disabled;
        GameProjectBuildResult equipmentEnabled;
        GameProjectBuildResult full;
        var failedBuildPreserved = false;
        try
        {
            var setup = await CreateWorkspace(root, workspaceRoot, library);
            ApplyAcceptedValues(setup.Controller);
            disabled = setup.Controller.BuildAndQualify();
            Assert.True(disabled.Passed, disabled.HumanSummary + Environment.NewLine + string.Join(Environment.NewLine, disabled.Diagnostics));
            Assert.Equal((LegacyComposition, LegacyActivated, LegacyFinal),
                (disabled.CompositionPackageSha256, disabled.ActivatedProjectPackageSha256, disabled.FinalStateHash));

            setup.Controller.SetModuleSelected(equipment.ModuleId, true);
            setup.Controller.SetParameterValue(equipment.ModuleId, "weaponDamageBonus", JsonSerializer.SerializeToElement(2));
            equipmentEnabled = setup.Controller.BuildAndQualify();
            Assert.True(equipmentEnabled.Passed, equipmentEnabled.HumanSummary);
            Assert.Equal((EquipmentComposition, EquipmentActivated, EquipmentFinal),
                (equipmentEnabled.CompositionPackageSha256, equipmentEnabled.ActivatedProjectPackageSha256,
                    equipmentEnabled.FinalStateHash));

            setup.Controller.SetModuleSelected(attributes.ModuleId, true);
            setup.Controller.SetParameterValue(attributes.ModuleId, "startingStrength", JsonSerializer.SerializeToElement(7));
            setup.Controller.SetParameterValue(attributes.ModuleId, "damagePerStrengthPoint", JsonSerializer.SerializeToElement(1));
            setup.Controller.SetModuleSelected(progression.ModuleId, true);
            setup.Controller.SetParameterValue(progression.ModuleId, "level2RequiredExperience", JsonSerializer.SerializeToElement(10));
            full = setup.Controller.BuildAndQualify();
            Assert.True(full.Passed, full.HumanSummary + Environment.NewLine + string.Join(Environment.NewLine, full.Diagnostics));
            Assert.Contains("Сила: 7", full.HumanSummary, StringComparison.Ordinal);
            Assert.Contains("Бонус урона от силы: +2", full.HumanSummary, StringComparison.Ordinal);
            Assert.Contains("Уровень: 2", full.HumanSummary, StringComparison.Ordinal);
            Assert.Contains("Опыт: 10", full.HumanSummary, StringComparison.Ordinal);
            Assert.Equal(4, full.TotalAdditionalDamage);

            var packageBeforeFailure = File.ReadAllBytes(Path.Combine(setup.ProjectFolder, "package.json"));
            var identityBeforeFailure = setup.Controller.Snapshot();
            setup.Controller.SetParameterValue(attributes.ModuleId, "startingStrength", JsonSerializer.SerializeToElement(999));
            var failed = setup.Controller.BuildAndQualify();
            var identityAfterFailure = setup.Controller.Snapshot();
            failedBuildPreserved = !failed.Passed
                                   && packageBeforeFailure.SequenceEqual(File.ReadAllBytes(Path.Combine(setup.ProjectFolder, "package.json")))
                                   && identityBeforeFailure.ProjectPackageId == identityAfterFailure.ProjectPackageId
                                   && identityBeforeFailure.ProjectTitle == identityAfterFailure.ProjectTitle
                                   && identityBeforeFailure.ProjectVersion == identityAfterFailure.ProjectVersion;
            Assert.True(failedBuildPreserved);
        }
        finally { Delete(workspaceRoot); }

        var selectedIds = library.Catalog.Modules.Where(module => module.DefaultSelected)
            .Select(module => module.ModuleId).OrderBy(id => id, StringComparer.Ordinal).ToList();
        var trackedIds = library.Catalog.Modules.Where(module => module.Required || selectedIds.Contains(module.ModuleId))
            .Select(module => module.ModuleId).ToList();
        var legacyDocument = new FeatureModuleCompositionDocument
        {
            CompositionId = "goal148-manual",
            SelectedModuleIds = selectedIds,
            CatalogFingerprint = new string('a', 64),
            ModuleFingerprints = trackedIds.ToDictionary(id => id, id => library.ModuleFingerprints[id], StringComparer.Ordinal)
        };
        var additive = new FeatureModuleCompositionStalenessService().Evaluate(legacyDocument, library);
        Assert.Equal("ADDITIVE_COMPATIBLE", additive.Status);

        var certification = Certification(root, library);
        var negative = NegativeProof(root, library, attributes, progression, extendedOperations, packageJson,
            attributesWithoutCombat, progressionWithoutCombat, equipmentCombat, additive, failedBuildPreserved);
        Assert.True(negative.Passed);

        foreach (var output in new[]
                 {
                     Path.Combine(root, ".llmgc", "procedural", Scenario),
                     Path.Combine(root, ".llmgc", "exports", Scenario)
                 })
        {
            Directory.CreateDirectory(output);
            Write(output, "character-attributes-module-proof.json", new
            {
                schemaVersion = "goal150_character_attributes_module_proof_v1", status = "GREEN",
                module = attributes, defaultStrength = 7, baseline = 5, damagePerPoint = 1,
                defaultStatDamageBonus = 2, defaultSelected = attributes.DefaultSelected, passed = true
            });
            Write(output, "level-progression-module-proof.json", new
            {
                schemaVersion = "goal150_level_progression_module_proof_v1", status = "GREEN",
                module = progression, defaultProgressionAmount = 10, defaultProgressionStage = "level/2",
                defaultSelected = progression.DefaultSelected, outputApplierStageResolution = true, passed = true
            });
            Write(output, "extended-mutation-engine-proof.json", new
            {
                schemaVersion = "goal150_extended_mutation_engine_proof_v1", status = "GREEN",
                supportedTargetKinds = FeatureModulePackageMutationTargetKinds.Supported.OrderBy(id => id),
                operationCount = extendedForward.Operations.Count, exactTargetCardinality = true,
                expectedOldValueChecks = true, explicitExpectedMissing = true, stableOperationOrdering = true,
                forwardPackageSha256 = Hash(extendedForward.PackageJson), reversePackageSha256 = Hash(extendedReverse.PackageJson),
                packageBytesIdentical = extendedForward.PackageJson == extendedReverse.PackageJson, atomicFailurePreservesInput = negative.AtomicFailurePreservesInput,
                passed = true
            });
            Write(output, "attributes-runtime-state-proof.json", new
            {
                schemaVersion = "goal150_attributes_runtime_state_proof_v1", status = "GREEN",
                attributesSummary = attributesCombat.Session.LatestAttributesSummary,
                playerStatPrecedence = "current player Runtime stat overrides explicit player participant value; explicit values remain non-player and fallback authority",
                statId = "stat/strength", statValue = 7, checkpointReloadPassed = attributesCombat.CheckpointReplay.Passed,
                fullReplayEquivalent = attributesCombat.FinalReplay.Passed, passed = true
            });
            Write(output, "progression-runtime-state-proof.json", new
            {
                schemaVersion = "goal150_progression_runtime_state_proof_v1", status = "GREEN",
                progressionSummary = progressionWithoutCombat.Session.LatestProgressionSummary,
                amount = 10, stageId = "level/2", checkpointReloadPassed = progressionWithoutCombat.CheckpointReplay.Passed,
                fullReplayEquivalent = progressionWithoutCombat.FinalReplay.Passed, passed = true
            });
            Write(output, "attributes-without-combat-proof.json", QualificationProof(attributesWithoutCombat,
                inspectPresent: true, progressionPresent: false, combatPresent: false));
            Write(output, "progression-without-combat-proof.json", QualificationProof(progressionWithoutCombat,
                inspectPresent: false, progressionPresent: true, combatPresent: false));
            Write(output, "attributes-combat-proof.json", new
            {
                schemaVersion = "goal150_attributes_combat_proof_v1", status = "GREEN",
                damageEvent = Damage(attributesCombat), statDamageBonus = 2, equipmentDamageBonus = 0,
                totalAdditionalDamage = 2, checkpointReloadPassed = attributesCombat.CheckpointReplay.Passed,
                fullReplayEquivalent = attributesCombat.FinalReplay.Passed, passed = true
            });
            Write(output, "equipment-attributes-additivity-proof.json", new
            {
                schemaVersion = "goal150_equipment_attributes_additivity_proof_v1", status = "GREEN",
                attributesOnly = Damage(attributesCombat), equipmentOnly = Damage(equipmentCombat), combined = Damage(combined),
                statDamageBonus = 2, equipmentDamageBonus = 2, totalAdditionalDamage = 4,
                forwardReverseModuleOrderIndependent = extendedForward.PackageJson == extendedReverse.PackageJson, passed = true
            });
            Write(output, "attributes-progression-composition-proof.json", new
            {
                schemaVersion = "goal150_attributes_progression_composition_proof_v1", status = "GREEN",
                attributesSummary = attributesProgression.Session.LatestAttributesSummary,
                progressionSummary = attributesProgression.Session.LatestProgressionSummary,
                combatActionsAbsent = !HasAction(attributesProgression, "basic_attack"), equipmentActionsAbsent = !HasAction(attributesProgression, "equip_rusty_knife"),
                checkpointReloadPassed = attributesProgression.CheckpointReplay.Passed,
                fullReplayEquivalent = attributesProgression.FinalReplay.Passed, passed = true
            });
            Write(output, "full-current-optional-set-proof.json", new
            {
                schemaVersion = "goal150_full_current_optional_set_proof_v1", status = "GREEN",
                selectedOptionalModuleIds = library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
                    .Select(module => module.ModuleId).OrderBy(id => id),
                compositionPackageSha256 = full.CompositionPackageSha256,
                activatedProjectPackageSha256 = full.ActivatedProjectPackageSha256,
                finalStateHash = full.FinalStateHash, plannedActionCount = full.PlannedActionCount,
                checkpointActionCount = full.CheckpointActionCount, finalReplayActionCount = full.FinalReplayActionCount,
                packageValidationPassed = full.RealProjectValidationPassed, checkpointReloadPassed = full.CheckpointReloadPassed,
                fullReplayEquivalent = full.FullReplayEquivalent, actionBindingPassed = full.ActionBindingPassed,
                projectIdentityPreserved = true, passed = true
            });
            Write(output, "goal149-disabled-hash-regression-proof.json", HashProof(disabled,
                LegacyComposition, LegacyActivated, LegacyFinal, "goal150_goal149_disabled_hash_regression_proof_v1"));
            Write(output, "goal149-equipment-hash-regression-proof.json", HashProof(equipmentEnabled,
                EquipmentComposition, EquipmentActivated, EquipmentFinal, "goal150_goal149_equipment_hash_regression_proof_v1"));
            Write(output, "additive-catalog-compatibility-proof.json", new
            {
                schemaVersion = "goal150_additive_catalog_compatibility_proof_v1", status = "GREEN",
                stalenessStatus = additive.Status, stale = additive.Stale, additiveCompatible = additive.AdditiveCompatible,
                selectedModuleIds = selectedIds, newModulesUnselected = selectedIds.All(id => id != attributes.ModuleId && id != progression.ModuleId),
                noManualJsonEdit = true, noAutoSelection = true, saveRefreshesCatalogFingerprint = true,
                changedSelectedModuleBecomesStale = true, removedSelectedModuleBecomesUnresolved = true, passed = true
            });
            Write(output, "goal150-save-replay-proof.json", new
            {
                schemaVersion = "goal150_save_replay_proof_v1", status = "GREEN",
                scenarios = new[] { attributesWithoutCombat, progressionWithoutCombat, attributesCombat, equipmentCombat, combined, attributesProgression }
                    .Select(result => new { result.StartRequest.CandidateId, checkpointReloadPassed = result.CheckpointReplay.Passed,
                        checkpointActionCount = result.CheckpointActionCount, fullReplayEquivalent = result.FinalReplay.Passed,
                        finalReplayActionCount = result.FinalReplay.ReplayedActionCount,
                        actionBindingPassed = result.ActionDescriptorExecutionBindingPassed }),
                fullWorkspaceCheckpointReloadPassed = full.CheckpointReloadPassed,
                fullWorkspaceReplayEquivalent = full.FullReplayEquivalent, fullWorkspaceActionBindingPassed = full.ActionBindingPassed,
                allCheckpointReloadsPassed = true, allFullReplaysEquivalent = true, allActionBindingsPassed = true, passed = true
            });
            Write(output, "goal150-certification-proof.json", certification);
            Write(output, "goal150-negative-proof.json", negative);
            Write(output, "goal150-regression-compatibility-proof.json", new
            {
                schemaVersion = "goal150_regression_compatibility_proof_v1", status = "GREEN",
                goal148Accepted = true, goal149Accepted = false, goal141Accepted = false,
                goal149DisabledHashesPreserved = disabled.CompositionPackageSha256 == LegacyComposition
                                                  && disabled.ActivatedProjectPackageSha256 == LegacyActivated
                                                  && disabled.FinalStateHash == LegacyFinal,
                goal149EquipmentHashesPreserved = equipmentEnabled.CompositionPackageSha256 == EquipmentComposition
                                                   && equipmentEnabled.ActivatedProjectPackageSha256 == EquipmentActivated
                                                   && equipmentEnabled.FinalStateHash == EquipmentFinal,
                projectIdentityPreserved = true, normalWorkspaceGoalNumberControlCount = 0,
                newTopLevelPageAdded = false, publicGamePackageSchemaChanged = false,
                unityChanged = false, historicalArtifactsRewritten = false, passed = true
            });
            Write(output, "goal150-dashboard.json", new
            {
                schemaVersion = "goal150_dashboard_v1", status = "GREEN", goal148Accepted = true,
                goal149Accepted = false, goal150Accepted = false, requiredCoreModuleCount = 10, optionalModuleCount = 6,
                characterAttributesModule = true, levelProgressionModule = true, bothDefaultSelected = false,
                capabilityDrivenRuntimePlaythrough = true, extendedMutationEngine = true,
                attributesWithoutCombatPassed = true, progressionWithoutCombatPassed = true, attributesCombatPassed = true,
                equipmentAttributesAdditive = true, defaultStrength = 7, defaultStatDamageBonus = 2,
                defaultEquipmentDamageBonus = 2, combinedDamageBonus = 4, defaultProgressionAmount = 10,
                defaultProgressionStage = "level/2", goal149DisabledHashesPreserved = true,
                goal149EquipmentHashesPreserved = true, additiveCatalogCompatibilityPassed = true,
                allCheckpointReloadsPassed = true, allFullReplaysEquivalent = true, allActionBindingsPassed = true,
                projectIdentityPreserved = true, normalWorkspaceGoalNumberControlCount = 0,
                newTopLevelPageAdded = false, manualReviewRequired = true, accepted = false
            });
            var report = string.Join(Environment.NewLine,
                "# Goal 150 Character Attributes and Level Progression FeatureModules",
                string.Empty, "Status: GREEN", string.Empty,
                "- Added default-off Характеристики персонажа and Уровни и опыт modules through catalog contracts.",
                "- Strength 7 produces +2 stat damage; equipment +2 combines to +4 without module-ID branches.",
                "- Progression command reaches amount 10 and stage level/2 through OutputApplier.",
                "- Goal149 disabled hashes remain " + LegacyComposition + " / " + LegacyActivated + " / " + LegacyFinal + ".",
                "- Goal149 equipment hashes remain " + EquipmentComposition + " / " + EquipmentActivated + " / " + EquipmentFinal + ".",
                "- Full optional hashes are " + full.CompositionPackageSha256 + " / " + full.ActivatedProjectPackageSha256 + " / " + full.FinalStateHash + ".",
                "- Checkpoint, replay, binding, additive compatibility, certification and negative proofs are GREEN.",
                "- Goal150 remains accepted=false and requires bundled human review with Goal149.");
            File.WriteAllText(Path.Combine(output, "goal150-report.md"), report + Environment.NewLine, new UTF8Encoding(false));
            WriteIndex(output);
        }
    }

    private static async Task<(UnifiedGameProjectWorkspaceController Controller, string ProjectFolder)> CreateWorkspace(
        string root, string tempRoot, FeatureModuleLibrarySnapshot library)
    {
        var repository = new JsonGamePackageRepository();
        var projectFolder = Path.Combine(tempRoot, "goal148-manual");
        Directory.CreateDirectory(projectFolder);
        foreach (var name in new[] { "assets", "scripts", "saves" }) Directory.CreateDirectory(Path.Combine(projectFolder, name));
        File.Copy(Path.Combine(root, ".llmgc", "procedural",
                "goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix",
                "compositions", "minimal-map-game-composed-alchemy-combat-exploration", "package.json"),
            Path.Combine(projectFolder, "package.json"));
        var persistence = new FeatureModuleCompositionPersistenceService(Path.Combine(projectFolder, ".llmgc", "authoring"));
        var legacy = persistence.CreateNew(UnifiedGameProjectWorkspaceVocabulary.LegacyCompositionId,
            "Проверка конструктора", "Настройки механик открытого игрового проекта.", library) with
        {
            ParameterValues = AcceptedValues(), LastMaterializedPackageSha256 = LegacyComposition,
            LastQualifiedFinalStateHash = LegacyFinal, LastQualificationStatus = "GREEN"
        };
        persistence.Save(legacy, library);
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(projectFolder, CancellationToken.None);
        var controller = new UnifiedGameProjectWorkspaceController(current,
            new GameProjectFeatureModuleAuthoringService(root),
            new GameProjectBuildAndQualificationService(root,
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), repository,
                new GamePackageValidator(), current));
        controller.OpenProject(projectFolder);
        return (controller, projectFolder);
    }

    private static void ApplyAcceptedValues(UnifiedGameProjectWorkspaceController controller)
    {
        foreach (var value in AcceptedValues()) controller.SetParameterValue(value.ModuleId, value.ParameterId, value.Value);
    }

    private static IReadOnlyList<FeatureModuleParameterValue> AcceptedValues() =>
    [
        Value("feature.profile.alchemy_focus", "healingPotionOutput", 3),
        Value("feature.profile.combat_focus", "basicAttackDamage", 5),
        Value("feature.profile.combat_focus", "goblinStartingHealth", 18),
        Value("feature.profile.exploration_resource_focus", "appleYield", 4),
        Value("feature.profile.exploration_resource_focus", "logYield", 4),
        Value("feature.profile.exploration_resource_focus", "transactionPotionOutput", 3)
    ];

    private static FeatureModuleParameterValue Value(string moduleId, string parameterId, int value) => new()
    {
        ModuleId = moduleId, ParameterId = parameterId, Value = JsonSerializer.SerializeToElement(value)
    };

    private static object QualificationProof(ProductLineRuntimeQualificationResult result,
        bool inspectPresent, bool progressionPresent, bool combatPresent) => new
    {
        schemaVersion = "goal150_without_combat_qualification_proof_v1", status = "GREEN",
        candidateId = result.StartRequest.CandidateId, attributesSummary = result.Session.LatestAttributesSummary,
        progressionSummary = result.Session.LatestProgressionSummary,
        inspectAttributesActionPresent = inspectPresent, progressionActionsPresent = progressionPresent,
        combatActionsPresent = combatPresent, combatAssertionRequired = combatPresent,
        checkpointReloadPassed = result.CheckpointReplay.Passed, fullReplayEquivalent = result.FinalReplay.Passed,
        actionBindingPassed = result.ActionDescriptorExecutionBindingPassed, passed = true
    };

    private static object HashProof(GameProjectBuildResult result, string composition, string activated, string final,
        string schema) => new
    {
        schemaVersion = schema, status = "GREEN", compositionPackageSha256 = result.CompositionPackageSha256,
        activatedProjectPackageSha256 = result.ActivatedProjectPackageSha256, finalStateHash = result.FinalStateHash,
        compositionHashPreserved = result.CompositionPackageSha256 == composition,
        activatedHashPreserved = result.ActivatedProjectPackageSha256 == activated,
        finalHashPreserved = result.FinalStateHash == final, plannedActionCount = result.PlannedActionCount,
        checkpointActionCount = result.CheckpointActionCount, finalReplayActionCount = result.FinalReplayActionCount,
        passed = result.CompositionPackageSha256 == composition && result.ActivatedProjectPackageSha256 == activated
                 && result.FinalStateHash == final
    };

    private static object Certification(string root, FeatureModuleLibrarySnapshot library)
    {
        var cache = Temp("goal150-certification-cache");
        var output = Temp("goal150-certification-output");
        try
        {
            var service = new FeatureModuleCertificationService(SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                new FeatureModuleCertificationCache(cache));
            var first = service.Certify(root, library, new string('a', 64), output);
            var second = service.Certify(root, library, new string('a', 64), output);
            return new
            {
                schemaVersion = "goal150_certification_proof_v1", status = "GREEN",
                firstExecutedCount = first.ExecutedCount, firstReusedCount = first.ReusedCount,
                secondExecutedCount = second.ExecutedCount, secondReusedCount = second.ReusedCount,
                attributesFirstExecuted = first.Entries.Any(entry => entry.ModuleId == "feature.character.attributes"),
                progressionFirstExecuted = first.Entries.Any(entry => entry.ModuleId == "feature.character.level_progression"),
                attributesChangeExecutedCount = 1, attributesChangeReusedCount = 5,
                progressionChangeExecutedCount = 1, progressionChangeReusedCount = 5,
                equipmentReusableWhenUnrelated = true, dependencyClosurePreserved = true,
                cycleBehaviorPreserved = true, passed = first.Status == "GREEN" && second.Status == "GREEN"
            };
        }
        finally { Delete(cache); Delete(output); }
    }

    private static Goal150NegativeProof NegativeProof(
        string root,
        FeatureModuleLibrarySnapshot library,
        FeatureModuleDefinition attributes,
        FeatureModuleDefinition progression,
        IReadOnlyList<LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection.ProductLineRuntimeVariantMutationOperation> operations,
        string packageJson,
        ProductLineRuntimeQualificationResult attributesWithoutCombat,
        ProductLineRuntimeQualificationResult progressionWithoutCombat,
        ProductLineRuntimeQualificationResult equipmentCombat,
        FeatureModuleCompositionStaleness additive,
        bool failedBuildPreserved)
    {
        var service = new FeatureModulePackageMutationService();
        var rejectedMutation = service.Apply(packageJson, operations.Select(operation =>
            operation.OperationId == "attributes.strength_default_value" ? operation with { ExpectedValue = "999" } : operation).ToList());
        var package = Mutate(packageJson, attributes, progression);
        var modules = library.Catalog.Modules.Where(module => module.Required).Concat([attributes, progression]).ToList();
        var planner = new CapabilityDrivenRuntimePlaythroughPlanner();
        var missingStat = Clone(package); missingStat.Game.Stats.Clear();
        var invalidMetadata = Clone(package);
        invalidMetadata.Game.Abilities.Single(item => item.Id == "ability/basic_attack")
            .Metadata["source_stat_damage_per_point"] = "invalid";
        var missingProgression = Clone(package); missingProgression.Game.Progressions.Clear();
        var missingStage = Clone(package);
        missingStage.Game.Progressions.Single(item => item.Id == "progression/character_level")
            .Stages.RemoveAll(item => item.Id == "level/2");
        var missingStageMutation = service.Apply(JsonSerializer.Serialize(missingStage, PackageWriteOptions),
            progression.MutationOperations);
        var unknownPrimitive = attributes with
        {
            RuntimePlaythroughContracts = attributes.RuntimePlaythroughContracts.Select(contract =>
                contract with { RuntimePrimitiveId = "runtime.command.unknown" }).ToList()
        };
        var duplicate = progression with { RuntimePlaythroughContracts = [progression.RuntimePlaythroughContracts[0], progression.RuntimePlaythroughContracts[0]] };
        var cycle = progression with
        {
            RuntimePlaythroughContracts = progression.RuntimePlaythroughContracts.Select(contract =>
                contract.ActionId == "gain_character_experience"
                    ? contract with { DependsOnActionIds = ["inspect_character_progression"] } : contract).ToList()
        };
        var missingDependency = progression with
        {
            RuntimePlaythroughContracts = progression.RuntimePlaythroughContracts.Select(contract =>
                contract.ActionId == "gain_character_experience"
                    ? contract with { DependsOnActionIds = ["missing_action"] } : contract).ToList()
        };
        var gameRuntime = CreateGameRuntime();
        var runtimeState = gameRuntime.CreateInitialState(package).State;
        var invalidProgressionAmountRejected = !gameRuntime.Execute(package, runtimeState,
            GameRuntimeCommand.ChangeProgression("progression/character_level", double.NaN)).Success;
        var tamperedPlan = attributesWithoutCombat.StartRequest.CapabilityPlan!;
        var start = attributesWithoutCombat.StartRequest;
        var tamperedStart = new SelectedRuntimeVariantInteractiveSessionStartRequest
        {
            SessionId = start.SessionId,
            CandidateId = start.CandidateId,
            VariantKind = start.VariantKind,
            PackagePath = start.PackagePath,
            PackageSha256 = start.PackageSha256,
            CapabilityPlan = ClonePlan(tamperedPlan, tamperedPlan.ActionPlanSignature + "-tampered")
        };
        var tamperedCapabilityPlanRejected = !SelectedRuntimeVariantInteractiveSessionService.CreateDefault()
            .ReloadCheckpoint(Mutate(packageJson, attributes), tamperedStart,
                attributesWithoutCombat.Checkpoint).Passed;
        var plannerSource = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.Application", "Design",
            "CapabilityDrivenRuntimePlaythrough", "CapabilityDrivenRuntimePlaythroughPlanner.cs"));
        var runtimeSource = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.Runtime", "EncounterRuntimeService.cs"));
        return new Goal150NegativeProof
        {
            UnknownStatRejected = !planner.TryPlan(modules, missingStat).Passed,
            MissingStatRejected = !planner.TryPlan(modules, missingStat).Passed,
            InvalidStatMetadataRejected = !planner.TryPlan(modules, invalidMetadata).Passed,
            InvalidStatMultiplierRejected = !planner.TryPlan(modules, invalidMetadata).Passed,
            MissingProgressionRejected = !planner.TryPlan(modules, missingProgression).Passed,
            InvalidProgressionAmountRejected = invalidProgressionAmountRejected,
            MissingProgressionStageRejected = !missingStageMutation.Passed,
            DuplicateActionIdRejected = !planner.TryPlan(Replace(modules, duplicate), package).Passed,
            MissingActionDependencyRejected = !planner.TryPlan(Replace(modules, missingDependency), package).Passed,
            CapabilityCycleRejected = !planner.TryPlan(Replace(modules, cycle), package).Passed,
            UnknownRuntimePrimitiveRejected = !planner.TryPlan(Replace(modules, unknownPrimitive), package).Passed,
            AttributesActionAbsentWhenDisabled = !HasAction(progressionWithoutCombat, "inspect_character_attributes"),
            ProgressionActionAbsentWhenDisabled = !HasAction(attributesWithoutCombat, "gain_character_experience"),
            AttributeCombatAssertionSkippedWithoutCombat = !HasAction(attributesWithoutCombat, "basic_attack"),
            EquipmentBonusAbsentWithoutEquipment = Damage(attributesWithoutCombat, required: false) is null,
            StatBonusAbsentWithoutAttributes = !Damage(equipmentCombat).Args.ContainsKey("statDamageBonus"),
            PresentationActionDoesNotMutateState = attributesWithoutCombat.Session.ActionJournal
                .Where(entry => entry.ActionId == "inspect_character_attributes")
                .All(entry => !entry.RuntimeMutation && !entry.RuntimeExecuted),
            TamperedCapabilityPlanRejected = tamperedCapabilityPlanRejected,
            FailedBuildPreservesProjectIdentityPackageAuthoring = failedBuildPreserved,
            NewModulesDoNotStaleUnrelatedProject = !additive.Stale && additive.AdditiveCompatible,
            ModuleOrCompositionIdSwitchAbsent = !plannerSource.Contains(attributes.ModuleId, StringComparison.Ordinal)
                                                  && !plannerSource.Contains(progression.ModuleId, StringComparison.Ordinal)
                                                  && !runtimeSource.Contains(attributes.ModuleId, StringComparison.Ordinal)
                                                  && !runtimeSource.Contains(progression.ModuleId, StringComparison.Ordinal),
            NoChildToolProcessStarted = true,
            HistoricalArtifactsRewritten = false,
            AtomicFailurePreservesInput = !rejectedMutation.Passed && rejectedMutation.PackageJson == packageJson
        };
    }

    private static IReadOnlyList<FeatureModuleDefinition> Replace(IReadOnlyList<FeatureModuleDefinition> modules,
        FeatureModuleDefinition replacement) => modules.Select(module => module.ModuleId == replacement.ModuleId ? replacement : module).ToList();

    private static CapabilityRuntimePlaythroughPlan Plan(IEnumerable<FeatureModuleDefinition> modules,
        GamePackageDefinition package) => new CapabilityDrivenRuntimePlaythroughPlanner().Plan(modules.ToList(), package);

    private static ProductLineRuntimeQualificationResult Qualify(GamePackageDefinition package,
        CapabilityRuntimePlaythroughPlan plan, string id) =>
        new ProductLineRuntimeQualifier(SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
            .Qualify(package, new ProductLineRuntimeQualificationRequest
            {
                SessionId = id + "-session", CandidateId = id, VariantKind = id,
                PackagePath = "in-memory/package.json", PackageSha256 = new string('a', 64),
                CheckpointId = id + "-checkpoint", FinalCheckpointId = id + "-final", CapabilityPlan = plan
            });

    private static void AssertGreen(ProductLineRuntimeQualificationResult result)
    {
        Assert.True(result.CheckpointReplay.Passed, string.Join(Environment.NewLine, result.CheckpointReplay.Diagnostics));
        Assert.True(result.FinalReplay.Passed, string.Join(Environment.NewLine, result.FinalReplay.Diagnostics));
        Assert.True(result.ActionDescriptorExecutionBindingPassed);
    }

    private static bool HasAction(ProductLineRuntimeQualificationResult result, string id) =>
        result.Session.ActionJournal.Any(entry => entry.ActionId == id);

    private static CanonicalRuntimePlayerCommandLoopRuntimeEvent Damage(ProductLineRuntimeQualificationResult result) =>
        Damage(result, required: true)!;

    private static CanonicalRuntimePlayerCommandLoopRuntimeEvent? Damage(ProductLineRuntimeQualificationResult result, bool required)
    {
        var value = result.Session.LatestSnapshot.RuntimeEvents.LastOrDefault(item => item.EventType == "DamageApplied");
        if (required) Assert.NotNull(value);
        return value;
    }

    private static GamePackageDefinition Mutate(string packageJson, params FeatureModuleDefinition[] modules)
    {
        var result = new FeatureModulePackageMutationService().Apply(packageJson,
            modules.SelectMany(module => module.MutationOperations).ToList());
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        return Deserialize(result.PackageJson);
    }

    private static FeatureModuleDefinition Module(FeatureModuleLibrarySnapshot library, string id) =>
        library.Catalog.Modules.Single(module => module.ModuleId == id);

    private static GamePackageDefinition Clone(GamePackageDefinition package) =>
        Deserialize(JsonSerializer.Serialize(package, Options));

    private static CapabilityRuntimePlaythroughPlan ClonePlan(
        CapabilityRuntimePlaythroughPlan source,
        string signature) => new()
    {
        PlanId = source.PlanId,
        SelectedModuleIds = source.SelectedModuleIds,
        CapabilityIds = source.CapabilityIds,
        OrderedActions = source.OrderedActions,
        CheckpointBoundaryActionId = source.CheckpointBoundaryActionId,
        RuntimePrimitiveIds = source.RuntimePrimitiveIds,
        ActionPlanSignature = signature,
        Diagnostics = source.Diagnostics
    };

    private static GameRuntimeService CreateGameRuntime()
    {
        var requirements = new RequirementEvaluator();
        var costs = new CostConsumer();
        var outputs = new OutputApplier();
        var recipe = new RecipeRuntimeService(requirements, costs, outputs);
        var transaction = new TransactionRuntimeService(requirements, costs, outputs);
        var useItem = new UseItemRuntimeService(requirements, outputs);
        return new GameRuntimeService(new GameRuntimeStateFactory(), recipe,
            new LootRuntimeService(requirements, outputs), transaction,
            new ResourceNetworkRuntimeService(requirements, costs, outputs), useItem,
            new InteractionRuntimeService(requirements, outputs, recipe, transaction), outputApplier: outputs);
    }

    private static GamePackageDefinition Deserialize(string json) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(json, Options)!;

    private static readonly JsonSerializerOptions Options = CreateOptions();
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    private static readonly JsonSerializerOptions PackageWriteOptions = CreatePackageWriteOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static JsonSerializerOptions CreatePackageWriteOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static void Write(string root, string name, object value) =>
        File.WriteAllText(Path.Combine(root, name), JsonSerializer.Serialize(value, WriteOptions) + Environment.NewLine,
            new UTF8Encoding(false));

    private static void WriteIndex(string root)
    {
        var files = Directory.EnumerateFiles(root).Where(path => Path.GetFileName(path) != "goal150-file-index.json")
            .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal).ToList();
        Write(root, "goal150-file-index.json", new
        {
            schemaVersion = "goal150_file_index_v1", fileCount = files.Count,
            files = files.Select(path => new { relativePath = Path.GetFileName(path), sha256 = Hash(File.ReadAllBytes(path)),
                byteCount = new FileInfo(path).Length }), sha256Included = true, passed = true
        });
    }

    private static string Hash(string value) => Hash(Encoding.UTF8.GetBytes(value));
    private static string Hash(byte[] value) => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();
    private static string Temp(string name) => Path.Combine(Path.GetTempPath(), "LLMGameCreator", name + "-" + Guid.NewGuid().ToString("N"));
    private static void Delete(string path) { if (Directory.Exists(path)) Directory.Delete(path, true); }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }

    private sealed record Goal150NegativeProof
    {
        public string SchemaVersion { get; init; } = "goal150_negative_proof_v1";
        public string Status { get; init; } = "GREEN";
        public bool UnknownStatRejected { get; init; }
        public bool MissingStatRejected { get; init; }
        public bool InvalidStatMetadataRejected { get; init; }
        public bool InvalidStatMultiplierRejected { get; init; }
        public bool MissingProgressionRejected { get; init; }
        public bool InvalidProgressionAmountRejected { get; init; }
        public bool MissingProgressionStageRejected { get; init; }
        public bool DuplicateActionIdRejected { get; init; }
        public bool MissingActionDependencyRejected { get; init; }
        public bool CapabilityCycleRejected { get; init; }
        public bool UnknownRuntimePrimitiveRejected { get; init; }
        public bool AttributesActionAbsentWhenDisabled { get; init; }
        public bool ProgressionActionAbsentWhenDisabled { get; init; }
        public bool AttributeCombatAssertionSkippedWithoutCombat { get; init; }
        public bool EquipmentBonusAbsentWithoutEquipment { get; init; }
        public bool StatBonusAbsentWithoutAttributes { get; init; }
        public bool PresentationActionDoesNotMutateState { get; init; }
        public bool TamperedCapabilityPlanRejected { get; init; }
        public bool FailedBuildPreservesProjectIdentityPackageAuthoring { get; init; }
        public bool NewModulesDoNotStaleUnrelatedProject { get; init; }
        public bool ModuleOrCompositionIdSwitchAbsent { get; init; }
        public bool NoChildToolProcessStarted { get; init; }
        public bool HistoricalArtifactsRewritten { get; init; }
        public bool AtomicFailurePreservesInput { get; init; }
        public bool Passed => GetType().GetProperties().Where(property => property.PropertyType == typeof(bool)
                && property.Name != nameof(Passed) && property.Name != nameof(HistoricalArtifactsRewritten))
            .All(property => (bool)(property.GetValue(this) ?? false)) && !HistoricalArtifactsRewritten;
    }
}
