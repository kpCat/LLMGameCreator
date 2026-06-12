# BATCH_004_REPORT — Capability and generator module manifest helpers

## Files generated

- `lua/generation/capability_manifest.lua`
- `lua/generation/module_manifest.lua`
- `lua/generation/generator_plan.lua`
- `docs/lua/capability_and_module_manifests.md`
- `manifests/generation_manifest.manifest.json`
- `tests/generation_manifest_examples.lua`
- `BATCH_004_REPORT.md`

No `BATCH_REPORT.md` file is included; this batch follows the numbered report convention.

## Contracts introduced

### Capability manifest contract

`capability_manifest.lua` defines and validates capability records with:

- capability id;
- inputs;
- outputs;
- config schema;
- supported runtime targets;
- supported time modes;
- supported combat modes;
- dependencies;
- incompatibilities.

Capability ids use lowercase dot notation such as `world.chunk.generate`. This matches the existing `manifest.capabilities` style used by earlier Lua modules.

### Module manifest contract

`module_manifest.lua` validates Lua generator module manifests with:

- module id;
- version;
- category/title/purpose;
- capabilities;
- input/output/config schemas;
- runtime targets;
- supported time and combat modes;
- module/capability dependencies;
- module/capability incompatibilities;
- unsafe feature metadata.

Module ids use lowercase slash notation such as `generation/module_manifest/v1`.

### Generator plan contract

`generator_plan.lua` validates ordered generator plans with:

- plan id;
- runtime target;
- turn mode;
- combat mode;
- plan inputs;
- expected outputs;
- ordered generator plan steps.

Each step can declare:

- step id;
- module id;
- capability id;
- inputs;
- outputs;
- config;
- config schema;
- dependencies on earlier steps;
- incompatibilities with other steps;
- supported runtime/time/combat modes.

Batch 004 intentionally validates ordered steps. It does not implement arbitrary graph sorting; that is reserved for a later orchestration batch.

## Dependencies between files

The three Lua modules are self-contained and do not read files or import each other.

- `capability_manifest.lua` can be used independently for capability registry slices.
- `module_manifest.lua` can be used independently for Lua module manifest registry slices.
- `generator_plan.lua` can be used after capability/module selection to validate an ordered plan.
- `tests/generation_manifest_examples.lua` expects a host-injected table containing these three modules.

## How to validate manually

1. Open the ZIP and confirm the listed file paths are present.
2. Parse `manifests/generation_manifest.manifest.json` as JSON.
3. In a Lua 5.4 host, inject the modules into the test runner:

```text
local report = generation_manifest_examples.run({
  capability_manifest = capability_manifest,
  module_manifest = module_manifest,
  generator_plan = generator_plan
})
```

4. Confirm `report.ok == true`.
5. Confirm invalid examples return diagnostics instead of throwing user-facing failures.

## Known limitations

- No filesystem access, module discovery, registry persistence, or C# integration is implemented.
- No arbitrary graph topological sort is implemented.
- No execution of generator steps is implemented.
- Schema validation is intentionally lightweight; deeper schema validation remains delegated to dedicated validation modules.
- The manual examples require host-side module injection because this library avoids file loading and external dependencies.

## Next recommended batch

Batch 005 — World blueprint.

Do not proceed to Batch 005 without an explicit user command.

## Implemented claims only

This batch implements manifest and plan helper modules only. It does not generate maps, entities, quests, UI, Unity scenes, or C# code.
