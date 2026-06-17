# CODE_QUALITY_AND_STYLE.md — критерии качества кода для локального агента

Цель: локальный агент должен писать маленький, проверяемый код в стиле LLMGameCreator, а не создавать новые абстрактные слои ради видимости работы.

Этот файл обязателен для любых code/test/docs task. Если task spec говорит короче, но этот файл строже — соблюдай этот файл, пока нет явного user approval на исключение.

## Quality routing

Для agent-task задач также читай shared quality docs из `docs/agent-tasks/`:

```text
docs/agent-tasks/_TEST_QUALITY_RULES.md
docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
docs/agent-tasks/_DIFF_HYGIENE_RULES.md
docs/agent-tasks/_AGENT_EXECUTION_QUALITY_RULES.md
```

Не нужно читать все task specs. Эти shared docs — общие правила исполнения, а не новые задачи.

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
- не переписывает unrelated tests/style;
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
- weak tests instead of proof tests;
- silent fallback там, где нужна ошибка;
- реальный LLM/provider call в тестах;
- изменение Designer без UI-задачи;
- изменение GamePackage schema без stop+approval;
- добавление generated run/log/TRX/build output в финальный diff.
```

## Layer ownership

Соблюдай ответственность проектов:

```text
Domain: Contracts, definitions, enums, validation primitives. No IO, no UI, no provider calls.
GamePackage: Root package definition and path conventions. No editor workflow logic.
Application: Use-cases, validators, workflow services, review/apply boundaries.
Infrastructure: JSON/files/settings/logging persistence. No UI decisions, no runtime behavior.
Generation: LLM authoring/generation models and editor-side generation abstractions. No runtime dependency.
Runtime: Headless game execution. No LLM, no WinForms, no provider calls, no package mutation.
Runtime.Abstractions: Runtime command/state/event/service contracts for frontends.
Scripting: Script execution abstractions and sandboxed/typed Lua support only when explicitly allowed.
AssetPipeline: Editor-side asset generation provider abstractions/jobs. Runtime must not depend on these providers.
WinForms: Shell/pages only. Thin UI over Application services/presenters. Layout in Designer, behavior in .cs.
Tests: Smoke/contract/regression/fake/corpus tests.
```

## Naming and diagnostics

Diagnostic codes must be stable and machine-readable:

```text
lowercase.dot.separated
```

Do not use transient prose as a diagnostic code.

If behavior produces diagnostics, proof tests must assert exact diagnostic codes unless the task spec explicitly allows more than one code. If multiple codes are allowed, the test must assert an explicit allowed set and the report must explain why.

## Tests and fixtures

For any new behavior:

```text
- at least one pass test;
- at least one fail/reject test;
- one regression test if fixing a bug;
- fake/corpus tests for LLM-facing code;
- runtime smoke/scenario for runtime-facing code;
- exact diagnostic/state/count/order assertions where applicable.
```

Weak tests are not proof tests.

Bad:

```text
Assert.False(result.Ok);
Assert.Single(result.Diagnostics);
Assert.NotEmpty(items);
```

Good:

```text
Assert.False(result.Ok);
Assert.Contains(result.Diagnostics, d => d.Code == ExpectedCode);
Assert.Equal(expectedCount, result.Items.Count);
Assert.Equal(expectedOrder, result.Items.Select(item => item.Id));
```

Follow `docs/agent-tasks/_TEST_QUALITY_RULES.md` and `docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md`.

## Existing style preservation

Do not mechanically rewrite unrelated code/tests.

Preserve readable C# raw string literals for JSON/Markdown/Lua when they improve readability.

Do not mix formatting churn with behavior changes. If formatting cleanup is needed, make it a separate explicit task.

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

If parser behavior is intentionally strict, do not turn it into permissive extraction without an explicit approved task.

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

## Diff hygiene

Before final report, verify the conceptual final diff:

```text
- only task allowed files changed;
- no generated run artifacts;
- no logs/TRX/build outputs;
- no unrelated formatting churn;
- no .sln/.csproj/dependency change unless explicitly allowed;
- no large artifact/corpus file unless task specifically required it.
```

The agent may be forbidden to use git commands. In that case it must report the files it changed from its own editing context and warn if it cannot verify the repository diff.

Follow `docs/agent-tasks/_DIFF_HYGIENE_RULES.md`.

## Before writing code

For code tasks, write in `CURRENT_RUN.md`:

```text
Local pattern chosen:
Why this layer owns the behavior:
Expected tests/fixtures:
Expected proof assertions:
Diagnostic codes to add/change:
Files expected to change:
Diff hygiene risks:
```
