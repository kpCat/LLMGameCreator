# Unified Game Project Workspace and Legacy Diagnostics Isolation

Status: GREEN, manual review required
Gate: `unified_game_project_workspace_and_legacy_goal_diagnostics_isolation_verification required`
Accepted: false
Accepted by human: false
Accepted by Codex: false

## Review checklist

1. Open `Игры` and confirm the normal start surface shows `Мои игры`, `Новая игра`, `Открыть выбранную` and `Открыть папку`.
2. Open a project and review `Обзор`, `Механики`, `Настройки`, `Сборка и проверка` and `Технические детали`.
3. Confirm required mechanics are locked, optional mechanics have friendly Russian titles and parameter controls come from catalog metadata.
4. Save and reopen the project; confirm selected mechanics and parameter values return without manual JSON editing.
5. Create a new game, run `Собрать и проверить игру` without copying files manually, and confirm the human summary includes `Файлы проекта подготовлены: 1` plus successful save/load, replay and package update.
6. Confirm technical hashes are visible only in `Технические детали`.
7. Open `Диагностика генератора`; confirm internal numbered checks are hidden until `Показать внутренние проверки` is selected.

Goal148 must remain `accepted=false` until an explicit human decision is recorded.

Goal148A automated hotfix evidence is GREEN: first production New Game build
copies the package-required relative script from the confined read-only
narrow-alpha source, repeat build reuses it, conflicts and missing sources are
rejected, and rollback removes a newly copied file. This does not accept
Goal148; the checklist above remains the active human gate.
