# Lane D Candidate Task - Navigation Pathfinding

## Working Copy

Use this working copy only:

```text
C:\Users\endim\LLMDevelop\lane-d\LLMGameCreator
```

Branch/lane name for human orientation only:

```text
lane-d
```

Do not perform branch management, pushing, rebasing, or merge work.

## Candidate

Candidate id:

```text
candidate_navigation_pathfinding_v1
```

Base accepted gate:

```text
modular_generator_kernel_parallel_readiness_verification passed
```

Final candidate status:

```text
candidate_ready_for_serial_adoption
```

## Goal

Create a low-conflict candidate for future navigation/pathfinding support.

The candidate must scout Recast/Detour and simpler grid/pathfinding options, then propose an LLMGameCreator-owned navigation contract that can support later runtime/export work without forcing native integration now.

Default expectation for this candidate: contract/reference design first. Do not integrate native Recast/Detour code in this candidate.

## Read First

Read these files before editing:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/MODULE_CONTRACT_MANIFEST_V1.md`
- `docs/PRODUCT_SMOKE_SCENARIO_MANIFEST_V1.md`
- `docs/PARALLEL_CANDIDATE_DEVELOPMENT_POLICY.md`
- `docs/PARALLEL_LANE_ADOPTION_RULES.md`
- `docs/EXTERNAL_TECHNOLOGY_SCOUTING_TEMPLATE.md`
- `docs/MODULAR_KERNEL_COMPATIBILITY_MODEL.md` if present
- existing map, movement, grid, path, region, NPC schedule, and runtime docs/tests found by local search

Use Windows/PowerShell or repo-relative paths. Do not use `/mnt`, `/home/oai`, `sandbox:/`, or `C:\mnt`.

## External Technology Scouting

Required. Evaluate at minimum:

- Recast/Detour: `https://github.com/recastnavigation/recastnavigation`
- any existing .NET pathfinding libraries only if directly relevant and compatible;
- simple deterministic grid A* as fallback/reference;
- Unity navigation as future export/runtime reference only, not current dependency.

Check:

- license;
- native build impact;
- whether direct dependency would require `.csproj`/native packaging changes;
- deterministic path results;
- grid vs navmesh fit for generated packages;
- runtime/export boundary;
- adapter boundary;
- replacement plan.

If live web access is unavailable, do not invent results. Mark scouting as blocked and use only repo-local/offline evidence.

## Allowed Files

Preferred candidate-owned paths:

- `docs/candidates/candidate_navigation_pathfinding_v1/**`
- `src/LLMGameCreator.Application/Design/CandidateModules/NavigationPathfinding/**`
- `tests/LLMGameCreator.Tests/Application/CandidateModules/NavigationPathfinding/**`
- `.devflow/product-smoke-scenarios/candidate-navigation-pathfinding-v1.json`
- `.llmgc/procedural/candidate-navigation-pathfinding-v1/**`

If existing conventions require a slightly different candidate-owned path, keep it narrow and explain why.

## Forbidden Files And Areas

Do not modify:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- public `GamePackage` schema
- `.sln`
- `.csproj`
- WinForms UI
- Unity runtime/build entrypoints
- `generator-library/**`
- provider/LLM/RAG/media code
- Lua execution code
- `.devflow/scripts/run-product-smoke.ps1`
- `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`

If the work requires any forbidden file, stop with:

```text
requires_kernel_or_serial_adoption_task
```

## Required Behavior

1. Search existing repo contracts for movement, map, region, grid, pathfinding, NPC schedule, and runtime state.
2. Produce `docs/candidates/candidate_navigation_pathfinding_v1/EXTERNAL_TECHNOLOGY_SCOUTING.md`.
3. Produce a candidate contract note under the same folder:
   - internal navigation query contract;
   - grid pathfinding fallback;
   - navmesh/export boundary;
   - deterministic tie-breaking rules;
   - absence behavior;
   - validation strategy.
4. If code is added, keep it candidate-owned and avoid native dependencies.
5. Add focused tests only for candidate-owned behavior.
6. Do not claim production integration.
7. Do not claim an accepted manual gate.

## Validation

Run focused candidate tests if code is added.

If no code is added, run lightweight docs validation if available.

If broad validation is too expensive, report exactly what was and was not run.

## Stop Conditions

Stop if:

- implementation requires native dependency packaging;
- implementation requires public schema changes;
- implementation requires shared kernel changes;
- implementation requires project file changes;
- implementation requires Unity runtime changes;
- implementation requires runtime provider, live API, LLM, RAG, or network dependency.

## Final Report

Include:

- candidate id;
- changed files;
- scouting summary;
- decision for Recast/Detour;
- fallback deterministic pathfinding approach;
- adapter recommendation;
- tests run;
- smoke run, if any;
- blockers;
- final status `candidate_ready_for_serial_adoption` or exact stop status.

