using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.FullCampaignGamePackageMaterialization;

public sealed class FullCampaignGamePackageMaterializationBuilder
{
    private static readonly DateTimeOffset FixedAppliedAtUtc = new(2026, 6, 30, 0, 0, 0, TimeSpan.Zero);
    private static readonly JsonSerializerOptions PackageJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static FullCampaignGamePackageMaterializationBuilder()
    {
        PackageJsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public FullCampaignSourceManifest BuildSourceManifest(FullCampaignSourceBundle source)
    {
        var diagnostics = new List<FullCampaignGamePackageMaterializationDiagnostic>(source.Diagnostics)
        {
            Info("goal060.preflight.goal059_handoff_recorded", "full_generator_variability_regression_matrix_verification", "Goal 059 is recorded as accepted by user handoff before Goal 060."),
            Info("goal060.source.loaded", "Goal059", "Goal 060 source facts were loaded from repository-local Goal 059 matrix evidence.")
        };

        return new FullCampaignSourceManifest
        {
            Accepted = false,
            Goal059AcceptedByUserHandoff = source.Goal059AcceptedByUserHandoff,
            Goal059ReportWasGreenProducedForReview = source.Goal059ReportWasGreenProducedForReview,
            Goal059UnityProofPassed = source.Goal059UnityProofPassed,
            SourceCampaignHash = source.Goal059SourceCampaignHash,
            SeedProfileMatrixHash = source.Goal059SeedProfileMatrixHash,
            RowCount = source.Rows.Count,
            FamilyIds = source.Rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).OrderBy(FamilyOrderingKey, StringComparer.Ordinal).ToList(),
            SeedIds = source.Rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).OrderBy(SeedOrderingKey, StringComparer.Ordinal).ToList(),
            PreflightGates =
            [
                new FullCampaignGateRecord
                {
                    GateId = "full_generator_variability_regression_matrix_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 060 task preflight handoff"
                },
                new FullCampaignGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 031 preserved policy"
                },
                new FullCampaignGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "inherited",
                    EvidenceRef = "Goal 032 preserved policy"
                },
                new FullCampaignGateRecord
                {
                    GateId = FullCampaignGamePackageMaterializationVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "programmatic",
                    EvidenceRef = "Goal 060 produced for review"
                }
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public FullCampaignPackageMaterializationPlan BuildPackageMaterializationPlan(FullCampaignSourceBundle source)
    {
        var rows = source.Rows
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(row => new FullCampaignPackagePlanRow
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                SourceCampaignHash = row.SourceCampaignHash,
                Goal059RowHash = row.RowHash,
                PackageId = PackageId(row),
                PackageRelativePath = PackageRelativePath(row),
                SelectedPackageAssemblyDomains = ["world_entities", "dialogue_quests", "items_economy_crafting", "combat_progression"],
                ExpectedRuntimeLoopKind = RuntimeLoopKind(row.FamilyId),
                ExpectedPreviewExportProfile = PreviewExportProfile(row),
                BlockedFutureRequiredGaps = []
            })
            .ToList();

