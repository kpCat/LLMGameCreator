Task id: PRODUCT_SLICE_032_GENERATED_PACKAGE_MVP
Goal: Map S029 generated plan, S030 rule pack and S031 tiny-loop result into the first minimal generated GamePackage MVP artifact

Read-first sources:
- AGENTS.md, README.md, docs/CONTEXT_INDEX.md and current-state pair
- docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md and docs/ROADMAP_TO_FULL_GENERATOR.md
- Product Slice 029, 030, 031 and 032 task docs
- generation policy, full generator master plan, game system taxonomy
- GamePackage format, validation strategy, Application/GamePackage/Runtime csproj files
- S029/S030/S031 procedural services, renderers and tests
- GamePackageDefinition, GamePackageValidator and package/runtime contract definitions
- product smoke runner and product smoke analogs

Implemented:
- S031 handoff cleanup in current-state docs and CONTEXT_INDEX
- missing formula-effect-action-registry product smoke route and summary artifact path
- deterministic GeneratedPackageMvp models, service and markdown renderer
- `.llmgc/procedural/generated-package-mvp/package.json`
- `.llmgc/procedural/generated-package-mvp/generated-package-mvp-report.json`
- `.llmgc/procedural/generated-package-mvp/generated-package-mvp-report.md`
- `.llmgc/procedural/generated-package-mvp/runtime-bootstrap-report.json`
- `.llmgc/procedural/generated-package-mvp/runtime-bootstrap-report.md`
- package mapping from generated regions, actors, items/resources, factions, encounters, quests, dialogues, interactions, formulas and rule-pack actions into existing GamePackage contracts
- package validation evidence and Application-layer bootstrap-adapter evidence in reports
- diagnostics for report-only/unmapped concepts instead of broad schema/runtime expansion
- focused GeneratedPackageMvp tests and generated-package-mvp product smoke
- README, CONTEXT_INDEX and current-state pair updated to recommend visible_generated_playable_preview next

Verification:
- CurrentGeneratorStateDocsTests filtered tests: 10/10 passed
- formula-effect-action-registry product smoke: 1/1 passed
- GeneratedPackageMvp filtered tests: 5/5 passed
- generated-package-mvp product smoke: 1/1 passed
- check-all.ps1: 674/674 tests passed, build 0 warnings / 0 errors

Forbidden scope preserved:
- M5/M6 remain Locked
- no WinForms UI, provider execution, LLM calls, Lua execution, Unity work or media generation
- no broad GamePackage schema redesign
- no public runtime command/state contract redesign
- no git commands
