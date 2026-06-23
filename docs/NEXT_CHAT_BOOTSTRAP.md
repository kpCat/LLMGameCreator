Я продолжаю разработку проекта LLMGameCreator / AI Game Builder.

Критически важно:
1. Отвечай на русском.
2. Не предлагай мелкие задачи для Codex/Kilo: агенты много тратят на входной контекст. Нужны крупные, но хорошо ограниченные macro-slices.
3. Branch management полностью на мне. В задачах для агентов не пиши “создай ветку”, “переключись на main”, “merge”, “rebase”, “cherry-pick”, “push”. Агент работает в текущем рабочем дереве.
4. У меня Windows. В задачах для агентов запрещай использовать `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt`. Пути должны быть repo-relative или обычные Windows/PowerShell. Агенты не должны читать внешние sandbox-пути.
5. Для backend/meta/JSON/materialization/validation/smoke задач сначала выбирай Kilo Code, чтобы экономить лимиты Codex.
6. Codex используй для сложной архитектуры, рискованных slices, ревью/ремонта, UI/WinForms/Runtime, либо когда Kilo провалился.
7. Все задачи должны иметь read-first список, allowed/forbidden files, exact behavior, tests, validation commands, stop conditions и финальный отчёт.
8. Нельзя трогать Runtime/GamePackage schema/WinForms/generator-library/.sln/*.csproj без явного разрешения.
9. Нельзя вызывать LLM/provider, ComfyUI/Suno, исполнять Lua/generators, реализовывать Unity, если это не цель slice.
10. После пуша ветки я обычно прошу тебя сделать review diff и решить: merge / cleanup / reject.

Проект:
- Repo: https://github.com/kpCat/LLMGameCreator
- Цель: C# WinForms/.NET 8 editor для data-driven games. LLM генерирует JSON/Lua, но не runtime/C#.
- Архитектура: Domain, GamePackage, Application, Generation, AssetPipeline, Runtime, WinForms, tests, docs, generator-library.
- Unity runtime позже отдельно. Сейчас мы строим deterministic archive/export pipeline.

Недавнее состояние:
- Slice 019: Unity archive game-data payload service. Пишет существующий GamePackageDefinition в архив, safe paths, UTF-8 no BOM, stable sorting.
- Slice 020: Unity archive asset/audio/Lua request pipeline. Пишет request metadata:
  - assets/asset-requests.json
  - assets/asset-request-index.json
  - audio/audio-requests.json
  - audio/audio-request-index.json
  - lua/module-requests.json
  - lua/modules-index.json
  Потом cleanup/refactor: BuildRequests один раз; readiness BlockedByErrors; future provider warnings aggregated; сервис разрезан на build context, asset/audio/Lua builders, diagnostics builder.
- Slice 021: Provider Job Plan. Добавлены fulfillment slots, provider-specific jobs, readiness report:
  - production/fulfillment-plan.json
  - production/readiness-report.json
  - assets/asset-slots.json
  - audio/audio-slots.json
  - lua/module-slots.json
  - providers/manual-import/jobs.json
  - providers/comfyui/jobs.json
  - providers/suno/jobs.json
  - providers/local-audio/jobs.json
  - providers/procedural/jobs.json
  Важно проверить/harden: provider job plan errors должны влиять на materialization readiness; request diagnostics не должны превращаться в request.request.diagnostic...
- Я пока не хочу терять контекст и хочу продолжать с проверки текущей рабочей ветки/S021 hardening, потом уже S022.

Следующая рабочая логика:
1. Сначала review/hardening текущего S021 provider job plan.
2. Потом S022: Provider Output Intake & Fulfillment State v1.
3. Для S022 предпочтительно Kilo Code first, Codex только на ревью/ремонт.
