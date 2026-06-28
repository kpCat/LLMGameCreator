# Candidate Contract Note - Dialogue Narrative Tooling

Candidate id: `candidate_dialogue_narrative_tooling_v1`  
Base accepted gate: `modular_generator_kernel_parallel_readiness_verification passed`  
Final candidate status: `candidate_ready_for_serial_adoption`

## Scope

This is a candidate note, not an accepted goal and not a production integration.

Allowed current outcome:

- document a future dialogue/narrative tooling boundary;
- record external technology scouting decisions;
- reuse existing package dialogue/quest contracts as the current source of truth;
- keep all changes inside `docs/candidates/candidate_dialogue_narrative_tooling_v1/`.

Non-goals:

- no public `GamePackage` schema change;
- no `.sln` or `.csproj` change;
- no WinForms UI change;
- no Unity runtime/build entrypoint change;
- no provider, media, LLM, RAG, network or Lua execution;
- no generator-library mutation;
- no accepted manual gate claim.

## Local Contract Inputs

Current repo-local narrative support already includes:

- `DialogueDefinition` with `Id`, `Title`, `StartNodeId`, `Nodes`, conditions, enter/exit effects, tags and metadata;
- `DialogueNodeDefinition` with speaker/expression/text, conditions, enter/exit effects, metadata and choices;
- `DialogueChoiceDefinition` with text, target node, close flag, conditions, requirements, costs, effects, rewards and links to quest, transaction or encounter ids;
- `QuestObjectiveDefinition` with kind, target id, required amount, conditions, completion effects, optional/hidden flags, tags and metadata;
- `GamePackageDefinition.Game.Dialogues`;
- `GamePackageDefinition.Game.Quests`;
- `GeneratedContent.Dialogues`;
- `GeneratedContent.Quests`;
- `GeneratedContent.PreservedArtifacts` for unsupported/future records;
- `NarrativeDefinitionValidator` for deterministic package validation.

The nearest accepted local contract is `docs/PACKAGE_ASSEMBLY_DIALOGUE_QUESTS_CONTRACT_V1.md`.

## Proposed Internal Boundary

Future serial adoption should introduce an internal normalized narrative record, not an external-format-first model.

Suggested logical shape:

- `dialogue_narrative_contract_v1`
- source refs: original artifact id, optional external format id, source line/span when available;
- dialogue graph: dialogue id, title, start node id, nodes and choices;
- node payload: speaker id, expression, localized/display text, conditions, enter/exit effects, metadata;
- choice payload: display text, target node id, close flag, conditions, requirements, costs, effects, rewards, quest links and metadata;
- quest hooks: start quest, advance quest, set quest stage, objective refs;
- preserved source: unsupported external constructs stored as sidecar/provenance, not silently dropped;
- diagnostics: stable codes for unsupported command, unsupported variable mutation, missing target, cyclic or unreachable node, non-deterministic host callback and schema-expansion-required.

The internal contract should remain mappable to current `DialogueDefinition`, `DialogueNodeDefinition`, `DialogueChoiceDefinition`, `QuestObjectiveDefinition` and generated-content sidecars without changing public package schema.

## Adapter Boundary

Future adapters should be editor-time only:

```text
External source (.yarn or .ink)
  -> DialogueNarrativeScriptAdapter
  -> normalized dialogue_narrative_contract_v1
  -> existing package assembly dialogue/quest mapping
  -> GamePackageDefinition.Game.Dialogues / GamePackageDefinition.Game.Quests
```

Recommended adapter contracts:

- `ImportExternalScript(sourceText, options) -> normalized record + diagnostics`
- `ExportExternalScript(normalized record, options) -> sourceText + diagnostics`
- `ValidateExternalScript(sourceText, options) -> diagnostics`

Forbidden adapter behavior:

- no runtime LLM/RAG/provider/media/network calls;
- no Unity scene access;
- no Lua execution;
- no application of generated output without deterministic validation;
- no package schema mutation;
- no hidden fallback that changes semantics without diagnostics.

## Import Direction

Initial import should be lossy only when diagnostics make the loss explicit.

Mappable now:

- Yarn/Ink simple lines to `DialogueNodeDefinition.Text`;
- speaker or tags to `SpeakerId`, `Tags` or metadata;
- options/choices to `DialogueChoiceDefinition`;
- choice target jumps to `TargetNodeId`;
- close/end markers to `CloseDialogue`;
- supported commands to declarative effects or quest links only when they match existing package-safe fields;
- source ids/spans to metadata or generated-content provenance.

Must be preserved or rejected:

- arbitrary host commands;
- external functions;
- random/time/file/network behavior;
- variable mutations that cannot map to existing conditions/effects;
- dynamic includes or non-local dependencies;
- full narrative runtime state that cannot map to current `GameRuntimeState`/quest state.

## Export Direction

Export should be optional and adapter-specific.

Allowed:

- export current normalized records into a simple `.yarn` or `.ink` authoring preview;
- include provenance comments/source refs;
- emit warnings for fields that cannot round-trip.

Not allowed in this candidate:

- treating exported `.yarn` or `.ink` as the source of truth;
- requiring Unity packages for export;
- requiring external tools at runtime.

## Absence Behavior

If no dialogue/narrative tooling adapter is installed:

- package assembly continues to use current `dialogue_pack_v1` and `quest_pack_v1` artifacts;
- current validators continue to validate `Game.Dialogues` and `Game.Quests`;
- candidate/adoption reports should mark external tooling as `absent_optional`;
- unsupported external source refs remain `future_required` or `preserved_sidecar`;
- no crash and no fake success.

If a required adapter is selected by a future serial task but missing:

- validation should fail with a stable `dialogue_narrative.adapter.missing_required` diagnostic;
- package assembly should not silently skip selected external sources.

## Deterministic Validation Strategy

Future serial adoption tests should remain small and focused:

- one fixed simple dialogue graph fixture;
- one fixed quest-linked choice fixture;
- one unsupported external-command fixture;
- one deterministic round-trip or stable-hash fixture if export is added.

Validation rules should prove:

- stable ordering by dialogue id, node id and choice id;
- no timestamp, absolute path, machine name or network data in artifacts;
- stable diagnostics for unsupported features;
- all target nodes and quest refs resolve or reject;
- unsupported external features are preserved as sidecar or rejected, never silently applied;
- no public schema, project file, Unity, WinForms, provider, LLM, RAG or Lua execution changes.

## Recommendation

- Ink: `reference_only`.
- Yarn Spinner core: `adapt_behind_adapter` in a future serial adoption task if dependency/project-file changes are explicitly allowed.
- Yarn Spinner Unity integration: `reference_only` for future Unity export/presentation work.
- Direct dependency now: rejected for this candidate.

This candidate is ready for serial adoption review as documentation/design evidence only.
