# Goal 009 - Rule-Pack Combat, Faction, Social, Work And Theft Foundations

## Purpose

Goal 009 starts only after the user/assistant accepted:

```text
rule_pack_gameplay_family_artifact_verification passed
```

Goal 008 proved inventory, item-use, equipment, crafting, trading and status foundations through the real runtime. Goal 009 must prove the second reusable gameplay-family set:

```text
validated rule-pack declarations
-> exact package/runtime bindings
-> real combat/faction/dialogue/interaction/container commands
-> runtime-owned encounter/reputation/social/work/theft state deltas
-> deterministic full-state save/load
-> invalid declaration/runtime/bypass rejection
```

This is a bounded headless product vertical slice. It is not a broad combat rewrite, NPC simulation, relationship system, jobs economy or stealth AI system.

## Final Gate

Stop at exactly one final gate:

```text
rule_pack_combat_faction_social_work_theft_artifact_verification
```

Do not create S085, Goal 010 or any post-Goal-009 work.

## Product Slices

- S078: record Goal 008 gate and establish the fail-closed acceptance/runtime-adapter spine.
- S079: deterministic combat/encounter/ability declarations, package bindings and real runtime execution.
- S080: faction/reputation plus social dialogue-choice declarations and runtime consequences.
- S081: bounded work-contract and theft-consequence compositions over existing interaction/container/faction primitives.
- S082: combined second-family gameplay loop with ordered real runtime evidence.
- S083: invalid/bypass rejection, deterministic replay, full-state save/load and isolation.
- S084: product smoke, artifacts, state docs and final verification.

## Architecture Boundary

### Existing C# primitives own execution

Use existing production runtime boundaries wherever possible:

- `GameRuntimeService` and `GameRuntimeStateFactory`;
- `EncounterRuntimeService` and `EncounterAiService`;
- `FactionRuntimeService`;
- `DialogueRuntimeService`;
- `InteractionRuntimeService`;
- `ContainerRuntimeService`;
- existing requirement/cost/output services;
- `RuntimeStateSerializer` and `RuntimeSnapshotStore`.

Application may define deterministic declarations, binding audit, evidence contracts and report rendering. It must not implement a second combat, reputation, dialogue, work or theft simulator.

The Application default runtime adapter must be unavailable/fail-closed. Focused integration tests and product smoke inject a real adapter from the test assembly, following the accepted S077A pattern.

### Data/rule-pack ownership

Rule-pack declarations own:

- encounter and ability selection;
- participant/target ids;
- faction and reputation consequence ids/amounts;
- dialogue/social choice ids and effects;
- work interaction, requirement, wage/reward and completion ids;
- theft container/item ids and consequence sequence;
- ordered command composition and provenance.

No C# branch may select behavior by scenario, genre, project, faction or content id. Scenario fixtures may select data declarations, but production execution remains generic.

### Honest bounded meaning of work and theft

For Goal 009:

- `work` means a validated data-driven interaction or quest-like contract using existing requirements, costs, outputs, flags/items/reputation and runtime commands;
- `theft` means a validated data-driven sequence using existing container transfer plus explicit rule-pack consequences such as a theft flag and reputation penalty.

Do not claim schedules, wages over time, employers, detection chance, witnesses, ownership law, stealth AI or relationship simulation unless existing runtime actually executes them. List those as remaining primitive limitations.

## Required Valid Scenarios

Implement at least these deterministic scenarios with distinct seeds/evidence hashes.

### 1. `combat_turn_based_encounter`

- Start an exact package encounter through the real runtime.
- Execute at least one valid player attack/ability.
- Execute an enemy turn through real AI or an explicitly valid enemy command.
- Prove turn order, resource/health delta, action history and encounter active/resolved state.
- Prove package encounter, participant, ability, stat/resource and reward refs are exact.

### 2. `combat_resolution_reward`

- Execute a bounded deterministic encounter to resolution.
- Prove defeated/alive state, encounter completion and actual reward/output state where declared.
- Do not use `ResolveEncounter` as a report-only shortcut before proving at least one real combat action.

### 3. `faction_reputation_change`

- Execute real reputation commands for an exact package faction.
- Prove before/after reputation, clamping and runtime event evidence.
- Preserve exact faction state through save/load.

### 4. `social_dialogue_reputation_consequence`

- Open a real package dialogue and execute an exact choice through `GameRuntimeService`.
- Choice effects must produce a real flag/item/reputation or other supported runtime consequence.
- Prove dialogue id/node/choice history plus attributable state delta.

