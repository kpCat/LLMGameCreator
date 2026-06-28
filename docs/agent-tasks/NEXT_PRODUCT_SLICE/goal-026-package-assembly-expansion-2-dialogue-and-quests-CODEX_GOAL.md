# Codex Goal 026: Package Assembly Expansion 2 - Dialogue And Quests

Start only after the prompt explicitly includes:

```text
package_assembly_world_entities_expansion_verification passed
```

Then execute strictly:

```text
docs/GOAL_026_PACKAGE_ASSEMBLY_EXPANSION_2_DIALOGUE_AND_QUESTS.md
```

Hard stop:

```text
package_assembly_dialogue_quests_expansion_verification required
```

Do not mark the gate passed.

Do not start Goal 027 or S213.

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
9. `docs/GOAL_025_PACKAGE_ASSEMBLY_EXPANSION_1_WORLD_AND_ENTITIES.md`
10. `docs/PACKAGE_ASSEMBLY_WORLD_ENTITIES_CONTRACT_V1.md`
11. `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-report.json`
12. `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-package-summary.json`
13. `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`
14. `src/LLMGameCreator.Domain/Definitions/ContentDefinitions.cs`
15. `src/LLMGameCreator.Domain/Definitions/DialogueDefinitions.cs`
16. `src/LLMGameCreator.Application/Validation/NarrativeDefinitionValidator.cs`
17. `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
18. local package assembly / narrative validation / quest-dialogue runtime tests needed by the goal.

## Scope reminder

This is a bounded composite goal:

```text
Contract -> Module -> Integration -> Proof
```

Those are internal phases, not separate goals.

Allowed package work is only dialogue/quest package assembly through existing schema and existing Application seams. Do not change public `GamePackage` schema, `.sln`, `.csproj`, Unity, WinForms UI, provider/media/RAG/LLM/Lua execution, generator-library, or historical accepted artifacts.

## Final response reminder

Final response must include changed files, compact artifacts, hashes, focused/product-smoke/check-all/scope-guard results, real + synthetic consumer proof, invalid matrix, acceptance evidence table, final gate status, no Goal 027/S213 confirmation, no public schema change confirmation, no product vertical gate confirmation, and bounded git usage confirmation.
