# Codex Goal - Runtime-Backed Generated Microgame State

## Execute this bounded goal

Primary goal file:

`docs/GOAL_002_RUNTIME_BACKED_GENERATED_MICROGAME_STATE.md`

## Rules

You may complete at most five product slices:

- S038: `docs/NEXT_PRODUCT_SLICE_038_RUNTIME_OWNED_GOAL_PROGRESS_TASK.md`
- S039: `docs/NEXT_PRODUCT_SLICE_039_RUNTIME_REWARD_CHALLENGE_STATE_TASK.md`
- S040: `docs/NEXT_PRODUCT_SLICE_040_RUNTIME_MICROGAME_STATE_ACCEPTANCE_TASK.md`
- S041: `docs/NEXT_PRODUCT_SLICE_041_GENERATION_PRESETS_AND_OPTIONS_TASK.md`
- S042: `docs/NEXT_PRODUCT_SLICE_042_MICROGAME_VARIATION_ACCEPTANCE_TASK.md`

Stop after S042 even if more work is possible.

Stop earlier if:

- a blocker requires broad runtime/schema/UI redesign;
- Unity/Lua/provider/media/LLM execution would be required;
- tests/smoke/check-all cannot be made green in a focused way;
- manual verification is required before continuing.

## First gate

Record user manual Goal 001 verification as passed before S038:

- active goal visible;
- progress reaches completion;
- challenge/reward/completion visible;
- movement and interactions work.

Update current state and run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Also fix any broken current-state references to S035 task files.

## Verification

After each slice run that slice's tests, product smoke and `check-all.ps1`.

Do not report the goal complete without final S040 acceptance evidence or explicit blocker report.

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
- what is runtime-backed vs fallback/projection-backed;
- verification results per slice;
- product smoke scenarios added;
- user manual verification instructions;
- whether the goal reached `manual_configurable_microgame_verification` or stopped earlier.