### 5. `work_contract_reward`

- Execute a selected work-contract declaration through an existing interaction/dialogue/quest/transaction primitive.
- Prove requirement satisfaction, exact interaction/contract id, wage/reward output and completion flag or equivalent state.
- Prove missing requirement rejection in an invalid scenario.

### 6. `theft_container_reputation_consequence`

- Open an exact container and take an exact declared item with real container commands.
- Execute the declared theft consequence sequence through real flag/reputation commands.
- Prove container decrease, player inventory increase, theft flag and faction reputation penalty.
- The report must call this an explicit rule-pack consequence sequence, not claim dynamic detection/stealth AI.

### 7. `combined_combat_social_work_theft_loop`

Execute a coherent bounded sequence using selected declarations, for example:

```text
social choice accepts contract
-> work/combat action
-> encounter resolution/reward
-> work completion/wage
-> container theft
-> theft flag and reputation consequence
```

Every required command must succeed and have attributable runtime events/state deltas. The complete final `GameRuntimeState` must survive save/load.

## Required Invalid Scenarios

Add at least these causally rejected fixtures:

1. `invalid_missing_encounter_or_participant_ref`.
2. `invalid_missing_ability_or_resource_ref`.
3. `invalid_combat_wrong_turn_or_target`.
4. `invalid_missing_faction_ref`.
5. `invalid_dialogue_or_choice_ref`.
6. `invalid_work_requirement_unmet`.
7. `invalid_theft_container_or_item_ref`.
8. `invalid_theft_nonpositive_amount`.
9. `invalid_command_not_covered_by_declaration`.
10. `invalid_fake_runtime_success`.
11. `invalid_save_load_mismatch`.
12. `invalid_cross_scenario_state_leakage`.

Invalid package/binding scenarios must not run. Runtime-invalid scenarios must run through the real runtime and fail with its actual diagnostics/events.

`ExpectedValid = false` is expectation metadata only. Compute `ActualValid` using the same binding/runtime/evidence predicate as valid scenarios. If an invalid mutation is removed, the expected-invalid matrix must fail.

## Exact Binding Audit

Before runtime, validate the selected scenario subset, not every global declaration indiscriminately.

Audit at least:

- every selected declaration id exists exactly once and belongs to a selected family;
- every command is covered by an exact selected declaration;
- encounter, participant, ability, stat, resource, status and reward refs exist;
- ability effects/costs reference supported package/runtime ids;
- faction ids and reputation amounts/bounds are valid;
- dialogue, node, choice, interaction and effect refs exist;
- work requirements/outputs and completion ids resolve;
- container inventory is actually a container and the stolen item exists in its initial state;
- theft consequence faction/flag/amount ids resolve;
- no runtime delta is stored as immutable source declaration data;
- no unknown or unselected declaration/command target can enter runtime evidence.

Use stable machine-readable diagnostic codes.

## Real Runtime Adapter

The real test/smoke adapter must:

1. Construct the production runtime services.
2. Create state through `GameRuntimeStateFactory`/`GameRuntimeService`.
3. Seed required fixture state using existing runtime commands or package initial state.
4. Translate command specs to real `GameRuntimeCommand` values.
5. Execute through `GameRuntimeService.Execute` or an existing real service only where no public command exists.
6. Capture real `GameRuntimeResult` success, diagnostics and event types.
7. Compute evidence from before/after real `GameRuntimeState` snapshots without mutating gameplay state itself.
8. Serialize and restore the full runtime state with `RuntimeStateSerializer` and verify `RuntimeSnapshotStore` success.

Application acceptance must reject unavailable or fabricated adapters. Copied ids, type-name strings and success booleans without exact command/state evidence are insufficient.

## Required Runtime Evidence

For each valid scenario include:

- scenario id, seed and deterministic hash;
- selected family/declaration ids;
- exact package/runtime binding audit;
- runtime boundary identity;
- ordered commands and command-to-declaration correlation;
- real event types and diagnostic codes;
- encounter before/after: id, active/resolved, round/turn, participants, resources, statuses and action history;
- faction reputation before/after;
- dialogue id/node/choice/open/history before/after;
- work requirement/result/reward/completion evidence;
- container/player inventory before/after and theft consequence evidence;
- completion/reward flags/items;
- full runtime state and restored-state hashes;
- serializer and snapshot-store success;
- save/load boolean and diagnostics.

