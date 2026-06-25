Task id: PRODUCT_SLICE_058A_SEMANTIC_GUIDED_ACCEPTANCE_CORRECTNESS_HOTFIX
Goal: Repair Goal 005 semantic-guided acceptance correctness gaps

Read-first sources:
- AGENTS.md, docs/CONTEXT_INDEX.md and docs/CURRENT_GENERATOR_STATE.json
- docs/agent-tasks/NEXT_PRODUCT_SLICE/product-slice-058a-semantic-guided-acceptance-correctness-hotfix-CODEX_TASK.md
- docs/SEMANTIC_PACK_CONTRACT_V1.md
- SemanticLayerCompilerService and SemanticGuidedCompositionAcceptanceService
- SemanticLayerCompiler/SemanticGuidedComposition focused tests
- QuestDialogInteractionFamilyAcceptance tests
- product smoke runner

Implemented:
- actual semantic-guided scenario acceptance is now derived from compiler, missing-layer, candidate-leakage, rule-pack and composition diagnostics
- ExpectedValid is expectation metadata only; expected-invalid scenarios must be rejected by a real error diagnostic for the matrix to pass
- active excludes, forbidden_in_tone and unsatisfied requires composition diagnostics now reject acceptance
- semantic_pack_contract_v1 schema and layer id/kind prefix identity are validated
- conflicted term and relation ids cannot re-enter active compiled output later in the same compilation
- malformed semantic-pack JSON is reported with deterministic relative-file diagnostics instead of a raw JsonException
- runtime evidence wording now states semantic-selected ids are generator-level selections while Goal 004 runtime evidence is an independent regression check
- existing semantic-guided-composition artifact folder and smoke route are preserved

Verification:
- SemanticLayerCompiler/SemanticGuidedComposition/QuestDialogInteractionFamily/CurrentGeneratorStateDocsTests filtered tests: 29/29 passed
- semantic-guided-composition product smoke: 1/1 passed
- check-all.ps1: 731/731 tests passed, build 0 warnings / 0 errors

Next:
- stay at semantic_guided_composition_artifact_verification

Forbidden scope preserved:
- no S059
- no Goal 006
- no Runtime Preview UI
- no LLM/RAG/provider/Lua/Unity/media execution
- no external dataset import
- no broad GamePackage/runtime redesign
- no git commands
