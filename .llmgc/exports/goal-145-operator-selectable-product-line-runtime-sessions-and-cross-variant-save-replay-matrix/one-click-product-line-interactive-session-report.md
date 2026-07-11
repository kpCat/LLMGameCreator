# Goal 145 Product-Line Interactive Runtime Session Matrix

Status: GREEN

- candidateCount: 4
- passedCandidateCount: 4
- failedCandidateCount: 0
- distinctFinalStateHashCount: 4
- activeSelectedCandidateId: `minimal-map-game-exploration-resource-focus`
- allCandidateCheckpointReloadsPassed: true
- allCandidateFullReplaysEquivalent: true
- allCandidateActionBindingsPassed: true
- allFocusEffectsObserved: true
- operatorUsesInProcessService: true
- runtimeAuthority: true
- projectionOnly: false
- unityGameplayTruth: false
- goal144Accepted: true
- goal145Accepted: false

## Fresh focus comparisons

- `minimal-map-game-alchemy-focus` / inventory: baseline=`inventory/chest_start=item/apple:2,item/rusty_knife:1; inventory/player_start=item/apple:3,item/fuel_can:1,item/healing_potion:3,item/log:1,item/woodcutting_axe:1`; candidate=`inventory/chest_start=item/apple:2,item/rusty_knife:1; inventory/player_start=item/apple:3,item/fuel_can:1,item/healing_potion:4,item/log:1,item/red_herb:2,item/water_flask:1,item/woodcutting_axe:1`; observed=true
- `minimal-map-game-balanced-baseline` / control: baseline=`29c99098d25aa934b72a06063d82b5bf44b6454cb7195a178ef959a6224b95c2`; candidate=`29c99098d25aa934b72a06063d82b5bf44b6454cb7195a178ef959a6224b95c2`; observed=true
- `minimal-map-game-combat-focus` / combat: baseline=`encounter/goblin_duel:round=1:turn=1:active=True:participants=goblin[alive=True;resource/health=8],player[alive=True;resource/health=30|resource/stamina=10]`; candidate=`encounter/goblin_duel:round=1:turn=1:active=True:participants=goblin[alive=True;resource/health=10],player[alive=True;resource/health=30|resource/stamina=10]`; observed=true
- `minimal-map-game-exploration-resource-focus` / inventory: baseline=`inventory/chest_start=item/apple:2,item/rusty_knife:1; inventory/player_start=item/apple:3,item/fuel_can:1,item/healing_potion:3,item/log:1,item/woodcutting_axe:1`; candidate=`inventory/chest_start=item/apple:2,item/rusty_knife:1; inventory/player_start=item/apple:4,item/fuel_can:1,item/healing_potion:4,item/log:2,item/woodcutting_axe:1`; observed=true
