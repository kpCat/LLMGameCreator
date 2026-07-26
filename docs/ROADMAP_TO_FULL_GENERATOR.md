# Roadmap To Full Generator

Status: proposed strategic roadmap after Goal 003.

Goal169C is the current `BLOCKED_AFTER_SINGLE_CACHED_SMOKE_PORTABLE_CORE_ONLY_CAMPAIGN_TRUTH` continuation. Goal169B independent audit remains `BLOCKED_AT_91BEF55B` with code findings closed. Base64/legacy preflight, required regressions, Player launch, exit 0, immutable publication, selected-history/actual-package/payload correlation, RC CURRENT and portable all-selectable pass. The final core-only portable campaign-truth assertion fails. Retry remains 0; Goal169C is unaccepted and requires independent blocker audit/follow-up before roadmap expansion.

## Source Of Truth

Before choosing the next milestone, read `docs/CURRENT_GENERATOR_STATE.md` and `docs/CURRENT_GENERATOR_STATE.json`.

The current state handoff, active manual gate and strategy reset override any older roadmap sequence that suggests starting a feature slice early.

Goal165 is the current completed product slice and is a GREEN acceptable candidate. It closes the Goal164 audit P1 by making combat qualification and v4 history neutral between BasicAttack-only, package-ability-only and both-route profiles, then adds truthful in-memory defeat recovery with retry, exact save continuation and new game. Goal165 remains unaccepted and requires independent audit. The next bounded planning subject is campaign choice branching; Unity presentation, media, persisted recovery and richer authored combat remain future work.

Current locked baseline: `M4.1` remains the last completed milestone until `docs/CURRENT_GENERATOR_STATE.json` explicitly records a later milestone.

## Definition Of Full Generator

The full generator is not a universal magic engine that can generate every possible game without new primitives.

The target is:

- user defines genre, tone, rules, semantic packs, asset direction, and gameplay families;
- combiner generates a coherent game package;
- most game-specific behavior comes from data/rule packs;
- C# core changes only when a new primitive family is required;
- runtime/export can play the generated game;
- Unity or another final runtime can consume the generated package;
- LLM is optional authoring assistance, not runtime dependency.

## Stage A: Proving Core Generation

Status: mostly done by Goals 001-003.

Purpose:

- prove deterministic procedural generation;
- prove formula/effect/action rules;
- prove generated package MVP;
- prove runtime-backed microgame loop;
- prove configurable seed/preset variation;
- prove extension through rule packs.

Exit criteria:

- multiple generated variants;
- runtime-owned progress/reward/completion;
- extension proof without bespoke gameplay C#;
- automated scenario harness;
- one final manual verification per goal.

## Stage B: Playable Generated Microgame

Purpose:

- make generated games understandable and playable for 5-10 minutes;
- add region travel;
- add goal HUD;
- add inventory/reward visibility;
- add interaction hints;
- add simple quest journal;
- keep Runtime Preview as proving ground only.

Likely goals:

- regional navigation and variable maps;
- player-facing HUD and journal;
- inventory/equipment visibility;
- simple dialogue interaction;
- multi-step quest acceptance.

Exit criteria:

- user can play a generated microgame without reading debug logs;
- at least 3 seed/preset variants are distinct;
- automated harness covers core loop;
- one manual check confirms playability.

## Stage C: Rule-Pack Driven Gameplay Families

Purpose:

- make mechanics extensible through data/rule packs;
- avoid C# per feature.

Gameplay families:

- inventory/equipment;
- jobs/work actions;
- crafting;
- trading/economy;
- reputation/factions;
- status effects;
- combat;
- stealth/theft;
- relationship/social interactions;
- adult content tags and gated scenes;
- weather/environment effects;
- simple building/ownership.

Exit criteria:

- each family has primitives in C#;
- concrete variants are data/rule-pack authored;
- invalid packs are rejected;
- headless scenario tests prove behavior;
- generated package can combine several families.

## Stage D: Procedural World Structure

Purpose:

- move from small generated maps to larger worlds.

Needed systems:

- regions;
- chunks;
- biome distribution;
- travel graph;
- points of interest;
- spawn tables;
- faction territories;
- simulation levels: near detailed, far abstract;
- save/load snapshots.

Exit criteria:

- world can be larger than loaded play area;
- generated regions connect;
- far-world state advances abstractly;
- no full-world per-frame simulation;
- deterministic save/load remains stable.

## Stage E: Content Generation At Scale

Purpose:

- produce varied quests, dialogue patterns, NPC archetypes, loot, regions, and events without LLM per instance.

Needed systems:

- quest grammar;
- dialogue intent grammar;
- event grammar;
- relationship/reputation-driven reactions;
- semantic-guided item/biome/location generation;
- conflict and dependency validation;
- repetition control.

Exit criteria:

- hundreds of generated instances from compact packs;
- repetition is controlled;
- semantic conflicts are caught;
- LLM authoring remains optional and offline.

