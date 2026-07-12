# Project-Scoped Windows Standalone Build & Launch

Status: superseded by Goal152A UX hotfix; Goal152 remains unaccepted
Gate: `goal152_project_scoped_windows_standalone_build_launch required`
Accepted: false
Accepted by human: false
Accepted by Codex: false
Manual review performed: true (feedback captured); acceptance remains false

Goal152 adds the normal `Игры → Сборка и проверка` route for a **Windows standalone Alpha**.
The host is generic and only presents Runtime-backed PlayerAdapter payload. Gameplay truth remains Runtime.

Short LocalAppData disposable copies now pass the required project-scoped executable rename and
launch smoke. The final review must verify a selected project’s standalone title, package identity, frame navigation,
`Gameplay truth: Runtime`, launch smoke result and build folder. This document intentionally does not
claim Goal152 human acceptance.
