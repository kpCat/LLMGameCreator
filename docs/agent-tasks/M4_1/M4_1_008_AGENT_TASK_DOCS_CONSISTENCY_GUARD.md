# M4_1_008 — Agent task docs consistency guard

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_008`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_with_user_approval`

Depends on:

```text
- BASELINE/check-all is green.
- User wants task-pack generation to be guarded by repository tests/scripts.
```

Unlocks:

```text
- Safer future generated task packs.
```

Risk level: low/medium

Expected changed files count: 3-8

## Gate status

Allowed before current gate review: yes

Requires user approval: yes, because this may add docs consistency tests or scripts.

## Source of truth

Source-of-truth docs:

```text
.devflow/RECURSIVE_TASK_SELECTION_PROTOCOL.md
.devflow/CONTEXT_BUDGET_POLICY.md
.devflow/DEFINITION_OF_DONE.md
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/_TASK_TEMPLATE.md
docs/agent-tasks/_TASK_READINESS_CHECKLIST.md
docs/agent-tasks/_GATE_MATRIX.md
docs/agent-tasks/_SYSTEM_GATES.md
```

Existing patterns to inspect:

```text
tests/LLMGameCreator.Tests/** docs/current-state consistency tests if present
.devflow/scripts/check-devflow-state.ps1
.devflow/scripts/check-all.ps1
```

## File boundaries

Allowed files:

```text
tests/LLMGameCreator.Tests/**/AgentTask*Tests.cs
tests/LLMGameCreator.Tests/**/Docs*Tests.cs
.devflow/scripts/check-devflow-state.ps1
.devflow/scripts/check-all.ps1
docs/agent-tasks/**
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.WinForms/**
LLMGameCreator.sln
*.csproj
```

Deleted files: none

## API / implementation contract

New interfaces: none.

New classes:

```text
Optional test-only helper class for scanning docs/agent-tasks markdown files.
```

New methods:

```text
Test helper methods only unless a devflow script update is explicitly chosen.
```

Public contracts changed: no.

Schema changed: no.

New dependencies: no.

## Exact behavior

The guard should detect broken task-pack contracts:

```text
- docs/agent-tasks/000_INDEX.md exists.
- docs/agent-tasks/001_TASK_PACK_LEDGER.md exists.
- docs/agent-tasks/002_NEXT_PACK_REQUEST.md exists.
- Every task spec listed in 000_INDEX.md exists.
- Every executable task spec contains required headings/fields.
- Every executable task spec has a Proof tests section.
- Every executable task spec has System gates and Stop conditions.
- Locked M5/M6 task specs remain locked while CURRENT_GENERATOR_STATE blocks them.
```

Input contract:

```text
repository markdown files
```

Output contract:

```text
test pass/fail or devflow script pass/fail
```

Failure behavior:

```text
- Missing required file/section fails with a readable assertion or script error.
- The guard must not auto-edit docs.
```

Diagnostic/failure behavior:

```text
Use test assertion messages or script errors that name the missing file/section.
```

Validation rules:

```text
- Guard must be deterministic.
- Guard must not read all src/.
- Guard must not require internet.
- Guard must not require a real LLM/provider.
```

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- index references existing task spec files;
- required framework files exist;
- every executable M4_1 task spec contains Proof tests, System gates, Stop conditions, Expected final report, Next task pointer;
- M5/M6 specs are still marked locked while current state blocks M5/M6;
- NEXT_PACK_REQUEST points to a pack id and has quality bar.
```

Required pass tests:

```text
Current docs/agent-tasks folder passes the guard.
```

Required fail/reject tests:

```text
If using pure test helper over in-memory strings, a missing Proof tests section fails. If testing real files only, document why a negative test is not feasible without temp fixtures.
```

Fake/corpus requirements: temp markdown fixtures allowed; no real LLM call.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Focused test command:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~AgentTask"
```

Docs consistency commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
```

Runtime scenario commands: not applicable.

## Stop conditions

Stop if:

```text
- task requires new test project;
- task requires new dependency;
- task requires editing many unrelated docs;
- task requires making M5/M6 executable;
- task requires more than 8 files;
- check-all fails after 2 repair attempts.
```

## Non-goals

```text
- Do not implement a full markdown parser.
- Do not enforce formatting beyond required sections/links.
- Do not update phase roadmap semantics.
- Do not unlock future phases.
```

## Expected final report

Final report must include:

```text
- guard implemented as tests/scripts;
- exact files/sections checked;
- focused test result;
- check-devflow-state result if script changed;
- check-all run directory;
- next task pointer.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_001
Task spec file: docs/agent-tasks/M4_1/M4_1_001_REAL_EVALUATION_REPORT_IMPORT.md
Reason: Return to real evaluation report import/analyzer when a report exists, or stop and request real evaluation evidence.
User approval: required if report location is ambiguous
```
