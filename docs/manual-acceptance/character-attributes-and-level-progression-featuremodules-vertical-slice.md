# Character Attributes and Level Progression FeatureModules Vertical Slice

Status: GREEN, bundled manual review required with Goal149
Gate: `character_attributes_and_level_progression_featuremodules_vertical_slice_verification required`
Accepted: false
Accepted by human: false
Accepted by Codex: false

## Automated evidence

- The `Игры` workspace exposes `Характеристики персонажа` and `Уровни и опыт` as default-off catalog mechanics without a new page or Goal-number control.
- Default strength is `7`; the generic basic-attack metadata produces a stat damage bonus of `2` from baseline `5` and multiplier `1`.
- Equipment `+2` and stat `+2` combine independently into total additional damage `4`.
- Progression amount `10` resolves to `level/2` through the existing `OutputApplier` stage handling.
- Attributes-only and progression-only plans pass without combat, equipment or each other.
- All six current optional modules pass package validation, checkpoint reload, full replay and action binding.
- Goal149 disabled and equipment-enabled hashes remain unchanged.
- Adding both unselected modules is additive-compatible with the existing Goal148 project and does not auto-select mechanics.

## Bundled Goals149/150 manual review

1. Open an existing Goal148 project in `Игры`; confirm all three new mechanics from Goals149/150 are visible and disabled.
2. Build unchanged; confirm the Goal149 disabled 13/8/13 path and accepted hashes remain unchanged.
3. Enable `Экипировка и оружие`, `Характеристики персонажа` and `Уровни и опыт`.
4. In `Настройки`, confirm controls come from metadata and show weapon bonus `2`, starting strength `7`, damage per strength point `1`, and level-two experience `10`.
5. Build and confirm the summary contains `Экипировано: Ржавый нож`, `Сила: 7`, `Бонус урона от силы: +2`, `Уровень: 2` and `Опыт: 10`.
6. Confirm technical details show stat bonus `2`, equipment bonus `2`, total additional damage `4`, attributes/progression summaries and a capability plan with 20/16/20 planned/checkpoint/final actions.
7. Save, close and reopen; confirm selections and values persist, then rebuild and compare hashes/replay markers.

Goal149 and Goal150 remain accepted=false until the owner records an explicit
bundled human acceptance decision.
