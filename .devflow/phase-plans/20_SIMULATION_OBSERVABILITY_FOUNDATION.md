# 20_SIMULATION_OBSERVABILITY_FOUNDATION.md — simulation and observability foundation

Read when user approves simulation/corpus or diagnostics bundle work.

## Phase goal

Reduce reliance on real model calls by making generation pipeline behavior reproducible through fake clients, raw-output corpus, fixtures, deterministic simulation and diagnostics bundles.

## TASK SIM-001 — Raw-output corpus simulation harness

Status: proposal, requires user start/approval.

Objective: add fixture-driven tests for common LLM raw output shapes.

Allowed before M4.1 gate: yes.
Requires approval: yes.

Source docs:

```text
.devflow/MODELING_STRATEGY.md
.devflow/VERIFICATION_MATRIX.md
.devflow/CODE_QUALITY_AND_STYLE.md
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/VALIDATION_STRATEGY.md
```

Target areas:

```text
tests/LLMGameCreator.Tests/
tests/fixtures/llm_raw_outputs/
```

Non-goals:

```text
- do not change production parser unless a failing fixture proves the issue;
- do not call real LLM;
- do not expand artifact contracts;
- do not change GamePackage schema.
```

Required fixture families:

```text
fenced_json
text_before_json
text_after_json
two_json_objects
broken_trailing_comma
invalid_escape
wrong_root
wrong_contract_id
missing_required_field
id_drift
placeholder_text
```

Required checks:

```text
dotnet build
dotnet test
raw-output fixture tests
check-all
```

Stop on:

```text
requires_production_refactor
requires_more_than_8_files
missing_existing_parser_entrypoint
```

Next candidate: M4HARDEN-001 if fixtures expose real failures; otherwise OBS-001 if user approves diagnostics bundle.

## TASK OBS-001 — Generation diagnostics bundle foundation

Status: proposal, requires user start/approval.

Objective: add a bounded diagnostics bundle concept around strict generation/evaluation: session summary, events JSONL, raw/cleaned outputs, validation issues, redacted settings, exportable zip/report.

Allowed before M4.1 gate: yes.
Requires approval: yes.

Source docs:

```text
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/VALIDATION_STRATEGY.md
docs/DEVELOPMENT_RULES.md
.devflow/MODELING_STRATEGY.md
.devflow/CODE_QUALITY_AND_STYLE.md
```

Target areas:

```text
src/LLMGameCreator.Application/
src/LLMGameCreator.Infrastructure/
src/LLMGameCreator.WinForms/Pages/ only if UI export button is explicitly included
tests/LLMGameCreator.Tests/
```

Non-goals:

```text
- no external logging dependency without approval;
- no secrets/API keys in bundle;
- no GamePackage mutation;
- no provider calls;
- no large logging framework.
```

Required event fields:

```text
timestampUtc
level
eventType
sessionId
jobId
contractId
artifactId
phase
durationMs
diagnosticCode
message
```

Required checks:

```text
dotnet build
dotnet test
diagnostics bundle creation test
redaction/no secrets check
check-all
```

Stop on:

```text
needs_new_dependency
needs_db_schema_change
requires_more_than_10_files
secrets_redaction_unclear
```
