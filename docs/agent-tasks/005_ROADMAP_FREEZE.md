# 005_ROADMAP_FREEZE.md — roadmap documentation freeze

This document freezes the documentation-only roadmap after the locked M10 draft specs.

## Freeze status

```text
Roadmap docs complete through M10 locked draft planning.
```

The repository now has:

```text
- M4.1 executable/gate-closure specs;
- M5 locked draft specs;
- M6 locked draft specs;
- M8 locked draft specs;
- M9 locked draft specs;
- M10 locked draft specs;
- pack generation policy;
- branch execution support.
```

## What this freeze means

Do not continue generating speculative future implementation specs while M4.1 is active.

Future locked drafts are allowed to be replaced or amended later, but only when their gate is close enough and current source layout is known.

## What remains active

Current active execution area:

```text
M4.1 real-model evaluation gate.
```

Expected next practical work:

```text
M4_1_005 -> M4_1_006 -> M4_1_008
```

Alternative real-evidence closure path:

```text
M4_1_013 -> M4_1_014 -> M4_1_015 -> M4_1_016 -> M4_1_017
```

## Allowed next pack types

After this freeze, future packs should be one of:

```text
- repair/hardening pack based on real Kilo/local-agent execution;
- M4.1 evidence/import/closure support pack;
- source-refreshed M5 executable pack after M4.1 is explicitly passed;
- small doc correction pack for a concrete inconsistency.
```

## Forbidden next pack types while M4.1 is active

```text
- more M5/M6/M8/M9/M10 implementation detail without execution evidence;
- executable M5/M6/M8/M9/M10 specs;
- broad schema/runtime/export contracts not needed for M4.1 closure;
- “just in case” future task packs.
```

## Why this freeze exists

The purpose is to avoid stale task specs. The current repository policy is:

```text
current gate: detailed and executable
next phase: detailed but locked unless gate is open
far phases: sequence skeletons only
```

Now the far-phase documentation is deep enough. Further progress should come from real execution evidence.
