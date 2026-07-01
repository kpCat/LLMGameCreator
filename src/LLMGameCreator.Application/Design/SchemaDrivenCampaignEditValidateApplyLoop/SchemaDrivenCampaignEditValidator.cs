namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignEditValidator
{
    private static readonly IReadOnlySet<string> AllowedProvenanceKinds =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "manual_user",
            "deterministic_auto_suggestion"
        };

    public ValidationDiagnosticsMatrix ValidateCandidates(
        CampaignEditSourceBundle source,
        EditableSchemaFieldCatalog fieldCatalog,
        ChangeSetCatalog changeSetCatalog)
    {
        var records = changeSetCatalog.Candidates
            .Select(candidate => ValidateCandidate(source, fieldCatalog, candidate))
            .OrderBy(item => item.CandidateId, StringComparer.Ordinal)
            .ToList();

        return new ValidationDiagnosticsMatrix
        {
            Passed = records.All(record => record.Valid) && records.Count == changeSetCatalog.CandidateCount,
            ValidCandidateCount = records.Count(record => record.Valid),
            RejectedCandidateCount = records.Count(record => !record.Valid),
            Records = records
        };
    }

    public CandidateValidationRecord ValidateCandidate(
        CampaignEditSourceBundle source,
        EditableSchemaFieldCatalog fieldCatalog,
        CampaignChangeSetCandidate candidate)
    {
        var diagnostics = new List<CampaignEditDiagnostic>();
        var row = source.Rows.FirstOrDefault(item => item.RowId == candidate.RowId);
        if (row is null)
        {
            diagnostics.Add(Error(
                "goal075.invalid.unknown_row_id",
                candidate.CandidateId,
                "Change-set candidate references a row outside the Goal 074 workspace."));
        }

        var field = fieldCatalog.Fields.FirstOrDefault(item => item.FieldId == candidate.FieldId);
        if (field is null && candidate.FieldId.StartsWith("quality_debt_panel.", StringComparison.Ordinal))
        {
            diagnostics.Add(Error(
                "goal075.invalid.illegal_field_domain",
                candidate.CandidateId,
                "Quality/debt fields are review-only and cannot be applied as campaign edits."));
        }
        else if (field is null)
        {
            diagnostics.Add(Error(
                "goal075.invalid.unknown_field_id",
                candidate.CandidateId,
                "Change-set candidate references a field outside the editable catalog."));
        }
        else
        {
            ValidateKnownField(candidate, field, diagnostics);
        }

        if (!AllowedProvenanceKinds.Contains(candidate.ProvenanceKind))
        {
            diagnostics.Add(Error(
                "goal075.invalid.fake_provenance",
                candidate.CandidateId,
                "Only manual_user and deterministic_auto_suggestion provenance can apply."));
        }

        if (candidate.CandidateState == "applied" && !candidate.ValidatedBeforeApply)
        {
            diagnostics.Add(Error(
                "goal075.invalid.candidate_as_applied_without_validation",
                candidate.CandidateId,
                "Applied candidates must have validation proof before apply."));
        }

        if (string.IsNullOrWhiteSpace(candidate.RollbackTargetRowId))
        {
            diagnostics.Add(Error(
                "goal075.invalid.rollback_target_missing",
                candidate.CandidateId,
                "Rollback proof needs an explicit target row id."));
        }

        if (!string.IsNullOrWhiteSpace(candidate.ExpectedBeforeHash)
            && candidate.ExpectedBeforeHash == candidate.ExpectedAfterHash)
        {
            diagnostics.Add(Error(
                "goal075.invalid.before_after_hash_unchanged_for_edit",
                candidate.CandidateId,
                "A supposed edit must change the deterministic row hash."));
        }

        if (!string.IsNullOrWhiteSpace(candidate.SourceFamilyId)
            && !string.Equals(candidate.SourceFamilyId, candidate.FamilyId, StringComparison.Ordinal))
        {
            diagnostics.Add(Error(
                "goal075.invalid.cross_family_leakage",
                candidate.CandidateId,
                "A change-set candidate cannot source values from another family row."));
        }

        ValidateClaimTags(candidate, diagnostics);

        if (!candidate.DeterministicOrder)
        {
            diagnostics.Add(Error(
                "goal075.invalid.nondeterministic_ordering",
                candidate.CandidateId,
                "Change-set ordering must be stable and deterministic."));
        }

        if (Path.IsPathFullyQualified(candidate.EvidenceRef))
        {
            diagnostics.Add(Error(
                "goal075.invalid.absolute_path_evidence",
                candidate.CandidateId,
                "Evidence references must remain repository-relative."));
        }

        var valid = diagnostics.Count == 0;
        return new CandidateValidationRecord
        {
            CandidateId = candidate.CandidateId,
            RowId = candidate.RowId,
            FieldId = candidate.FieldId,
            Valid = valid,
            Status = valid ? "accepted_for_apply" : "rejected",
            Diagnostics = SchemaDrivenCampaignEditValidateApplySourceLoader.SortDiagnostics(diagnostics)
        };
    }

    public IReadOnlyList<CampaignEditDiagnostic> ValidateSourceManifest(CampaignEditSourceManifest manifest)
    {
        var diagnostics = new List<CampaignEditDiagnostic>(manifest.Diagnostics.Where(item => item.Severity == "error"));
        Require(manifest.RowCount == 9, "goal075.source.row_count", "sourceManifest.rowCount", diagnostics);
        Require(manifest.FamilyCount == 3, "goal075.source.family_count", "sourceManifest.familyCount", diagnostics);
        Require(manifest.SeedCount == 3, "goal075.source.seed_count", "sourceManifest.seedCount", diagnostics);
        Require(
            manifest.Goal074AcceptedByUserHandoff,
            "goal075.source.goal074_handoff",
            "sourceManifest.goal074AcceptedByUserHandoff",
            diagnostics);
        Require(
            manifest.Goal072RemainsHistoricalBlocked,
            "goal075.source.goal072_blocked",
            "sourceManifest.goal072RemainsHistoricalBlocked",
            diagnostics);
        Require(
            manifest.Goal031And032RemainProducedForReview,
            "goal075.source.goal031_goal032_produced",
            "sourceManifest.goal031And032RemainProducedForReview",
            diagnostics);
        foreach (var artifact in manifest.SourceArtifacts)
        {
            Require(artifact.Exists, "goal075.source.goal074_artifact_missing", artifact.ArtifactRelativePath, diagnostics);
            Require(
                !Path.IsPathFullyQualified(artifact.ArtifactRelativePath),
                "goal075.source.absolute_path",
                artifact.ArtifactRelativePath,
                diagnostics);
        }

        return SchemaDrivenCampaignEditValidateApplySourceLoader.SortDiagnostics(diagnostics);
    }

    public InvalidEditDiagnosticsMatrix BuildInvalidDiagnosticsMatrix(
        CampaignEditSourceBundle source,
        EditableSchemaFieldCatalog fieldCatalog)
    {
        var baseCandidate = BuildInvalidBaseCandidate(source, fieldCatalog);
        var scenarios = new[]
            {
                Scenario("unknown_row_id", baseCandidate with { RowId = "missing-row", CandidateId = "invalid-unknown-row" }),
                Scenario("unknown_field_id", baseCandidate with { FieldId = "unknown.field", CandidateId = "invalid-unknown-field" }),
                Scenario(
                    "illegal_field_domain",
                    baseCandidate with { FieldId = "quality_debt_panel.p0_count", CandidateId = "invalid-illegal-domain" }),
                Scenario(
                    "invalid_value_shape",
                    baseCandidate with
                    {
                        CandidateId = "invalid-value-shape",
                        ProposedValueKind = "object",
                        ProposedValue = "{\"value\":\"high\"}"
                    }),
                Scenario(
                    "unsafe_free_form_prose",
                    baseCandidate with
                    {
                        CandidateId = "invalid-unsafe-prose",
                        ProposedValue = "make this a final dramatic paragraph."
                    }),
                Scenario(
                    "fake_provenance",
                    baseCandidate with { CandidateId = "invalid-fake-provenance", ProvenanceKind = "llm_provider_claim" }),
                Scenario(
                    "candidate_as_applied_without_validation",
                    baseCandidate with
                    {
                        CandidateId = "invalid-applied-without-validation",
                        CandidateState = "applied",
                        ValidatedBeforeApply = false
                    }),
                Scenario(
                    "rollback_target_missing",
                    baseCandidate with { CandidateId = "invalid-rollback-missing", RollbackTargetRowId = string.Empty }),
                Scenario(
                    "before_after_hash_unchanged_for_edit",
                    baseCandidate with
                    {
                        CandidateId = "invalid-unchanged-hash",
                        ExpectedAfterHash = baseCandidate.ExpectedBeforeHash
                    }),
                Scenario(
                    "cross_family_leakage",
                    baseCandidate with { CandidateId = "invalid-cross-family", SourceFamilyId = "survival_sandbox" }),
                Scenario(
                    "llm_provider_rag_media_network_claim",
                    baseCandidate with
                    {
                        CandidateId = "invalid-llm-provider-claim",
                        ClaimTags = ["llm_provider_rag_media_network"]
                    }),
                Scenario(
                    "runtime_gamepackage_ui_broad_mutation_claim",
                    baseCandidate with
                    {
                        CandidateId = "invalid-runtime-gamepackage-ui-claim",
                        ClaimTags = ["runtime_gamepackage_ui_broad_mutation"]
                    }),
                Scenario(
                    "unity_mutation_claim",
                    baseCandidate with { CandidateId = "invalid-unity-claim", ClaimTags = ["unity_mutation"] }),
                Scenario(
                    "lua_generated_code_claim",
                    baseCandidate with { CandidateId = "invalid-lua-generated-code", ClaimTags = ["lua_generated_code"] }),
                Scenario(
                    "nondeterministic_ordering",
                    baseCandidate with { CandidateId = "invalid-nondeterministic-order", DeterministicOrder = false }),
                Scenario(
                    "absolute_path_evidence",
                    baseCandidate with
                    {
                        CandidateId = "invalid-absolute-path",
                        EvidenceRef = "C:/Users/endim/LLMGameCreator/.llmgc/procedural/leak.json"
                    })
            }
            .Select(scenario => ValidateInvalidScenario(source, fieldCatalog, scenario.ScenarioId, scenario.Candidate))
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();

        return new InvalidEditDiagnosticsMatrix
        {
            Passed = scenarios.Count == SchemaDrivenCampaignEditVocabulary.RequiredInvalidScenarioIds.Count
                && scenarios.All(item => item.ActualStatus == "rejected")
                && SchemaDrivenCampaignEditVocabulary.RequiredInvalidScenarioIds.All(
                    id => scenarios.Any(item => item.ScenarioId == id)),
            ScenarioCount = scenarios.Count,
            Scenarios = scenarios
        };
    }

    private static (string ScenarioId, CampaignChangeSetCandidate Candidate) Scenario(
        string scenarioId,
        CampaignChangeSetCandidate candidate) =>
        (scenarioId, candidate);

    private InvalidEditScenarioRecord ValidateInvalidScenario(
        CampaignEditSourceBundle source,
        EditableSchemaFieldCatalog fieldCatalog,
        string scenarioId,
        CampaignChangeSetCandidate candidate)
    {
        var validation = ValidateCandidate(source, fieldCatalog, candidate);
        return new InvalidEditScenarioRecord
        {
            ScenarioId = scenarioId,
            CandidateId = candidate.CandidateId,
            ActualStatus = validation.Valid ? "accepted" : "rejected",
            Diagnostics = validation.Diagnostics
        };
    }

    private static CampaignChangeSetCandidate BuildInvalidBaseCandidate(
        CampaignEditSourceBundle source,
        EditableSchemaFieldCatalog fieldCatalog)
    {
        var row = source.Rows.First();
        var field = fieldCatalog.Fields.First();
        var values = SchemaDrivenCampaignEditCatalog.BuildInitialValues(row, fieldCatalog);
        return new CampaignChangeSetCandidate
        {
            CandidateId = "invalid-base",
            CandidateKind = "manual",
            CandidateState = "candidate",
            ValidatedBeforeApply = true,
            RowId = row.RowId,
            FamilyId = row.FamilyId,
            SeedId = row.SeedId,
            SourceFamilyId = row.FamilyId,
            FieldId = field.FieldId,
            FieldDomain = field.DomainId,
            ProposedValueKind = field.ValueShape,
            BeforeValue = values[field.FieldId],
            ProposedValue = field.AllowedValues.First(value => value != values[field.FieldId]),
            ProvenanceKind = "manual_user",
            EvidenceRef = SchemaDrivenCampaignEditVocabulary.RelativeOutputDirectory + "/invalid-edit-diagnostics-matrix.json",
            RollbackTargetRowId = row.RowId,
            ExpectedBeforeHash = SchemaDrivenCampaignEditCatalog.HashRow(row, values),
            ExpectedAfterHash = SchemaDrivenCampaignEditCatalog.HashRow(row, values.Set(field.FieldId, "medium"))
        };
    }

    private static void ValidateKnownField(
        CampaignChangeSetCandidate candidate,
        EditableSchemaField field,
        ICollection<CampaignEditDiagnostic> diagnostics)
    {
        if (!field.Editable)
        {
            diagnostics.Add(Error(
                "goal075.invalid.illegal_field_domain",
                candidate.CandidateId,
                "The referenced field is not editable in Goal 075."));
        }

        if (!string.Equals(candidate.ProposedValueKind, field.ValueShape, StringComparison.Ordinal))
        {
            diagnostics.Add(Error(
                "goal075.invalid.invalid_value_shape",
                candidate.CandidateId,
                "The proposed value shape does not match the editable field catalog."));
        }

        if (!field.AllowedValues.Contains(candidate.ProposedValue, StringComparer.Ordinal))
        {
            diagnostics.Add(Error(
                "goal075.invalid.invalid_value_shape",
                candidate.CandidateId,
                "The proposed value is outside the allowed deterministic domain."));
        }

        if (ContainsUnsafeProse(candidate.ProposedValue))
        {
            diagnostics.Add(Error(
                "goal075.invalid.unsafe_free_form_prose",
                candidate.CandidateId,
                "Free-form prose cannot become authoritative campaign content."));
        }
    }

    private static void ValidateClaimTags(
        CampaignChangeSetCandidate candidate,
        ICollection<CampaignEditDiagnostic> diagnostics)
    {
        foreach (var tag in candidate.ClaimTags)
        {
            var code = tag switch
            {
                "llm_provider_rag_media_network" => "goal075.invalid.llm_provider_rag_media_network_claim",
                "runtime_gamepackage_ui_broad_mutation" => "goal075.invalid.runtime_gamepackage_ui_broad_mutation_claim",
                "unity_mutation" => "goal075.invalid.unity_mutation_claim",
                "lua_generated_code" => "goal075.invalid.lua_generated_code_claim",
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(code))
            {
                diagnostics.Add(Error(code, candidate.CandidateId, "Forbidden mutation or provider claim rejected."));
            }
        }
    }

    private static bool ContainsUnsafeProse(string value) =>
        value.Length > 24
        || value.Contains(' ', StringComparison.Ordinal)
        || value.Contains('.', StringComparison.Ordinal)
        || value.Contains('!', StringComparison.Ordinal)
        || value.Contains('\n', StringComparison.Ordinal);

    private static void Require(
        bool condition,
        string code,
        string target,
        ICollection<CampaignEditDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(Error(code, target, "Validation rule failed."));
        }
    }

    private static CampaignEditDiagnostic Error(string code, string target, string message) =>
        CampaignEditDiagnostic.Error(code, target, message);
}
