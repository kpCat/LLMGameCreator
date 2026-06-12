# Game Systems Model

Документ фиксирует универсальную модель игровых систем `LLMGameCreator`.

Цель — не сделать одну жёсткую RPG-систему, а заложить основу, где одна игра может быть простой сценовой текстовкой, другая — 2D/изометрической RPG с картой, NPC, квестами, инвентарём, боёвкой, Lua-генераторами, звуком и ассетами.

## Главный принцип

В ядре не должно быть обязательных полей вроде `Gold`, `Mana`, `Experience`, `Level`, `Strength`.

Плохо:

```csharp
Player.Gold
Player.Mana
Player.Experience
Player.Level
```

Правильно:

```csharp
GameState.Resources["resource/gold"]
GameState.Resources["resource/mana"]
GameState.Progressions["progression/character_level"]
GameState.Stats["stat/strength"]
```

Игра сама объявляет, какие системы ей нужны. Если игра не использует деньги, маны, уровни, опыт или боёвку — этих систем в `GamePackage` нет.

## Слои

```text
Definition Layer:
  StatDefinition
  ResourceDefinition
  AbilityDefinition
  AbilityStageDefinition
  ProgressionDefinition
  InteractionDefinition
  FormulaDefinition
  ConditionDefinition
  EffectDefinition

State Layer:
  EntityState
  StatValue
  ResourceValue
  AbilityState
  ProgressionState
  InventoryState
  EquipmentState
  ActiveStatusEffectState

Execution Layer:
  RuntimeCommand
  RuntimeSystem
  ConditionEvaluator
  EffectExecutor
  FormulaEvaluator
  ScriptEngine

Validation Layer:
  ReferenceValidation
  FormulaValidation
  AbilityValidation
  ProgressionValidation
  InteractionValidation
  RuntimeSimulationValidation
```

## Stats

`StatDefinition` — любое числовое или категориальное свойство сущности.

Примеры:

```text
stat/strength
stat/agility
stat/sword_skill
stat/fire_magic
stat/stealth
stat/persuasion
stat/relationship/old_guard
stat/fear_resistance
stat/corruption_resistance
```

Стат может относиться к игроку, NPC, фракции, предмету, биому, encounter или миру.

## Resources

`ResourceDefinition` — значение, которое тратится, копится, восстанавливается или потребляется.

Примеры:

```text
resource/health
resource/stamina
resource/mana
resource/gold
resource/food
resource/arrows
resource/focus
resource/rage
resource/action_points
```

Ресурс может быть finite, currency, consumable, regenerating или abstract.

## Economy Bundles

Economy systems use reusable data bundles instead of hardcoded engine fields.

`RequirementDefinition` describes a gate:

```text
kind, id, operator, amount, value, scope, tags, metadata
```

Examples: `has_item`, `resource_at_least`, `stat_at_least`, `flag_equals`, `quest_state`, `faction_reputation_at_least`, `recipe_known`, `station_available`, `network_resource_at_least`, `time_available`.

`CostDefinition` describes something consumed or reserved:

```text
kind, id, amount, scope, consume_mode, tags, metadata
```

Examples: item cost, resource/currency cost, mana/stamina/stat cost, time cost, durability/charge cost, faction reputation, quest token, base electricity/water/fuel.

`OutputDefinition` describes something produced or granted:

```text
kind, id, amount, scope, mode, tags, metadata
```

Examples: give item/resource, set flag, unlock recipe, add status, progression gain, reputation change, spawn entity, add loot roll, add base resource.

## Recipes, Crafting, Alchemy and Production

`RecipeDefinition` is data-only and supports crafting, alchemy, cooking, smithing, rituals, research, base production, repair and upgrades. Recipes declare requirements, item/resource inputs, additional costs, outputs, failure outputs, optional station id, duration, cooldown and success chance.

The runtime simulation for crafting is not implemented in this layer. Validators only check ids, numeric ranges and references.

## Loot Tables

`LootTableDefinition` contains weighted `LootEntryDefinition` records. Entries point to an `OutputDefinition`, may declare requirements, rarity, min/max counts, quest item and unique flags, max global count, and optional flag hooks.

Quest loot and unique loot are data contracts. A unique quest entry with `max_global_count = 1` is valid; duplicate guaranteed unique outputs are surfaced as diagnostics.

## Transactions

`TransactionDefinition` models shops, barter, services, training, repair, upgrades, rent, bribes, tribute, quest exchange, faction vendors and black markets. Transactions declare requirements, costs, outputs, optional vendor id, stock loot table id and restock rule.

No shop or transaction runtime is implemented here; data with known references validates, while not-yet-handled output systems can surface warnings.

## Resource Networks and Nodes

Base systems such as electricity, water, heat, fuel, oxygen, steam, mana flow, pressure, waste and data networks are modeled as resources plus `ResourceNetworkDefinition`.

`ResourceNodeDefinition` describes producers, consumers, storage, converters, switches, generic network nodes and harvesters. Nodes can reference a network, an entity prototype, production, consumption, storage, conversion inputs/outputs and requirements.

