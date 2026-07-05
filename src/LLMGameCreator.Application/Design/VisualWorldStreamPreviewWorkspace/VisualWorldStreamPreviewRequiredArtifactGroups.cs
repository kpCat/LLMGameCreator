namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<string> BuildRequiredArtifactGroupIds() =>
    [
        "microtiles",
        "map_patches",
        "region_composer",
        "world_profiles",
        "chunk_stream_windows",
        "cache_exports",
        "unity_handoff",
        "geoworld",
        "offline_geoworld_handoff",
        "offline_geoworld_unity_preview",
        "offline_geoworld_unity_editor_preview",
        "offline_geoworld_playmode_travel",
        "offline_geoworld_interactive_travel",
        "offline_geoworld_interactions",
        "offline_geoworld_session_replay",
        "offline_geoworld_objective_acceptance",
        "offline_geoworld_alpha_slice",
        "offline_geoworld_alpha_export_package",
        "offline_geoworld_alpha_manual_acceptance",
        "offline_geoworld_alpha_manual_result_intake",
        "offline_geoworld_alpha_acceptance_operator_pack",
        "offline_geoworld_alpha_manual_result_workbench",
        "offline_geoworld_alpha_human_result_revalidation"
    ];
}
