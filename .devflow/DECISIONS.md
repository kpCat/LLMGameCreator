# DECISIONS.md — зафиксированные решения автономного режима

## D-001. Не создаём отдельный тяжёлый dev-agent проект

Autonomous development pack реализуется как `.devflow/` + scripts + prompts + task graph. Это дешевле и безопаснее, чем новый большой агентный проект.

## D-002. Локальная модель — исполнитель по рельсам, не главный архитектор

Локальный агент может планировать, читать, патчить, запускать проверки и чинить простые ошибки. Но он останавливается при schema/dependency/runtime-boundary/architecture risks.

## D-003. Один task по умолчанию

Базовый режим: одна задача за запуск. Пачка 2-3 задач разрешается только после стабильных успешных прогонов и только для low-risk задач.

## D-004. Build/test/checks являются oracle

Мнение модели не является доказательством. Доказательство — build/test/simulation/report.

## D-005. Реальные LLM calls не используются в автотестах

Для тестов используются fake clients, corpus, fixtures, deterministic simulation. Реальная модель используется только в explicit evaluation/manual gate.

## D-006. M4.1 gate уважается

Пока current state не обновлён, задачи M5/M6/broad expansion/runtime repair loop заблокированы.

## D-007. Нет git-команд

Агент не выполняет git-команды. Пользователь сам управляет ветками и публикацией изменений.

## D-008. Не читаем весь проект без причины

Агент читает source docs, context index, task-specific docs, target files и 2-3 локальных аналога. Broad source scan запрещён без причины.
