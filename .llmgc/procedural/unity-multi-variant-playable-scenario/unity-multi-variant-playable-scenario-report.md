# Unity Multi-Variant Playable Scenario Report

- Accepted: false
- Final status: unity_generated_multi_variant_playable_scenario_verification
- Previous gate: unity_generated_quest_completion_loop_verification passed
- Completed slices: S146, S147, S148, S149, S150, S151, S152, S153
- Product smoke route: unity-multi-variant-playable-scenario
- Variants: 3/3
- Styles: frontier_survival, gothic_mystery, trade_caravan
- Distinct package/quest/scene/objective: 3 / 3 / 3 / 3
- All variants quest complete/reward granted: true / true
- Multi-variant scenario verified: true
- Variants hash: 39d8149424f1265f5a83323eaf081f7cc9653058e3dd880b7add83683f340b13
- Deterministic report hash: 45b9455e99c3cb7fbb2e9d7b6ebf9b7b51d7a47f421fdecbb4f084ed0a55f401
- Invalid/fake/leak scenarios rejected: 26/26

## Variants

- frontier_survival: package=game/content_generation/frontier-survival thread=thread/frontier-survival/000 quest=quest/frontier-survival/c2ed6dc235/000 reward=item/frontier-survival/7f81222990/004 accepted=true
- gothic_mystery: package=game/content_generation/gothic-mystery thread=thread/gothic-mystery/000 quest=quest/gothic-mystery/3629ab93b2/001 reward=item/gothic-mystery/412a858b32/001 accepted=true
- trade_caravan: package=game/content_generation/trade-caravan thread=thread/trade-caravan/000 quest=quest/trade-caravan/80137887dd/002 reward=item/trade-caravan/7bbca01309/011 accepted=true

## Diagnostics

- info: unity_multi_variant.goal017_gate_recorded [unity_generated_multi_variant_playable_scenario_verification] User-confirmed Goal 017 quest completion verification is recorded as passed.
- info: unity_multi_variant.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak multi-variant scenarios must reject through multi-variant, quest-loop, previous-evidence, artifact or firewall validation paths.
- info: unity_multi_variant.no_external_providers [execution_boundary] No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.
- info: unity_multi_variant.previous.goal017_evidence_present [unity-quest-completion-loop-report.json] Accepted Goal 017 compact evidence is present and matching.
