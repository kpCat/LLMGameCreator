# Goal 078 — Edit-Driven Review Package Playable Session

Repo URL:
https://github.com/kpCat/LLMGameCreator

Working copy:
C:\Users\endim\LLMGameCreator\

Branch:
main

Codex reasoning:
very high

## Primary objective

Consume the real disk-backed Goal 077 review package and produce a deterministic headless playable/simulatable review session over that package.

Goal 077 materialized a review package with manifest, package index, player-readable index, target files, ledger, lineage proof and negative proof. Goal 078 must load those real files from disk, validate the package, execute a deterministic player-like action replay across all package rows/targets, maintain a state hash chain, prove replay determinism, reject missing/tampered/illegal action scenarios, and expose the result through a separate WinForms tab in Campaign Authoring Review Workspace.

This is not a Runtime/Unity implementation goal. It is an Application-level review-session seam that proves the generated review package is not merely materialized but actually loadable, navigable, replayable and stateful.

## Required preflight

1. Confirm current branch is `main`.
2. Fetch `origin/main` for inspection only.
3. Confirm current main includes commit `72bd57e` or later with `GREEN Goal 077 edit-driven review package materialization`.
4. Confirm Goal 077 artifacts exist and the report says:
   - `implementationStatus: GREEN`
   - `accepted: false`
   - `edit_driven_review_package_materialization_verification required`
5. Record Goal 077 as accepted by user handoff before Goal 078 in current-state docs. Do not mutate Goal 077 artifacts to accepted=true.
6. Confirm `c8343e8 docs adaptive quality` remains only P3 docs-context debt unless it directly breaks this goal.
7. Inspect `AlphaRuntimeBootstrap.cs` read-only and record line count/hash. Do not edit Unity.
8. Inspect the Goal 077 Application service sizes. `EditDrivenPlayableReviewPackageMaterializationEvidenceService.cs` is already near the line-count ceiling; do not append Goal 078 logic to it.

## Exact behavior

### 1. Application seam

Add a new BCL-only Application seam under:

```text
src/LLMGameCreator.Application/Design/EditDrivenReviewPackagePlayableSession/
```

Use clear, split files rather than one giant service. Suggested files:

- `EditDrivenReviewPackagePlayableSessionEvidenceService.cs`
- `EditDrivenReviewPackagePlayableSessionModels.cs`
- `EditDrivenReviewPackagePlayableSessionHash.cs`
- `EditDrivenReviewPackagePlayableSessionReadValidator.cs`
- `EditDrivenReviewPackagePlayableSessionReplayEngine.cs`
- `EditDrivenReviewPackagePlayableSessionQualityGateScanner.cs`

Keep each new C# file below 1000 lines, preferably below 750 where practical.

The seam must consume real Goal 077 package files, not hardcoded success markers:

- `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/edit-driven-review-package-materialization-report.md`
- `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/package-file-ledger.json`
- `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/review-package/manifest.json`
- `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/review-package/package-index.json`
- `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/review-package/player-readable-index.json`
- all ledger-listed target JSON files under the Goal 077 review package.

### 2. Package read validation

Implement a read validator that proves:

- Goal 077 report hash/source fields are present and match current files.
- Review package manifest exists.
- Package ledger exists.
- Package index exists.
- Player-readable index exists.
- Every ledger-listed file exists.
- Every ledger-listed hash matches the actual file bytes.
- Every target referenced by package/player index exists in the ledger.
- There are 9 rows and 18 target files unless the real Goal 077 evidence intentionally changes; if changed, diagnostics must explain the difference.
- No absolute local paths are stored in Goal 078 evidence.
- No timestamp-like values are stored in Goal 078 evidence.

Use `System.Text.Json` only; do not add dependencies.

### 3. Deterministic playable session model

Build a deterministic action replay from the package content.

Minimum required session actions:

- `load_package`
- for each playable row/profile combination:
  - `enter_row`
  - for each target in that row:
    - `inspect_target`
    - `apply_target_outcome`
  - `complete_row`
- `save_session`
- `replay_session`

The session must cover all 9 rows and all 18 target files from Goal 077. It must read target JSON payloads from disk and include each target file hash in the action log. Do not synthesize action success from row/target IDs alone.

The session state must include at least:

- current row/profile
- visited rows
- visited targets
- completed rows
- applied target outcomes
- package manifest hash
- package ledger hash
- player-readable index hash
- action count
- diagnostics
- deterministic state hash after each action

The state hash chain must prove:

- initial state hash differs from final state hash
- replay of the same package/action list produces the same final hash
- replay order mismatch is rejected
- missing target is rejected
- tampered target payload is rejected
- illegal action referencing a non-existent row/target is rejected
- fake success without reading target payload is rejected or impossible by construction and covered by test.

### 4. Artifacts

Generate deterministic Goal 078 artifacts under:

```text
.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/
```

Required artifacts:

- `edit-driven-review-package-playable-session-report.md`
- `playable-session-manifest.json`
- `playable-session-action-log.json`
- `playable-session-state-chain.json`
- `playable-session-replay-proof.json`
- `package-read-proof.json`
- `tamper-negative-proof.json`
- `player-command-index.json`
- `winforms-binding-inventory.json`
- `quality-gate-scan.json`
- `source-artifact-manifest.json`