## Stage F: Asset Pipeline

Purpose:

- connect generated package to visual/audio assets.

Needed systems:

- asset request queue;
- tile set requests;
- portrait requests;
- UI icon requests;
- sound effect requests;
- music import/loop metadata;
- ComfyUI/Fooocus integration;
- Suno/manual music import path;
- review/import workflow;
- deterministic asset mapping.

Exit criteria:

- generated game can request missing assets;
- user can approve/import assets;
- assets are mapped to semantic roles;
- runtime has fallback assets if generation is missing.

## Stage G: Unity Runtime Export

Purpose:

- move from proving ground to real playable runtime.

Needed systems:

- package-to-Unity mapping;
- tile/chunk renderer;
- 2D assets unfolded into 2.5D/3D presentation;
- input/controller;
- interaction UI;
- dialogue UI;
- inventory UI;
- save/load;
- streaming/chunk loading;
- performance budget.

Exit criteria:

- one generated game exports and runs in Unity;
- runtime supports at least one complete gameplay family set;
- performance remains acceptable on target hardware;
- no Runtime Preview-only dependency.

## Stage H: Advanced Runtime Primitives

Purpose:

- support richer game ambitions.

Primitive families:

- NPC perception;
- line of sight;
- projectile/ranged combat;
- cover;
- group AI;
- siege/destruction;
- building;
- economy simulation;
- political/faction simulation;
- weather/environment interaction;
- relationship/NSFW scene gating;
- procedural settlement/city systems.

Exit criteria:

- new primitive families are C# core;
- game-specific behavior is still rule-pack/data-driven;
- simulation has near/far levels;
- performance budgets are enforced.

## Stage I: Authoring UX

Purpose:

- let user control generation without editing raw files.

Needed systems:

- semantic pack editor;
- rule pack editor;
- preset editor;
- generator profile editor;
- asset request/review UI;
- validation dashboard;
- package comparison;
- authoring assistant/RAG integration.

Exit criteria:

- user can create/modify a project through UI;
- validation explains issues;
- generation remains reproducible;
- raw files still remain source-controlled.

## Stage J: Alpha Definition

Alpha is reached when:

- generated game can be configured by seed/preset/semantic/rule packs;
- generated game has a playable loop of 15-30 minutes;
- at least 3 game styles can be created from different packs;
- most content is generated deterministically;
- runtime/export is playable;
- extension proof exists for several mechanics without C# changes;
- manual checks are reduced to final acceptance per milestone;
- LLM is optional and offline.

## Stage K: Beyond Alpha

Beyond alpha:

- richer AI;
- larger worlds;
- more content families;
- better asset generation;
- Unity polish;
- modding tools;
- more advanced semantic authoring;
- balancing tools;
- scenario simulation;
- performance tuning;
- multiplayer only if explicitly chosen much later.

## Main Risk

The project fails if it becomes:

- C# slice per mechanic;
- Runtime Preview as the final engine;
- semantic dump instead of curated meaning;
- LLM runtime dependency;
- manual verification after every small step;
- endless documentation without playable gain.

The project succeeds if each stage increases either:

- playable generated output;
- extensibility without C#;
- validation automation;
- export/runtime reality;
- user control over generated games.

## Current Goal161 bridge

Goal156 completed the durable bridge from the deterministic procedural generator spine to ordinary project creation and the modern FeatureModule workspace. Its independent audit then identified missing causal source provenance and canonical overlay/base reconstruction.

Goal157 closes that provenance blocker and activates the generated start map in the final player lane while retaining a baseline-start accepted-mechanics compatibility lane. Its independent audit is GREEN at `8939aea0`.

Goal158 adds deterministic generated-region travel through those same seams: exact strict-plan bindings, one safe gate per directed connection, generic atomic Runtime map transitions, origin and destination interactions, multi-hop planning, replay, exact state roundtrip, primary route hashes/frames, `TRAVEL_CURRENT` history/UI, cached standalone truth and portable RC recovery. Lane A AcceptedMechanics/Social remains unchanged.

Goal158 independent audit is GREEN at `9a350c63`. Goal159 adds source v2 request/resolution truth and a shared creation/regeneration factory, then proves isolated candidate build/repeat/reopen, typed world diff and journaled promotion. Its independent audit is `BLOCKED_AT_C7788E1E` at the commit boundary.

Goal160 closes that P1 by placing build, standalone, authoring mutation, regeneration, history rollback and recovery behind one operation coordinator and cross-process project lock. Candidate truth is immutably sealed; Apply trusts cached attempt authority, rechecks truth/inventory inside the locked transaction, enters journal `validating`, and performs semantic validation before commit/cleanup. Failure and validating crash restore exact before hashes.

Generated-world history stores only generation source/sidecars. Historical rollback rebuilds the target generation with current mechanics/parameters/identity, repeats and reopens `TRAVEL_CURRENT`, then uses the same sealed transaction. Old histories and RC bytes are retained; ordinary standalone renews CURRENT RC.

