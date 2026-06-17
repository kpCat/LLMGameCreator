# M4_1_010 — Real evaluation artifact discovery and safe handoff

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_010`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_when_real_artifacts_may_exist`

Depends on:

```text
- User may have run real strict LLM evaluation locally.
- BASELINE/check-all is green.
```

Unlocks:

```text
- M4_1_001 real report import/analyzer when a report path is found.
- M4_1_007 gate decision report when enough evidence exists.
```

Risk level: low

Expected changed files count: 1-4

## Gate status

Allowed before current gate review: yes.

Requires user approval: no, if limited to discovery/reporting and no production behavior changes.

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/agent-tasks/M4_1/M4_1_001_REAL_EVALUATION_REPORT_IMPORT.md
.devflow/CONTEXT_BUDGET_POLICY.md
```

Context budget:

```text
Read only this task spec, current-state docs, strict evaluation plan, and at most 5 candidate files under .llmgc/generator-plans/ if they exist.
```

Do not read:

```text
- all .llmgc artifacts;
- large raw responses unless needed;
- M5/M6 docs;
- src/** unless adding a discovery helper is explicitly required.
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmEvaluationArtifactService.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactGenerationArtifactService.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmEvaluationServiceTests.cs
```

## File boundaries

Allowed files:

```text
.devflow/CURRENT_RUN.md
.devflow/BLOCKERS.md
.devflow/NEXT_TASK.md
.devflow/OVERNIGHT_RUN_REPORT.md
docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
```

Optional code files only if the task is explicitly upgraded from reporting to implementation:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/*StrictLlmEvaluation*.cs
tests/LLMGameCreator.Tests/Design/*StrictLlmEvaluation*.cs
tests/fixtures/strict-llm-evaluation/*
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.GamePackage/**
LLMGameCreator.sln
*.csproj
```

Deleted files: none

## API / implementation contract

New interfaces: none for discovery-only task.

New classes: none for discovery-only task.

If implementation is explicitly approved, prefer a small helper that accepts paths/content and returns a typed discovery report. Do not call LLM/provider.

## Exact behavior

Discovery-only behavior:

```text
1. Check whether .llmgc/generator-plans/ exists.
2. Look for likely strict evaluation/generation artifacts by filename:
   - generator_plan_strict_llm_evaluation.json
   - generator_plan_strict_llm_artifact_generation.json
   - markdown report if present
3. Record discovered paths, sizes, and whether the files appear parseable.
4. Do not copy secrets or raw prompts into the report.
5. Do not mutate GamePackage.
6. Update BLOCKERS.md if no real report exists.
```

Success behavior:

```text
- If a real evaluation report exists, update NEXT_TASK to M4_1_001 with the discovered path.
- If only generation audit exists, recommend EvaluateLatestAudit or report import path.
- If neither exists, write clear blocker: real evaluation report missing.
```

Failure behavior:

```text
- Missing folder -> blocker, not exception.
- Invalid/ambiguous artifact -> blocker with candidate paths.
```

Diagnostic/report labels:

```text
m4_1.discovery.no_folder
m4_1.discovery.no_report
m4_1.discovery.report_found
m4_1.discovery.ambiguous_candidates
m4_1.discovery.invalid_json
```

## Proof tests

Tests/proofs to add before/with implementation:

```text
- Discovery report lists found evaluation artifact path when fixture/candidate exists.
- Missing .llmgc/generator-plans/ produces blocker text, not a crash.
- Ambiguous multiple candidates are reported without choosing randomly.
- No provider/LLM calls occur.
```

For discovery-only docs/devflow execution, proof may be the final `CURRENT_RUN.md`/`BLOCKERS.md` report and `check-all.ps1`.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Focused commands: not applicable unless code is added.

## Stop conditions

Stop if:

```text
- more than 5 candidate artifacts must be read;
- raw prompts/secrets would need to be copied into docs;
- task requires provider/LLM call;
- task requires GamePackage mutation;
- task requires schema/dependency/project changes;
- report path is ambiguous and user decision is needed.
```

## Non-goals

```text
- Do not run a real LLM evaluation.
- Do not import/analyze full report here if M4_1_001 is the better task.
- Do not unlock M5/M6.
```

## Expected final report

Final report must include:

```text
- whether .llmgc/generator-plans/ exists;
- candidate files found;
- chosen next task or blocker;
- whether JSON looked parseable if inspected;
- commands run;
- risk notes.
```

## Next task pointer

If report exists:

```text
Task source: agent_task_spec
Task id: M4_1_001
Task spec file: docs/agent-tasks/M4_1/M4_1_001_REAL_EVALUATION_REPORT_IMPORT.md
Reason: Import/analyze discovered real M4.1 evaluation report.
```

If no report exists:

```text
Stop: real strict LLM evaluation report missing.
```
