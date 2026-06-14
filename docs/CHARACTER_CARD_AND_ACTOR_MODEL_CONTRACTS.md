# Character Card And Actor Model Contracts

Status: future artifact contract plan  
Scope: character card families, actor models and review gates  
Non-scope: generated classes, production code, schema migration, UI implementation

Character cards are data contracts. They are not generated C# classes and do not authorize new runtime behavior by themselves.

## Contract Families

The future artifact contract ids are:

- `character_card_v1`
- `player_character_card_v1`
- `party_member_card_v1`
- `companion_card_v1`
- `npc_card_v1`
- `enemy_card_v1`
- `boss_card_v1`
- `vendor_card_v1`
- `faction_leader_card_v1`
- `party_roster_v1`
- `actor_model_profile_v1`

## Shared Card Fields

Every card family should declare these fields when applicable:

- `identity`: stable id, source artifact id and version.
- `display_name`: player-facing name.
- `species`, `archetype`, `origin`: typed semantic descriptors.
- `role`: gameplay, narrative and party role.
- `presentation_refs`: portrait refs, sprite refs, billboard refs and fallback ids.
- `stats`: stat ids, base values, bounds and derived formula refs.
- `skills`: skill ids, ranks, tags and trainer/progression hooks.
- `traits`: semantic traits, tags and state channel refs.
- `faction`: faction id, reputation defaults and alignment hooks.
- `relationships`: relationship refs and starting values.
- `dialogue_style`: speech style, tone and dialogue pack refs.
- `combat_role`: role, ability refs, target preferences and frontline/backline notes.
- `progression_hooks`: xp, skill use, perk, class, relationship or reputation hooks.
- `inventory_equipment`: inventory model, equipment model, starting items and restrictions.
- `schedule_behavior`: schedule refs, NPC behavior profile and pathfinding profile.
- `quest_hooks`: quest giver, quest target, companion route or boss encounter refs.
- `semantic_memory_hooks`: memory tags, remembered facts and relationship memory refs.
- `validation_gates`: schema, ids, refs, bounds, compatibility and approval gates.
- `promotion_target`: package field, runtime DB group, Unity IR group or blocked status.

## Family Notes

| Contract | Purpose | Required review |
|---|---|---|
| `character_card_v1` | Generic base card shape for shared fields. | Schema/id compatibility review. |
| `player_character_card_v1` | Player avatar or created protagonist. | Human approval required before canon/player promotion. |
| `party_member_card_v1` | Party member with inventory, equipment and progression hooks. | Human approval for roster membership. |
| `companion_card_v1` | Companion with relationship, dialogue and route hooks. | Human approval for companion canon. |
| `npc_card_v1` | Non-player actor card. | Review required when quest/faction/dialogue refs are canonical. |
| `enemy_card_v1` | Enemy archetype or encounter participant card. | Combat/stat bounds validation required. |
| `boss_card_v1` | Important enemy with narrative and combat significance. | Human approval required. |
| `vendor_card_v1` | Merchant/service NPC. | Inventory, transaction and faction refs validation required. |
| `faction_leader_card_v1` | Leader tied to faction/political state. | Human approval required. |
| `party_roster_v1` | Party composition, formation, shared inventory and active members. | Party compatibility validation required. |
| `actor_model_profile_v1` | Declares how actors are controlled and represented. | Compatibility review with presentation/world/combat ids. |

## Actor Models

| Id | Meaning | Card implications |
|---|---|---|
| `actor_model/single_player_character` | One main controlled avatar. | Uses `player_character_card_v1`; optional companion/NPC cards. |
| `actor_model/party_blob` | Party moves as one exploration actor. | Requires `party_roster_v1`; members use card contracts. |
| `actor_model/party_individuals` | Party members have individual map/combat presence. | Requires roster, per-member refs and combat-space placement. |
| `actor_model/controllable_squad` | Squad/unit control. | Requires unit cards, roles, ability refs and tactical validators. |
| `actor_model/colony_population` | Many citizens/workers. | Requires population archetype cards, job/schedule behavior and scalable validation. |
| `actor_model/vehicle_or_ship` | Vehicle or ship is the main actor. | Requires vehicle card/profile, crew slots and storage/equipment refs. |
| `actor_model/god_controller` | Player controls systems rather than one avatar. | Requires policy/system cards rather than avatar assumptions. |

## Responsibility Contract

- LLM may draft cards as contract-bound JSON.
- C# validates ids, refs, bounds, compatibility and promotion rules.
- Lua may expand approved cards into runtime-ready IR/config through sandboxed generator modules later.
- Human approval is required for canon/player/companion/boss/faction-leader cards.
- Runtime owns mutable card state in runtime/save state, not inside immutable source card contracts.

## Non-Goals

- no generated C# classes;
- no hidden runtime logic in card text;
- no direct GamePackage mutation from draft cards;
- no portrait/sprite generation in this task;
- no assumption that every game has a single humanoid player avatar.
