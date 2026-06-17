# 022_REPORT.md — post-run review template for M4_1_005

Use this template after Kilo/local agent completes `M4_1_005`.

## Branch

```text
Branch:
Base commit:
Head commit:
```

## Agent task

```text
Task id: M4_1_005
Task spec file: docs/agent-tasks/M4_1/M4_1_005_EVALUATION_MARKDOWN_GOLDEN_RECOMMENDATIONS.md
```

## Changed files

Paste the changed-file list here.

```text
```

## Required boundary check

Pass/fail each item:

```text
[ ] Only allowed files changed.
[ ] No Runtime project changes.
[ ] No Scripting project changes.
[ ] No GamePackage project changes.
[ ] No WinForms project changes.
[ ] No .sln/.csproj changes.
[ ] No provider/LLM call introduced.
[ ] No CURRENT_GENERATOR_STATE unlock.
[ ] No M5/M6/M8/M9/M10 unlock wording.
```

## Required behavior check

Pass/fail each item:

```text
[ ] Summary section is covered.
[ ] Per-contract summary section is covered.
[ ] Diagnostic hot spots section is covered.
[ ] Samples section is covered.
[ ] Recommendations section is covered.
[ ] High JSON invalid/wrapper/fence counts recommend prompt/parser/repair hardening.
[ ] High pass rate does not hide warnings.
[ ] Stable result can say contract looks stable without unlocking M5/M6.
[ ] Rendering is deterministic for identical input.
[ ] Empty diagnostics/samples do not crash.
```

## Test evidence

Focused command:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GeneratorPlanStrictLlmEvaluationMarkdownRendererTests"
```

Result:

```text
```

Full command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Result:

```text
```

## Decision

Choose exactly one:

```text
[ ] Accept M4_1_005 and continue to M4_1_006.
[ ] Request focused repair pack before continuing.
[ ] Reject run and rerun M4_1_005 with stricter prompt.
[ ] Stop for architecture/user decision.
```

## Notes for next pack

```text
```
