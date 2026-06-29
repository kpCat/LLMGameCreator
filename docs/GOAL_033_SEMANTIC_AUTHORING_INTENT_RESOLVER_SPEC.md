# Goal 033 spec — Semantic Authoring Workspace And Feature-Driven Intent Resolver

## Goal id

`goal-033-semantic-authoring-intent-resolver-v1`

## Gate marker

`semantic_authoring_intent_resolver_verification required`

## Purpose

Goal 030 established semantic artifact contracts and compatibility planning. Goal 031 composed semantic packs into cross-artifact blueprint plans. Goal 032 added dynamic semantic features, applicability, inheritance, influence rules, resolver traces and UI-ready authoring schema records.

Goal 033 must connect those layers into a practical authoring workflow and first downstream content-intent resolver:

1. A user or LLM-assisted author can define high-level lore, world parts, kingdoms, species, archetypes and manual settings.
2. The program derives a deterministic authoring workspace schema from semantic packs and dynamic features.
3. The program separates user-owned, programmatic, inherited, imported and LLM-candidate values.
4. The program resolves feature-driven content intents for NPCs, factions, quests, dialogue acts, events, economy pressure, combat pressure and settlement needs.
5. The program does not generate final prose, final GamePackage content, runtime state, Unity output or media.

## Non-negotiable architecture

The LLM must not own semantic combinatorics.

LLM may help with:

- lore intake;
- lore critique;
- high-level seed-pack drafting;
- optional quarantined candidates.

The deterministic C# Application layer owns:

- semantic features;
- applicability;
- inheritance;
- influence rules;
- authoring schema;
- missing/conflict diagnostics;
- intent planning;
- scenario variation;
- evidence.

## Expected output model families

Exact C# names may follow repository style, but the implementation should include these meanings.

### Authoring workspace records

- workspace id;
- scenario/profile id;
- domain groups: world, kingdom, region, species, archetype, faction, NPC, quest, dialogue, economy, combat, settlement, event;
- dynamic sections;
- fields with feature references, value kind, optional/required status, default strategy, inherited/default/programmatic/user value hints;
- editor control hints, without touching WinForms/UI;
- provenance: `user`, `programmatic`, `inherited`, `semantic_pack`, `llm_candidate`, `imported_candidate`, `unset`, `blocked`;
- completion status: complete, partial, missing required, optional absent, conflict, overconstrained, review needed;
- validation diagnostics.

### Lore intake skeleton records

- lore brief id;
- scenario/profile id;
- world themes;
- kingdom count/ids;
- region families;
- species/archetype families;
- conflict axes;
- magic/system axes;
- capability gaps;
- values that may be filled manually;
- values that the program can infer;
- values that may be proposed by LLM but must stay quarantined until accepted.

### Feature-driven intent records

No final prose. No final dialogue lines.

Intent records should include:

- stable intent id;
- source scenario id;
- target entity/domain id;
- source feature ids and resolved feature values;
- intent family: NPC role, relationship pressure, faction reaction, quest motive, dialogue act, event, economy pressure, combat pressure, settlement need, lore gap;
- deterministic priority/weight;
- template hint id or localization key hint when relevant;
- dependencies and blockers;
- provenance summary;
- trace summary.

### Metamodule scenario scale proof

`metamodule_kingdoms` must be represented as a complex fantasy-world authoring scenario. It should not need 100 giant hand-written objects, but it must prove the architecture can represent and validate high-complexity worlds.

Minimum expectation:

- 6 or 7 kingdom records;
- at least 100 species/archetype slots or compact generated slot records;
- feature families for mana resonance, module capacity, forbidden affinity, kingdom pressure, faction relation, relationship tension, quest motive, dialogue intent, event intent, combat threat and settlement/economy pressure;
- deterministic compact evidence showing slot counts, coverage and sample resolved intents.

## Evidence path

`.llmgc/procedural/goal-033-semantic-authoring-intent-resolver/`

Required compact files:

- `authoring-workspace-schema-summary.json`
- `lore-intake-skeleton-metamodule-kingdoms.json`
- `manual-vs-auto-authoring-matrix.json`
- `intent-resolution-frontier.json`
- `intent-resolution-gothic.json`
- `intent-resolution-caravan.json`
- `intent-resolution-metamodule-kingdoms.json`
- `invalid-authoring-intent-diagnostics-matrix.json`
- `semantic-authoring-intent-resolver-report.md`

Evidence must be deterministic, compact, sorted, timestamp-free unless the repository has a deterministic convention, and path-free except repo-relative evidence references.

## Required scenarios

- `frontier_survival`
- `gothic_intrigue`
- `caravan_trade`
- `metamodule_kingdoms`

## Manual gate

Do not mark accepted/passed inside this goal. Final report and evidence must contain:

`semantic_authoring_intent_resolver_verification required`
