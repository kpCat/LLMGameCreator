/goal

Repo URL:
https://github.com/kpCat/LLMGameCreator

Working copy:
C:\Users\endim\LLMGameCreator\

Branch:
main

Goal ID / name:
Goal 079 — Edit-Driven Spine Quality Consolidation & Acceptance Dashboard

Codex reasoning:
very high

Primary objective:
After five consecutive feature goals (074-078), perform a bounded quality consolidation that is still product-useful: add a BCL-only Application seam and a WinForms dashboard tab that consolidates the whole edit-driven playable spine, verifies the real artifact chain, proves acceptance readiness, indexes negative proof, and classifies remaining debt. Do not build another gameplay layer on top yet.

Why this goal exists:
Goals 074-078 produced a real chain:
- Goal 074: schema-driven campaign authoring/review workspace.
- Goal 075: edit/validate/apply loop.
- Goal 076: playable preview refresh and staged handoff proof.
- Goal 077: disk-backed review package materialization.
- Goal 078: review package playable session/replay proof.

This chain is now useful, but it is spread across multiple sidecar services and evidence folders. The next task must not be paper-only. It must make the chain inspectable and harder to fake, while keeping the architecture bounded and not touching forbidden runtime/schema/provider/Unity areas.

Required preflight:
1. Confirm branch is `main`.
2. Fetch `origin/main` for inspection only.
3. Confirm current history includes Goal 078 commit `4a68e9c` or a later commit containing it.
4. Confirm Goal 078 evidence exists:
   - `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/edit-driven-review-package-playable-session-report.md`
   - `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/quality-gate-scan.json`
   - `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/package-read-proof.json`
   - `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/playable-session-replay-proof.json`
   - `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/tamper-negative-proof.json`
5. Confirm Goal 078 report is GREEN, accepted=false, and gate `edit_driven_review_package_playable_session_verification required`.
6. Record Goal 078 as accepted by user handoff before Goal 079 in current state docs. Do not mutate Goal 078 artifact report to accepted=true.
7. Confirm Goal 072 remains historical BLOCKED/produced-for-review evidence and does not block Goal 079.
8. Confirm `c8343e8 docs adaptive quality` remains P3 docs-context debt unless already resolved by a later commit. Do not block Goal 079 on it.
9. Inspect `AlphaRuntimeBootstrap.cs` line count and hash. It must remain read-only/no-change.

Read-first:
Use `read-first.md` in this task folder.

Allowed files:
Use `allowed-files.md` in this task folder.

Forbidden files:
Use `forbidden-files.md` in this task folder.

Exact behavior:

A. Application seam

Add a new BCL-only Application namespace:

`src/LLMGameCreator.Application/Design/EditDrivenSpineQualityConsolidation/`

Recommended classes:
- `EditDrivenSpineQualityConsolidationEvidenceService`
- `EditDrivenSpineQualityConsolidationModels`
- `EditDrivenSpineQualityConsolidationHash`
- `EditDrivenSpineQualityConsolidationQualityGateScanner`
- optional narrow readers/helpers if needed, each under 700 lines.

The service must consume real artifacts from Goals 074-078 and produce a deterministic Goal 079 result. It must not call LLM/provider/media/runtime/Unity and must not mutate public GamePackage schema.

The service must verify at minimum:
1. Goal 074-078 report artifacts exist.
2. Goal 074-078 quality-gate artifacts exist where available.
3. Goal 078 is GREEN and accepted=false in its own artifact, then recorded as user-handoff accepted before Goal 079 in current state docs.
4. Goal 078 package read proof exists and records actual ledger/index/hash checks.
5. Goal 078 replay proof exists and proves final state hash equals replay final hash, and invalid replay/order/target cases reject.
6. Goal 078 tamper/negative proof exists and is not merely a report flag.
7. All consumed report hashes are deterministic and included in the Goal 079 chain manifest.
8. Current workspace parent page binds all five child surfaces: Goal 075 edit loop, Goal 076 playable refresh, Goal 077 review package, Goal 078 play session, and the new Goal 079 consolidation dashboard.
9. All child surfaces remain separate UserControls; do not merge them into the parent form.
10. Source formatting is still safe: no minified source candidates, max line <= 500, no file > 1000 lines.
11. Parent `CampaignAuthoringReviewWorkspacePageControl.cs` stays under 275 lines if possible. If unavoidable, BLOCKED unless a small extraction into the new child control keeps it bounded.
12. `AlphaRuntimeBootstrap.cs` is read-only/no-change and its line count/hash are recorded as P2/P3 debt, not repaired here.

B. Goal 079 evidence artifacts

Generate deterministic artifacts under:

`.llmgc/procedural/goal-079-edit-driven-spine-quality-consolidation/`

Required artifacts:
- `edit-driven-spine-quality-consolidation-report.md`
- `spine-chain-manifest.json`
- `acceptance-readiness-dashboard.json`
- `negative-proof-index.json`
- `workspace-binding-inventory.json`
- `source-health-scan.json`
- `quality-debt-classification.json`
- `artifact-hygiene-scan.json`
- `quality-gate-scan.json`
- `source-artifact-manifest.json`

