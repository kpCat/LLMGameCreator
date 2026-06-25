# Codex Goal 004 - Rule-Pack Driven Quest, Dialogue, And Interaction Families

## Command

Execute:

```text
docs/GOAL_004_RULE_PACK_DRIVEN_QUEST_DIALOG_INTERACTION_FAMILIES.md
```

## Mandatory User Confirmation

This goal may start only if the user prompt explicitly says:

```text
manual_extension_spine_verification passed
```

If the prompt does not include that confirmation, stop and ask for it.

## Hard Limits

- Complete only S049-S053.
- Do not create S054.
- Do not add Unity/media/provider/LLM execution.
- Do not add arbitrary Lua execution.
- Do not broaden Runtime Preview into a game/editor.
- Do not use git commands.
- Prefer data/rule-pack declarations over bespoke C# gameplay code.
- Add C# only for reusable contracts, validators, adapters, scenario harness, or narrow runtime API required by the family.

## Context Budget Rule

Read first:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/GOAL_004_RULE_PACK_DRIVEN_QUEST_DIALOG_INTERACTION_FAMILIES.md`

Read `docs/CURRENT_GENERATOR_STATE.md` only because S049 updates state.

Read broad strategy docs only if directly needed:

- `docs/ARCHITECTURE_STRATEGY_AND_BOUNDARIES.md`
- `docs/SEMANTIC_PACK_AND_RAG_STRATEGY.md`
- `docs/EXTENSION_RULE_PACK_CONTRACT_V1.md`
- `docs/AGENT_CONTEXT_BUDGET_POLICY.md`

Do not read old task packs or historical reports unless a test failure or local code reference requires it.

## Verification

Run focused tests per slice.

At the end:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario quest-dialog-interaction-families
.\.devflow\scripts\check-all.ps1
```

If scenario name must differ to fit local naming, use the nearest explicit `quest-dialog-interaction` route and report the actual command.

## Stop Condition

After S053:

- update state to `manual_quest_dialog_interaction_family_verification`;
- stop;
- provide handoff report;
- do not start the next goal.

