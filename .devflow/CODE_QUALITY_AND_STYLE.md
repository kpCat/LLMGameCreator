# CODE_QUALITY_AND_STYLE.md — критерии качества кода для локального агента

Цель: локальный агент должен писать маленький, проверяемый код в стиле LLMGameCreator, а не создавать новые абстрактные слои ради видимости работы.

## Общие правила качества

Хороший patch:

```text
- минимален по diff;
- решает ровно текущую задачу;
- следует существующим локальным паттернам;
- имеет понятные tests/fixtures, если behavior изменился;
- не расширяет публичные контракты без explicit task;
- не создаёт новый слой архитектуры без user approval;
- не прячет ошибки;
- не снижает строгость validators/diagnostics;
- проходит .devflow/scripts/check-all.ps1.
```

Плохой patch:

```text
- большой refactor без необходимости;
- rename/formatting вперемешку с feature;
- новый God Service;
- service, который только прокидывает вызов без поведения;
- static mutable state;
- catch { } или catch с потерей diagnostic;
- bool success + string error вместо существующего report/diagnostic pattern;
- TODO вместо завершённого behavior;
- удаление/ослабление тестов ради прохождения;
- silent fallback там, где нужна ошибка;
- реальный LLM/provider call в тестах;
- изменение Designer без UI-задачи;
- изменение GamePackage schema без stop+approval.
```

## Layer ownership

Соблюдай ответственность проектов:

```text
Domain:
  Contracts, definitions, enums, validation primitives.
  No IO, no UI, no provider calls.

GamePackage:
  Root package definition and path conventions.
  No editor workflow logic.

Application:
  Use-cases, validators, workflow services, review/apply boundaries.
  UI calls this layer instead of owning behavior.

Infrastructure:
  JSON/files/settings/logging persistence.
  No UI decisions, no runtime behavior.

Generation:
  LLM authoring/generation models and editor-side generation abstractions.
  No runtime dependency.

Runtime:
  Headless game execution.
  No LLM, no WinForms, no provider calls, no package mutation.

Runtime.Abstractions:
  Runtime command/state/event/service contracts for frontends.

Scripting:
  Script execution abstractions and sandboxed/typed Lua support only when explicitly allowed.

AssetPipeline:
  Editor-side asset generation provider abstractions/jobs.
  Runtime must not depend on these providers.

WinForms:
  Shell/pages only. Thin UI over Application services/presenters.
  Layout in Designer, behavior in .cs.

Tests:
  Smoke/contract/regression/fake/corpus tests.
```

## Naming and diagnostics

Diagnostic codes must be stable and machine-readable:

```text
lowercase.dot.separated
```

Examples:

```text
strict_json.parse.failed
strict_json.contract.missing_id
artifact.review.invalid_decision
package.reference.missing
runtime.smoke.command_failed
```

Do not use transient prose as a diagnostic code.

## Result/report style

Prefer typed result/report objects already used in the project.

Do not invent a new generic result abstraction unless the task explicitly asks for it.

A good failure path should provide:

```text
severity
code
message
path or target id when available
suggested fix when useful
source artifact/file when available
```

## Tests and fixtures

For any new behavior:

```text
- at least one pass test;
- at least one fail/reject test;
- one regression test if fixing a bug;
- fake/corpus tests for LLM-facing code;
- runtime smoke/scenario for runtime-facing code.
```

Do not add dozens of low-value tests. Add small tests that pin the contract.

## UI rules

WinForms rules:

```text
- Designer contains layout only.
- Constructor does not contain business logic.
- UI event handler delegates to service/presenter/use-case.
- UI does not directly read/write package JSON.
- UI does not execute game effects.
- UI does not call LLM/provider directly unless existing explicit editor generation service path requires it.
```

## Parser/repair/LLM rules

LLM-facing code must be strict:

```text
- raw output is stored/inspectable when relevant;
- extraction is deterministic;
- parse failures produce diagnostic codes;
- repair has max attempts;
- repaired output is validated again;
- no artifact is accepted without validation;
- tests use fake clients/corpus, not real models.
```

## Runtime rules

Runtime code must remain deterministic and headless:

```text
- no LLM calls;
- no WinForms calls;
- no file/provider/generator dependency unless existing runtime snapshot/storage boundary explicitly owns it;
- command input -> state/events output;
- rendering never mutates state;
- serialization roundtrip must preserve required state.
```

## Before writing code

For code tasks, write in CURRENT_RUN.md:

```text
Local pattern chosen:
Why this layer owns the behavior:
Expected tests/fixtures:
Diagnostic codes to add/change:
Files expected to change:
```
