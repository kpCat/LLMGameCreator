# Dynamic Semantic Feature System Report

- accepted: false
- finalStatus: dynamic_semantic_feature_system_verification
- manualGate: dynamic_semantic_feature_system_verification
- required marker: dynamic_semantic_feature_system_verification required
- previousProducedGate: semantic_pack_composition_blueprint_verification required
- productSmokeRoute: goal-032-dynamic-semantic-feature-system
- contractProofPassed: true
- featureCount: 23
- influenceRuleCount: 5
- scenarioCount: 4
- invalidMatrixPassed: true
- featureCatalogSummaryHash: 072c095cf11bba2b7d440be36c4e0fee464583cb4aec426ba3a13f070ddb208a
- influenceRuleSummaryHash: f098074880d26a062a5ba2f2952dd976489f8aff3e48c9db084534aaf4d69efd
- authoringSchemaMatrixHash: 648a50e8bbe9db30eb906c90f1ed951948a7a245ac98a7f2dd101c53aedf5172
- invalidMatrixHash: 9b04cb24a34f64cf93412e082fcea4f2c33bc74cea5ad343cd8c409d21b2c312
- reportHash: a81e50ed5c66ed80019259b43108d95321a37d721ff22501073196f9f71c59bf

## What became more real

Semantic variability now has an Application-layer feature, inheritance, influence and authoring-schema kernel. LLM can remain a seed/lore drafting helper while deterministic C# resolves NPC, faction, quest, dialogue, species/archetype and kingdom pressure combinations.

## Scenarios

- caravan_trade: targets=3, summary=caravan_trade|seed=3203|targets=3|features=11|diagnostics=0
- frontier_survival: targets=4, summary=frontier_survival|seed=3201|targets=4|features=23|diagnostics=0
- gothic_intrigue: targets=3, summary=gothic_intrigue|seed=3202|targets=3|features=18|diagnostics=0
- metamodule_kingdoms: targets=5, summary=metamodule_kingdoms|seed=3204|targets=5|features=25|diagnostics=0

## Invalid/fake/leak matrix

- circular_influence: expectedValid=false, actualValid=false, codes=dynamic_semantic.influence.circular
- circular_inheritance: expectedValid=false, actualValid=false, codes=dynamic_semantic.inheritance.circular
- duplicate_feature_id: expectedValid=false, actualValid=false, codes=dynamic_semantic.feature_id.duplicate
- fake_selected_feature_id: expectedValid=false, actualValid=false, codes=dynamic_semantic.target.unknown
- feature_conflict: expectedValid=false, actualValid=false, codes=dynamic_semantic.feature.conflict
- forbidden_leakage_terms: expectedValid=false, actualValid=false, codes=dynamic_semantic.boundary.leakage
- illegal_assignment_scope: expectedValid=false, actualValid=false, codes=dynamic_semantic.assignment.scope_illegal,dynamic_semantic.value_shape.invalid
- invalid_empty_id: expectedValid=false, actualValid=false, codes=dynamic_semantic.feature_id.invalid
- invalid_value_shape: expectedValid=false, actualValid=false, codes=dynamic_semantic.value_shape.invalid
- optional_feature_missing_is_traceable: expectedValid=true, actualValid=true, codes=
- overconstrained_output: expectedValid=false, actualValid=false, codes=dynamic_semantic.influence.self_feeding,dynamic_semantic.output.overconstrained
- required_feature_missing: expectedValid=false, actualValid=false, codes=dynamic_semantic.required_feature.missing
- self_feeding_influence: expectedValid=false, actualValid=false, codes=dynamic_semantic.influence.self_feeding
- unknown_feature_reference: expectedValid=false, actualValid=false, codes=dynamic_semantic.feature_ref.unknown
- unknown_influence_target: expectedValid=false, actualValid=false, codes=dynamic_semantic.influence.target.unknown
- unknown_inheritance_source: expectedValid=false, actualValid=false, codes=dynamic_semantic.inheritance.source.unknown
- unknown_target_scope: expectedValid=false, actualValid=false, codes=dynamic_semantic.assignment.scope_mismatch,dynamic_semantic.scope.unknown

## Boundaries

- publicGamePackageSchemaChanged: false
- runtimeBehaviorChanged: false
- unityBuildExecuted: false
- llmRagProviderMediaLuaExecuted: false
