# Capability Composer v2 Russian Glossary

This document defines user-facing Russian names and explanations for Capability Picker / Composer options.

## UI wording principles

- Keep machine ids in technical/details view only.
- Show Russian readable names in the primary UI.
- Every option must have Russian name, short explanation, examples, best-used-for, compatibility notes, and implementation status.

## Examples

### presentation_mode/map_and_panel_rpg

User-facing name: Карта + панельная RPG

Description: игра, где игрок перемещается по карте/регионам/узлам, а события, сцены, диалоги и бой отображаются через панели интерфейса.

Good for: text RPG, exploration RPG, narrative RPG, region graph worlds.

Usually not good for: direct first-person action, full 3D free movement, infinite tile streaming as primary world model.

### world_topology/region_graph

User-facing name: Граф регионов

Description: мир состоит из регионов и связей между ними. Внутри региона могут быть сцены, события, поселения, ресурсы и переходы.

### progression_model/perk_tree

User-facing name: Дерево перков

Description: игрок открывает улучшения в виде веток перков.

Important: this should be a module, not the only progression choice. It can coexist with level-up stat allocation, skill XP, class trees, faction ranks, and metamodule growth.

### combat_model/dialogue_combat

User-facing name: Диалоговый бой

Description: бой строится как серия выборов, проверок, эффектов и последствий через диалоговые/текстовые узлы.

### combat_time_mode/hybrid_realtime_turn_toggle

User-facing name: Гибрид: реалтайм + пошаговый режим

Description: игрок может переключаться между реальным временем и пошаговым режимом, как в некоторых старых партийных RPG.

Example: Might and Magic VII-style combat.

### economy_model/simulation_light

User-facing name: Лёгкая симуляция экономики

Description: цены и доступность товаров зависят от региона, редкости, репутации, фракций и событий, но без тяжёлой экономической симуляции.

### balance/no_player_rubberbanding

User-facing name: Без прямой подстройки мира под игрока

Description: мир не делает всех врагов слабее только потому, что игрок слаб. Баланс строится через зоны опасности, регионы, фракции, редкость и прогрессию.
