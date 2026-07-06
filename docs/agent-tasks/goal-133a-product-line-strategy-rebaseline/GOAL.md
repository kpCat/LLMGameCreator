# Goal 133A — Product-Line Strategy Rebaseline And Canonical Runtime Pivot

Status: proposed docs/process goal.

## Purpose

Rebaseline the project strategy so future work preserves the broad LLMGameCreator
dream scope without continuing proof-only/projection-only drift.

This goal should supersede a pure selected-candidate review-package goal if that
goal would only add another projection/evidence wrapper. The selected candidate
review lane remains useful, but the next product direction must explicitly pivot
toward canonical runtime execution.

Goal133A canonical-runtime-pivot routing supersedes this earlier selected-candidate
review-package framing. Do not treat this older task as the immediate next
product goal unless it is explicitly tied to the canonical runtime path:
candidate package -> package validation -> canonical runtime playthrough ->
save/load/replay proof -> Unity/player consumes canonical transcript/state
summary -> one-click report. A selected-candidate review package may return
later only after the canonical runtime pivot is established.

## Read First

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `README.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`

## Required Changes

### 1. README

Update `README.md` to describe LLMGameCreator as a configurable data-driven game
product-line combiner.

The README must clearly state:

- not prompt-to-game;
- not LLM-written runtime;
- LLM is optional local authoring assistance;
- `GamePackage` remains source of truth;
- narrow alpha must prove an expansion-safe kernel;
- future broad scope is reached through feature modules, semantic packs, visual
  packs, world-source adapters and player adapters;
- current strategic pressure is to reduce `projectionOnly=true`.

### 2. New Strategy Docs

Add:

```text
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md
```

The docs must be concise enough to be read during task routing and concrete
enough to influence future goal shaping.

### 3. Routing Updates

Update `AGENTS.md` so generator/Codex orientation includes the new docs after
current state and before task-specific docs.

Update `docs/CONTEXT_INDEX.md` so the Generator Task Routing and source-of-truth
table include the new docs.

Update `docs/CURRENT_GENERATOR_STATE.md` and `docs/CURRENT_GENERATOR_STATE.json`
to record:

```text
current strategic pivot: product-line kernel, not proof-only expansion
next recommended product milestone: canonical runtime selected-candidate playthrough
projectionOnly risk remains open after Goal132
manual checks should stay rare; automated tiers should be strengthened
```

Update `docs/FULL_GENERATOR_GOAL_QUEUE.md` with a short section referencing the
new policy docs and the next product milestone.

### 4. Next Goal Recommendation

Write a concise next-goal recommendation for the handoff:

```text
Goal134 — Canonical Runtime Selected Candidate Playthrough Matrix
```

The recommendation must require:

- consume Goal131/132 selected candidate;
- read physical candidate package from disk;
- validate package;
- execute canonical runtime playthrough without Unity as source of truth;
- write command transcript and state-hash chain;
- prove save/load/replay;
- expose result in WinForms/VisualWorld workspace;
- keep Unity inspection optional;
- avoid public schema/Runtime contract changes unless explicitly required and
  justified.

## Forbidden Scope

Do not change product code except tiny routing constants if unavoidable.

Do not change:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
unity/**
samples/minimal-map-game/package.json
.llmgc/manual/**
provider/network/media generation
Lua execution
public GamePackage schema
```

Do not create real LLM/provider integration.

Do not mark human acceptance as passed.

## Evidence

Write compact evidence under:

```text
.llmgc/procedural/goal-133a-product-line-strategy-rebaseline/
```

Minimum evidence:

```text
strategy-rebaseline-report.md
strategy-rebaseline-summary.json
changed-docs-index.json
forbidden-scope-report.json
```

Export a short report under:

```text
.llmgc/exports/goal-133a-product-line-strategy-rebaseline/
```

## Acceptance

Final gate:

```text
product_line_strategy_rebaseline_verification required
implementationStatus=GREEN|BLOCKED|FAILED
accepted=false
```

`GREEN` requires:

- README updated;
- three new docs added;
- AGENTS routing updated;
- CONTEXT_INDEX routing updated;
- CURRENT_GENERATOR_STATE.md/json updated;
- FULL_GENERATOR_GOAL_QUEUE updated;
- next recommended goal points to canonical runtime selected-candidate
  playthrough, not another projection-only wrapper;
- forbidden-scope report proves no forbidden lanes changed.
