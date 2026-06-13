# Generator Plan Map

Status: seed architecture document  
Version: 0.1  
Related files:

```text
generator-library/atlas/generator_plan_map.json
generator-library/atlas/feature_bundle_map.json
generator-library/atlas/artifact_contracts.json
generator-library/atlas/validation_pipeline.json
generator-library/atlas/prompt_context_pack_map.json
generator-library/atlas/model_workflow_roles_and_prompts.json
generator-library/atlas/library_growth_pipeline.json
```

## Purpose

The Generator Plan Map defines how selected feature bundles become ordered, validated and repairable generator plans.

It exists to prevent the project from turning into:

```text
profile idea -> giant prompt -> random JSON/Lua/code pile -> manual cleanup forever
```

The intended flow is:

```text
game profile
  -> selected feature bundles
  -> target artifact contracts
  -> ordered generator plan
  -> context-bound steps
  -> validation gates
  -> staged artifacts
  -> approval / compilation / export
```

A generator plan is not trusted execution. It is an execution proposal with explicit contracts, inputs, outputs, gates and traceability.

## Core rule

Every generator plan step must declare:

```text
- step id;
- producer role;
- input artifacts;
- prompt context template;
- expected artifact contract;
- validation gates;
- repair policy;
- success target;
- failure behavior.
```

A step that does not declare an `expected_artifact_contract` should not be allowed to run as a production generator step.

## Why this matters

The architecture should grow by adding reusable modules, contracts and validators, not by adding one-off glue for every new idea.

Without generator plans, a feature bundle is still too abstract:

```text
feature bundle -> what actually runs?
```

With generator plans, the answer becomes deterministic:

```text
feature bundle
  -> artifact contracts
  -> known step kinds
  -> validation gates
  -> repair/staging/approval policy
```

## Plan states

Standard plan states:

```text
draft
normalized
validated
awaiting_approval
approved
running
paused_for_review
repairing
completed
failed
rejected
archived
```

Standard step states:

```text
pending
context_selected
prompt_rendered
generated
parsed
normalized
validated
repair_requested
staged
approved
compiled
exported
failed
skipped
```

## Plan build flow

The default build flow is:

```text
1. Read approved or candidate game profile.
2. Read selected feature bundles.
3. Resolve target artifact contracts.
4. Select known generators or create library growth gaps.
5. Order steps by dependencies and validation cost.
6. Attach prompt context templates.
7. Validate the plan before execution.
8. Pause for approval when required.
```

This means the system should not start generating Lua, JSON packs, Unity IR or runtime DB plans until it knows why those artifacts are needed and how they will be validated.

## Step kinds

Initial step kinds include:

```text
- design profile summary;
- semantic pack generation;
- text pack generation;
- morphology pack compile plan;
- content overlay pack generation;
- Lua module proposal;
- runtime DB build plan;
- Unity IR export plan;
- media request generation.
```

These step kinds are not final implementation classes. They are planning records that future C# services can import, validate and execute through safe pipelines.

## Plan templates

Initial plan templates:

```text
minimal_profile_to_semantic_pack
profile_to_runtime_preview
profile_to_unity_export_dry_run
adult_overlay_generation
library_gap_to_module_proposal
```

These templates keep the roadmap small. For example, early work can run only `minimal_profile_to_semantic_pack` instead of pretending the whole game can be generated at once.

## Repair policy

Repair loops are allowed only when the validation failure is targeted and repairable:

```text
- malformed JSON;
- missing required field;
- enum drift;
- minor schema mismatch;
- local semantic issue.
```

Repair should not redesign unrelated systems.

A repair prompt should include:

```text
- original input;
- invalid output;
- exact artifact contract;
- validation report;
- allowed fields/enums;
- instruction to return only the repaired artifact.
```

## Adult/NSFW overlay rule

Adult/NSFW generation is not a hidden side effect of dialogue, media or story generation.

It may appear only when:

```text
- the active profile explicitly enables the adult/NSFW overlay;
- the generator plan contains an explicit content overlay step;
- generated artifacts are tagged;
- export/platform filters are known;
- required validation and approval gates pass.
```

This allows adult content to exist in many game styles without contaminating unrelated core mechanics.

## Runtime and Unity export rule

Runtime DB and Unity export plans should be downstream products of approved artifacts.

Invalid path:

```text
LLM -> Unity code / runtime.db directly
```

Valid path:

```text
LLM/Lua -> artifact contract -> validation -> staged/approved artifact -> runtime DB build plan -> export dry-run -> compiled runtime inputs
```

## Anti-patterns

Avoid:

```text
- one huge generate-the-whole-game prompt;
- generator step without expected_artifact_contract;
- context dump without selected source reasons;
- repair prompt that invites unrelated redesign;
- hidden adult/NSFW generation inside generic dialogue or image generation step;
- Unity code generation before Unity IR and export dry-run;
- runtime.db compilation from unapproved or unvalidated artifacts.
```

## Near-term implementation meaning

This document does not require immediate C# execution.

The next practical stages can be:

```text
1. Keep generator plans as data-only atlas records.
2. Add example plan JSONs for reference profiles.
3. Add importer support later.
4. Add validation-only C# checks before execution.
5. Add execution only after contracts, validation and context selection are stable.
```
