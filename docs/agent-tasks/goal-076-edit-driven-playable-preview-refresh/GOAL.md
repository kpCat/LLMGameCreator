<!--
Repo-relative task location after unpacking archive into repository root:
docs/agent-tasks/goal-076-edit-driven-playable-preview-refresh/GOAL.md
-->

# Goal 076 — Edit-Driven Playable Preview Refresh & Unity Handoff Smoke

Repo URL:
https://github.com/kpCat/LLMGameCreator

Working copy:
C:\Users\endim\LLMGameCreator\

Branch:
main

Codex reasoning:
very high

## Objective

Implement an aggressive composite feature goal that turns the Goal 075 schema-driven campaign edit/validate/apply loop into a real deterministic playable preview refresh handoff.

This goal must prove that accepted edit-loop output can drive a refreshed preview/materialization/handoff artifact set for the existing GamePackage/Unity player pipeline without mutating public GamePackage schema and without runtime LLM/provider calls.

The result must be more than paper evidence:

- real BCL-only Application seam;
- deterministic before/after state hashes;
- edit-driven refresh plan derived from Goal 075 data;
- replay/rollback proof preserved from Goal 075;
- staged artifact manifest consumed by product smoke;
- tamper/missing-artifact negative proof;
- bounded WinForms workspace tab/control showing the playable refresh status;
- current-state docs and procedural artifacts updated.

## Mandatory preflight

1. Confirm branch is `main`.
2. Inspect latest commit on `origin/main`.
3. Confirm Goal 075A commit `60d602b57135c8dad82b88080a821dc751220906` exists in history.
4. Detect whether commit `c8343e8` / `docs adaptive quality` exists above 60d602b.
5. Do not revert or rewrite `c8343e8`.
6. If c8343e8 is present, treat it as tracked docs-only strategic context:
   - do not block Goal 076 solely because it exists;
   - do not reorganize it unless it breaks validation;
   - if it is not indexed into CURRENT_GENERATOR_STATE/CONTEXT_INDEX/FULL_GENERATOR_GOAL_QUEUE, record this as P2/P3 debt or a compact context note only.
7. Record Goal 075 as accepted by user handoff before Goal 076. Do not create a standalone acceptance-only task.
8. Verify Goal 075 still reports:
   - `schema_driven_campaign_edit_validate_apply_loop_verification required` before handoff;
   - `accepted=false` in the original Goal 075 evidence;
   - implementationStatus GREEN;
   - WinForms parent activation binding passed after 075A.
9. Verify no existing uncommitted tracked modifications are present before starting. If unrelated untracked docs remain, list them and leave them untouched unless they are the c8343e8 docs already committed.

## Required feature behavior

### 1. Application seam

Add a new BCL-only Application area, preferably:

```text
src/LLMGameCreator.Application/Design/EditDrivenPlayablePreviewRefresh/
```

Implement a deterministic service with clear model types, for example:

- `EditDrivenPlayablePreviewRefreshService`
- `EditDrivenPlayablePreviewRefreshModels`
- `EditDrivenPlayablePreviewRefreshHash`
- `EditDrivenPlayablePreviewRefreshEvidenceService`
- `EditDrivenPlayablePreviewRefreshQualityGateScanner`

The service must consume real Goal 075 output through `SchemaDrivenCampaignEditEvidenceService`, not duplicate or hardcode Goal 075 rows.

It must produce a `EditDrivenPlayablePreviewRefreshBuildResult` containing at least:

- source Goal 075 report hash;
- selected/applied edit rows;
- before state hash;
- after state hash;
- rollback/restored hash;
- materialization refresh plan;
- preview export refresh payload reference;
- staged player handoff manifest;
- tamper/missing-artifact negative proof;
- diagnostics;
- implementation status;
- manual gate name;
- accepted=false.

The before/after hashes must prove a real state transition:

- before hash != after hash;
- rollback hash == before hash;
- replay hash == after hash.

Do not generate fake success markers.

### 2. Materialization / GamePackage refresh plan

Use existing GamePackage/materialization concepts and artifacts where available. Do not change public GamePackage schema.

Generate a deterministic refresh plan that maps Goal 075 edit results to existing package/materialization targets, for example:

- campaign metadata/semantic row impact;
- affected scenario/family ids;
- affected staged files or package logical paths;
- changed field summaries;
- validation requirements before applying;
- preview export refresh payload lineage.

If existing GamePackage materialization services can be called safely, call them. If they cannot be called without forbidden changes, produce a bounded sidecar refresh plan and clearly record why full materialization is deferred.

The output must still be behaviorally tested: rows and hashes must be derived from Goal 075 data, not hardcoded.

### 3. Unity/player staged handoff proof

Prefer not to change Unity code. First inspect existing Unity/player handoff mechanisms from prior goals.

Create a staged player handoff manifest under Goal 076 artifacts that a product smoke test reads. The proof must include:

