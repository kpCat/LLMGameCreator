# Goal 010 - Deterministic Content Generation At Scale

## Purpose

Start only after the user/assistant explicitly confirms:

```text
rule_pack_combat_faction_social_work_theft_artifact_verification passed
```

Goals 006-009 proved semantic-selected package/runtime composition, connected world state and two reusable gameplay-family sets. Goal 010 must prove the next product chain:

```text
compact data packs and grammars
-> deterministic large content expansion
-> repetition/conflict/dependency validation
-> actual GamePackage content
-> selected generated ids executed through the real runtime
-> stable scale/report/save-load evidence
```

The target is useful procedural breadth, not bulk template spam and not one LLM call per NPC, quest, event or dialogue line.

## Final Gate

Stop at exactly one final gate:

```text
content_generation_at_scale_artifact_verification
```

Do not create S092, Goal 011 or post-Goal-010 work.

## Product Slices

- S085: record Goal 009 gate and define compact content-pack/grammar acceptance contracts.
- S086: deterministic bounded expansion kernel, stable IDs, provenance and generation budgets.
- S087: NPC archetype, item/loot and region/faction-aware variation.
- S088: quest motif and event grammar expansion with exact dependencies/conflicts.
- S089: dialogue intent/voice/phrase expansion with contextual slots and repetition control.
- S090: materialize generated content into real GamePackage definitions and execute selected generated loops through production runtime services.
- S091: scale matrix, invalid/fake/leak rejection, product smoke, artifacts, state handoff and final verification.

## Architecture Boundary

### Compact packs own creative variation

The compact pack must own data such as:

- style and semantic tag sets;
- NPC archetypes, roles, traits and name/description phrase banks;
- quest motifs, objective shapes, target selectors and reward selectors;
- event motifs, triggers, conditions and supported action sequences;
- dialogue intents, voices, phrase banks and slot requirements;
- item/loot archetypes, weights, tiers and region/faction constraints;
- exclusions, requirements, compatibility relations and repetition budgets.

C# owns deterministic expansion, validation, exact binding, serialization, package assembly and acceptance. Do not add branches for named style/genre/project/faction/content ids. Three reference styles must differ because their input data differs.

### LLM role

No LLM/provider/RAG call is allowed in Goal 010. A future LLM may draft a compact pack offline, but the entire acceptance path must run from checked-in/reference data and deterministic C#.

No generated instance may require its own LLM call.

### Runtime ownership

Application may generate and validate data. Production runtime services own gameplay execution and mutable state. Do not implement an Application-side quest/dialogue/event/loot simulator.

Use the accepted fail-closed adapter shape:

- Application default runtime adapter unavailable;
- focused integration tests and product smoke inject a real adapter from the test assembly;
- real adapter constructs existing production runtime services;
- copied ids/booleans/report strings without commands and state deltas fail acceptance.

## Reference Data Packs

Provide at least three compact reference packs as JSON data outside `generator-library`, for example under:

```text
samples/content-generation-packs/
```

Required style coverage:

1. frontier/survival-oriented;
2. gothic/mystery-oriented;
3. trade/caravan-oriented.

Names are fixture metadata only. Production generation must not branch on these ids.

Each pack must remain compact enough to demonstrate combinatorial expansion rather than contain hundreds of pre-authored final instances. Include schema/version identity and deterministic source hash/provenance.

## Scale Contract

For each valid reference pack, generate at least 200 concrete content instances in total, including at minimum:

- 24 NPC instances;
- 24 quests;
- 24 events;
- 48 contextual dialogue lines/nodes;
- 48 item/loot/spawn entries;
- enough package entities/interactions/encounters/rewards to bind and execute selected loops.

Counts may exceed these minima within explicit maximum budgets. Generation must be bounded and must reject requests above configured safe caps rather than loop or allocate without limit.

At least 90 percent of the emitted instances must be derived combinations rather than verbatim authored examples. Report authored versus expanded counts structurally.

