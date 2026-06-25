# Goal 006 - Semantic-Selected Generated Package And Runtime Composition

## Goal

Close the boundary left explicit by Goal 005:

```text
compiled semantic catalog
-> selected quest/dialogue/interaction declarations
-> generated package content
-> executed headless runtime scenario
-> runtime-owned state evidence
```

Goal 005 proved generator-level semantic selection and kept Goal 004 runtime evidence as an independent regression. Goal 006 must prove that the semantic-selected declarations themselves are materialized into the generated package and are the declarations exercised by the runtime scenario.

This is not Runtime Preview polish. The primary acceptance path is headless and deterministic.

## Required Starting Evidence

The user prompt must explicitly contain:

```text
semantic_guided_composition_artifact_verification passed.
```

S058A is accepted. Do not repeat Goal 005 artifact review.

## Context Budget Rule

Read first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. this goal file

Read `docs/CURRENT_GENERATOR_STATE.md` only for S059/S063 state changes.

Inspect only the implementation seams needed by this goal:

- Goal 005 semantic compiler/composition services and models;
- Goal 004 quest/dialogue/interaction declarations and validator;
- generated package MVP assembly;
- visible/headless generated runtime adapter and runtime-backed state acceptance;
- existing GamePackage and runtime contracts actually required for the binding;
- focused tests and product-smoke routing.

Read broad strategy docs only when a concrete boundary is unclear. Do not read old product-slice packs or historical reports by default.

## Architecture Rules

- Semantics select declarations; they do not execute gameplay directly.
- Rule-pack declarations are the bridge between semantic meaning and stable C# primitives.
- The generated package must contain enough deterministic provenance to trace selected semantics and rule declarations.
- Runtime evidence must identify the selected package content it executed.
- Do not claim runtime execution when only a report projection was produced.
- Do not add C# branches for individual genre, project, term, quest or dialogue ids.
- New data variants must remain data-only.
- Reuse existing runtime commands/state containers where they can represent the behavior honestly.
- If a selected declaration cannot be represented by existing runtime primitives, emit an explicit unsupported-binding diagnostic. Do not fake execution.

## Non-Goals

- No Runtime Preview UI work.
- No semantic/rule-pack editor UI.
- No regional world navigation yet.
- No RAG or vector database.
- No LLM/provider execution.
- No arbitrary Lua execution.
- No Unity or media work.
- No large GamePackage/runtime redesign.
- No unrelated gameplay families.
- No S064 or later slice.

## Product Slices

### S059 - Record Goal 005 Gate And Establish Runtime Composition Plan Contract

Purpose:

- record `semantic_guided_composition_artifact_verification passed`;
- create a narrow deterministic composition-plan contract that carries semantic selections into package assembly;
- expose/reuse Goal 004 family declarations without expanding the existing monolithic acceptance service.

The plan must include at least:

- seed and selected semantic layer/catalog hashes;
- selected semantic term/relation trace;
- selected quest pattern id and concrete objectives;
- selected dialogue intent/template id and bound semantic slots;
- selected interaction pattern id and target/result bindings;
- selected NPC/item/encounter/location references where available;
- diagnostics and provenance;
- deterministic plan hash.

Required behavior:

- every selected semantic target resolves to a validated Goal 004 declaration;
- every content binding resolves to an existing generated id or produces an error diagnostic;
- candidate/deprecated/conflict/invalid terms cannot enter the plan;
- plan output is byte/hash stable;
- no genre/project-specific C# conditionals.

If Goal 004 contracts are trapped inside its acceptance service, perform only a narrow behavior-preserving extraction into reusable models/validator/factory seams. Keep Goal 004 tests green.

Acceptance:

- state records the Goal 005 gate as passed;
- at least three reference semantic compositions produce valid plans;
- invalid/missing selection references reject the plan;
- focused tests pass.

### S060 - Materialize Selected Composition Into Generated Package

Purpose:

Map the S059 plan into actual generated package content rather than report-only selections.

Required behavior:

- the selected quest pattern produces concrete package quest/objective content;
- the selected dialogue intent produces concrete package dialogue/node/line content using deterministic slots/templates;
- the selected interaction pattern is represented through existing package mechanics/interactions/artifacts where supported;
- semantic/rule-pack provenance is included through the narrowest existing package artifact/provenance seam;
- different semantic packs produce meaningfully different package quest/dialogue/interaction content;
- package validation runs after materialization;
- unresolved or unsupported bindings reject package composition with diagnostics.

Do not redesign the public GamePackage schema merely to copy every authoring field. Prefer existing quest/dialogue/mechanics/applied-artifact structures and a narrow adapter. A small compatible contract addition is allowed only if no existing structure can honestly represent required provenance or binding, and it must be justified in the report.

Acceptance:

- at least three semantic variants produce validator-clean packages;
- selected declaration ids can be traced to concrete package content;
- project overlay changes concrete package content without C# changes;
- identical inputs produce identical package bytes/hash;
- Goal 032/033 package and preview regression tests remain green.

### S061 - Execute Semantic-Selected Package Behavior Headlessly

Purpose:

Run the package produced by S060 and prove the semantic-selected family is the family exercised by runtime commands and state transitions.

Required behavior:

