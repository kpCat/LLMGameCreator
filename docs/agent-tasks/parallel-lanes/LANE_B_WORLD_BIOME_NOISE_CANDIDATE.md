# Lane B Candidate Task - World Biome Noise

## Working Copy

Use this working copy only:

```text
C:\Users\endim\LLMDevelop\lane-b\LLMGameCreator
```

Branch/lane name for human orientation only:

```text
lane-b
```

Do not perform branch management, pushing, rebasing, or merge work.

## Candidate

Candidate id:

```text
candidate_world_biome_noise_v1
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

Create a low-conflict candidate for deterministic world biome/noise generation.

The candidate must scout FastNoise Lite and local alternatives, then propose an LLMGameCreator-owned contract for seeded noise and biome classification.

Default expectation for this candidate: internal contract plus reference/adaptation decision. Do not make FastNoise Lite a hard dependency unless adoption evidence is strong and the change stays in allowed paths.

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
- existing world, region, biome, terrain, map, and package assembly docs/tests found by local search

Use Windows/PowerShell or repo-relative paths. Do not use `/mnt`, `/home/oai`, `sandbox:/`, or `C:\mnt`.

## External Technology Scouting

Required. Evaluate at minimum:

- FastNoise Lite: `https://github.com/Auburn/FastNoiseLite`
- any existing .NET/noise utilities already present in the repository
- simple deterministic built-in algorithm as fallback/reference

Check:

- license;
- C# support;
- deterministic seed behavior;
- 2D/3D support;
- float/double stability concerns;
- build/package impact;
- no runtime network/provider dependency;
- adapter boundary;
- replacement plan.

If live web access is unavailable, do not invent results. Mark scouting as blocked and use only repo-local/offline evidence.

## Allowed Files

Preferred candidate-owned paths:

- `docs/candidates/candidate_world_biome_noise_v1/**`
- `src/LLMGameCreator.Application/Design/CandidateModules/WorldBiomeNoise/**`
- `tests/LLMGameCreator.Tests/Application/CandidateModules/WorldBiomeNoise/**`
- `.devflow/product-smoke-scenarios/candidate-world-biome-noise-v1.json`
- `.llmgc/procedural/candidate-world-biome-noise-v1/**`

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

1. Search existing repo contracts for world/map/biome/terrain generation.
2. Produce `docs/candidates/candidate_world_biome_noise_v1/EXTERNAL_TECHNOLOGY_SCOUTING.md`.
3. Produce a candidate contract note under the same folder:
   - deterministic noise input contract;
   - biome classification contract;
   - seed and coordinate normalization rules;
   - adapter boundary;
   - absence behavior;
   - validation strategy.
4. If code is added, keep it candidate-owned and deterministic.
5. Add focused tests only for candidate-owned behavior.
6. Do not claim production integration.
7. Do not claim an accepted manual gate.

## Validation

Run focused candidate tests if code is added.

If no code is added, run lightweight docs validation if available.

If broad validation is too expensive, report exactly what was and was not run.

## Stop Conditions

Stop if:

- deterministic behavior cannot be demonstrated or specified;
- FastNoise Lite license/current source cannot be verified and the candidate would depend on it;
- implementation requires public schema changes;
- implementation requires shared kernel changes;
- implementation requires project file changes;
- implementation requires runtime provider, live API, LLM, RAG, or network dependency.

## Final Report

Include:

- candidate id;
- changed files;
- scouting summary;
- decision for FastNoise Lite;
- fallback deterministic approach;
- adapter recommendation;
- tests run;
- smoke run, if any;
- blockers;
- final status `candidate_ready_for_serial_adoption` or exact stop status.

