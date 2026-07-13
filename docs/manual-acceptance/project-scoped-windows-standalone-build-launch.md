# Project-Scoped Windows Standalone Build & Launch

Status: accepted by human before Goal153
Gate: accepted
Accepted: true
Accepted by human: true
Accepted by Codex: false
Manual review performed: true

> Я принимаю Goals152/152A/152C: standalone показал зелёную автопроверку, интерфейс читаемый, кнопки Далее/Назад/В конец/Сбросить работают, текст обновляется без наложения; host cache переиспользован без запуска Unity Editor.

Accepted commit: `ac97859c8de861641e07f886250d053b5330fbe9`

Goal152 adds the normal `Игры → Сборка и проверка` route for a **Windows standalone Alpha**.
The host is generic and only presents Runtime-backed PlayerAdapter payload. Gameplay truth remains Runtime.

Short LocalAppData disposable copies now pass the required project-scoped executable rename and
launch smoke. The final review must verify a selected project’s standalone title, package identity, frame navigation,
`Gameplay truth: Runtime`, launch smoke result and build folder. This document records the owner's
completed review; Codex does not claim the acceptance decision.
