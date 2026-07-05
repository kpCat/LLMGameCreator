# Goal 113 Offline Geoworld Alpha Manual Result Workbench Runbook

This workbench is authoring and review tooling only. It does not accept the Alpha gate and does not create the real manual result.

## Active Gate

- manualGate: offline_geoworld_alpha_manual_acceptance_verification
- workbenchStatus: WORKBENCH_READY_PENDING_HUMAN_RESULT
- goal111DecisionStatus: BLOCKED_PENDING_MANUAL_RESULT
- goal112OperatorStatus: OPERATOR_READY_PENDING_HUMAN_RUN
- acceptedByCodex: false
- humanAcceptanceStillRequired: true

## Result Paths

- preferredManualResultPath: .llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
- draftTemplatePath: .llmgc/procedural/goal-113-offline-geoworld-alpha-manual-result-workbench/offline-geoworld-alpha-manual-result-workbench-draft-template.json
- draftTemplateOnly: true
- doesNotWritePreferredManualResultPath: true
- candidateManualResultPaths:
  - .llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
  - .llmgc/procedural/goal-111-offline-geoworld-alpha-manual-result-intake/input/offline-geoworld-alpha-acceptance-result.json
  - unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal110/offline-geoworld-alpha-acceptance-result.json

## Checklist

- checklistHash: 106b43dad61c0bf9dafde965a712cf8e370402b6f9d5664352bcac549ebeae41
- checklistStepCount: 12
- 1. open_unity_project - Open Unity project
- 2. open_alpha_slice_window - Open Alpha Slice window
- 3. setup_rig - Setup rig
- 4. verify_package - Verify package
- 5. run_travel - Run travel
- 6. run_interaction - Run interaction
- 7. save_snapshot - Save snapshot
- 8. load_snapshot - Load snapshot
- 9. replay - Replay
- 10. complete_objectives - Complete objectives
- 11. run_package_verifier - Run package verifier
- 12. record_diagnostics - Record diagnostics

## Human Run Rules

- Copy the Goal113 draft only as a starting point.
- Save the real manually-created JSON only at the preferred manual result path.
- Every required step must appear exactly once with status passed.
- Duplicate, missing, unknown, failed, pending, skipped or malformed steps are rejected.
- A valid candidate still requires explicit human gate acceptance.

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
- Use the Goal113 draft template only as a copy/edit starting point.
- Run every Goal110 checklist step manually and record real evidence.
- Write the real result JSON to .llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
- Re-run Goal111, Goal112 and Goal113 validation before deciding the manual gate.
