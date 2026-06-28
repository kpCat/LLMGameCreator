# Parallel Candidate Development Policy

Status: Goal 029 policy  
Final gate: `modular_generator_kernel_parallel_readiness_verification`

## Purpose

Parallel Codex work is allowed only as candidate work until it is adopted
serially. Goal 029 creates the first manifest/registry proof needed for that
workflow; it does not authorize auto-merge, live execution surfaces or accepted
gate claims from candidates.

## Rules

- Exactly one active task writes state and routing docs.
- Parallel Codex work must be candidate work unless the user explicitly starts a
  serial adoption task.
- Candidate tasks must not change:
  - `docs/CURRENT_GENERATOR_STATE.json`;
  - `docs/CURRENT_GENERATOR_STATE.md`;
  - `docs/CONTEXT_INDEX.md`;
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`.
- Candidate tasks must not claim accepted gates.
- Candidates must own source roots and artifact roots through module manifests.
- Serial adoption updates state once after compatibility proof is rerun.

## Conflict Resolution Order

1. Rebase candidate onto accepted main.
2. Rerun module compatibility matrix.
3. Accept or reject candidate in serial adoption.
4. Never auto-merge contradictory module manifests.

## Verification Tiers

Tier 1: module proof

- focused module tests;
- product smoke scenario manifest test;
- module compatibility matrix;
- artifact scope guard.

Tier 2: kernel proof

- all module/smoke manifests parse;
- registry tests;
- compatibility matrix;
- selected smoke set;
- ordinary tests when kernel/shared files changed.

Tier 3: campaign proof

- `check-all.ps1`;
- selected cross-module smokes;
- used after several modules or before adoption.

Tier 4: product vertical proof

- rare playable, simulatable or runtime-facing gate.

`check-all.ps1` remains required for Goal 029 because kernel/devflow behavior is
changed. Future module-only goals may use Tier 1 only after this gate is
accepted.
