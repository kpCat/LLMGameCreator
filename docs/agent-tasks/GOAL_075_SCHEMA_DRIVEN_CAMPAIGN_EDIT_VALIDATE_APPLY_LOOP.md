# Codex task — GOAL 075 Schema-Driven Campaign Edit/Validate/Apply Loop

## Assignment metadata

Repository:
```text
https://github.com/kpCat/LLMGameCreator
```

Working copy:
```text
C:\Users\endim\LLMGameCreator\
```

Branch:
```text
main
```

Composite goal id/name:
```text
goal-075-schema-driven-campaign-edit-validate-apply-loop
Goal 075: Schema-Driven Campaign Edit/Validate/Apply Loop
```

Codex reasoning level:
```text
very high
```

Expected manual gate:
```text
schema_driven_campaign_edit_validate_apply_loop_verification required
```

## Hard process rule

This is an aggressive development goal. It must end with commit/push to `origin/main` even if the outcome is GREEN, BLOCKED or FAILED. The commit message must honestly reflect the status:

```text
GREEN Goal 075 schema-driven campaign edit validate apply loop
BLOCKED Goal 075 schema-driven campaign edit validate apply loop
FAILED Goal 075 schema-driven campaign edit validate apply loop
```

Do not mark the manual gate as passed.

## Preflight

1. Confirm branch `main`.
2. Confirm current state is after Goal 074 formatting readability hotfix.
3. Record Goal 074 user-handoff acceptance before Goal 075:
   ```text
   schema_driven_campaign_authoring_review_workspace_verification passed before Goal 075
   ```
4. Keep Goal 072 BLOCKED historical state and Goal 073 repaired P0 state intact.
5. Keep Goal 031/032 produced-for-review/not-passed if current docs do so.
6. Do not start Goal 076.

## Read-first list

Read in this order:

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CURRENT_GENERATOR_STATE.json`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_075_SCHEMA_DRIVEN_CAMPAIGN_EDIT_VALIDATE_APPLY_LOOP_SPEC.md`
8. `docs/EXTERNAL_SCOUTING_GOAL_075_SCHEMA_DRIVEN_CAMPAIGN_EDIT_VALIDATE_APPLY_LOOP.md`
9. Goal 074 source:
   - `src/LLMGameCreator.Application/Design/SchemaDrivenCampaignAuthoringReviewWorkspace/**`
   - `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/**`
   - `src/LLMGameCreator.WinForms/CompositionRoot.cs`
   - Goal 074 tests and product smoke
   - Goal 074 evidence folder
10. Goal 063-070 evidence summaries sufficient to understand edit domains.
11. Goal 072/073 quality evidence and debt register.

Do not read the whole repository unless a local search shows a directly relevant file.

## Allowed files / areas

