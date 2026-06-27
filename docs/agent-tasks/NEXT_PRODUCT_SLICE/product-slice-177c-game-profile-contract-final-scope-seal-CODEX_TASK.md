# Product Slice 177C: Game Profile Contract Final Scope Seal Hotfix

Bounded corrective hotfix for Goal 021 / S177B review findings only. This is not a new `/goal`.

## Starting context

The user has accepted the previous product gate:

```text
minimum_playable_generated_game_verification passed
```

Goal 021 / S177A/S177B currently stops at:

```text
generated_game_profile_contract_verification required
```

Keep that same gate `required`, not `passed`.

Do not start S178 or Goal 022.

## Problem found in pushed repository review

S177B improved the Goal 021 tests and kept the Goal 021 contract report valid, but the pushed `main` still contains out-of-scope changes under older artifact families.

The current pushed head changed these legacy artifacts again:

```text
.llmgc/procedural/minimum-playable-generated-game/minimum-playable-generated-game-manifest.json
.llmgc/procedural/minimum-playable-generated-game/minimum-playable-generated-game-report.json
.llmgc/procedural/minimum-playable-generated-game/minimum-playable-generated-game-report.md
.llmgc/procedural/minimum-playable-generated-game/minimum-playable-generated-game-verification.md
.llmgc/procedural/minimum-playable-generated-game/review-package/LLMGameCreatorAlpha_Data/boot.config
.llmgc/procedural/minimum-playable-generated-game/review-package/LLMGameCreatorAlpha_Data/globalgamemanagers
.llmgc/procedural/minimum-playable-generated-game/review-package/generated-scenario-summary.json
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.json
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.md
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-variants.json
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-verification.md
```

The likely cause is that full test/check-all execution still mutates tracked legacy generated artifacts. This task must seal the final pushed state for Goal 021; the broader artifact-mutation process debt must be handled only by the next approved stabilization goal, not inside this hotfix.

