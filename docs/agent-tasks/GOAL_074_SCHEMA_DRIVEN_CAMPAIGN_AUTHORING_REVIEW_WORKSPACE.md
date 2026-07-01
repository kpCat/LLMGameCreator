# Codex task — GOAL 074 Schema-Driven Campaign Authoring And Review Workspace

## Assignment metadata

Repository:
https://github.com/kpCat/LLMGameCreator

Working copy:
C:\Users\endim\LLMGameCreator\

Branch:
main

Composite goal id/name:
goal-074-schema-driven-campaign-authoring-review-workspace
Goal 074: Schema-Driven Campaign Authoring And Review Workspace

Codex reasoning level:
very high

Required gate:
schema_driven_campaign_authoring_review_workspace_verification required

## Status policy
This is an aggressive composite goal, but not a free-form refactor.

At the end, always commit and push final state to origin/main, even if GREEN/BLOCKED/FAILED.

Commit message:
- GREEN: `GREEN Goal 074 schema-driven campaign authoring review workspace`
- BLOCKED: `BLOCKED Goal 074 schema-driven campaign authoring review workspace`
- FAILED: `FAILED Goal 074 schema-driven campaign authoring review workspace`

Do not mark the manual gate passed. Use `accepted=false` / produced-for-review.

## Read-first list
Read in this order:
1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CURRENT_GENERATOR_STATE.json`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
8. `.llmgc/procedural/goal-073-source-format-p0-readability-repair/source-format-p0-repair-summary.json`
9. `.llmgc/procedural/goal-072-generator-spine-quality-consolidation/quality-dashboard.json`
10. Goal 074 spec/scouting/task/launcher files.
11. Existing WinForms page/control patterns. Search narrowly for UserControls/pages/navigation; do not read the whole UI project.
12. Existing tests for WinForms or UI binding contracts, if any.
13. Application seam/source-loader/evidence patterns from Goals 060, 063, 067, 070, 071.
14. `.devflow/artifact-scope/artifact-scope-policy.json`

Before editing, report a short read-pass summary.

## Preflight handoff
Before Goal 074 implementation:
- Record Goal 073 as accepted by user handoff before Goal 074: `source_format_p0_readability_repair_verification passed before Goal 074`.
- Preserve Goal 072 as historical BLOCKED/progress evidence, not passed.
- Preserve Goal 031/032 produced-for-review/not passed.
- Set current gate to `schema_driven_campaign_authoring_review_workspace_verification required`.
- Do not start or mention Goal 075 implementation.

## Allowed files / areas
Allowed:
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/GOAL_074_SCHEMA_DRIVEN_CAMPAIGN_AUTHORING_REVIEW_WORKSPACE_SPEC.md`
- `docs/EXTERNAL_SCOUTING_GOAL_074_SCHEMA_DRIVEN_CAMPAIGN_AUTHORING_REVIEW_WORKSPACE.md`
- `docs/agent-tasks/GOAL_074_SCHEMA_DRIVEN_CAMPAIGN_AUTHORING_REVIEW_WORKSPACE.md`
- `docs/agent-tasks/GOAL_074_LAUNCHER.txt`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `.llmgc/procedural/goal-074-schema-driven-campaign-authoring-review-workspace/**`
- `src/LLMGameCreator.Application/Design/SchemaDrivenCampaignAuthoringReviewWorkspace/**`
- `tests/LLMGameCreator.Tests/Application/SchemaDrivenCampaignAuthoringReviewWorkspace/**`
- `tests/LLMGameCreator.Tests/ProductSmoke/SchemaDrivenCampaignAuthoringReviewWorkspaceProductSmokeTests.cs`
- Existing WinForms project files only in a bounded UI area discovered from local patterns, preferably a new folder/page/control family such as `src/LLMGameCreator.WinForms/**CampaignAuthoringReviewWorkspace**` or the existing equivalent pages folder.
- If required by existing WinForms navigation pattern, the minimal menu/navigation registration file(s) may be edited, but only if the pattern is clear and focused.

If the existing WinForms navigation/project pattern cannot be discovered safely, implement Application workspace + UI binding contract + UserControls, mark UI navigation integration as BLOCKED, and still commit/push.

## Forbidden files / areas
Forbidden unless explicitly listed above:
- public GamePackage schema/model
- Runtime / Runtime.Abstractions source
- broad Unity source or AlphaRuntimeBootstrap changes
- provider/LLM/RAG/media generation paths
- Lua execution / generator-library
- `.sln`, `.csproj`, package references, external dependencies
- broad WinForms global resources, generic app shell rewrites, or designer-wide refactors
- branch/merge/rebase/cherry-pick/reset/stash/clean/force-push

## Exact behavior

### 1. Application workspace seam
Create `SchemaDrivenCampaignAuthoringReviewWorkspace` in Application.

It must load prior compact artifacts from Goals 060, 061, 062, 063, 064, 065, 066, 067, 068, 069, 070, 071, 072, 073 as needed, but it should not require heavy Unity logs/build output.

