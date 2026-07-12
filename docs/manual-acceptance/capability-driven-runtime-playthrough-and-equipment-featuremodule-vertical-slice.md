# Capability-Driven Runtime Playthrough and Equipment FeatureModule Vertical Slice

Status: ACCEPTED by human
Gate: historical accepted mechanics milestone
Accepted: true
Accepted by human: true
Accepted by Codex: false

## Human acceptance record

Я принимаю Goal151 и объединённую ручную проверку Goals149/150/150A/150B: свежий бинарник commit 2516931f успешно собрал и проверил проект goal148-manual с параметрами 3/8/2/12; equipment/stat/total=3/6/9, level/XP=2/12, интерфейс и диагностика корректны.

- Accepted commit: `2516931f9c8242bbd59fe5cf73f9e66b405ef16c`
- Custom values: `3/8/2/12`; equipment/stat/total: `3/6/9`; level/XP: `2/12`.
- This acceptance is human-only; `acceptedByCodex=false`.

## Automated evidence

- Goal148 is accepted by the exact human statement recorded in its manual-acceptance document.
- The primary Игры workflow derives its Runtime plan from structured FeatureModule contracts; it does not use the legacy fixed 13-action fallback.
- Equipment disabled preserves the accepted Goal148 composition, activated-package and final-state hashes with 13/8/13 planned, checkpoint and final actions.
- Equipment enabled qualifies and replays 17/13/17 actions, equips `item/rusty_knife` in `slot/weapon`, and preserves that summary through checkpoint/save/replay.
- The configured weapon bonus and observed player combat delta are both `2`.
- Equipment without combat and combat without equipment both qualify; non-player combat does not receive the player weapon bonus.
- Adding the unselected equipment module is additive-compatible with unrelated saved projects. Selected/required module drift remains stale or unresolved.
- The normal workspace still exposes zero Goal-number controls and adds no top-level page.

## Suggested manual review

1. Open an existing Goal148 project in `Игры`; confirm it opens without a stale warning and equipment is not selected.
2. Build it unchanged; confirm the technical details show a capability-driven 13/8/13 plan and the accepted Goal148 hashes.
3. After Goal150C hermetic validation is GREEN and independently audited, select `Экипировка и оружие`, `Характеристики персонажа` and `Уровни и опыт`.
4. Set weapon bonus `3`, starting strength `8`, damage per strength point `2` and level-two experience `12`.
5. Build and confirm `Экипировано: Ржавый нож`, `Бонус урона: +3`, `Сила: 8`, `Бонус урона от силы: +6`, `Уровень: 2` and `Опыт: 12`.
6. In technical details, confirm stat/equipment/total damage `6/3/9`, progression `12:level/2`, and 20/16/20 planned/checkpoint/final actions.
7. Save, close and reopen the project; confirm selections and all four custom values persist, then rebuild and compare hashes/replay markers.

Goal149 is accepted by human as part of the bundled mechanics review. Goal150C through
Goal150F remain historical validation-infrastructure/debt records and are not the active product gate.

Goal151 is accepted by human through the same exact decision. The original saved project
remained byte-identical; diagnostic truth and executable provenance were accepted.
