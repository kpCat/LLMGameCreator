# CURRENT_RUN.md

Task id: PRODUCT_SLICE_018_UNITY_ARCHIVE_MATERIALIZATION_V1
Goal: materialize deterministic editor-side Unity archive contract/meta files without implementing Unity
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/018_UNITY_ARCHIVE_MATERIALIZATION_V1.md

Source docs/code read:
- AGENTS.md, docs/CONTEXT_INDEX.md, docs/CURRENT_GENERATOR_STATE.md/json and docs/ROADMAP_TO_FULL_GENERATOR.md
- Product Slice 016/017 docs, Unity target, dry-run and materialization specs
- target Application/test csproj files, Application/Composition contracts/export services, focused tests and product-smoke runner/docs

Patterns reused:
- Slice 017 validator-first dry-run and readiness mapping
- deterministic project-local UTF-8 JSON/markdown with resolved-path containment checks
- immutable Application/Composition records plus named headless xUnit ProductSmoke routing

Implemented:
- materialization request/result, file, diagnostic, readiness and metadata-index models
- dry-run-backed materialization service with safe `.llmgc/unity-archive/` output
- deterministic manifest, composition, runtime-module, UI, asset/audio, localization, Lua, report and validation files
- current playable-contract, warning, future metadata-only, missing-requirement blocked and invalid behavior
- unity-archive-materialization product smoke and docs/state handoff
- optional zip intentionally omitted from v1

Non-goals preserved:
- no Unity project/runtime/build and no Runtime, GamePackageDefinition/package schema, WinForms, Scripting, Infrastructure/Generation or generator-library changes
- no solution/project/package changes, provider calls, ComfyUI/Suno integration or generator/Lua execution
- M4.1 and controlled-slice gates remain guarded; Product Slice 017 is accepted as the parent

Checks run before state update:
- UnityArchiveMaterialization filtered tests: passed, 5 tests including product smoke
- ProductSmoke filtered tests: passed, 16 tests
- unity-target-contract: passed, run 20260622_115324-product-smoke
- unity-archive-export-dry-run: passed, run 20260622_115329-product-smoke
- unity-archive-materialization: passed, run 20260622_115334-product-smoke

Final guards:
- check-devflow-state.ps1: passed; STOP_REVIEW preserved, 9 tasks, 2 known warnings
- check-all.ps1: passed; build 0 warnings/0 errors, tests 546 passed; run 20260622_115629-check-all
- CURRENT_GENERATOR_STATE.json parse/guard inspection: passed; M4.1 phase/milestone preserved and Product Slice 018 records Product Slice 017 as parent
- mojibake marker and UTF-8 BOM scan over all 9 changed files: passed, no markers or BOM found
