# Codex task queue

Задачи должны быть маленькими. Не давать Codex широкие формулировки.

## Task 001 — Add docs/lua baseline

Применить patch v0.1.1. Проверить, что сборка не сломана.

## Task 002 — Script manifest validation

Добавить validator, который проверяет:

- script id уникален;
- script type известен;
- path существует;
- declared entry points не пустые;
- capabilities соответствуют типу script.

Не добавлять реальный Lua engine.

## Task 003 — Asset contracts validation

Проверять:

- asset id уникален;
- asset type известен;
- contract id существует;
- required portrait variant `neutral` есть;
- missing files дают warning/error.

Не добавлять ComfyUI.

## Task 004 — GamePackage folder structure check

Добавить use-case проверки структуры папки игры:

- manifest/package exists;
- lualib exists;
- scripts dirs optional;
- assets dirs optional;
- reports diagnostics.

## Task 005 — Runtime Preview cleanup

Не добавлять новые механики. Только отделить UI от runtime preview presenter/service, если сейчас смешано.

## Task 006 — Manual asset catalog page

Только UI для просмотра AssetCatalog. Без генерации.

## Task 007 — Lua engine spike

Отдельная research/spike задача. Ничего не интегрировать глубоко до подтверждения sandbox limitations.

## Task 008 — ComfyUI workflow profile model

Добавить модели workflow profile и repository. Без HTTP client.

## Task 009 — LLM profile model hardening

Профили моделей:

- endpoint;
- model;
- context window;
- role;
- LAN support;
- timeout;
- max parallel jobs.

Без реального клиента.

## Task 010 — ContextPack skeleton

Добавить модели ContextPack, ContextPackBuilder interface, сохранение в generation/context-packs.

Без реальной LLM.