Goal160 independent audit found a profile-neutral P1 at `d8dd05e7`: core-only world changes were incorrectly forced through complete AcceptedMechanics. Goal161 implements and tests the correction with exact sealed summary/compatibility hashes and generic RC projections. Generated gameplay saves now use immutable revisions and an atomic slot manifest, exact same-world load and explicit fingerprint-based package/world migration. Cross-world map/transients reset, compatible state is preserved, incompatible references are reported/dropped, and historical revisions become current again when their world is restored. The single permitted cached standalone attempt reused the host with zero Unity starts but failed the player self-check, so Goal161 is BLOCKED and portable post-smoke proof was not reached. The next roadmap decision is a bounded standalone diagnosis with a newly authorized smoke budget; independent audit and player-driven campaign planning wait.
## Goal167C generated campaign choice branching

Goal167C closes the published FAILED scaffold as a GREEN acceptable candidate. Exact generated dialogue provenance, deterministic controlled overlays and Runtime state/event authority now support qualified Support, Challenge and Refuse branches with persistent flags, locked alternatives, follow-ups, atomic rollback and replay. v5 choice-current readiness, v4 rebuild guidance, choice-aware regeneration seals, exact save/continue, branch-aware migration, one cached GREEN smoke, RC `CURRENT` and portable all/core recovery are proven. The next roadmap action is an independent audit before planning campaign relationships and multi-quest arcs.

## Goal168 choice-driven relationships and multi-quest arcs

Goal167 audit result `BLOCKED_AT_FD69BFC8` is closed by exact Goal166 qualified-action catalog reuse; successful utility/no-op actions are not combat progress. Exact generated dialogue IDs define relationships, generated quests are assigned once and ordered from data, and Support advances sequentially through dialogue, exact combat and manual turn-in. Challenge/Refuse remain exclusive. v6 readiness, sealed regeneration/rollback, exact save continuation, explicit preserve/reset/drop migration, one cached smoke, RC and portable all/core qualification are GREEN. The next roadmap action is independent Goal168 audit before world events or deeper relationship consequences.

## Goal169 profile-neutral relationships and reactive regional events

Goal168 independent audit `BLOCKED_AT_BBFD46A2` is closed by branch-availability truth, exact health/stat/status combat progress and honest build-time save facts. Six relationship profiles qualify without executing absent branches. Support completion, Challenge victory aftermath and Refusal fallout derive deterministic regional events with exact dialogue/flag identity, safe reachable placement, controlled overlay and ordinary Runtime resolution.

The visible product slice adds state-backed «События мира» rows and human map markers. History v7, genuine-v6 pending upgrade, sealed regeneration/rollback, AVAILABLE/RESOLVED continuation, exact-compatible migration, CURRENT RC and portable all/core qualification are GREEN. The single cached standalone launch passed, but Goal169 payload qualification rejected direction-only movement frame titles. The post-smoke fix emits `Move.<Direction>` and was verified only by non-smoke contracts because corrective retry is zero. The next roadmap action is `independent_goal169_blocker_audit_and_followup_without_retrying_consumed_smoke`.

## Goal169A strict replay v7 correlation and post-fix smoke closure

This continuation does not add a product slice. It makes the Goal169 event milestone auditable: independent locked/resolution replay pairs, exact v7 branch-event graph, provenance-derived Challenge region, semantic inventory and typed migration facts. The original blocked smoke stays immutable; one new smoke proves the fixed payload with explicit `Move.*` and zero bare direction frames. The next roadmap action is `independent_goal169a_audit`.

## Goal169B package-bound event proof, nested combat replay and payload closure

This focused continuation implements closure of the Goal169A audit result `BLOCKED_AT_D012B8AC`. Actual nested combat commands/events/action/effect/state chains replace synthetic final frames; exact ID sets and actual-package event definitions bind history and save truth; absent and migration behavior are sealed; and standalone payloads expose event/route/replay/sequence identity plus 24 signatures. Retained Goal169/Goal169A artifacts remain byte-identical. The only cached smoke failed before launch/publication at `standalone.payload.human_facts_parse_mismatch`; the diagnosed Base64/title correction is focused-test GREEN but cannot be re-smoked under retry 0. The next roadmap action is `independent_goal169b_blocker_audit_and_followup_without_retrying_consumed_smoke`.

## Goal169C post-fix immutable standalone RC and portable closure

This focused continuation qualifies the published Goal169B Base64 correction without adding a product slice. The only cached smoke reuses the host, launches the Player, exits 0 and publishes a distinct immutable pointer/run. Selected v7 history, actual payload package and Base64 authority correlate exactly; 24 signatures, 392 frames and 124 nested-combat frames are proven, and RC plus portable all-selectable are current. The final core-only portable campaign-truth assertion fails, so no correction or retry is made. The next roadmap action is `independent_goal169c_blocker_audit_and_followup_without_retrying_consumed_smoke`.
