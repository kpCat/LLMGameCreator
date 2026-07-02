# Allowed files / areas

Primary allowed edits:

- src/LLMGameCreator.Application/Design/EditDrivenSpineQualityConsolidation/**
- src/LLMGameCreator.WinForms/CompositionRoot.cs only if required by generated evidence/service registration consistency; prefer no change.
- src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs
- src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignEditDrivenSpineQualityControl.cs
- src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignEditDrivenSpineQualityControl.Designer.cs
- tests/LLMGameCreator.Tests/Application/EditDrivenSpineQualityConsolidation/**
- tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenSpineQualityConsolidationProductSmokeTests.cs
- .llmgc/procedural/goal-079-edit-driven-spine-quality-consolidation/** when regenerating Goal 079 evidence after strengthening the scanner.
- .llmgc/procedural/goal-079a-source-format-line-ending-guard/** for hotfix-specific evidence.
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/CONTEXT_INDEX.md
- docs/FULL_GENERATOR_GOAL_QUEUE.md
- docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
- .devflow/artifact-scope/artifact-scope-policy.json
- docs/agent-tasks/goal-079a-source-format-line-ending-guard/**

Bounded formatting-only allowance:

- You may normalize line endings and readable formatting in C# files under these already-related areas if your raw-byte scan proves they have CR-only/no-LF/one-physical-line source-format debt:
  - src/LLMGameCreator.Application/Design/SchemaDrivenCampaignAuthoringReviewWorkspace/**
  - src/LLMGameCreator.Application/Design/SchemaDrivenCampaignEditValidateApplyLoop/**
  - src/LLMGameCreator.Application/Design/EditDrivenPlayablePreviewRefresh/**
  - src/LLMGameCreator.Application/Design/EditDrivenPlayableReviewPackageMaterialization/**
  - src/LLMGameCreator.Application/Design/EditDrivenReviewPackagePlayableSession/**
  - src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/**
  - tests/LLMGameCreator.Tests/Application/SchemaDrivenCampaignEditValidateApplyLoop/**
  - tests/LLMGameCreator.Tests/Application/EditDrivenPlayablePreviewRefresh/**
  - tests/LLMGameCreator.Tests/Application/EditDrivenPlayableReviewPackageMaterialization/**
  - tests/LLMGameCreator.Tests/Application/EditDrivenReviewPackagePlayableSession/**
  - tests/LLMGameCreator.Tests/Application/EditDrivenSpineQualityConsolidation/**
  - tests/LLMGameCreator.Tests/ProductSmoke/*EditDriven*.cs

The bounded formatting-only allowance must not change behavior. If a change is more than line-ending/readability normalization, it is forbidden unless it is in the primary Goal 079A scanner/test/evidence code.
