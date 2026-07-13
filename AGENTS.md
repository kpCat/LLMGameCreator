# AGENTS.md — LLMGameCreator

## Required orientation order for agents

1. Read this AGENTS.md.
2. Read docs/CONTEXT_INDEX.md.
3. Read docs/CURRENT_GENERATOR_STATE.md.
4. Read docs/PRODUCT_LINE_CORE_STRATEGY.md.
5. Read docs/NARROW_ALPHA_EXPANSION_POLICY.md.
6. Read docs/AUTOMATED_VALIDATION_TIERS.md.
7. For generator tasks, read docs/ROADMAP_TO_FULL_GENERATOR.md and the docs named by CURRENT_GENERATOR_STATE.

If a task changes generator milestones, roadmap, LLM generation flow, artifact review flow, capability selection, evaluation, Lua integration, package assembly or recommended next steps, it must update docs/CURRENT_GENERATOR_STATE.md and docs/CURRENT_GENERATOR_STATE.json.

LLMGameCreator is a data-driven game product-line combiner, not prompt-to-game.
LLM is optional local authoring assistance only.
Next broad product work must preserve FeatureModule / RuntimePrimitive / SemanticPack / VisualPartPack / WorldSourceAdapter / PlayerAdapter seams.

## Главная цель проекта

LLMGameCreator — это WinForms-редактор и генератор GamePackage для data-driven игр.

Проект не является “чатом, который пишет игру одним prompt-ом”.

LLMGameCreator создаёт GamePackage:

* JSON definitions;
* typed Lua scripts;
* Lua generators;
* asset catalog;
* asset generation requests;
* maps/chunks;
* entities/components;
* dialogues;
* quests;
* validation reports;
* generation history.

Runtime/Player исполняет GamePackage без LLM.

## Жёсткие архитектурные правила

1. Runtime не вызывает LLM.
2. Runtime не вызывает ComfyUI/Fooocus.
3. LLM используется только в editor/generation pipeline.
4. Единственный источник правды для готовой игры — GamePackage.
5. Unity Player в будущем является универсальным frontend/player, а не редактором конкретной игры.
6. Unity Player не должен содержать конкретную игровую логику в C#.
7. WinForms Editor не должен быть runtime-player финальной игры.
8. WinForms Runtime Preview — только отладочный preview.
9. UI не читает и не пишет JSON напрямую.
10. UI не исполняет игровые effects напрямую.
11. UI вызывает Application services/use-cases.
12. MainForm — только shell.
13. Каждая страница WinForms — отдельный UserControl.
14. Layout форм и UserControl хранить в InitializeComponent() в *.Designer.cs.
15. Логика и зависимости — в основном *.cs.
16. DryIoc использовать только в CompositionRoot.
17. Не использовать container.Resolve() внутри бизнес-логики.
18. Не создавать God Services и God Forms.
19. Lua строго типизирован по назначению: prototype, generator, behavior, interaction, formula, event, migration.
20. Lua не должен напрямую мутировать C# GameState.
21. Lua должен возвращать draft/effects/actions, которые проверяет runtime/validator.
22. LLM-generated Lua применяется только через draft/validation/apply pipeline.
23. Ассеты являются data-driven сущностями и используются через assetId.
24. Сущности не должны ссылаться на hardcoded filesystem path напрямую.
25. ComfyUI/Fooocus — внешние editor providers, не часть runtime.
26. Runtime должен работать без asset generation providers.
27. Отсутствующий ассет должен иметь fallback-поведение.
28. Размер игры не должен зависеть от размера LLM context.
29. Большая генерация разбивается на GenerationSession/GenerationJob/ContextPack.
30. LLM не получает весь проект, только релевантный ContextPack.

## Документы, которые нужно читать перед крупными задачами

Для проектирования и аудита Goal:

* docs/GOAL_DESIGN_QUALITY_POLICY.md

Для архитектурных задач:

* docs/PROJECT_VISION.md
* docs/ARCHITECTURE.md
* docs/DEVELOPMENT_RULES.md
* docs/ROADMAP.md
* docs/CODEX_PATCH_RULES.md

Для WinForms UI:

* docs/WINFORMS_DESIGNER_RULES.md

Для GamePackage:

* docs/GAME_PACKAGE_FORMAT.md
* docs/ENTITY_COMPONENT_MODEL.md
* docs/GAME_SYSTEMS_MODEL.md

Для Lua:

* docs/LUA_SCRIPTING.md
* docs/LUA_STANDARD_LIBRARY.md
* docs/LUA_BLUEPRINT_CATALOG.md
* docs/LUA_BLUEPRINT_EXPANSION.md
* docs/SCRIPT_MANIFEST_SPEC.md

Для ассетов:

* docs/ASSET_PIPELINE.md
* docs/ASSET_CONTRACT_SPEC.md
* docs/ASSET_WORKFLOW_PROFILES.md

Для Unity Player:

* docs/UNITY_PLAYER_CONTRACT.md

Для генерации:

* docs/GENERATION_PIPELINE_DETAILED.md
* docs/LIMIT_BUDGET_AND_GOALS.md

Для validation:

* docs/VALIDATION_STRATEGY.md

## Правила изменения кода

Перед изменениями:

1. прочитай этот AGENTS.md;
2. прочитай релевантные docs;
3. прочитай csproj целевого проекта;
4. найди 2-3 локальных аналога;
5. кратко отчитай, что изучено и какой паттерн будет использован.

Не делай крупный рефакторинг без прямого запроса.

Если задача затрагивает больше 8-10 файлов, сначала предложи план и разбиение.

Если задача требует Unity, ComfyUI, Lua engine или SQLite — не подключай реальную интеграцию без отдельного подтверждения.

## Тесты

Тесты должны быть минимальными и полезными.

Обычно:

* 1 smoke test;
* 1 contract/validator test;
* 1 regression test, если исправляется баг.

Не добавлять десятки тестов ради видимости качества.

## Git

Не выполнять git-команды без прямого запроса пользователя.

## Unity execution

Before Unity-host work, read and follow [docs/UNITY_EXECUTION_POLICY.md](docs/UNITY_EXECUTION_POLICY.md). It governs Unity invocation budgets, standalone host-cache reuse, atomic player files and hidden automated smoke.
