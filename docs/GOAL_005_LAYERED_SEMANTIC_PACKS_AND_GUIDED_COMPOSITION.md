# Goal 005 - Layered Semantic Packs And Semantic-Guided Composition

## Goal

Turn the existing semantic catalog foundation into a practical, deterministic authoring and generation input.

This goal must prove that compact semantic packs can be layered, validated, compiled and used to produce meaningfully different quest, dialogue, interaction and generated-content choices without runtime LLM/RAG execution and without bespoke C# code for each semantic term.

The result is an authoring/generation foundation, not a semantic editor UI and not a large external knowledge-base import.

## Required Starting Condition

The user prompt must explicitly contain:

```text
manual_quest_dialog_interaction_family_verification passed.

Artifact review confirmed:
- distinct quest pattern variants;
- contextual non-empty dialogue intent output;
- inspect and resolve_challenge interaction execution;
- invalid rule-pack rejection;
- preserved runtime-backed progress, reward and completion;
- no external LLM/provider/Lua/Unity/media execution.
```

This evidence was obtained from the deterministic Goal 004 acceptance artifacts. No WinForms run is required to start Goal 005.

## Context Budget Rule

Read first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. this goal file

Read `docs/CURRENT_GENERATOR_STATE.md` only because S054 and S058 update the state/gate handoff.

Read these only where directly required:

- `docs/SEMANTIC_PACK_AND_RAG_STRATEGY.md`
- `docs/ARCHITECTURE_STRATEGY_AND_BOUNDARIES.md`
- `docs/AGENT_CONTEXT_BUDGET_POLICY.md`
- `docs/EXTENSION_RULE_PACK_CONTRACT_V1.md`

Do not read historical slice packs or old reports unless a failing test or direct code reference requires them.

## Existing Seams To Reuse

Reuse and extend the existing behavior instead of creating a parallel semantic system:

- `SemanticCatalog`
- `SemanticCatalogTerm`
- `SemanticCatalogRelation`
- `SemanticCatalogService`
- `SemanticGenerationContextPreviewService`
- `semantic_pack_v1`
- Goal 004 quest/dialogue/interaction contracts and acceptance behavior
- existing deterministic JSON/Markdown sidecar conventions
- existing product-smoke routing

Do not append substantial semantic logic to the already large `QuestDialogInteractionFamilyAcceptanceService`. Prefer a separate semantic compiler/composer and narrow adapters over another monolithic acceptance service. If Goal 004 contracts are inaccessible, a small behavior-preserving extraction is allowed and must retain Goal 004 regression coverage.

## Architecture Rules

- Raw semantic layers are authoring inputs.
- Only a validated compiled semantic catalog is consumed by deterministic generators.
- `candidate`, `deprecated`, `conflict` and `invalid` terms must not silently enter active generation.
- Imported and LLM-proposed candidates are untrusted until explicitly represented as accepted/known input.
- Layer precedence and conflict behavior must be explicit and deterministic.
- Semantic relations must be game-useful and validated, not arbitrary text links.
- Semantic selection must be seed-stable and traceable in reports.
- Adding a new safe term or relation instance must not require C#.
- Adding an entirely new semantic kind or executable primitive may require a reviewed C# contract change; this goal does not attempt arbitrary extensibility.

## Non-Goals

- No WinForms semantic editor.
- No Runtime Preview feature work.
- No RAG index/vector database.
- No external dataset download or bulk WordNet/ConceptNet/Wikidata/OSM import.
- No LLM/provider execution.
- No Unity or media execution.
- No arbitrary Lua execution.
- No GamePackage or public runtime contract redesign.
- No broad rewrite of the S028 semantic foundation.
- No S059 or later slice.

## Product Slices

### S054 - Close Goal 004 Gate And Define Layered Semantic Pack Contract V1

Purpose:

- record the supplied Goal 004 artifact verification as passed;
- define a compact layered semantic-pack contract that builds on `semantic_pack_v1` and existing semantic catalog models;
- establish deterministic layer identities and provenance.