- manifest path;
- source Goal 075 hash;
- preview refresh hash;
- expected package/logical targets;
- player-facing scenario/family ids;
- at least one assertion that would fail if the staged manifest is missing or tampered.

If existing Unity/player code already has a generic staged artifact reader, use it in tests.

If no such reader exists and a small generic helper is necessary, it may be added only under the scoped optional Unity allowance from `allowed-files.md`. The helper must remain data-driven and generic. It must not contain game-specific generated logic.

Do not bloat `AlphaRuntimeBootstrap.cs`. Inspect and report its line count/risk. If it must be changed, keep the delta tiny and justify it.

### 4. WinForms workspace usability

Extend the existing Campaign Authoring Review Workspace with a separate Goal 076 UserControl tab/surface, for example:

```text
CampaignPlayableRefreshControl.cs
CampaignPlayableRefreshControl.Designer.cs
```

The parent `CampaignAuthoringReviewWorkspacePageControl` may be updated to load/bind the Goal 076 result, but it must remain bounded and readable.

The UI must show at least:

- Goal 076 status/gate;
- before/after/rollback/replay hashes;
- count of changed rows / package targets;
- staged handoff path/hash;
- diagnostics/negative proof result;
- clear status if the refresh cannot be built.

Keep the design UserControl-based. Do not collapse everything into the parent page.

### 5. Evidence artifacts

Write deterministic artifacts under:

```text
.llmgc/procedural/goal-076-edit-driven-playable-preview-refresh/
```

Required minimum artifact set:

- `edit-driven-playable-preview-refresh-report.md`
- `playable-preview-refresh-manifest.json`
- `gamepackage-refresh-plan.json`
- `unity-player-handoff-manifest.json`
- `state-transition-proof.json`
- `tamper-negative-proof.json`
- `winforms-binding-inventory.json`
- `quality-gate-scan.json`
- `source-artifact-manifest.json`

The report must be deterministic and include a final report hash.

Manual gate:

```text
edit_driven_playable_preview_refresh_verification required
accepted=false
```

### 6. Tests

Add focused tests proving:

- service consumes Goal 075 data rather than hardcoding rows;
- before/after/replay/rollback hashes satisfy the required relationships;
- GamePackage/materialization refresh plan targets are derived from changed rows;
- staged player handoff manifest is produced and read by smoke/proof;
- missing/tampered manifest fails;
- WinForms parent workspace binds the Goal 076 control through normal activation or explicit bind path;
- quality scanner catches minified/overlong source, parent UI non-binding if applicable, and report-only smoke if applicable.

Add or update product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenPlayablePreviewRefreshProductSmokeTests.cs
```

The smoke must verify behavior, not only `report=true`.

### 7. Docs / current state

Update:

- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/CONTEXT_INDEX.md
- docs/FULL_GENERATOR_GOAL_QUEUE.md

Record Goal 076 as produced for review, accepted=false, with manual gate:

```text
edit_driven_playable_preview_refresh_verification
```

Do not erase historical Goal 072 BLOCKED evidence.

Do not mark Goal 031/032 accepted if they remain produced-for-review.

If c8343e8 docs adaptive quality is present and unindexed, either:

- add a small context/debt note that it exists as strategic docs-only context, not active implementation; or
- record it in the debt register as P2/P3.

Do not let this derail Goal 076.

## Quality gate

- No new minified/one-line `.cs` files.
- Max C# line length <= 500.
- Prefer max line length <= 180 for touched files unless generated/designer code makes that unreasonable.
- No new C# file over 1000 lines.
- `AlphaRuntimeBootstrap.cs` must not be bloated.
- WinForms remains separate UserControl composition.
- Parent workspace remains bounded and readable.
- Application seam remains BCL-only.
- No public GamePackage schema mutation.
- No runtime LLM/provider/media calls.
- No external dependencies.
- No absolute local paths, timestamps, or heavy logs in tracked Goal 076 evidence.
- Product smoke must fail on tampered/missing staged handoff data.
- Artifact scope must include Goal 076 artifacts and changed files.

## Validation commands

Use `validation.md`.

## Stop / block conditions

Return BLOCKED, with commit/push, if:

- the feature requires public GamePackage schema mutation;
- the feature requires Runtime/Runtime.Abstractions changes outside the allowed scope;
- the feature requires broad Unity rewrite or AlphaRuntimeBootstrap bloat;
- the feature cannot prove staged handoff without hardcoding success;
- check-all fails for reasons caused by this goal and cannot be repaired inside allowed files;
- artifact scope cannot be made honest without broad policy churn.

Return FAILED, with commit/push, if:

- build is broken;
- tests regress due to this goal;
- no bounded repair is possible inside allowed files.

## Mandatory commit/push policy

Always commit and push to origin/main even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:

- `GREEN Goal 076 edit-driven playable preview refresh`
- `BLOCKED Goal 076 edit-driven playable preview refresh`
- `FAILED Goal 076 edit-driven playable preview refresh`

Do not leave tracked modifications uncommitted.

## Final report

Use `final-report-format.md`.
