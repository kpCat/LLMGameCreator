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

## Recorded manual failure and retry

The real Goal148 manual attempt used project title `Проверка конструктора` and
failed after `Собрать и проверить игру`. Package activation raised
`CurrentChanged` from the build worker, and an unsafe WinForms subscriber reached
`_navigation` from that worker thread. Goal148B records and repairs this exact
failure class; it does not accept Goal148 and does not replace the required human
retry.

```text
goal148Accepted=false
manualRetryRequired=true
manualFailureClass=current_package_changed_cross_thread_ui_dispatch
rawScreenshotNotCommitted=true
```

The screenshot and raw manual files remain outside the repository. Retry the
same checklist after the Goal148B automated hotfix evidence is GREEN.

## Recorded manual identity-preservation failure

The human repeated the real workflow after Goal148B in project
`goal148-manual`. The build and canonical Runtime checks completed without the
cross-thread error, with six configured parameters, one prepared support file,
composition package SHA
`e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221`
and final Runtime state hash
`95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8`.
The activated package nevertheless replaced the user's title
`Проверка конструктора` with template title `Minimal Map Game`. Goal148C records
and repairs this identity-preservation failure; Goal148 remains unaccepted and
requires another human retry.

```text
goal148Accepted=false
manualRetryRequired=true
manualBuildExecutionPassed=true
manualCrossThreadFailureResolved=true
manualFailureClass=project_identity_overwritten_by_template_manifest
rawScreenshotsNotCommitted=true
```
