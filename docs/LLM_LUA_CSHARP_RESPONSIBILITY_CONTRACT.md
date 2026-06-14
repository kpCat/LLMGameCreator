# LLM / Lua / C# Responsibility Contract

Status: authoritative role contract  
Scope: generation, validation, promotion, runtime and export authority  
Non-scope: implementation changes

## 1. C# Owns Authority

C# is the only authority for:

- parsing generated artifacts;
- validating schema, ids, enums, references, dependencies and safety rules;
- storing artifacts, validation rows, approval decisions and audit records;
- promoting approved artifacts into `GamePackage`;
- assembling and exporting package data;
- runtime command execution and mutable runtime state;
- runtime preview/smoke validation;
- rollback, dry-run and repair state decisions.

C# may call an LLM or Lua module only through an explicit editor/generation workflow. Runtime must not call LLM providers or external generation providers.

## 2. LLM Owns Drafts Only

LLM may produce:

- game profile drafts;
- lore/design notes;
- contract-bound JSON artifacts;
- repair proposals for failed artifacts;
- generator module proposals before approval;
- review notes that are not pass/fail authority.

LLM output is never trusted until C# parses and validates it. A prompt response is not a package mutation, approval, runtime event or migration.

## 3. Lua Owns Deterministic Module Generation / IR Only

Lua may produce deterministic:

- generator configs;
- sparse chunk overrides;
- biome/region/map IR;
- entity, dialogue, quest, item, combat or UI IR;
- validation helper diagnostics;
- compact rules and tables.

Lua output must be JSON-serializable and manifest-declared. Lua does not own package promotion, runtime state mutation, file IO or Unity scene/code emission.

## 4. Forbidden LLM Outputs

LLM must not output:

- C# production code for direct insertion;
- WinForms Designer code;
- direct DB schema migrations;
- runtime state mutations;
- arbitrary Lua intended for immediate execution;
- unbounded JSON dumps for huge maps/worlds;
- Unity C# scripts or scenes as trusted outputs;
- hidden content overlays;
- "valid" decisions that bypass deterministic validators.

## 5. Forbidden Lua Features

Lua modules and Lua-generated code must not use:

- `io`;
- `os`;
- `debug`;
- `package`;
- `load`;
- `loadfile`;
- `dofile`;
- network access;
- filesystem access;
- external dependencies;
- global environment writes;
- nondeterministic `math.random` without an approved deterministic RNG context.

Lua must not directly mutate C# `GameState`, call provider APIs, write package files or emit executable Unity objects.

## 6. What Can Be Promoted To GamePackage

Only data that satisfies all of these conditions can be promoted:

- the artifact has a known contract and schema version;
- the artifact was parsed by C#;
- required validators passed or warnings were explicitly accepted;
- referenced ids and enums are stable;
- human approval exists when required;
- assembly mapping exists for the target package field;
- post-assembly `GamePackageValidator` passes the required gate.

Artifacts without a package mapping may remain in generated-artifact storage and produce diagnostics. They must not be silently stuffed into unrelated package fields.

## 7. What Requires Human Approval

Human approval is required for:

- game profile and feature bundle selection;
- canon/lore changes that affect future generation;
- new capability domains;
- new Lua module activation;
- package promotion;
- schema changes and migrations;
- export profile changes;
- content overlay enablement;
- repair proposals that alter approved ids, canon or mechanics.

## 8. What Requires Deterministic Validation

Deterministic validation is required for:

- JSON syntax and schema;
- artifact contract version;
- ids, enums and reference closure;
- feature bundle dependency closure;
- Lua manifest shape and unsafe features;
- map bounds, reachability and chunk rules;
- dialogue and quest graph validity;
- economy costs, outputs and inventory constraints;
- combat participants, turn order, abilities and rewards;
- package validation after assembly;
- runtime smoke before treating a generated package as playable.

## 9. Repair Loop Contract

Repair is a bounded workflow:

```text
invalid artifact
  -> deterministic validation report
  -> repair prompt with immutable fields and allowed fields
  -> repaired artifact
  -> deterministic validation
  -> approve / retry / fail
```

Default max attempts: 2.

Repair must stop when:

- the same error repeats;
- an immutable field changes;
- the failure is blocked rather than repairable;
- validation passes;
- max attempts are reached.

Repair must not redesign unrelated systems, introduce new capabilities or bypass approval.

## 10. Examples

Good:

```text
LLM generates `quest_pack_v1` JSON with quest stages and objective refs.
```

Bad:

```text
LLM writes C# quest runtime code.
```

Good:

```text
Lua `chunk_generator` produces compact sparse tile overrides for a seed/chunk.
```

Bad:

```text
Lua reads files or emits Unity scene objects directly.
```

Good:

```text
C# validates reachability and missing refs before promotion.
```

Bad:

```text
Model says "valid" and package is accepted.
```

Good:

```text
LLM proposes a new automation feature bundle with required artifact contracts and validators.
```

Bad:

```text
LLM silently adds automation runtime behavior to package JSON without a contract.
```

Good:

```text
Lua produces `ui_ir_v1` records for inventory and quest journal screens.
```

Bad:

```text
Lua writes WinForms controls or Unity MonoBehaviour code.
```
