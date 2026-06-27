# Codex Goal 023: Capability Bundle Selection To Pipeline Inputs

Start only after the prompt explicitly includes:

```text
development_complexity_stabilization_verification passed
```

Then execute strictly:

```text
docs/GOAL_023_CAPABILITY_BUNDLE_SELECTION_TO_PIPELINE_INPUTS.md
```

Hard stop:

```text
capability_bundle_pipeline_inputs_verification required
```

Do not mark the gate passed.

Do not start Goal 024 or S192.

## Required read-first order

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md`
8. `.devflow/artifact-scope/artifact-scope-policy.json`
9. `docs/GOAL_023_CAPABILITY_BUNDLE_SELECTION_TO_PIPELINE_INPUTS.md`
10. `docs/GAME_PROFILE_CONTRACT_V1.md`
11. existing GeneratorPlan capability selection service/tests and atlas files named by the goal.

## Scope reminder

This goal turns accepted `game_profile_v1` files into deterministic capability-bundle selections and concrete generation pipeline input records.

It must not add Unity polish, WinForms UI, public GamePackage schema changes, generator-library edits, provider/LLM/RAG/Lua/media execution, package assembly expansion, `.sln` or `.csproj` changes.

The final verification must include the Goal 022 artifact-scope guard with the Goal 023 allowlist.

## Final response reminder

Report changed files, artifacts, hashes, selected profiles/bundles/gaps, tests/smoke/check-all/scope-guard results, final gate status, bounded git usage and confirmation that Goal 024/S192 was not started.
