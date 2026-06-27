# Product Slice 177B: Game Profile Contract Scope Repair Hotfix

Bounded corrective hotfix for Goal 021 / S177A review findings only. This is not a new `/goal` and must not start S178 or Goal 022.

## Starting context

The user has accepted the previous product gate:

```text
minimum_playable_generated_game_verification passed
```

Goal 021 / S177A currently stops at:

```text
generated_game_profile_contract_verification required
```

Keep that same gate `required`, not `passed`.

## Problem found in pushed repository review

The current pushed repository has a product-correct-looking Goal 021 profile-contract report, but the final S177A pushed state does not match the bounded hotfix perimeter.

A pushed commit after `e0af861c950750dddbf7774429496c580641f766` changed files outside the S177A allowed Goal 021 artifact family, including prior product-slice artifacts and Unity review-package files. The Codex report claimed the hotfix changed only the GameProfiles service/tests, state/context docs and `.llmgc/procedural/generated-game-profile-contract/**`; that claim is false for the pushed repository.

Do not accept `generated_game_profile_contract_verification` and do not prepare Goal 022 until this scope repair is complete.

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

## Scope

Allowed:

- Repair S177A scope drift and keep Goal 021 evidence self-consistent.
- Restore the unrelated artifact files listed below to their pre-final-S177A-hotfix content if they differ because of the last pushed hotfix.
- Edit only Goal 021 GameProfiles service/tests if needed to keep required-field validation covered.
- Regenerate compact Goal 021 artifacts under `.llmgc/procedural/generated-game-profile-contract/` only.
- Update `docs/CURRENT_GENERATOR_STATE.json`, `docs/CURRENT_GENERATOR_STATE.md`, `docs/CONTEXT_INDEX.md`, and `docs/FULL_GENERATOR_GOAL_QUEUE.md` only when their Goal 021 status/hash/check text must match regenerated Goal 021 artifacts.

Forbidden:

- Do not start S178 or Goal 022.
- Do not mark `generated_game_profile_contract_verification` passed.
- Do not change public `GamePackage` schema.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms/Runtime Preview UI work.
- Do not run or modify Unity player/build entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not edit generator-library.
- Do not perform broad repository cleanup or tracked-ignored Unity output housekeeping beyond restoring the specific out-of-scope files listed here.
- Do not change architecture or planning direction.

## Bounded git exception

This task exists because pushed files drifted outside the prior hotfix perimeter. For this task only, git may be used for local inspection and exact path restoration. Do not commit, push, branch, reset the whole repository, rebase, merge or alter history.

Allowed git commands only:

```powershell
git diff --name-only e0af861c950750dddbf7774429496c580641f766..HEAD
git restore --source=e0af861c950750dddbf7774429496c580641f766 -- <listed-paths-only>
git diff --name-only
```

If the local repository cannot resolve `e0af861c950750dddbf7774429496c580641f766`, stop and report a blocker with the exact command output.

## Out-of-scope drift to restore or explicitly prove absent

Inspect and restore only these paths if they differ because of the post-`e0af861c...` hotfix:

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

Do not restore or alter any other historical artifacts.

## Required fixes

### 1. Keep Goal 021 valid after scope repair

After restoring unrelated drift, run the Goal 021 GameProfiles service path/product smoke so `.llmgc/procedural/generated-game-profile-contract/**` is self-consistent with the current repository.

The final Goal 021 report must still contain:

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

### 2. Preserve physical Goal 020 evidence validation without regenerating Goal 020

Goal 021 must continue to physically validate committed compact Goal 020 report/manifest evidence, but this hotfix must not regenerate or rewrite the Goal 020 artifact family except for the exact restoration listed above.

The valid path must still reject missing Goal 020 evidence causally.

### 3. Cover required profile fields explicitly

If not already covered, add focused tests proving missing/empty fields reject through `GeneratedGameProfileContractAcceptanceService` validation:

- `targetExperience`
- `progressionScope`
- `contentScale.target`
- `assetPolicy.mode`
- `assetPolicy.fallbackPolicy`
- empty `selectedCapabilityIds`
- empty or incomplete `expectedDownstreamPipelineSlices`

Do not expand the public contract or add new profile fields.

### 4. Preserve future capability separation

Confirm gothic/trade future capabilities remain `future_required` and are not in `supportedCapabilityIds`:

```text
capability/dialogue_clue_graph_future
capability/vendor_economy_future
```

Unsupported/future topology must not be accepted as complete.

### 5. State and context docs

State docs must remain consistent:

- `docs/CURRENT_GENERATOR_STATE.json` `last_completed_product_slice_id = goal_021_generated_game_profile_contract_refresh`.
- `last_completed_product_slice` describes Goal 021, not Goal 020.
- Current active gate remains `generated_game_profile_contract_verification required`.
- `docs/CURRENT_GENERATOR_STATE.md` records Goal 020 accepted before Goal 021.
- `docs/CONTEXT_INDEX.md` current next work remains `generated_game_profile_contract_verification`.
- S178/Goal 022 are not started except queue/prohibition text.

## Required verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GameProfile|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-game-profile-contract
.\.devflow\scripts\check-all.ps1
```

Also directly inspect:

```text
.llmgc/procedural/generated-game-profile-contract/generated-game-profile-contract-report.json
.llmgc/procedural/generated-game-profile-contract/generated-game-profile-contract-pipeline-plan.json
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/CONTEXT_INDEX.md
```

And verify that no S178/Goal 022 files were created:

```powershell
Get-ChildItem docs -Recurse | Select-String -Pattern "goal_022|Goal 022|S178" -CaseSensitive
```

Only queue/prohibition/historical references are acceptable.

## Final response requirements

Report:

- exact changed files;
- exact restored out-of-scope files, if any;
- regenerated Goal 021 profile/pipeline/report hashes;
- whether final valid report has zero top-level error diagnostics;
- focused/product-smoke/check-all results;
- confirmation that `generated_game_profile_contract_verification` remains required, not passed;
- confirmation that S178/Goal 022 were not started;
- exact git commands used under the bounded exception, or confirmation that none were needed.
