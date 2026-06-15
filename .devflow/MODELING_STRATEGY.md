# MODELING_STRATEGY.md — как моделировать работу комбайна без реальных LLM-вызовов

Цель моделирования: проверить поведение pipeline без дорогих, нестабильных и медленных реальных model calls.

## Главный принцип

95% проверок генерационного pipeline должны работать через:

```text
fake client
saved raw-output corpus
fixtures
deterministic simulation
validators
runtime smoke
```

Реальная модель нужна только для compatibility/evaluation gate, а не для каждого теста разработки.

## Pipeline-граф для моделирования

Базовый граф:

```text
CapabilitySelection
 -> ContextPackBuilder
 -> PromptRenderer
 -> ILlmChatClient
 -> RawOutputStore
 -> RawOutputExtractor
 -> JsonParser
 -> ContractValidator
 -> RepairPolicy
 -> ArtifactStaging
 -> ArtifactReview
 -> ApprovedArtifactSet
 -> PackageAssembler
 -> PackageValidator
 -> RuntimeSmoke
```

Для каждого узла нужно понимать:

```text
input contract
output contract
preconditions
postconditions
diagnostic codes
failure modes
fake implementation
fixtures
```

## Обязательные fake-сценарии для LLM-facing кода

Минимальный набор:

```text
AlwaysValidJson
MarkdownFencedJson
TextBeforeJson
TextAfterJson
TwoJsonObjects
BrokenJsonComma
InvalidEscape
WrongRootObject
WrongContractId
MissingRequiredField
IdDrift
PlaceholderText
RepairSuccess
RepairFailure
Timeout
Exception
```

Реальные provider calls запрещены в unit/integration tests.

## Raw output corpus

Рекомендуемая будущая структура:

```text
tests/fixtures/llm_raw_outputs/
  game_profile_v1/
    valid_minimal.json.txt
    fenced_json.txt
    text_before_json.txt
    text_after_json.txt
    broken_trailing_comma.txt
    wrong_root.txt
    id_drift.txt
    placeholder_text.txt
  quest_pack_v1/
  scene_pack_v1/
  mechanics_pack_v1/
```

Каждый fixture должен иметь ожидаемый результат:

```text
expected: parse_pass
expected: repair_pass
expected: fail_with_diagnostic:<code>
```

## Scenario-based runtime smoke

Рекомендуемая будущая структура:

```text
tests/fixtures/runtime_smoke/
  minimal_adventure.smoke.json
  inventory_crafting.smoke.json
  dialogue_quest.smoke.json
  encounter_loot.smoke.json
```

Пример сценария:

```json
{
  "packagePath": "samples/minimal-map-game",
  "steps": [
    { "command": "LoadPackage" },
    { "command": "ValidatePackage", "maxSeverity": "Warning" },
    { "command": "StartRuntime" },
    { "command": "Wait" },
    { "command": "SerializeState" },
    { "command": "DeserializeState" },
    { "command": "Wait" }
  ]
}
```

## Что моделирование должно ловить

1. Prompt просит не ту schema version.
2. Raw output содержит markdown fence.
3. Raw output содержит текст до/после JSON.
4. Extractor неправильно режет объект.
5. Parser не даёт стабильный diagnostic code.
6. Repair исправляет JSON, но ломает contract fields.
7. Validator пропускает placeholder/test/example.
8. Artifact staging мутирует не тот artifact.
9. Review decision не пересобирает approved set.
10. Package assembly принимает invalid refs.
11. Runtime smoke падает после load/start/wait.
12. Serialization/deserialization ломает state.
13. Ошибка теряется вместо записи в validation/report.

## Forbidden shortcuts

Запрещено:

- заменять fake/corpus тесты реальным LLM call;
- считать “сгенерировалось один раз” доказательством стабильности;
- делать repair без max attempts;
- принимать artifact без validation;
- мутировать GamePackage внутри evaluation/review;
- исправлять runtime behavior через prompt.
