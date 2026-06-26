# Product Slice 121B: Unity Playable Alpha Root Artifacts Hotfix

## Task Type

Bounded hotfix inside the Goal 014 artifact family.

Do not start S122 or Goal 015.

Do not mark `unity_playable_presentation_firewall_safe_build_verification` as passed.

Do not use git commands.

## Problem

Goal 014 code and smoke evidence exist, but the reviewable repo-local artifact folder was reported missing:

```text
.llmgc/procedural/unity-playable-alpha
```

The gate cannot be accepted as an artifact verification gate until compact root artifacts are present and reviewable:

```text
.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-report.json
.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-report.md
.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-verification.md
```

The likely cause is that the product smoke can pass using a temp project root, while not regenerating repo-local root artifacts.

## Starting Gate Context

This task is allowed only after Goal 014 and S121A.

Current gate must remain:

```text
unity_playable_presentation_firewall_safe_build_verification = required
```

## Read First

Read these files before editing:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `.gitignore`
7. `.devflow/scripts/run-product-smoke.ps1`
8. `src/LLMGameCreator.Application/Design/UnityPlayableAlpha/UnityPlayableAlphaAcceptanceService.cs`
9. `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
10. `tests/LLMGameCreator.Tests/ProductSmoke/UnityPlayableAlphaSmokeTests.cs`
11. `tests/LLMGameCreator.Tests/Application/UnityPlayableAlpha/UnityPlayableAlphaAcceptanceTests.cs`

If present locally, read the current contents of:

1. `.llmgc/procedural/unity-playable-alpha/`
2. `.llmgc/procedural/alpha-runnable-build/`

If the Unity playable Alpha artifact folder is missing, do not fake report files by hand. Regenerate them through the existing acceptance service path.

## Allowed Files

You may edit only:

1. `.devflow/scripts/run-product-smoke.ps1`
2. `tests/LLMGameCreator.Tests/ProductSmoke/UnityPlayableAlphaSmokeTests.cs`
3. `tests/LLMGameCreator.Tests/Application/UnityPlayableAlpha/UnityPlayableAlphaAcceptanceTests.cs`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/CURRENT_GENERATOR_STATE.md`
6. `docs/CONTEXT_INDEX.md`

You may create or regenerate only compact review artifacts under:

```text
.llmgc/procedural/unity-playable-alpha/
```

Do not create or commit heavy Unity build/log/cache outputs. They must remain covered by `.gitignore`:

```text
.llmgc/procedural/**/build/
.llmgc/procedural/**/logs/
.llmgc/procedural/**/unity-work/
```

Do not edit public GamePackage/runtime schemas, `.sln`, `.csproj`, WinForms/UI, Unity source files, generator-library, provider/LLM/RAG/Lua/media code, or any Goal 015/S122 files.

## Required Behavior

### 1. Ensure repo-local root artifact regeneration

Make the `unity-playable-alpha` product smoke route regenerate root artifacts under the repository root, not only under a temp folder.

Acceptable approaches:

1. Update the product smoke test so the `unity-playable-alpha` smoke uses the repo root as `projectRoot` when invoked by the product smoke route.
2. Or update `.devflow/scripts/run-product-smoke.ps1` to set the existing `LLMGC_PRODUCT_SMOKE_PROJECT_DIR` environment variable to the repository root for the `unity-playable-alpha` scenario, then restore it after the run.
3. Or add a focused test/helper in the allowed test file that regenerates repo-local root artifacts through `UnityPlayableAlphaAcceptanceService` and is included by the smoke route.

Prefer the smallest change that matches existing smoke patterns in the repo.

Do not duplicate service logic.

Do not write report JSON/MD manually.

### 2. Preserve `.gitignore` intent

After regeneration, the root artifact folder should contain compact review files:

```text
unity-playable-alpha-report.json
unity-playable-alpha-report.md
unity-playable-alpha-verification.md
```

Heavy generated output may exist locally but must be ignored:

```text
.llmgc/procedural/unity-playable-alpha/build/
.llmgc/procedural/unity-playable-alpha/logs/
.llmgc/procedural/unity-playable-alpha/unity-work/
```

Do not add a broad `.llmgc/` ignore rule.

Do not ignore `.llmgc/procedural/**/*.json` or `.llmgc/procedural/**/*.md`.

### 3. Artifact content checks

The regenerated `unity-playable-alpha-report.json` must record:

```text
accepted=false
finalStatus=unity_playable_presentation_firewall_safe_build_verification
manualGate=unity_playable_presentation_firewall_safe_build_verification
previousAcceptedGate=alpha_runnable_windows_build_verification passed
productSmokeRoute=unity-playable-alpha
visiblePresentationVerified=true
movementVerified=true
interactionVerified=true
playLoopVerified=true
firewallSafeBuildVerified=true
windowsExecutableProduced=true
unityBuildProduced=true
launchVerified=true
runtimePreviewDependency=false
publicGamePackageSchemaChanged=false
projectFilesChanged=false
generatorLibraryChanged=false
```

It must include the selected package/hash evidence from Goal 014:

```text
selectedPackageId=game/content_generation/frontier-survival
selectedStyleId=frontier_survival
packageHash=3e8a42663e1a2fdabd98cdd8c30ab6188810bd4d0f4d36aa4e3089a71b952d53
assetManifestHash=3dd392bae4cbac24db34b1810a52c83cf64791521df8849c75ac61e8fdcfa595
```

The verification markdown must mention:

```text
Unity project: unity/LLMGameCreatorAlpha
Build options: BuildOptions.None
Firewall prompt observed: not observed by automated noninteractive run
```

### 4. State docs

Update state/context docs only to record this S121B hotfix.

Keep:

```json
"unity_playable_presentation_firewall_safe_build_verification": {
  "status": "required"
}
```

Do not mark it passed.

Mention that S121B regenerates compact repo-local root review artifacts while `.gitignore` keeps generated Unity build/log outputs ignored.

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

After validation, directly inspect the filesystem without git commands and confirm these files exist:

```text
.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-report.json
.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-report.md
.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-verification.md
```

Also inspect the report JSON and verification markdown for the required values above.

Scan changed text files and regenerated compact artifacts for mojibake markers.

Scan regenerated compact artifacts for local absolute paths, user/temp paths, timestamps, GUIDs, machine names, `S122`, `Goal 015`, and `goal_015`. If any appear, remove nondeterministic content by fixing generation, not by manual report editing.

Do not run git commands.

## Final Report Required

Report:

1. Changed files.
2. Regenerated root artifact files.
3. Whether build/log/unity-work heavy outputs are still only generated/ignored and not part of the intended review artifact set.
4. Exact report hash and build manifest hash from the regenerated report.
5. Verification command results.
6. Confirmation that `unity_playable_presentation_firewall_safe_build_verification` remains `required`, not `passed`.
7. Confirmation that S122/Goal 015 was not started.
8. Confirmation that no git commands were used.
