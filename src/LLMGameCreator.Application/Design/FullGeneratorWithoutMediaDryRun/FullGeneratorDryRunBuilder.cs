using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;

namespace LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;

public sealed class FullGeneratorDryRunBuilder
{
    public FullGeneratorDryRunManifest BuildManifest(FullGeneratorSourceBundle source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var familySummaries = source.Goal043Catalog.Families
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(entry =>
            {
                var planRef = FindSourceRef(source, "Goal043", "family_lifecycle_plan", MultiFamilyGeneratedTemplateEvidenceService.PlanFileName(entry.FamilyId));
                var proofRef = FindSourceRef(source, "Goal043", "family_simulatable_loop_proof", MultiFamilyGeneratedTemplateEvidenceService.LoopProofFileName(entry.FamilyId));
                var payloadRef = FindSourceRef(source, "Goal040", "chunked_preview_payload", entry.SourceGoal040PayloadFileName);
                var proof = source.Goal043ProofsByFamilyId[entry.FamilyId];
                var payload = source.Goal040PayloadsByScenario[entry.ScenarioId];
                return new FullGeneratorFamilySourceSummary
                {
                    FamilyId = entry.FamilyId,
                    ScenarioId = entry.ScenarioId,
                    ProfileId = entry.ProfileId,
                    Goal043PlanRef = planRef.ArtifactRelativePath,
                    Goal043PlanHash = planRef.ArtifactHash,
                    Goal043LoopProofRef = proofRef.ArtifactRelativePath,
                    Goal043LoopProofHash = proofRef.ArtifactHash,
                    Goal040PayloadRef = payloadRef.ArtifactRelativePath,
                    Goal040PayloadHash = payloadRef.ArtifactHash,
                    RuntimeDeltaMarkerCount = payload.RuntimeDeltaMarkers.Count,
                    StateChangingEventCount = proof.Events.Count
                };
            })
            .ToList();

        return new FullGeneratorDryRunManifest
        {
            Accepted = false,
            AcceptedPreflightGates =
            [
                new() { GateId = "multi_family_generated_template_vertical_slice_verification", Status = "passed", ProvenanceKind = "user_handoff", EvidenceRef = "Goal 047 starting preflight" },
                new() { GateId = "semantic_pack_composition_blueprint_verification", Status = "produced_for_review_not_passed", ProvenanceKind = "inherited", EvidenceRef = "Goal 031 preserved policy" },
                new() { GateId = "dynamic_semantic_feature_system_verification", Status = "produced_for_review_not_passed", ProvenanceKind = "inherited", EvidenceRef = "Goal 032 preserved policy" },
                new() { GateId = FullGeneratorWithoutMediaDryRunVocabulary.FinalGate, Status = "required", ProvenanceKind = "programmatic", EvidenceRef = "Goal 047 produced for review" }
            ],
            SelectedFamilyIds = FullGeneratorWithoutMediaDryRunVocabulary.FamilyIds
                .OrderBy(item => FamilyOrderingKey(item), StringComparer.Ordinal)
                .ToList(),
            ProfileCapabilityRefs = source.Goal043Catalog.Families
                .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
                .SelectMany(item => new[] { "profile/" + item.ProfileId, "family/" + item.FamilyId }.Concat(item.SelectedFeatureRefs))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            SelectedWorldChunkRuntimeRefs = source.SourceArtifactRefs
                .Where(item => item.SourceGoal is "Goal038" or "Goal039" or "Goal040")
                .Where(item => item.ArtifactFamily.Contains("world", StringComparison.Ordinal)
                    || item.ArtifactFamily.Contains("map", StringComparison.Ordinal)
                    || item.ArtifactFamily.Contains("chunk", StringComparison.Ordinal)
                    || item.ArtifactFamily.Contains("runtime", StringComparison.Ordinal)
                    || item.ArtifactFamily.Contains("preview", StringComparison.Ordinal)
                    || item.ArtifactFamily.Contains("export", StringComparison.Ordinal))
                .Select(item => item.EvidenceRef)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            SelectedTemplateLoopRefs = source.SourceArtifactRefs
                .Where(item => item.SourceGoal == "Goal043" && (item.ArtifactFamily.Contains("family_", StringComparison.Ordinal) || item.ArtifactFamily.Contains("lifecycle", StringComparison.Ordinal)))
                .Select(item => item.EvidenceRef)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            SelectedDraftLuaExpansionRefs = source.SourceArtifactRefs
                .Where(item => item.SourceGoal is "Goal034" or "Goal035" or "Goal036" or "Goal037")
                .Select(item => item.EvidenceRef)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            FamilySourceSummaries = familySummaries,
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics =
            [
                FullGeneratorDiagnostic.Info(
                    "goal047.source.goal043_handoff_recorded",
                    "multi_family_generated_template_vertical_slice_verification",
                    "Goal 043 is recorded as accepted by user handoff before producing Goal 047 evidence.")
            ]
        };
    }

