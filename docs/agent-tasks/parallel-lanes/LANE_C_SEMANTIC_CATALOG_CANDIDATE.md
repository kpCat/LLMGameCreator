# Lane C Candidate Task - Semantic Catalog

## Working Copy

Use this working copy only:

```text
C:\Users\endim\LLMDevelop\lane-c\LLMGameCreator
```

Branch/lane name for human orientation only:

```text
lane-c
```

Do not perform branch management, pushing, rebasing, or merge work.

## Candidate

Candidate id:

```text
candidate_semantic_catalog_v1
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

Create a low-conflict candidate for semantic catalog inputs: lexical relations, tags, synonyms, hypernyms, archetype families, and concept links that can help generation without becoming a live RAG/provider dependency.

The candidate must scout ConceptNet and Open English WordNet style resources, then propose an offline/editor-time catalog boundary.

Default expectation for this candidate: offline curated subset/reference design. Do not add a runtime API dependency.

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
- existing semantic, catalog, archetype, tag, profile, and prompt/generator docs/tests found by local search

Use Windows/PowerShell or repo-relative paths. Do not use `/mnt`, `/home/oai`, `sandbox:/`, or `C:\mnt`.

## External Technology Scouting

Required. Evaluate at minimum:

- ConceptNet: `https://conceptnet.io`
- Open English WordNet: `https://github.com/globalwordnet/english-wordnet`
- any repo-local semantic/tag/archetype helpers
- manually curated JSON/YAML catalog as fallback

Check:

- data license and attribution;
- share-alike or redistribution impact;
- offline subset feasibility;
- generated output contamination risk;
- no live runtime API;
- deterministic catalog versioning;
- language coverage;
- adapter/import pipeline boundary;
- replacement plan.

If live web access is unavailable, do not invent results. Mark scouting as blocked and use only repo-local/offline evidence.

## Allowed Files

Preferred candidate-owned paths:

- `docs/candidates/candidate_semantic_catalog_v1/**`
- `src/LLMGameCreator.Application/Design/CandidateModules/SemanticCatalog/**`
- `tests/LLMGameCreator.Tests/Application/CandidateModules/SemanticCatalog/**`
- `.devflow/product-smoke-scenarios/candidate-semantic-catalog-v1.json`
- `.llmgc/procedural/candidate-semantic-catalog-v1/**`

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

1. Search existing repo contracts for semantic packs, tags, archetypes, and generation profiles.
2. Produce `docs/candidates/candidate_semantic_catalog_v1/EXTERNAL_TECHNOLOGY_SCOUTING.md`.
3. Produce a candidate contract note under the same folder:
   - internal semantic catalog format;
   - source attribution metadata;
   - import pipeline boundary;
   - offline subset/versioning approach;
   - absence behavior;
   - validation strategy.
4. If code is added, keep it candidate-owned and avoid importing large datasets.
5. Add focused tests only for candidate-owned behavior.
6. Do not claim production integration.
7. Do not claim an accepted manual gate.

## Validation

Run focused candidate tests if code is added.

If no code is added, run lightweight docs validation if available.

If broad validation is too expensive, report exactly what was and was not run.

## Stop Conditions

Stop if:

- license impact cannot be evaluated and the candidate would include external data;
- implementation requires live runtime API/RAG/provider behavior;
- implementation requires public schema changes;
- implementation requires shared kernel changes;
- implementation requires project file changes;
- implementation imports large datasets into the repository without explicit permission.

## Final Report

Include:

- candidate id;
- changed files;
- scouting summary;
- decision for ConceptNet;
- decision for Open English WordNet;
- recommended offline catalog boundary;
- attribution/license risks;
- tests run;
- smoke run, if any;
- blockers;
- final status `candidate_ready_for_serial_adoption` or exact stop status.

