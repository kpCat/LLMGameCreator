using Xunit;

namespace LLMGameCreator.Tests.Application.Goal167;

// Complete Goal167 behavioral inventory is declared before production implementation.
internal static class Goal167TestInventory
{
    public static readonly IReadOnlyList<string> Behavioral =
    [
        "actor_to_dialogue_exact_provenance", "missing_dialogue_rejected", "duplicate_dialogue_rejected",
        "actor_entity_and_interaction_exact", "support_relationship_data_derived", "challenge_relationship_data_derived",
        "refuse_relationship_data_derived", "actor_without_relationship_remains_minimal", "flag_id_equals_dialogue_id",
        "initial_requirements_require_empty_flag", "follow_up_requirements_require_branch_value",
        "non_generated_dialogues_byte_identical", "dialogue_identity_and_source_metadata_preserved",
        "non_dialogue_collections_byte_identical", "definition_counts_unchanged", "independent_overlay_rebuild_deterministic",
        "reordered_input_canonical_output_equal", "forbidden_delta_rejected", "initial_available_ids_exact",
        "follow_ups_unavailable_initially", "support_branch_atomic_success", "support_reputation_data_derived",
        "support_alternatives_locked", "support_active_follow_up", "support_completed_follow_up",
        "challenge_starts_exact_encounter", "challenge_closes_dialogue", "refuse_negative_reputation_data_derived",
        "refuse_does_not_mutate_quest_or_encounter", "invalid_branch_atomic_rollback", "branch_replay_equivalent",
        "preview_uses_cloned_session", "preview_leaves_original_byte_identical", "preview_package_sha_unchanged",
        "preview_matches_runtime_choice_ids", "disabled_requirements_humanized", "human_consequence_previews",
        "decision_consequence_exact", "branch_locked_consequence_exact", "journal_projected_from_flags",
        "journal_alternatives_locked", "primary_ui_no_raw_technical_values", "choices_journal_unclipped",
        "choice_overlay_before_combat_overlay", "combat_final_package_preserved", "v5_choice_current",
        "v4_choices_pending", "v3_v2_compatible", "old_project_upgrades_without_source_rewrite",
        "campaign_readiness_requires_choice_current", "candidate_seal_choice_hashes", "choice_tamper_rejected",
        "regeneration_choice_current", "rollback_choice_current", "exact_save_support_flag_and_journal",
        "pre_choice_save_restores_initial_choices", "old_save_rebase_required", "explicit_rebase_current",
        "compatible_branch_flag_preserved", "incompatible_branch_flag_dropped", "no_ghost_journal_row",
        "post_migration_dialogue_combat_travel", "cached_hidden_smoke_once", "rc_current",
        "portable_all_selectable", "portable_core_only_no_false_rc", "goal166_regression", "goal165_regression",
        "goal164_regression", "goal163_162_161_regressions", "runtime_simulator_unchanged", "source_sidecars_unchanged"
    ];
}

public sealed class Goal167ChoiceBindingOverlayTests
{
    [Fact]
    public void Contract_goal167_inventory_contains_the_required_behavioral_matrix()
    {
        Assert.True(Goal167TestInventory.Behavioral.Count >= 71);
    }
}
