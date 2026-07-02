# Allowed files / areas

Primary implementation areas:

- src/LLMGameCreator.Application/Design/EditDrivenPlayablePreviewRefresh/**
- tests/LLMGameCreator.Tests/Application/EditDrivenPlayablePreviewRefresh/**
- tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenPlayablePreviewRefreshProductSmokeTests.cs
- src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/**
- src/LLMGameCreator.WinForms/CompositionRoot.cs, only if needed to register/bind the new WinForms surface
- .llmgc/procedural/goal-076-edit-driven-playable-preview-refresh/**
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/CONTEXT_INDEX.md
- docs/FULL_GENERATOR_GOAL_QUEUE.md
- docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md, if present or if needed for residual P2/P3 debt
- .devflow/artifact-scope/artifact-scope-policy.json, only if needed for the new Goal 076 artifacts

Scoped optional Unity areas:

- Prefer read-only inspection of Unity/player code.
- If and only if Goal 076 cannot prove staged player handoff without a bounded code change, you may add a small, generic, data-driven reader/proof helper under the existing Unity/player area.
- Do not add game-specific logic to Unity.
- Do not bloat AlphaRuntimeBootstrap.cs. Prefer a new helper/class over editing AlphaRuntimeBootstrap.cs. If AlphaRuntimeBootstrap.cs must be edited, keep the delta tiny and justify it in the final report.

Documentation areas for preflight inventory only:

- README_ADAPTIVE_GENERATOR_HANDOFF.md
- README_VISUAL_WORLD_GENERATION_HANDOFF.md
- docs/agent-tasks/** adaptive/visual planning docs
- docs/context/** adaptive/visual planning docs
- docs/proposals/** adaptive/visual planning docs

For these docs, do not rewrite or reorganize them unless they directly break validation. If they are merely unindexed strategic docs, record P2/P3 debt and move on.
