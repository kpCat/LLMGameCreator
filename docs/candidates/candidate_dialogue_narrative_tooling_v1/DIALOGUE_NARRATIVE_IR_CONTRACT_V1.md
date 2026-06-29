# Dialogue Narrative IR Contract v1

Candidate id: `candidate_dialogue_narrative_tooling_v1`  
Contract id: `dialogue_narrative_ir_contract_v1`  
Status: candidate-only internal contract

## Purpose

`dialogue_narrative_ir_contract_v1` is an internal intermediate
representation for future dialogue, narrative and quest text tooling. It is
not the public `GamePackage` schema and must not require a public schema change
to be validated as candidate evidence.

The IR exists so editor-time import/export adapters can normalize external
authoring formats before any later serial adoption maps accepted records into
existing package dialogue, quest and generated-content structures.

## External Stance

- Ink: `reference_only`.
- Yarn Spinner core: `adapt_behind_adapter`.
- Yarn Spinner Unity: `reference_only`.

No Ink, Yarn Spinner or Unity package is accepted by this candidate. A future
serial adoption task may add an adapter implementation only after dependency,
project-file, scope and validation review.

## Required Shape

The IR must cover:

- branching narrative through stable node ids and option target refs;
- dialogue options with line ids, optional target nodes and declarative refs;
- commands/stage directions as declarative command records;
- localization line ids plus base-locale text;
- quest text blocks as internal references or sidecar text blocks;
- NPC reaction hooks as internal references or sidecar reaction hooks;
- tags and metadata on the document, nodes, lines, options and commands;
- variables, conditions and effects only as declarative references, not
  executable code.

## Core Records

`DialogueNarrativeDocument`

- `documentId`: stable non-empty id.
- `startNodeId`: id of the first narrative node.
- `nodes`: ordered node records.
- `localization`: line id to base-locale text records.
- `questTextBlocks`: quest-facing text blocks that remain candidate-side until
  an explicit package mapping exists.
- `npcReactionHooks`: NPC reaction refs that remain candidate-side until an
  explicit package mapping exists.
- `tags` and `metadata`: deterministic descriptive fields.

`DialogueNarrativeNode`

- `nodeId`: stable unique node id inside a document.
- `lines`: localized dialogue/narrative lines.
- `options`: player or system choices.
- `commands`: declarative stage directions.
- `conditionRefs` and `effectRefs`: ids only.
- `tags` and `metadata`.

`DialogueNarrativeLine`

- `lineId`: stable localization line id or explicit generated-id placeholder
  using `generated:<stable-local-key>`.
- `speakerId`: optional speaker/NPC ref.
- `baseLocaleText`: authoring text for the base locale.
- `tags` and `metadata`.

`DialogueNarrativeOption`

- `optionId`: stable id unique within its node.
- `lineId`: stable localization line id or explicit generated-id placeholder
  using `generated:<stable-local-key>`.
- `targetNodeId`: optional target node. Empty target means close/end/no target
  according to the consuming adapter.
- `conditionRefs`, `effectRefs`, `questTextBlockRefs`, `npcReactionHookRefs`,
  `tags` and `metadata`.

`DialogueNarrativeCommand`

- `commandId`: optional stable id.
- `name`: non-empty declarative command name, such as `stage_direction`,
  `set_flag_ref` or `quest_stage_ref`.
- `targetRef`: optional declarative target id.
- `arguments`: deterministic string values only.
- `tags` and `metadata`.

Commands are not executable callbacks. Names or payloads that request C#,
Lua, provider, LLM, RAG, network, filesystem, process, reflection, script,
`eval` or `exec` behavior must fail validation.

## Validation Rules

The candidate validator must reject:

- missing or blank document id;
- duplicate node ids;
- missing or invalid start node reference;
- duplicate localization line ids;
- blank dialogue line ids and option line ids;
- line ids that are neither stable ids nor `generated:<stable-local-key>`
  placeholders;
- option target node refs that do not exist;
- blank command names;
- executable-looking command names or payloads;
- executable-looking condition/effect refs, tags or metadata;
- quest text blocks or NPC reaction hooks that claim public
  `GamePackage` schema mutation is required.

The validator is side-effect free. It must not parse external `.ink` or
`.yarn` source, execute scripts, call providers, call network APIs, run Unity,
invoke LLM/RAG, mutate package state or write files.

## Adapter Boundary

Future adapters must sit outside the IR:

```text
External source (.yarn, .ink, or another editor-time format)
  -> IDialogueNarrativeScriptAdapter
  -> DialogueNarrativeDocument
  -> deterministic validation
  -> later serial package mapping, if accepted
```

The adapter boundary may expose import, export and validation operations, but
this candidate adds no real parser and no external dependency. A tiny in-memory
test fixture may implement the interface for focused acceptance tests.

## Absence Behavior

If no adapter is installed, current package assembly continues to use existing
dialogue and quest contracts. Candidate reports may record external tooling as
`absent_optional`; they must not crash, fake success or silently change
semantics.

If a future serial adoption selects a required adapter and the adapter is
missing, validation should fail with a stable diagnostic rather than skipping
the selected external source.

## Public Schema Boundary

This IR is candidate-side only. Quest text blocks and NPC reaction hooks are
kept as internal refs or sidecar records until a separate serial task explicitly
maps them into existing package fields or approves a schema change.

For this candidate:

```text
public_gamepackage_schema_changes_required = false
```

