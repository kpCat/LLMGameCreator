# Generator Plan Strict LLM Evaluation

Status: M4.1 implementation guide  
Scope: strict LLM generation quality evaluation, metrics, diagnostics, quality heuristics, persisted report and WinForms visibility  
Non-scope: Lua execution, runtime preview, GamePackage mutation, package export, DB schema changes, media generation, provider calls outside explicit user-triggered batch evaluation

## Purpose

M4 added a safe strict LLM entrypoint. M4.1 measures whether real model output is stable enough before expanding contracts or Lua generation.

The workflow is:

```text
Capability Selection
  -> Strict LLM artifact generation run(s)
  -> parse / validate / repair attempts
  -> evaluation metrics
  -> repeated error patterns
  -> contract pass / repair / fail rates
  -> content quality heuristics
  -> saved evaluation artifact
  -> markdown report
  -> WinForms Evaluation UI
```

This is a quality gate, not a package generation feature.

## Boundaries

Preserved boundaries:

- LLM produces draft artifacts only.
- C# owns parsing, validation, evaluation, metrics, storage and reports.
- Lua is not executed.
- Runtime does not call LLM.
- `GamePackage` is not mutated.
- Package export is not performed.
- Artifact Review remains the human approval gate.

## Modes

### Evaluate latest audit

This mode does not call an LLM. It reads:

```text
artifact/generator_plan_strict_llm_artifact_generation/latest
```

The evaluator computes metrics and quality warnings from the saved strict LLM generation audit, then saves a new evaluation artifact and markdown report. This mode is safe without a configured model.

### Run evaluation batch

This mode is user-triggered only. It reads the latest capability selection and calls the existing strict LLM generation service for selected contracts and iterations.

Defaults:

```text
contracts: game_profile_v1, scene_pack_v1, quest_pack_v1, mechanics_pack_v1
iterations per contract: 1
repair: enabled
stage_for_review: false
```

Limits:

```text
1..10 iterations per contract
1..4 contracts
expected max LLM calls = contracts * iterations * (1 + maxRepairAttempts when repair is enabled)
```

The batch evaluator does not call `ILlmChatClient` directly. It reuses `GeneratorPlanStrictLlmArtifactGenerationService` so strict parsing, validation, repair and optional pending review staging remain in one place.

## Metrics

The summary includes:

- requested contract count;
- generation run count;
- attempt count;
- initial pass count;
- repair pass count;
- failed count;
- valid artifact count;
- staged-for-review count;
- strict JSON/parser/validator error counts;
- overall pass rate;
- repair recovery rate.

Per-contract summaries include runs, initial passes, repair passes, failures, valid artifacts, average attempts and top diagnostic codes.

Diagnostic hot spots group by severity, code, contract and target.

## Quality Heuristics

Quality heuristics are deterministic warnings, not hard validation failures:

- `generic_text_warning`: title/name/description contains placeholder-like text such as `...`, `TBD`, `test`, `sample` or `example`.
- `short_description_warning`: description is shorter than 20 characters.
- `empty_tags_warning`: mechanics tags are missing or empty.
- `missing_source_context_warning`: `source_context` is missing or empty.
- `variant_mismatch_warning`: `game_profile_v1` selected variant fields differ from latest capability selection ids.
- `repeated_title_warning`: the same title appears across multiple samples for the same contract.

Warnings should guide prompt, repair or validator tightening. They do not block storage.

## Persistence

Evaluation JSON:

```text
id: artifact/generator_plan_strict_llm_evaluation/latest
kind: generator_plan.strict_llm_evaluation
path: .llmgc/generator-plans/generator_plan_strict_llm_evaluation.json
```

Markdown report:

```text
id: artifact/generator_plan_strict_llm_evaluation/report/latest
kind: generator_plan.strict_llm_evaluation.report
path: .llmgc/generator-plans/generator_plan_strict_llm_evaluation_report.md
```

Evaluation diagnostics are also saved as validation rows for the evaluation artifact. No Design DB schema change is required.

## UI

The WinForms page is `LLM Evaluation`.

Available actions:

- load latest strict LLM generation audit;
- evaluate latest audit without an LLM call;
- run an explicit batch evaluation;
- load latest evaluation;
- copy markdown report;
- copy evaluation JSON.

The page shows expected maximum LLM calls before batch runs. It does not call an LLM on startup or page activation.

## Interpreting Results

Use pass rate to decide whether a contract is stable enough for expansion. Use repair recovery rate to decide whether repair prompts are effective. Use diagnostic hot spots to identify prompt, parser, validator or contract weaknesses.

Suggested decisions:

- high JSON wrapper/fence failures: tighten strict output prompt;
- repeated missing fields or id drift: add repair guidance and validator examples;
- low pass rate: do not expand contracts yet;
- high pass rate with no failures: contract looks stable enough for the sampled profile;
- quality warnings: improve prompt specificity before promotion.

## Tests

Tests must use fake `ILlmChatClient` implementations. No real HTTP, local model, external provider, Lua execution, package export or runtime preview should run in evaluation tests.
