# Codex Goal 015 Wrapper: Unity Generated Scene Content Projection

## Command

Run this goal with:

```text
/goal docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-015-unity-generated-scene-content-projection-CODEX_GOAL.md
```

## Required User Gate Confirmation

Do not start unless the user message includes exactly:

```text
unity_playable_presentation_firewall_safe_build_verification passed
```

If the confirmation is missing, stop before editing and ask for that line.

## Primary Task File

Read and follow:

```text
docs/GOAL_015_UNITY_GENERATED_SCENE_CONTENT_PROJECTION.md
```

That file is the source of truth for allowed files, forbidden files, exact behavior, validation commands and final reporting.

## Mandatory Process Rules

Read first:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/GOAL_015_UNITY_GENERATED_SCENE_CONTENT_PROJECTION.md`

Then continue with the task-specific read list from the primary task file.

Do not use git commands.

Do not start S130.

Do not start Goal 016.

Do not mark `unity_generated_scene_content_projection_verification` as passed.

Do not edit `.sln`, `.csproj`, WinForms/UI, public GamePackage/runtime schema contracts, generator-library, provider/LLM/RAG/Lua/media execution code, or Unity package/project settings unless a real compile/build blocker proves it is required.

## Final Stop

Stop at:

```text
unity_generated_scene_content_projection_verification
```

The gate must remain:

```text
required
```

not:

```text
passed
```
