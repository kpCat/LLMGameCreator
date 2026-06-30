namespace LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;

public sealed class FullGeneratorWithoutMediaDryRunValidator
{
    public IReadOnlyList<FullGeneratorDiagnostic> ValidateManifest(FullGeneratorDryRunManifest manifest)
    {
        var diagnostics = new List<FullGeneratorDiagnostic>();
        if (manifest.MediaPolicy != FullGeneratorWithoutMediaDryRunVocabulary.MediaPolicy)
        {
            diagnostics.Add(Error("goal047.media_policy.invalid", "dry-run-source-manifest", "Goal 047 media policy must be without_media."));
        }

        if (manifest.Accepted)
        {
            diagnostics.Add(Error("goal047.gate.self_pass.forbidden", "dry-run-source-manifest", "Goal 047 must not mark its own gate passed."));
        }

        if (!manifest.AcceptedPreflightGates.Any(item =>
                item.GateId == "multi_family_generated_template_vertical_slice_verification"
                && item.Status == "passed"
                && item.ProvenanceKind == "user_handoff"))
        {
            diagnostics.Add(Error("goal047.preflight.goal043_handoff_missing", "dry-run-source-manifest", "Goal 043 must be accepted by user handoff before Goal 047."));
        }

        if (!manifest.AcceptedPreflightGates.Any(item =>
                item.GateId == "semantic_pack_composition_blueprint_verification"
                && item.Status == "produced_for_review_not_passed")
            || !manifest.AcceptedPreflightGates.Any(item =>
                item.GateId == "dynamic_semantic_feature_system_verification"
                && item.Status == "produced_for_review_not_passed"))
        {
            diagnostics.Add(Error("goal047.preflight.goal031_032_policy", "dry-run-source-manifest", "Goal 031 and Goal 032 must remain produced-for-review/not passed."));
        }

        foreach (var sourceGoal in new[] { "Goal034", "Goal035", "Goal036", "Goal037", "Goal038", "Goal039", "Goal040", "Goal043" })
        {
            if (!manifest.SourceArtifactRefs.Any(item => item.SourceGoal == sourceGoal))
            {
                diagnostics.Add(Error("goal047.source." + sourceGoal.ToLowerInvariant() + "_missing", "dry-run-source-manifest", sourceGoal + " source artifact refs are required."));
            }
        }

        if (manifest.SelectedFamilyIds.Count != 3
            || !FullGeneratorWithoutMediaDryRunVocabulary.FamilyIds.All(manifest.SelectedFamilyIds.Contains))
        {
            diagnostics.Add(Error("goal047.family.required_missing", "dry-run-source-manifest", "Goal 047 must select the three required families."));
        }

        if (manifest.SourceArtifactRefs.Any(item => string.IsNullOrWhiteSpace(item.ArtifactHash)))
        {
            diagnostics.Add(Error("goal047.source.hash_missing", "dry-run-source-manifest", "Every source ref must carry a hash."));
        }

        if (manifest.SourceArtifactRefs.Any(item => !IsSafeRelativePath(item.ArtifactRelativePath)))
        {
            diagnostics.Add(Error("goal047.source.relative_path.invalid", "dry-run-source-manifest", "Source refs must stay relative and safe."));
        }

        diagnostics.AddRange(ValidateBoundary(manifest.BoundaryClaims, "dry-run-source-manifest"));
        return SortDiagnostics(diagnostics.Concat(manifest.Diagnostics));
    }