## S085 - Compact Pack Contract And Gate Record

Define a narrow versioned internal Application contract for content generation packs. Do not change public GamePackage/runtime schemas.

Required validation:

- schema/version and pack id;
- stable unique ids;
- non-empty source pools;
- positive bounded generation budgets;
- exact required/excluded semantic tags;
- known motif/archetype/voice/loot references;
- supported objective, event trigger/action and runtime binding kinds;
- positive finite loot weights and amounts;
- all required dialogue slots are declared;
- dependency ids exist;
- dependency graph is acyclic;
- conflicting requirements/exclusions reject the pack;
- no path, script, provider, runtime C# type or executable payload injection.

Malformed JSON must produce deterministic relative-path diagnostics, not an unhandled exception. No absolute machine paths may enter artifacts.

## S086 - Deterministic Expansion Kernel

Implement one generic deterministic expansion service consuming pack + seed + bounded options.

Required behavior:

- identical pack/seed/options produce byte-stable content and hashes;
- different seeds produce meaningful combinations while respecting all constraints;
- IDs are stable, safe and derived from pack/seed/archetype/motif/ordinal provenance;
- enumeration order is explicitly sorted before hashing/serialization;
- RNG state is local and seed-controlled;
- no timestamps, GUIDs, machine names, temp paths or hash-randomized collection order;
- source pack data is immutable;
- every generated instance records source pack, source archetype/motif/intent and generation seed/ordinal;
- all loops have explicit attempt/budget limits;
- exhausted valid combination space produces a clear diagnostic instead of duplicate spam or an infinite loop.

Generate a structured catalog/model suitable for package materialization, not only counts or prose.

## S087 - NPC, Item And Loot Variation

Generate data-driven NPC and item/loot variants.

NPC requirements:

- archetype, role, faction/region, traits, display name and short description;
- exact region/faction references;
- compatible and excluded trait enforcement;
- no C# conditionals for the three reference style ids;
- deterministic distribution evidence across archetypes/roles/traits.

Item/loot requirements:

- item archetype/tier/tags/value or supported metadata;
- positive weighted deterministic selection;
- loot/spawn entries bind only to generated/existing item ids;
- region/faction/encounter constraints resolve;
- no impossible or empty loot table is accepted;
- reward and requirement items needed by generated loops are reachable.

Use existing GamePackage structures where possible. Preserve source provenance through the narrowest existing metadata/generated-content seam.

## S088 - Quest And Event Grammars

Expand compact motifs into concrete quest and event content.

Quest requirements:

- at least three reusable objective shapes across the valid matrix;
- concrete NPC/item/location/encounter/reward bindings;
- objective ordering/dependencies are acyclic and reachable;
- all required interactions and runtime targets are materialized;
- reward/requirement balance is minimally coherent: required items are obtainable before consumption and rewards are not dangling;
- selected content can be executed with existing runtime primitives.

Event requirements:

- data-declared supported trigger, condition and action sequence;
- exact target and output ids;
- no arbitrary code/Lua execution;
- event conflicts/exclusions and prerequisite ordering are validated;
- at least one event consequence per style is reflected through an existing runtime-owned flag/item/reputation/encounter/quest state primitive.

If a motif/action cannot be represented honestly by existing primitives, reject it as unsupported. Do not report prose as execution.

## S089 - Dialogue Variation And Repetition Control

Generate contextual dialogue from intent + voice + phrase-bank data.

Required behavior:

- every required slot resolves from actual generated context;
- no unresolved `{slot}` or equivalent token enters package content;
- line/node/choice ids are stable and exact;
- speaker/NPC, quest, faction, item, location and event refs resolve;
- choices use only supported existing effects/outputs;
- voices affect data selection, not C# branches;
- at least three intent families and three voice/archetype combinations appear in the valid matrix.

Repetition control must be structural, deterministic and reported:

