# Validation Pipeline Atlas

Status: seed architecture document  
Version: 0.1  
Related files:

```text
generator-library/atlas/validation_pipeline.json
generator-library/atlas/artifact_contracts.json
generator-library/atlas/capability_atlas.json
generator-library/atlas/reference_profiles.json
```

## Purpose

The Validation Pipeline Atlas defines how generated artifacts move from raw LLM/Lua output into trusted editor, runtime, Unity export or database build inputs.

It exists to avoid two bad extremes:

```text
1. Manually reviewing every generated item forever.
2. Blindly trusting LLM output as runtime truth.
```

The intended model is:

```text
LLM/Lua draft
  -> parse
  -> normalize
  -> validate
  -> stage
  -> approve when needed
  -> compile/export
  -> runtime load
```

Most bulk generation should be checked automatically. Human approval is reserved for decisions that change architecture, canon, content overlays, runtime targets or trust boundaries.

## Core rule

```text
LLM proposes.
Lua generates deterministic data/IR.
C# validates and promotes.
Unity/runtime consumes compiled data.
```

Generated content is not trusted because it looks plausible. It becomes usable only after it passes the required validation levels for its artifact contract.

## Artifact states

Standard states:

```text
draft
  -> generated
  -> parsed
  -> normalized
  -> validated
  -> staged
  -> approved
  -> compiled
  -> exported
  -> runtime_loaded
```

Failure states:

```text
rejected
archived
```

A failed artifact should keep its validation report so it can be repaired, compared, sampled or used to improve prompts.

## Validation levels

### Level 0 — JSON and shape validation

Checks:

```text
- valid JSON;
- expected top-level object/array shape;
- required top-level fields;
- primitive field types;
- JSON-serializable data only.
```

This level catches malformed model output quickly and cheaply.

Repair is allowed for drafts. A repair prompt must be narrow: preserve meaning, return only corrected JSON, do not rename ids or enums.

### Level 1 — IDs, enums and references

Checks:

```text
- canonical slash/dot ids;
- stable enum values are not translated or renamed;
- duplicate ids;
- required references resolve;
- declared dependencies exist;
- runtime targets are known;
- artifact contract id is known.
```

This level prevents the common LLM problem where it creatively translates or improves machine-facing ids.

### Level 2 — Artifact contract semantics

Checks:

```text
- required sections match the artifact contract;
- must_not rules are not violated;
- allowed sources are respected;
- content tags are present when required;
- adult/NSFW material is not hidden in unmarked artifacts;
- new facts are marked as proposals unless already approved;
- no direct GamePackage mutation unless the contract explicitly allows patch artifacts;
- no unapproved codegen output;
- no unrestricted Lua/runtime authority.
```

This is the main data-only trust boundary.

### Level 3 — Cross-artifact consistency

Checks consistency across artifacts:

```text
- semantic traits referenced by text packs exist;
- morphology targets have lexemes or fallbacks;
- dialogue references match NPC/quest references;
- formulas exist and are safe;
- asset refs exist or have asset request packs;
- Unity IR references known entities/data;
- runtime DB build sources exist;
- content overlays are compatible with the selected profile/export target.
```

This level is where independent generated packs are stitched together safely.

### Level 4 — Headless smoke test

Runs deterministic non-Unity checks:

```text
- generator plan dry-run;
- GamePackage patch dry-run;
- quest graph reachability;
- dialogue graph reachability;
- combat formula bounds;
- economy/production graph smoke tests;
- inventory/equipment consistency;
- runtime DB query plan smoke tests.
```

This level is important because many problems are formally valid but mechanically broken.

### Level 5 — Export dry-run

Checks export implications without trusting final output:

```text
- Unity runtime target support;
- Unity IR adapter compatibility;
- asset index binding;
- runtime DB build plan dry-run;
- compiled content size estimate;
- no requirement to load thousands of loose runtime JSON files;
- save overlay compatibility.
```

This is the gate before Unity/runtime-facing output.

### Level 6 — Human approval

Human approval is required for:

```text
- new capability domains;
- new artifact contracts;
- new runtime targets;
- enabling a new content overlay for a project/profile;
- major story canon decisions;
- promoting new Lua library modules from staging;
- new codegen/export paths;
- any unsafe-boundary change.
```

Human approval is not intended for every generated item. Once a profile enables an optional content overlay, bulk generated entries under that overlay can be validated automatically and sample-reviewed.

## Adult/NSFW content overlay handling

Adult/NSFW support is an optional project/profile content overlay, not a separate genre and not a hidden default.

When enabled, generated artifacts must remain:

```text
- tagged;
- filterable;
- export-aware;
- separate from core mechanics;
- compatible with project settings and runtime target constraints.
```

This applies to text, image requests, future animation requests, dialogue routes, scene metadata and relationship content.

The overlay is approved at the profile/project level. Individual generated entries should then pass automated checks and sampling review instead of requiring manual approval one by one.

## Repair loops

A repair loop may be used for:

```text
- invalid JSON;
- missing required fields;
- wrong shape;
- known enum/id drift when the expected value is available;
- missing content tags;
- local contract normalization.
```

A repair loop must not be used automatically for:

```text
- new artifact contracts;
- new runtime targets;
- unsafe boundary changes;
- enabling content overlays;
- major canon decisions;
- generated source code output.
```

Default maximum attempts: 2.

## Sampling review

Bulk artifacts should not require manual review of every record.

Typical bulk artifacts:

```text
semantic_pack_v1
text_pack_v1
rule_pack_v1
dialogue_pack_v1
asset_request_pack_v1
```

Default sampling target: 5%.

Increase sampling when:

```text
- a new model or prompt preset is used;
- warning rate rises;
- a content overlay is newly enabled;
- a new genre/profile is generated;
- output affects canon or runtime mechanics.
```

## Model roles

Recommended roles:

```text
designer_llm
  Larger/flexible model for lore, style, mechanics, high-level drafts.

batch_generator_llm
  Fast local model for large batches of strict JSON artifacts.

repair_llm
  Narrow repair role for bounded JSON/contract fixes.

validator_service
  Deterministic C# service. This is the trust boundary.
```

The validator service is the validator of record. An LLM may explain or propose repairs, but it does not decide that an artifact is trusted.

## Next implementation direction

This seed does not require C# changes yet.

The next implementation layer should read these atlas files as data and expose them in editor-side planning workflows. Until then, they serve as the architectural contract for prompt design, Lua library expansion and artifact generation.
