# Read-first list

Read these before editing anything:

1. `AGENTS.md`
2. `docs/agent-tasks/goal-079-edit-driven-spine-quality-consolidation/GOAL.md`
3. `docs/agent-tasks/goal-079-edit-driven-spine-quality-consolidation/allowed-files.md`
4. `docs/agent-tasks/goal-079-edit-driven-spine-quality-consolidation/forbidden-files.md`
5. `docs/agent-tasks/goal-079-edit-driven-spine-quality-consolidation/validation.md`
6. `docs/agent-tasks/goal-079-edit-driven-spine-quality-consolidation/final-report-format.md`
7. `docs/CURRENT_GENERATOR_STATE.md`
8. `docs/CURRENT_GENERATOR_STATE.json`
9. `docs/CONTEXT_INDEX.md`
10. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
11. `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
12. `.devflow/artifact-scope/artifact-scope-policy.json`
13. `.llmgc/procedural/goal-074-schema-driven-campaign-authoring-review-workspace/schema-driven-campaign-authoring-review-workspace-report.md`
14. `.llmgc/procedural/goal-074-schema-driven-campaign-authoring-review-workspace/quality-gate-scan.json`
15. `.llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/schema-driven-campaign-edit-validate-apply-loop-report.md`
16. `.llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/quality-gate-scan.json`
17. `.llmgc/procedural/goal-076-edit-driven-playable-preview-refresh/edit-driven-playable-preview-refresh-report.md`
18. `.llmgc/procedural/goal-076-edit-driven-playable-preview-refresh/quality-gate-scan.json`
19. `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/edit-driven-review-package-materialization-report.md`
20. `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/quality-gate-scan.json`
21. `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/edit-driven-review-package-playable-session-report.md`
22. `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/quality-gate-scan.json`
23. `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/package-read-proof.json`
24. `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/playable-session-replay-proof.json`
25. `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/tamper-negative-proof.json`
26. `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs`
27. `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs`
28. `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignReviewPackagePlaySessionControl.cs`
29. `src/LLMGameCreator.WinForms/CompositionRoot.cs`
30. `src/LLMGameCreator.Application/Design/SchemaDrivenCampaignAuthoringReviewWorkspace/**`
31. `src/LLMGameCreator.Application/Design/SchemaDrivenCampaignEditValidateApplyLoop/**`
32. `src/LLMGameCreator.Application/Design/EditDrivenPlayablePreviewRefresh/**`
33. `src/LLMGameCreator.Application/Design/EditDrivenPlayableReviewPackageMaterialization/**`
34. `src/LLMGameCreator.Application/Design/EditDrivenReviewPackagePlayableSession/**`
35. Existing product-smoke tests for Goals 074-078.

Also inspect current `origin/main` history before edits:

- current branch must be `main`;
- latest `origin/main` must include Goal 078 commit `4a68e9c` or a later commit containing it;
- do not rewrite, revert, cherry-pick, rebase, reset, clean, stash, or force-push.
