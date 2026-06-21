# Game Composition Diagnostics Spec

## Suggested service

```text
GameCompositionDiagnosticsService
```

## Suggested models

```text
GameCompositionDiagnosticsReport
GameCompositionReadiness
GameCompositionDiagnosticItem
GameCompositionRecommendedAction
GameCompositionDiagnosticsMarkdownRenderer
```

## Inputs

```text
GameBlueprint blueprint
ContentLanguagePolicy contentLanguagePolicy
CapabilityRegistry capabilityRegistry
GeneratorCatalog generatorCatalog
```

## Output fields

```text
BlueprintId
Title
GameKind
ContentLanguage
Readiness
CapabilityStatus
GeneratorCatalogOk
CapabilityDiagnostics
GeneratorCatalogDiagnostics
GeneratorPlanningDiagnostics
SelectedCurrentGenerators
RelatedPlannedGenerators
MissingGeneratorCapabilityIds
RecommendedActions
MarkdownSummary
```

## Readiness statuses

```text
BuildableNow
BuildableWithWarnings
PlannedFuture
MissingRequirements
Conflict
Invalid
```

## Readiness rules

Recommended:

```text
capability errors -> Invalid / MissingRequirements / Conflict depending on status
generator catalog errors -> Invalid
baseline ok with warnings -> BuildableWithWarnings
baseline ok no errors -> BuildableNow
future planned capabilities with no errors but planned diagnostics -> PlannedFuture
```

## Recommended actions

Examples:

```text
add missing capability
add generator manifest
wait for planned generator
remove conflicting capability
select compatible world source
select compatible presentation
```

## Determinism

Diagnostics and recommended actions must be sorted deterministically.
No current time values in reports.
No provider calls.
