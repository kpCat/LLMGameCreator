# Continue up to 3 tasks prompt

Используй этот режим только после стабильного successful baseline.

Ты можешь выполнить максимум 3 low-risk задачи подряд.

Перед каждой следующей задачей проверь:

- предыдущая задача полностью прошла checks;
- следующая задача не требует approval;
- следующая задача allowed_before_m4_1_gate_review = true;
- нет stop conditions;
- суммарный diff не становится широким;
- ты не меняешь schema/dependency/runtime boundary.

Если любое условие не выполнено — остановись.
