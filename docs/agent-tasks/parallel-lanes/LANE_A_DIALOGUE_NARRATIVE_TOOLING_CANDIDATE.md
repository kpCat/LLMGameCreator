# Lane A Candidate Task - Dialogue Narrative Tooling

## Working Copy

Use this working copy only:

```text
C:\Users\endim\LLMDevelop\lane-a\LLMGameCreator
```

Branch/lane name for human orientation only:

```text
lane-a
```

Do not perform branch management, pushing, rebasing, or merge work.

## Candidate

Candidate id:

```text
candidate_dialogue_narrative_tooling_v1
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

Create a low-conflict candidate for future dialogue/narrative tooling support.

The candidate must evaluate whether Ink and/or Yarn Spinner should be:

- adopted as a dependency;
- adapted behind an LLMGameCreator adapter;
- used as reference only;
- deferred;
- rejected.

Default expectation for this candidate: reference/adaptation design first, no direct dependency unless the evidence is very strong and the change stays in allowed paths.

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
- existing dialogue, quest, and package assembly docs/tests found by local search

Use Windows/PowerShell or repo-relative paths. Do not use `/mnt`, `/home/oai`, `sandbox:/`, or `C:\mnt`.

## External Technology Scouting

Required. Evaluate at minimum:

- Ink: `https://github.com/inkle/ink`
- Yarn Spinner core: `https://github.com/YarnSpinnerTool/YarnSpinner`
- Yarn Spinner Unity integration only as a separate component, if relevant.

Check:

- exact component license;
- current maintenance;
- C#/.NET compatibility;
- Unity compatibility only as future export reference;
- deterministic/offline behavior;
- whether the tool can be used editor-time only;
- whether generated dialogue can be represented in current LLMGameCreator contracts without public schema changes;
- adapter/wrapper boundary;
- how to replace it later.

If live web access is unavailable, do not invent results. Mark scouting as blocked and use only repo-local/offline evidence.

## Allowed Files

Preferred candidate-owned paths:

- `docs/candidates/candidate_dialogue_narrative_tooling_v1/**`
- `docs/agent-tasks/parallel-lanes/**` only if this task file needs a narrow correction
- `src/LLMGameCreator.Application/Design/CandidateModules/DialogueNarrativeTooling/**`
- `tests/LLMGameCreator.Tests/Application/CandidateModules/DialogueNarrativeTooling/**`
- `.devflow/product-smoke-scenarios/candidate-dialogue-narrative-tooling-v1.json`
- `.llmgc/procedural/candidate-dialogue-narrative-tooling-v1/**`

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

1. Search existing repo contracts for dialogue/quest/narrative formats.
2. Produce `docs/candidates/candidate_dialogue_narrative_tooling_v1/EXTERNAL_TECHNOLOGY_SCOUTING.md`.
3. Produce a candidate contract note under the same folder:
   - proposed internal dialogue/narrative contract boundary;
   - adapter boundary;
   - import/export direction;
   - absence behavior;
   - deterministic validation strategy.
4. If code is added, keep it as candidate-owned interfaces/models/services only.
5. If tests are added, keep them focused on candidate-owned behavior.
6. Do not claim production integration.
7. Do not claim an accepted manual gate.

## Validation

Run focused tests for candidate-owned code, if any.

If no code is added, run current state docs tests only if available and cheap.

If broad validation is too expensive, do not fake it. Report exactly what was and was not run.

## Stop Conditions

Stop if:

- required repo docs are missing and cannot be inferred safely;
- Ink/Yarn license status cannot be verified and the candidate would depend on it;
- implementation requires public schema changes;
- implementation requires shared kernel changes;
- implementation requires project file changes;
- implementation requires runtime provider, live API, LLM, RAG, or network dependency.

## Final Report

Include:

- candidate id;
- changed files;
- scouting summary;
- decision for Ink;
- decision for Yarn Spinner core;
- decision for Yarn Spinner Unity integration if evaluated;
- adapter recommendation;
- tests run;
- smoke run, if any;
- blockers;
- final status `candidate_ready_for_serial_adoption` or exact stop status.

