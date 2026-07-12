# Capability-Driven Runtime Playthrough and Equipment FeatureModule Vertical Slice

Status: GREEN, manual review deferred
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
3. Select `Экипировка и оружие`, keep `Бонус урона оружия` at `2`, and build again.
4. Confirm the human summary contains `Экипировано: Ржавый нож`, `Слот: Оружие` and `Бонус урона: +2`.
5. In technical details, confirm the plan has 17/13/17 actions, equipment is `slot/weapon:item/rusty_knife`, and the combat delta is `2`.
6. Save, close and reopen the project; confirm the equipment selection, parameter value, equipped-item proof and deterministic replay remain stable.

Manual review is optional for this delivery and has been deferred. Do not mark
Goal149 accepted until an explicit human decision is recorded. Do not start
Goal150 from this document alone.
