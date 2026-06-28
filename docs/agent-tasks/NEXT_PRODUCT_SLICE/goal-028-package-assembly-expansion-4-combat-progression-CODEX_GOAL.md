# Codex Goal 028: Package Assembly Expansion 4 - Combat And Progression

Start only after the prompt explicitly includes:

```text
package_assembly_items_economy_crafting_expansion_verification passed
```

Then execute strictly:

```text
docs/GOAL_028_PACKAGE_ASSEMBLY_EXPANSION_4_COMBAT_PROGRESSION.md
```

Hard stop:

```text
package_assembly_combat_progression_expansion_verification required
```

Do not mark the gate passed.

Do not start Goal 029 or S227.

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
9. `docs/GOAL_027_PACKAGE_ASSEMBLY_EXPANSION_3_ITEMS_ECONOMY_CRAFTING.md`
10. `docs/PACKAGE_ASSEMBLY_ITEMS_ECONOMY_CRAFTING_CONTRACT_V1.md`
11. `.llmgc/procedural/package-assembly-items-economy-crafting/package-assembly-items-economy-crafting-report.json`
12. `.llmgc/procedural/package-assembly-items-economy-crafting/package-assembly-items-economy-crafting-package-summary.json`
13. `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`
14. `src/LLMGameCreator.Domain/Definitions/ContentDefinitions.cs`
15. `src/LLMGameCreator.Domain/Definitions/EncounterDefinitions.cs`
16. `src/LLMGameCreator.Domain/Definitions/EconomyDefinitions.cs`
17. `src/LLMGameCreator.Application/Validation/EncounterDefinitionValidator.cs`
18. `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
19. local package assembly / encounter validation / combat-progression runtime tests needed by the goal.

## Scope reminder

This is a bounded composite goal:

```text
Contract -> Module -> Integration -> Proof
```

Those are internal phases, not separate goals.

Allowed package work is only combat/progression package assembly through existing schema and existing Application seams. Do not change public `GamePackage` schema, `.sln`, `.csproj`, Unity, WinForms UI, provider/media/RAG/LLM/Lua execution, generator-library, or historical accepted artifacts.

## Final response reminder

Final response must include changed files, compact artifacts, hashes, focused/product-smoke/check-all/scope-guard results, real + synthetic consumer proof, invalid matrix, acceptance evidence table, final gate status, no Goal 029/S227 confirmation, no public schema change confirmation, no product vertical gate confirmation, and bounded git usage confirmation.
