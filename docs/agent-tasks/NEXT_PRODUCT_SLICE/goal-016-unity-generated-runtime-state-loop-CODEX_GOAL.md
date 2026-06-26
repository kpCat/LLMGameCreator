# Codex Goal 016 Wrapper: Unity Generated Runtime State Loop

## Command

Run this goal with:

```text
/goal docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-016-unity-generated-runtime-state-loop-CODEX_GOAL.md
```

## Required User Gate Confirmation

Do not start unless the user message includes exactly:

```text
unity_generated_scene_content_projection_verification passed
```

If the confirmation is missing, stop before editing and ask for that line.

## Primary Task File

Read and follow:

```text
docs/GOAL_016_UNITY_GENERATED_RUNTIME_STATE_LOOP.md
```

That file is the source of truth for allowed files, forbidden files, exact behavior, validation commands, anti-false-positive review and final reporting.

## Mandatory Process Rules

Read first:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_016_UNITY_GENERATED_RUNTIME_STATE_LOOP.md`

Then continue with the task-specific read list from the primary task file.

Do not use git commands.

Do not start S138.

Do not start Goal 017.

Do not mark `unity_generated_runtime_state_loop_verification` as passed.

Do not edit `.sln`, `.csproj`, WinForms/UI, public GamePackage/runtime schema contracts, generator-library, provider/LLM/RAG/Lua/media execution code, or Unity package/project settings.

## Final Stop

Stop at:

```text
unity_generated_runtime_state_loop_verification
```

The gate must remain:

```text
required
```

not:

```text
passed
```
