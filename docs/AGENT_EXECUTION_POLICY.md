# Agent Execution Policy for LLMGameCreator

## Git and branch policy

Every agent prompt must include:

```text
Work in the repository as it is currently checked out.
Do not create branches.
Do not switch branches.
Do not merge.
Do not rebase.
Do not cherry-pick.
Do not push.
Do not run git commands.
Branch management is handled manually by the user.
```

## Windows path policy

Every agent prompt must include:

```text
Use repository-relative paths.
Use PowerShell commands.
Do not use /mnt, /home/oai, sandbox:/..., C:\mnt, or any ChatGPT/container path.
Do not read or reference paths outside the repository unless explicitly listed.
```

This prevents agents from trying to read nonexistent Linux/sandbox paths on Windows.

## Read-first policy

Every task must include a limited `Read first` list. Agents must not scan the full repository unless explicitly told.

## Default forbidden areas

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Infrastructure/**
src/LLMGameCreator.WinForms/**
generator-library/**
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
```

## Executor choice

Kilo Code first:
- deterministic backend;
- metadata;
- JSON;
- materialization;
- validation;
- smoke tests;
- bounded refactors.

Codex first:
- risky architecture;
- complex cross-layer changes;
- Runtime;
- WinForms/UI;
- hard bug fixes;
- review/repair after Kilo.

## Common stop conditions

```text
Stop if Unity implementation becomes necessary.
Stop if Runtime/GamePackage schema/WinForms changes become necessary.
Stop if .sln/.csproj changes are required.
Stop if provider/LLM/Lua/generator execution becomes necessary.
Stop if more than N files need changes.
Stop if check-all fails after 2 repair attempts.
Stop if task expands into a new feature.
```

## Final report

```text
files read
files changed
what was implemented/fixed
what behavior was preserved
tests/checks run
confirmation of forbidden areas not touched
recommendation: merge / cleanup / reject
```
