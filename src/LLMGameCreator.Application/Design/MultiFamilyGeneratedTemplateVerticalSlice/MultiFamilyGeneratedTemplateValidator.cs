namespace LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class MultiFamilyGeneratedTemplateValidator
{
    public IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> ValidateCatalog(FamilyTemplateCatalog catalog)
    {
        var diagnostics = new List<MultiFamilyGeneratedTemplateDiagnostic>();
        if (catalog.FamilyCount != 3 || catalog.Families.Count != 3)
        {
            diagnostics.Add(Error("goal043.catalog.family_count", "family-template-catalog", "Exactly three family templates are required."));
        }

        if (catalog.Families.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count() != catalog.Families.Count)
        {
            diagnostics.Add(Error("goal043.catalog.duplicate_family_id", "family-template-catalog", "Family ids must be unique."));
        }

        if (catalog.Families.Select(item => item.DeterministicOrderingKey).Distinct(StringComparer.Ordinal).Count() != catalog.Families.Count)
        {
            diagnostics.Add(Error("goal043.catalog.cross_family_id_collision", "family-template-catalog", "Family ordering keys must not collide."));
        }

        foreach (var familyId in MultiFamilyGeneratedTemplateVocabulary.FamilyIds)
        {
            if (!catalog.Families.Any(item => item.FamilyId == familyId))
            {
                diagnostics.Add(Error("goal043.catalog.family_missing", familyId, "Required family id is missing."));
            }
        }

        if (!catalog.Goal040AcceptedByUserHandoff)
        {
            diagnostics.Add(Error("goal043.catalog.goal040_handoff_missing", "goal040_handoff", "Goal 040 must be recorded as accepted by user handoff before Goal 043."));
        }

        if (!catalog.SourceGoal037HybridExpansionConsumed || !catalog.SourceGoal038WorldMapConsumed || !catalog.SourceGoal039RuntimeTraversalConsumed || !catalog.SourceGoal040PreviewExportConsumed)
        {
            diagnostics.Add(Error("goal043.catalog.source_goal_chain_missing", "source_artifacts", "Goal 043 must consume Goal 037, 038, 039 and 040 evidence."));
        }

        return SortDiagnostics(diagnostics);
    }

    public IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> ValidatePlan(FamilyLifecyclePlan plan)
    {
        var diagnostics = new List<MultiFamilyGeneratedTemplateDiagnostic>();
        if (!MultiFamilyGeneratedTemplateVocabulary.FamilyIds.Contains(plan.FamilyId, StringComparer.Ordinal))
        {
            diagnostics.Add(Error("goal043.family.unknown", plan.FamilyId, "Family id must be one of the Goal 043 families."));
        }

        if (!MultiFamilyGeneratedTemplateVocabulary.ScenarioByFamilyId.TryGetValue(plan.FamilyId, out var expectedScenario)
            || !string.Equals(expectedScenario, plan.ScenarioId, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("goal043.scenario.unknown_or_mismatch", plan.FamilyId, "Family scenario id must match the deterministic Goal 043 mapping."));
        }

        if (!string.Equals(plan.ProfileId, plan.ScenarioId, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("goal043.scenario.profile_mismatch", plan.FamilyId, "Scenario/profile ids must match the consumed source payload."));
        }

        if (!string.Equals(plan.SharedLifecycleContractId, MultiFamilyGeneratedTemplateVocabulary.SharedLifecycleContractId, StringComparison.Ordinal)
            || !plan.LifecyclePhases.SequenceEqual(MultiFamilyGeneratedTemplateVocabulary.SharedLifecyclePhases, StringComparer.Ordinal))
        {
            diagnostics.Add(Error("goal043.lifecycle.section_missing", plan.FamilyId, "All shared lifecycle phases are required in deterministic order."));
        }

        if (plan.SelectedFeatureRefs.Count == 0 || plan.SelectedIntentionRefs.Count == 0)
        {
            diagnostics.Add(Error("goal043.lifecycle.intent_missing", plan.FamilyId, "Selected feature and intention references are required."));
        }

        RequireSourceGoals(plan, diagnostics);

        if (plan.PreviewExportConsumerRefs.Count == 0 || !plan.PreviewExportConsumerRefs.Any(item => item.SourceGoal == "Goal040"))
        {
            diagnostics.Add(Error("goal043.source.preview_export_missing", plan.FamilyId, "Goal 040 preview/export source refs are required."));
        }

        if (plan.RegionChunkTraversalSourceRefs.Count == 0 || !plan.RegionChunkTraversalSourceRefs.Any(item => item.SourceGoal == "Goal039"))
        {
            diagnostics.Add(Error("goal043.source.chunk_traversal_missing", plan.FamilyId, "Goal 039 chunk traversal source refs are required."));
        }

        if (plan.SourceReferences.Any(item => item.EvidenceRef.Contains("fake", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(item.ArtifactHash)))
        {
            diagnostics.Add(Error("goal043.source.fake_reference", plan.FamilyId, "Fake or hashless source references are rejected."));
        }

        if (plan.FamilyExtension.FamilyId != plan.FamilyId
            || string.IsNullOrWhiteSpace(plan.FamilyExtension.ExtensionSchemaId)
            || plan.UnscopedFamilySpecificFields.Count > 0)
        {
            diagnostics.Add(Error("goal043.family.extension_scope", plan.FamilyId, "Family-specific fields must stay in the family extension section."));
        }

        if (plan.ArchitectureForkAttempted)
        {
            diagnostics.Add(Error("goal043.architecture_fork.blocked", plan.FamilyId, "Architecture fork attempts are blocked."));
        }

        diagnostics.AddRange(ValidateBoundary(plan.BoundaryClaims, plan.FamilyId));

        if (plan.FinalProsePromotedAsPlayableContent)
        {
            diagnostics.Add(Error("goal043.final_prose.forbidden", plan.FamilyId, "Final prose must not be promoted as playable content."));
        }

        if (plan.LoopCommands.Count == 0)
        {
            diagnostics.Add(Error("goal043.loop.commands_missing", plan.FamilyId, "Loop commands are required."));
        }

        var commandOrders = plan.LoopCommands.Select(item => item.Order).ToList();
        if (!commandOrders.SequenceEqual(commandOrders.Order()))
        {
            diagnostics.Add(Error("goal043.order.nondeterministic", plan.FamilyId, "Loop commands must be deterministically ordered."));
        }

        if (plan.ValidationTrace.Count == 0)
        {
            diagnostics.Add(Error("goal043.validation_trace.missing", plan.FamilyId, "Lifecycle plan must include validation trace."));
        }

        return SortDiagnostics(diagnostics);
    }

    public IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> ValidateProof(FamilySimulatableLoopProof proof)
    {
        var diagnostics = new List<MultiFamilyGeneratedTemplateDiagnostic>();
        if (!proof.StateChanged || proof.InitialState.Values.SequenceEqual(proof.AfterState.Values))
        {
            diagnostics.Add(Error("goal043.loop.state_transition_missing", proof.FamilyId, "Simulatable loop proof must have a before/after state transition."));
        }

        if (proof.OrderedCommands.Count == 0 || proof.Events.Count == 0)
        {
            diagnostics.Add(Error("goal043.loop.events_missing", proof.FamilyId, "Loop proof must include ordered commands and events."));
        }

        if (!proof.BlockedInvalidAction.Blocked)
        {
            diagnostics.Add(Error("goal043.loop.invalid_action_not_blocked", proof.FamilyId, "Loop proof must block an invalid action."));
        }

        if (string.IsNullOrWhiteSpace(proof.ReplayDeterminismHash)
            || !string.Equals(proof.ReplayDeterminismHash, proof.ReplayedDeterminismHash, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("goal043.loop.replay_nondeterministic", proof.FamilyId, "Replay determinism hash must be stable."));
        }

        var requiredMarkers = MultiFamilyGeneratedTemplateCatalog.RequiredFamilyMarkers(proof.FamilyId);
        if (!requiredMarkers.All(proof.ChangedMarkers.Contains))
        {
            diagnostics.Add(Error("goal043.loop.family_minimum_missing", proof.FamilyId, "Loop proof is missing one or more family-specific minimum markers."));
        }

        return SortDiagnostics(diagnostics.Concat(proof.Diagnostics));
    }

    public IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> ValidateSharedContract(SharedLifecycleContract contract)
    {
        var diagnostics = new List<MultiFamilyGeneratedTemplateDiagnostic>();
        if (!contract.Passed)
        {
            diagnostics.Add(Error("goal043.lifecycle.shared_contract_failed", "shared-lifecycle-contract", "Shared lifecycle contract did not pass."));
        }

        if (contract.Families.Any(item => item.ArchitectureForked || !item.OnlyFamilyExtensionDiffers))
        {
            diagnostics.Add(Error("goal043.architecture_fork.blocked", "shared-lifecycle-contract", "Only family-scoped extensions may differ."));
        }

        return SortDiagnostics(diagnostics.Concat(contract.Diagnostics));
    }

    public IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> ValidatePreviewExportMatrix(PreviewExportConsumptionMatrix matrix)
    {
        var diagnostics = new List<MultiFamilyGeneratedTemplateDiagnostic>();
        if (!matrix.Passed || matrix.FamilyCount != 3 || !matrix.SourceGoal040PreviewExportConsumed)
        {
            diagnostics.Add(Error("goal043.preview_export.consumption_missing", "preview-export-consumption-matrix", "Goal 040 preview/export payloads must be consumed for all three families."));
        }

        if (matrix.Rows.Any(item => item.PayloadCopiedWithoutTransformation))
        {
            diagnostics.Add(Error("goal043.preview_export.payload_copy", "preview-export-consumption-matrix", "Goal 043 plans must transform Goal 040 payloads instead of copying them."));
        }

        return SortDiagnostics(diagnostics);
    }

    public InvalidFamilyDiagnosticsMatrix BuildInvalidMatrix(
        FamilyTemplateCatalog catalog,
        IReadOnlyList<FamilyLifecyclePlan> plans,
        IReadOnlyList<FamilySimulatableLoopProof> proofs,
        PreviewExportConsumptionMatrix previewMatrix)
    {
        var plan = plans.Single(item => item.FamilyId == "map_panel_rpg");
        var proof = proofs.Single(item => item.FamilyId == "map_panel_rpg");
        var invalid = new List<InvalidFamilyDiagnosticsScenario>
        {
            CatalogInvalid("duplicate_family_id", "duplicate family id", catalog with { Families = catalog.Families.Select((item, index) => index == 1 ? item with { FamilyId = catalog.Families[0].FamilyId } : item).ToList() }),
            PlanInvalid("unknown_family_id", "unknown family id", plan with { FamilyId = "fake_family" }),
            PlanInvalid("unknown_scenario_id", "unknown scenario id", plan with { ScenarioId = "fake_scenario" }),
            PlanInvalid("missing_required_lifecycle_section", "missing required lifecycle section", plan with { LifecyclePhases = plan.LifecyclePhases.Where(item => item != "simulatable_loop_proof").ToList() }),
            PlanInvalid("missing_preview_export_source_ref", "missing preview/export source ref", plan with { PreviewExportConsumerRefs = [] }),
            PlanInvalid("missing_chunk_traversal_source_ref", "missing chunk traversal source ref", plan with { RegionChunkTraversalSourceRefs = plan.RegionChunkTraversalSourceRefs.Where(item => item.SourceGoal != "Goal039").ToList() }),
            PlanInvalid("fake_goal034_reference", "fake Goal 034 reference", WithFakeSource(plan, "Goal034")),
            PlanInvalid("fake_goal035_reference", "fake Goal 035 reference", WithFakeSource(plan, "Goal035")),
            PlanInvalid("fake_goal036_reference", "fake Goal 036 reference", WithFakeSource(plan, "Goal036")),
            PlanInvalid("fake_goal037_reference", "fake Goal 037 reference", WithFakeSource(plan, "Goal037")),
            PlanInvalid("fake_goal038_reference", "fake Goal 038 reference", WithFakeSource(plan, "Goal038")),
            PlanInvalid("fake_goal039_reference", "fake Goal 039 reference", WithFakeSource(plan, "Goal039")),
            PlanInvalid("fake_goal040_reference", "fake Goal 040 reference", WithFakeSource(plan, "Goal040")),
            PlanInvalid("family_specific_field_outside_extension_scope", "family-specific field outside extension scope", plan with { UnscopedFamilySpecificFields = ["survival.hazardId"] }),
            PlanInvalid("architecture_fork_attempt", "architecture fork attempt", plan with { ArchitectureForkAttempted = true, SharedLifecycleContractId = "forked_lifecycle_contract" }, "blocked"),
            PlanInvalid("gamepackage_schema_mutation_claim", "GamePackage schema mutation claim", plan with { BoundaryClaims = new FamilyTemplateBoundaryClaims { GamePackageSchemaMutation = true } }, "blocked"),
            PlanInvalid("runtime_ui_unity_provider_llm_rag_media_lua_source_leakage", "Runtime/UI/Unity/provider/LLM/RAG/media/Lua-source leakage", plan with { BoundaryClaims = new FamilyTemplateBoundaryClaims { RuntimeSourceMutation = true, RuntimeAbstractionsMutation = true, WinFormsUiMutation = true, UnitySourceMutation = true, ProviderLlmRagMedia = true, LuaSourceOrExecutor = true } }, "blocked"),
            PlanInvalid("final_prose_promoted_as_playable_content", "final prose promoted as playable content", plan with { FinalProsePromotedAsPlayableContent = true }),
            PlanInvalid("nondeterministic_ordering", "nondeterministic ordering", plan with { LoopCommands = plan.LoopCommands.Reverse().ToList() }),
            CatalogInvalid("cross_family_id_collision", "cross-family ID collision", catalog with { Families = catalog.Families.Select((item, index) => index == 1 ? item with { DeterministicOrderingKey = catalog.Families[0].DeterministicOrderingKey } : item).ToList() }),
            PlanInvalid("scenario_profile_mismatch", "scenario profile mismatch", plan with { ProfileId = "wrong_profile" }),
            ProofInvalid("simulatable_loop_proof_without_state_transition", "simulatable loop proof without state transition", proof with { AfterState = proof.InitialState, StateChanged = false, ChangedMarkers = [] }),
            PreviewInvalid("preview_export_payload_copied_without_transformation", "preview/export payload copied without transformation", previewMatrix with { Passed = false, Rows = previewMatrix.Rows.Select((item, index) => index == 0 ? item with { PayloadCopiedWithoutTransformation = true, TransformedIntoLifecyclePlan = false } : item).ToList() }),
            PlanInvalid("missing_validation_trace", "missing validation trace", plan with { ValidationTrace = [] })
        };

        return new InvalidFamilyDiagnosticsMatrix
        {
            ScenarioCount = invalid.Count,
            MatchedExpectationCount = invalid.Count(item => item.ExpectedStatus == item.ActualStatus),
            RejectedCount = invalid.Count(item => item.ActualStatus == "rejected"),
            BlockedCount = invalid.Count(item => item.ActualStatus == "blocked"),
            Passed = invalid.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            Scenarios = invalid.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> SortDiagnostics(IEnumerable<MultiFamilyGeneratedTemplateDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(item => item.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static void RequireSourceGoals(
        FamilyLifecyclePlan plan,
        ICollection<MultiFamilyGeneratedTemplateDiagnostic> diagnostics)
    {
        foreach (var sourceGoal in new[] { "Goal034", "Goal035", "Goal036", "Goal037", "Goal038", "Goal039", "Goal040" })
        {
            if (!plan.SourceReferences.Any(item => item.SourceGoal == sourceGoal))
            {
                diagnostics.Add(Error("goal043.source." + sourceGoal.ToLowerInvariant() + "_missing", plan.FamilyId, sourceGoal + " source reference is required."));
            }
        }
    }

    private static IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> ValidateBoundary(
        FamilyTemplateBoundaryClaims claims,
        string target)
    {
        var diagnostics = new List<MultiFamilyGeneratedTemplateDiagnostic>();
        if (claims.GamePackageSchemaMutation)
        {
            diagnostics.Add(Error("goal043.boundary.gamepackage_schema.forbidden", target, "Public GamePackage schema mutation is forbidden."));
        }

        if (claims.RuntimeSourceMutation || claims.RuntimeAbstractionsMutation || claims.WinFormsUiMutation || claims.UnitySourceMutation)
        {
            diagnostics.Add(Error("goal043.boundary.runtime_ui_unity.forbidden", target, "Runtime/Runtime.Abstractions/WinForms/Unity source mutation is forbidden."));
        }

        if (claims.ProviderLlmRagMedia)
        {
            diagnostics.Add(Error("goal043.boundary.provider_llm_rag_media.forbidden", target, "Provider/LLM/RAG/media integration is forbidden."));
        }

        if (claims.LuaSourceOrExecutor)
        {
            diagnostics.Add(Error("goal043.boundary.lua_source_executor.forbidden", target, "Lua source or executor integration is forbidden."));
        }

        if (claims.GeneratorLibraryMutation || claims.ExternalDependency || claims.FilesystemNetworkProcessReflectionThreadTimeRandomNativeInterop)
        {
            diagnostics.Add(Error("goal043.boundary.external_or_generator_library.forbidden", target, "Generator-library mutation, external dependency and broad interop surfaces are forbidden."));
        }

        return diagnostics;
    }

    private InvalidFamilyDiagnosticsScenario CatalogInvalid(
        string scenarioId,
        string kind,
        FamilyTemplateCatalog catalog,
        string expectedStatus = "rejected") =>
        Invalid(scenarioId, kind, expectedStatus, ValidateCatalog(catalog).Where(item => item.Severity == "error").ToList());

    private InvalidFamilyDiagnosticsScenario PlanInvalid(
        string scenarioId,
        string kind,
        FamilyLifecyclePlan plan,
        string expectedStatus = "rejected") =>
        Invalid(scenarioId, kind, expectedStatus, ValidatePlan(plan).Where(item => item.Severity == "error").ToList());

    private InvalidFamilyDiagnosticsScenario ProofInvalid(
        string scenarioId,
        string kind,
        FamilySimulatableLoopProof proof,
        string expectedStatus = "rejected") =>
        Invalid(scenarioId, kind, expectedStatus, ValidateProof(proof).Where(item => item.Severity == "error").ToList());

    private InvalidFamilyDiagnosticsScenario PreviewInvalid(
        string scenarioId,
        string kind,
        PreviewExportConsumptionMatrix matrix,
        string expectedStatus = "rejected") =>
        Invalid(scenarioId, kind, expectedStatus, ValidatePreviewExportMatrix(matrix).Where(item => item.Severity == "error").ToList());

    private static InvalidFamilyDiagnosticsScenario Invalid(
        string scenarioId,
        string kind,
        string expectedStatus,
        IReadOnlyList<MultiFamilyGeneratedTemplateDiagnostic> diagnostics)
    {
        var actualStatus = diagnostics.Any(item => item.Code.Contains(".boundary.", StringComparison.Ordinal)
                || item.Code.Contains(".architecture_fork.", StringComparison.Ordinal))
            ? "blocked"
            : diagnostics.Any(item => item.Severity == "error")
                ? "rejected"
                : "accepted";
        return new InvalidFamilyDiagnosticsScenario
        {
            ScenarioId = scenarioId,
            MutatedEvidenceKind = kind,
            ExpectedStatus = expectedStatus,
            ActualStatus = actualStatus,
            ExpectedValid = false,
            ActualValid = actualStatus == "accepted",
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static FamilyLifecyclePlan WithFakeSource(FamilyLifecyclePlan plan, string sourceGoal)
    {
        var refs = plan.SourceReferences
            .Select(item => item.SourceGoal == sourceGoal ? item with { EvidenceRef = "fake_" + sourceGoal.ToLowerInvariant() + "_reference", ArtifactHash = "fake_hash" } : item)
            .ToList();
        var draftRefs = plan.DraftLuaExpansionSourceRefs
            .Select(item => item.SourceGoal == sourceGoal ? item with { EvidenceRef = "fake_" + sourceGoal.ToLowerInvariant() + "_reference", ArtifactHash = "fake_hash" } : item)
            .ToList();
        var traversalRefs = plan.RegionChunkTraversalSourceRefs
            .Select(item => item.SourceGoal == sourceGoal ? item with { EvidenceRef = "fake_" + sourceGoal.ToLowerInvariant() + "_reference", ArtifactHash = "fake_hash" } : item)
            .ToList();
        var previewRefs = plan.PreviewExportConsumerRefs
            .Select(item => item.SourceGoal == sourceGoal ? item with { EvidenceRef = "fake_" + sourceGoal.ToLowerInvariant() + "_reference", ArtifactHash = "fake_hash" } : item)
            .ToList();
        return plan with
        {
            SourceReferences = refs,
            DraftLuaExpansionSourceRefs = draftRefs,
            RegionChunkTraversalSourceRefs = traversalRefs,
            PreviewExportConsumerRefs = previewRefs
        };
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static MultiFamilyGeneratedTemplateDiagnostic Error(string code, string target, string message) =>
        MultiFamilyGeneratedTemplateDiagnostic.Error(code, target, message);
}
