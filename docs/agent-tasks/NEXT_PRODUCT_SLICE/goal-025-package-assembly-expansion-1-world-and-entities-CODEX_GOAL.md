# Codex Goal 025: Package Assembly Expansion 1 - World And Entities

Start only after the prompt explicitly includes:

```text
modular_contract_goal_policy_adoption_verification passed
```

Then execute strictly:

```text
docs/GOAL_025_PACKAGE_ASSEMBLY_EXPANSION_1_WORLD_AND_ENTITIES.md
```

Hard stop:

```text
package_assembly_world_entities_expansion_verification required
```

Do not mark the gate passed.

Do not start Goal 026 or S206.

Do not create a product vertical gate.

## Required read-first order

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/CURRENT_GENERATOR_STATE.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/MODULAR_CONTRACT_GOAL_POLICY.md`
8. `docs/PACKAGE_ASSEMBLY_EXPANSION_CAMPAIGN_PACK.md`
9. `docs/RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT_V1.md`
10. `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-coverage-audit-report.json`
11. `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-coverage-matrix.json`
12. `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-next-slice-plan.json`
13. `docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md`
14. `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-generator-inputs.json`
15. `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`
16. `src/LLMGameCreator.Domain/Definitions/GameDefinitions.cs`
17. `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
18. local package assembly / validation / runtime-loader tests needed by the goal.

## Scope reminder

This is a bounded composite goal:

```text
Contract -> Module -> Integration -> Proof
```

Those are internal phases, not separate goals.

Allowed package work is only world/entities package assembly through existing schema and existing Application seams. Do not change public `GamePackage` schema, `.sln`, `.csproj`, Unity, WinForms UI, provider/media/RAG/LLM/Lua execution, generator-library, or historical accepted artifacts.

## Final response reminder

Final response must include changed files, compact artifacts, hashes, focused/product-smoke/check-all/scope-guard results, real + synthetic consumer proof, invalid matrix, acceptance evidence table, final gate status, no Goal 026/S206 confirmation, no public schema change confirmation, no product vertical gate confirmation, and bounded git usage confirmation.
