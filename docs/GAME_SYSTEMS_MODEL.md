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