## Read first

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_021_GENERATED_GAME_PROFILE_CONTRACT_REFRESH.md`
8. `docs/GAME_PROFILE_CONTRACT_V1.md`
9. `docs/agent-tasks/NEXT_PRODUCT_SLICE/product-slice-177a-game-profile-contract-correctness-hotfix-CODEX_TASK.md`
10. `docs/agent-tasks/NEXT_PRODUCT_SLICE/product-slice-177b-game-profile-contract-scope-repair-CODEX_TASK.md`

## Allowed files

Only these files may remain changed after this hotfix:

```text
.llmgc/procedural/generated-game-profile-contract/**
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/product-slice-177b-game-profile-contract-scope-repair-CODEX_TASK.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/product-slice-177c-game-profile-contract-final-scope-seal-CODEX_TASK.md
src/LLMGameCreator.Application/Design/GameProfiles/GeneratedGameProfileContractAcceptanceService.cs
tests/LLMGameCreator.Tests/Application/GameProfiles/GeneratedGameProfileContractAcceptanceTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/GeneratedGameProfileContractSmokeTests.cs
```

The legacy artifact paths listed in the problem section may be touched only to restore them to the exact baseline content from:

```text
e0af861c950750dddbf7774429496c580641f766
```

They must not remain different from that baseline in the final working tree.

## Forbidden

- Do not start S178 or Goal 022.
- Do not mark `generated_game_profile_contract_verification` passed.
- Do not change public `GamePackage` schema.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms/Runtime Preview UI work.
- Do not run or modify Unity player/build entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not edit generator-library.
- Do not perform broad repository cleanup.
- Do not solve tracked ignored Unity output housekeeping in this hotfix.
- Do not change architecture or planning direction.

## Bounded git exception

This task exists to fix pushed scope drift. For this task only, git may be used for exact inspection and exact path restoration. Do not commit, push, branch, reset the whole repository, rebase, merge or alter history.

Allowed git commands only:

```powershell
git diff --name-only 5fa1912402880eb3c3b1b8e15ab36b126be4b621..HEAD
git diff --name-only e0af861c950750dddbf7774429496c580641f766..HEAD
git restore --source=e0af861c950750dddbf7774429496c580641f766 -- <listed-legacy-artifact-paths-only>
git diff --name-only e0af861c950750dddbf7774429496c580641f766 -- <listed-legacy-artifact-paths-only>
git diff --name-only
```

If `git diff --name-only e0af861c950750dddbf7774429496c580641f766 -- <listed-legacy-artifact-paths-only>` prints any legacy artifact path after the final restore, stop and report a blocker.

## Required final state

### 1. Goal 021 report remains valid

Final `.llmgc/procedural/generated-game-profile-contract/generated-game-profile-contract-report.json` must contain:

```text
accepted=false
finalStatus=generated_game_profile_contract_verification
manualGate=generated_game_profile_contract_verification
previousAcceptedGate=minimum_playable_generated_game_verification passed
contractProofPassed=true
invalidMatrix.passed=true
invalidMatrix.rejectedCount/scenarioCount=20/20
publicGamePackageSchemaChanged=false
projectFilesChanged=false
generatorLibraryChanged=false
unityBuildExecuted=false
noExternalProviderLlmRagLuaMedia=true
```

Top-level `diagnostics` must contain no `severity=error`.

Expected Goal 021 hashes should remain:

```text
profile artifact hash: 498cd01ecd0826ca832d5255c41444445106dbc476a0b369086acd60ebb763e6
pipeline plan hash: 4e5afdfe5248a8411a8f4055d2b3919b6a75c425e50815c92ffcf8f363cc41f8
deterministic report hash: 192365fc21c0bdec0d48f879f0e7089825f86c61b22b46aa4076d02a54a026d9
```

### 2. Legacy artifact drift is sealed

After all verification that may mutate the repository, restore the listed legacy artifact paths from:

```text
e0af861c950750dddbf7774429496c580641f766
```

Then verify baseline equality with:

```powershell
git diff --name-only e0af861c950750dddbf7774429496c580641f766 -- `
  .llmgc/procedural/minimum-playable-generated-game/minimum-playable-generated-game-manifest.json `
  .llmgc/procedural/minimum-playable-generated-game/minimum-playable-generated-game-report.json `
  .llmgc/procedural/minimum-playable-generated-game/minimum-playable-generated-game-report.md `
  .llmgc/procedural/minimum-playable-generated-game/minimum-playable-generated-game-verification.md `
  .llmgc/procedural/minimum-playable-generated-game/review-package/LLMGameCreatorAlpha_Data/boot.config `
  .llmgc/procedural/minimum-playable-generated-game/review-package/LLMGameCreatorAlpha_Data/globalgamemanagers `
  .llmgc/procedural/minimum-playable-generated-game/review-package/generated-scenario-summary.json `
  .llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.json `
  .llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.md `
  .llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-variants.json `
  .llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-verification.md
```

This command must output nothing.

### 3. State docs remain at review gate

State docs must remain consistent:

- `docs/CURRENT_GENERATOR_STATE.json` `last_completed_product_slice_id = goal_021_generated_game_profile_contract_refresh`.
- `last_completed_product_slice` describes Goal 021, not Goal 020.
- `gate_status = generated_game_profile_contract_verification`.
- `generated_game_profile_contract_verification` remains `required`, not `passed`.
- `docs/CURRENT_GENERATOR_STATE.md` records Goal 020 accepted before Goal 021.
- `docs/CONTEXT_INDEX.md` current next work remains `generated_game_profile_contract_verification`.
- S178/Goal 022 are not started except queue/prohibition/historical references.

## Required verification order

Run this order exactly.

### Phase A: full verification before final artifact restore

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GameProfile|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-game-profile-contract
.\.devflow\scripts\check-all.ps1
```

If any command fails because of a real code defect, fix within the allowed Goal 021 scope. If it fails because of an environment blocker, stop and report exact logs.

### Phase B: final legacy artifact restore

After `check-all.ps1`, restore the listed legacy artifact files from `e0af861c950750dddbf7774429496c580641f766`.

Do not run `check-all.ps1` again after this final restore, because the pushed repository already shows that full test execution may mutate tracked legacy generated artifacts. This is a known process debt to be addressed by the next stabilization goal.

### Phase C: final non-mutating/limited validation after restore

After final restore, run only:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GameProfile|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-game-profile-contract
```

Then re-run the exact baseline equality check for listed legacy artifact paths:

```powershell
git diff --name-only e0af861c950750dddbf7774429496c580641f766 -- <listed-legacy-artifact-paths-only>
```

It must output nothing.

Directly inspect:

```text
.llmgc/procedural/generated-game-profile-contract/generated-game-profile-contract-report.json
.llmgc/procedural/generated-game-profile-contract/generated-game-profile-contract-pipeline-plan.json
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/CONTEXT_INDEX.md
```

## Final response requirements

Report:

- exact changed files;
- exact legacy artifact files restored;
- result of the final baseline equality check for legacy artifact paths;
- regenerated Goal 021 profile/pipeline/report hashes;
- whether final valid report has zero top-level error diagnostics;
- focused/product-smoke/check-all results, clearly separating Phase A and Phase C;
- confirmation that `generated_game_profile_contract_verification` remains required, not passed;
- confirmation that S178/Goal 022 were not started;
- exact git commands used under the bounded exception.
