# Goal 082A — Source Formatting Physical-Line Repair & Guard Backstop

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

```text
C:\Users\endim\LLMGameCreator\
```

## Branch

```text
main
```

## Codex reasoning

very high

## Status

Bounded P0 hotfix/audit goal.

## Problem

Post-Goal-082 GitHub audit found a source-format regression that the current quality gate did not catch.

Current `origin/main` is expected to have the user’s docs-only commit `21f2525 adult docs` on top of the Goal 082 commits `2a74a39` and `f26309b`. Do not revert or rewrite that docs-only commit.

The problem is not the adult docs. The problem is that several C# files in current `main` are rendered by GitHub raw as one physical line. Examples found during audit:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/EditDrivenGamePackageHandoffProbe.cs
src/LLMGameCreator.Application/Design/EditDrivenUnityAlphaStreamingAssetsHandoff/EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService.cs
src/LLMGameCreator.Application/Design/EditDrivenUnityAlphaStreamingAssetsHandoff/EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner.cs
src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs
```

Goal 082 quality evidence still claims `rawPhysicalOneLineSourceCount=0`, `zeroLfSourceCount=0`, `crOnlySourceCount=0`, and `maxLineLength=315`, so the scanner has a false negative. This must be fixed before any new feature goal.

## Objectives

1. Repair the current physical formatting of all malformed Goal082-related `.cs` files.
2. Strengthen the Goal082 source-format guard so it detects raw physical one-line C# files, zero-LF files, CR-only files and raw physical lines over 500 characters.
3. Add regression tests with synthetic CR-only and synthetic zero-LF/one-physical-line samples.
4. Regenerate Goal082 quality evidence and add compact Goal082A evidence.
5. Keep Goal082 `accepted=false`; do not mark its manual gate passed.
6. Preserve the user’s separate `adult docs` commit and treat it as docs context, not as source/evidence scope damage.
7. Commit and push the result to `origin/main` even if GREEN/BLOCKED/FAILED.

## Exact behavior

### 1. Preflight

- Confirm current branch is `main`.
- Fetch `origin/main`.
- Confirm latest `origin/main` includes `21f2525 adult docs` above Goal082 commits.
- Confirm Goal082 artifacts exist and still say `implementationStatus=GREEN`, `accepted=false`, gate required.
- Confirm `AlphaRuntimeBootstrap.cs` baseline hash before edits.
- Inspect raw physical source format, using byte/text checks, not only Roslyn/logical line APIs.

The raw scan must include at minimum:

```text
src/LLMGameCreator.Application/Design/EditDrivenUnityAlphaStreamingAssetsHandoff/**/*.cs
src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignUnityAlphaStreamingAssetsHandoffControl.cs
src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignUnityAlphaStreamingAssetsHandoffControl.Designer.cs
src/LLMGameCreator.WinForms/CompositionRoot.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/EditDrivenGamePackageHandoffProbe.cs
tests/LLMGameCreator.Tests/Application/EditDrivenUnityAlphaStreamingAssetsHandoff/**/*.cs
tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenUnityAlphaStreamingAssetsHandoffProductSmokeTests.cs
```

### 2. Reformat malformed C# files

For every malformed `.cs` file in the scan scope:

- Preserve behavior.
- Preserve public API unless the file has a compile issue.
- Reformat into normal readable C# with LF or CRLF line endings accepted by GitHub raw and common diff tools.
- Keep max physical line length <= 500.
- Keep files below 1000 logical lines.
- Do not turn the parent WinForms page into a god-form.
- Do not touch `AlphaRuntimeBootstrap.cs`.
- Do not change Unity project settings, scenes or prefabs.

### 3. Strengthen scanner

Update Goal082 source-quality scanner so it fails if any guarded source file has:

- zero LF bytes while containing substantial C# source;
- CR-only line separators;
- one raw physical line with substantial C# source;
- raw physical max line length > 500;
- minified / too-few-lines-for-size shape;
- over 1000 logical lines;
- missing Unity probe scan coverage;
- missing WinForms parent scan coverage;
- missing Goal082 Application scan coverage.

The scanner must record at least:

```text
rawByteScannedFileCount
zeroLfSourceFileCount
crOnlySourceFileCount
rawPhysicalOneLineSourceFileCount
rawPhysicalMaxLineLength
logicalMaxLineLength
unityProbeIncludedInRawScan
winFormsParentIncludedInRawScan
goal082ApplicationFilesIncludedInRawScan
syntheticCrOnlySourceRejected
syntheticZeroLfOnePhysicalLineRejected
```

### 4. Tests

Add or update focused tests proving:

- synthetic CR-only sample fails the scanner;
- synthetic zero-LF one-physical-line C# sample fails the scanner;
- real Goal082 scan includes Unity probe, WinForms parent and Goal082 Application files;
- real Goal082 scan passes after repair;
- Goal082 product smoke still reads mirrored StreamingAssets payload and negative proof still rejects tamper/missing/fake success.

### 5. Evidence

Regenerate Goal082 evidence under:

```text
.llmgc/procedural/goal-082-edit-driven-unity-alpha-streamingassets-handoff/
```

Add Goal082A evidence under:

```text
.llmgc/procedural/goal-082a-source-format-physical-line-repair/
```

Goal082A evidence must include at least:

```text
source-format-physical-line-repair-report.md
source-format-physical-line-repair-scan.json
```

The evidence must show before/after malformed source counts and must explicitly state that Goal082 remains accepted=false.

### 6. Docs/state

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
```

Required state:

- Goal082 remains produced for review and accepted=false.
- Goal082A records the P0 source-format repair as produced for review / GREEN if successful.
- Adult docs commit is recorded only as docs context, not as an active implementation milestone.
- Do not mark Goal082 verification passed.

### 7. Artifact scope

Update `.devflow/artifact-scope/artifact-scope-policy.json` for scenario:

```text
goal-082a-source-format-physical-line-repair
```

The final artifact-scope command must pass for the committed paths.

## Quality gate

- No `.cs` file in the guarded scope may be one raw physical line.
- No zero-LF or CR-only C# source in guarded scope.
- Max raw physical line length <= 500.
- Max logical line length <= 500.
- No minified C# source.
- No C# file over 1000 logical lines.
- `AlphaRuntimeBootstrap.cs` unchanged by hash.
- Goal082 manual gate remains required and accepted=false.
- No forbidden path changes.
- No absolute local paths, timestamps, heavy logs, scratch/tamper files in tracked evidence.
- No mojibake markers in changed files.

## Stop / block conditions

Return BLOCKED, commit and push if:

- the source-format repair requires touching forbidden Runtime/schema/provider/Lua/project files;
- `AlphaRuntimeBootstrap.cs` must be modified to pass the hotfix;
- adult docs create an active conflict that cannot be resolved without broad docs rewrite;
- check-all or artifact scope fails for reasons caused by this hotfix and cannot be repaired within allowed files.

Return FAILED, commit and push if:

- compilation breaks and no bounded repair inside allowed paths is possible;
- tests regress due to this hotfix and cannot be repaired inside allowed paths.

## Mandatory commit / push policy

Always commit and push to `origin/main` even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:

```text
GREEN Goal 082A source format physical line repair
BLOCKED Goal 082A source format physical line repair
FAILED Goal 082A source format physical line repair
```

## Final report

Use `final-report-format.md`.
