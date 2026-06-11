# LLMGameCreator

`LLMGameCreator` — скелет редактора/генератора data-driven игр с отдельным runtime contract.

Цель проекта: создавать **Game Package**, который может быть исполнен отдельным player/runtime без LLM.

## v0.1 skeleton

Включено:

- чистый WinForms editor shell;
- отдельные UserControl-страницы;
- DryIoc composition root;
- простое файловое логирование;
- app settings с профилями локальных LLM/ComfyUI на разных ПК;
- GamePackage contract;
- Domain-модели для карты, сущностей, ассетов, Lua-скриптов, диалогов, способностей, квестов;
- headless runtime с командами `Move` и `Interact`;
- WinForms Runtime Preview: WASD + Enter;
- JSON storage;
- минимальный валидатор;
- sample `minimal-map-game`;
- минимальные smoke-тесты.

Не включено:

- реальный Lua engine;
- реальная интеграция LM Studio/OpenAI-compatible API;
- реальная интеграция ComfyUI/Fooocus;
- Unity Player;
- полноценная боёвка/инвентарь/способности;
- редакторы всех сущностей.

## Первый запуск

```powershell
dotnet restore
dotnet build
```

Далее открыть `LLMGameCreator.sln`, запустить `LLMGameCreator.WinForms`.

В программе открыть папку:

```text
samples\minimal-map-game
```

Потом перейти в `Runtime Preview`, нажать `Старт`, управлять `WASD`, взаимодействие — `Enter`.
