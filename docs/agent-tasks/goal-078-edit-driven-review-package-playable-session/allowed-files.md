# Allowed files / areas

You may change only these areas unless a stop condition requires BLOCKED/FAILED:

- `docs/agent-tasks/goal-078-edit-driven-review-package-playable-session/**`
- `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/**`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `src/LLMGameCreator.Application/Design/EditDrivenReviewPackagePlayableSession/**`
- `src/LLMGameCreator.WinForms/CompositionRoot.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignReviewPackagePlaySessionControl.cs`
- `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignReviewPackagePlaySessionControl.Designer.cs`
- `tests/LLMGameCreator.Tests/Application/EditDrivenReviewPackagePlayableSession/**`
- `tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenReviewPackagePlayableSessionProductSmokeTests.cs`

Historical artifact handling exception:

- Running old evidence tests can regenerate historical `.llmgc/procedural/goal-074/**`, `goal-075/**`, `goal-076/**`, or `goal-077/**` files. Do not commit that noise unless Goal 078 explicitly needs a lineage update. Prefer exact-path `git restore --source=HEAD -- <historical artifact path>` after validation.

Expected artifact scope:

- The final `check-artifact-scope.ps1 -Scenario goal-078-edit-driven-review-package-playable-session` must pass.
