# Read-first list

Read these before editing:

1. `AGENTS.md`
2. `README.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
8. `.devflow/artifact-scope/artifact-scope-policy.json`
9. `.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/edit-driven-gamepackage-runtime-preview-bridge-report.md`
10. `.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/projected-gamepackage/package.json`
11. `.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/projected-gamepackage/projected-package-index.json`
12. `.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/projected-gamepackage/player-readable-bridge-index.json`
13. `.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/projected-gamepackage/source-targets.json`
14. `.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/runtime-preview-bridge-proof.json`
15. `.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/runtime-preview-negative-proof.json`
16. `.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/quality-gate-scan.json`
17. `src/LLMGameCreator.Application/Design/EditDrivenGamePackageRuntimePreviewBridge/**`
18. `tests/LLMGameCreator.Tests/Application/EditDrivenGamePackageRuntimePreviewBridge/**`
19. `tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenGamePackageRuntimePreviewBridgeProductSmokeTests.cs`
20. `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs`
21. `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs`
22. `src/LLMGameCreator.WinForms/CompositionRoot.cs`

Discovery reads allowed before editing:

- Search for existing GamePackage validator/runtime-preview/interaction-preview seams with `rg -n "GamePackageValidator|RuntimePreview|Interaction|Preview|GamePackage" src tests -g "*.cs"`.
- Read only the minimal files needed to reuse those existing seams.
- If an existing seam is unavailable or requires forbidden changes, stop with BLOCKED and commit/push the BLOCKED report.

Preflight checks:

- Confirm branch is `main`.
- Confirm current `HEAD` equals `origin/main` after `git fetch origin main` unless the user has local changes unrelated to this task.
- Confirm Goal 080 exists, is GREEN, accepted=false, and its handoff is recorded before Goal 081.
- Record that the Goal 080 commit message used `Goal 080 ... GREEN` rather than the preferred `GREEN Goal 080 ...`; classify this as P3 process debt only. Do not rewrite history.
- Confirm `AlphaRuntimeBootstrap.cs` baseline line count/hash and keep it unchanged.
- Confirm no public schema/Runtime/Unity/provider/Lua/generator-library/project files are already staged for unrelated changes.
