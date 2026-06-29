# Goal 032 Spec: Dynamic Semantic Feature System And Influence Rule Kernel

## Summary

Goal 032 adds a deterministic semantic feature system that makes LLMGameCreator capable of representing user/lore-defined feature dimensions without hardcoding every NPC/species/faction/quest field.

This is the next layer after:

- Goal 030 semantic artifact contract registry;
- Goal 031 semantic pack composition blueprint.

Goal 032 must prove that semantic variability can be driven by an internal feature/influence resolver rather than by asking an LLM to decide every combination.

## Core principle

```text
LLM helps with lore/seed authoring.
The program resolves combinatorics.
```

The system must support manual, semi-manual and future full-auto authoring:

- user manually defines features/factors/rules;
- program validates and suggests missing pieces;
- future LLM may propose draft seed features, but does not execute the resolver;
- final semantic state is deterministic and traceable.

## Required scope

The goal should implement an Application-layer, BCL-only kernel for:

1. Semantic feature definitions.
2. Feature assignments across dynamic scopes.
3. Applicability rules.
4. Inheritance rules.
5. Typed influence rules.
6. Deterministic feature/state resolver.
7. Dynamic authoring schema hints for future UI.
8. Diagnostics and evidence artifacts.

No public `GamePackage` schema, Runtime, Unity, UI, provider/LLM/RAG, Lua, generator-library, `.sln` or `.csproj` changes.

## Feature scopes

The system should support at least these semantic target scopes:

```text
world
kingdom
region
biome
settlement
faction
species
archetype
npc
item
resource
quest
dialogue
event
magic
combat
relationship
```

The model must allow scopes to be missing. Example: an NPC can legally have no faction or no mood if definitions say those features are optional or inapplicable.

## Feature definition model

Each feature definition should represent, at minimum:

- `id`;
- `displayName`;
- `scope`;
- `valueKind`;
- `cardinality`;
- `requiredMode`;
- `defaultStrategy`;
- `inheritanceMode`;
- `applicability conditions`;
- `allowed values` or bounds when relevant;
- `tags`;
- `conflicts`;
- `requires`;
- `provenance`;
- `authoring group`;
- `notes`.

Suggested value kinds:

```text
flag
number
enum
weighted_tag
relation
text_key
list
```

Do not use string expression evaluation for this goal.

## Feature assignment model

Feature assignments should include:

- `targetId`;
- `targetScope`;
- `featureId`;
- `value`;
- `sourceLayer`;
- `sourceId`;
- `overrideMode`;
- `weight`;
- `priority`;
- `provenance`;
- `status`.

Source layers should allow at least:

```text
world
kingdom
region
biome
settlement
faction
species
archetype
instance
manual_override
generated_default
```

## Inheritance

Goal 032 must support deterministic inheritance from broader scope to narrower scope where definitions allow it.

Example chains:

```text
world -> kingdom -> region -> biome -> settlement
world -> kingdom -> faction -> npc
world -> species -> archetype -> npc
world -> kingdom -> species -> archetype -> npc
```

Inheritance must be data-driven, not hardcoded to NPC-only behavior.

The resolver must preserve trace:

- inherited from where;
- overridden by what;
- defaulted by which strategy;
- absent because optional/inapplicable;
- blocked because illegal.

## Influence rules

Influence rules must be typed data records, not arbitrary C# expression strings.

Each rule should include:

- stable id;
- target scope/family;
- condition clauses;
- effect records;
- weight;
- priority;
- deterministic tie-breaker;
- status;
- provenance;
- explanation.

Minimum condition operators:

```text
feature_exists
feature_missing
enum_equals
number_at_least
number_at_most
tag_contains
relation_exists
scope_is
target_has_tag
```

Minimum effect kinds:

```text
set_feature
adjust_number
add_weighted_tag
add_relation
add_intent
block_feature
raise_diagnostic
suggest_feature
```

Effects must produce trace records. If a rule tries to affect an unknown feature or illegal scope, validation must catch it.

## Resolver

The resolver should accept:

- feature definitions;
- feature assignments;
- influence rules;
- target hierarchy/context;
- selected profile/style id;
- deterministic seed.

It should return a resolved semantic state:

- target id/scope;
- resolved feature values;
- inherited/defaulted/manual/generated flags;
- influence outputs;
- authoring suggestions;
- diagnostics;
- deterministic summary/hash if repository style has hash helpers;
- stable ordering.

The same input must produce structurally equivalent output on repeated runs.

## Dynamic authoring schema

Goal 032 must not touch WinForms/UI.

However, it must produce UI-ready authoring schema records, such as:

- feature group;
- field kind;
- label/key;
- value options;
- required/optional state;
- visible/applicable state;
- inherited value;
- can override;
- suggested default;
- diagnostics for this field;
- safe editor hints.

This is the future bridge to dynamic tabs/UserControls.

## Required evidence scenarios

Create deterministic evidence for at least four scenarios:

1. `frontier_survival`
   - grounded survival profile;
   - faction optional;
   - mood/state can be derived from hunger/trust/threat.

2. `gothic_intrigue`
   - faction/reputation/social pressure matters;
   - forbidden magic or court pressure influences dialogue/quest intent.

3. `caravan_trade`
   - economy/trade relation and route pressure matter;
   - mood/faction may be less central than trust, stock pressure and contract risk.

4. `metamodule_kingdoms`
   - a high-complexity fantasy proof scenario;
   - world with multiple kingdoms and a species/archetype concept such as `metamodule_bearer`;
   - proves dynamic features can be attached to species/archetypes without forcing every NPC into the same fields.

## Required artifacts

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-032-dynamic-semantic-feature-system/
```

Required files:

```text
feature-catalog-summary.json
influence-rule-summary.json
dynamic-authoring-schema-matrix.json
resolved-feature-state-frontier.json
resolved-feature-state-gothic.json
resolved-feature-state-caravan.json
resolved-feature-state-metamodule-kingdoms.json
invalid-feature-diagnostics-matrix.json
dynamic-semantic-feature-system-report.md
```

Report must include:

```text
dynamic_semantic_feature_system_verification required
```

## Diagnostics

Stable diagnostics must cover:

- duplicate feature id;
- invalid id;
- unknown feature reference;
- unknown target scope;
- invalid value kind/value shape;
- illegal assignment for scope;
- required feature missing;
- optional feature missing without failure;
- conflict;
- unknown inheritance source;
- circular inheritance;
- unknown influence rule target;
- circular influence;
- overconstrained output;
- nondeterministic order detected where practical;
- forbidden leakage: Runtime/UI/Unity/provider/LLM/RAG/Lua/GamePackage schema.

## Non-goals

Do not:

- generate final dialogue text;
- write dialogue lines through LLM;
- add Runtime or GamePackage behavior;
- build UI;
- add external libraries;
- execute Lua;
- call providers;
- mutate generator-library;
- weaken existing evidence tests.

## Manual gate

Goal 032 must stop at:

```text
dynamic_semantic_feature_system_verification required
```

Do not mark this passed in the same goal.
