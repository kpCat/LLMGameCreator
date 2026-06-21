# GameBlueprint and Capability Graph Spec

## GameBlueprint fields

```text
BlueprintId
Title
GameKind
WorldSources
Presentations
GenerationModes
RequestedCapabilityIds
ContentLanguage
Notes
```

## Initial enum values

Game kinds:

```text
TextRpg
MapPanelRpg
FantasyOpenWorldRpg
RealisticCitySurvival
ZombieCitySurvival
CrimeSandbox
Custom
```

World sources:

```text
ProceduralPackage
ImportedRealMap
HandAuthoredMap
HybridImportedPlusGenerated
LazyInfiniteWorld
Custom
```

Presentation modes:

```text
Text
TopDown2D
Isometric2D
StrategyMap
FirstPerson3D
ThirdPerson3D
Custom
```

Generation modes:

```text
OfflineReviewed
LazyRuntime
HybridOfflinePlusLazy
HandAuthored
```

## CapabilityDefinition fields

```text
Id
Title
Description
Category
Requires
OptionalRequires
Provides
Conflicts
SupportedWorldSources
SupportedPresentations
GenerationModes
RuntimeCost
Maturity
```

## Compatibility statuses

```text
Compatible
CompatibleWithAdapter
DegradedButUsable
Conflict
UnsupportedYet
MissingRequirement
```

## Composition validator behavior

The validator should:
1. detect duplicate built-in capability ids;
2. detect unknown requested capabilities;
3. detect missing required capabilities;
4. detect direct conflicts;
5. report optional missing requirements as warnings;
6. return deterministic diagnostics.
