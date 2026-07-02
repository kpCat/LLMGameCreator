# Allowed files / areas

You may change only these areas:

- New Application seam:
  - `src/LLMGameCreator.Application/Design/EditDrivenGamePackageRuntimePreviewBridge/**`

- Campaign Authoring Review Workspace WinForms integration:
  - `src/LLMGameCreator.WinForms/CompositionRoot.cs`
  - `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs`
  - `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs`
  - new files under `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/` whose names start with `CampaignGamePackageRuntimePreviewBridge`

- Tests:
  - `tests/LLMGameCreator.Tests/Application/EditDrivenGamePackageRuntimePreviewBridge/**`
  - `tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenGamePackageRuntimePreviewBridgeProductSmokeTests.cs`
  - narrowly scoped updates to an existing docs/current-state test only if required by the docs quartet

- Goal 080 artifacts:
  - `.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/**`

- Current state/docs/debt/artifact scope:
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
  - `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
  - `.devflow/artifact-scope/artifact-scope-policy.json`

- The unpacked task pack:
  - `docs/agent-tasks/goal-080-edit-driven-gamepackage-runtime-preview-bridge/**`

Historical artifact restore exception:
- Focused/product smoke tests for Goal 074-079 may regenerate historical artifact files. Do not commit that churn unless this GOAL explicitly asks for it. You may restore only exact regenerated historical artifact paths back to HEAD after validation, then regenerate the final Goal 080 artifacts and rerun scope.

Read-only reference areas:
- `src/LLMGameCreator.Domain/**`
- existing `src/LLMGameCreator.Application/**GamePackage**`
- existing `src/LLMGameCreator.Application/**Package**`
- existing `src/LLMGameCreator.Application/**RuntimePreview**`
- existing `src/LLMGameCreator.WinForms/Pages/RuntimePreview/**`
- existing Unity/player/materialization analogs
