# Product Slice 013: Catalog-backed Composition Diagnostics Foundation

## Goal

Add a consolidated composition diagnostics/reporting layer that consumes:

```text
GameBlueprint
CapabilityRegistry
GameBlueprintCompositionValidator
GeneratorCatalog
GeneratorCatalogValidator
GeneratorPlanResolver
ContentLanguagePolicy
```

and produces a single deterministic report describing whether a game blueprint is currently buildable, partially planned, missing support, or conflicting.

This slice does not execute generators and does not add UI. It prepares the backend data that a future UI wizard/Workbench page can show.

## Why now

Slice 011 added GameBlueprint + capability compatibility.
Slice 012 added GeneratorCatalog + generator planning.

The next necessary foundation is a single report that combines both layers:

```text
blueprint validity
capability diagnostics
generator catalog diagnostics
selected current generators
related planned generators
missing generator support
content language summary
recommended next actions
```

Without this report, the future UI would have to stitch together multiple services ad hoc.

## Non-goals

Do not implement:
- UI wizard;
- plugin execution;
- generator execution;
- semantic world model;
- imported map pipeline;
- lazy world generation;
- procedural quest engine;
- runtime changes;
- package schema changes.

## Product smoke

Add scenario:

```text
composition-diagnostics-report
```

It should verify:
- baseline generated RPG preset produces a buildable/current report;
- realistic city imported-map future preset produces a not-currently-buildable report with planned/missing diagnostics;
- broken blueprint reports errors;
- report ordering is deterministic;
- no LLM/provider calls.