The report must include:
- `implementationStatus: GREEN` only if all checks pass;
- `accepted: false`;
- gate `edit_driven_spine_quality_consolidation_verification required`;
- Goal 078 handoff recorded before Goal 079;
- consumed report hashes from Goals 074-078;
- P0/P1/P2/P3 debt counts;
- final report hash.

C. Quality debt classification

Classify debt without turning this into broad refactor:

P0/P1 examples:
- missing required evidence;
- parent page no longer binds a child tab that exists in Designer;
- fake/report-only product smoke;
- minified source;
- forbidden area changes;
- no real negative proof for Goal 078 session.

P2 examples:
- long but below-limit services, especially Goal 077/078 services;
- duplicated sidecar hash/read helper patterns across edit-driven seams;
- `AlphaRuntimeBootstrap.cs` very large but untouched.

P3 examples:
- docs adaptive quality context still not fully integrated;
- cosmetic dashboard wording/layout limitations.

If any P0 is detected, do not mark GREEN. Fix it inside the allowed scope if possible; otherwise commit BLOCKED.

D. WinForms dashboard

Add a new separate UserControl under Campaign Authoring Review Workspace, for example:

- `CampaignEditDrivenSpineQualityControl.cs`
- `CampaignEditDrivenSpineQualityControl.Designer.cs`

Integrate it as a separate tab in `CampaignAuthoringReviewWorkspacePageControl`.

The parent page must:
- own the new `EditDrivenSpineQualityConsolidationEvidenceService` narrowly;
- call it during normal `OnActivated()` after existing 074-078 builds;
- bind the new dashboard control with the Goal 079 result;
- keep existing tabs and child controls intact;
- avoid becoming a god-form.

The dashboard control must show at least:
- overall status/gate;
- chain summary for Goals 074-078;
- package/session proof summary;
- negative proof summary;
- source-health summary;
- P0/P1 blocker count;
- P2/P3 debt list.

E. Tests

Add focused tests under:

`tests/LLMGameCreator.Tests/Application/EditDrivenSpineQualityConsolidation/`

Required tests:
1. Service builds a GREEN consolidation result from current Goal 074-078 artifacts.
2. Service fails or returns BLOCKED diagnostics if a required Goal 078 proof artifact is missing.
3. Service fails or returns BLOCKED diagnostics if Goal 078 fake/report-only proof is simulated through a temporary tampered copy.
4. Negative proof index includes missing target, tampered payload, replay order mismatch, illegal target, and fake success without payload read from Goal 078 evidence.
5. Source health scan catches minified/overlong/god-form scenarios using temporary test inputs, not by modifying real source.
6. WinForms parent activation binds the new Goal 079 dashboard through the real parent page path, not only standalone control bind.
7. Quality gate scanner catches when Designer has the Goal 079 dashboard tab/control but parent source does not bind it.

Add product smoke:

`tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenSpineQualityConsolidationProductSmokeTests.cs`

Product smoke must prove behavior, not just `report=true`:
- build/write Goal 079 evidence;
- read the generated dashboard artifact;
- verify consumed Goal 074-078 hashes;
- verify P0/P1 count is zero;
- verify known P2/P3 debts are classified, not hidden;
- verify a temporary tampered artifact copy is rejected.

F. State docs

Update:
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`

Required state transition:
- Goal 078 is accepted by user handoff before Goal 079.
- Goal 079 is produced for review with gate `edit_driven_spine_quality_consolidation_verification required`, accepted=false.
- Goal 072 remains historical BLOCKED evidence.
- Goal 031/032 produced-for-review status remains unchanged if still present.
- `c8343e8 docs adaptive quality` remains P3 debt unless already resolved.

G. Artifact scope

Update `.devflow/artifact-scope/artifact-scope-policy.json` with scenario:

`goal-079-edit-driven-spine-quality-consolidation`

The scenario must allow only the files/areas listed in allowed-files.md.

H. Quality gate

The final Goal 079 quality gate must fail unless:
- all required Goal 079 artifacts are present and deterministic;
- parent WinForms binding is real;
- no minified source files exist;
- max C# line length <= 500;
- no C# file > 1000 lines;
- parent page is not bloated beyond the threshold;
- product smoke is not report-only;
- no absolute local paths or volatile timestamps/heavy logs are present in Goal 079 evidence;
- no forbidden areas are touched;
- artifact scope passes.

Validation:
Use `validation.md` in this task folder.

Stop / block conditions:
- BLOCKED if any required Goal 074-078 evidence is missing or inconsistent and cannot be repaired inside allowed files.
- BLOCKED if a real P0/P1 is found that requires touching forbidden areas.
- BLOCKED if parent workspace binding cannot remain bounded without broad refactor.
- BLOCKED if `check-all.ps1` fails due to Goal 079 changes and cannot be repaired inside allowed files.
- FAILED if compilation breaks and cannot be repaired inside allowed files.

Final report:
Use `final-report-format.md` in this task folder.

Mandatory commit/push policy:
Always commit and push to `origin/main` even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:
- `GREEN Goal 079 edit-driven spine quality consolidation`
- `BLOCKED Goal 079 edit-driven spine quality consolidation`
- `FAILED Goal 079 edit-driven spine quality consolidation`