The final report must remain:

- `implementationStatus: GREEN` only if all checks pass.
- `accepted: false`
- `edit_driven_review_package_playable_session_verification required`

Do not mark Goal 078 accepted.

### 5. WinForms workspace

Add a new separate UserControl tab in the existing Campaign Authoring Review Workspace:

```text
CampaignReviewPackagePlaySessionControl.cs
CampaignReviewPackagePlaySessionControl.Designer.cs
```

The parent page may be minimally updated to load and bind the Goal 078 Application seam during normal activation.

Requirements:

- Keep the new UI as a separate UserControl.
- Do not turn `CampaignAuthoringReviewWorkspacePageControl` into a god-form.
- The tab should show at least:
  - package/read proof status
  - action count
  - row/target coverage
  - initial/final/replay hash summary
  - negative proof status
  - diagnostics
- Parent `OnActivated()` must load Goal 078 result and call the new control’s `Bind` path.
- Focused test must prove the real parent page activation path binds the new control, not only standalone control tests.

### 6. Quality scanner

The Goal 078 scanner must fail if:

- New or touched C# files are minified/one-line.
- Any new or touched C# file exceeds 1000 lines.
- Any C# line exceeds 500 characters.
- Parent workspace has the Goal 078 tab/control in Designer but does not bind the Goal 078 result in `.cs`.
- Product smoke only checks a report flag without reading real package files.
- Goal 078 evidence contains absolute local paths, timestamp-like values, heavy logs, or scratch tamper files.
- `AlphaRuntimeBootstrap.cs` is edited by Goal 078.
- Forbidden areas are changed.

### 7. Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/EditDrivenReviewPackagePlayableSession/
```

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenReviewPackagePlayableSessionProductSmokeTests.cs
```

Required test coverage:

- Service builds Goal 078 artifacts from real Goal 077 package.
- Reads actual package target payloads from disk and records their hashes.
- Action log covers all rows and targets.
- State hash chain initial != final.
- Replay final hash == original final hash.
- Missing target file rejected.
- Tampered target file rejected.
- Illegal action target rejected.
- Replay order mismatch rejected.
- Parent workspace activation binds Goal 078 result into the new UserControl.
- Scanner negative test fails when the tab exists but parent bind is missing.
- Product smoke reads the generated session artifacts and validates real package read/replay proof, not only `report=true`.

### 8. Docs/state update

Update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md` only if needed for P2/P3 notes.

State requirements:

- Record Goal 077 as passed by user handoff before Goal 078.
- Set active gate to `edit_driven_review_package_playable_session_verification`.
- Record Goal 078 as produced for review, accepted=false.
- Keep Goal 072 historical BLOCKED evidence as historical, not current blocker.
- Keep Goal 031/032 produced-for-review status unchanged if still present.
- Keep c8343e8 adaptive docs note as P3 unless you actually resolve it within allowed scope.

### 9. Artifact scope

Update `.devflow/artifact-scope/artifact-scope-policy.json` for scenario:

```text
goal-078-edit-driven-review-package-playable-session
```

The scenario allowlist must include the new task pack, new `.llmgc` artifact root, docs quartet, WinForms additions, new Application namespace, new tests, product smoke, and policy/debt files.

Run final artifact scope validation.

## Quality gate

- No forbidden path changes.
- No public GamePackage schema changes.
- No Runtime/Runtime.Abstractions changes.
- No Unity changes.
- No providers/LLM/RAG/media provider changes.
- No Lua/Scripting changes.
- No generator-library changes.
- No `.sln` or `.csproj` changes.
- No external dependencies.
- No minified/one-line `.cs` files.
- Max C# line length <= 500.
- No new/touched C# file over 1000 lines.
- Avoid appending to already-large Goal 076/077 services.
- Product smoke must prove behavior over real files, not only report flags.
- Goal 078 evidence must be deterministic and free of absolute local paths/timestamps/heavy logs/scratch files.

## Validation

Use `validation.md` exactly.

## Stop / block conditions

Mark BLOCKED and commit/push if:

- Real package-loaded playable session requires touching Runtime, Unity, public GamePackage schema, providers, Lua, generator-library, `.sln`, or `.csproj`.
- Goal 077 package evidence is missing or not GREEN and cannot be repaired inside allowed files.
- Parent WinForms binding cannot be implemented without broad UI rewrite.
- Tests cannot read package files from disk and only fake/report flags would be possible.
- `check-all.ps1` cannot complete and the failure/hang is caused by Goal 078 code.

Mark FAILED and commit/push if:

- Compilation breaks and cannot be fixed inside allowed files.
- New tests regress and cannot be fixed inside allowed files.
- Artifact scope cannot be made clean without violating forbidden paths.

## Mandatory commit/push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:

- `GREEN Goal 078 edit-driven review package playable session`
- `BLOCKED Goal 078 edit-driven review package playable session`
- `FAILED Goal 078 edit-driven review package playable session`
