namespace LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;

public sealed class FullGeneratorRepairDiagnosticsPlanner
{
    public FullGeneratorRepairDiagnosticsMatrix BuildMatrix()
    {
        var rows = new List<FullGeneratorRepairDiagnosticRow>
        {
            Row("missing_source_artifact", "goal047.source_artifact.missing", "bounded_restore_or_rerun_source_goal", "Restore or rerun the missing accepted source artifact within its owning goal evidence folder; do not synthesize replacement JSON.", manualRequired: true),
            Row("hash_mismatch", "goal047.source_artifact.hash_mismatch", "bounded_restore_verified_source", "Restore the exact source artifact from a verified repository state or rerun its owning deterministic evidence service.", manualRequired: true),
            Row("missing_family_loop", "goal047.family.loop_missing", "bounded_goal043_regeneration", "Regenerate Goal 043 family loop evidence through the accepted Goal 043 service, then rerun Goal 047.", manualRequired: true),
            Row("missing_runtime_preview_payload", "goal047.runtime_preview.payload_missing", "bounded_goal040_regeneration", "Regenerate Goal 040 preview/export payload evidence through the accepted Goal 040 service, then rerun Goal 047.", manualRequired: true),
            Row("missing_export_profile", "goal047.export_profile.missing", "select_deterministic_profile", "Select the deterministic without-media export profile from family id and Goal 040 export manifest.", manualRequired: false),
            Row("unresolved_profile_capability_ref", "goal047.profile_capability.unresolved", "repair_ref_from_manifest", "Replace unresolved profile/capability refs with refs present in Goal 043 family catalog and source manifest.", manualRequired: false),
            Row("rejected_candidate_provenance", "goal047.provenance.rejected_candidate", "manual_review_required", "Keep candidate quarantined and require manual review before promotion.", manualRequired: true),
            Row("final_prose_leakage", "goal047.boundary.final_prose", "reject_and_strip_candidate", "Reject promoted prose-only content and keep only contract-bound ids, hashes and structured summaries.", manualRequired: false),
            Row("provider_llm_rag_leakage", "goal047.boundary.provider_llm_rag", "manual_required_blocked_boundary", "Block the dry-run and remove any live provider/LLM/RAG call claim before retry.", manualRequired: true),
            Row("media_leakage", "goal047.boundary.media", "manual_required_blocked_boundary", "Block the dry-run and remove media generation or media asset claims before retry.", manualRequired: true),
            Row("unity_runtime_source_mutation_claim", "goal047.boundary.runtime_unity_source", "manual_required_blocked_boundary", "Block the dry-run and split any Runtime/Unity source change into a separate explicit task.", manualRequired: true),
            Row("gamepackage_schema_mutation_claim", "goal047.boundary.gamepackage_schema", "manual_required_blocked_boundary", "Block the dry-run and split public GamePackage schema changes into a separate explicit task.", manualRequired: true),
            Row("nondeterministic_ordering", "goal047.order.nondeterministic", "sort_by_deterministic_key", "Sort families, refs, transitions and matrix rows by stable ordinal keys.", manualRequired: false),
            Row("cross_family_leakage", "goal047.family.cross_leakage", "repair_family_scope", "Reject cross-family refs and rebuild records from the matching family/scenario source mapping.", manualRequired: false)
        };

        return new FullGeneratorRepairDiagnosticsMatrix
        {
            Passed = FullGeneratorWithoutMediaDryRunVocabulary.RequiredRepairDiagnostics
                .All(required => rows.Any(row => row.DiagnosticId == required && !string.IsNullOrWhiteSpace(row.BoundedRepairAction))),
            DiagnosticCount = rows.Count,
            ManualRequiredCount = rows.Count(row => row.ManualRequired),
            BoundedRepairCount = rows.Count(row => !row.ManualRequired),
            Rows = rows.OrderBy(row => row.DiagnosticId, StringComparer.Ordinal).ToList()
        };
    }

    private static FullGeneratorRepairDiagnosticRow Row(
        string diagnosticId,
        string normalizedCode,
        string repairActionKind,
        string boundedRepairAction,
        bool manualRequired) =>
        new()
        {
            DiagnosticId = diagnosticId,
            NormalizedCode = normalizedCode,
            Severity = normalizedCode.Contains(".boundary.", StringComparison.Ordinal) ? "critical" : "error",
            RepairActionKind = repairActionKind,
            BoundedRepairAction = boundedRepairAction,
            ManualRequired = manualRequired,
            MutatesHistoricalArtifacts = false,
            Decision = manualRequired ? "manual_required" : "bounded_repair_available"
        };
}
