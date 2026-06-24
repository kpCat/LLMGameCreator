Task id: PRODUCT_SLICE_037_MICROGAME_ACCEPTANCE_PLAYABILITY_POLISH
Goal: Close Goal 001 headless scope with generated microgame acceptance and manual-verification handoff

Read-first sources:
- AGENTS.md, README.md, docs/CONTEXT_INDEX.md and current-state pair
- docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md and docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/GOAL_001_FIRST_5_MINUTE_GENERATED_MICROGAME_LOOP.md
- docs/NEXT_PRODUCT_SLICE_037_MICROGAME_ACCEPTANCE_POLISH_TASK.md
- Application/Runtime/WinForms/Test csproj files
- VisibleGeneratedPlayablePreviewService and OneClickGeneratedPreviewWorkflowService
- GeneratedPackageRuntimePreviewService, GeneratedContentInteractionPreviewService, GeneratedQuestDialoguePreviewService and GeneratedMicrogameGoalPreviewService
- EncounterRuntimeService, QuestRuntimeService and OutputApplier as contract boundaries
- RuntimePreviewPageControl and CompositionRoot
- DefaultGameRuntime and Runtime.Abstractions contracts
- product smoke runner and product smoke analogs

Implemented:
- GeneratedMicrogameAcceptanceService for deterministic microgame loop acceptance sidecars
- OneClickGeneratedPreviewWorkflow writes generated-microgame-loop snapshot/report/manual verification docs
- generated-microgame-loop product smoke route
- README, manual verification doc, current-state pair and context index updated to recommend manual_microgame_loop_verification next

Verification:
- GeneratedMicrogameAcceptance filtered tests: 2/2 passed
- generated-microgame-loop product smoke: 1/1 passed
- CurrentGeneratorStateDocsTests after S037 handoff: 10/10 passed
- check-all.ps1: 691/691 tests passed, build 0 warnings / 0 errors

Forbidden scope preserved:
- M5/M6 remain Locked
- no provider execution, LLM calls, Lua execution, Unity work or media generation
- no broad GamePackage schema redesign
- no public runtime command/state contract redesign
- no git commands
