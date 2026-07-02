# LLMGameCreator — Visual World Generation Handoff

Дата: 2026-07-02

Этот пакет документации фиксирует стратегическую идею graphics/pseudo-3D roadmap для LLMGameCreator.

## Главная формула

```text
LLM = автор законов мира.
LLMGameCreator = детерминированный генератор мира по этим законам.
Unity/Player = отображатель готового GamePackage/manifest data.
```

LLM не должна постоянно генерировать каждый дом, дерево, камень, NPC, тайл, текстуру, существо или модель. LLM должна редко помогать создавать компактные семантические профили мира, доменов, культур, религий, архитектур, материалов, мотивов, правил смешения и уникальных landmark-профилей. Массовую генерацию должен делать обычный код по seed и rules.

## Куда положить

Рекомендуется распаковать архив в корень репозитория:

```text
C:\Users\endim\LLMGameCreator\
```

Файлы уже разложены по путям:

```text
docs/proposals/
docs/agent-tasks/
docs/context/
```

## Как подать это в другом диалоге, чтобы не сбить текущую разработку

Не начинай новый диалог с “теперь реализуй это”. Лучше вставить так:

```text
Я добавил в репозиторий документы по Visual World Generation:
- docs/proposals/VISUAL_WORLD_GRAMMAR_AND_PSEUDO3D_GENERATION.md
- docs/proposals/VISUAL_RULE_STACK_AND_DOMAIN_PROFILES.md
- docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md
- docs/proposals/PSEUDO3D_ASSET_PRESENTATION_CONTRACTS.md
- docs/context/VISUAL_WORLD_GENERATION_CONTEXT_BRIEF.md

Прими это как стратегический контекст для LLMGameCreator. Не переключай текущую разработку на графику автоматически. Когда будешь читать репозиторий и формировать следующие goal-задачи, учитывай эти документы как направление для будущего graphics/pseudo-3D roadmap. Если текущая задача не про графику, просто держи это в фоне.
```

Если нужно именно продолжить графическое направление, тогда добавить:

```text
Теперь на основе этих документов предложи следующий безопасный composite goal для Codex/Kilo. Сначала contracts/recipes/validators/fixtures, без ComfyUI, без Unity runtime изменений, без изменения публичной GamePackage schema, если это не обосновано.
```

## Ключевая идея

```text
World/domain/culture/biome/object semantic features
→ VisualRuleStack
→ VisualRecipe
→ Procedural part composition
→ Surface/facade/billboard/atlas outputs
→ Pseudo-3D presentation package
→ Unity/player binding
```

## Почему это важно

Без такого слоя проект рискует уйти в тупик:

- генерировать тысячи разрозненных PNG;
- держать 50 готовых домиков, которые быстро повторяются;
- постоянно вызывать LLM для каждого объекта;
- получить style drift;
- получить графику, которую нельзя нормально использовать в Unity/player;
- смешать лор, gameplay и visual rules в хаотичных prompt-описаниях.

С этим слоем LLMGameCreator получает универсальный механизм:

- один и тот же генератор работает для фэнтези, техно-будущего, космоса, постапокалипсиса и любого нового лора;
- LLM используется редко и стратегически;
- массовая генерация мира остаётся быстрой, deterministic и seed-based;
- pseudo-3D становится главным целевым presentation mode для визуальных игр.


## Дополнительные документы в обновлённой версии

```text
docs/proposals/PROCEDURAL_VISUAL_DETAIL_GENERATOR_STRATEGY.md
docs/agent-tasks/CODEX_TASK_VISUAL_DETAIL_GENERATOR_CORE.md
```

Эти файлы фиксируют отдельное решение: просить Codex не генерировать тысячи визуальных деталей, а внедрить deterministic compiler/generator/validator, который позволит LLMGameCreator самому локально производить большие наборы variants по seed.
