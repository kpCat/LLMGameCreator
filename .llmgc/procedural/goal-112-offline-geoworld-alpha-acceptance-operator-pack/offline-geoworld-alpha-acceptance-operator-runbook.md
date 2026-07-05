# Goal 112 Offline Geoworld Alpha Acceptance Operator Runbook

This operator pack is readiness tooling only. It does not accept the Alpha slice, does not create a real human result, and is not final release packaging.

## Active Gate

- manualGate: offline_geoworld_alpha_manual_acceptance_verification
- operatorStatus: OPERATOR_READY_PENDING_HUMAN_RUN
- goal111DecisionStatus: BLOCKED_PENDING_MANUAL_RESULT
- acceptedByCodex: false
- humanAcceptanceStillRequired: true

## Unity Runner

- unityProjectPath: unity/LLMGameCreatorAlpha
- runnerWindow: unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaAcceptanceRunnerWindow.cs
- resultModel: unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResult.cs
- resultStore: unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResultStore.cs

## Result Paths

- preferredManualResultPath: .llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
- alternateCandidatePaths:
  - .llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
  - .llmgc/procedural/goal-111-offline-geoworld-alpha-manual-result-intake/input/offline-geoworld-alpha-acceptance-result.json
  - unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal110/offline-geoworld-alpha-acceptance-result.json

## Human Run Rules

- A human must run the Goal110 checklist and decide the gate.
- accepted=true is valid only when every required checklist step passed, there are no failed/pending/skipped/missing/malformed/duplicate/unknown steps, and the checklist hash matches Goal110.
- accepted=false remains blocked for acceptance even if the JSON is well-formed.
- failed, pending, skipped, malformed, duplicate, missing, unknown or hash-mismatched steps are not acceptance.
- A pending template copy is not a real manual result.

## Do Not Start Yet

- live geodata
- providers
- Runtime consumer
- public schema
- Lua
- generator-library
- final art
- atlas
- scene/prefab/project settings
- release packaging

## Next Human Actions

- Open unity/LLMGameCreatorAlpha in Unity.
- Open LLMGameCreator/Offline Geoworld Alpha Acceptance Runner.
- Run the Goal110 checklist manually and record every required step.
- Write the real result JSON to .llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
- Re-run the Goal111 intake verifier and review the decision before deciding the gate.
