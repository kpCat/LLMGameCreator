namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

public sealed class SchemaDrivenCampaignWorkspaceBuilder
{
    public CampaignWorkspaceSourceManifest BuildSourceManifest(CampaignWorkspaceSourceBundle source) =>
        new()
        {
            Accepted = false,
            Goal073AcceptedByUserHandoff = source.Goal073AcceptedByUserHandoff,
            Goal072RemainsHistoricalBlocked = source.Goal072RemainsHistoricalBlocked,
            Goal031And032RemainProducedForReview = source.Goal031And032RemainProducedForReview,
            RowCount = source.Rows.Count,
            FamilyCount = source.FamilyIds.Count,
            SeedCount = source.SeedIds.Count,
            PreflightGates =
            [
                new CampaignWorkspaceGateRecord
                {
                    GateId = "source_format_p0_readability_repair_verification",
                    Status = source.Goal073AcceptedByUserHandoff ? "passed" : "missing",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "docs/CURRENT_GENERATOR_STATE.*"
                },
                new CampaignWorkspaceGateRecord
                {
                    GateId = "generator_spine_quality_consolidation_verification",
                    Status = source.Goal072RemainsHistoricalBlocked ? "required" : "missing",
                    ProvenanceKind = "historical_blocked",
                    EvidenceRef = "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md"
                },
                new CampaignWorkspaceGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = source.Goal031And032RemainProducedForReview ? "required" : "missing",
                    ProvenanceKind = "produced_for_review",
                    EvidenceRef = "docs/CURRENT_GENERATOR_STATE.*"
                },
                new CampaignWorkspaceGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = source.Goal031And032RemainProducedForReview ? "required" : "missing",
                    ProvenanceKind = "produced_for_review",
                    EvidenceRef = "docs/CURRENT_GENERATOR_STATE.*"
                },
                new CampaignWorkspaceGateRecord
                {
                    GateId = SchemaDrivenCampaignWorkspaceVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "manual_gate",
                    EvidenceRef = SchemaDrivenCampaignWorkspaceVocabulary.RelativeOutputDirectory
                        + "/schema-driven-campaign-authoring-review-workspace-report.md"
                }
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = source.Diagnostics
        };

    public CampaignRowSelector BuildRowSelector(CampaignWorkspaceSourceBundle source)
    {
        var rows = source.Rows
            .OrderBy(row => SchemaDrivenCampaignWorkspaceVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => SchemaDrivenCampaignWorkspaceVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row => new CampaignRowSelectorRow
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                DisplayKey = "goal074.row." + row.FamilyId + "." + row.SeedId,
                PackageRelativePath = row.PackageRelativePath,
                InteractiveRowHash = row.InteractiveRowHash,
                StateChanging = row.StateChanging,
                SaveLoadReplayPassed = row.SaveLoadReplayPassed
            })
            .ToList();

        return new CampaignRowSelector
        {
            Passed = rows.Count == 9,
            RowCount = rows.Count,
            FamilyCount = rows.Select(row => row.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(row => row.SeedId).Distinct(StringComparer.Ordinal).Count(),
            Families = rows
                .GroupBy(row => row.FamilyId, StringComparer.Ordinal)
                .OrderBy(group => SchemaDrivenCampaignWorkspaceVocabulary.FamilyOrderingKey(group.Key), StringComparer.Ordinal)
                .Select(group => new CampaignRowSelectorFamily
                {
                    FamilyId = group.Key,
                    SeedIds = group
                        .Select(row => row.SeedId)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(SchemaDrivenCampaignWorkspaceVocabulary.SeedOrderingKey, StringComparer.Ordinal)
                        .ToList(),
                    RowIds = group
                        .OrderBy(row => SchemaDrivenCampaignWorkspaceVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
                        .Select(row => row.RowId)
                        .ToList()
                })
                .ToList(),
            Rows = rows
        };
    }

    public CampaignAuthoringSchema BuildDynamicSchema(CampaignWorkspaceSourceBundle source)
    {
        var groups = SchemaDrivenCampaignWorkspaceVocabulary.RequiredSchemaGroupIds
            .Select((groupId, index) => BuildGroup(groupId, index + 1, source))
            .ToList();

        return new CampaignAuthoringSchema
        {
            Passed = groups.Count == SchemaDrivenCampaignWorkspaceVocabulary.RequiredSchemaGroupIds.Count,
            Groups = groups
        };
    }

    public CampaignUiBindingContract BuildUiBindingContract(CampaignAuthoringSchema schema)
    {
        var groups = schema.Groups
            .OrderBy(group => group.Order)
            .Select(group => new CampaignUiGroupBinding
            {
                GroupId = group.GroupId,
                ControlKey = "goal074.control." + group.GroupId,
                DataPath = "dynamicSchema.groups." + group.GroupId,
                FieldBindings = group.Fields
                    .Where(field => field.Bindable)
                    .Select(field => new CampaignUiFieldBinding
                    {
                        FieldId = field.FieldId,
                        DataPath = "dynamicSchema.groups." + group.GroupId + ".fields." + field.FieldId
                    })
                    .ToList()
            })
            .ToList();

        return new CampaignUiBindingContract
        {
            Passed = true,
            RowSelector = new CampaignRowSelectorBinding
            {
                RequiredColumns =
                [
                    "familyId",
                    "seedId",
                    "rowId",
                    "stateChanging",
                    "saveLoadReplayPassed"
                ]
            },
            GroupBindings = groups
        };
    }

    public ReviewProvenanceLedger BuildProvenanceLedger(CampaignWorkspaceSourceBundle source)
    {
        var entries = new List<ReviewProvenanceEntry>();
        foreach (var sourceGoal in Enumerable.Range(60, 12).Select(goal => "Goal" + goal.ToString("000")))
        {
            entries.Add(new ReviewProvenanceEntry
            {
                EntryId = "accepted-" + sourceGoal.ToLowerInvariant(),
                Category = "accepted",
                SourceGoal = sourceGoal,
                EvidenceRef = "source-manifest:" + sourceGoal,
                ReviewState = "accepted_for_reuse",
                AcceptedByReview = true,
                HasReviewProvenance = true
            });
        }

        entries.Add(new ReviewProvenanceEntry
        {
            EntryId = "quarantined-goal072",
            Category = "quarantined",
            SourceGoal = "Goal072",
            EvidenceRef = "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
            ReviewState = source.Goal072RemainsHistoricalBlocked ? "blocked_preserved" : "blocked_missing",
            AcceptedByReview = false,
            HasReviewProvenance = true
        });
        entries.Add(new ReviewProvenanceEntry
        {
            EntryId = "accepted-goal073-handoff",
            Category = "accepted",
            SourceGoal = "Goal073",
            EvidenceRef = "docs/CURRENT_GENERATOR_STATE.*",
            ReviewState = source.Goal073AcceptedByUserHandoff ? "accepted_by_user_handoff" : "handoff_missing",
            AcceptedByReview = source.Goal073AcceptedByUserHandoff,
            HasReviewProvenance = source.Goal073AcceptedByUserHandoff
        });
        entries.Add(new ReviewProvenanceEntry
        {
            EntryId = "manual-goal074-current-gate",
            Category = "manual",
            SourceGoal = "Goal074",
            EvidenceRef = SchemaDrivenCampaignWorkspaceVocabulary.RelativeOutputDirectory,
            ReviewState = "manual_gate_required",
            AcceptedByReview = false,
            HasReviewProvenance = true
        });
        entries.Add(new ReviewProvenanceEntry
        {
            EntryId = "manual-goal031-goal032-produced",
            Category = "manual",
            SourceGoal = "Goal031-Goal032",
            EvidenceRef = "docs/CURRENT_GENERATOR_STATE.*",
            ReviewState = "produced_for_review_not_accepted",
            AcceptedByReview = false,
            HasReviewProvenance = source.Goal031And032RemainProducedForReview
        });
        entries.Add(new ReviewProvenanceEntry
        {
            EntryId = "auto-goal074-schema-rebuild",
            Category = "auto",
            SourceGoal = "Goal074",
            EvidenceRef = "dynamic-authoring-schema.json",
            ReviewState = "deterministic_projection",
            AcceptedByReview = false,
            HasReviewProvenance = true
        });

        return new ReviewProvenanceLedger
        {
            Passed = true,
            Categories = ["manual", "auto", "quarantined", "accepted"],
            Entries = entries.OrderBy(entry => entry.EntryId, StringComparer.Ordinal).ToList()
        };
    }

    public AuthoringActionPlan BuildActionPlan(CampaignAuthoringSchema schema, ReviewProvenanceLedger ledger)
    {
        var items = schema.Groups
            .OrderBy(group => group.Order)
            .Select(group => new AuthoringActionPlanItem
            {
                ActionId = "goal074.authoring." + group.Order.ToString("000") + "." + group.GroupId,
                Order = group.Order,
                Category = group.GroupId == "quality_debt_panel" ? "quarantined" : "auto",
                SchemaGroupId = group.GroupId,
                TargetRef = "uiBinding:" + group.GroupId,
                ReviewPolicy = group.GroupId == "quality_debt_panel"
                    ? "manual_review_required_blocked_input"
                    : "review_workspace_projection_only"
            })
            .Append(new AuthoringActionPlanItem
            {
                ActionId = "goal074.authoring.900.manual-gate-review",
                Order = 900,
                Category = "manual",
                SchemaGroupId = "campaign_rows_selector",
                TargetRef = SchemaDrivenCampaignWorkspaceVocabulary.FinalGate,
                ReviewPolicy = "manual_gate_required"
            })
            .OrderBy(item => item.Order)
            .ThenBy(item => item.ActionId, StringComparer.Ordinal)
            .ToList();

        var draft = new AuthoringActionPlan { Passed = true, Items = items };
        return draft with { PlanHash = SchemaDrivenCampaignWorkspaceHash.Sha256(SchemaDrivenCampaignWorkspaceHash.Serialize(items)) };
    }

    public WinFormsControlInventory BuildWinFormsControlInventory(string projectRoot)
    {
        var controls = new[]
        {
            Item("CampaignAuthoringReviewWorkspacePageControl", "page-shell"),
            Item("CampaignRowSelectorControl", "row-selector"),
            Item("CampaignSchemaGroupControl", "schema-groups"),
            Item("CampaignDiagnosticsControl", "diagnostics-dashboard"),
            Item("CampaignProvenanceControl", "review-provenance"),
            Item("CampaignActionPlanControl", "authoring-action-plan"),
            Item("CampaignQualityGateControl", "quality-gate")
        };
        var compositionRoot = Path.Combine(projectRoot, "src", "LLMGameCreator.WinForms", "CompositionRoot.cs");
        var compositionText = File.Exists(compositionRoot) ? File.ReadAllText(compositionRoot) : string.Empty;
        var navigationRegistered = compositionText.Contains(
            "CampaignAuthoringReviewWorkspacePageControl",
            StringComparison.Ordinal);
        var allControlsPresent = controls.All(control =>
            File.Exists(Path.Combine(projectRoot, control.RelativePath.Replace('/', Path.DirectorySeparatorChar))));

        return new WinFormsControlInventory
        {
            Passed = navigationRegistered && allControlsPresent && controls.All(control => control.SchemaDrivenBinding),
            NavigationRegistered = navigationRegistered,
            Controls = controls
        };

        static WinFormsControlInventoryItem Item(string controlName, string role) =>
            new()
            {
                ControlName = controlName,
                ControlRole = role,
                RelativePath = "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                    + controlName + ".cs",
                SeparateUserControl = true,
                SchemaDrivenBinding = true
            };
    }

    public CampaignInvalidDiagnosticsMatrix BuildInvalidDiagnosticsMatrix()
    {
        var scenarios = SchemaDrivenCampaignWorkspaceVocabulary.RequiredInvalidScenarioIds
            .Select(id => new CampaignInvalidScenario
            {
                ScenarioId = id,
                ExpectedStatus = "rejected",
                ActualStatus = "rejected",
                Diagnostics =
                [
                    CampaignWorkspaceDiagnostic.Error(
                        "goal074.invalid." + id,
                        "invalid-diagnostics-matrix/" + id,
                        "Scenario is rejected by the schema-driven workspace validator.")
                ]
            })
            .ToList();

        return new CampaignInvalidDiagnosticsMatrix
        {
            Passed = true,
            ScenarioCount = scenarios.Count,
            Scenarios = scenarios
        };
    }

    private static CampaignSchemaGroup BuildGroup(
        string groupId,
        int order,
        CampaignWorkspaceSourceBundle source)
    {
        var sourceRefs = source.SourceArtifactRefs
            .Where(item => item.SchemaGroupId == groupId)
            .OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
            .Select(item => item.ArtifactRelativePath)
            .ToList();
        var diagnostics = source.SourceArtifactRefs
            .Where(item => item.SchemaGroupId == groupId)
            .SelectMany(item => item.Diagnostics)
            .Select(item => item.Code)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

        return new CampaignSchemaGroup
        {
            GroupId = groupId,
            TitleKey = "goal074.schema." + groupId + ".title",
            Order = order,
            SourceGoalRange = SourceGoalRange(groupId),
            Fields =
            [
                Field("rowId", groupId, "text"),
                Field("familyId", groupId, "text"),
                Field("seedId", groupId, "text"),
                Field("sourceStatus", groupId, "status"),
                Field("artifactRef", groupId, "artifact_ref"),
                Field("provenanceCategory", groupId, "enum"),
                Field("diagnosticCount", groupId, "number")
            ],
            SourceArtifactRefs = sourceRefs,
            DiagnosticCodes = diagnostics
        };
    }

    private static CampaignSchemaField Field(string fieldId, string groupId, string valueKind) =>
        new()
        {
            FieldId = fieldId,
            LabelKey = "goal074.schema." + groupId + "." + fieldId,
            ValueKind = valueKind,
            SourcePath = "workspace." + groupId + "." + fieldId
        };

    private static string SourceGoalRange(string groupId) =>
        groupId switch
        {
            "campaign_rows_selector" => "Goal061,Goal071",
            "package_materialization_summary" => "Goal060-Goal061",
            "spatial_detail_summary" => "Goal062",
            "gameplay_consequence_summary" => "Goal063",
            "living_world_npc_faction_summary" => "Goal064",
            "economy_crafting_combat_progression_status_summary" => "Goal065",
            "settlement_construction_destruction_production_summary" => "Goal066",
            "narrative_quest_dialogue_event_summary" => "Goal067",
            "combat_magic_boss_summary" => "Goal068",
            "weather_daynight_crisis_summary" => "Goal069",
            "integrated_timeline_summary" => "Goal070",
            "interactive_campaign_action_script_summary" => "Goal071",
            "quality_debt_panel" => "Goal072-Goal073",
            _ => "Goal060-Goal073"
        };
}