Required layers:

- `core`;
- `genre`;
- `project`;
- `imported_candidate`;
- `llm_candidate`.

Required contract behavior:

- every layer has a safe stable id, layer kind, source/provenance and ordered terms/relations;
- known/accepted terms are distinguished from quarantined candidates;
- aliases, tags, generation hints and constraints may be added through compatible models where needed;
- unsafe ids, rooted/traversal paths and malformed declarations are rejected with diagnostics;
- the contract is documented succinctly in `docs/SEMANTIC_PACK_CONTRACT_V1.md`.

Acceptance:

- Goal 004 manual gate is recorded as passed in the state pair;
- valid layer fixtures parse and normalize deterministically;
- invalid layer ids/statuses/declarations are rejected;
- no duplicate parallel `SemanticCatalog` model is introduced;
- focused tests pass.

### S055 - Deterministic Semantic Layer Compiler And Conflict Policy

Purpose:

Compile semantic layers into one runtime/generator-safe catalog.

Required precedence:

```text
project > genre > core
```

Candidate layers do not override accepted layers and do not enter active generation merely because they exist.

Required behavior:

- stable normalization and ordering;
- explicit provenance retained for every compiled term/relation;
- deterministic handling of duplicate identical declarations;
- diagnostics for incompatible kind/status/meaning collisions;
- validation of relation endpoints;
- allow-list of game-useful relation kinds, including at least `requires`, `excludes`, `implies`, `compatible_with`, `preferred_in_tone`, `forbidden_in_tone`, `prefers_quest_pattern`, `prefers_dialogue_intent` and `prefers_interaction_family`;
- no candidate/deprecated/conflict/invalid leakage into the active compiled view;
- byte-stable JSON and Markdown output for identical input.

Required compiled outputs:

```text
.llmgc/semantic/compiled-semantic-pack.json
.llmgc/semantic/compiled-semantic-pack-report.md
```

Acceptance:

- precedence, candidate quarantine, duplicate collapse, conflicts and unknown relation endpoints are covered by focused tests;
- same input produces identical bytes/hash;
- compiled output is consumable through a narrow existing-or-compatible semantic catalog boundary.

### S056 - Usable Reference Packs And Project Overlay

Purpose:

Provide small, audited packs that answer how a project obtains a semantic base without importing a giant knowledge graph.

Add compact JSON reference inputs under a clear `generator-library/semantic-packs/` structure:

- one core pack;
- at least three deliberately different genre/example packs;
- one project overlay example;
- one quarantined candidate example showing imported/LLM-proposed terms that are not active until accepted.

Reference packs must be small and game-useful. They should cover enough existing kinds to drive Goal 005 acceptance, including:

- tone;
- biome or location mood;
- faction/entity role;
- quest motif;
- dialogue intent;
- item affordance;
- interaction preferences through relations/hints.

Required behavior:

- files are deterministic inputs, not generated test-only strings hidden in C#;
- project overlay can override or extend a genre/core choice according to the S055 policy;
- candidate promotion is an explicit data/status change, not an implicit compiler guess;
- a concise authoring example explains how to add a project term and relation without C# and without running an LLM.

Acceptance:

- every shipped reference pack validates and compiles;
- quarantined candidate terms remain absent from active compiled generation;
- at least one project overlay changes a compiled semantic choice without a C# change;
- no large external dataset is vendored.

### S057 - Semantic-Guided Quest, Dialogue And Interaction Composition

Purpose:

Use the compiled semantic catalog to guide the existing Goal 004 family declarations and procedural choices.

Required composition behavior:

- choose compatible quest patterns from quest motifs and semantic relations/hints;
- choose dialogue intents/templates from tone, role, quest and interaction context;
- choose interaction families from affordances and semantic relations/hints;
- bind item, NPC, encounter and location semantic roles where existing generated inputs permit it;
- use deterministic seed-based tie-breaking when more than one valid choice remains;
- emit trace evidence explaining which terms/relations caused each choice;
- reject or diagnose unsatisfied `requires`, `excludes`, forbidden-tone conflicts and unknown references;
- preserve Goal 004 runtime-backed progress/reward/completion evidence where the existing harness supports it.

