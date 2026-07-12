# Project-Scoped Windows Standalone Build & Launch

Status: BLOCKED before manual review
Gate: `goal152_project_scoped_windows_standalone_build_launch required`
Accepted: false
Accepted by human: false
Accepted by Codex: false
Manual review performed: false

Goal152 adds the normal `Игры → Сборка и проверка` route for a **Windows standalone Alpha**.
The host is generic and only presents Runtime-backed PlayerAdapter payload. Gameplay truth remains Runtime.

The current blocker is Unity player startup after the required project-scoped executable rename:
the renamed player cannot load its byte-identical adjacent MonoBleedingEdge runtime, so no standalone
smoke or manual review can be claimed. The final review must verify a selected project’s standalone title, package identity, frame navigation,
`Gameplay truth: Runtime`, launch smoke result and build folder. This document intentionally does not
claim Goal152 human acceptance.
