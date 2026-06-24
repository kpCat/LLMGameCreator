Task id: PRODUCT_SLICE_026_CONTROLLED_MANUAL_PROVIDER_OUTPUT_IMPORT_V1
Goal: Controlled manifest-based provider output import plus selected archive-history snapshot detail

Read-first sources:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md and .json
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/WINFORMS_DESIGNER_RULES.md
- target Application project and Unity archive materialization/provider-plan/fulfillment/review/history/comparison services/models
- target WinForms presenter/view/page/Designer
- required Application, WinForms, ProductSmoke tests and smoke runner

Implemented:
- UnityArchiveManualProviderImportService, models, and markdown renderer
- safe manifest/source/target containment validation over materialized slot metadata
- exact extension/path matching, duplicate/unknown slot diagnostics, idempotent same-byte handling, opt-in overwrite, SHA-256 reporting
- deterministic JSON/Markdown reports under production/
- existing fulfillment/review/history/comparison refresh chain after import
- selected snapshot JSON/status/path/sequence in Unity Archive Review
- manual import Markdown/JSON read-only tabs
- focused Application/WinForms tests and unity-archive-manual-provider-import ProductSmoke
- S026 product/smoke/current-state documentation with M5/M6 Locked preserved

Verification:
- ManualProviderImport/UnityArchiveReview filtered tests: 48/48 passed
- unity-archive-manual-provider-import product smoke: 1/1 passed
- ProductSmoke filtered tests: 25/25 passed
- check-devflow-state.ps1: passed in STOP_REVIEW mode
- check-all.ps1: 630/630 tests passed, build 0 warnings / 0 errors

Forbidden scope preserved:
- no Runtime or Runtime.Abstractions edits
- no GamePackage schema, Scripting, Infrastructure, generator-library, solution, or project-file edits
- no Unity implementation
- no provider, generator, LLM, Lua, Unity, or Runtime gameplay execution
- no git commands