- start the generated package through the existing runtime boundary;
- execute a bounded command sequence appropriate to the selected interaction declaration;
- observe runtime-owned quest/objective progress;
- observe dialogue evidence when the selected interaction starts/uses dialogue and the existing runtime supports it;
- observe reward/inventory/flag/encounter/completion state where declared;
- associate each runtime command/state delta with the selected rule declaration and package content id;
- report unsupported runtime bindings explicitly rather than treating report text as execution.

Required executed scenarios:

- at least three semantic variants with distinct selected quest/dialogue/interaction combinations;
- the project-overlay variant;
- one invalid/unsupported binding rejection scenario.

The headless harness may use existing runtime adapters. Do not add new UI. Do not add a new runtime command family unless a very small general-purpose primitive is strictly required and justified; if that would broaden runtime contracts, stop and report the blocker instead.

Acceptance:

- valid scenarios have `SemanticSelectedIdsExecutedInRuntime = true` or an equivalent structured field backed by command/state evidence;
- runtime evidence references the same selected ids and package hash as S059/S060;
- runtime-backed progress/reward/completion remains serializable;
- no fallback to independent Goal 004 evidence is counted as semantic-selected execution;
- Goal 004 and Goal 005 regressions remain green.

### S062 - Deterministic Replay, Save/Load And Variant Isolation

Purpose:

Prove that semantic-selected runtime composition remains deterministic and does not leak state or content between variants.

Required behavior:

- replay the same semantic layers/seed twice and compare composition plan, package and runtime evidence hashes;
- serialize runtime state after at least one meaningful state transition;
- restore through existing serializer/snapshot seams;
- verify quest progress, reward/inventory, encounter/completion and current package/map identity survive roundtrip where applicable;
- run at least three different semantic variants sequentially and prove their selected ids, package hashes and runtime state do not leak into one another;
- bounded multi-seed check with no dangling references.

Acceptance:

- deterministic replay passes;
- save/load roundtrip passes;
- cross-variant isolation passes;
- invalid composition cannot create a runnable package;
- focused tests cover failure as well as success.

### S063 - Semantic-Selected Runtime Composition Acceptance

Purpose:

Provide one headless acceptance route for the full Goal 006 chain.

Required acceptance matrix:

- core + wildland/frontier genre;
- core + gothic/mystery genre;
- core + trade/caravan genre;
- core + genre + project overlay;
- invalid semantic/rule binding rejection;
- deterministic replay;
- state serialization/snapshot roundtrip;
- cross-variant isolation;
- Goal 004 and Goal 005 regression.

Required artifacts:

```text
.llmgc/procedural/semantic-runtime-composition/semantic-runtime-composition-report.json
.llmgc/procedural/semantic-runtime-composition/semantic-runtime-composition-report.md
.llmgc/procedural/semantic-runtime-composition/semantic-runtime-composition-verification.md
```

The JSON report must provide a trace chain for each valid scenario:

```text
semantic layer/hash
-> compiled catalog/hash
-> selected semantic relation
-> selected rule declaration
-> composition plan/hash
-> generated package content/hash
-> runtime command(s)
-> runtime state delta/evidence
```

It must also include:

- actual/expected scenario validity;
- diagnostics;
- candidate/conflict leakage status;
- unsupported binding status;
- package validation result;
- semantic-selected runtime execution boolean;
- replay/save-load/isolation evidence;
- explicit no-external-execution flags;
- remaining C# primitive limitations.

Add product smoke route:

```text
semantic-runtime-composition
```

Acceptance:

- all valid variants are package-validator clean;
- all required valid variants execute their selected composition headlessly;
- invalid variant is rejected by actual diagnostics;
- deterministic/save-load/isolation checks pass;
- focused Goal 006 tests pass;
- Goal 004/005 focused regressions pass;
- `CurrentGeneratorStateDocsTests` pass;
- product smoke passes;
- final `check-all.ps1` passes with zero build warnings/errors;
- stop at `semantic_selected_runtime_composition_artifact_verification`;
- do not create S064.

## Verification Policy

Run focused tests per slice. Run the full suite once at final acceptance unless an earlier contract change/failure justifies it.

Final commands:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~SemanticRuntimeComposition|FullyQualifiedName~SemanticGuidedComposition|FullyQualifiedName~QuestDialogInteractionFamily|FullyQualifiedName~GeneratedPackageMvp|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario semantic-runtime-composition
.\.devflow\scripts\check-all.ps1
```

Report actual commands if the focused filter must be split.

## Final Gate

No WinForms launch and no local LLM are required.

After S063, stop at:

```text
semantic_selected_runtime_composition_artifact_verification
```

The user is not required to inspect report files manually. The generated artifact folder can be supplied to an external reviewer/assistant.

Do not start another goal and do not create S064.

## Final Report

Report:

- S059-S063 completion status;
- changed files;
- tests/smoke/full verification;
- whether any public package/runtime contract changed and why;
- semantic variants executed;
- exact semantic-to-runtime trace evidence;
- unsupported bindings and remaining C# primitive limits;
- artifact folder;
- confirmation that no UI/LLM/RAG/Lua/Unity/media execution was added;
- confirmation that S064 was not created.
