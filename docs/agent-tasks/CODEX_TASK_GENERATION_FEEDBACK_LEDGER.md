# Codex Task — Generation Feedback Ledger

## Task ID

`generation-feedback-ledger-v1`

## Goal

Add a candidate-owned Generation Feedback Ledger that records generation runs, candidates, metrics, diagnostics, review decisions and feedback signals.

This task must not tune packs yet. It only records evidence.

## Read first

- `docs/proposals/ADAPTIVE_GENERATOR_FEEDBACK_LOOP.md`
- `docs/proposals/GENERATION_QUALITY_SCORING.md`
- Existing candidate module patterns.
- Existing validation/report models.

## Allowed scope

Prefer:

```text
src/LLMGameCreator.Application/Design/AdaptiveGeneration/**
tests/LLMGameCreator.Tests/Application/AdaptiveGeneration/**
docs/candidates/adaptive-generation/**
```

## Forbidden scope

Do not:

- change GamePackage schema;
- change Unity/player;
- call LLM/providers;
- implement automatic tuning;
- add UI;
- modify `.sln` / `.csproj` unless unavoidable;
- add git instructions.

## Required models

- `GenerationRunRecord`
- `GenerationCandidateRecord`
- `GenerationFeedbackRecord`
- `GenerationMetricSet`
- `GenerationDecision`
- `GenerationFeedbackLedger`
- `GenerationFeedbackQuery`
- `GenerationFeedbackReport`
- `GenerationFeedbackDiagnostic`

## Required behavior

- record run metadata;
- record candidate metadata;
- record score/metric sets;
- record diagnostics;
- record review decisions;
- query feedback by target pack/recipe/domain/object kind;
- generate deterministic report.

## Tests

- can record a run with candidates;
- same input produces stable report;
- invalid missing run id produces diagnostic;
- can query by target recipe;
- can query rejected candidates;
- can query positive signals;
- ledger does not mutate packs.

## Stop conditions

Stop if implementation requires schema/runtime/UI changes.

## Final report

Report files read/changed, models, tests, validation commands and limitations.
