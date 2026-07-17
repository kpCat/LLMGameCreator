# Goal163 package-truth campaign consequences

Status: `BLOCKED_EXACT_GENERATED_ENCOUNTER_PACKAGE_CONTRACT`; `accepted=false`; no human gate; independent audit remains required.

Goal163 removes the package clone, transient campaign attack, fixed combat power and BasicAttack rewrite. Runtime dispatch records the exact captured package reference and unchanged SHA/inventories. BasicAttack remains BasicAttack; UseAbility is available only for an exact participant-owned package ability. Generated quest readiness is computed read-only from victory and player inventory, generated quests are not refreshed into completion, and a ready controlled package dispatches one CompleteQuest with rewards/reputation and zero AdvanceQuestObjective.

The `Последствия` UI shows human damage, encounter outcome, reward, quest, reputation, travel, save/load and migration rows only when supported by state deltas, exact Runtime events or typed save results. Actual FinalStateHash is separate from SelectedBuildHistorySha256. Goal163 focused tests and all required regressions are GREEN; protected bytes remain unchanged and Player/Unity/standalone counts are zero.

The real qualified package's ordinary `encounter/goblin_duel` executes exact BasicAttack. Its generated quest encounters do not: participants own `generated/resource/health` and only `generated/ability/action_resolve_encounter`, an effectless `generated_action`. The player-facing generated encounter is therefore causally disabled. Generated victory, manual turn-in, post-turn-in continue and post-migration combat cannot be claimed until a separately scoped package-generation contract repair is completed.

Goal164 closes the independent-audit blocker at `d5d614a8` with a deterministic build-time generated encounter combat contract. Goal163 exact-package dispatch, read-only readiness, manual turn-in and consequence behavior remain unchanged and their real all-selectable/core-only routes are now GREEN.

Next action: `independent_goal164_audit_and_plan_campaign_choice_branching_or_failure_recovery`.
