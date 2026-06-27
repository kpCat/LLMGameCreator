# Package Assembly World And Entities Report

- Accepted: false
- Manual gate: package_assembly_world_entities_expansion_verification
- Previous accepted gate: modular_contract_goal_policy_adoption_verification passed
- Goal 024 evidence verified: true
- Goal 023 evidence verified: true
- Real consumer passed: true
- Synthetic consumer passed: true
- Anti-overfit proof passed: true
- Package summary hash: f7eef6a85be0672b74af4bb8ed54ac0b165b1f646e490646e4ab68178fb56613
- Report hash: 55f103af63f4dae735b11ca131684477f1e50b1d77e7dcca92879697dcbdef9b
- Invalid/fake/leak scenarios rejected: 15/15
- External execution: none

## Consumer Summaries

- goal025_real_consumer_trade_caravan: maps=2, prototypes=4, placements=3, packageHash=415886db473d267eac2cee478441b1cf7ef7d3fc3aa19ccea3aa0cbcb639ebfb
- npc_city_walk: maps=2, prototypes=4, placements=3, packageHash=aceaf625e33d6ddcfb034596dd0c621a0528f888b1c7078e33e3c9da498379d3

## Diagnostics

- info: package_world_entities.boundary [execution_boundary] Goal 025 executes bounded in-memory package assembly only; no Unity, LLM, RAG, provider, media or Lua execution is invoked.
- info: package_world_entities.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak scenarios reject through Goal 023/024 evidence, placement validation, anti-overfit checks or scope guard diagnostics.
- info: package_world_entities.previous_gate_recorded [modular_contract_goal_policy_adoption_verification passed] User-confirmed modular contract goal policy adoption verification is recorded as passed.
