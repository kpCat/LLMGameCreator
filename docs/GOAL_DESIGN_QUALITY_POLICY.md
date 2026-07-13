# Goal Design Quality Policy

Every future Codex `GOAL.md` must complete this review before implementation and record the answers in compact evidence before GREEN.

1. State the exact user-visible claim, gameplay source of truth, fixture/proof-only data, and data that must never enter an activated package.
2. Provide a parameter-domain table for each typed parameter: minimum, default, maximum, invalid inputs, cross-parameter relations, affected package/Runtime/playthrough/save fields. Cover min/default/max/interior/out-of-range and every relation boundary; an extreme plan-only proof requires an explicit bounded-determinism reason.
3. Provide a state/event/rollback matrix for each command or effect: success mutation/events, failure mutation/events, transaction boundary, checkpoint and replay. A rolled-back state may not publish a success event.
4. Explicitly separate product and proof data. Any target, dummy, fixture encounter or special numeric value in activated data must be justified as product content.
5. Add a generic architecture scan for every normal FeatureModule Goal. Generic production services may not contain module, parameter, fixture ability/status/item/entity, composition or Goal literals except documented stable protocol vocabulary.
6. Cover independent modules where possible, direct dependencies, one accepted-mechanic interaction, all-current-optionals when practical, and the default-off unchanged path; no powerset is required.
7. For user-facing authoring work, cover existing-project open, save, close/reopen, build, repeat deterministic build, failed-build rollback and project identity.
8. State model/reasoning, read/command/Unity budgets, long-command retry policy and publication policy.
9. Before commit answer: unexecuted valid parameter combinations; proof-only product data; new generic-production literals; rollback event leaks; maximum-valid Runtime-state risk; and old-project behavior. Any unanswered answer blocks GREEN.
10. Every ordinary FeatureModule Goal that changes package mutations must record a base package vs activated product package structured diff. Every changed definition, participant and property must map through typed operation metadata to exactly one classification: `declared_user_facing_mechanic`, `declared_user_facing_starter_content`, `authoring_identity_metadata` or `forbidden_qualification_proof_fixture`.
11. The executable pre-commit quality gate is GREEN only when `forbidden qualification/proof fixture count=0`, every mutation maps to a declared module capability or visible starter-content claim, and there is no unexplained global capacity or rule change. Any proof-only product data, missing classification or orphan claim blocks GREEN.