Evidence categories must remain command-specific. Generic state differences must not masquerade as combat, social, work, theft or reputation evidence for unrelated commands.

## Determinism And Isolation

- Re-run the combined scenario and compare plan/spec, package, command evidence and full-state hashes.
- Run all valid scenarios sequentially through the same adapter instance and prove no scenario retains previous encounter, dialogue, faction, inventory, flags or command history.
- Inject previous-scenario evidence in a negative fixture and require rejection.
- Do not include timestamps, GUIDs, absolute paths, machine names or temporary directories in deterministic artifacts.

## Artifacts

Write exactly:

```text
.llmgc/procedural/rule-pack-combat-faction-social-work-theft/rule-pack-combat-faction-social-work-theft-report.json
.llmgc/procedural/rule-pack-combat-faction-social-work-theft/rule-pack-combat-faction-social-work-theft-report.md
.llmgc/procedural/rule-pack-combat-faction-social-work-theft/rule-pack-combat-faction-social-work-theft-verification.md
```

JSON must be deserializable and structurally asserted by tests.

Overall report fields must include:

- `accepted`;
- `manualGate = rule_pack_combat_faction_social_work_theft_artifact_verification`;
- `goal008GateRecorded = true`;
- completed S078-S084;
- valid/invalid counts;
- binding/runtime/save-load/determinism/isolation/fake-success booleans;
- external-execution flags all false;
- explicit bounded work/theft semantics and remaining primitive limits.

## Product Smoke

Add scenario:

```text
rule-pack-combat-faction-social-work-theft
```

Product smoke must inject the real runtime adapter, generate artifacts, deserialize JSON and structurally assert:

- real runtime and full-state serializer/snapshot evidence;
- combat action and resolution evidence;
- social dialogue consequence;
- work reward/completion;
- theft transfer plus reputation/flag consequence;
- combined ordered loop;
- invalid causal diagnostics;
- deterministic/save-load/isolation success;
- all external execution flags false.

No raw `Assert.Contains` acceptance based only on JSON text.

## State Update

Record the user-confirmed prior gate:

```text
rule_pack_gameplay_family_artifact_verification passed
```

Record Goal 009/S078-S084 completion, but leave this gate required:

```text
rule_pack_combat_faction_social_work_theft_artifact_verification
```

Do not mark it passed. Do not recommend or create Goal 010 in this run.

## Verification

Run focused tests:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~RulePackCombatFactionSocialWorkTheft|FullyQualifiedName~RulePackGameplayFamily|FullyQualifiedName~ConnectedWorldTravel|FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Run product smoke:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario rule-pack-combat-faction-social-work-theft
```

Run once at final acceptance:

```powershell
.\.devflow\scripts\check-all.ps1
```

Search implementation, tests, state and generated artifacts for mojibake and accidental `S085`, `Goal 010` or `goal_010` work. Exclude this Goal document and its CODEX_GOAL wrapper because their prohibition text necessarily contains those markers.

## Stop Conditions

Stop with a blocker report instead of weakening acceptance if:

- existing runtime cannot honestly execute a required family/composition;
- a new public runtime command/state or GamePackage schema is required;
- real full-state save/load cannot preserve required evidence;
- work/theft can only be presented as report text without actual generic runtime deltas;
- invalid fixtures can only fail through expectation metadata;
- a `.sln`/`.csproj` or project-reference change is required;
- full verification exposes an unrelated pre-existing failure.

Do not implement a second Application gameplay simulator. Do not overclaim unsupported social/work/theft depth.

## Hard Limits

- No S085 or Goal 010.
- No git commands or branch operations.
- No WinForms/UI/Designer work.
- No Unity, provider/media, LLM/RAG, Lua or generator execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` edits.
- No genre/project/faction/content-specific C# branches.
- No broad combat, jobs, relationship, crime, stealth or economy subsystem.

## Final Report

Report:

- S078-S084 status;
- changed files;
- exact declarations and package/runtime bindings;
- exact real runtime services/commands used;
- valid scenario command/state evidence;
- invalid scenarios and causal diagnostic codes;
- bounded meaning and remaining limits of social/work/theft;
- replay, isolation and full-state save/load evidence;
- artifact folder and report hash;
- focused/smoke/full verification results;
- confirmation that public schemas, UI, Unity, Lua/provider/media, generator-library and project files were untouched;
- confirmation that S085/Goal 010 were not created.