## Authoring Projection V1

`DialogueNarrativeAuthoringProjectionBuilder` is a candidate-only deterministic
projection over `DialogueNarrativeDocument` plus
`DialogueNarrativeAuthoringAnalysis`.

The projection is side-effect free and exists for future UI/adoption review. It
does not parse Yarn or Ink, does not call external packages, does not write
files, and does not imply public `GamePackage` schema mapping.

Projection rows cover graph outline, Yarn-style localization string-table
readiness, command usage, quest text references, NPC reaction references and
an analyzer diagnostics snapshot. The string-table rows are inspired by
Yarn-style stable localization exports, but no Yarn dependency exists.

## Authoring Text Export V1

`DialogueNarrativeAuthoringProjectionTextExporter` is a candidate-only
in-memory text export over `DialogueNarrativeAuthoringProjection`.

The export returns strings only:

- CSV for string-table review rows;
- CSV for metadata review rows;
- CSV for diagnostics review rows;
- deterministic JSON snapshot text for projection review.

The string-table and metadata CSV shapes are inspired by Yarn-style
localization review exports, but this is not a Yarn or Ink parser/compiler and
does not produce Yarn/Ink compiled JSON. The JSON snapshot is a candidate
review artifact for this IR projection only.

The exporter is side-effect free: no file IO, no external parser invocation, no
Yarn/Ink package call, no provider, no LLM/RAG, no Unity, no Lua and no runtime
state mutation. It does not imply public `GamePackage` schema mapping and does
not require public `GamePackage` schema changes.

## Localization Roundtrip Review V1

`DialogueNarrativeLocalizationRoundTripReviewer` is a candidate-only in-memory
review over the string-table CSV produced by
`DialogueNarrativeAuthoringProjectionTextExporter`.

The review is inspired by Yarn-style translation CSV workflows where source
string-table export rows are handed to translators, translators edit only
`language` and `text`, and the edited CSV is reviewed against the current source
projection/export before any adoption path exists.

Required string-table columns are:

```text
language,id,text,file,node,lineNumber,lock,comment
```

The reviewer:

- matches translated rows to the current source export by stable `id`;
- detects missing, unknown and duplicate translated line ids;
- verifies protected source columns: `file`, `node`, `lineNumber`, `lock` and
  `comment`;
- treats `lock` mismatch as stale translation / needs-update evidence because
  the source text may have changed;
- optionally checks that translated rows use the requested target language;
- reports empty translated text as missing translation evidence;
- produces deterministic per-line rows, summary counts and diagnostics;
- keeps `RequiresPublicGamePackageSchemaChanges` false.

The CSV parser is deterministic and in-memory only. It supports comma, quote and
CR/LF escaping without external dependencies. Malformed CSV, missing headers and
row/header column mismatches return diagnostics instead of unhandled exceptions.

This review does not import translations into the public `GamePackage` schema,
does not execute commands or metadata, does not parse Yarn or Ink, and does not
call Yarn/Ink packages, providers, LLM/RAG, Unity, Lua, runtime services, network
or filesystem APIs.

## Authoring Diagnostics V1

`DialogueNarrativeAuthoringAnalyzer` is a candidate-only authoring analyzer on
top of the IR. It is not a Yarn/Ink parser, not a compiler and not an adoption
surface.

The analyzer accepts a `DialogueNarrativeDocument` plus optional declarative
rules/catalogs and returns:

- graph summary;
- localization readiness summary;
- command summary;
- quest text / NPC reaction reference summary;
- stable diagnostics.

It must be side-effect free: no file IO, no external parser invocation, no
Yarn/Ink package call, no provider, no LLM/RAG, no Unity, no Lua and no runtime
state mutation.

Diagnostic severities:

- `Info`: authoring observation that does not block validation.
- `Warning`: authoring risk or incomplete catalog/readiness state.
- `Error`: deterministic contract/safety failure, including validator errors
  and executable-looking command payloads.

Stable diagnostic code policy:

- codes are lower-case, dot-separated and prefixed with
  `dialogue_narrative.authoring.`;
- codes should remain stable across candidate iterations;
- diagnostics include a stable target id when one exists;
- analyzer warnings must not be converted into public `GamePackage` schema
  requirements.

Graph diagnostics:

- reachable nodes are calculated from `startNodeId`;
- unreachable nodes are warnings;
- option target edges are reported in the graph summary;
- missing option targets may be reported by composing validator errors;
- nodes with no outgoing option target and no explicit terminal marker are
  warnings;
- cycles are allowed and reported in summary/diagnostics, but cycles are not
  errors.

Localization diagnostics:

- every user-facing dialogue line, option, quest text block and NPC reaction
  hook should be represented in localization readiness summary;
- missing base-locale text for a used line id is a warning;
- orphan localization entries are warnings;
- this analyzer does not generate line ids and does not rewrite source text.

Command diagnostics:

- commands are declarative stage directions or integration hints;
- known commands pass when a catalog is supplied;
- unknown commands are warnings by default;
- unknown commands become errors only when strict command catalog mode is
  explicitly requested;
- executable-looking command names, targets or payloads remain errors.

Quest/NPC reaction diagnostics:

- optional catalogs may list known quest text block ids and known NPC reaction
  hook ids;
- unknown option references to quest text blocks or NPC reaction hooks are
  authoring warnings;
- unknown references do not imply public `GamePackage` schema coupling.
