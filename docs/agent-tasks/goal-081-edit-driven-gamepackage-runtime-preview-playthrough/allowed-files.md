# Allowed files / areas

Allowed to add or modify:

- `.devflow/artifact-scope/artifact-scope-policy.json`
- `.llmgc/procedural/goal-081-edit-driven-gamepackage-runtime-preview-playthrough/**`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `docs/agent-tasks/goal-081-edit-driven-gamepackage-runtime-preview-playthrough/**`
- `src/LLMGameCreator.Application/Design/EditDrivenGamePackageRuntimePreviewPlaythrough/**`
- `src/LLMGameCreator.WinForms/CompositionRoot.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignGamePackageRuntimePreviewPlaythroughControl.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignGamePackageRuntimePreviewPlaythroughControl.Designer.cs`
- `tests/LLMGameCreator.Tests/Application/EditDrivenGamePackageRuntimePreviewPlaythrough/**`
- `tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenGamePackageRuntimePreviewPlaythroughProductSmokeTests.cs`

Read-only reference areas:

- Existing Goal 080 Application seam and artifacts.
- Existing GamePackage validation/runtime-preview/interaction-preview Application code, but do not change it unless the task explicitly blocks without a tiny allowed adapter in the new Goal 081 namespace.

Historical artifact cleanup exception:

- Validation runs may regenerate historical `.llmgc/procedural/goal-074` through `goal-080` artifacts. You may restore those exact historical artifact paths with `git restore --source=HEAD -- <exact paths>` only to remove regenerated noise outside the Goal 081 allowlist.
- Do not use broad restore/reset/clean/stash.
