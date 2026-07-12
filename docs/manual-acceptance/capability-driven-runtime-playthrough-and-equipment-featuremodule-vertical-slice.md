# Capability-Driven Runtime Playthrough and Equipment FeatureModule Vertical Slice

Status: GREEN, bundled manual review pending after Goal150A full suite and independent audit
Gate: `capability_driven_runtime_playthrough_and_equipment_featuremodule_vertical_slice_verification required`
Accepted: false
Accepted by human: false
Accepted by Codex: false

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
3. After Goal150A completes its exact full suite and is independently audited, select `Экипировка и оружие`, `Характеристики персонажа` and `Уровни и опыт`.
4. Set weapon bonus `3`, starting strength `8`, damage per strength point `2` and level-two experience `12`.
5. Build and confirm `Экипировано: Ржавый нож`, `Бонус урона: +3`, `Сила: 8`, `Бонус урона от силы: +6`, `Уровень: 2` and `Опыт: 12`.
6. In technical details, confirm stat/equipment/total damage `6/3/9`, progression `12:level/2`, and 20/16/20 planned/checkpoint/final actions.
7. Save, close and reopen the project; confirm selections and all four custom values persist, then rebuild and compare hashes/replay markers.

Goal149 remains accepted=false. Its deferred review is now routed into the
bundled Goals149/150/150A mechanics review; do not mark any goal accepted until
Goal150A is independently audited and an explicit human decision is recorded.
