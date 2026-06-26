# CODEX GOAL - Goal 008 Rule-Pack Gameplay Family Foundations

## Command

This file is intended to be run by Codex with:

```text
/goal
```

## Read First

Read in this order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/GOAL_008_RULE_PACK_GAMEPLAY_FAMILY_FOUNDATIONS.md`
6. Relevant Goal 007 implementation/tests only as an analog for deterministic acceptance, state docs and product smoke routing:
   - `src/LLMGameCreator.Application/Design/World/ConnectedWorldTravelAcceptanceService.cs`
   - `tests/LLMGameCreator.Tests/Application/World/ConnectedWorldTravelAcceptanceTests.cs`
   - `tests/LLMGameCreator.Tests/ProductSmoke/ConnectedWorldTravelSmokeTests.cs`
7. Existing runtime/domain/package definitions needed to execute inventory, equipment, recipe/crafting, transaction/trading, item-use and status/effect behavior.

Do not read historical apply READMEs, old task packs or broad roadmaps unless a concrete contract blocker requires it.

## Starting State

The user/assistant accepted the previous gate before this Goal:

```text
connected_world_travel_state_artifact_verification passed
```

Goal 008 may create and complete S071-S077.

Goal 008 must stop at exactly one final gate:

```text
rule_pack_gameplay_family_artifact_verification
```

Do not create S078, Goal 009 or any post-Goal-008 task.

## Primary Goal Document

Implement exactly:

```text
docs/GOAL_008_RULE_PACK_GAMEPLAY_FAMILY_FOUNDATIONS.md
```

## Allowed Files

Primary allowed files:

- `docs/GOAL_008_RULE_PACK_GAMEPLAY_FAMILY_FOUNDATIONS.md`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-008-rule-pack-gameplay-family-foundations-CODEX_GOAL.md`
- `src/LLMGameCreator.Application/Design/Gameplay/RulePackGameplayFamilyAcceptanceService.cs`
- `tests/LLMGameCreator.Tests/Application/Gameplay/RulePackGameplayFamilyAcceptanceTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/RulePackGameplayFamilySmokeTests.cs`
- `.devflow/scripts/run-product-smoke.ps1`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Conditionally allowed only when a focused failing test proves it necessary:

- Existing `src/LLMGameCreator.Application/Validation/*` files for small generic validation helpers.
- Existing `src/LLMGameCreator.Runtime/*` files for narrow runtime-owned state/command repairs around already-supported inventory, item use, equipment, recipes, transactions, flags/status/effects and serialization.
- Existing runtime test files for regression coverage of behavior touched by this goal.

Do not change any other file without first reporting a concrete blocker. Do not edit solution or project files.

## Forbidden Files And Work

Hard bans:

- No git commands.
- No branch, merge, push, rebase or cherry-pick instructions/actions.
- No WinForms/UI/Designer work.
- No Unity, Unity archive, Unity project, Windows build or media work.
- No LLM/RAG/provider/ComfyUI/Suno execution.
- No Lua execution, generator execution or generator-library edits.
- No `.sln` or `.csproj` edits.
- No public GamePackage schema redesign.
- No broad Runtime architecture replacement.
- No broad item/equipment/crafting/economy/combat/faction system.
- No genre/project/term-specific C# branches.
- No S078, Goal 009 or post-Goal-008 work.
- Do not use `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths in repo docs/tasks/reports. Use repository-relative paths or normal Windows/PowerShell paths.

## Required Implementation Shape

Add a deterministic headless acceptance service, preferably:

```text
src/LLMGameCreator.Application/Design/Gameplay/RulePackGameplayFamilyAcceptanceService.cs
```

It should build a compact proof pack or declaration set for the first gameplay family foundations:

- items/inventory/item-use;
- equipment/loadout;
- crafting/recipe/resource conversion;
- trading/transaction;
- status/effect/flag/duration or equivalent runtime-owned status evidence.

Then it must materialize exact package/runtime bindings using existing contracts and execute ordered runtime commands through existing runtime primitives or narrow runtime-owned command/state helpers.

The report must not accept copied ids plus a boolean. Acceptance must validate structured evidence.

## Required Runtime Proof

For valid scenarios, prove at minimum:

```text
start state
-> use/equip/craft/trade/status command sequence
-> inventory/equipment/status/flag deltas
-> completion/reward evidence
-> save/load exact restoration
```

The implementation must validate:

- selected declaration ids match package/runtime ids;
- every required package/runtime id exists;
- every command required by the scenario succeeded;
- every state delta is attributable to command execution;
- fake success without command/state deltas is rejected;
- save/load restored exact evidence;
- deterministic replay hash is stable.

## Required Negative Fixtures

Add tests/fixtures proving rejection of:

- missing item/recipe/effect refs;
- equipment slot mismatch;
- missing crafting inputs;
- insufficient trade cost;
- invalid status/effect binding;
- fake adapter/evidence success;
- save/load mismatch or missing restored evidence.

Do not accept invalid scenarios merely because `ExpectedValid = false`.

## Product Smoke

Update product smoke routing with scenario:

```text
rule-pack-gameplay-family-foundations
```

The product smoke test must deserialize JSON and assert structured fields, not only use `Assert.Contains` on raw text.

## Artifacts

Regenerate artifacts under:

```text
.llmgc/procedural/rule-pack-gameplay-family-foundations/
```

Expected files:

```text
rule-pack-gameplay-family-report.json
rule-pack-gameplay-family-report.md
rule-pack-gameplay-family-verification.md
```

Do not invent a parallel folder.

## State Docs

Update state docs to record:

```text
connected_world_travel_state_artifact_verification passed
goal_008_rule_pack_gameplay_family_foundations completed
```

Active next gate must be:

```text
rule_pack_gameplay_family_artifact_verification
```

Do not mark the Goal 008 gate passed.

Do not recommend or create Goal 009.

## Verification Commands

Run focused tests first:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~RulePackGameplayFamily|FullyQualifiedName~ConnectedWorldTravel|FullyQualifiedName~SemanticRuntimeComposition|FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Run product smoke:

```powershell
.\.devflow\scriptsun-product-smoke.ps1 -Scenario rule-pack-gameplay-family-foundations
```

Run full verification once at the end:

```powershell
.\.devflow\scripts\check-all.ps1
```

Also search changed files for mojibake markers and for accidental `S078` / `Goal 009` work. Report the result.

## Stop Conditions

Stop and report blocker instead of weakening acceptance if:

- a required gameplay family cannot be honestly executed with existing primitives or a narrow runtime-owned state helper;
- a public GamePackage schema change is necessary;
- runtime save/load cannot preserve the evidence;
- invalid scenarios only fail because of expectation metadata;
- full verification exposes an unrelated pre-existing failure;
- implementing this honestly requires UI, Unity, provider/media, arbitrary Lua, generator-library or project-file changes.

## Final Report

Report:

- changed files;
- slices S071-S077 completed;
- exact valid scenarios and gameplay families covered;
- exact invalid scenarios and diagnostic codes;
- package/rule binding audit results;
- runtime command sequence and state deltas;
- save/load evidence;
- deterministic replay evidence;
- regenerated artifact folder;
- focused/product-smoke/full verification results;
- confirmation no public GamePackage schema, UI, Unity, Lua, provider/media, generator-library, `.sln`/`.csproj`, S078/Goal009 or git commands were used.
