# Product Slice 121A: Unity Generated Artifact Gitignore And State Handoff Fix

## Task Type

Bounded hotfix inside the Goal 014 artifact family.

Do not start S122 or Goal 015.

Do not mark `unity_playable_presentation_firewall_safe_build_verification` as passed.

Do not use git commands.

## Starting Gate Context

This task is allowed only after Goal 014 produced the Unity playable Alpha evidence and stopped at:

```text
unity_playable_presentation_firewall_safe_build_verification = required
```

The purpose of this hotfix is not to change gameplay or architecture. It fixes repository hygiene and handoff accuracy before the gate is accepted.

## Read First

Read these files before editing:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `.gitignore`
7. `src/LLMGameCreator.Application/Design/UnityPlayableAlpha/UnityPlayableAlphaAcceptanceService.cs`
8. `tests/LLMGameCreator.Tests/ProductSmoke/UnityPlayableAlphaSmokeTests.cs`
9. `unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs`
10. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`

If present locally, also read:

1. `.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-report.json`
2. `.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-report.md`
3. `.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-verification.md`

If those `.llmgc/procedural/unity-playable-alpha` report files are missing, do not fake them. Report that they were missing and keep the gate required.

## Allowed Files

You may edit only:

1. `.gitignore`
2. `docs/CURRENT_GENERATOR_STATE.json`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CONTEXT_INDEX.md`

You may create no new source, test, project, Unity, package, schema, provider, LLM, Lua, WinForms, generator-library, `.sln` or `.csproj` files.

Do not delete generated build outputs in this task unless they are clearly temporary duplicates created by this task itself. The point is to prevent future accidental staging through `.gitignore`, not to remove evidence the user may still inspect.

## Required Fixes

### 1. Add generated Unity artifact ignore rules

Extend `.gitignore` with a small, explicit section for generated Unity and LLMGameCreator build artifacts.

The rules must ignore generated heavy outputs such as:

```text
.llmgc/procedural/**/build/
.llmgc/procedural/**/logs/
.llmgc/procedural/**/unity-work/
.llmgc/procedural/**/[Ll]ibrary/
.llmgc/procedural/**/[Tt]emp/
.llmgc/procedural/**/[Oo]bj/
.llmgc/procedural/**/[Uu]ser[Ss]ettings/

unity/**/[Ll]ibrary/
unity/**/[Tt]emp/
unity/**/[Oo]bj/
unity/**/[Bb]uild/
unity/**/[Bb]uilds/
unity/**/[Ll]ogs/
unity/**/[Uu]ser[Ss]ettings/
unity/**/*.csproj
unity/**/*.sln
unity/**/*.user
unity/**/*.pidb
unity/**/*.booproj
unity/**/*.svd
unity/**/*.pdb
unity/**/*.mdb
unity/**/*.opendb
unity/**/*.VC.db
```

Do not add a broad `.llmgc/` ignore rule.

Do not ignore `.llmgc/procedural/**/*.json` or `.llmgc/procedural/**/*.md` globally. Compact deterministic reports and verification markdown must remain possible to commit when the process requires them.

Do not ignore Unity source/template files required for the repo-local project:

```text
unity/LLMGameCreatorAlpha/Assets/**
unity/LLMGameCreatorAlpha/Packages/manifest.json
unity/LLMGameCreatorAlpha/ProjectSettings/ProjectVersion.txt
```

### 2. Fix stale Goal 014 handoff checks

Update `docs/CURRENT_GENERATOR_STATE.json` so the `goal_014_unity_playable_presentation_firewall_safe_build.checks` list matches the final Goal 014 verification report:

```text
UnityPlayableAlpha/AlphaRunnableBuild/CurrentGeneratorStateDocsTests filtered tests: 27/27 passed
unity-playable-alpha product smoke: 1/1 passed
check-all.ps1: 803/803 tests passed, build 0 warnings / 0 errors
```

Keep:

```json
"unity_playable_presentation_firewall_safe_build_verification": {
  "status": "required"
}
```

Do not change that status to `passed`.

### 3. Fix stale markdown handoff text

Update `docs/CURRENT_GENERATOR_STATE.md` and `docs/CONTEXT_INDEX.md` so they no longer recommend the old Goal 013 gate after Goal 014.

The current recommended next work must remain:

```text
unity_playable_presentation_firewall_safe_build_verification
```

Mention that generated Unity build outputs are now ignored, while compact report/verification artifacts remain eligible for review.

## Validation

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~UnityPlayableAlpha|FullyQualifiedName~AlphaRunnableBuild|FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Run:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-playable-alpha
```

Run:

```powershell
.\.devflow\scripts\check-all.ps1
```

Do not use `git status`, `git diff`, `git check-ignore`, `git rm`, `git add`, `git reset`, `git checkout` or any other git command.

Because git commands are forbidden, validate `.gitignore` by direct file inspection and by confirming the ignore rules explicitly cover the known problematic path family:

```text
.llmgc/procedural/unity-playable-alpha/build/windows/LLMGameCreatorAlpha_Data/Managed/*.dll
.llmgc/procedural/alpha-runnable-build/build/windows/LLMGameCreatorAlpha_Data/Managed/*.dll
```

## Final Report Required

Report:

1. Changed files.
2. Exact `.gitignore` rules added.
3. Whether `.llmgc/procedural/unity-playable-alpha` report JSON/MD files were present locally.
4. Verification command results.
5. Confirmation that `unity_playable_presentation_firewall_safe_build_verification` remains `required`, not `passed`.
6. Confirmation that S122/Goal 015 was not started.
7. Confirmation that no git commands were used.

If the user still sees ignored generated files in the Git UI after this task, explain in the report that they may already be staged or tracked and require the user to unstage/remove them from the Git index manually. Do not run those commands yourself.
