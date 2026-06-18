# LLMGameCreator 1.0 Roadmap

## 1.0 definition

LLMGameCreator 1.0 is a usable local-first authoring tool where a user can go from a vague game idea to a validated playable/exportable game package.

1.0 is not “every possible game genre fully solved”. 1.0 is a complete product loop:

```text
idea
-> design assistant
-> capability composition
-> strict LLM artifacts
-> artifact review
-> apply to package
-> validate/simulate
-> runtime preview
-> export
```

## Already completed

### M4.1 real-model evaluation gate

Status: passed for sampled baseline contracts.

Evidence:

```text
game_profile_v1
mechanics_pack_v1
quest_pack_v1
scene_pack_v1
4/4 initial pass
0 repair needed
0 diagnostics
0 quality warnings
```

## Big tasks remaining to 1.0

The remaining route should use **large vertical slices**, not dozens of tiny tasks.

### 1. Capability Composer v2 Foundation

Goal: make game design selection understandable and extensible.

Includes:

- Russian help and readable names;
- option/bundle details panel;
- compatibility diagnostic categories;
- non-breaking selected modules/modifiers/constraints model;
- prompt context includes composable capability data.

Output: a user can understand what they are choosing and why combinations fail/warn.

### 2. Design Assistant / Brainstorming Mode

Goal: let a user start from a vague idea instead of manually knowing all fields.

Includes:

- freeform design conversation;
- AI asks clarifying questions;
- AI suggests genre/system options;
- AI converts confirmed design into capability composition;
- no strict artifact generation until user confirms.

Output: the tool helps design a game, not just fill dropdowns.

### 3. Artifact Review v2

Goal: make artifact review usable for real work.

Includes:

- show artifact grouped by contract;
- approve/reject/needs-edit;
- show diagnostics and source prompt/audit;
- allow user notes;
- track accepted artifact versions.

Output: artifacts become actionable editor objects.

### 4. Baseline Artifacts -> GamePackage Assembly

Goal: map accepted baseline artifacts into a GamePackage draft.

Includes:

- game profile -> package metadata/core loop;
- scene pack -> scenes/locations;
- quest pack -> quest graph;
- mechanics pack -> rule/mechanic definitions;
- validation and save/export.

Output: generated artifacts become real package state.

### 5. Runtime Preview: Text RPG / Region Graph Slice

Goal: preview a generated text RPG without full Unity/runtime complexity.

Includes:

- start scene;
- choices;
- region/scene transitions;
- quest step progress;
- simple mechanics invocation;
- debug state view.

Output: user can “play” a minimal generated package.

### 6. Progression and Combat Composition

Goal: support combinations like:

```text
level-based + perk tree + stat allocation + skill XP
realtime + turn-based toggle
dialogue combat + direct combat
party combat + individual actors
```

Output: progression/combat are composable systems, not single dropdown locks.

### 7. World Generation Slice

Goal: support procedural region graph and early large-world generation.

Includes regions, biomes, weather, time of day, procedural events, encounter/resource distribution.

### 8. Economy and Trading Slice

Goal: support markets, prices, trade routes and scarcity.

Includes currency, base prices, supply/demand, regional modifiers, faction/reputation modifiers, route danger/taxes/events.

### 9. Balance and Simulation Slice

Goal: prevent unplayable generated games.

Includes power budgets, encounter tiers, progression curves, economy sanity checks, reachability, dead-end detection, “too easy / impossible” reports.

### 10. Lua-backed Generator Module Integration

Goal: use the Lua generator-library safely.

Includes manifest loading, module capability filtering, no arbitrary runtime mutation, validated artifact envelopes, sandboxed execution strategy if/when allowed.

### 11. Asset Pipeline Slice

Goal: connect generated content to visual/audio placeholders.

Includes asset requests, portrait/tile/SFX slots, external generator request bundles, import/attach generated assets.

### 12. Export Pipeline

Goal: export packages for preview/runtime.

Includes package export, manifest, validation report, runtime target metadata, sample package output.

### 13. 1.0 Hardening

Goal: make it reliable.

Includes migrations, crash recovery, diagnostics, sample games, user docs, packaging, regression test suite.

## Estimated task count

Target: 13 large product slices.

Reality: some slices may require 1 repair task after Codex implementation. That is acceptable. Avoid turning each slice into 10 microtasks unless a slice clearly fails or becomes too broad.

## Next task

Start with:

```text
Product Slice 001: Capability Composer v2 Foundation
```
