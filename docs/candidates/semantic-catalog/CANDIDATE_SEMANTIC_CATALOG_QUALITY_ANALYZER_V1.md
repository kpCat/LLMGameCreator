# Candidate Semantic Catalog Quality Analyzer v1

Status: candidate-owned offline quality and coverage analyzer.

This candidate layer analyzes an already compiled `CompiledSemanticCatalog` and produces a deterministic quality report.
It is authoring guidance for future catalog curation, not runtime behavior and not a product gate by itself.

## Flow

```text
curated draft catalog
-> SemanticCatalogDraftCompiler
-> CompiledSemanticCatalog
-> SemanticCatalogQualityAnalyzer
-> deterministic quality report
-> future reviewed catalog curation input
```

The analyzer can also run lookup smoke expectations through the existing in-memory `SemanticCatalogLookupIndex`.
It does not persist a database, query a provider, import external data or infer semantics beyond explicit catalog fields.

## Report Shape

The report contains:

- `accepted`: false only when error diagnostics are present;
- `diagnostics`: deterministic errors and warnings with stable codes, targets and messages;
- `metrics`: deterministic catalog metrics sorted by name and key;
- `lookupSmokeResults`: deterministic lookup smoke results sorted by expectation name.

Ordinary quality and coverage findings are warnings. Profile gates such as required kinds, minimum counts and lookup
smoke failures are errors because they express an explicit authoring expectation.

## Analyzer Checks

The analyzer checks:

- structural catalog metrics including total concept count, concept count by kind and relation counts;
- coverage counts for descriptions, alternate labels, tags and facets;
- top tag counts and top facet-key counts;
- missing description, alternate labels, tags and facets on important authoring kinds;
- invalid facet syntax when a facet does not contain a non-empty `key:value` pair;
- orphan concepts with no relations, tags or facets;
- asymmetric `relatedIds`;
- `relatedIds` that duplicate direct broader/narrower hierarchy;
- `relatedIds` that conflict with transitive broader/narrower hierarchy;
- required kind and minimum concept count profile gates;
- lookup smoke expectations over `SemanticCatalogLookupIndex`.

Important authoring kinds are `archetype`, `npc_archetype`, `dialogue_intent`, `quest_motif` and `theme`.
Alternate-label guidance applies to `archetype`, `npc_archetype`, `dialogue_intent` and `quest_motif`.

## Profile

`SemanticCatalogQualityProfile` supports:

- `RequiredKinds`;
- `MinimumConceptsByKind`;
- `LookupSmokeExpectations`.

Each lookup smoke expectation includes:

- `Name`;
- `Query`;
- `ExpectedConceptIds`;
- `RequireAllExpectedConceptIds`.

The analyzer records actual result ids in `SemanticCatalogLookupSmokeResult`.

## Determinism

The analyzer sorts diagnostics by severity, code, target and message. Metrics are sorted by name and key. Smoke results
are sorted by name. Concept ids use ordinal tie-breaks.

Normal quality failures do not throw. They become diagnostics.

## Non-Goals

- No external datasets.
- No qSKOS dependency.
- No RDF engine.
- No SHACL engine.
- No live network lookup.
- No provider, embedding, RAG or LLM call.
- No vector database or runtime API lookup.
- No `.sln` or `.csproj` change.
- No public GamePackage schema change.
- No WinForms, Unity, runtime, Lua, media or generator-library change.
