# Allowed files and areas

Primary allowed areas:

- `src/LLMGameCreator.Application/Design/EditDrivenUnityAlphaStreamingAssetsHandoff/**`
- `tests/LLMGameCreator.Tests/Application/EditDrivenUnityAlphaStreamingAssetsHandoff/**`
- `tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenUnityAlphaStreamingAssetsHandoffProductSmokeTests.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignUnityAlphaStreamingAssetsHandoffControl.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignUnityAlphaStreamingAssetsHandoffControl.Designer.cs`
- `src/LLMGameCreator.WinForms/CompositionRoot.cs`
- `.llmgc/procedural/goal-082-edit-driven-unity-alpha-streamingassets-handoff/**`
- `docs/agent-tasks/goal-082-edit-driven-unity-alpha-streamingassets-handoff/**`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.devflow/artifact-scope/artifact-scope-policy.json`

Explicit bounded Unity allowlist:

- `unity/LLMGameCreatorAlpha/Assets/Scripts/EditDrivenGamePackageHandoffProbe.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/EditDrivenGamePackageHandoffProbe.cs.meta` if Unity meta files are tracked/needed by the repo convention.
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/EditDrivenGoal082/**`
- matching `.meta` files under that StreamingAssets subtree if Unity meta files are tracked/needed by the repo convention.

Historical artifact restore exception:

- If validation regenerates historical `.llmgc/procedural/goal-074` through `goal-081` artifacts outside this goal's allowlist, you may use `git restore --source=HEAD -- <exact historical artifact paths>` only for those regenerated historical files. Do not use broad restore/reset/clean/stash.
