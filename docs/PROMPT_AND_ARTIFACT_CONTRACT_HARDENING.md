# Prompt And Artifact Contract Hardening

Status: authoritative prompt/contract requirements  
Scope: LLM prompts, repair prompts, artifact envelopes and validation gates  
Non-scope: provider integration, code changes, schema migrations

## 1. Prompt Families

Allowed prompt families:

- `design_discussion`: creative but bounded by profile, capability atlas and enabled overlays.
- `strict_single_json_artifact`: one artifact, one contract, exact JSON only.
- `batch_generation_round_robin`: many independent records of one contract family.
- `targeted_repair`: repair one failed artifact using a validation report.
- `lua_module_proposal`: propose a sandboxed manifest-declared module before approval.
- `quality_sample_review`: non-authoritative quality notes only.

A prompt family must state whether its output is draft, review, repair, module proposal or final candidate artifact. None of these outputs is final authority.

## 2. Required Prompt Fields

Every production prompt must include:

- model role;
- artifact contract;
- allowed input context;
- exact output schema;
- immutable ids/enums;
- validation gates;
- repair policy;
- forbidden outputs.

Recommended prompt envelope:

```text
role:
contract:
schema_version:
allowed_context:
immutable_ids:
immutable_enums:
enabled_overlays:
disabled_overlays:
output_rules:
validation_gates:
repair_policy:
forbidden_outputs:
```

If any required field is missing, the prompt is not production-ready.

## 3. Strict JSON Output Rules

Strict JSON prompts must say:

```text
Return exactly one JSON object.
Do not wrap the response in Markdown.
Do not include code fences.
Do not include explanations before or after JSON.
Do not translate machine-readable ids or enums.
Do not invent fields outside the schema.
Use null or an empty list only where the schema allows it.
```

The parser must reject markdown-wrapped JSON, extra prose, invalid JSON and contract mismatch.

## 4. ID / Enum Preservation Rules

Prompts must list immutable ids and enums explicitly. The model may reference them, but must not rename, translate, pluralize, prettify or infer replacements.

Examples:

- `quest/help_healer` must stay `quest/help_healer`.
- `turn_based` must not become `turn based`.
- `dialogue_combat` must not become `conversation battle`.

Repair prompts must include a machine-readable immutable field list. If a repair changes an immutable field, the repair fails.

## 5. Context Pack Selection Rules

Context selection must be contract-driven:

```text
task -> artifact contract -> capability ids -> profile -> selected snippets -> prompt pack
```

Do not include:

- full project dumps;
- unrelated old chats;
- disabled overlay context;
- unapproved canon as fact;
- unrelated schemas;
- large generated artifacts when a summary is enough.

Strict artifact prompts prefer exact schema, enums, validator rules and one task input over broad lore.

## 6. Overlay / Safety / Content Flags

Content overlays must be explicit. Adult/NSFW, violence, horror, political themes or other sensitive overlays are not hidden defaults.

When an overlay is disabled:

- the prompt must say it is disabled;
- generated records must not include it;
- context selector must exclude matching snippets.

When an overlay is enabled:

- records must be tagged;
- export/platform filters must be respected;
- core mechanics must not silently depend on the overlay;
- approval is required before promotion.

## 7. Validation Gate Requirements

Every artifact contract must name validation gates. Minimum gates:

- JSON syntax;
- schema version;
- required fields;
- id/enum preservation;
- reference closure;
- duplicate detection;
- contract-specific semantic checks.

Playable artifacts also require:

- package assembly validation;
- `GamePackageValidator`;
- runtime smoke where applicable;
- export dry-run where applicable.

## 8. Repair Prompt Requirements

Repair prompts must include:

- original input;
- invalid output;
- validation errors and warnings;
- target artifact contract;
- exact schema;
- immutable fields;
- fields allowed to change;
- max repair attempt number;
- instruction to return only repaired JSON.

Repair prompt must not ask for unrelated improvements. It must not invite redesign.

Example repair boundary:

```text
You may change only `title`, `description`, `objectives[].text` and missing required fields.
You must not change `id`, `quest_id`, `objective_id`, enum values or referenced item ids.
```

## 9. Artifact Contract Versioning

Every artifact must include:

- `schema_version`;
- `artifact_id` or stable id assigned by C#;
- `artifact_kind`;
- `expected_artifact_contract`;
- source context references;
- generated/proposed state;
- optional validation metadata only after C# adds it.

Contract version changes require:

- migration or compatibility note;
- validator update;
- prompt update;
- docs update;
- focused tests.

Do not overload an existing contract with incompatible meaning. Create a new version when shape or semantics change.

## 10. Examples Of Hardened Prompts

Strict single JSON artifact prompt:

```text
Model role: batch_generator
Artifact contract: quest_pack_v1
Schema version: 0.1
Allowed input context: approved game_profile_v1, approved faction ids, approved item ids.
Output schema: one JSON object with `schema_version`, `quests`.
Immutable ids/enums: copy provided ids exactly; do not translate enum values.
Validation gates: JSON syntax, schema, quest id uniqueness, objective refs, reward refs.
Repair policy: repairable for missing fields and enum drift; blocked for new mechanics.
Forbidden outputs: markdown, C#, Lua, Unity code, package mutation, explanations.
```

Targeted repair prompt:

```text
Model role: repair_generator
Artifact contract: dialogue_pack_v1
Invalid artifact: included below.
Validation report: included below.
Immutable fields: dialogue ids, node ids, speaker ids, enum values.
Allowed changes: missing choice text, missing target node refs when target exists.
Return exactly one repaired JSON object.
```

Lua module proposal prompt:

```text
Model role: lua_module_proposer
Target: chunk_rule_pack_v1 producer
Required manifest: id, version, category, capabilities, input_schema, output_schema, config_schema, deterministic=true.
Forbidden Lua features: io, os, debug, package, load, loadfile, dofile, network, filesystem, global writes.
Output: module proposal artifact only, not trusted Lua activation.
```

## Prompt Acceptance Gate

A prompt is acceptable for a Codex task only when it names the source-of-truth docs, strict non-goals, artifact contract, validation path and expected test/smoke evidence.
