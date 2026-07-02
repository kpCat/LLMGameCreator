# Allowed files / areas

Allowed to change:

- docs/agent-tasks/goal-077-edit-driven-review-package-materialization/**
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/CONTEXT_INDEX.md
- docs/FULL_GENERATOR_GOAL_QUEUE.md
- docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
- .devflow/artifact-scope/artifact-scope-policy.json
- .llmgc/procedural/goal-077-edit-driven-review-package-materialization/**
- src/LLMGameCreator.Application/Design/EditDrivenPlayableReviewPackageMaterialization/**
- src/LLMGameCreator.WinForms/CompositionRoot.cs
- src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs
- src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs
- src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignReviewPackageControl.cs
- src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignReviewPackageControl.Designer.cs
- tests/LLMGameCreator.Tests/Application/EditDrivenPlayableReviewPackageMaterialization/**
- tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenPlayableReviewPackageMaterializationProductSmokeTests.cs

Allowed to read but not change unless already listed above:

- src/LLMGameCreator.Application/Design/EditDrivenPlayablePreviewRefresh/**
- src/LLMGameCreator.Application/Design/SchemaDrivenCampaignEditValidateApplyLoop/**
- src/LLMGameCreator.Application/Design/SchemaDrivenCampaignAuthoringReviewWorkspace/**
- src/LLMGameCreator.Application/Design/FullCampaignGamePackageMaterialization/**
- src/LLMGameCreator.Application/Design/UnityAlphaInteractiveCampaignPlayer/**
- existing tests used as analogs

Bounded exception:

- If artifact scope policy blocks the new Goal 077 artifacts/task pack, update only the minimal scenario allowlist for Goal 077.
- If existing historical Goal 075/076 tests regenerate their tracked artifacts during validation, do not commit historical regenerated noise. Restore exact historical artifact diffs and explain it in the final report.
