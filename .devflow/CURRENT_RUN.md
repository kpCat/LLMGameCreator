Task id: GOAL_005_LAYERED_SEMANTIC_PACKS_AND_GUIDED_COMPOSITION
Goal: Prove layered semantic packs and semantic-guided composition

Read-first sources:
- AGENTS.md, docs/CONTEXT_INDEX.md and current-state pair
- docs/GOAL_005_LAYERED_SEMANTIC_PACKS_AND_GUIDED_COMPOSITION.md
- docs/SEMANTIC_PACK_AND_RAG_STRATEGY.md
- docs/ARCHITECTURE_STRATEGY_AND_BOUNDARIES.md
- docs/AGENT_CONTEXT_BUDGET_POLICY.md
- SemanticCatalogService and SemanticGenerationContextPreviewService
- QuestDialogInteractionFamilyAcceptanceService and tests
- Application/Test csproj files
- product smoke runner

Implemented:
- recorded Goal 004 manual quest/dialog/interaction family verification as passed from the user artifact-review report
- added docs/SEMANTIC_PACK_CONTRACT_V1.md
- extended existing semantic catalog records with tags, generation hints, constraints and layer provenance
- added SemanticLayerCompilerService for semantic_pack_contract_v1 validation, precedence, candidate quarantine, conflict diagnostics and compiled semantic sidecars
- added compact reference packs under generator-library/semantic-packs
- added SemanticGuidedCompositionAcceptanceService that consumes compiled semantics and existing Goal 004 rule-pack ids
- semantic-guided choices now drive quest pattern, dialogue intent and interaction pattern selection
- candidate layers remain quarantined until explicitly accepted as known active layers
- invalid/conflicting semantic compositions are rejected with diagnostics
- deterministic reports are written under .llmgc/procedural/semantic-guided-composition
- added semantic-guided-composition product smoke route

Verification:
- SemanticCatalog/SemanticGuided/QuestDialogInteractionFamily/CurrentGeneratorStateDocsTests filtered tests: 23/23 passed
- semantic-guided-composition product smoke: 1/1 passed
- check-all.ps1: 721/721 tests passed, build 0 warnings / 0 errors

Next:
- stop at semantic_guided_composition_artifact_verification

Forbidden scope preserved:
- no S059
- no Runtime Preview features
- no semantic editor UI or RAG/vector database
- no external dataset download/vendor
- no provider execution, LLM calls, Lua execution, Unity work or media generation
- no GamePackage or public runtime contract redesign
- no git commands
