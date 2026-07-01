# Goal 073 — Source Format P0 Readability Repair Spec

## Purpose

Repair the concrete P0 blocker found by Goal 072 without broad refactoring.

Goal 072 correctly found a real source-format risk: several existing checked-in C# files have extreme line lengths. This goal must fix only that P0 readability issue, prove the result with focused scanners/tests, and leave deeper P1/P2 debt registered for future work.

## Non-goals

- No feature development.
- No broad architecture cleanup.
- No extraction of shared source-loader/evidence-service frameworks.
- No public GamePackage schema changes.
- No Runtime, Runtime.Abstractions, WinForms, provider/LLM/RAG, Lua, generator-library, .sln or .csproj changes.
- No Unity gameplay expansion. `AlphaRuntimeBootstrap.cs` is not part of this repair.

## Known P0 candidates from Goal 072

Repair extreme line lengths in these files only unless the post-repair scanner proves a directly related line-format P0 remains in the same bounded set:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilityHelpCatalog.cs
src/LLMGameCreator.Application/Design/PackageAssemblyCombatProgression/PackageAssemblyCombatProgressionAcceptanceService.cs
src/LLMGameCreator.Application/Design/CombatMagicAbilityBossEncounterMatrix/CombatMagicAbilityBossEncounterProjector.cs
tests/LLMGameCreator.Tests/Application/PackageAssemblyCombatProgression/PackageAssemblyCombatProgressionAcceptanceTests.cs
src/LLMGameCreator.Application/Design/LuaModuleManifestRegistry/LuaModuleManifestRegistryCatalog.cs
src/LLMGameCreator.Application/Design/PackageAssemblyDialogueQuests/PackageAssemblyDialogueQuestsAcceptanceService.cs
src/LLMGameCreator.Application/Design/PackageAssemblyItemsEconomyCrafting/PackageAssemblyItemsEconomyCraftingAcceptanceService.cs
src/LLMGameCreator.Application/Design/CandidateModules/WorldBiomeNoise/WorldBiomeNoiseCandidateService.cs
```

## Repair rules

Allowed repair types:

- Wrap long collection initializers, object initializers, array literals, interpolated strings or assertion argument lists.
- Split long diagnostic/result construction into local variables.
- Split long constants only if semantics are preserved exactly.
- Extract small private helper methods only inside the same file and only when required to shorten extreme lines safely.
- Keep ordering, ids, hashes, generated evidence semantics and public behavior unchanged unless an existing test already covers the expected deterministic output and remains green.

Forbidden repair types:

- Changing domain values or expected proof values.
- Rewriting algorithms.
- Moving types across files.
- Introducing common helper abstractions.
- Modifying public GamePackage schema.
- Changing Unity proof behavior.
- Adding dependencies.

## Required proof

The goal is GREEN only if:

- No remaining `GQ-P0-SOURCE-EXTREME-LINE-LENGTH` risk is reported for the repaired bounded set.
- All repaired files have no line over 500 characters.
- Goal 072 focused quality scanner/test route passes or an equivalent targeted scan proves P0=0 for source-format extreme line length.
- All directly affected focused tests pass.
- `check-all.ps1` passes.
- Artifact scope guard for this goal passes.