You may create or edit only:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/GOAL_075_SCHEMA_DRIVEN_CAMPAIGN_EDIT_VALIDATE_APPLY_LOOP_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_075_SCHEMA_DRIVEN_CAMPAIGN_EDIT_VALIDATE_APPLY_LOOP.md
docs/agent-tasks/GOAL_075_SCHEMA_DRIVEN_CAMPAIGN_EDIT_VALIDATE_APPLY_LOOP.md
docs/agent-tasks/GOAL_075_LAUNCHER.txt
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/SchemaDrivenCampaignEditValidateApplyLoop/**
src/LLMGameCreator.Application/Design/SchemaDrivenCampaignAuthoringReviewWorkspace/**
src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/**
tests/LLMGameCreator.Tests/Application/SchemaDrivenCampaignEditValidateApplyLoop/**
tests/LLMGameCreator.Tests/Application/SchemaDrivenCampaignAuthoringReviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/SchemaDrivenCampaignEditValidateApplyLoopProductSmokeTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/SchemaDrivenCampaignAuthoringReviewWorkspaceProductSmokeTests.cs
.llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/**
```

## Forbidden files / areas

Do not edit:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG/media paths
src/LLMGameCreator.Scripting/**
unity/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Also forbidden:

- external dependencies;
- broad WinForms navigation architecture rewrite;
- broad CompositionRoot refactor;
- public GamePackage schema changes;
- Runtime behavior changes;
- Unity changes;
- LLM/provider/RAG calls;
- arbitrary Lua execution;
- generated final prose promotion;
- branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. State docs

Update docs quartet consistently:

- Goal 074 accepted by user handoff before Goal 075.
- Goal 075 produced-for-review with:
  ```text
  schema_driven_campaign_edit_validate_apply_loop_verification required
  accepted=false
  ```
- Do not mark Goal 075 passed.
- Do not start Goal 076.

### 2. Application seam

Create `SchemaDrivenCampaignEditValidateApplyLoop` with small files, not one monolith. Recommended files:

- `SchemaDrivenCampaignEditValidateApplyModels.cs`
- `SchemaDrivenCampaignEditValidateApplySourceLoader.cs`
- `SchemaDrivenCampaignEditCatalog.cs`
- `SchemaDrivenCampaignEditValidator.cs`
- `SchemaDrivenCampaignApplyEngine.cs`
- `SchemaDrivenCampaignRollbackPlanner.cs`
- `SchemaDrivenCampaignEditEvidenceService.cs`
- `SchemaDrivenCampaignEditHash.cs`
- optional reports/quality scanner if useful.

Must support:

- 9 family/seed rows from Goal 074 workspace;
- editable field catalog;
- manual change-set candidates;
- deterministic auto-suggestion candidates;
- validation diagnostics;
- apply ledger;
- rollback ledger;
- before/after row diffs;
- preview/export refresh payload;
- invalid edit matrix.

Edits must remain contract-bound and deterministic. They must not promote final LLM prose.

### 3. WinForms bounded surface

Extend the Campaign Authoring Review Workspace with a bounded edit/review panel.

Rules:

- each new tab/subsection must be its own UserControl;
- avoid giant `PageControl`;
- do not use Visual Studio Designer tooling;
- hand-write Designer.cs only in the style already used in Goal 074;
- no one-line/minified files;
- no broad WinForms architecture changes.

Minimum UI contract:

- row selector can feed selected family/seed row to edit panel;
- edit panel shows editable field summary;
- validation panel shows diagnostics;
- apply/rollback panel shows deterministic before/after summary;
- if full interactive button wiring is too risky, implement UI binding inventory and product smoke proof; report any navigation limitation honestly.

### 4. Quality gate

Before final commit, enforce:

- no changed/new C# file over maxLineLength 500;
- preferred maxLineLength <= 300 where practical;
- no changed/new one-line/minified C# file;
- `CompositionRoot.cs` remains readable;
- Goal 074 WinForms files remain readable;
- no new file over 1000 lines unless justified in report;
- no test that only checks `report passed=true`.

If quality gate fails and cannot be safely repaired, commit/push BLOCKED.

### 5. Evidence

Write deterministic evidence under:

```text
.llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/
```

Required artifacts:

```text
edit-workspace-source-manifest.json
editable-schema-field-catalog.json
change-set-catalog.json
validation-diagnostics-matrix.json
apply-rollback-ledger.json
row-before-after-diff-matrix.json
preview-export-refresh-payload.json
winforms-binding-inventory.json
quality-gate-scan.json
invalid-edit-diagnostics-matrix.json
artifact-scope-report.json
schema-driven-campaign-edit-validate-apply-loop-report.md
```

Evidence requirements:

- stable ordering;
- no absolute local paths;
- no timestamps unless deterministic convention already exists;
- compact JSON;
- no heavy logs/build output;
- report contains exact gate:
  ```text
  schema_driven_campaign_edit_validate_apply_loop_verification required
  ```

### 6. Invalid/fake/leak matrix

Cover at minimum:

- unknown row id;
- unknown field id;
- illegal field domain;
- invalid value shape;
- unsafe free-form prose;
- fake provenance;
- candidate-as-applied without validation;
- rollback target missing;
- before/after hash unchanged for supposed edit;
- cross-family leakage;
- LLM/provider/RAG/media/network claim;
- Runtime/GamePackage/UI broad mutation claim;
- Unity mutation claim;
- Lua/generated code claim;
- nondeterministic ordering;
- absolute path evidence.

## Tests

Add focused tests in existing style:

```text
SchemaDrivenCampaignEditValidateApplyLoopTests
SchemaDrivenCampaignEditValidateApplyEvidenceTests
SchemaDrivenCampaignEditValidateApplyInvalidMatrixTests
SchemaDrivenCampaignEditValidateApplyWinFormsBindingTests
SchemaDrivenCampaignEditValidateApplyQualityGateTests
SchemaDrivenCampaignEditValidateApplyProductSmokeTests
```

Exact names may follow local conventions.

Tests must assert:

- rowCount=9;
- editable field catalog not empty;
- manual/auto candidates per family;
- valid apply changes before/after hash;
- rollback restores prior hash;
- invalid candidates rejected causally;
- preview/export refresh payload references changed rows;
- UI binding inventory has row selector/edit/validation/apply groups;
- formatting quality gate scans changed/new C# and CompositionRoot;
- product smoke writes all artifacts.

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~SchemaDrivenCampaignEditValidateApply|FullyQualifiedName~Goal075"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~SchemaDrivenCampaignEditValidateApplyLoopProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal075|FullyQualifiedName~SchemaDriven"

.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-075-schema-driven-campaign-edit-validate-apply-loop"
```

Also run a line/readability scan over changed/new C# files and report file path, line count and max line length.

## Stop / BLOCKED conditions

Commit/push BLOCKED if:

- public GamePackage schema is required;
- Runtime/Unity/provider/Lua broad changes are required;
- WinForms integration requires a broad architecture rewrite;
- changed/new C# formatting guard fails and cannot be safely repaired;
- edits cannot be proven state-changing;
- product smoke only proves report flags but not apply/rollback/diff;
- check-all fails;
- artifact scope fails.

## Git policy

Allowed final git flow:

```powershell
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit files>
git add -- <explicit allowed paths>
git diff --cached --name-status
git diff --cached --stat
git diff --cached --check
git commit -m "<GREEN/BLOCKED/FAILED message>"
git rev-parse HEAD
git push origin main
```

Forbidden:

```text
git checkout
git switch
git merge
git rebase
git cherry-pick
git reset
git stash
git clean
git push --force
```

## Final report format

Report in Russian:

```text
Goal 075 выполнен / заблокирован / провален
Status: GREEN / BLOCKED / FAILED
Gate: schema_driven_campaign_edit_validate_apply_loop_verification required
Commit: <hash>

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<list>

Application seam:
<summary>

WinForms surface:
<summary>

Edit/apply/rollback proof:
<counts and proof>

Quality gate:
<max line / minified / CompositionRoot / changed files>

Evidence:
<required files>

Checks:
<commands and results>

Git:
<commit/push result>

Ограничения:
<forbidden areas not touched>

Следующий разумный шаг:
<one paragraph>
```
