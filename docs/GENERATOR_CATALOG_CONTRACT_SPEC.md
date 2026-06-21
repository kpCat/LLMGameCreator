# Generator Catalog Contract Spec

## GeneratorModuleManifest

Suggested fields:

```text
GeneratorId
Title
Description
Maturity
UsesLlm
Deterministic
CanRunOffline
CanRunAtRuntime
InputContracts
OutputContracts
RequiresCapabilities
ProvidesCapabilities
OptionalCapabilities
ConflictsWithGenerators
SupportedGameKinds
SupportedWorldSources
SupportedPresentations
SupportedGenerationModes
RuntimeCost
ValidationRules
Notes
```

## Generator maturity

```text
Current
Preview
Planned
UnsupportedYet
Deprecated
```

## GeneratorCatalog

Responsibilities:

```text
list manifests
find by generator id
detect duplicate ids
detect unknown capability references
detect unknown contract references where applicable
return current/planned modules separately
```

No dynamic plugin loading in this slice.

## GeneratorCatalogValidator

Should detect:
- duplicate generator ids;
- blank ids;
- unknown required capabilities;
- unknown provided capabilities if capability registry does not know them;
- conflicts pointing to unknown generator ids;
- output contracts duplicated by multiple current generators where this is unsafe;
- planned modules requested by a blueprint as warning, not crash.

## GeneratorPlanResolver

Given:
```text
GameBlueprint
CapabilityRegistry
GeneratorCatalog
```

It should return a simple planning result:
- selected current generators useful for requested/provided capabilities;
- missing planned generators;
- warnings/errors;
- no execution.

## Product smoke

Scenario:

```text
generator-catalog-contract
```

Should verify:
- catalog ids are unique;
- current strict LLM modules are present;
- package assembly/activation modules are present;
- planned future modules are present;
- baseline_generated_rpg_preview resolves to current generators;
- realistic_city_survival_imported_map_future reports planned/missing generators but does not crash;
- no LLM/provider calls.
