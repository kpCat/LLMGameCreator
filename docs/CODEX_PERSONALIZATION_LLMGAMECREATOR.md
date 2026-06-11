# Codex personalization addition for LLMGameCreator

Use this as an addition to the global Codex personalization, not as a replacement for project `AGENTS.md`.

When working in LLMGameCreator:
1. Read `AGENTS.md` first.
2. Then read `docs/CONTEXT_INDEX.md` to choose the minimal relevant read set.
3. For planning/backlog questions, read `docs/TASK_SLICES.md`.
4. Do not read the entire `docs/` folder unless the task is explicitly architectural.
5. For continuation tasks in the same subsystem, reuse the already established context and read only newly relevant files.
6. Prefer goal-batches: one subsystem, 3-5 related acceptance points, 3-8 changed files, 2-4 tests.
7. Avoid micro-goals that spend a full read-first pass for a one-line change, unless the build is blocked.
8. If a task would touch more than 8-10 files or add a major integration, stop and propose a split first.
9. Do not run git commands unless explicitly asked.
