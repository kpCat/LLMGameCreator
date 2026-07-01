# External Scouting — Goal 070 Integrated Campaign Timeline Simulation Matrix

## Purpose

Goal 070 is intended to prove that the already-generated family/seed rows are not isolated proof islands. It should integrate Goals 060-069 into one deterministic multi-step campaign timeline simulation matrix.

This scouting file is advisory. The task implementation must remain BCL-only unless a dependency is explicitly approved in a future goal.

## Checked options

### SimSharp / Sim#

- Repository/package: `heal-research/SimSharp`, NuGet `SimSharp`.
- Category: discrete-event simulation.
- License signal: MIT in source headers / repository license.
- Useful later for: process/resource style simulation, scheduled events, queue/resource pressure, economic production timing.
- Decision for Goal 070: do **not** add. The current goal needs a domain-specific, compact, deterministic timeline simulator with JSON evidence and causal diagnostics, not a general discrete-event framework.

### Stateless

- Repository/package: `dotnet-state-machine/stateless`.
- Category: lightweight state machines/workflows.
- Useful later for: UI workflow, authoring state machine, quest/dialogue state transition tools.
- Decision for Goal 070: do **not** add. Goal 070 needs cross-system tick/timeline records and replay evidence, not a library-level workflow engine.

### Akka.NET / Orleans

- Category: actor model / distributed actor frameworks.
- Useful much later for: scalable background simulation experiments, distributed NPC/world processes, server-like runtime.
- Risk now: too heavy, adds concurrency/distribution semantics before single-process deterministic simulation is stable.
- Decision for Goal 070: do **not** add.

## Architectural decision

Use a BCL-only Application-layer seam:

```text
Goal060 package rows
+ Goal061 review package RC
+ Goal062 spatial detail rows
+ Goal063 gameplay consequence rows
+ Goal064 living world rows
+ Goal065 interlocked gameplay rows
+ Goal066 settlement rows
+ Goal067 narrative rows
+ Goal068 combat/magic rows
+ Goal069 world event/weather/day-night/crisis rows
-> integrated multi-step timeline rows
-> conflict/arbitration ledger
-> cascading cross-system consequences
-> replay determinism
-> save/load checkpoints
-> Unity Alpha timeline markers
```

The simulator must be deterministic and explainable:

- fixed seed + scenario row identity;
- ordered ticks/phases;
- causal event queue records;
- conflict resolution records;
- cross-system deltas;
- stable hashes;
- invalid/fake/leak diagnostics;
- no live LLM/provider/RAG calls;
- no Runtime/GamePackage schema changes.

## Future dependency candidates

These libraries can be reconsidered after the in-house domain model proves what it needs:

- SimSharp for process/resource simulation;
- Stateless for workflow/quest state-machine authoring;
- actor frameworks only after single-process deterministic simulation and persistence are mature.
