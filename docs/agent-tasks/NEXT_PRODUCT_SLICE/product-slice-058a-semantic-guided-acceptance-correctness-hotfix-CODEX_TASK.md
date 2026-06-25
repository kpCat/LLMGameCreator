# Product Slice 058A - Semantic-Guided Acceptance Correctness Hotfix

## Purpose

Repair false-positive acceptance and contract-validation gaps found during external review of Goal 005.

This is a bounded correctness hotfix. It is not Goal 006, must not create S059, and must not add new gameplay, UI, Runtime Preview, LLM, RAG, Lua, Unity, provider or media features.

## Starting State

Goal 005 S054-S058 is implemented, but `semantic_guided_composition_artifact_verification` has not passed.

External review found that Goal 005 cannot yet be accepted because invalid semantic composition can be reported as rejected for the wrong reason and valid scenarios can ignore composition errors.

## Context Budget

Read only:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. this task
5. the Goal 005 compiler/composition services and focused tests
6. `docs/SEMANTIC_PACK_CONTRACT_V1.md`

Read broader strategy/history only if a direct code or test failure requires it.

## Required Fixes

### 1. Derive scenario acceptance from validation, not scenario id

Current behavior uses the scenario id `invalid_conflict_rejection` to force `Accepted = false`.

Fix this so:

- `ExpectedValid` remains test expectation metadata only;
- actual `Accepted` is derived exclusively from compiler result, missing-layer checks, candidate leakage, rule-pack validation and composition diagnostics;
- any error-severity composition diagnostic makes actual acceptance false;
- the invalid/conflict scenario passes the acceptance matrix only when it is actually rejected by a real error diagnostic;
- removing the conflicting relation from an invalid fixture would make the expected-invalid matrix fail rather than remain green.

The overall report must evaluate expected versus actual explicitly.

### 2. Make `excludes` and `forbidden_in_tone` errors effective

`BuildCompositionDiagnostics` currently emits errors that are not included in scenario acceptance.

Fix and test:

- active `excludes` conflict rejects composition;
- active `forbidden_in_tone` conflict rejects composition;
- unsatisfied `requires` is rejected or diagnosed through a clear compiler/composition error path;
- valid `compatible_with` or preference relations do not create false errors.

Do not claim that tags, hints, `compatible_with`, `preferred_in_tone` or other semantics actively influence selection unless code actually consumes them. Keep report wording exact.

### 3. Validate the semantic-pack contract identity

Reject with deterministic diagnostics:

- any `SchemaVersion` other than `semantic_pack_contract_v1`;
- a `LayerId` prefix inconsistent with `LayerKind`:
  - `core/*` -> `core`;
  - `genre/*` -> `genre`;
  - `project/*` -> `project`;
  - `imported_candidate/*` -> `imported_candidate`;
  - `llm_candidate/*` -> `llm_candidate`.

This is required because relation priority currently observes layer-id prefixes while compiler precedence observes layer kind.

### 4. Prevent conflicted declarations from re-entering active output

For three or more same-precedence declarations of the same term/relation:

- once an id is marked conflicted, later declarations in the same compilation must not re-add it to the active catalog;
- conflicted ids remain quarantined/diagnosed;
- the compiler result remains rejected;
- active compiled output contains no conflicted declaration.

Apply equivalent deterministic behavior to relation-id conflicts.

### 5. Handle malformed pack files without an unhandled JSON exception

Directory loading must not terminate the acceptance run with a raw `JsonException` when one pack is malformed.

Introduce a narrow load-result/diagnostic path or equivalent behavior so:

- malformed JSON is associated with its relative source file;
- an error diagnostic is returned deterministically;
- acceptance becomes false;
- valid neighboring packs may still be reported;
- no absolute machine-specific paths enter byte-stable artifacts.

Preserve an existing compatibility method only if useful; do not silently skip malformed files.

### 6. Make runtime evidence wording honest

Goal 005 currently selects semantic quest/dialogue/interaction ids and separately runs the Goal 004 regression harness.

Do not imply that the selected semantic variant itself was executed in runtime unless it actually was.

For this hotfix choose one bounded option:

- preferred: expose/reuse a narrow Goal 004 scenario seam so selected ids are applied to the generated family scenario and runtime-backed evidence is tied to that selection; or
- acceptable if runtime application would require redesign: report explicitly that semantic selection is generator-level evidence and Goal 004 runtime evidence is an independent regression check. Add a structured boolean/source field so this limitation cannot be mistaken for runtime execution.

Do not perform a broad Goal 004/Runtime Preview rewrite.

## Required Tests

Add focused regression tests for at least:

1. invalid scenario without a real error does not satisfy expected-invalid acceptance;
2. `excludes` error makes actual scenario acceptance false;
3. `forbidden_in_tone` error makes actual scenario acceptance false;
4. wrong schema version is rejected;
5. layer id/kind mismatch is rejected;
6. three same-precedence conflicting term declarations cannot reactivate the term;
7. conflicting relation id cannot remain active;
8. malformed JSON produces deterministic diagnostics rather than an unhandled exception;
9. existing reference packs still compile;
10. Goal 004 regression remains green;
11. repeated Goal 005 output remains byte/hash stable.

Tests must assert behavior, not only search for hardcoded report strings.

## Artifacts And State

Regenerate the existing artifacts under:

```text
.llmgc/procedural/semantic-guided-composition/
```

Do not invent a parallel report folder.

Update `CURRENT_GENERATOR_STATE.md` and `.json` to record S058A, but keep the next gate as:

```text
semantic_guided_composition_artifact_verification
```

Do not mark the gate passed. Do not recommend Goal 006 yet.

## Verification

Run focused tests first, then:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~SemanticLayerCompiler|FullyQualifiedName~SemanticGuidedComposition|FullyQualifiedName~QuestDialogInteractionFamily|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario semantic-guided-composition
.\.devflow\scripts\check-all.ps1
```

## Hard Limits

- No S059.
- No Goal 006.
- No new Runtime Preview UI.
- No LLM/RAG/provider/Lua/Unity/media execution.
- No external dataset import.
- No broad GamePackage/runtime redesign.
- No git commands.

## Final Report

Report:

- root cause of each fixed acceptance gap;
- changed files;
- added regression cases;
- actual focused/smoke/full verification results;
- whether semantic-selected ids were executed or only generator-level selected;
- regenerated artifact folder;
- confirmation that the gate remains `semantic_guided_composition_artifact_verification` and S059 was not created.
