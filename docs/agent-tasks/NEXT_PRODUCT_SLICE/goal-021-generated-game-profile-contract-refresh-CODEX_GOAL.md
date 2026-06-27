# Codex Goal 021: Generated Game Profile Contract Refresh

Start only after the prompt explicitly includes:

```text
minimum_playable_generated_game_verification passed
```

Then execute strictly:

```text
docs/GOAL_021_GENERATED_GAME_PROFILE_CONTRACT_REFRESH.md
```

Hard stop:

```text
generated_game_profile_contract_verification required
```

Do not mark the gate passed.

Do not start S178 or Goal 022.

Do not use git commands.

## Required read-first order

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_021_GENERATED_GAME_PROFILE_CONTRACT_REFRESH.md`
8. the local analog files needed by the goal.

## Scope reminder

This goal is a contract/profile generalization step after the minimum playable generated game gate. It must not add Unity build/play polish, WinForms UI, public schema changes, provider/LLM/RAG/Lua/media execution, generator-library edits, `.sln` or `.csproj` changes.

## Final response reminder

Report changed files, compact artifacts, hashes, tests/smoke/check-all results, final gate status, no-git confirmation, and no S178/Goal 022 confirmation.
