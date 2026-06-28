# Package Assembly Items Economy Crafting Report

- accepted: false
- finalStatus: package_assembly_items_economy_crafting_expansion_verification
- manualGate: package_assembly_items_economy_crafting_expansion_verification
- previousAcceptedGate: package_assembly_dialogue_quests_expansion_verification passed
- Goal 026 evidence verified: true
- Goal 025 evidence verified: true
- Goal 024 evidence verified: true
- Goal 023 evidence verified: true
- Real consumer passed: true
- Synthetic consumer passed: true
- Anti-overfit proof passed: true
- Package summary hash: f73bb4f76ace915519a27e8e040318c42afa8423935da80e567b77055cbc3269
- Report hash: a4c6c3a61895786d4cfdf4ace802d52df6c0ceb2bb86e5b9b0f26f9d54aaee00
- Invalid/fake/leak scenarios rejected: 18/18
- External execution: none

## Consumer Summaries

- goal027_real_consumer_trade_caravan: items=2, resources=1, recipes=1, lootTables=1, transactions=1, inventories=1, equipmentSlots=1, packageHash=2014dad8542ab06919461ee32016498e1e8e36ff724c599fb0371e716ed535d5
- vendor_crafting_transaction: items=2, resources=1, recipes=1, lootTables=1, transactions=1, inventories=1, equipmentSlots=1, packageHash=db043f6a50991eb9329bc21916891851d634a733d95863db34b07dea8f4776d8

## Diagnostics

- info: package_items_economy_crafting.boundary [execution_boundary] Goal 027 executes bounded in-memory package assembly only; no Unity, LLM, RAG, provider, media or Lua execution is invoked.
- info: package_items_economy_crafting.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak scenarios reject through Goal 023/024/025/026 evidence, economy validation, anti-overfit checks or scope guard diagnostics.
- info: package_items_economy_crafting.previous_gate_recorded [package_assembly_dialogue_quests_expansion_verification passed] User-confirmed Goal 026 package assembly dialogue/quests verification is recorded as passed.
