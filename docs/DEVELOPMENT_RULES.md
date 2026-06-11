# Development Rules

1. Не добавлять фичи без сохранения вертикального smoke-сценария.
2. Не смешивать UI, runtime, LLM и storage.
3. Не писать game-specific логику в C# runtime/player.
4. Все вкладки — отдельные UserControl.
5. Любая тяжёлая операция должна быть async/cancellable в будущих версиях.
6. DryIoc используется только в composition root.
7. Запрещены God Services и God Forms.
8. Ошибки не глотаются.
9. Runtime должен запускаться headless.
10. Тестов мало, но они должны защищать архитектурный минимум.