- no exact duplicate final NPC display names within a generated pack unless explicitly allowed;
- no exact duplicate quest title+objective signature;
- no exact duplicate dialogue line after normalized whitespace/case;
- no exact duplicate event signature;
- do not reuse a combination until its eligible pool is exhausted;
- configurable maximum share for one archetype/motif/voice/phrase family;
- report top-frequency shares, unique counts, duplicate counts and exhausted-pool diagnostics;
- a deliberately tiny/exhausted pack must fail or degrade only according to an explicit declared fallback policy, never silently claim healthy diversity.

Do not invent subjective LLM quality scoring. Use deterministic measurable diversity/repetition invariants.

## S090 - GamePackage Materialization And Real Runtime Execution

Materialize generated catalog content into actual existing GamePackage definitions.

Required package audit:

- generated NPC/entity ids resolve to maps/regions/factions/interactions;
- quest objectives, dialogues, choices, events, items, loot, encounters and rewards resolve exactly;
- generated content/provenance hashes match the catalog;
- package validator is clean for all valid packs;
- no public package/runtime schema change;
- identical input produces identical package JSON/hash;
- three style packs produce meaningfully different package content without production C# changes.

For each valid style, select at least two deterministic generated content threads and execute a bounded loop through existing real runtime services. Across the matrix prove:

```text
generated NPC/dialogue or event
-> generated quest/interaction/encounter
-> objective/progress or supported consequence
-> generated reward/loot/state delta
-> full-state save/load
```

Runtime evidence must reference the same generated ids and package hash. Require exact command-to-generated-declaration correlation, actual events/state deltas, reward evidence and full `GameRuntimeState` serialization/snapshot roundtrip.

Run valid styles sequentially through the same adapter instance and prove no catalog/package/runtime state leakage. Add a concrete injected-leak negative fixture.

## S091 - Scale Acceptance And Final Artifacts

Required valid matrix:

- all three reference packs;
- same-pack same-seed replay;
- same-pack multiple-seed variation;
- sequential cross-pack isolation;
- package validation/materialization;
- at least six total generated runtime threads;
- full-state save/load.

Required invalid/fake matrix includes at least:

1. wrong schema/version;
2. malformed JSON;
3. duplicate source ids;
4. missing archetype/motif/voice/loot reference;
5. cyclic quest/event dependency;
6. semantic required/excluded conflict;
7. unresolved dialogue slot;
8. nonpositive/NaN/infinite loot weight or amount;
9. impossible/dangling reward or requirement;
10. unsupported trigger/action/runtime binding;
11. generation budget above safe cap;
12. exhausted combination pool without allowed fallback;
13. repetition limit breach;
14. command not covered by selected generated declaration;
15. fake runtime success;
16. save/load mismatch;
17. cross-pack catalog/runtime leakage;
18. expectation-only invalid fixture with its mutation removed must make the expected-invalid matrix fail.

`ExpectedValid` is expectation metadata only. Actual validity must derive from pack parsing, validation, expansion, repetition metrics, package audit and real runtime evidence.

## Artifacts

Write exactly:

```text
.llmgc/procedural/content-generation-scale/content-generation-scale-report.json
.llmgc/procedural/content-generation-scale/content-generation-scale-report.md
.llmgc/procedural/content-generation-scale/content-generation-scale-verification.md
```

JSON must be deserializable and structurally asserted by tests. It must include:

- accepted/manual gate/prior gate/completed slices;
- pack source hashes and seeds;
- authored/expanded counts by content kind;
- catalog/package/runtime hashes;
- deterministic replay and multi-seed variation;
- repetition metrics and distribution shares;
- conflict/dependency/package diagnostics;
- selected generated runtime trace chains;
- save/load and cross-pack isolation evidence;
- invalid scenario causal diagnostics;
- all external execution flags false;
- bounded claims and remaining primitive limits.

