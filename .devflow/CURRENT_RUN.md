Task id: PRODUCT_SLICE_028_MANUAL_IMPORT_REPAIR_SEMANTIC_CATALOG_FOUNDATION_V1
Goal: Repair S027 manual import behavior and add deterministic project-local semantic memory plus LLM minimization policy

Read-first sources:
- AGENTS.md, README.md, docs/CONTEXT_INDEX.md, current-state pair and roadmap
- Product Smoke scenarios and Product Slice 026/027 docs
- S026/S027 manual import models, services, renderer, presenter/page and focused tests
- archive review/history/comparison services
- approved artifact set, deterministic semantic producer and GamePackage assembler/export seams
- target Application/WinForms/test project files

Implemented:
- dedicated archive-contained directory validation for `manual-import` and nested helper directories
- no manifest creation or import side effects from the folder helper
- `TargetOutputsChanged` result contract and review/history/comparison refresh only when target bytes are written
- deterministic project-local semantic catalog with small known seed set, candidates, relations, provenance and diagnostics
- flexible `semantic_pack_v1` mapping for terms, nested semantic objects, compact kind arrays and existing semantic groups
- deterministic semantic generation-context preview with compact sections and explicit LLM-minimization policy
- `.llmgc/semantic/` JSON/Markdown writers, focused tests and `semantic-catalog-foundation` ProductSmoke
- generation procedure, LLM policy, extensibility tiers, product-slice and current-state documentation

Verification:
- ManualImport/UnityArchiveReview filtered tests: 54/54 passed
- Semantic filtered tests: 9/9 passed
- semantic-catalog-foundation product smoke: 1/1 passed
- unity-archive-manual-import-workflow-ui product smoke: 1/1 passed
- ProductSmoke filtered tests: 27/27 passed
- check-devflow-state.ps1: passed in STOP_REVIEW mode
- check-all.ps1: 655/655 tests passed, build 0 warnings / 0 errors

Forbidden scope preserved:
- M5/M6 remain Locked
- no Runtime, Runtime.Abstractions, GamePackage schema, Scripting, Infrastructure, generator-library, solution, or project-file edits
- no provider, generator, LLM, Lua, Unity, or Runtime gameplay execution
- no git commands
