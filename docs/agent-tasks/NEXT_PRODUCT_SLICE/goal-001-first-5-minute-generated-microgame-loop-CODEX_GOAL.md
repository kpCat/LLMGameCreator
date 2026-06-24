# Codex Goal - First 5-Minute Generated Microgame Loop

## Execute this bounded goal

Primary goal file:

`docs/GOAL_001_FIRST_5_MINUTE_GENERATED_MICROGAME_LOOP.md`

## Rules

You may complete at most three product slices:

- S035: `docs/NEXT_PRODUCT_SLICE_035_ACTIVE_GOAL_QUEST_PROGRESS_TASK.md`
- S036: `docs/NEXT_PRODUCT_SLICE_036_ENCOUNTER_REWARD_COMPLETION_TASK.md`
- S037: `docs/NEXT_PRODUCT_SLICE_037_MICROGAME_ACCEPTANCE_POLISH_TASK.md`

Stop after S037 even if more work is possible.

Stop earlier if:

- a blocker requires broad runtime/schema/UI redesign;
- Unity/Lua/provider/media/LLM execution would be required;
- tests/smoke/check-all cannot be made green in a focused way;
- manual verification is required before continuing.

## First gate

Record user manual S034 one-click verification as passed before S035:

- Generate Preview works after hotfix.
- Generated package loads automatically.
- Start works.
- Movement and interactions work.

Update current state and run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

## Verification

After each slice run that slice's tests, product smoke and `check-all.ps1`.

Do not report the goal complete without final S037 acceptance evidence or explicit blocker report.

## Forbidden

Do not add:

- LLM/provider execution;
- Lua execution;
- Unity work;
- media generation;
- broad GamePackage schema redesign;
- broad runtime command/state redesign;
- large UI rewrite.

## Final report

Report:

- which slices were completed;
- verification results per slice;
- product smoke scenarios added;
- user manual verification instructions;
- whether the goal reached `manual_microgame_loop_verification` or stopped earlier.

