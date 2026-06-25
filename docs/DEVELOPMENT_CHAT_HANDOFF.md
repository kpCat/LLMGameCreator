# LLMGameCreator Development Chat Handoff

Status date: 2026-06-26.

Purpose: preserve product direction, architecture decisions and collaboration rules when continuing development in a new ChatGPT conversation. This file is a durable orientation document, not the live milestone state.

## Live Sources Of Truth

Always inspect the pushed repository before making a task or accepting a Codex report:

- Repository: `https://github.com/kpCat/LLMGameCreator`
- User local checkout: `C:\Users\endim\LLMGameCreator`
- Live machine-readable state: `docs/CURRENT_GENERATOR_STATE.json`
- Routing index: `docs/CONTEXT_INDEX.md`
- Context budget policy: `docs/AGENT_CONTEXT_BUDGET_POLICY.md`
- Architecture boundaries: `docs/ARCHITECTURE_STRATEGY_AND_BOUNDARIES.md`
- Semantic strategy: `docs/SEMANTIC_PACK_AND_RAG_STRATEGY.md`
- Full roadmap: `docs/ROADMAP_TO_FULL_GENERATOR.md`

`CURRENT_GENERATOR_STATE.json` overrides this handoff for completed slices, active gates and the next action. Do not assume this handoff's current-goal note is still current.

## Product Target

LLMGameCreator is not intended to clone Heroes 3, Might and Magic 7, RimWorld, Factorio, Mount and Blade, Workers & Resources, Kenshi, Skyrim or other games.

The user wants to create original games using reusable classes of mechanics inspired by many games. Desired long-term capabilities include procedural worlds, regions/chunks, quests, dialogue, NPCs, factions, reputation, inventories, equipment, crafting, economy, combat, jobs, theft, relationships, adult-content gating, status effects, skills/perks/stats, magic, ranged weapons, perception, weather, building, sieges/destruction, music, sound and tile-based 2D assets presented in 2.5D/3D.

Not every capability must exist in Alpha. The architecture must let new game-specific variants come from packs/scripts, while only genuinely new engine primitive families require C# work.

## Non-Negotiable Architecture

The durable stack is:

```text
C# stable primitives/runtime/state/validation/serialization
+ data and rule packs
+ restricted declarative Lua API later
+ deterministic procedural generators
+ optional offline LLM authoring assistance
+ final Unity runtime/export
```

Rules:

- C# owns safe reusable primitives, not every quest/item/mechanic variant.
- Data/rule packs own triggers, conditions, formulas, actions, rewards, objective patterns, dialogue intents, loot/spawn tables and game-specific combinations.
- Lua must eventually declare rules through a restricted API. It must not gain arbitrary filesystem, network, UI, thread or C# object access.
- LLM/RAG may help author packs offline. Runtime gameplay must not depend on an LLM or RAG service.
- Semantic packs are layered: core, genre, project, imported candidates and LLM candidates.
- Candidates are quarantined until explicitly accepted.
- Runtime Preview is a proving ground, not the final engine or a separate game project.
- Unity is the final presentation/runtime host for Alpha.

## Alpha Contract

Goal 013 is intended to be the minimum useful Alpha, not a report-only prototype.

By the end of Goal 013 the required user-level path is:

```text
configure seed/preset/semantic/rule packs
-> generate
-> validate
-> package assets and game data
-> produce a runnable Unity Windows game
-> launch and play for approximately 15-30 minutes
```

The final deliverable must include a runnable Windows executable/folder, not merely a Unity project, archive, JSON report or Runtime Preview demonstration.

Preferred export architecture:

```text
stable generic Unity runtime/player
+ generated GamePackage
+ generated/imported assets
= distributable game folder with EXE
```

Data-only game changes should not require rebuilding engine C#. The Unity runtime/template is rebuilt when a new primitive family requires it; ordinary game generation should package data/assets into the stable runtime where technically possible.

If Goal 013 does not produce a runnable generated Unity game, Goal 013 is not complete.

## Working Goal Sequence

This sequence is a planning contract. Exact task files are created only when the previous goal has been reviewed.