No full base electricity or resource-network simulation is implemented in this layer.

## Inventories and Item Stacks

`InventoryDefinition` declares owner kind/id, slots and starting stacks. `ItemStackDefinition` stores item id, amount, optional unique instance id, quest flag, durability, charge and metadata.

Items remain normal definitions and can optionally describe kind, rarity, max stack, value, weight, quest/unique flags, durability, charge, ammo/fuel type, sell/drop restrictions, requirements and metadata.

## Progression

`ProgressionDefinition` — способ развития чего-либо.

Режимы:

```text
none
xp_level
point_buy
usage_based
milestone_based
item_unlock
teacher_based
quest_unlock
hybrid
```

В одной игре персонаж получает XP и уровни. В другой заклинания изучаются через свитки. В третьей навык владения мечом растёт только от боёв. Runtime не должен предполагать наличие XP.

## Abilities

`AbilityDefinition` — универсальное действие/умение/свойство.

Ability может быть:

```text
active
passive
toggle
reaction
ritual
combat
dialogue
exploration
item_granted
scripted
```

Состояния способности:

```text
unknown
discovered
learnable
learned
mastered
disabled
```

Ability может иметь стадии: novice, adept, master или любые игровые стадии. Каждая стадия может иметь условия открытия, costs, effects, modifiers, formula overrides, cooldown modifiers и asset references.

## Conditions

`ConditionDefinition` — проверка, можно ли выполнить действие, открыть сцену, активировать способность, начать interaction или применить effect.

Типы:

```text
has_item
resource_at_least
stat_at_least
flag_equals
quest_state
ability_learned
progression_stage_at_least
relationship_at_least
time_between
weather_is
entity_nearby
script_condition
```

## Effects

`EffectDefinition` — изменение состояния игры.

Типы:

```text
add_item
remove_item
change_resource
change_stat
set_flag
start_quest
complete_quest
unlock_ability
advance_progression
add_status
remove_status
open_dialogue
change_map
spawn_entity
despawn_entity
play_sound
play_music
run_script
```

Lua не должен напрямую менять `GameState`; Lua возвращает effects/action drafts, Runtime централизованно проверяет и применяет их.

## Interactions

`InteractionDefinition` — универсальная модель взаимодействия.

Interaction может быть:

```text
talk
trade
steal
fight
inspect
use_item_on_target
train
teach
heal
craft
romance
sexual
persuade
intimidate
open_container
harvest_resource
enter_portal
```

Боёвка, торговля, обучение, кража и диалог — разные виды interaction, но используют общие blocks: conditions, costs, checks, effects, outcomes, risks, cooldowns, scripts.

## Combat как Encounter

Боёвка — частный случай `Encounter`.

```text
Encounter:
  participants
  availableActions
  initiative
  abilityRules
  effects
  winConditions
  loseConditions
  rewards
  consequences
```

На этой же модели позже можно строить бой, погоню, спор, ритуал, воровство или survival event.

## Formulas

Формулы используются для урона, лечения, опыта, цен, шансов, вероятности событий, длительности эффектов, восстановления ресурсов и loot scaling.

Формулы от LLM не применяются напрямую:

```text
LLM formula draft
  -> parse
  -> validate references
  -> validate allowed functions
  -> sample evaluation
  -> preview
  -> user approve
  -> apply
```

## Definition of Done для новой игровой системы

Система считается готовой только если есть:

1. definition;
2. state representation;
3. validator;
4. runtime handler или явно documented not implemented;
5. sample в GamePackage;
6. smoke-test;
7. UI/viewer или diagnostic report.
## Gameplay Runtime v1

Gameplay runtime v1 makes the economy definitions executable in a headless deterministic simulation.

Runtime state is stored in `GameRuntimeState`, separate from `GamePackageDefinition`. It contains package id, current map id, player entity id, tick, inventories, resources, flags, statuses, optional quest states and metadata. Runtime state does not hold definition objects.

Runtime v1 supports:

- requirements: `has_item`, `inventory_has`, `resource_at_least`, `network_resource_at_least`, `flag_equals`, `status_present`, `status_active`, `time_available`, `always`;
- costs: item, resource-like costs, time/tick, durability and charge;
- outputs: item, resource-like outputs, status, flag, log message and simple loot-table output rolls;
- recipes: requirements, inputs, costs and outputs are checked/applied through a working state copy;
- loot tables: weighted deterministic rolls use a supplied seed or a stable package/table/tick seed, never global random;
- transactions: shops, barter and services execute as generic requirement/cost/output bundles;
- resource nodes: a simple tick loop evaluates each node, consumes conversion/consumption costs, applies production/storage/conversion outputs and clamps resources by definition min/max.

Runtime v1 deliberately does not implement combat, quest/dialogue execution, full electricity-grid routing, building placement, multiplayer, runtime Lua or generator Lua. Missing fuel/input on a resource node is reported as a runtime diagnostic and the node produces nothing for that tick.
