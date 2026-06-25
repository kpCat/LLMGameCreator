# Agent Context Budget Policy

Purpose: keep Codex feature slices read-first without spending context on stale planning packs.

## Default Reading Order

For current generator/product-slice work, start with:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. the user-selected goal or slice file

Read `docs/CURRENT_GENERATOR_STATE.md` when the task updates state or manual gates.

## Expand Only When Needed

Read broad strategy docs only when the selected goal explicitly requires them or when a local code/test failure needs architectural clarification:

- `docs/ARCHITECTURE_STRATEGY_AND_BOUNDARIES.md`
- `docs/SEMANTIC_PACK_AND_RAG_STRATEGY.md`
- `docs/EXTENSION_RULE_PACK_CONTRACT_V1.md`
- `docs/ROADMAP_TO_FULL_GENERATOR.md`

Do not read old task packs, archived prompts or historical reports as planning authority unless a test failure or a direct code reference requires them.

## Hard Boundaries

- Do not use git commands unless the user explicitly asks.
- Do not start Unity, media providers, LLM providers or arbitrary Lua execution during bounded Codex slices.
- Stop at the manual gate named by the current goal.
- Prefer existing Application services, validators, scenario harnesses and smoke routes before adding new architecture.