Required scenarios:

- at least three distinct semantic compositions using the same generation machinery but different genre/project packs;
- at least one project-overlay variant;
- at least one invalid/conflicting semantic combination;
- at least one quarantined-candidate non-leakage case.

Do not hardcode behavior as `if fantasy`, `if zombie`, or checks against particular reference-pack ids. Selection must be driven by normalized kinds, tags, relations, constraints and seed.

Acceptance:

- the three valid scenarios differ in meaningful selected quest/dialogue/interaction/content semantics, not only titles or ids;
- every selected id exists in the compiled catalog or referenced Goal 004 rule pack;
- composition is deterministic;
- invalid/conflicting input fails with actionable diagnostics;
- no LLM/RAG/provider call occurs.

### S058 - Semantic-Guided Composition Acceptance And Handoff

Purpose:

Prove the entire Goal 005 chain headlessly and leave compact artifacts that can be externally reviewed without running the application.

Required acceptance matrix:

- core + genre A;
- core + genre B;
- core + genre C;
- core + genre + project overlay;
- candidate quarantine;
- invalid/conflict rejection;
- repeated identical run byte/hash stability;
- multiple deterministic seeds with no dangling semantic references.

The multiple-seed check should be bounded and useful, not a huge brute-force test. Prefer a modest deterministic matrix/property-style test that catches ordering, dangling-reference and leakage errors.

Required reports:

```text
.llmgc/procedural/semantic-guided-composition/semantic-guided-composition-report.json
.llmgc/procedural/semantic-guided-composition/semantic-guided-composition-report.md
.llmgc/procedural/semantic-guided-composition/semantic-guided-composition-verification.md
```

The report must include:

- input layers and hashes;
- compiled catalog hash;
- active versus quarantined counts;
- conflicts/diagnostics;
- selected quest/dialogue/interaction semantics per scenario;
- trace/provenance for selections;
- deterministic replay evidence;
- explicit external-execution flags proving no LLM, RAG, provider, Lua, Unity or media execution;
- remaining limitations and what still requires a C# primitive.

Add a product-smoke route named `semantic-guided-composition` unless an existing exact route already exists.

Acceptance:

- focused Goal 005 tests pass;
- Goal 004 focused regression tests still pass;
- semantic catalog foundation tests still pass;
- `CurrentGeneratorStateDocsTests` pass;
- `run-product-smoke.ps1 -Scenario semantic-guided-composition` passes;
- final `check-all.ps1` passes with zero build warnings/errors;
- state recommends only `semantic_guided_composition_artifact_verification`;
- stop after S058 and do not create S059.

## Verification Policy

Run focused tests after each slice. Do not run `check-all.ps1` after every slice unless a broad contract change or failure justifies it.

At the end run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~SemanticCatalog|FullyQualifiedName~SemanticGuided|FullyQualifiedName~QuestDialogInteractionFamily|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario semantic-guided-composition
.\.devflow\scripts\check-all.ps1
```

If the exact focused filter must be split because of the local test runner, report the actual commands and results.

## Final Acceptance Gate

This goal requires no WinForms launch and no local LLM.

After S058, stop at:

```text
semantic_guided_composition_artifact_verification
```

The user is not required to inspect Markdown or JSON manually. The generated `semantic-guided-composition` folder can be supplied to an external reviewer/assistant for acceptance.

Do not continue to another feature goal.

## Final Report

The final Codex report must state:

- S054-S058 completion status;
- changed files;
- tests and product smoke commands/results;
- reference packs added;
- exact precedence/conflict/candidate policy implemented;
- which generated choices are now semantic-pack driven;
- whether Goal 004 behavior regressed;
- whether any C# executable/gameplay primitive was added and why;
- what remains impossible without a new C# primitive;
- confirmation that S059 was not created.
