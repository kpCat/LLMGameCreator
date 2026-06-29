# Goal 030 Semantic Artifact Contract Registry — Spec V1

Status: planning/spec document for the next composite goal after Goal 029.

Gate marker:

```text
semantic_artifact_contract_registry_verification
```

Root evidence folder:

```text
.llmgc/procedural/goal-030-semantic-artifact-contract-registry/
```

## Why this goal exists

After modular generator kernel / parallel readiness, the next bottleneck is not another isolated gameplay feature. The next bottleneck is that future systems need a common contract registry that says:

- what artifact kinds exist;
- which generator modules can produce or consume them;
- what semantic scopes/tags they require;
- which dependencies and compatibility constraints they have;
- what is ready now, blocked, optional, future-required, or module-absent;
- what deterministic semantic expansion slots should be created before GamePackage assembly.

Without this layer, NPCs, quests, items, factions, biome events, settlements, economy, combat and world systems will keep growing as isolated vertical paths.

## Non-goals

Goal 030 must not:

- change public `GamePackage` schema;
- add WinForms/UI pages;
- add Runtime behavior;
- add Unity export behavior;
- call or integrate LLM/provider/RAG;
- execute Lua;
- change `generator-library` contents;
- add external NuGet dependencies;
- generate media;
- make broad package assembly changes.

## Required model concepts

The implementation may choose exact class names based on existing repository style, but it should cover these concepts.

### Artifact contract descriptor

A contract descriptor represents a generator artifact contract or artifact family. Required meaning:

- stable id;
- display name;
- version;
- artifact kind;
- domain/system area;
- produced artifact types;
- consumed artifact types;
- required semantic scopes;
- optional semantic scopes;
- capability tags;
- compatibility tags;
- dependencies;
- module owner / module id if known;
- lifecycle status: ready, optional, blocked, future-required, deprecated;
- validator/diagnostic code prefix;
- promotion notes / proof notes.

### Semantic pack descriptor

A semantic pack descriptor represents semantic authoring input, not runtime data. Required meaning:

- stable id;
- profile/family ids it supports;
- semantic scopes it contributes to;
- tags;
- relation hints;
- expansion hints;
- blocked/future capability hints;
- deterministic ordering key.

### Compatibility plan

A compatibility plan answers: “For this profile/capability/semantic pack selection, what artifact contracts can be used, what is blocked, and what expansion slots should be staged?”

Required contents:

- input profile/family id;
- selected contract ids;
- selected semantic pack ids;
- dependency order;
- missing dependencies;
- conflicts;
- blocked/future-required items;
- module absence behavior;
- semantic expansion slots;
- diagnostics with stable machine-readable codes;
- deterministic summary.

### Semantic expansion slot

A semantic expansion slot is not a generated GamePackage object. It is a deterministic planning record for later contract-bound artifact generation.

Minimum slot families:

- NPC/actor archetype variation;
- faction/reputation relation;
- quest motive/objective pattern;
- dialogue tone/localization/string-table hint;
- biome/weather/hazard/event hint;
- item/resource/recipe/loot hint;
- combat/progression/ability hint;
- settlement/region/route/landmark hint.

## Required evidence artifacts

Goal 030 should write compact deterministic artifacts under the root evidence folder:

```text
registry-summary.json
compatibility-matrix.json
semantic-expansion-plan-frontier.json
semantic-expansion-plan-gothic.json
semantic-expansion-plan-caravan.json
semantic-artifact-contract-registry-report.md
```

If existing profile ids differ, use the actual current ids, but keep three distinct accepted-style scenarios.

## Required tests

Focused tests should prove:

- deterministic registry ordering;
- no duplicate ids;
- dependency resolution and cycle detection;
- unknown dependency diagnostics;
- incompatible tag diagnostics;
- missing semantic scope diagnostics;
- module absence behavior is explicit and non-throwing;
- future-required contracts are not treated as ready;
- semantic expansion plan is stable for repeated runs;
- fake/leak matrix catches forbidden runtime/provider/Lua/UI/package-schema leakage;
- three profile/style scenarios produce different compatibility/expansion plans through the same planner.

## Expected result

The generator becomes more real because future goals can ask one deterministic registry: “given this profile and semantic pack selection, which artifact contracts and expansion slots are valid?” This reduces future vertical rewrites and preserves the rule that LLM drafts high-level authoring artifacts, while C# deterministically validates, expands and promotes them.
