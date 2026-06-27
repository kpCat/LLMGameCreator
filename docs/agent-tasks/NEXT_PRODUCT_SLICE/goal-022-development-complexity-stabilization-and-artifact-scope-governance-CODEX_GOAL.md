# Codex Goal 022: Development Complexity Stabilization And Artifact Scope Governance

Start only after the prompt explicitly includes:

```text
generated_game_profile_contract_verification passed
```

Then execute strictly:

```text
docs/GOAL_022_DEVELOPMENT_COMPLEXITY_STABILIZATION_AND_ARTIFACT_SCOPE_GOVERNANCE.md
```

Hard stop:

```text
development_complexity_stabilization_verification required
```

Do not mark the gate passed.

Do not start Capability Bundle Selection To Pipeline Inputs, Goal 023, or S185.

## Required read-first order

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_022_DEVELOPMENT_COMPLEXITY_STABILIZATION_AND_ARTIFACT_SCOPE_GOVERNANCE.md`
8. `.devflow/scripts/check-all.ps1`
9. `.devflow/scripts/run-product-smoke.ps1`
10. local product-smoke/test analogs that write `.llmgc/procedural/**`.

## Scope reminder

This goal is a stabilization/process-governance goal requested by the user after Goal 021 scope drift. It must reduce future development complexity by adding artifact-scope policy, guard automation, check-all artifact isolation, and compact stabilization evidence.

It must not add gameplay capability selection, Unity polish/build work, WinForms UI, public schema changes, provider/LLM/RAG/Lua/media execution, generator-library edits, `.sln` or `.csproj` changes.

Bounded git inspection is allowed only as specified in the goal document. No commit/push/branch/reset/history-changing commands.

## Final response reminder

Report changed files, policy/script/config paths, compact artifacts, hashes, tests/smoke/check-all/scope-guard results, final gate status, bounded git command usage, and confirmation that Capability Bundle Selection / Goal 023 was not started.
