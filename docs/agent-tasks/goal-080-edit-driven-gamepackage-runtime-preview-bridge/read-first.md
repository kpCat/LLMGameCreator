# Read-first list

Read these before editing code:

1. `AGENTS.md`
2. `docs/CURRENT_GENERATOR_STATE.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CONTEXT_INDEX.md`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
7. `.devflow/artifact-scope/artifact-scope-policy.json`
8. `.llmgc/procedural/goal-079a-source-format-line-ending-guard/source-format-line-ending-guard-report.md`
9. `.llmgc/procedural/goal-079a-source-format-line-ending-guard/source-format-line-ending-guard-scan.json`
10. `.llmgc/procedural/goal-079-edit-driven-spine-quality-consolidation/edit-driven-spine-quality-consolidation-report.md`
11. `.llmgc/procedural/goal-079-edit-driven-spine-quality-consolidation/quality-gate-scan.json`
12. `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/edit-driven-review-package-playable-session-report.md`
13. `.llmgc/procedural/goal-078-edit-driven-review-package-playable-session/session-replay-proof.json`
14. `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/edit-driven-review-package-materialization-report.md`
15. `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/package-file-ledger.json`
16. `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/review-package/manifest.json`
17. `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/review-package/package-index.json`
18. `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/review-package/player-readable-index.json`
19. Existing GamePackage/domain/application package contracts and validators:
    - `src/LLMGameCreator.Domain/**`
    - `src/LLMGameCreator.Application/**GamePackage**`
    - `src/LLMGameCreator.Application/**Package**`
    - `src/LLMGameCreator.Application/**RuntimePreview**`
    - `src/LLMGameCreator.Application/**Validation**`
20. Existing runtime-preview / projection / player-facing WinForms code:
    - `src/LLMGameCreator.WinForms/Pages/RuntimePreview/**`
    - `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/**`
21. Existing materialization and Unity/player handoff analogs:
    - `src/LLMGameCreator.Application/Design/FullCampaignGamePackageMaterialization/**`
    - `src/LLMGameCreator.Application/Design/UnityAlphaInteractiveCampaignPlayer/**`
    - `tests/LLMGameCreator.Tests/ProductSmoke/*GamePackage*`
    - `tests/LLMGameCreator.Tests/ProductSmoke/*RuntimePreview*`
    - `tests/LLMGameCreator.Tests/ProductSmoke/*UnityAlphaInteractive*`
22. Goal 074-079 Application seams and WinForms workspace controls for style/boundaries:
    - `src/LLMGameCreator.Application/Design/SchemaDrivenCampaignEditValidateApplyLoop/**`
    - `src/LLMGameCreator.Application/Design/EditDrivenPlayablePreviewRefresh/**`
    - `src/LLMGameCreator.Application/Design/EditDrivenPlayableReviewPackageMaterialization/**`
    - `src/LLMGameCreator.Application/Design/EditDrivenReviewPackagePlayableSession/**`
    - `src/LLMGameCreator.Application/Design/EditDrivenSpineQualityConsolidation/**`
    - `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/**`

Read enough of each area to identify the existing public package schema and existing runtime-preview/projection seam. Do not change a file merely because it is in the read-first list.
