# Generator Plan Artifact Review UI

Status: implemented M2 workflow  
Scope: human review of staged draft artifacts before approved artifact set promotion  
Non-scope: LLM calls, Lua execution, GamePackage mutation, package export, DB schema changes

## Purpose

The Artifact Review page exposes the human-control step between draft artifact production and package assembly. The current generation baseline can produce draft artifacts, stage them, and build an approved artifact set, but full game generation needs a visible review loop before later LLM or Lua expansion.

The workflow is:

```text
.example.json
  -> draft artifacts
  -> staging snapshot
  -> Artifact Review UI
  -> approve / reject / request repair
  -> updated staging artifact
  -> updated approved artifact set
  -> validation rows and optional markdown
```

## Boundaries

C# owns staging, validation, persistence, promotion rules and approved artifact set generation. The UI only collects human decisions and calls application services.

This workflow does not:

- call an LLM or provider;
- execute Lua;
- mutate `GamePackage`;
- export a package;
- change GamePackage schema;
- change Design DB schema;
- run runtime preview.

## Capture For Review

The page accepts a `.example.json` path and calls the review application service to produce a staging snapshot with:

```text
AutoApproveValidArtifacts = false
RenderMarkdown = true
```

This is intentionally different from the one-click package export path, which may auto-approve valid artifacts for the narrow export MVP. Review capture starts artifacts as pending so the user can inspect each item before promotion.

## Decisions

Each staged artifact item can be:

- `approved`: included in the approved artifact set when validation permits it.
- `rejected`: persisted in the staging snapshot but excluded from the approved artifact set.
- `repair_requested`: persisted in the staging snapshot and excluded from the approved artifact set.
- `blocked`: remains blocked unless the review service supports a valid explicit transition.

Decision reason code, decision comment and decision timestamp are persisted in the staging snapshot when a decision is applied.

## Approved Artifact Set Rules

The approved artifact set is rebuilt from the updated staging snapshot after decisions are applied. It contains only items whose state is `approved`.

Rejected, repair-requested, blocked and invalid items are excluded. A decision that tries to approve an item with invalid JSON, validation issues or a missing expected artifact contract is rejected by the review service and recorded as a validation diagnostic.

## Persistence

The review service reuses the existing generated artifact storage:

- `artifact/generator_plan_draft_artifact_staging/latest`;
- `artifact/generator_plan_draft_artifact_staging_markdown/latest`;
- `artifact/generator_plan_approved_artifact_set/latest`;
- validation rows for the staging artifact.

No new database tables are required.

## Future Repair Loop

`repair_requested` is a durable human decision for later repair orchestration. Future repair tasks can read the staging snapshot worklist, generate bounded repair requests, and return repaired draft artifacts to the same review loop.

Package assembly remains downstream of the approved artifact set. Artifact review does not assemble or export packages directly.
