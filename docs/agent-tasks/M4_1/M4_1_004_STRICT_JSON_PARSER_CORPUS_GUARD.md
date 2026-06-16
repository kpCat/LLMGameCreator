# M4_1_004 — Strict JSON parser corpus guard

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_004`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_with_user_approval`

Depends on:

```text
- BASELINE/check-all is green.
- User approved deterministic M4.1 parser/corpus coverage.
```

Unlocks:

```text
- M4_1_005 evaluation markdown/golden guard.
- M4_1_006 repair prompt guardrails.
```

Risk level: low

Expected changed files count: 2-6

## Gate status

Allowed before current gate review: yes

Requires user approval: yes, because this adds new fixtures/tests.

Approval text required in NEXT_TASK.md:

```text
User approval: approved
```

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
.devflow/MODELING_STRATEGY.md
```

Context budget:

```text
Read only this task spec, the strict parser class, current strict parser tests, diagnostic code model, and only the fixtures created by this task.
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictJsonResponseParser.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactGenerationModels.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictJsonResponseParserTests.cs
```

## File boundaries

Allowed files:

```text
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictJsonResponseParserTests.cs
tests/fixtures/strict-llm-raw-output/**
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Allowed production files only if an added fixture exposes a confirmed parser bug:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictJsonResponseParser.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactGenerationModels.cs
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

New classes: none unless a tiny test fixture loader is needed inside the test file or tests namespace.

New methods:

```text
Prefer test helper methods local to GeneratorPlanStrictJsonResponseParserTests.
```

Public contracts changed: no unless a bug requires one new diagnostic code.

Schema changed: no.

New dependencies: no.

## Exact behavior

The strict parser contract is intentionally strict:

```text
- Accept exactly one JSON object.
- Reject empty response.
- Reject Markdown fences.
- Reject text before JSON.
- Reject text after JSON.
- Reject JSON array root.
- Reject malformed JSON.
- Return deterministic diagnostic codes.
```

Input contract:

```text
raw LLM response string + optional contract id
```

Output contract:

```text
GeneratorPlanStrictJsonParseResult with Ok/Json/Diagnostics
```

Failure behavior:

```text
- Never throw for normal malformed LLM output.
- Return Ok=false and a diagnostic code.
- Preserve contract id in diagnostics when supplied.
```

Diagnostic codes expected:

```text
strict_llm_artifact_generation.json_markdown_fence
strict_llm_artifact_generation.json_text_wrapper
strict_llm_artifact_generation.json_invalid
strict_llm_artifact_generation.json_root_not_object
```

Validation rules:

```text
- Same fixture must produce same diagnostic code across runs.
- Fixture tests must not call ILlmChatClient/provider/network.
- Production behavior must not be loosened from strict parser to permissive extraction unless a separate approved task says so.
```

Security/sandbox rules: no provider/network/file writes outside fixtures.

Persistence rules: fixtures only.

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- fixture valid_minimal_json_object.txt -> Ok=true
- fixture empty_response.txt -> JsonInvalid or existing empty diagnostic behavior
- fixture markdown_fenced_json.txt -> JsonMarkdownFence
- fixture text_before_json.txt -> JsonTextWrapper
- fixture text_after_json.txt -> JsonTextWrapper
- fixture two_json_objects.txt -> JsonTextWrapper or JsonInvalid, but deterministic
- fixture json_array_root.txt -> JsonRootNotObject
- fixture broken_trailing_comma.txt -> JsonInvalid
- fixture invalid_escape.txt -> JsonInvalid
- parser diagnostic preserves supplied contract id
```

Required pass tests:

```text
Valid minimal object parses and returns original normalized JSON string currently expected by parser behavior.
```

Required fail/reject tests:

```text
Each invalid fixture fails with a stable code and no exception.
```

Regression tests:

```text
If a real model output later exposes a new wrapper form, add it as a redacted fixture in a follow-up task.
```

Golden/snapshot fixtures: fixture input files are the golden input corpus.

Fake/corpus requirements: fixtures only; no real LLM call.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Focused test command:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GeneratorPlanStrictJsonResponseParserTests"
```

Docs consistency commands: `check-all.ps1` unless a dedicated docs gate exists.

Manifest integrity commands: not applicable.

Artifact schema commands: parser fixture tests.

Package validator commands: not applicable.

Runtime scenario commands: not applicable.

Snapshot/golden commands: fixture tests.

## Stop conditions

Stop if:

```text
- task requires changing parser semantics from strict rejection to permissive extraction;
- task requires more than one production parser file change;
- task requires new dependency;
- task requires real LLM/provider call;
- fixture behavior is ambiguous and no deterministic expected code can be chosen;
- check-all fails after 2 repair attempts.
```

## Non-goals

```text
- Do not repair JSON.
- Do not extract JSON from Markdown fences.
- Do not mutate generated artifacts.
- Do not change artifact contracts.
- Do not change prompt text.
```

## Expected final report

Final report must include:

```text
- fixtures added;
- expected diagnostic code per invalid fixture;
- parser/tests changed;
- focused test result;
- check-all run directory;
- next task pointer.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_005
Task spec file: docs/agent-tasks/M4_1/M4_1_005_EVALUATION_MARKDOWN_GOLDEN_RECOMMENDATIONS.md
Reason: Add golden coverage for evaluation report recommendations after parser diagnostics are fixture-guarded.
User approval: required
```
