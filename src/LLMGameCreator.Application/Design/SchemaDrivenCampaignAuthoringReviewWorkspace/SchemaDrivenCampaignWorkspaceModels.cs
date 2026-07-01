namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

public static class SchemaDrivenCampaignWorkspaceVocabulary
{
    public const string GoalId = "goal_074_schema_driven_campaign_authoring_review_workspace";
    public const string ProductSmokeRoute = "goal-074-schema-driven-campaign-authoring-review-workspace";
    public const string FinalGate = "schema_driven_campaign_authoring_review_workspace_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-074-schema-driven-campaign-authoring-review-workspace";

    public static readonly IReadOnlyList<string> FamilyIds =
    [
        "map_panel_rpg",
        "survival_sandbox",
        "first_person_grid_dungeon"
    ];

    public static readonly IReadOnlyList<string> SeedIds =
    [
        "seed_alpha",
        "seed_beta",
        "seed_gamma"
    ];

    public static readonly IReadOnlyList<string> RequiredSchemaGroupIds =
    [
        "campaign_rows_selector",
        "package_materialization_summary",
        "spatial_detail_summary",
        "gameplay_consequence_summary",
        "living_world_npc_faction_summary",
        "economy_crafting_combat_progression_status_summary",
        "settlement_construction_destruction_production_summary",
        "narrative_quest_dialogue_event_summary",
        "combat_magic_boss_summary",
        "weather_daynight_crisis_summary",
        "integrated_timeline_summary",
        "interactive_campaign_action_script_summary",
        "quality_debt_panel"
    ];

    public static readonly IReadOnlyList<string> RequiredInvalidScenarioIds =
    [
        "missing_goal060_source_artifact",
        "missing_goal061_source_artifact",
        "missing_goal071_source_artifact",
        "missing_goal073_source_artifact",
        "fake_family_id",
        "fake_seed_id",
        "duplicate_row_id",
        "missing_schema_group",
        "ui_binding_unknown_field",
        "candidate_accepted_without_review_provenance",
        "final_prose_leak",
        "provider_llm_rag_media_generation_claim",
        "runtime_gamepackage_schema_mutation_claim",
        "unity_broad_mutation_claim",
        "new_p0_line_length",
        "nondeterministic_ordering"
    ];

    public static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };

    public static string SeedOrderingKey(string seedId) =>
        seedId switch
        {
            "seed_alpha" => "001-seed-alpha",
            "seed_beta" => "002-seed-beta",
            "seed_gamma" => "003-seed-gamma",
            _ => "999-" + seedId
        };
}

public sealed record CampaignWorkspaceDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static CampaignWorkspaceDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static CampaignWorkspaceDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static CampaignWorkspaceDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record CampaignWorkspaceSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string SchemaGroupId { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<CampaignWorkspaceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CampaignWorkspaceSourceArtifactStats
{
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string SourceGoal { get; init; } = string.Empty;
    public string SchemaGroupId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int ItemCount { get; init; }
    public IReadOnlyList<string> RepresentativeIds { get; init; } = [];
}

public sealed record CampaignWorkspaceSourceRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string InteractiveRowHash { get; init; } = string.Empty;
    public string InitialStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public int ActionCount { get; init; }
    public bool StateChanging { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
}

public sealed record CampaignWorkspaceSourceBundle
{
    public bool Goal073AcceptedByUserHandoff { get; init; }
    public bool Goal072RemainsHistoricalBlocked { get; init; }
    public bool Goal031And032RemainProducedForReview { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<CampaignWorkspaceSourceRow> Rows { get; init; } = [];
    public IReadOnlyList<CampaignWorkspaceSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<CampaignWorkspaceSourceArtifactStats> ArtifactStats { get; init; } = [];
    public IReadOnlyList<CampaignWorkspaceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CampaignWorkspaceGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record CampaignWorkspaceSourceManifest
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_workspace_source_manifest_v1";
    public string GoalId { get; init; } = SchemaDrivenCampaignWorkspaceVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = SchemaDrivenCampaignWorkspaceVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = SchemaDrivenCampaignWorkspaceVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal073AcceptedByUserHandoff { get; init; }
    public bool Goal072RemainsHistoricalBlocked { get; init; }
    public bool Goal031And032RemainProducedForReview { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<CampaignWorkspaceGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<CampaignWorkspaceSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<CampaignWorkspaceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CampaignRowSelector
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_row_selector_v1";
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<CampaignRowSelectorFamily> Families { get; init; } = [];
    public IReadOnlyList<CampaignRowSelectorRow> Rows { get; init; } = [];
}

public sealed record CampaignRowSelectorFamily
{
    public string FamilyId { get; init; } = string.Empty;
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<string> RowIds { get; init; } = [];
}

public sealed record CampaignRowSelectorRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string DisplayKey { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string InteractiveRowHash { get; init; } = string.Empty;
    public bool StateChanging { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
}
