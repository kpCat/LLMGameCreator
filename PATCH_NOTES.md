# LLMGameCreator v0.1.2 local patch

Назначение патча:

1. Исправить ошибку компиляции `CS0234` в `Program.cs`, где `Application.Run(...)` мог разрешаться как namespace `LLMGameCreator.Application`, а не как `System.Windows.Forms.Application`.
2. Зафиксировать Unity Player Contract, чтобы GamePackage дальше проектировался под будущий Unity runtime, а не только под WinForms preview.
3. Расширить Lua-заготовки: добавить blueprint-скрипты для бесконечной/ограниченной карты, биомов, поселений, дорог, NPC поведения, взаимодействий, loot, encounter, погоды и базовых формул.

Патч в основном добавляет документы и template/sample Lua-файлы. Из C# изменён только `src/LLMGameCreator.WinForms/Program.cs`.

После распаковки поверх репозитория выполнить:

```powershell
dotnet restore
dotnet build
dotnet test
```

В этом окружении `dotnet` недоступен, поэтому сборка здесь не выполнялась.
