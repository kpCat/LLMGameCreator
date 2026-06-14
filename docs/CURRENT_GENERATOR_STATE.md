# Current Generator State

Status: source-of-truth handoff  
Updated by: Codex task  
State file pair: docs/CURRENT_GENERATOR_STATE.json

## Current phase

M4.1 real-model evaluation gate is active.

The project has a safe Capability Picker -> LLM Artifacts -> LLM Evaluation path, but broader contract expansion, Lua integration and rich package assembly are blocked until at least one real strict LLM evaluation report is reviewed.

## Last completed milestones

- M4.1: Strict LLM Generation Evaluation Pack.
- The M4.1 layer can evaluate the latest strict LLM generation audit without an LLM call or run a small explicit batch through the existing strict generation service.
- Evaluation stores JSON and markdown report artifacts with pass, repair, fail, diagnostic hot spot and quality warning metrics.

## Active manual gate

Run and review real strict LLM evaluation before expanding contracts, Lua or assembly.

## Current user action

Run a real local model through:

```text
Capability Picker
  -> LLM Artifacts
  -> LLM Evaluation
```

Then review the evaluation report for pass rate, repair recovery, repeated diagnostics and quality warnings.

## Allowed next Codex tasks

- Tighten prompt, repair or validator behavior based on a real evaluation report.
- Add a small evaluation report import/analyzer if the report is provided.
- Update docs and current state after the manual test.
- Add one carefully selected artifact contract only if evaluation is stable.
- Improve docs consistency guards around current-state onboarding.

## Blocked next Codex tasks

Blocked until at least one real strict LLM evaluation report is reviewed:

- M5 Lua module executor integration.
- M6 rich GamePackage assembly.
- Broad contract expansion.
- Runtime preview repair loop.

## Current generator workflow

```text
Capability Picker
  -> LLM Artifacts
  -> LLM Evaluation
  -> Artifact Review
  -> later assembly/export
```

## Where to start reading

1. AGENTS.md
2. docs/CONTEXT_INDEX.md
3. docs/CURRENT_GENERATOR_STATE.md
4. docs/ROADMAP_TO_FULL_GENERATOR.md
5. docs/GENERATOR_PLAN_CAPABILITY_SELECTION_PICKER.md
6. docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
7. docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
8. docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md

## What not to do next

- Do not skip the real-model evaluation gate.
- Do not move into M5 Lua module executor integration yet.
- Do not move into M6 rich GamePackage assembly yet.
- Do not expand artifact contracts broadly without evaluation evidence.
- Do not add runtime/provider/package mutation behavior as part of this gate.

## State update rule

Current state is manually updated by milestone tasks and automatically guarded by docs tests.

When a future Codex task completes a milestone or changes the recommended next step, update:

- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/CONTEXT_INDEX.md if new source docs were added
- docs/ROADMAP_TO_FULL_GENERATOR.md if milestone status or notes changed
