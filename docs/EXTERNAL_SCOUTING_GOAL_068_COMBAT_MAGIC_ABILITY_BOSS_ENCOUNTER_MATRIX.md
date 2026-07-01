# External scouting — Goal 068 Combat/Magic/Ability/Boss Encounter Matrix

## Decision

Do **not** add external dependencies in Goal 068.

Goal 068 must implement a BCL-only Application-layer proof for combat, magic, ability, status, boss/elite encounters and progression consequences. The purpose is to prove deterministic state-changing generated gameplay depth across the existing 3 families × 3 seeds matrix, not to adopt a full Unity ability framework.

## Scouted references

### Unity Gameplay Ability System style frameworks

- `sjai013/unity-gameplay-ability-system`
  - Useful reference for data-driven abilities, attributes, effects, cooldowns and costs.
  - Risk: Unity-facing framework and architecture commitment. It would pull the project toward Unity runtime ability implementation before the generator contracts are stable.
  - Decision: do not add. Use conceptually as reference only.

- `Unity3D-Projects/UnityGameplayAbilitySystem`
  - Similar GAS-style Unity approach.
  - Useful later if Unity runtime ability presentation becomes a concrete adapter target.
  - Decision: defer.

- `PhysaliaStudio/Flexi`
  - Unity ability system framework inspired by Unreal GAS.
  - Useful reference for separation between ability definitions, activation, effects and attributes.
  - Decision: defer.

### ScriptableObject ability/status patterns

Unity ScriptableObject-based ability/status systems are common and useful for later editor/runtime presentation, but Goal 068 should stay Application-only and JSON/evidence-friendly.

Decision:
- no ScriptableObject authoring;
- no Unity asset database mutation beyond the already established narrow `AlphaRuntimeBootstrap.cs` marker loader pattern;
- no Unity package dependency.

## In-house design direction

Goal 068 should introduce deterministic domain records for:

- combatants;
- attributes;
- resources;
- active abilities;
- passive traits;
- cooldowns;
- costs;
- damage/effect packets;
- status effects;
- resistances/weaknesses;
- boss/elite phase plans;
- encounter rounds;
- counterplay opportunities;
- loot/progression consequences;
- save/load/replay proof.

The system should remain programmatic and generated from existing Goal 060–067 evidence. It must not use LLM final prose and must not depend on external runtime ability systems.

## Future adapter note

After generator contracts stabilize, a future Unity runtime adapter may map these records to ScriptableObjects, ECS, or a GAS-like system. That adapter is explicitly out of scope for Goal 068.
