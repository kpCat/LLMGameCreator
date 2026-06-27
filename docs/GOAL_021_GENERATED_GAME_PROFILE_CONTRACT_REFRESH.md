# Goal 021: Generated Game Profile Contract Refresh

## Starting gate

This goal may start only after the user explicitly provides:

```text
minimum_playable_generated_game_verification passed
```

## Final gate

Stop at exactly one final gate:

```text
generated_game_profile_contract_verification
```

Leave this gate `required`, not `passed`.

Do not start S178 or Goal 022.

## Product outcome

Goal 020 produced the first minimum playable generated game review package. Goal 021 must turn that result into an explicit generated-game profile contract so future generation is no longer hardwired to one `frontier_survival` Alpha review scenario.

The concrete user-visible / generator-capability improvement must be:

```text
A deterministic profile contract and acceptance artifact that explains how user/game intent selects a game family, presentation mode, topology, actor model, core loops, content scale, asset policy and downstream Unity/runtime pipeline targets.
```

This is a generalization step, not a Unity polish step.

## Read first

Read these before editing code:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/FULL_GAME_GENERATION_MASTER_PLAN.md`
8. `docs/GAME_GENERATION_CAPABILITY_MATRIX.md`
9. `docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md`
10. `docs/GAME_FORM_FACTORS_AND_PRESENTATION_MODES.md`
11. `docs/GAME_SYSTEM_VARIANT_TAXONOMY.md`
12. `docs/ARCHITECTURE_STRATEGY_AND_BOUNDARIES.md`
13. nearest local analogs under `src/LLMGameCreator.Application/Design/` and `tests/LLMGameCreator.Tests/ProductSmoke/` from Goals 010-020.

## Scope

Allowed:

- New contract doc, preferably `docs/GAME_PROFILE_CONTRACT_V1.md`.
- New compact sample profile files under `samples/game-profiles/`.
- New Application-layer acceptance seam under `src/LLMGameCreator.Application/Design/GameProfiles/` or a similarly narrow namespace.
- New focused tests under `tests/LLMGameCreator.Tests/Application/GameProfiles/`.
- New product smoke under `tests/LLMGameCreator.Tests/ProductSmoke/`.
- Update `.devflow/scripts/run-product-smoke.ps1` with one scenario route.
- Update `docs/CURRENT_GENERATOR_STATE.json`, `docs/CURRENT_GENERATOR_STATE.md`, `docs/CONTEXT_INDEX.md`, and `docs/FULL_GENERATOR_GOAL_QUEUE.md`.
- Compact root artifacts under `.llmgc/procedural/generated-game-profile-contract/`.

Forbidden:

- Do not change public `GamePackage` schema.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms/Runtime Preview UI work.
- Do not run or modify Unity player/build entrypoints for this goal.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not edit generator-library.
- Do not create S178/Goal 022 work.
- Do not use git commands.

## Required slices

### S170: Record accepted Goal 020 gate and current position

Record `minimum_playable_generated_game_verification passed` as the starting evidence in Goal 021 artifacts and state docs.

Do not mark the new final gate passed.

### S171: Define generated game profile contract v1

Create a concise contract document that defines a generated game profile with at least these domains:

- profile id and display name;
- player intent / target experience summary;
- game family id;
- presentation mode;
- world topology;
- actor model;
- quest/dialogue/interaction loop family;
- inventory/item/economy loop family;
- combat/faction/social/work/theft capability flags where applicable;
- progression scope;
- content scale target;
- asset policy;
- runtime/export target;
- forbidden runtime dependencies;
- expected downstream pipeline slices.

This contract must be data/profile oriented. It must not grant runtime authority to LLM, Lua, providers, media systems or UI code.

### S172: Add compact profile samples

Add at least three compact deterministic sample profiles under `samples/game-profiles/`:

- `frontier-survival-minimum-alpha.game-profile.json`
- `gothic-mystery-investigation-alpha.game-profile.json`
- `trade-caravan-social-economy-alpha.game-profile.json`

Each profile must select a different family/flavor but still map onto existing generated-content, asset, Unity Alpha, quest-loop and multi-variant proof vocabulary.

### S173: Add Application-layer acceptance service

Add a deterministic acceptance service that:

- loads the sample profiles;
- validates the contract shape;
- derives a pipeline plan from each profile;
- maps profile choices to existing capability ids and downstream proof stages from Goals 010-020;
- records which choices are supported now, partially supported, or future-required;
- rejects invalid profiles through shared validation paths;
- writes compact artifacts.

Suggested artifact folder:

```text
.llmgc/procedural/generated-game-profile-contract/
```

Suggested files:

```text
generated-game-profile-contract-profiles.json
generated-game-profile-contract-pipeline-plan.json
generated-game-profile-contract-report.json
generated-game-profile-contract-report.md
generated-game-profile-contract-verification.md
```

The final report must have `accepted=false`, `manualGate=generated_game_profile_contract_verification`, and `previousAcceptedGate=minimum_playable_generated_game_verification passed`.

### S174: Profile-to-pipeline proof

For each valid profile, prove deterministic mapping into downstream pipeline requirements, including at minimum:

- required content generation pack / family;
- asset policy;
- Unity/export target;
- runtime loop requirements;
- quest completion requirement;
- readable presentation requirement;
- minimum playable review requirement;
- unsupported/future-required capability declarations.

The proof must not be a boolean-only mapping. It must include exact ids/keys for profile choices, capability ids, pipeline stage ids and current evidence gates.

### S175: Invalid/fake/leak matrix

Reject invalid scenarios causally. Minimum invalid matrix:

- missing profile id;
- duplicate profile ids;
- unknown game family;
- unknown presentation mode;
- incompatible presentation and topology;
- missing required loop family;
- unknown capability id;
- combat required but no combat/progression capability mapping;
- provider/media/LLM runtime dependency requested;
- arbitrary Lua runtime authority requested;
- public GamePackage schema mutation claim;
- Unity build claim in this non-Unity-build goal;
- missing accepted Goal 020 evidence;
- stale or mismatched previous gate;
- copied profile report without profile files;
- cross-family leakage where gothic profile maps to frontier package ids;
- unbounded content scale without budget;
- unsupported topology accepted as complete instead of future-required.

Invalid cases must mutate inputs or shared validation data, not just manually append diagnostics.

### S176: Product smoke and focused tests

Add focused tests for:

- deterministic output;
- three valid profiles;
- exact profile-to-pipeline mapping;
- future-required capabilities not falsely marked supported;
- invalid/fake/leak matrix;
- root artifacts written;
- state docs consistency.

Add product smoke route:

```text
generated-game-profile-contract
```

The product smoke summary must point to the repo-local compact root report when appropriate.

### S177: State handoff and final review

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

The current gate after this goal must be:

```text
generated_game_profile_contract_verification required
```

Do not mark it passed.

## Anti-false-positive review

Before final response, perform and report these checks:

- Root artifacts exist under `.llmgc/procedural/generated-game-profile-contract/`.
- `report.accepted=false`.
- `report.finalStatus` and `report.manualGate` equal `generated_game_profile_contract_verification`.
- `previousAcceptedGate` equals `minimum_playable_generated_game_verification passed`.
- Every valid profile maps to a deterministic pipeline plan with exact profile/capability/stage ids.
- Unsupported future capabilities are explicit and not treated as complete.
- Invalid/fake/leak scenarios reject through validation paths.
- Product smoke summary points to the expected report path.
- State docs hashes/counts match actual artifacts.
- No S178/Goal 022 markers except explicit prohibition/queue text.
- No local absolute paths, timestamps, GUID-like nondeterminism, temp/user paths in compact deterministic artifacts.
- Mojibake markers absent in changed text files.

## Required verification

Run, at minimum:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GameProfile|FullyQualifiedName~MinimumPlayableGame|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-game-profile-contract
.\.devflow\scripts\check-all.ps1
```

If `check-all.ps1` fails because of a real defect, fix within scope. If it fails because of an environmental blocker, stop and report the blocker with exact logs.

## Final response requirements

The final Codex response must include:

- changed files;
- artifact paths;
- selected valid profiles;
- generated profile/pipeline/report hashes;
- invalid/fake/leak matrix count;
- verification commands and results;
- confirmation that the final gate remains required, not passed;
- confirmation that S178/Goal 022 was not started;
- confirmation that no git commands were used.