- Goal 006: semantic-selected rule declarations materialized into generated package and executed headlessly.
- Goal 007: connected regions, variable maps, travel, basic chunk/world structure and deterministic save state.
- Goal 008: first rule-pack gameplay family set, expected to cover inventory/equipment/items/crafting/trading/status foundations.
- Goal 009: second family set, expected to cover baseline combat/factions/reputation/social/work/theft combinations.
- Goal 010: content generation at scale, including quest/dialogue/event grammars, NPC/loot variation and repetition control.
- Goal 011: minimum asset pipeline for tiles, portraits, UI graphics, sound and music with deterministic mapping, imports and fallbacks.
- Goal 012: Unity runtime/export vertical slice outside Runtime Preview.
- Goal 013: Alpha integration, minimal authoring flow, three game styles, 15-30 minute loop and runnable Windows build.

This is a bounded route to Alpha. Advanced AI, ballistics, destruction, siege, deep city/economy simulation, infinite-world maturity, complete ComfyUI/RAG automation and broad polish are beyond Alpha unless a later decision explicitly moves a narrow part into the Alpha contract.

## Current Development Context

At the time this handoff was written:

- Goal 005 plus S058A correctness hotfix was externally reviewed and accepted.
- Layered semantic packs, candidate quarantine, deterministic compilation and semantic-guided generator-level selection exist.
- Goal 005 honestly records that semantic-selected ids were not yet the variants executed by runtime; Goal 004 runtime evidence was an independent regression.
- Goal 006 task files were supplied and Codex was executing S059-S063 to close that semantic-to-package-to-runtime chain.

Before acting in a new chat, fetch the live repository state because Goal 006 may already be complete or may have stopped on a blocker.

## Codex Goal Process

The user manually adds supplied task archives to the repository and pushes changes. The assistant does not push to the user's repository.

For a new multi-slice goal:

- provide a ZIP with the exact repository folder structure;
- normally include a detailed `docs/GOAL_...md` and a wrapper under `docs/agent-tasks/NEXT_PRODUCT_SLICE/...CODEX_GOAL.md`;
- provide the exact user prompt beginning with `/goal`;
- use approximately 5-8 bounded slices when coherent;
- stop at one final acceptance gate;
- do not create the next goal automatically.

For a narrow correctness hotfix:

- provide a bounded `...CODEX_TASK.md` archive;
- do not use `/goal` unless it is genuinely a multi-slice goal;
- keep the existing final gate until the hotfix is reviewed.

Each goal should prefer:

- focused tests per slice;
- deterministic sidecars;
- a product-smoke route;
- one final `check-all.ps1` run;
- one acceptance gate at the end.

Do not run the full suite after every slice without a risk-based reason.

## Manual Verification Policy

The user will not manually inspect Markdown/JSON report files.

The assistant must:

- inspect pushed code instead of trusting the Codex summary alone;
- inspect generated artifacts supplied by the user when artifact evidence matters;
- identify false-positive acceptance and missing tests;
- accept or reject the gate explicitly.

Ask the user to launch the real application only when headless automation cannot prove the relevant behavior. If a real run is required, provide exact steps:

- executable/project to launch;
- page/button to use;
- exact input values;
- expected visible result;
- whether LM Studio/local LLM must be running;
- exact files/logs/screenshots to return.

Default: no intermediate manual gates inside a goal. Exceptions are crashes, threading failures, unsafe behavior, schema/runtime blockers or a broad redesign decision.

## Review Standard

When Codex reports completion:

1. Read current state and changed implementation from the pushed repository.
2. Check acceptance calculations for false positives, not only green tests.
3. Check that invalid scenarios fail because of real diagnostics.
4. Check determinism, path safety, candidate quarantine and provenance where relevant.
5. Check that reports do not claim runtime execution for report-only projections.
6. Check that new variants remain data/rule-pack driven rather than hardcoded by genre/project ids.
7. Check focused regression quality and the product-smoke route.
8. Only then mark the gate passed or create a bounded hotfix task.

Do not ask the user to review source or report files manually.

## Context Efficiency

Codex/agents should start with:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. the selected goal/task file

Read `CURRENT_GENERATOR_STATE.md` only for state-changing/ambiguous tasks. Read broad strategy docs only when directly needed. Do not read historical task packs or old apply READMEs by default.

The assistant should not provide lists of old task files for the user to delete before every goal. Repository cleanup is a separate explicit task.

## New Chat First Action

When this handoff is used in a new conversation:

1. Inspect the live pushed repository.
2. Read the compact state and current goal/task.
3. Ask for no repeated background explanation already covered here.
4. If Codex has completed Goal 006, review the actual implementation and tests before accepting it.
5. Continue from the live gate; do not restart planning from Goal 001 and do not invent a different architecture without identifying a concrete defect.
