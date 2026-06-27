# Codex Task: Modular Contract Goal Policy Adoption

Start only after the prompt explicitly includes:

```text
rich_package_assembly_coverage_audit_verification passed
```

Then execute strictly:

```text
docs/PROCESS_TASK_MODULAR_CONTRACT_GOAL_POLICY_ADOPTION.md
```

Hard stop:

```text
modular_contract_goal_policy_adoption_verification required
```

Do not mark the gate passed.

Do not start Goal 025 or S199.

Do not implement package assembly expansion.

Do not change production code, GamePackage schema, runtime, Unity, WinForms UI, generator-library, provider/media/RAG/LLM/Lua integration or historical accepted artifacts.

## Required read-first order

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CURRENT_GENERATOR_STATE.json`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/CODEX_EXECUTION_DOCTRINE.md`
8. `docs/CODEX_PATCH_RULES.md`
9. `docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md`
10. `docs/GOAL_024_RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT.md`
11. `docs/RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT_V1.md`
12. proposed drafts if present:
    - `README.proposed.md`
    - `MODULAR_CONTRACT_GOAL_POLICY.proposed.md`
    - `LLMGameCreator_FEATURE_BACKLOG_AUDIT.proposed.md`

## Scope reminder

This is a bounded docs/process task. It exists to adopt modular contract goal policy, clean README source-of-truth routing, preserve wanted capabilities in backlog/campaign planning, and create a plan-only Package Assembly Campaign Pack.

It must reduce manual goal cycles.

Contract / Module / Integration / Proof phases are internal phases of bounded composite goals by default, not separate goals.

## Final response reminder

Final response must include changed files, proposed-doc promotion/adaptation notes, rejected/changed proposed content, acceptance evidence table, tests run/not run, active gate, next recommended work, Goal 025/S199 not started confirmation, no production code confirmation, no live runtime LLM/RAG path confirmation, and git usage confirmation.
