# VERIFICATION_MATRIX.md — обязательные проверки по типам изменений

Этот файл определяет, какие проверки нужны для разных типов изменений. Агент не имеет права считать задачу завершённой без соответствующего набора проверок.

## Базовая проверка для любой задачи

Всегда:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Если проверка невозможна, причина должна быть записана в отчёт.

## Матрица

| Тип изменения | Обязательные проверки | Запрещено |
|---|---|---|
| Docs-only | build/test желательно; devflow/docs state guard обязательно | Менять roadmap/current state без причины |
| Devflow-only | `check-devflow-state.ps1`; по возможности `check-all.ps1` | Менять production-код |
| Validator | valid sample + invalid sample + focused validator test + full test run | Ослаблять severity без причины |
| LLM strict contract | raw output fixtures + parser test + validator test + repair success/fail test + fake LLM batch | Реальный LLM в автотестах |
| Raw output parser | corpus: fenced JSON, text before/after JSON, broken JSON, wrong root, id drift | Глотать parse errors без diagnostics |
| Repair loop | fake client: invalid -> repaired valid, invalid -> still invalid, repair max attempts | Бесконечный repair |
| Evaluation report | saved JSON/report import test + hot spot grouping test + markdown output test | Делать provider calls при import |
| Artifact Review | service tests for approve/reject/repair decision + approved set rebuild test | Мутировать GamePackage из review UI |
| Package assembly | fixture artifacts -> assembly -> package validation + invalid refs rejection | Менять schema silently |
| Runtime behavior | runtime smoke scenario: load/start/wait/command/serialize/deserialize | Runtime calls LLM/provider/UI |
| WinForms UI wiring | presenter/service fake + UI action smoke if possible + build | Бизнес-логика в Designer |
| Storage | read/write roundtrip + backward compatibility sample | Менять package layout без approval |
| Lua registry/manifest | manifest fixture + forbidden field/duplicate/id/path tests | Выполнять Lua без explicit task |
| Lua execution | sandbox policy tests + deterministic RNG + forbidden API rejection | Filesystem/network/process/debug API |
| Asset workflow | asset contract fixture + missing asset fallback validation | Runtime depends on generator provider |
| Current state docs | `CURRENT_GENERATOR_STATE.md` and `.json` consistency check | Обновить только один из пары |

## Минимальный тестовый стиль

Новая фича не требует десятков тестов. Минимум:

```text
1 smoke test
1 contract/validator test
1 regression test if fixing a bug
```

Если фича LLM-facing, обязательно добавь fake/corpus проверку.

Если фича runtime-facing, обязательно добавь runtime smoke или scenario fixture.

Если фича UI-facing, UI должен быть thin over service; сначала проверяется service/presenter.