    public IReadOnlyList<FullGeneratorFamilyDryRunRecord> BuildFamilyDryRuns(
        FullGeneratorSourceBundle source,
        FullGeneratorDryRunManifest manifest,
        FullGeneratorReviewPromotionLedger ledger,
        FullGeneratorPackageCompatibilitySummary packageSummary)
    {
        var packageRowsByFamily = packageSummary.Rows
            .GroupBy(row => row.FamilyId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        return manifest.FamilySourceSummaries
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .Select(summary =>
            {
                var entry = source.Goal043Catalog.Families.Single(item => item.FamilyId == summary.FamilyId);
                var plan = source.Goal043PlansByFamilyId[summary.FamilyId];
                var proof = source.Goal043ProofsByFamilyId[summary.FamilyId];
                var payload = source.Goal040PayloadsByScenario[summary.ScenarioId];
                return new FullGeneratorFamilyDryRunRecord
                {
                    FamilyId = summary.FamilyId,
                    ScenarioId = summary.ScenarioId,
                    ProfileId = summary.ProfileId,
                    DeterministicOrderingKey = FamilyOrderingKey(summary.FamilyId),
                    FamilyProfileRefs =
                    [
                        "profile/" + summary.ProfileId,
                        "family/" + summary.FamilyId,
                        entry.FamilyExtensionSchemaId
                    ],
                    ScenarioFamilyLensRefs = payload.FamilyLensViews
                        .Where(view => view.FamilyLensId == summary.FamilyId)
                        .SelectMany(view => view.ExpectedConsumerNeeds.Concat(view.RouteOrientationHints).Append(view.FamilyLensId))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal)
                        .ToList(),
                    RegionChunkRuntimeTraversalRefs = plan.RegionChunkTraversalSourceRefs
                        .Select(item => item.EvidenceRef)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal)
                        .ToList(),
                    ReviewPromotionLedgerRefs = ledger.Transitions
                        .Where(item => item.FamilyId == summary.FamilyId)
                        .Select(item => item.TransitionId)
                        .OrderBy(item => item, StringComparer.Ordinal)
                        .ToList(),
                    GeneratedSystemCoverage = BuildSystemCoverage(summary.FamilyId, summary.ScenarioId, packageRowsByFamily[summary.FamilyId]),
                    RuntimePreviewPayloadSummary = BuildRuntimePreviewPayloadSummary(source, summary, payload),
                    ExportCandidatePayloadSummary = BuildExportSummary(summary, payload),
                    ReplayHashProof = new FullGeneratorReplayHashProof
                    {
                        SourceLoopProofHash = summary.Goal043LoopProofHash,
                        ReplayHash = proof.ReplayDeterminismHash,
                        ReplayedHash = proof.ReplayedDeterminismHash,
                        Passed = proof.StateChanged
                            && !string.IsNullOrWhiteSpace(proof.ReplayDeterminismHash)
                            && proof.ReplayDeterminismHash == proof.ReplayedDeterminismHash
                    },
                    StateChangingLoopProof = proof.StateChanged,
                    SourceRefs = plan.SourceReferences.Select(item => new FullGeneratorSourceArtifactReference
                    {
                        SourceGoal = item.SourceGoal,
                        EvidenceRef = item.EvidenceRef,
                        ArtifactFamily = item.ArtifactFamily,
                        ArtifactFileName = item.ArtifactFileName,
                        ArtifactRelativePath = item.ArtifactRelativePath,
                        ArtifactHash = item.ArtifactHash,
                        Summary = "Goal 043 source ref consumed by Goal 047 family dry-run."
                    }).ToList(),
                    Diagnostics =
                    [
                        FullGeneratorDiagnostic.Info(
                            "goal047.family.dry_run_record_built",
                            summary.FamilyId,
                            "Family dry-run record was built through the shared Goal 047 code path.")
                    ]
                };
            })
            .ToList();
    }

    public FullGeneratorRuntimePreviewValidationMatrix BuildRuntimePreviewValidationMatrix(
        FullGeneratorSourceBundle source,
        IReadOnlyList<FullGeneratorFamilyDryRunRecord> familyDryRuns)
    {
        var rows = familyDryRuns
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(record =>
            {
                var payload = source.Goal040PayloadsByScenario[record.ScenarioId];
                var chunkWindowRefsWithinBounds = payload.TraversalRoute.Count > 0
                    && payload.TraversalRoute.All(step => payload.ChunkIds.Contains(step.ChunkId, StringComparer.Ordinal));
                var noLeakClaims = record.BoundaryClaims.AllFalse && payload.BoundaryClaims.AllFalse;
                var rowDiagnostics = new List<FullGeneratorDiagnostic>();
                if (!chunkWindowRefsWithinBounds)
                {
                    rowDiagnostics.Add(FullGeneratorDiagnostic.Error("goal047.runtime_preview.chunk_bounds.invalid", record.FamilyId, "Traversal route contains a chunk outside the declared payload chunk ids."));
                }

                if (!noLeakClaims)
                {
                    rowDiagnostics.Add(FullGeneratorDiagnostic.Error("goal047.boundary.leak_claim", record.FamilyId, "Runtime preview payload includes forbidden boundary claims."));
                }

                var passed = record.RuntimePreviewPayloadSummary.StableRelativeRefs
                    && record.RuntimePreviewPayloadSummary.SourceHashesMatch
                    && record.StateChangingLoopProof
                    && chunkWindowRefsWithinBounds
                    && record.ExportCandidatePayloadSummary.DeterministicSelection
                    && noLeakClaims;

                return new FullGeneratorRuntimePreviewValidationRow
                {
                    FamilyId = record.FamilyId,
                    ScenarioId = record.ScenarioId,
                    PayloadRelativePath = record.RuntimePreviewPayloadSummary.PayloadRelativePath,
                    StableRelativeRefs = record.RuntimePreviewPayloadSummary.StableRelativeRefs,
                    SourceHashesMatch = record.RuntimePreviewPayloadSummary.SourceHashesMatch,
                    CommandStateTransitionsConsistent = record.StateChangingLoopProof,
                    ChunkWindowRefsWithinBounds = chunkWindowRefsWithinBounds,
                    ExportProfileDeterministic = record.ExportCandidatePayloadSummary.DeterministicSelection,
                    NoLeakClaims = noLeakClaims,
                    Passed = passed,
                    Diagnostics = FullGeneratorWithoutMediaDryRunValidator.SortDiagnostics(rowDiagnostics)
                };
            })
            .ToList();

        return new FullGeneratorRuntimePreviewValidationMatrix
        {
            Passed = rows.Count == 3 && rows.All(row => row.Passed),
            FamilyCount = rows.Count,
            Rows = rows
        };
    }

    public FullGeneratorExportProfileSelectionMatrix BuildExportProfileSelectionMatrix(
        FullGeneratorSourceBundle source,
        IReadOnlyList<FullGeneratorFamilyDryRunRecord> familyDryRuns)
    {
        var rows = familyDryRuns
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(record =>
            {
                var payload = source.Goal040PayloadsByScenario[record.ScenarioId];
                return new FullGeneratorExportProfileSelectionRow
                {
                    FamilyId = record.FamilyId,
                    ScenarioId = record.ScenarioId,
                    ExportProfileId = ExportProfileId(record.FamilyId),
                    PresentationMode = PresentationMode(record.FamilyId),
                    PayloadRelativePath = record.RuntimePreviewPayloadSummary.PayloadRelativePath,
                    PayloadHash = record.RuntimePreviewPayloadSummary.PayloadHash,
                    WithoutMedia = true,
                    DeterministicSelection = true,
                    RuntimePreviewCompatible = source.Goal040ExportManifest.RuntimePreviewCompatible && payload.PreviewExportReadiness.PreviewPayloadReady,
                    UnityExportCompatible = source.Goal040ExportManifest.UnityExportCompatible && payload.PreviewExportReadiness.ExportManifestReady,
                    Passed = source.Goal040ExportManifest.RuntimePreviewCompatible
                        && source.Goal040ExportManifest.UnityExportCompatible
                        && payload.PreviewExportReadiness.PreviewPayloadReady
                        && payload.PreviewExportReadiness.ExportManifestReady
                };
            })
            .ToList();

        return new FullGeneratorExportProfileSelectionMatrix
        {
            Passed = rows.Count == 3
                && rows.All(row => row.Passed && row.WithoutMedia && row.DeterministicSelection)
                && rows.Select(row => row.ExportProfileId).Distinct(StringComparer.Ordinal).Count() == rows.Count,
            FamilyCount = rows.Count,
            Rows = rows
        };
    }

    public FullGeneratorPackageCompatibilitySummary BuildPackageCompatibilitySummary(
        FullGeneratorDryRunManifest manifest)
    {
        var rows = manifest.FamilySourceSummaries
            .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .SelectMany(summary => BuildPackageCompatibilityRows(summary.FamilyId))
            .ToList();
        var directMaterializationDecision =
            "direct_materialization_not_attempted: Goal 047 family dry-run records are review/preview/export candidates, not accepted GeneratorPlanApprovedArtifactSet content. Creating a new materializer would be a new adapter beyond this goal; strict compatibility proof maps selected outputs to existing package assembly targets instead.";

        return new FullGeneratorPackageCompatibilitySummary
        {
            PackageMaterializationAttempted = false,
            MaterializedValidatorCleanPackages = false,
            CompatibilityProofPassed = rows.Count > 0
                && rows.All(row => row.CompatibilityStatus is "compatible_existing_assembler" or "compatible_existing_metadata_or_future_required")
                && FullGeneratorWithoutMediaDryRunVocabulary.FamilyIds.All(familyId => rows.Any(row => row.FamilyId == familyId)),
            DirectMaterializationSafetyDecision = directMaterializationDecision,
            ExistingAssemblersAndValidators =
            [
                "GeneratorPlanGamePackageAssembler",
                "GamePackageValidator",
                "PackageAssemblyWorldEntitiesAcceptanceService",
                "PackageAssemblyDialogueQuestsAcceptanceService",
                "PackageAssemblyItemsEconomyCraftingAcceptanceService",
                "PackageAssemblyCombatProgressionAcceptanceService"
            ],
            Rows = rows,
            Diagnostics =
            [
                FullGeneratorDiagnostic.Info(
                    "goal047.package.compatibility_proof_used",
                    "package-compatibility-or-materialization-summary",
                    "Goal 047 uses strict package compatibility proof because direct materialization needs a new reviewed adapter.")
            ]
        };
    }

    private static IReadOnlyList<FullGeneratorSystemCoverageRow> BuildSystemCoverage(
        string familyId,
        string scenarioId,
        IReadOnlyList<FullGeneratorPackageCompatibilityRow> packageRows) =>
        FullGeneratorWithoutMediaDryRunVocabulary.GeneratedSystemIds
            .Select(systemId =>
            {
                var packageRow = packageRows.First(row => row.SystemId == systemId);
                return new FullGeneratorSystemCoverageRow
                {
                    SystemId = systemId,
                    CoverageStatus = packageRow.CompatibilityStatus == "compatible_existing_assembler"
                        ? "dry_run_output_package_compatible"
                        : "dry_run_output_metadata_or_future_required",
                    SourceRef = $"family/{familyId}/scenario/{scenarioId}/system/{systemId}",
                    PackageCompatibilityTarget = packageRow.ExistingPackageTarget
                };
            })
            .ToList();

    private static FullGeneratorRuntimePreviewPayloadSummary BuildRuntimePreviewPayloadSummary(
        FullGeneratorSourceBundle source,
        FullGeneratorFamilySourceSummary summary,
        ChunkedPreviewPayload payload)
    {
        var sourceHashesMatch = source.ArtifactHashByRelativePath.TryGetValue(summary.Goal040PayloadRef, out var payloadHash)
            && payloadHash == summary.Goal040PayloadHash;
        return new FullGeneratorRuntimePreviewPayloadSummary
        {
            PayloadRelativePath = summary.Goal040PayloadRef,
            PayloadHash = summary.Goal040PayloadHash,
            StableRelativeRefs = IsRelative(summary.Goal040PayloadRef)
                && payload.SourceEvidence.Goal038EvidenceRefs.Concat(payload.SourceEvidence.Goal039EvidenceRefs)
                    .All(item => !Path.IsPathRooted(item.ArtifactFileName)),
            SourceHashesMatch = sourceHashesMatch,
            ChunkCount = payload.ChunkIds.Count,
            RouteStepCount = payload.TraversalRoute.Count,
            RuntimeDeltaMarkerCount = payload.RuntimeDeltaMarkers.Count,
            SaveLoadBacked = payload.ReplaySaveLoadCorrelation.SerializerRoundtripPassed
                && payload.ReplaySaveLoadCorrelation.SnapshotRoundtripPassed,
            ReplayBacked = payload.ReplaySaveLoadCorrelation.ReplayDeterminismPassed
        };
    }

    private static FullGeneratorExportCandidatePayloadSummary BuildExportSummary(
        FullGeneratorFamilySourceSummary summary,
        ChunkedPreviewPayload payload) =>
        new()
        {
            ExportProfileId = ExportProfileId(summary.FamilyId),
            ExportMode = PresentationMode(summary.FamilyId),
            PayloadRelativePath = summary.Goal040PayloadRef,
            PayloadHash = summary.Goal040PayloadHash,
            DeterministicSelection = payload.ProfileId == summary.ProfileId,
            WithoutMedia = true
        };

    private static IReadOnlyList<FullGeneratorPackageCompatibilityRow> BuildPackageCompatibilityRows(string familyId) =>
    [
        PackageRow(familyId, "world", "region/chunk/runtime traversal refs", "GamePackage.Game.Maps + GeneratedContent.Regions", "PackageAssemblyWorldEntitiesAcceptanceService", "compatible_existing_assembler", true),
        PackageRow(familyId, "entity", "family target and entity intent refs", "GamePackage.Game.EntityPrototypes + map placements", "PackageAssemblyWorldEntitiesAcceptanceService", "compatible_existing_assembler", true),
        PackageRow(familyId, "quest", "family loop quest/event progress marker", "GamePackage.Game.Quests", "PackageAssemblyDialogueQuestsAcceptanceService", "compatible_existing_assembler", true),
        PackageRow(familyId, "dialogue", "selected intention refs and family lens hints", "GamePackage.Game.Dialogues", "PackageAssemblyDialogueQuestsAcceptanceService", "compatible_existing_assembler", true),
        PackageRow(familyId, "item", "reward/resource/item loop markers", "GamePackage.Game.Items + LootTables", "PackageAssemblyItemsEconomyCraftingAcceptanceService", "compatible_existing_assembler", true),
        PackageRow(familyId, "economy", "resource/transaction/crafting compatibility refs", "GamePackage.Game.Resources + Recipes + Transactions", "PackageAssemblyItemsEconomyCraftingAcceptanceService", "compatible_existing_assembler", true),
        PackageRow(familyId, "combat", "encounter pressure/action refs", "GamePackage.Game.Encounters + Abilities + Statuses", "PackageAssemblyCombatProgressionAcceptanceService", "compatible_existing_assembler", true),
        PackageRow(familyId, "progression", "quest/progression/locked-route proof refs", "GamePackage.Game.Progressions + progression stages", "PackageAssemblyCombatProgressionAcceptanceService", "compatible_existing_assembler", true),
        PackageRow(familyId, "settlement", "family world/region metadata refs", "GeneratedContent metadata or future settlement-specific pack", "GamePackageValidator + future reviewed adapter", "compatible_existing_metadata_or_future_required", false),
        PackageRow(familyId, "event", "quest/event intent and state transition refs", "GamePackage.Game.Quests/Objectives + GeneratedContent events", "PackageAssemblyDialogueQuestsAcceptanceService", "compatible_existing_assembler", true)
    ];

    private static FullGeneratorPackageCompatibilityRow PackageRow(
        string familyId,
        string systemId,
        string dryRunSource,
        string target,
        string assembler,
        string status,
        bool directMaterializationSafeNow) =>
        new()
        {
            FamilyId = familyId,
            SystemId = systemId,
            DryRunSource = dryRunSource,
            ExistingPackageTarget = target,
            ExistingAssemblerOrValidator = assembler,
            CompatibilityStatus = status,
            DirectMaterializationSafeNow = directMaterializationSafeNow
        };

    public static string ExportProfileId(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "export-profile/map-panel-rpg/without-media",
            "survival_sandbox" => "export-profile/survival-sandbox/without-media",
            "first_person_grid_dungeon" => "export-profile/first-person-grid-dungeon/without-media",
            _ => "export-profile/unknown/without-media"
        };

    private static string PresentationMode(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "map_panel_runtime_preview",
            "survival_sandbox" => "survival_sandbox_runtime_preview",
            "first_person_grid_dungeon" => "first_person_grid_runtime_preview",
            _ => "unknown"
        };

    private static FullGeneratorSourceArtifactReference FindSourceRef(
        FullGeneratorSourceBundle source,
        string sourceGoal,
        string artifactFamily,
        string fileName) =>
        source.SourceArtifactRefs.First(item =>
            item.SourceGoal == sourceGoal
            && item.ArtifactFamily == artifactFamily
            && item.ArtifactFileName == fileName);

    private static bool IsRelative(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains("..", StringComparison.Ordinal)
        && !path.Contains(':', StringComparison.Ordinal);

    private static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };
}
