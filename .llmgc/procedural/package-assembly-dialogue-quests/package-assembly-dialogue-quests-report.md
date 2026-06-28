# Package Assembly Dialogue And Quests Report

- Accepted: false
- Manual gate: package_assembly_dialogue_quests_expansion_verification
- Previous accepted gate: package_assembly_world_entities_expansion_verification passed
- Goal 025 evidence verified: true
- Goal 024 evidence verified: true
- Goal 023 evidence verified: true
- Real consumer passed: true
- Synthetic consumer passed: true
- Anti-overfit proof passed: true
- Package summary hash: b8c8b5098496eb3e12dd7ac301454870ed5f107e55d3a1ef25a3a5eb2508e17c
- Report hash: b6bba3826b1577d5840bd356e6f23bc044fb1c45b16598fa408fdab4e05d8940
- Invalid/fake/leak scenarios rejected: 16/16
- External execution: none

## Consumer Summaries

- goal026_real_consumer_gothic_mystery: quests=1, stages=2, objectives=3, dialogues=1, nodes=2, choices=2, packageHash=a21c33840906291bc0883fe23bfad2be9edd5b98a7bc1ef81c92bff92b404dda
- rumor_board_tutorial: quests=1, stages=1, objectives=2, dialogues=1, nodes=1, choices=1, packageHash=28814f5cd81eae8be8c2c90182163c43fece10aaa08a33fd65e78581398d1d4e

## Diagnostics

- info: package_dialogue_quests.boundary [execution_boundary] Goal 026 executes bounded in-memory package assembly only; no Unity, LLM, RAG, provider, media or Lua execution is invoked.
- info: package_dialogue_quests.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak scenarios reject through Goal 023/024/025 evidence, narrative validation, anti-overfit checks or scope guard diagnostics.
- info: package_dialogue_quests.previous_gate_recorded [package_assembly_world_entities_expansion_verification passed] User-confirmed Goal 025 package assembly world/entities verification is recorded as passed.
