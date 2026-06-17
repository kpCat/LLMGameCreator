# 004_PACK_GENERATION_POLICY.md — policy for generating future task packs

This file tells ChatGPT how to generate future packs. It is for the pack author, not for the local coding agent.

## Source of truth

Generate packs from repository state, not from chat memory.

Required first read set:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/PHASE_PLAN_INDEX.md
.devflow/RECURSIVE_TASK_SELECTION_PROTOCOL.md
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
```

Then read only the phase docs/source files required for the next pack.

## Pack types

### Quality/devflow pack

Use when local-agent execution exposes repeatable weakness.

Examples:

```text
- weak tests;
- formatting churn;
- generated artifacts in diff;
- poor final report;
- context over-read;
- boundary confusion.
```

Quality packs may update:

```text
.devflow/*.md
docs/agent-tasks/_*.md
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
```

They should not update source code unless explicitly requested.

### Executable task-spec pack

Use only when the current gate allows actual implementation work.

Executable specs must include:

```text
- Task ID;
- status/gate/approval;
- dependencies;
- source docs;
- allowed files;
- forbidden files;
- exact behavior;
- diagnostics/failure behavior;
- proof tests with exact assertions;
- system gates;
- stop conditions;
- next task pointer.
```

No executable spec may be broad, vague, or missing proof tests.

### Locked future planning pack

Use for future phases whose gate is not yet open.

Locked specs may define:

```text
- sequence;
- intent;
- boundaries;
- expected contracts;
- non-goals;
- unlock conditions;
- future proof-test categories.
```

Locked specs must not be executable until current-state docs unlock them.

## Generation order

Default sequence after Pack 005:

```text
Pack 006 — full sequence skeletons for M5/M6/M8/M9/M10
Pack 007 — M5 executable entry specs, only after M4.1 passes
Pack 008 — M5 first vertical Lua slice
Pack 009 — M6 artifact-to-package mapping contracts
Pack 010 — M6 first rich package vertical slice
Pack 011 — M8 runtime preview validation loop
Pack 012 — M9 templates/balancing
Pack 013 — M10 export profiles / Unity IR
```

Insert repair/hardening packs whenever execution feedback requires it.

## Gate policy

M5/M6/M8 production work is locked until:

```text
- docs/CURRENT_GENERATOR_STATE.md says M4.1 passed;
- docs/CURRENT_GENERATOR_STATE.json says M4.1 passed;
- the pack author has reviewed current source layout for stale task assumptions.
```

Task-pack files alone cannot unlock phases.

## Independence policy

Do not build a long chain where every future pack assumes all previous speculative packs were perfect.

Each pack should be one of:

```text
- immediately executable and small;
- locked and safe to replace;
- repair/hardening and local;
- roadmap-only.
```

A future skeleton does not force implementation. Before turning skeleton into executable specs, refresh it from current repo state.

## Avoid stale specs

Do not generate detailed executable specs for a phase more than one gate ahead.

Bad:

```text
Write detailed M10 export implementation tasks while M4.1 is still unresolved.
```

Good:

```text
Write M10 sequence skeleton and boundary constraints now; generate executable M10 tasks only when M9 state is known.
```

## Response format for generated archive

Every pack archive should include:

```text
ARCHIVE_MANIFEST.md
README_APPLY_AGENT_TASK_PACK_XXX.md
new/updated docs
```

The final ChatGPT response should include:

```text
- archive link;
- pack id;
- file list summary;
- apply commands;
- explicit statement about locked/unlocked phases;
- suggested next action.
```
