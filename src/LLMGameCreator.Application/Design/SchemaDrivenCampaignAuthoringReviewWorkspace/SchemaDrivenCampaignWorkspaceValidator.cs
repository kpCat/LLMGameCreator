namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

public sealed class SchemaDrivenCampaignWorkspaceValidator
{
    public IReadOnlyList<CampaignWorkspaceDiagnostic> ValidateSourceManifest(
        CampaignWorkspaceSourceManifest manifest)
    {
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        diagnostics.AddRange(manifest.Diagnostics.Where(item => item.Severity == "error"));
        Require(manifest.RowCount == 9, "goal074.source_manifest.row_count", "sourceManifest.rowCount", diagnostics);
        Require(manifest.FamilyCount == 3, "goal074.source_manifest.family_count", "sourceManifest.familyCount", diagnostics);
        Require(manifest.SeedCount == 3, "goal074.source_manifest.seed_count", "sourceManifest.seedCount", diagnostics);
        Require(
            manifest.Goal073AcceptedByUserHandoff,
            "goal074.source_manifest.goal073_handoff",
            "sourceManifest.goal073AcceptedByUserHandoff",
            diagnostics);
        Require(
            manifest.Goal072RemainsHistoricalBlocked,
            "goal074.source_manifest.goal072_blocked",
            "sourceManifest.goal072RemainsHistoricalBlocked",
            diagnostics);
        Require(
            manifest.Goal031And032RemainProducedForReview,
            "goal074.source_manifest.goal031_goal032_produced",
            "sourceManifest.goal031And032RemainProducedForReview",
            diagnostics);
        foreach (var artifact in manifest.SourceArtifactRefs)
        {
            Require(artifact.Exists, "goal074.source_manifest.missing_source", artifact.ArtifactRelativePath, diagnostics);
            Require(
                !Path.IsPathFullyQualified(artifact.ArtifactRelativePath),
                "goal074.source_manifest.absolute_path",
                artifact.ArtifactRelativePath,
                diagnostics);
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CampaignWorkspaceDiagnostic> ValidateRowSelector(CampaignRowSelector selector)
    {
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        Require(selector.RowCount == 9, "goal074.row_selector.row_count", "rowSelector.rowCount", diagnostics);
        Require(selector.FamilyCount == 3, "goal074.row_selector.family_count", "rowSelector.familyCount", diagnostics);
        Require(selector.SeedCount == 3, "goal074.row_selector.seed_count", "rowSelector.seedCount", diagnostics);
        Require(
            selector.Rows.Select(row => row.RowId).Distinct(StringComparer.Ordinal).Count() == selector.Rows.Count,
            "goal074.row_selector.duplicate_row_id",
            "rowSelector.rows",
            diagnostics);

        var expectedPairs = SchemaDrivenCampaignWorkspaceVocabulary.FamilyIds
            .SelectMany(family => SchemaDrivenCampaignWorkspaceVocabulary.SeedIds.Select(seed => family + "/" + seed))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var actualPairs = selector.Rows
            .Select(row => row.FamilyId + "/" + row.SeedId)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        Require(
            expectedPairs.SequenceEqual(actualPairs, StringComparer.Ordinal),
            "goal074.row_selector.missing_family_seed_pair",
            "rowSelector.rows",
            diagnostics);
        foreach (var row in selector.Rows)
        {
            Require(
                SchemaDrivenCampaignWorkspaceVocabulary.FamilyIds.Contains(row.FamilyId),
                "goal074.row_selector.unknown_family",
                row.RowId,
                diagnostics);
            Require(
                SchemaDrivenCampaignWorkspaceVocabulary.SeedIds.Contains(row.SeedId),
                "goal074.row_selector.unknown_seed",
                row.RowId,
                diagnostics);
            Require(row.StateChanging, "goal074.row_selector.non_state_changing_row", row.RowId, diagnostics);
            Require(row.SaveLoadReplayPassed, "goal074.row_selector.save_load_replay_failed", row.RowId, diagnostics);
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CampaignWorkspaceDiagnostic> ValidateSchema(CampaignAuthoringSchema schema)
    {
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        var groupIds = schema.Groups.Select(group => group.GroupId).ToList();
        Require(
            SchemaDrivenCampaignWorkspaceVocabulary.RequiredSchemaGroupIds.All(groupIds.Contains),
            "goal074.schema.required_group_missing",
            "dynamicSchema.groups",
            diagnostics);
        Require(
            groupIds.Distinct(StringComparer.Ordinal).Count() == groupIds.Count,
            "goal074.schema.duplicate_group",
            "dynamicSchema.groups",
            diagnostics);
        foreach (var group in schema.Groups)
        {
            Require(group.Fields.Count > 0, "goal074.schema.empty_group", group.GroupId, diagnostics);
            Require(
                group.Fields.Select(field => field.FieldId).Distinct(StringComparer.Ordinal).Count() == group.Fields.Count,
                "goal074.schema.duplicate_field",
                group.GroupId,
                diagnostics);
            foreach (var field in group.Fields)
            {
                Require(!string.IsNullOrWhiteSpace(field.FieldId), "goal074.schema.empty_field", group.GroupId, diagnostics);
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CampaignWorkspaceDiagnostic> ValidateUiBinding(
        CampaignAuthoringSchema schema,
        CampaignUiBindingContract binding)
    {
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        var schemaFields = schema.Groups.ToDictionary(
            group => group.GroupId,
            group => group.Fields.Select(field => field.FieldId).ToHashSet(StringComparer.Ordinal),
            StringComparer.Ordinal);
        foreach (var groupBinding in binding.GroupBindings)
        {
            Require(
                schemaFields.ContainsKey(groupBinding.GroupId),
                "goal074.ui_binding.unknown_group",
                groupBinding.GroupId,
                diagnostics);
            if (!schemaFields.TryGetValue(groupBinding.GroupId, out var fieldIds))
            {
                continue;
            }

            foreach (var fieldBinding in groupBinding.FieldBindings)
            {
                Require(
                    fieldIds.Contains(fieldBinding.FieldId),
                    "goal074.ui_binding.unknown_field",
                    groupBinding.GroupId + "/" + fieldBinding.FieldId,
                    diagnostics);
            }
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CampaignWorkspaceDiagnostic> ValidateProvenance(ReviewProvenanceLedger ledger)
    {
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        foreach (var category in new[] { "manual", "auto", "quarantined", "accepted" })
        {
            Require(ledger.Categories.Contains(category), "goal074.provenance.missing_category", category, diagnostics);
        }

        foreach (var entry in ledger.Entries.Where(entry => entry.Category == "accepted"))
        {
            Require(
                entry.HasReviewProvenance,
                "goal074.provenance.accepted_without_review",
                entry.EntryId,
                diagnostics);
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CampaignWorkspaceDiagnostic> ValidateActionPlan(
        CampaignAuthoringSchema schema,
        AuthoringActionPlan actionPlan)
    {
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        var groupIds = schema.Groups.Select(group => group.GroupId).ToHashSet(StringComparer.Ordinal);
        var ordered = actionPlan.Items.OrderBy(item => item.Order).ThenBy(item => item.ActionId, StringComparer.Ordinal);
        Require(
            actionPlan.Items.SequenceEqual(ordered),
            "goal074.action_plan.nondeterministic_order",
            "authoringActionPlan.items",
            diagnostics);
        foreach (var item in actionPlan.Items)
        {
            Require(groupIds.Contains(item.SchemaGroupId), "goal074.action_plan.unknown_group", item.ActionId, diagnostics);
            Require(item.Deterministic, "goal074.action_plan.nondeterministic_item", item.ActionId, diagnostics);
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CampaignWorkspaceDiagnostic> ValidateInvalidMatrix(
        CampaignInvalidDiagnosticsMatrix matrix)
    {
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        var ids = matrix.Scenarios.Select(scenario => scenario.ScenarioId).ToHashSet(StringComparer.Ordinal);
        foreach (var scenarioId in SchemaDrivenCampaignWorkspaceVocabulary.RequiredInvalidScenarioIds)
        {
            Require(ids.Contains(scenarioId), "goal074.invalid_matrix.missing_scenario", scenarioId, diagnostics);
        }

        foreach (var scenario in matrix.Scenarios)
        {
            Require(
                scenario.ActualStatus == "rejected",
                "goal074.invalid_matrix.not_rejected",
                scenario.ScenarioId,
                diagnostics);
            Require(
                scenario.Diagnostics.Any(item => item.Code == "goal074.invalid." + scenario.ScenarioId),
                "goal074.invalid_matrix.missing_diagnostic",
                scenario.ScenarioId,
                diagnostics);
        }

        return Sort(diagnostics);
    }

    public IReadOnlyList<CampaignWorkspaceDiagnostic> ValidateQualityGate(QualityGateScan scan)
    {
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        diagnostics.AddRange(scan.Diagnostics.Where(item => item.Severity == "error"));
        Require(scan.Passed, "goal074.quality_gate.failed", "qualityGateScan", diagnostics);
        return Sort(diagnostics);
    }

    public IReadOnlyList<CampaignWorkspaceDiagnostic> ValidateWinFormsInventory(WinFormsControlInventory inventory)
    {
        var diagnostics = new List<CampaignWorkspaceDiagnostic>();
        Require(inventory.NavigationRegistered, "goal074.winforms.navigation_missing", "CompositionRoot.cs", diagnostics);
        Require(inventory.Controls.Count >= 7, "goal074.winforms.control_count", "winformsControlInventory", diagnostics);
        foreach (var control in inventory.Controls)
        {
            Require(control.SeparateUserControl, "goal074.winforms.not_user_control", control.ControlName, diagnostics);
            Require(control.SchemaDrivenBinding, "goal074.winforms.not_schema_driven", control.ControlName, diagnostics);
        }

        return Sort(diagnostics);
    }

    private static void Require(
        bool condition,
        string code,
        string target,
        ICollection<CampaignWorkspaceDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(CampaignWorkspaceDiagnostic.Error(code, target, "Validation rule failed."));
        }
    }

    private static IReadOnlyList<CampaignWorkspaceDiagnostic> Sort(IEnumerable<CampaignWorkspaceDiagnostic> diagnostics) =>
        SchemaDrivenCampaignWorkspaceSourceLoader.SortDiagnostics(diagnostics);
}
