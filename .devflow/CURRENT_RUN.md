Task id: GOAL_003_AUTOMATED_VERIFICATION_EXTENSION_SPINE
Goal: Prove automated generated gameplay acceptance and data/rule-pack extension spine

Read-first sources:
- AGENTS.md, README.md, docs/CONTEXT_INDEX.md and current-state pair
- docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/NEXT_PRODUCT_SLICE_041_GENERATION_PRESETS_AND_OPTIONS_TASK.md
- docs/NEXT_PRODUCT_SLICE_042_MICROGAME_VARIATION_ACCEPTANCE_TASK.md
- docs/MANUAL_CONFIGURABLE_MICROGAME_VERIFICATION.md
- docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md
- docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md
- docs/FULL_GAME_GENERATION_MASTER_PLAN.md
- docs/GAME_SYSTEM_VARIANT_TAXONOMY.md
- MicrogameVariationAcceptanceService, RuntimeBackedMicrogameStateAcceptanceService and visible-preview/runtime-backed analogs
- FormulaEffectActionRulePackValidator and tests
- Application/Test csproj files
- product smoke runner

Implemented:
- recorded Goal 002 manual configurable verification as passed from the user report
- added one-manual-gate-per-goal process policy in state docs
- added docs/EXTENSION_RULE_PACK_CONTRACT_V1.md
- added docs/MANUAL_EXTENSION_SPINE_VERIFICATION.md
- added ExtensionRulePackValidator declaration-level validation
- added ExtensionSpineScenarioHarnessService
- harness runs base + extension variants through Generate -> Package -> Runtime start -> move -> interact -> goal progress -> reward -> completion
- extension proof pack adds an inventory objective and additional reward through data/rule-pack declaration only
- invalid extension pack is rejected for unsafe ids/paths, unknown API calls, invalid formula text and unsupported mutation targets
- deterministic reports are written under .llmgc/procedural/extension-spine
- added extension-spine product smoke route

Verification:
- ExtensionSpineScenarioHarness filtered tests: 4/4 passed
- CurrentGeneratorStateDocsTests before final handoff: 10/10 passed
- extension-spine product smoke: 1/1 passed
- check-all.ps1: 712/712 tests passed, build 0 warnings / 0 errors

Next:
- stop at manual_extension_spine_verification

Forbidden scope preserved:
- no S048
- no provider execution, LLM calls, Lua execution, Unity work or media generation
- no broad Runtime Preview game work
- no bespoke C# gameplay mechanic for the proof objective/reward
- no git commands