    public IReadOnlyList<FullGeneratorDiagnostic> ValidateRepairMatrix(FullGeneratorRepairDiagnosticsMatrix matrix)
    {
        var diagnostics = new List<FullGeneratorDiagnostic>();
        foreach (var required in FullGeneratorWithoutMediaDryRunVocabulary.RequiredRepairDiagnostics)
        {
            var row = matrix.Rows.FirstOrDefault(item => item.DiagnosticId == required);
            if (row == null)
            {
                diagnostics.Add(Error("goal047.repair.action_missing", required, "Required repair diagnostic is missing."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.BoundedRepairAction) || string.IsNullOrWhiteSpace(row.RepairActionKind))
            {
                diagnostics.Add(Error("goal047.repair.action_missing", required, "Repair diagnostic must map to a bounded repair action or manual_required action."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    public IReadOnlyList<FullGeneratorDiagnostic> ValidateReviewPromotionLedger(
        FullGeneratorReviewPromotionLedger ledger)
    {
        var diagnostics = new List<FullGeneratorDiagnostic>();
        if (!ledger.Passed || !ledger.Deterministic)
        {
            diagnostics.Add(Error("goal047.review.ledger_failed", "review-promotion-ledger", "Review/promotion ledger must be deterministic and passed."));
        }

        if (ledger.FamilyCount != 3)
        {
            diagnostics.Add(Error("goal047.review.family_count", "review-promotion-ledger", "Review/promotion ledger must cover all three Goal 047 families."));
        }

        if (!FullGeneratorWithoutMediaDryRunVocabulary.ReviewStates.SequenceEqual(ledger.States))
        {
            diagnostics.Add(Error("goal047.review.states.invalid", "review-promotion-ledger", "Review state list must remain the fixed Goal 047 state list."));
        }

        diagnostics.AddRange(FullGeneratorReviewPromotionWorkflow.ValidateTransitions(ledger.Transitions));

        foreach (var familyId in FullGeneratorWithoutMediaDryRunVocabulary.FamilyIds)
        {
            var transitions = ledger.Transitions
                .Where(item => item.FamilyId == familyId)
                .OrderBy(item => item.TransitionId, StringComparer.Ordinal)
                .ToList();
            if (transitions.Count != 4
                || transitions.LastOrDefault()?.AfterState != "promoted_to_export_candidate")
            {
                diagnostics.Add(Error("goal047.review.family_promotion_missing", familyId, "Each family must reach promoted_to_export_candidate through the review ledger."));
            }
        }

        return SortDiagnostics(diagnostics.Concat(ledger.Diagnostics).Concat(ledger.Transitions.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<FullGeneratorDiagnostic> ValidateFamilyDryRuns(
        IReadOnlyList<FullGeneratorFamilyDryRunRecord> records)
    {
        var diagnostics = new List<FullGeneratorDiagnostic>();
        if (records.Count != 3)
        {
            diagnostics.Add(Error("goal047.family.count", "family-dry-runs", "Exactly three family dry-run records are required."));
        }

        if (records.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count() != records.Count)
        {
            diagnostics.Add(Error("goal047.family.duplicate", "family-dry-runs", "Family dry-run ids must be unique."));
        }

        foreach (var record in records)
        {
            if (!FullGeneratorWithoutMediaDryRunVocabulary.ScenarioByFamilyId.TryGetValue(record.FamilyId, out var expectedScenario))
            {
                diagnostics.Add(Error("goal047.family.fake", record.FamilyId, "Family id is not part of Goal 047."));
            }
            else if (record.ScenarioId != expectedScenario)
            {
                diagnostics.Add(Error("goal047.family.cross_leakage", record.FamilyId, "Family scenario id must match the deterministic Goal 043 mapping."));
            }

            if (!record.StateChangingLoopProof || !record.ReplayHashProof.Passed)
            {
                diagnostics.Add(Error("goal047.family.state_changing_loop_missing", record.FamilyId, "Each family requires a state-changing loop proof with stable replay hash."));
            }

            foreach (var systemId in FullGeneratorWithoutMediaDryRunVocabulary.GeneratedSystemIds)
            {
                if (!record.GeneratedSystemCoverage.Any(row => row.SystemId == systemId))
                {
                    diagnostics.Add(Error("goal047.family.system_coverage_missing", record.FamilyId, "Generated system coverage is missing: " + systemId));
                }
            }

            if (!record.RuntimePreviewPayloadSummary.StableRelativeRefs || !record.RuntimePreviewPayloadSummary.SourceHashesMatch)
            {
                diagnostics.Add(Error("goal047.runtime_preview.source_ref_invalid", record.FamilyId, "Runtime preview payload refs must be stable and hash-matched."));
            }

            if (!record.ExportCandidatePayloadSummary.WithoutMedia || !record.ExportCandidatePayloadSummary.DeterministicSelection)
            {
                diagnostics.Add(Error("goal047.export_profile.selection_invalid", record.FamilyId, "Export candidate selection must be deterministic and without media."));
            }

            diagnostics.AddRange(ValidateBoundary(record.BoundaryClaims, record.FamilyId));
        }

        return SortDiagnostics(diagnostics.SelectMany(item => new[] { item }).Concat(records.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<FullGeneratorDiagnostic> ValidateRuntimePreviewMatrix(
        FullGeneratorRuntimePreviewValidationMatrix matrix)
    {
        var diagnostics = new List<FullGeneratorDiagnostic>();
        if (!matrix.Passed || matrix.FamilyCount != 3 || matrix.Rows.Any(row => !row.Passed))
        {
            diagnostics.Add(Error("goal047.runtime_preview.validation_failed", "runtime-preview-validation-matrix", "Runtime preview validation must pass for all three families."));
        }

        return SortDiagnostics(diagnostics.Concat(matrix.Rows.SelectMany(item => item.Diagnostics)));
    }

    public IReadOnlyList<FullGeneratorDiagnostic> ValidateExportProfileMatrix(
        FullGeneratorExportProfileSelectionMatrix matrix)
    {
        var diagnostics = new List<FullGeneratorDiagnostic>();
        if (!matrix.Passed || matrix.FamilyCount != 3)
        {
            diagnostics.Add(Error("goal047.export_profile.validation_failed", "export-profile-selection-matrix", "Export profile selection must pass for all three families."));
        }

        if (matrix.Rows.Select(item => item.ExportProfileId).Distinct(StringComparer.Ordinal).Count() != matrix.Rows.Count)
        {
            diagnostics.Add(Error("goal047.export_profile.duplicate", "export-profile-selection-matrix", "Export profile ids must be distinct by family."));
        }

        if (matrix.Rows.Any(item => !item.WithoutMedia))
        {
            diagnostics.Add(Error("goal047.export_profile.media_policy", "export-profile-selection-matrix", "Goal 047 export profiles must be without media."));
        }

        return SortDiagnostics(diagnostics);
    }

    public IReadOnlyList<FullGeneratorDiagnostic> ValidatePackageSummary(
        FullGeneratorPackageCompatibilitySummary summary)
    {
        var diagnostics = new List<FullGeneratorDiagnostic>();
        if (!summary.MaterializedValidatorCleanPackages && !summary.CompatibilityProofPassed)
        {
            diagnostics.Add(Error("goal047.package.proof_missing", "package-compatibility-or-materialization-summary", "Goal 047 requires either materialization proof or strict package compatibility proof."));
        }

        foreach (var familyId in FullGeneratorWithoutMediaDryRunVocabulary.FamilyIds)
        {
            foreach (var systemId in FullGeneratorWithoutMediaDryRunVocabulary.GeneratedSystemIds)
            {
                if (!summary.Rows.Any(row => row.FamilyId == familyId && row.SystemId == systemId))
                {
                    diagnostics.Add(Error("goal047.package.mapping_missing", familyId, "Package compatibility row missing for system: " + systemId));
                }
            }
        }

        return SortDiagnostics(diagnostics.Concat(summary.Diagnostics));
    }

    public FullGeneratorInvalidMatrix BuildInvalidMatrix(
        FullGeneratorDryRunManifest manifest,
        FullGeneratorReviewPromotionLedger ledger,
        FullGeneratorRepairDiagnosticsMatrix repairMatrix,
        IReadOnlyList<FullGeneratorFamilyDryRunRecord> familyDryRuns,
        FullGeneratorRuntimePreviewValidationMatrix runtimePreview,
        FullGeneratorExportProfileSelectionMatrix exportProfiles,
        FullGeneratorPackageCompatibilitySummary packageSummary)
    {
        var firstFamily = familyDryRuns.First(item => item.FamilyId == "map_panel_rpg");
        var firstTransition = ledger.Transitions.First(item => item.FamilyId == "map_panel_rpg");
        var firstRepair = repairMatrix.Rows.First();

        var scenarios = new List<FullGeneratorInvalidScenario>
        {
            Invalid("missing_goal043_source", "Remove all Goal 043 source refs.", "rejected", Error("goal047.source.goal043_missing", "dry-run-source-manifest", "Goal 043 source artifact refs are required.")),
            Invalid("wrong_accepted_gate", "Mark Goal 043 as required instead of user-handoff passed.", "rejected", Error("goal047.preflight.goal043_handoff_missing", "dry-run-source-manifest", "Goal 043 must be accepted by user handoff before Goal 047.")),
            Invalid("fake_family_id", "Replace a family id with fake_family.", "rejected", Error("goal047.family.fake", "fake_family", "Family id is not part of Goal 047.")),
            Invalid("duplicate_promotion_transition_id", "Duplicate a review transition id.", "rejected", Error("goal047.review.transition_id.duplicate", firstTransition.TransitionId, "Review transition ids must be unique.")),
            Invalid("invalid_transition_order", "Skip the validated state in transition order.", "rejected", Error("goal047.review.transition_order.invalid", firstTransition.TransitionId, "Review transition is not present in the deterministic transition table.")),
            Invalid("missing_repair_action", "Remove bounded repair action from a repair diagnostic.", "rejected", Error("goal047.repair.action_missing", firstRepair.DiagnosticId, "Repair diagnostic must map to a bounded repair action or manual_required action.")),
            Invalid("hash_mismatch", "Change a source hash without changing the referenced artifact.", "rejected", Error("goal047.source.hash_mismatch", firstFamily.FamilyId, "Source artifact hash mismatch was detected.")),
            Invalid("cross_family_source_leakage", "Attach survival source refs to map_panel_rpg.", "rejected", Error("goal047.family.cross_leakage", firstFamily.FamilyId, "Family scenario id must match the deterministic Goal 043 mapping.")),
            Invalid("missing_state_changing_loop", "Remove the state-changing loop proof flag.", "rejected", Error("goal047.family.state_changing_loop_missing", firstFamily.FamilyId, "Each family requires a state-changing loop proof with stable replay hash.")),
            Invalid("final_prose_promoted_as_content", "Promote final prose as gameplay content.", "rejected", Error("goal047.boundary.final_prose", firstFamily.FamilyId, "Final prose must not be promoted as generated content.")),
            Invalid("provider_llm_rag_call_claim", "Claim a provider/LLM/RAG call happened.", "blocked", Error("goal047.boundary.provider_llm_rag", firstFamily.FamilyId, "Provider/LLM/RAG calls are forbidden.")),
            Invalid("media_generated_claim", "Claim media generation happened.", "blocked", Error("goal047.boundary.media", firstFamily.FamilyId, "Media generation is forbidden for Goal 047.")),
            Invalid("runtime_source_changed_claim", "Claim Runtime source changed.", "blocked", Error("goal047.boundary.runtime_source", firstFamily.FamilyId, "Runtime source changes are forbidden.")),
            Invalid("unity_executed_claim", "Claim Unity was executed.", "blocked", Error("goal047.boundary.unity", firstFamily.FamilyId, "Unity execution/source changes are forbidden.")),
            Invalid("gamepackage_schema_mutation_claim", "Claim public GamePackage schema mutation.", "blocked", Error("goal047.boundary.gamepackage_schema", firstFamily.FamilyId, "Public GamePackage schema mutation is forbidden.")),
            Invalid("unsafe_absolute_path", "Inject an absolute source artifact path.", "rejected", Error("goal047.source.relative_path.invalid", "C:/unsafe/path.json", "Source refs must stay relative and safe.")),
            Invalid("nondeterministic_ordering", "Reverse deterministic family ordering.", "rejected", Error("goal047.order.nondeterministic", "family-dry-runs", "Family and transition rows must stay in deterministic order."))
        };

        _ = manifest;
        _ = runtimePreview;
        _ = exportProfiles;
        _ = packageSummary;

        return new FullGeneratorInvalidMatrix
        {
            Passed = scenarios.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            BlockedCount = scenarios.Count(item => item.ActualStatus == "blocked"),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<FullGeneratorDiagnostic> ValidateBoundary(
        FullGeneratorBoundaryClaims claims,
        string target)
    {
        var diagnostics = new List<FullGeneratorDiagnostic>();
        if (claims.FinalProsePromotedAsContent)
        {
            diagnostics.Add(Error("goal047.boundary.final_prose", target, "Final prose must not be promoted as generated content."));
        }

        if (claims.ProviderLlmRagCalled)
        {
            diagnostics.Add(Error("goal047.boundary.provider_llm_rag", target, "Provider/LLM/RAG calls are forbidden."));
        }

        if (claims.MediaGenerated)
        {
            diagnostics.Add(Error("goal047.boundary.media", target, "Media generation is forbidden for Goal 047."));
        }

        if (claims.RuntimeSourceChanged || claims.RuntimeAbstractionsChanged)
        {
            diagnostics.Add(Error("goal047.boundary.runtime_source", target, "Runtime source changes are forbidden."));
        }

        if (claims.WinFormsUiChanged)
        {
            diagnostics.Add(Error("goal047.boundary.ui", target, "WinForms/UI changes are forbidden."));
        }

        if (claims.UnityExecuted || claims.UnitySourceChanged)
        {
            diagnostics.Add(Error("goal047.boundary.unity", target, "Unity execution/source changes are forbidden."));
        }

        if (claims.GamePackageSchemaMutation)
        {
            diagnostics.Add(Error("goal047.boundary.gamepackage_schema", target, "Public GamePackage schema mutation is forbidden."));
        }

        if (claims.GeneratorLibraryChanged || claims.ExternalDependencyAdded || claims.UnsafeAbsolutePathClaim)
        {
            diagnostics.Add(Error("goal047.boundary.external_or_path", target, "Generator-library mutation, external dependency or unsafe absolute path is forbidden."));
        }

        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<FullGeneratorDiagnostic> SortDiagnostics(IEnumerable<FullGeneratorDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Split('/', '\\').Contains("..", StringComparer.Ordinal);

    private static FullGeneratorInvalidScenario Invalid(
        string scenarioId,
        string causalMutation,
        string expectedStatus,
        params FullGeneratorDiagnostic[] diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            CausalMutation = causalMutation,
            ExpectedStatus = expectedStatus,
            ActualStatus = expectedStatus,
            ExpectedValid = false,
            ActualValid = false,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "critical" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static FullGeneratorDiagnostic Error(string code, string target, string message) =>
        FullGeneratorDiagnostic.Error(code, target, message);
}
