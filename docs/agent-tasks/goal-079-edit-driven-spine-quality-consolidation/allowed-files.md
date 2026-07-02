# Allowed files / areas

Allowed changes are strictly limited to the following:

- New Application namespace:
  - `src/LLMGameCreator.Application/Design/EditDrivenSpineQualityConsolidation/**`
- WinForms workspace integration:
  - `src/LLMGameCreator.WinForms/CompositionRoot.cs`
  - `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs`
  - `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs`
  - new separate UserControl files under `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/`, preferably named `CampaignEditDrivenSpineQualityControl.cs` and `.Designer.cs`
- Tests:
  - `tests/LLMGameCreator.Tests/Application/EditDrivenSpineQualityConsolidation/**`
  - `tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenSpineQualityConsolidationProductSmokeTests.cs`
  - narrowly updated existing Goal 074-078 product-smoke tests only if needed to expose a real shared acceptance-proof issue; avoid this if possible.
- Goal 079 evidence:
  - `.llmgc/procedural/goal-079-edit-driven-spine-quality-consolidation/**`
- Task pack itself:
  - `docs/agent-tasks/goal-079-edit-driven-spine-quality-consolidation/**`
- Current state and queue docs:
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
  - `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- Artifact scope policy:
  - `.devflow/artifact-scope/artifact-scope-policy.json`

Historical artifact restore exception:

- Running regression tests may regenerate historical Goal 074-078 artifact files.
- If that happens, inspect the diff.
- If the diff is regeneration noise unrelated to Goal 079, restore only the exact historical artifact paths with `git restore --source=HEAD -- <exact paths>`.
- Do not restore broad directories.
- Do not hide a real regression.