Do not embed every full generated object in the main report if that makes review impractical. Include deterministic hashes, counts, representative samples and exact runtime-selected objects sufficient to audit correctness. Actual package/catalog objects must still exist in memory and be structurally tested.

## Product Smoke

Add scenario:

```text
content-generation-scale
```

Product smoke must:

- load all reference JSON packs;
- expand and package at scale;
- inject the real runtime adapter;
- generate the artifacts;
- deserialize JSON;
- structurally assert minimum counts, provenance, zero prohibited duplicates, diversity caps, clean packages, six runtime threads, real runtime boundaries, reward/state deltas, save/load, determinism, isolation and causal invalid diagnostics;
- assert all external execution flags false.

No raw `Assert.Contains` acceptance based only on report text.

## Tests

Add focused tests for:

- contract parsing/validation and malformed files;
- deterministic stable expansion;
- different-seed variation;
- stable safe ids/provenance;
- bounded caps and exhausted pools;
- every NPC/item/loot/quest/event/dialogue binding family;
- dependency cycles/conflicts/unresolved slots;
- measurable repetition-control invariants;
- actual package validator and byte/hash stability;
- exact generated-id runtime execution;
- reward/progress/event consequence state;
- fake success/save-load/leak rejection;
- removing an invalid mutation breaks the expected-invalid matrix;
- Goal 007-009 focused regressions and state-doc guards.

Prefer a few high-value theory/matrix tests over hundreds of one-assert test methods.

## State Update

Record:

```text
rule_pack_combat_faction_social_work_theft_artifact_verification passed
```

Record Goal 010/S085-S091 completion, but leave required:

```text
content_generation_at_scale_artifact_verification
```

Do not mark it passed and do not recommend/create Goal 011 in this run.

## Verification

Run focused tests using the actual implemented class names, with at least this coverage:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~ContentGenerationScale|FullyQualifiedName~RulePackCombatFactionSocialWorkTheft|FullyQualifiedName~RulePackGameplayFamily|FullyQualifiedName~ConnectedWorldTravel|FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Run product smoke:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario content-generation-scale
```

Run once at final acceptance:

```powershell
.\.devflow\scripts\check-all.ps1
```

Scan changed/generated files for mojibake, absolute paths, timestamps/GUIDs in deterministic artifacts and accidental `S092|Goal 011|goal_011` markers. Exclude this Goal/task prohibition text from the marker scan.

## Stop Conditions

Stop with a blocker report rather than weaken acceptance if:

- existing package/runtime primitives cannot honestly materialize and execute the selected generated loops;
- a public GamePackage/runtime command/state schema change is required;
- repetition control can only be claimed through subjective prose;
- scale output is only a report projection rather than real generated package content;
- runtime evidence cannot reference the same generated ids/package hash;
- `.sln`/`.csproj` or project-reference changes are required;
- full verification exposes an unrelated pre-existing failure.

Do not shrink scale minima, remove invalid scenarios or substitute copied ids/booleans merely to make the gate green.

## Hard Limits

- No S092 or Goal 011.
- No git commands or branch operations.
- No WinForms/UI/Designer work.
- No Unity, asset/media generation or export work.
- No LLM/RAG/provider calls.
- No arbitrary Lua or generator execution.
- No `generator-library` edits.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` edits.
- No named style/genre/project/content-specific production C# branches.
- No bulk pre-authored final content disguised as procedural generation.

## Final Report

Report:

- S085-S091 status;
- changed files;
- compact pack sizes and generated counts;
- deterministic expansion/provenance design;
- repetition metrics and cap results;
- exact package binding and runtime-selected trace chains;
- six or more real generated runtime threads and state deltas;
- invalid/fake/leak causal diagnostics;
- replay, multi-seed, save/load and isolation results;
- artifact folder and report hash;
- focused/smoke/full verification totals;
- confirmation that public schemas, UI, Unity, Lua/provider/media, generator-library and project files were untouched;
- confirmation that S092/Goal 011 were not created.
