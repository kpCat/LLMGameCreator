# LLMGameCreator AI Development Handoff

## Purpose

This document preserves project context, development preferences, agent workflow, and recent product-slice history so future ChatGPT/Codex/Kilo sessions do not lose continuity.

## Collaboration style

- User prefers Russian.
- User wants direct, practical, engineering-focused answers.
- Avoid vague advice. Provide concrete task specs, commands, file scopes, acceptance criteria and stop conditions.
- Do not propose tiny agent tasks by default: each run consumes input context. Prefer large but bounded macro-slices.
- User manually handles branch management and pushes. Agents must not run git commands.

## Repository

```text
https://github.com/kpCat/LLMGameCreator
```

Project goal:

```text
C# WinForms/.NET 8 editor for data-driven games.
LLM drafts JSON/Lua content, but does not generate C# runtime code.
Unity runtime/player is planned later and is separate.
```

Core layers:

```text
src/LLMGameCreator.Domain
src/LLMGameCreator.GamePackage
src/LLMGameCreator.Application
src/LLMGameCreator.Generation
src/LLMGameCreator.AssetPipeline
src/LLMGameCreator.Runtime
src/LLMGameCreator.WinForms
tests/LLMGameCreator.Tests
docs
generator-library
.devflow
```

## Permanent architectural constraints

Unless explicitly allowed:

```text
Do not change GamePackage schema.
Do not touch Runtime.
Do not touch WinForms/Designer.
Do not touch generator-library.
Do not change .sln or .csproj.
Do not call LLM/provider.
Do not call ComfyUI/Suno.
Do not execute Lua.
Do not execute generators.
Do not implement Unity runtime/player.
```

## Branch and git policy for all agent prompts

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

The user's environment is Windows.

```text
Use repository-relative paths.
Use PowerShell commands.
Do not use /mnt, /home/oai, sandbox:/..., C:\mnt, or any ChatGPT/container path.
Do not read or reference paths outside the repository unless explicitly listed.
```

## Agent selection policy

### Kilo Code first

Use Kilo Code for:

```text
deterministic backend slices
metadata models
JSON materialization
archive/index files
validation
smoke tests
small/medium refactors inside clear boundaries
docs/state updates
```

Kilo works well when:
- read-first file list is explicit;
- allowed/forbidden files are strict;
- non-goals are repeated;
- tests and stop conditions are clear.

### Codex first

Use Codex for:

```text
risky architecture decisions
Runtime logic
WinForms/UI/Designer-adjacent work
complex bug fixing
deep review/repair after Kilo
tasks with high cross-layer coupling
```

Codex consumes limited 5-hour budget, so use it where quality/reliability matters more than cost.

## Standard task spec shape

Every substantial agent task should include:

```text
Task type
Executor decision
Branch/git policy
Windows path policy
Goal
Current context
Read first
Allowed files
Forbidden files
Required behavior
Non-goals
Tests
Required checks
Stop conditions
Final report format
```

## Recent product slices

### Slice 019: Unity Archive Game Data Payload

Added metadata-only serialization of existing `GamePackageDefinition` into the Unity archive.

Important behavior:
- safe paths;
- UTF-8 without BOM;
- stable sorting;
- payload attached only when package is passed;
- focused and product smoke tests added.

### Slice 020: Unity Archive Asset/Audio/Lua Request Pipeline

Archive files:
```text
assets/asset-requests.json
assets/asset-request-index.json
audio/audio-requests.json
audio/audio-request-index.json
lua/module-requests.json
lua/modules-index.json
```

Cleanup/refactor:
- `BuildRequests` called once by materialization;
- readiness uses `BlockedByErrors`;
- future provider warnings aggregated by provider kind/category;
- duplicate/blank/unknown IDs tested;
- monolith service split into build context, asset/audio/Lua builders, diagnostics builder.

### Slice 021: Unity Archive Provider Job Plan

Archive files:
```text
production/fulfillment-plan.json
production/readiness-report.json
assets/asset-slots.json
audio/audio-slots.json
lua/module-slots.json
providers/manual-import/jobs.json
providers/comfyui/jobs.json
providers/suno/jobs.json
providers/local-audio/jobs.json
providers/procedural/jobs.json
```

Expected behavior:
- one slot per asset/audio/Lua request;
- provider `none` creates no executable provider job;
- all provider batches are `ExecutionEnabled=false`;
- jobs are `planned_not_executed`;
- expected output paths are safe archive-relative paths;
- no expected output file is physically created;
- product smoke `unity-archive-provider-job-plan`.

Known hardening finding before merge:
- provider job plan errors should affect final materialization readiness;
- request pipeline diagnostic codes should not become `request.request.diagnostic...` in materialization validation report.

## Planned next slice

### Slice 022: Provider Output Intake & Fulfillment State

Purpose:

```text
expected output paths
→ scanner
→ fulfillment state
```

Archive files:

```text
production/fulfillment-state.json
production/fulfilled-assets-index.json
production/fulfilled-audio-index.json
production/fulfilled-lua-index.json
production/invalid-outputs.json
```

Behavior:
- status enum: `missing`, `available`, `invalid`;
- missing expected file -> `missing`;
- existing safe non-empty file with correct extension -> `available`;
- unsafe path/wrong extension/empty file/directory -> `invalid`;
- materialization must not create expected outputs;
- scanner should detect manually created fake outputs after materialization;
- deterministic output;
- product smoke `unity-archive-fulfillment-state`.

Preferred executor: Kilo Code first. Codex for review/repair if needed.