Implement:
- source loader;
- workspace model;
- dynamic schema builder;
- row selector model for 3 families x 3 seeds;
- validation/diagnostics dashboard;
- review/provenance summary;
- authoring action plan with manual/auto/quarantined provenance categories;
- UI binding contract;
- evidence writer;
- validator.

The workspace must expose at least these logical panels as data, regardless of whether UI navigation is fully connected:
- campaign rows selector;
- package/materialization summary;
- spatial detail summary;
- gameplay consequence summary;
- living world/NPC/faction summary;
- economy/crafting/combat/progression/status summary;
- settlement construction/destruction/production summary;
- narrative/quest/dialogue/event summary;
- combat/magic/boss summary;
- weather/day-night/crisis summary;
- integrated timeline summary;
- interactive campaign action script summary;
- quality/debt panel from Goal 072/073.

No final LLM prose. Use localization/template keys, slots, ids and diagnostic text.

### 2. WinForms workspace surface
Add bounded WinForms UserControls following existing project patterns.

Rules:
- Every tab/sub-tab/panel added by this goal must be a separate UserControl.
- Do not dump everything into one giant form/control.
- Use schema-driven binding: controls render workspace groups/fields/diagnostics from the Application workspace contract.
- The UI should be useful even without running providers/Unity: select family/seed row, inspect summaries, diagnostics, provenance, action plan and artifact paths.
- If navigation registration is safe and local pattern is clear, register the workspace page. Otherwise leave it as a discoverable UserControl plus product-smoke/UI-binding contract and mark navigation as blocked.

### 3. Quality guard built into the goal
This goal must not reintroduce P0 source-format debt.

Changed `.cs` files must satisfy:
- no one-line/minified source files;
- no line above 500 characters;
- prefer <=300 where reasonably safe;
- new files over 900 lines require split or BLOCKED justification;
- no new AlphaRuntimeBootstrap marker route;
- no product smoke that only asserts `report passed=true`.

Run a local changed-file scanner and write its result into Goal 074 artifacts.

### 4. Evidence artifacts
Write compact deterministic artifacts under:
`.llmgc/procedural/goal-074-schema-driven-campaign-authoring-review-workspace/`

Required artifacts:
- `workspace-source-manifest.json`
- `campaign-row-selector.json`
- `dynamic-authoring-schema.json`
- `ui-binding-contract.json`
- `workspace-validation-dashboard.json`
- `review-provenance-ledger.json`
- `authoring-action-plan.json`
- `quality-gate-scan.json`
- `winforms-control-inventory.json`
- `invalid-diagnostics-matrix.json`
- `schema-driven-campaign-authoring-review-workspace-report.md`
- `artifact-scope-report.json`

No absolute machine paths. No timestamps unless existing deterministic convention requires them.

### 5. Invalid/fake/leak matrix
Cover at minimum:
- missing Goal 060/061/071/073 source artifact;
- fake family id;
- fake seed id;
- duplicate row id;
- missing schema group;
- UI binding references unknown field/group;
- candidate marked accepted without review provenance;
- final prose leak;
- provider/LLM/RAG/media-generation claim;
- Runtime/GamePackage schema mutation claim;
- Unity broad mutation claim;
- new P0 line length in changed file;
- nondeterministic ordering.

## Tests
Add focused tests:
- workspace loads 9 rows;
- schema has required panel groups;
- UI binding contract references valid schema groups/fields;
- review/provenance ledger distinguishes manual/auto/quarantined/accepted;
- authoring action plan is deterministic;
- invalid matrix matches expectations;
- evidence writer creates all required files;
- WinForms UserControls can be instantiated/bound in the safest available local pattern. Use STA if existing tests require it.

Product smoke:
- build workspace;
- write canonical artifacts;
- verify UI binding contract;
- if WinForms controls are available, instantiate/bind them or explicitly prove why navigation is blocked.

## Validation commands
Run:
```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~SchemaDrivenCampaignAuthoringReviewWorkspace|FullyQualifiedName~Goal074"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~SchemaDrivenCampaignAuthoringReviewWorkspaceProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal074|FullyQualifiedName~SchemaDriven"

.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-074-schema-driven-campaign-authoring-review-workspace"
```

Also run a changed-file max-line/mojibake scan. Report exact result.

## Stop / BLOCKED conditions
Commit/push BLOCKED if:
- WinForms integration requires broad global UI rewrite;
- existing UI project pattern is unclear and navigation cannot be safely registered;
- Application workspace cannot read prior artifacts deterministically;
- changed files introduce P0 line length or minified source;
- tests only prove report flags and not real workspace/binding data;
- check-all or artifact scope fails;
- forbidden area must be touched.

## Git policy
Use only bounded status/diff/add/commit/push commands.
Always commit/push final state.
No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Final report format
Report in Russian:
- Status: GREEN/BLOCKED/FAILED
- Gate
- What became real
- Changed files/areas
- WinForms integration result
- Quality gate result
- Tests/checks
- Artifact scope
- Git commit/push
- Limitations and next recommended step
