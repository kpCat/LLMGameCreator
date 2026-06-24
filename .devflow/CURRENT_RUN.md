Task id: PRODUCT_SLICE_027_CONTROLLED_MANUAL_IMPORT_WORKSPACE_UI_V1
Goal: Complete safe manual-import workspace inside Unity Archive Review

Read-first sources:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md and .json
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/PRODUCT_SLICE_026_CONTROLLED_MANUAL_PROVIDER_OUTPUT_IMPORT.md
- docs/WINFORMS_DESIGNER_RULES.md and target project files
- S026 import service/models/renderer plus slot/fulfillment models
- Unity Archive Review presenter/view/page/Designer and local grid/filter/open/busy analogs
- required Application, WinForms, ProductSmoke tests and smoke runner

Implemented:
- slot dashboard/checklist over fulfillment plan/state plus typed asset/audio/Lua indexes
- All/Missing/Available/Invalid/manual-provider/future-provider filters, selection detail, and copy helpers
- deterministic missing/invalid-only `manual-import/import-manifest.template.json` generation without overwriting the run manifest
- create/open manual-import folder helper with shell launch isolated in UI
- Run manual import action through the existing S026 service with overwrite disabled by default and explicit risk opt-in
- refreshed manual import reports, fulfillment state, review, history, comparison, and preserved selected snapshot
- focused Application/WinForms tests and `unity-archive-manual-import-workflow-ui` ProductSmoke
- S027 product/smoke/current-state documentation with M5/M6 Locked preserved

Verification:
- ManualImport/UnityArchiveReview filtered tests: 51/51 passed
- WinForms filtered tests: 52/52 passed
- unity-archive-manual-import-workflow-ui product smoke: 1/1 passed
- ProductSmoke filtered tests: 26/26 passed
- check-devflow-state.ps1: passed in STOP_REVIEW mode
- check-all.ps1: 641/641 tests passed, build 0 warnings / 0 errors

Forbidden scope preserved:
- no Runtime or Runtime.Abstractions edits
- no GamePackage schema, Scripting, Infrastructure, generator-library, solution, or project-file edits
- no Unity implementation
- no provider, generator, LLM, Lua, Unity, or Runtime gameplay execution
- no git commands