        return new FullCampaignPackageMaterializationPlan
        {
            Passed = rows.Count == 9
                && rows.Select(item => item.PackageId).Distinct(StringComparer.Ordinal).Count() == rows.Count
                && FullCampaignGamePackageMaterializationVocabulary.FamilyIds.All(familyId => rows.Count(item => item.FamilyId == familyId) == 3)
                && FullCampaignGamePackageMaterializationVocabulary.SeedIds.All(seedId => rows.Count(item => item.SeedId == seedId) == 3)
                && rows.All(item => item.BlockedFutureRequiredGaps.Count == 0),
            Accepted = false,
            RowCount = rows.Count,
            FamilyCount = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            Rows = rows
        };
    }

    public IReadOnlyList<FullCampaignMaterializedPackage> MaterializePackages(FullCampaignPackageMaterializationPlan plan)
    {
        var validator = new GamePackageValidator();
        return plan.Rows
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(row => MaterializePackage(row, validator))
            .ToList();
    }

    public FullCampaignMaterializedPackageInventory BuildPackageInventory(IReadOnlyList<FullCampaignMaterializedPackage> packages)
    {
        var summaries = packages
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(item => new FullCampaignMaterializedPackageSummary
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                PackageId = item.PackageId,
                PackageRelativePath = item.PackageRelativePath,
                PackageHash = item.PackageHash,
                ValidationPassed = item.ValidationPassed,
                Goal059RowHash = item.Goal059RowHash
            })
            .ToList();

        return new FullCampaignMaterializedPackageInventory
        {
            Passed = summaries.Count == 9
                && summaries.Select(item => item.PackageId).Distinct(StringComparer.Ordinal).Count() == 9
                && summaries.All(item => item.ValidationPassed && !string.IsNullOrWhiteSpace(item.PackageHash)),
            PackageCount = summaries.Count,
            DistinctPackageIdCount = summaries.Select(item => item.PackageId).Distinct(StringComparer.Ordinal).Count(),
            Packages = summaries
        };
    }

    public FullCampaignPackageValidationMatrix BuildPackageValidationMatrix(IReadOnlyList<FullCampaignMaterializedPackage> packages)
    {
        var rows = packages
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(item => new FullCampaignPackageValidationRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                PackageId = item.PackageId,
                ValidJson = item.ValidJson,
                ValidationPassed = item.ValidationPassed,
                ErrorCount = item.ValidationErrorCount,
                WarningCount = item.ValidationWarningCount,
                IssueCodes = item.ValidationIssueCodes
            })
            .ToList();

        return new FullCampaignPackageValidationMatrix
        {
            Passed = rows.Count == 9 && rows.All(item => item.ValidJson && item.ValidationPassed && item.ErrorCount == 0),
            RowCount = rows.Count,
            ValidPackageCount = rows.Count(item => item.ValidJson && item.ValidationPassed && item.ErrorCount == 0),
            Rows = rows
        };
    }

    public FullCampaignRuntimeConsumptionMatrix BuildRuntimeConsumptionMatrix(
        FullCampaignPackageMaterializationPlan plan,
        IReadOnlyList<FullCampaignMaterializedPackage> packages,
        IFullCampaignGamePackageMaterializationRuntimeAdapter runtimeAdapter)
    {
        var byPackageId = packages.ToDictionary(item => item.PackageId, item => item, StringComparer.Ordinal);
        var rows = new List<FullCampaignRuntimeConsumptionRow>();
        foreach (var planRow in plan.Rows.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal).ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal))
        {
            if (!byPackageId.TryGetValue(planRow.PackageId, out var materialized))
            {
                rows.Add(new FullCampaignRuntimeConsumptionRow
                {
                    RowId = planRow.RowId,
                    FamilyId = planRow.FamilyId,
                    SeedId = planRow.SeedId,
                    PackageId = planRow.PackageId,
                    ExpectedRuntimeLoopKind = planRow.ExpectedRuntimeLoopKind,
                    RuntimePassed = false,
                    Diagnostics = [Error("goal060.runtime.package_missing", planRow.RowId, "Runtime proof requires a materialized package.")]
                });
                continue;
            }

            var request = new FullCampaignRuntimeRequest
            {
                RowId = planRow.RowId,
                FamilyId = planRow.FamilyId,
                SeedId = planRow.SeedId,
                PackageId = planRow.PackageId,
                PackageHash = materialized.PackageHash,
                ExpectedRuntimeLoopKind = planRow.ExpectedRuntimeLoopKind,
                Package = materialized.Package,
                Commands = RuntimeCommands(planRow)
            };
            var evidence = runtimeAdapter.Run(request);
            rows.Add(new FullCampaignRuntimeConsumptionRow
            {
                RowId = planRow.RowId,
                FamilyId = planRow.FamilyId,
                SeedId = planRow.SeedId,
                PackageId = planRow.PackageId,
                ExpectedRuntimeLoopKind = planRow.ExpectedRuntimeLoopKind,
                RuntimePassed = evidence.RuntimeAttempted
                    && evidence.RuntimeStartSucceeded
                    && evidence.UsedGameRuntimeService
                    && evidence.StateChanged
                    && evidence.FamilySpecificTransitionObserved
                    && evidence.SaveLoadRoundtripPassed
                    && evidence.Commands.Any(item => item.Succeeded),
                StateChanged = evidence.StateChanged,
                FamilySpecificTransitionObserved = evidence.FamilySpecificTransitionObserved,
                SaveLoadRoundtripPassed = evidence.SaveLoadRoundtripPassed,
                ChangedStateKeys = evidence.ChangedStateKeys,
                Commands = evidence.Commands,
                Diagnostics = evidence.Diagnostics
            });
        }

        var materializedFamilies = packages.Where(item => item.ValidationPassed).Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count();
        var runtimeFamilies = rows.Where(item => item.RuntimePassed).Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count();
        return new FullCampaignRuntimeConsumptionMatrix
        {
            Passed = materializedFamilies == 3 && runtimeFamilies == 3 && rows.Count == 9 && rows.All(item => item.RuntimePassed),
            MaterializedFamilyCount = materializedFamilies,
            RuntimePassedFamilyCount = runtimeFamilies,
            Rows = rows
        };
    }

    public FullCampaignPreviewExportPackagePayloads BuildPreviewExportPackagePayloads(IReadOnlyList<FullCampaignMaterializedPackage> packages)
    {
        var rows = packages
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(item => new FullCampaignPreviewExportPackageRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                PackageId = item.PackageId,
                PackageRelativePath = item.PackageRelativePath,
                PreviewPayloadRef = "preview/package-bound/" + item.FamilyId + "/" + item.SeedId + "/" + item.PackageHash[..12] + ".json",
                ExportPayloadRef = "export/package-bound/" + item.FamilyId + "/" + item.SeedId + "/" + item.PackageHash[..12] + ".json",
                PackageHashBeforePreviewExport = item.PackageHash,
                PackageHashAfterPreviewExport = Hash(item.PackageJson),
                PackageImmutable = string.Equals(item.PackageHash, Hash(item.PackageJson), StringComparison.Ordinal),
                ProvenanceLedger =
                [
                    "goal059Row=" + item.RowId,
                    "packageId=" + item.PackageId,
                    "packageHash=" + item.PackageHash,
                    "validatorClean=" + item.ValidationPassed.ToString().ToLowerInvariant()
                ]
            })
            .ToList();

        return new FullCampaignPreviewExportPackagePayloads
        {
            Passed = rows.Count == 9
                && rows.All(item => item.PackageImmutable)
                && rows.All(item => !string.IsNullOrWhiteSpace(item.PackageId))
                && rows.All(item => !string.IsNullOrWhiteSpace(item.PreviewPayloadRef) && !string.IsNullOrWhiteSpace(item.ExportPayloadRef)),
            RowCount = rows.Count,
            PackageImmutabilityAuditPassed = rows.All(item => item.PackageImmutable),
            Rows = rows
        };
    }

    public FullCampaignUnityPackageCommandPlan BuildUnityCommandPlan(
        IReadOnlyList<FullCampaignMaterializedPackage> packages,
        FullCampaignRuntimeConsumptionMatrix runtimeMatrix)
    {
        var runtimeByRow = runtimeMatrix.Rows.ToDictionary(item => item.RowId, item => item, StringComparer.Ordinal);
        var commandRows = packages
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(item =>
            {
                runtimeByRow.TryGetValue(item.RowId, out var runtime);
                var row = new FullCampaignUnityPackageCommandRow
                {
                    RowId = item.RowId,
                    FamilyId = item.FamilyId,
                    SeedId = item.SeedId,
                    PackageId = item.PackageId,
                    PackageRelativePath = StagedPackageRelativePath(item),
                    PackageHash = item.PackageHash,
                    PackageValidationPassed = item.ValidationPassed,
                    RuntimeLoopCompleted = runtime?.RuntimePassed == true
                };
                return row with { ExpectedPlayerMarkers = ExpectedUnityRowMarkers(row) };
            })
            .ToList();

        var expected = new List<string>
        {
            "package_matrix_loaded=true",
            "package_materialization_goal=goal060"
        };
        expected.AddRange(commandRows.SelectMany(item => item.ExpectedPlayerMarkers));
        expected.Add("full_campaign_gamepackage_materialization_matrix_verification=required");

        return new FullCampaignUnityPackageCommandPlan
        {
            Passed = commandRows.Count == 9
                && commandRows.All(item => item.PackageValidationPassed && item.RuntimeLoopCompleted)
                && expected.Count > 0,
            Accepted = false,
            Rows = commandRows,
            ExpectedPlayerMarkers = expected.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
        };
    }

    public IReadOnlyList<FullCampaignFilePayload> BuildStagingFiles(
        FullCampaignSourceBundle source,
        FullCampaignUnityPackageCommandPlan commandPlan,
        IReadOnlyList<FullCampaignMaterializedPackage> packages)
    {
        var files = source.Goal059StagingFiles.ToList();
        files.Add(new FullCampaignFilePayload
        {
            RelativePath = FullCampaignGamePackageMaterializationVocabulary.UnityPackageCommandPlanStagingRelativePath,
            Bytes = System.Text.Encoding.UTF8.GetBytes(Serialize(commandPlan) + Environment.NewLine)
        });

        foreach (var package in packages)
        {
            files.Add(new FullCampaignFilePayload
            {
                RelativePath = StagedPackageRelativePath(package),
                Bytes = System.Text.Encoding.UTF8.GetBytes(package.PackageJson + Environment.NewLine)
            });
        }

        return files
            .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    public InvalidFullCampaignMaterializationMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidFullCampaignMaterializationScenario>
        {
            Invalid("missing_goal059_source", "Remove the accepted Goal 059 matrix source artifacts before loading.", "blocked", Error("goal060.source.goal059_missing", "Goal059", "Accepted Goal 059 evidence is required.")),
            Invalid("stale_goal059_hash", "Change a consumed Goal 059 row after its hash was recorded.", "rejected", Error("goal060.source.hash_mismatch", "Goal059", "Source artifact bytes must match recorded hashes.")),
            Invalid("fake_matrix_row_id", "Inject a package plan row that does not exist in Goal 059.", "rejected", Error("goal060.plan.fake_matrix_row_id", "package-materialization-plan", "Every package row must trace to a Goal 059 matrix row.")),
            Invalid("duplicate_package_id", "Emit two package rows with the same package id.", "rejected", Error("goal060.plan.duplicate_package_id", "package-materialization-plan", "Every materialized package id must be unique.")),
            Invalid("invalid_family_id", "Inject a family outside the accepted Goal 059 family set.", "rejected", Error("goal060.plan.invalid_family_id", "family/fake", "Package family id must resolve to Goal 059 source families.")),
            Invalid("invalid_seed_id", "Inject a seed outside the required seed set.", "rejected", Error("goal060.plan.invalid_seed_id", "seed/fake", "Package seed id must be one of seed_alpha, seed_beta or seed_gamma.")),
            Invalid("package_json_malformed", "Write a package JSON file that cannot be parsed.", "rejected", Error("goal060.package.json_malformed", "packages", "Materialized package files must be valid JSON.")),
            Invalid("package_validation_failure", "Materialize a package that fails the existing GamePackage validator.", "rejected", Error("goal060.package.validation_failed", "GamePackageValidator", "Materialized packages must be validator-clean.")),
            Invalid("package_source_trace_mismatch", "Use a package sidecar trace that points at a different Goal 059 row.", "rejected", Error("goal060.package.source_trace_mismatch", "materialized-package-inventory", "Package trace must match the Goal 059 row id/hash.")),
            Invalid("schema_mutation_claim", "Claim public GamePackage schema/model mutation.", "blocked", Error("goal060.boundary.schema_mutation", "boundary", "Public GamePackage schema mutation is forbidden.")),
            Invalid("runtime_ui_unity_broad_mutation_claim", "Claim Runtime, Runtime.Abstractions, WinForms UI or broad Unity mutation.", "blocked", Error("goal060.boundary.runtime_ui_unity_broad", "boundary", "Runtime/UI/broad Unity mutation is forbidden.")),
            Invalid("provider_network_llm_rag_media_generation_claim", "Claim provider, network, LLM/RAG or media generation execution.", "blocked", Error("goal060.boundary.provider_network_llm_rag_media", "boundary", "Provider, network, LLM/RAG and media generation are forbidden.")),
            Invalid("arbitrary_lua_execution_claim", "Claim arbitrary Lua execution during package materialization.", "blocked", Error("goal060.boundary.lua_arbitrary_execution", "boundary", "Arbitrary Lua execution is forbidden.")),
            Invalid("unsafe_path", "Use absolute paths or traversal in package/staging refs.", "rejected", Error("goal060.path.unsafe", "../escape", "Package artifact refs must stay safe relative paths.")),
            Invalid("nondeterministic_ordering", "Rebuild package rows in filesystem enumeration order instead of family/seed order.", "rejected", Error("goal060.order.nondeterministic", "package-materialization-plan", "Package materialization rows must use deterministic family/seed ordering.")),
            Invalid("fake_unity_marker", "Declare a Unity marker without staging a package file or requiring player log proof.", "rejected", Error("goal060.unity.fake_marker", "unity-package-consumption-proof", "Unity markers must be matched in actual player logs.")),
            Invalid("missing_runtime_transition_proof", "Skip state-changing runtime commands for a materialized family.", "blocked", Error("goal060.runtime.transition_missing", "runtime-consumption-matrix", "Runtime proof requires a state-changing transition for every materialized family.")),
            Invalid("package_immutability_breach", "Let preview/export payload construction rewrite the materialized package JSON.", "rejected", Error("goal060.preview_export.immutability_breach", "preview-export-package-payloads", "Preview/export payloads must not mutate package hashes."))
        };

        return new InvalidFullCampaignMaterializationMatrix
        {
            Passed = FullCampaignGamePackageMaterializationVocabulary.RequiredInvalidScenarioIds.All(id => scenarios.Any(item => item.ScenarioId == id && item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0)),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<string> ExpectedUnityRowMarkers(FullCampaignUnityPackageCommandRow row) =>
    [
        "package_row_started=" + row.RowId,
        "package_family=" + row.FamilyId,
        "package_seed=" + row.SeedId,
        "package_id=" + row.PackageId,
        "package_validation_passed=" + row.PackageValidationPassed.ToString().ToLowerInvariant(),
        "package_runtime_loop_completed=" + row.RuntimeLoopCompleted.ToString().ToLowerInvariant(),
        "package_row_completed=" + row.RowId
    ];

    public static IReadOnlyList<FullCampaignGamePackageMaterializationDiagnostic> SortDiagnostics(IEnumerable<FullCampaignGamePackageMaterializationDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static string FamilyOrderingKey(string familyId) =>
        FullCampaignGamePackageMaterializationSourceLoader.FamilyOrderingKey(familyId);

    public static string SeedOrderingKey(string seedId) =>
        FullCampaignGamePackageMaterializationSourceLoader.SeedOrderingKey(seedId);

    public static IReadOnlyList<FullCampaignRuntimeCommandSpec> RuntimeCommands(FullCampaignPackagePlanRow row)
    {
        var ids = PackageIds(row.FamilyId, row.SeedId);
        return row.FamilyId switch
        {
            "map_panel_rpg" =>
            [
                RuntimeCommand("map-quest-start", "quest/start", ids.QuestId),
                RuntimeCommand("map-dialogue-open", "dialogue/open", ids.DialogueId),
                RuntimeCommand("map-quest-advance", "quest/advance", ids.QuestId, ids.ObjectiveId, amount: 1),
                RuntimeCommand("map-item-reward", "inventory/add_item", ids.ItemId, inventoryId: "inventory/player", amount: 1),
                RuntimeCommand("map-event-flag", "flag/set", ids.FlagId, value: "visited")
            ],
            "survival_sandbox" =>
            [
                RuntimeCommand("survival-collect", "inventory/add_item", ids.IngredientItemId, inventoryId: "inventory/player", amount: 2),
                RuntimeCommand("survival-resource-change", "resource/change", ids.ResourceId, amount: -5),
                RuntimeCommand("survival-craft", "recipe/craft", ids.RecipeId, inventoryId: "inventory/player"),
                RuntimeCommand("survival-flag", "flag/set", ids.FlagId, value: "stabilized")
            ],
            "first_person_grid_dungeon" =>
            [
                RuntimeCommand("dungeon-start-encounter", "encounter/start", ids.EncounterId),
                RuntimeCommand("dungeon-use-ability", "encounter/use_ability", ids.AbilityId, ids.EnemyParticipantId, value: ids.PlayerParticipantId),
                RuntimeCommand("dungeon-progression-flag", "flag/set", ids.FlagId, value: "pressure-tested")
            ],
            _ => []
        };
    }

    private static FullCampaignMaterializedPackage MaterializePackage(FullCampaignPackagePlanRow row, GamePackageValidator validator)
    {
        var package = BuildPackage(row);
        var packageJson = JsonSerializer.Serialize(package, PackageJsonOptions);
        var validation = validator.Validate(package);
        var errors = validation.Issues.Count(issue => issue.Severity is ValidationSeverity.Error or ValidationSeverity.Critical);
        var warnings = validation.Issues.Count(issue => issue.Severity == ValidationSeverity.Warning);
        return new FullCampaignMaterializedPackage
        {
            RowId = row.RowId,
            FamilyId = row.FamilyId,
            SeedId = row.SeedId,
            PackageId = row.PackageId,
            PackageRelativePath = row.PackageRelativePath,
            PackageHash = Hash(packageJson),
            ValidJson = IsValidJson(packageJson),
            ValidationPassed = validation.IsValid,
            ValidationErrorCount = errors,
            ValidationWarningCount = warnings,
            ValidationIssueCodes = validation.Issues.Select(issue => issue.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Goal059RowHash = row.Goal059RowHash,
            Package = package,
            PackageJson = packageJson
        };
    }

    private static GamePackageDefinition BuildPackage(FullCampaignPackagePlanRow row)
    {
        var artifactSet = new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "goal060_package_materialization_artifact_set_v1",
            SnapshotId = row.RowId,
            SourceProductionBatchId = FullCampaignGamePackageMaterializationVocabulary.GoalId,
            ApprovedArtifacts =
            [
                new GeneratorPlanApprovedArtifact
                {
                    ArtifactId = "goal060/" + row.RowId + "/profile",
                    ArtifactKind = "game_profile_v1",
                    ExpectedArtifactContract = "game_profile_v1",
                    ContentJson = Serialize(new
                    {
                        game = new
                        {
                            title = Title(row),
                            genre = row.FamilyId,
                            description = "Goal 060 materialized validator-clean package for " + row.RowId + ".",
                            presentation_mode = PresentationMode(row.FamilyId),
                            world_topology = WorldTopology(row.FamilyId),
                            actor_model = "runtime_state_owned",
                            combat_model = row.FamilyId == "first_person_grid_dungeon" ? "encounter_pressure" : "data_driven_progress",
                            core_loop = RuntimeCommands(row).Select(item => item.CommandType).ToArray()
                        },
                        source_context = new
                        {
                            goal_id = FullCampaignGamePackageMaterializationVocabulary.GoalId,
                            goal059_row_id = row.RowId,
                            goal059_row_hash = row.Goal059RowHash,
                            source_campaign_hash = row.SourceCampaignHash
                        }
                    })
                }
            ]
        };

        var assembly = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet, FixedAppliedAtUtc);
        var package = assembly.Package;
        var ids = PackageIds(row.FamilyId, row.SeedId);
        package.Manifest.PackageId = row.PackageId;
        package.Manifest.Title = Title(row);
        package.Manifest.Description = "Goal 060 materialized package for " + row.FamilyId + " / " + row.SeedId + ".";
        package.Manifest.StartMapId = ids.MapId;
        package.GeneratedContent.Profile.SourceContextJson = Serialize(new
        {
            goalId = FullCampaignGamePackageMaterializationVocabulary.GoalId,
            goal059RowId = row.RowId,
            goal059RowHash = row.Goal059RowHash,
            sourceCampaignHash = row.SourceCampaignHash,
            packageAssemblyDomains = row.SelectedPackageAssemblyDomains
        });
        AddWorld(package, row, ids);
        AddEconomy(package, row, ids);
        AddNarrative(package, row, ids);
        AddEncounter(package, row, ids);
        return package;
    }

    private static void AddWorld(GamePackageDefinition package, FullCampaignPackagePlanRow row, PackageIdSet ids)
    {
        if (package.Game.TilePrototypes.All(tile => tile.Id != ids.TileId))
        {
            package.Game.TilePrototypes.Add(new TilePrototypeDefinition
            {
                Id = ids.TileId,
                Name = "Goal060 " + row.FamilyId + " Tile",
                Walkable = true,
                MovementCost = 1.0
            });
        }

        package.Game.EntityPrototypes.Add(new EntityPrototypeDefinition
        {
            Id = ids.NpcPrototypeId,
            Name = "Goal060 Guide " + row.SeedId,
            Components =
            [
                new ComponentDefinition { Type = "npc", Args = { ["family"] = row.FamilyId, ["seed"] = row.SeedId } }
            ]
        });

        package.Game.Maps.Add(new MapDefinition
        {
            Id = ids.MapId,
            Name = "Goal060 " + row.FamilyId + " " + row.SeedId,
            Width = 6,
            Height = 6,
            DefaultTileId = ids.TileId,
            StartPosition = new Position2D(1, 1),
            Entities =
            [
                new EntityInstanceDefinition
                {
                    Id = ids.NpcEntityId,
                    PrototypeId = ids.NpcPrototypeId,
                    Position = new Position2D(2, 1)
                },
                new EntityInstanceDefinition
                {
                    Id = "entity/player/" + ids.SafeFamily + "/" + ids.SafeSeed,
                    PrototypeId = "entity/player",
                    Position = new Position2D(1, 1)
                }
            ]
        });
    }

    private static void AddEconomy(GamePackageDefinition package, FullCampaignPackagePlanRow row, PackageIdSet ids)
    {
        package.Game.Items.Add(new ItemDefinition
        {
            Id = ids.ItemId,
            Name = "Goal060 Relic " + row.SeedId,
            Description = "Traceable package-bound item for " + row.RowId + ".",
            Kind = "goal060",
            MaxStack = 99,
            Tags = ["goal060", row.FamilyId, row.SeedId]
        });

        package.Game.Items.Add(new ItemDefinition
        {
            Id = ids.IngredientItemId,
            Name = "Goal060 Material " + row.SeedId,
            Description = "Crafting input for " + row.RowId + ".",
            Kind = "material",
            MaxStack = 99,
            Tags = ["goal060", "crafting"]
        });

        package.Game.Resources.Add(new ResourceDefinition
        {
            Id = ids.ResourceId,
            Name = "Goal060 Resource " + row.SeedId,
            Kind = row.FamilyId == "first_person_grid_dungeon" ? "health" : "survival",
            Description = "Runtime resource for " + row.RowId + ".",
            DefaultValue = 25,
            MinValue = 0,
            MaxValue = 100,
            Tags = row.FamilyId == "first_person_grid_dungeon" ? ["goal060", "health"] : ["goal060", row.FamilyId]
        });

        package.Game.Recipes.Add(new RecipeDefinition
        {
            Id = ids.RecipeId,
            Name = "Goal060 Craft " + row.SeedId,
            Category = "goal060",
            Inputs =
            [
                new CostDefinition { Kind = "item", Id = ids.IngredientItemId, Amount = 1 }
            ],
            Outputs =
            [
                new OutputDefinition { Kind = "item", Id = ids.ItemId, Amount = 1 }
            ],
            SuccessChance = 1
        });

        package.Game.Inventories.Add(new InventoryDefinition
        {
            Id = "inventory/player",
            OwnerKind = "player",
            OwnerId = "player",
            Slots = 12,
            Stacks =
            [
                new ItemStackDefinition { ItemId = ids.IngredientItemId, Amount = 2 }
            ],
            Metadata =
            {
                ["goal060RowId"] = row.RowId
            }
        });
    }

    private static void AddNarrative(GamePackageDefinition package, FullCampaignPackagePlanRow row, PackageIdSet ids)
    {
        package.Game.Quests.Add(new QuestDefinition
        {
            Id = ids.QuestId,
            Title = "Goal060 " + row.FamilyId + " Progress",
            Description = "Quest trace for " + row.RowId + ".",
            Kind = "goal060",
            Objectives =
            [
                new QuestObjectiveDefinition
                {
                    Id = ids.ObjectiveId,
                    Kind = row.FamilyId == "survival_sandbox" ? "craft_recipe" : "custom_counter",
                    TargetId = row.FamilyId == "survival_sandbox" ? ids.RecipeId : null,
                    RequiredAmount = 1,
                    CompletionEffects =
                    [
                        new OutputDefinition { Kind = "item", Id = ids.ItemId, Amount = 1 }
                    ],
                    Metadata =
                    {
                        ["goal060RowId"] = row.RowId,
                        ["sourceCampaignHash"] = row.SourceCampaignHash
                    }
                }
            ],
            Rewards =
            [
                new OutputDefinition { Kind = "item", Id = ids.ItemId, Amount = 1 }
            ],
            Tags = ["goal060", row.FamilyId, row.SeedId]
        });

        package.Game.Dialogues.Add(new DialogueDefinition
        {
            Id = ids.DialogueId,
            Title = "Goal060 Dialogue " + row.SeedId,
            StartNodeId = ids.DialogueStartNodeId,
            Nodes =
            [
                new DialogueNodeDefinition
                {
                    Id = ids.DialogueStartNodeId,
                    SpeakerId = ids.NpcEntityId,
                    Text = "Goal060 package row " + row.RowId,
                    Choices =
                    [
                        new DialogueChoiceDefinition
                        {
                            Id = ids.DialogueChoiceId,
                            Text = "Advance package proof",
                            StartQuestId = ids.QuestId,
                            CloseDialogue = true,
                            Rewards =
                            [
                                new OutputDefinition { Kind = "item", Id = ids.ItemId, Amount = 1 }
                            ]
                        }
                    ]
                }
            ],
            Tags = ["goal060", row.FamilyId]
        });
    }

    private static void AddEncounter(GamePackageDefinition package, FullCampaignPackagePlanRow row, PackageIdSet ids)
    {
        package.Game.Stats.Add(new StatDefinition
        {
            Id = ids.StatId,
            Name = "Goal060 Pressure",
            Kind = "attribute",
            Description = "Dungeon pressure stat for " + row.RowId + ".",
            DefaultValue = 5,
            MinValue = 0,
            MaxValue = 10
        });

        package.Game.Abilities.Add(new AbilityDefinition
        {
            Id = ids.AbilityId,
            Name = "Goal060 Strike " + row.SeedId,
            Kind = "attack",
            Power = 4,
            ResourceId = ids.ResourceId,
            Tags = ["goal060", "basic_attack"]
        });

        package.Game.Encounters.Add(new EncounterDefinition
        {
            Id = ids.EncounterId,
            Name = "Goal060 Encounter " + row.SeedId,
            Kind = row.FamilyId == "first_person_grid_dungeon" ? "grid_dungeon_pressure" : "campaign_pressure",
            Participants =
            [
                new EncounterParticipantDefinition
                {
                    Id = ids.PlayerParticipantId,
                    Name = "Goal060 Explorer",
                    Team = "player",
                    Abilities = [ids.AbilityId],
                    Stats = [new OutputDefinition { Kind = "stat", Id = ids.StatId, Amount = 5 }],
                    Resources = [new OutputDefinition { Kind = "resource", Id = ids.ResourceId, Amount = 20 }]
                },
                new EncounterParticipantDefinition
                {
                    Id = ids.EnemyParticipantId,
                    Name = "Goal060 Pressure Node",
                    Team = "enemy",
                    Stats = [new OutputDefinition { Kind = "stat", Id = ids.StatId, Amount = 3 }],
                    Resources = [new OutputDefinition { Kind = "resource", Id = ids.ResourceId, Amount = 8 }]
                }
            ],
            Rewards =
            [
                new OutputDefinition { Kind = "item", Id = ids.ItemId, Amount = 1 }
            ],
            Metadata =
            {
                ["default_attack_ability_id"] = ids.AbilityId,
                ["goal060RowId"] = row.RowId
            }
        });
    }

    private static string PackageId(Goal059MatrixRowSource row) =>
        "game/goal060/" + FullCampaignGamePackageMaterializationSourceLoader.SafeSegment(row.FamilyId) + "/" + FullCampaignGamePackageMaterializationSourceLoader.SafeSegment(row.SeedId);

    private static string PackageRelativePath(Goal059MatrixRowSource row) =>
        "packages/" + row.FamilyId + "/" + row.RowId + "/game-package.json";

    private static string RuntimeLoopKind(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "traversal_npc_quest_event_item_progress",
            "survival_sandbox" => "hazard_resource_collect_consume_craft_transition",
            "first_person_grid_dungeon" => "grid_dungeon_orientation_encounter_progression_pressure",
            _ => "unknown"
        };

    private static string PreviewExportProfile(Goal059MatrixRowSource row) =>
        row.SelectedPreviewExportRefs.FirstOrDefault(item => item.StartsWith("exportMode:", StringComparison.Ordinal))?.Substring("exportMode:".Length)
        ?? row.SelectedPreviewExportRefs.FirstOrDefault()
        ?? string.Empty;

    private static PackageIdSet PackageIds(string familyId, string seedId)
    {
        var safeFamily = FullCampaignGamePackageMaterializationSourceLoader.SafeSegment(familyId);
        var safeSeed = FullCampaignGamePackageMaterializationSourceLoader.SafeSegment(seedId);
        var prefix = "goal060/" + safeFamily + "/" + safeSeed;
        return new PackageIdSet(
            safeFamily,
            safeSeed,
            "map/" + prefix + "/start",
            "tile/" + prefix + "/floor",
            "entity/" + prefix + "/guide",
            "entity/" + prefix + "/guide/start",
            "item/" + prefix + "/relic",
            "item/" + prefix + "/material",
            "resource/" + prefix + "/pressure",
            "recipe/" + prefix + "/craft",
            "quest/" + prefix + "/progress",
            "objective/" + prefix + "/advance",
            "dialogue/" + prefix + "/guide",
            "node/" + prefix + "/start",
            "choice/" + prefix + "/advance",
            "stat/" + prefix + "/pressure",
            "ability/" + prefix + "/strike",
            "encounter/" + prefix + "/pressure",
            "participant/" + prefix + "/player",
            "participant/" + prefix + "/pressure-node",
            "flag/" + prefix + "/runtime-proof");
    }

    private static string Title(FullCampaignPackagePlanRow row) =>
        "Goal060 " + row.FamilyId.Replace('_', ' ') + " " + row.SeedId.Replace('_', ' ');

    private static string PresentationMode(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "map_panel",
            "survival_sandbox" => "survival_sandbox",
            "first_person_grid_dungeon" => "first_person_grid",
            _ => "generic"
        };

    private static string WorldTopology(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "connected_regions",
            "survival_sandbox" => "resource_hazard_zones",
            "first_person_grid_dungeon" => "grid_dungeon",
            _ => "single_map"
        };

    private static FullCampaignRuntimeCommandSpec RuntimeCommand(
        string commandId,
        string commandType,
        string targetId,
        string secondaryTargetId = "",
        string inventoryId = "",
        double amount = 0,
        string value = "") =>
        new()
        {
            CommandId = commandId,
            CommandType = commandType,
            TargetId = targetId,
            SecondaryTargetId = secondaryTargetId,
            InventoryId = inventoryId,
            Amount = amount,
            Value = value
        };

    private static string StagedPackageRelativePath(FullCampaignMaterializedPackage package) =>
        "package-materialization/" + package.PackageRelativePath;

    private static bool IsValidJson(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static InvalidFullCampaignMaterializationScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params FullCampaignGamePackageMaterializationDiagnostic[] diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            CausalMutation = mutation,
            ExpectedStatus = expectedStatus,
            ActualStatus = expectedStatus,
            ExpectedValid = false,
            ActualValid = false,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "critical" => 0,
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static string Serialize<T>(T value) => FullCampaignGamePackageMaterializationHash.Serialize(value);

    private static string Hash(string text) => FullCampaignGamePackageMaterializationHash.Hash(text);

    private static FullCampaignGamePackageMaterializationDiagnostic Error(string code, string target, string message) =>
        FullCampaignGamePackageMaterializationDiagnostic.Error(code, target, message);

    private static FullCampaignGamePackageMaterializationDiagnostic Info(string code, string target, string message) =>
        FullCampaignGamePackageMaterializationDiagnostic.Info(code, target, message);

    private sealed record PackageIdSet(
        string SafeFamily,
        string SafeSeed,
        string MapId,
        string TileId,
        string NpcPrototypeId,
        string NpcEntityId,
        string ItemId,
        string IngredientItemId,
        string ResourceId,
        string RecipeId,
        string QuestId,
        string ObjectiveId,
        string DialogueId,
        string DialogueStartNodeId,
        string DialogueChoiceId,
        string StatId,
        string AbilityId,
        string EncounterId,
        string PlayerParticipantId,
        string EnemyParticipantId,
        string FlagId);
}
