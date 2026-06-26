# CODEX GOAL - Goal 009 Rule-Pack Combat, Faction, Social, Work And Theft Foundations

## Command

Run this file with:

```text
/goal
```

## Read First

Read in this order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/GOAL_009_RULE_PACK_COMBAT_FACTION_SOCIAL_WORK_THEFT.md`
6. Accepted S077A implementation/tests only as the runtime-adapter, binding-audit, full-state save/load and smoke analog.
7. Existing runtime/domain/package files directly required for encounter, faction, dialogue, interaction and container commands.

Do not read historical apply packs, old task prompts or broad roadmaps unless a concrete blocker requires it.

## Starting Evidence

Start only because the user prompt explicitly provides:

```text
rule_pack_gameplay_family_artifact_verification passed
```

Goal 009 may create S078-S084 and must stop at:

```text
rule_pack_combat_faction_social_work_theft_artifact_verification
```

Do not create S085, Goal 010 or post-Goal-009 work.

## Execute

Implement exactly:

```text
docs/GOAL_009_RULE_PACK_COMBAT_FACTION_SOCIAL_WORK_THEFT.md
```

## Allowed Files

Primary allowed files:

- `docs/GOAL_009_RULE_PACK_COMBAT_FACTION_SOCIAL_WORK_THEFT.md`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-009-rule-pack-combat-faction-social-work-theft-CODEX_GOAL.md`
- `src/LLMGameCreator.Application/Design/Gameplay/RulePackCombatFactionSocialWorkTheftAcceptanceService.cs`
- `tests/LLMGameCreator.Tests/Application/Gameplay/RulePackCombatFactionSocialWorkTheftAcceptanceTests.cs`
- `tests/LLMGameCreator.Tests/Application/Gameplay/RulePackCombatFactionSocialWorkTheftRealRuntimeAdapter.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/RulePackCombatFactionSocialWorkTheftSmokeTests.cs`
- `.devflow/scripts/run-product-smoke.ps1`
- `.llmgc/procedural/rule-pack-combat-faction-social-work-theft/*`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Conditionally allowed only after a focused test proves an existing defect:

- the narrow existing Runtime service containing that defect;
- its focused runtime regression test.

Do not edit any other file without reporting a blocker first. Do not edit `.sln` or `.csproj`.

## Non-Negotiable Execution Shape

- Application default runtime adapter is unavailable/fail-closed.
- Test/product-smoke adapter constructs and invokes real production runtime services.
- No duplicate Application combat/social/work/theft executor.
- Full `GameRuntimeState` save/load uses existing runtime serializer/snapshot seams.
- Binding audit is scenario-exact.
- `ActualValid` never derives from `ExpectedValid` or scenario name.
- Fake success and cross-scenario leakage are rejected structurally.
- Work/theft remain explicit combinations of existing primitives and are not overclaimed as full subsystems.

## Verification

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~RulePackCombatFactionSocialWorkTheft|FullyQualifiedName~RulePackGameplayFamily|FullyQualifiedName~ConnectedWorldTravel|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario rule-pack-combat-faction-social-work-theft
.\.devflow\scripts\check-all.ps1
```

Also scan implementation, tests, state and generated artifacts for mojibake and exact `S085|Goal 010|goal_010` markers. Exclude the Goal/task documents whose prohibition text necessarily contains those markers.

## Stop Conditions

Stop rather than weaken acceptance if real runtime execution, full-state save/load or exact binding needs a public schema/command/state/project-reference change or a second Application simulator.

## Hard Bans

- No git commands.
- No branch/merge/push/rebase/cherry-pick instructions.
- No S085 or Goal 010.
- No WinForms/UI, Unity, Lua, LLM/RAG, provider/media or generator work.
- No generator-library, `.sln` or `.csproj` edits.
- No public GamePackage/runtime schema redesign.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report

Report all items required by the primary Goal document, then stop at the single final gate. Do not mark it passed.
