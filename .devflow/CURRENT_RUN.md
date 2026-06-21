# CURRENT_RUN.md

Task id: PRODUCT_SLICE_016_UNITY_TARGET_CONTRACT_FOUNDATION
Goal: add machine-readable Game Design Brief and Unity archive/player target contracts without implementing Unity
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/016_UNITY_TARGET_CONTRACT_FOUNDATION.md

Source docs/code read:
- AGENTS.md, README.md, docs/CONTEXT_INDEX.md, docs/CURRENT_GENERATOR_STATE.md/json and docs/ROADMAP_TO_FULL_GENERATOR.md
- official product plan, workbench/capability plans and all four Product Slice 016 contract/rationale docs
- target Application/test csproj files, Application/Composition, nearby application/product-smoke tests and product-smoke runner

Patterns reused:
- immutable Application/Composition records and deterministic built-in preset providers
- stable diagnostic codes, error/warning results and deterministic sorting
- named headless xUnit ProductSmoke routing through run-product-smoke.ps1

Implemented:
- structured GameDesignBrief lore/rules/wishes plus generation, scale and performance policies
- Unity target profile, archive manifest, 22 runtime module, dynamic UI, asset/audio request and large-world streaming contracts
- three built-in Unity target profiles, a top-down generated RPG archive preset and metadata-only future provider source kinds
- validation for blank/duplicate/unknown/unsafe ids, blank bindings, duplicate requests, future modules and inconsistent large-world policy
- unity-target-contract product smoke and docs/state handoff

Non-goals preserved:
- no Unity project/runtime/build and no Runtime, GamePackageDefinition/package schema, WinForms, Scripting, Infrastructure/Generation or generator-library changes
- no solution/project/package changes, provider calls, ComfyUI/Suno integration or generator execution
- no semantic world model, imported-map implementation, lazy-world engine, NPC schedules, police/crime, vehicles or army battles
- M4.1 and controlled-slice gates remain guarded; Product Slice 015 is accepted as the parent

Checks run before state update:
- UnityTarget filtered tests: passed, 9 tests
- GameDesignBrief filtered tests: passed, 1 test
- ProductSmoke filtered tests: passed, 14 tests
- all thirteen named product smoke scenarios: passed
- unity-target-contract: passed, run 20260621_233401-product-smoke

Final guards:
- check-devflow-state.ps1: passed; STOP_REVIEW preserved, 9 tasks, 2 known warnings
- check-all.ps1: passed; build 0 warnings/0 errors, tests 537 passed; run 20260621_233624-check-all
- CURRENT_GENERATOR_STATE.json parse: passed; M4.1 phase/milestone preserved and Product Slice 016 recorded with Product Slice 015 as parent
- mojibake marker scan over all 13 changed files: passed, no markers found
