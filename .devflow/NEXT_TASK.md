# NEXT_TASK.md — следующая разрешённая задача

Текущая следующая задача:

```text
BASELINE-001
```

## Что нужно сделать

1. Не менять production-код.
2. Запустить базовую проверку:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
   ```

3. Обновить `.devflow/CURRENT_RUN.md`:
   - прошёл ли build;
   - прошли ли tests;
   - есть ли baseline failures;
   - где сохранены логи;
   - какой следующий безопасный task.

4. Если baseline не проходит — записать блокер в `.devflow/BLOCKERS.md` и остановиться.

## Почему сначала baseline

Нельзя поручать агенту исправления, пока не понятно, в каком состоянии проект был до его изменений.
