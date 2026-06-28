# Codex Goal 029: Modular Generator Kernel And Parallel Development Readiness

Start only after the prompt explicitly includes:

```text
package_assembly_combat_progression_expansion_verification passed
```

Then execute strictly:

```text
docs/GOAL_029_MODULAR_GENERATOR_KERNEL_PARALLEL_READINESS.md
```

Hard stop:

```text
modular_generator_kernel_parallel_readiness_verification required
```

Do not mark the gate passed.

Do not start Goal 030 or S234.

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
9. `docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md`
10. `.devflow/scripts/run-product-smoke.ps1`
11. `.devflow/scripts/check-all.ps1`
12. `docs/GOAL_028_PACKAGE_ASSEMBLY_EXPANSION_4_COMBAT_PROGRESSION.md`
13. `.llmgc/procedural/package-assembly-combat-progression/package-assembly-combat-progression-report.json`
14. `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
15. package assembly services/tests from Goals 025-028.

## Scope reminder

This is a bounded composite goal:

```text
Contract -> Module -> Integration -> Proof
```

Those are internal phases, not separate goals.

This goal must create real technical readiness for modular/parallel development: module manifests, smoke scenario manifests, a narrow registry/compatibility seam, module absence behavior proof, and fast module-verification tier rules.

Do not implement a new gameplay feature module. Do not change public `GamePackage` schema, `.sln`, `.csproj`, Unity, WinForms UI, provider/media/RAG/LLM/Lua execution, generator-library, or historical accepted artifacts.

## Final response reminder

Final response must include changed files, compact artifacts, hashes, focused/product-smoke/check-all/scope-guard results, module registry summary, smoke scenario manifest summary, compatibility matrix, absence behavior, verification tiers, acceptance evidence table, final gate status, no Goal 030/S234 confirmation, no public schema change confirmation, no product vertical gate confirmation, and bounded git usage confirmation.
