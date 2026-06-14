# Codex Execution Doctrine

Status: authoritative future-task doctrine  
Scope: task selection, boundaries, validation and final reporting  
Non-scope: code implementation

## Core Rule

Codex work should move LLMGameCreator toward a validated full game generation platform. It should not spend limit on disconnected polish, speculative layers or unvalidated features unless the user explicitly asks for them.

## Limit ROI Rule

A Codex task should justify its limit usage by delivering one of:

- a complete user-visible vertical slice;
- a foundational architecture doc used by future tasks;
- a validated new capability family;
- a repair/validation loop that prevents future failures.

If a task does not deliver one of those outcomes, it should be questioned before implementation.

## Required Task Shape

Every non-trivial task must state:

- source-of-truth docs;
- exact scope;
- strict non-goals;
- expected changed areas;
- validation path;
- acceptance tests;
- what Codex must not decide by itself.

## Mandatory Rules

- no small cosmetic tasks unless explicitly requested;
- prefer product slices or strategic docs;
- every task must name source-of-truth docs;
- every task must state strict non-goals;
- every task must have acceptance tests;
- every task must preserve C# / LLM / Lua boundaries;
- no new pipeline layer without user-visible value;
- no feature without validation path;
- no LLM call without contract-bound JSON and repair plan;
- no Lua execution without sandbox + manifest + validation;
- no schema change without migration plan;
- no UI without backend/service test seam;
- no GamePackage mutation without validation, dry-run/apply boundary and audit where applicable;
- no runtime dependency on LLM, provider calls, WinForms or external generation tools;
- no broad source scan for docs-only tasks;
- no broad refactor when a local seam exists.

## Source-Of-Truth Priority

For full generator tasks, read in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/FULL_GAME_GENERATION_MASTER_PLAN.md`
4. `docs/GAME_GENERATION_CAPABILITY_MATRIX.md`
5. `docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md`
6. task-specific docs such as prompt, GamePackage, validation, Lua, Design DB or export docs.

If a task conflicts with the role contract, stop and clarify rather than coding around it.

## Acceptable Codex Tasks

Good task examples:

- add one artifact contract plus validator and focused tests;
- add one capability family end-to-end through docs, artifacts, validation and assembly;
- add a bounded repair loop for one contract;
- add Lua sandbox support for one approved manifest-declared module family;
- improve artifact review workflow with service tests;
- create strategic docs that future tasks must reference.

Poor task examples unless explicitly requested:

- rename buttons without behavior change;
- add another pipeline service that only forwards calls;
- generate more templates while current contracts are weak;
- add UI before service/validator path exists;
- make the model generate a full game as one prompt;
- let Lua write package files directly;
- change package schema without migration plan.

## C# / LLM / Lua Boundary Enforcement

Codex must preserve these boundaries:

- C# owns authority, validators, storage, promotion, assembly, export and runtime.
- LLM owns drafts only.
- Lua owns deterministic sandboxed IR/config/data only.
- Human approval owns canon, capability activation and promotion.

Any task that blurs these boundaries must be split or rejected.

## Validation Requirements

A feature is not complete until it has at least one appropriate validation gate:

- schema/contract validator for new artifact shape;
- package validator for package-facing data;
- runtime smoke for playable runtime behavior;
- UI smoke only when UI wiring changed;
- repair-loop tests when repair behavior is introduced.

Docs-only tasks must still run build/test when requested by the task, or explain why they were not run.

## UI Task Rule

No UI work should begin until:

- the backend/application service exists;
- validation behavior exists;
- ownership boundary is clear;
- UI is thin over services;
- Designer layout and runtime logic stay separated.

## Schema Change Rule

No schema change is allowed without:

- explicit user approval;
- migration/compatibility note;
- validator update;
- sample update;
- tests;
- docs update;
- clear rollback or load compatibility decision.

## LLM Task Rule

No LLM generation task is acceptable unless it names:

- model role;
- prompt family;
- artifact contract;
- output schema;
- context pack;
- immutable ids/enums;
- validation gates;
- repair policy;
- forbidden outputs.

## Lua Task Rule

No Lua execution task is acceptable unless it names:

- sandbox policy;
- manifest contract;
- allowed module category;
- input/output/config schemas;
- forbidden APIs;
- deterministic RNG policy;
- validation gates;
- produced artifact contract.

## Final Report Format

Future Codex final reports should include:

```text
Changed files:
- ...

Added:
- ...

Fixed/Adjusted:
- ...

Tests:
- command: pass/fail/not run + reason

Notes:
- source-of-truth docs used
- boundaries preserved
- mojibake check result when text files with Russian content changed
```

Do not rely on git summaries unless the user explicitly requested git commands.

## Stop Conditions

Stop and ask or propose a plan when:

- more than 8-10 files would be touched;
- a task needs a new project or dependency;
- runtime would call LLM/provider/UI;
- package schema would change;
- Lua would need forbidden APIs;
- UI would own business logic;
- the acceptance criteria are missing or vague;
- a new feature has no validator.
