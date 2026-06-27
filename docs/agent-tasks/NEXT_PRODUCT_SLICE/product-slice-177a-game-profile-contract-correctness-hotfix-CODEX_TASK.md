# Product Slice 177A: Game Profile Contract Correctness Hotfix

Bounded hotfix for Goal 021 only. This is not a new `/goal`.

## Starting context

The previous gate has been accepted by the user:

```text
minimum_playable_generated_game_verification passed
```

Goal 021 produced `game_profile_v1` and stopped at:

```text
generated_game_profile_contract_verification required
```

Keep that same gate `required`, not `passed`.

## Problem found in external review

The pushed Goal 021 implementation has two correctness/handoff issues:

1. `docs/CURRENT_GENERATOR_STATE.json` has `last_completed_product_slice_id = goal_021_generated_game_profile_contract_refresh`, but the `last_completed_product_slice` object still describes Goal 020. The Markdown state also contains stale text implying Goal 020 is still required/not passed in a current handoff section.
2. Some Goal 021 invalid/fake/leak cases are synthetic diagnostics instead of flowing through shared validation paths. The valid profile contract also does not validate all required contract fields from `docs/GAME_PROFILE_CONTRACT_V1.md`, especially `targetExperience` and `progressionScope`, and it does not physically verify accepted Goal 020 compact evidence.

## Allowed files

You may edit only the Goal 021 artifact family and state handoff files:

- `src/LLMGameCreator.Application/Design/GameProfiles/GeneratedGameProfileContractAcceptanceService.cs`
- `tests/LLMGameCreator.Tests/Application/GameProfiles/GeneratedGameProfileContractAcceptanceTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/GeneratedGameProfileContractSmokeTests.cs` if needed
- `.llmgc/procedural/generated-game-profile-contract/**`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md` only if a route/status line is stale
- `docs/FULL_GENERATOR_GOAL_QUEUE.md` only if a route/status line is stale

Do not edit unrelated product slices.

## Forbidden

- Do not use git commands.
- Do not start S178 or Goal 022.
- Do not mark `generated_game_profile_contract_verification` passed.
- Do not change public `GamePackage` schema.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms/Runtime Preview UI work.
- Do not run or modify Unity build/player entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not edit generator-library.

## Required fixes

### 1. State handoff consistency

Update `docs/CURRENT_GENERATOR_STATE.json` so that:

- `last_completed_product_slice_id` remains `goal_021_generated_game_profile_contract_refresh`.
- `gate_status` remains `generated_game_profile_contract_verification`.
- `last_completed_product_slice` describes Goal 021, not Goal 020.
- recorded checks match the committed Goal 021 artifacts:
  - profile artifact hash `90f2ce0f20e3dcbad710a0a563c05b174bb9ad0cbb81d21736f037a725396d8e`
  - pipeline plan hash `4e5afdfe5248a8411a8f4055d2b3919b6a75c425e50815c92ffcf8f363cc41f8`
  - report hash `4254a461863081992e99a974a71e4a0b54f50358f3e8ac4a936bba536bd83efe`
  - valid profiles `3`
  - invalid scenarios `18`

Update `docs/CURRENT_GENERATOR_STATE.md` so the current/recommended handoff says:

- Goal 020 was accepted before Goal 021.
- Goal 021 produced the profile contract and stops at `generated_game_profile_contract_verification required`.
- S178/Goal 022 are not started.

Do not leave current handoff text that says Goal 020 is still required/not passed.

### 2. Validate all required profile contract fields

`GeneratedGameProfileContractAcceptanceService.ValidateProfile` must reject missing/empty required fields from `GAME_PROFILE_CONTRACT_V1.md`, including at minimum:

- `targetExperience`
- `progressionScope`
- `contentScale.target`
- `assetPolicy.mode`
- `assetPolicy.fallbackPolicy`
- `selectedCapabilityIds` non-empty
- `expectedDownstreamPipelineSlices` non-empty and containing required stage ids

Keep existing valid sample profiles valid.

### 3. Physically verify accepted Goal 020 evidence

The valid Goal 021 path must verify the repo-local compact Goal 020 evidence instead of relying only on a constant string.

Add a small evidence validator that reads the committed compact Goal 020 report/manifest under:

```text
.llmgc/procedural/minimum-playable-generated-game/
```

It must confirm at least:

- `minimum-playable-generated-game-report.json` exists.
- report `finalStatus` and `manualGate` equal `minimum_playable_generated_game_verification`.
- report `minimumPlayableGeneratedGameVerified == true`.
- report top-level diagnostics contain no severity `error`.
- manifest exists and its `manifestHash` matches the report `manifestHash` when both are present.

The final valid report must keep:

```text
previousAcceptedGate = minimum_playable_generated_game_verification passed
contractProofPassed = true
accepted = false
manualGate = generated_game_profile_contract_verification
```

### 4. Make invalid/fake/leak scenarios causal

Do not leave key invalid cases as manually appended diagnostics when they can be proven through shared validation.

At minimum add or repair tests and implementation so these cases flow through real validation helpers:

- `missing_accepted_goal020_evidence`: caused by missing or invalid Goal 020 compact evidence.
- `stale_or_mismatched_previous_gate`: caused by mismatched previous gate option/evidence.
- `duplicate_profile_ids`: caused by a duplicated profile id collection passed through profile-set validation.
- `missing_target_experience`: caused by a profile with empty `targetExperience`.
- `missing_progression_scope`: caused by a profile with empty `progressionScope`.
- `unsupported_topology_accepted_as_complete`: caused by a pipeline/evidence mutation that treats future-required topology as complete instead of explicit future-required.

The invalid matrix may still include compact diagnostics, but tests must prove these diagnostics come from the shared profile/evidence/plan validators.

### 5. Focused regression tests

Add or update tests so they prove:

- Valid final Goal 021 report has no severity `error` diagnostics.
- Valid final report has `contractProofPassed=true`.
- Valid final report physically depends on accepted Goal 020 report/manifest evidence.
- Missing Goal 020 compact evidence rejects causally.
- Empty `targetExperience` rejects.
- Empty `progressionScope` rejects.
- Duplicate profile ids reject through shared profile-set validation.
- Unsupported/future-required capabilities are not marked supported.
- State docs `last_completed_product_slice` matches `goal_021_generated_game_profile_contract_refresh`.

## Regenerate artifacts

Regenerate compact root artifacts under:

```text
.llmgc/procedural/generated-game-profile-contract/
```

Do not regenerate unrelated artifacts unless required by the existing product smoke path.

If any Goal 021 hashes change, update `CURRENT_GENERATOR_STATE.json` and `.md` to match the regenerated artifacts.

## Required verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GameProfile|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-game-profile-contract
.\.devflow\scripts\check-all.ps1
```

Also directly inspect:

- `.llmgc/procedural/generated-game-profile-contract/generated-game-profile-contract-report.json`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`

## Final response requirements

Report:

- changed files;
- regenerated artifact hashes;
- focused/product-smoke/check-all results;
- whether final valid report has zero top-level error diagnostics;
- confirmation that `generated_game_profile_contract_verification` remains required, not passed;
- confirmation that S178/Goal 022 were not started;
- confirmation that no git commands were used.
