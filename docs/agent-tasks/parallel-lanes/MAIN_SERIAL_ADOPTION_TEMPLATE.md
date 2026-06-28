# Main Serial Adoption Template

## Working Copy

Use the main working copy only:

```text
C:\Users\endim\LLMDevelop\main\LLMGameCreator
```

This task accepts or rejects exactly one candidate.

Do not perform broad branch management, pushing, rebasing, or uncontrolled merge work.

## Adoption Input

Candidate lane:

```text
<lane-a | lane-b | lane-c | lane-d>
```

Candidate id:

```text
<candidate_id>
```

Candidate summary supplied by user:

```text
<paste candidate final report here>
```

Candidate working copy path supplied by user if available:

```text
C:\Users\endim\LLMDevelop\<lane>\LLMGameCreator
```

## Goal

Review one candidate and bring only acceptable changes into `main`.

This is the only place where accepted state docs may be updated for the candidate.

## Read First

Read in `main`:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/MODULE_CONTRACT_MANIFEST_V1.md`
- `docs/PRODUCT_SMOKE_SCENARIO_MANIFEST_V1.md`
- `docs/PARALLEL_CANDIDATE_DEVELOPMENT_POLICY.md`
- `docs/PARALLEL_LANE_ADOPTION_RULES.md`
- `docs/EXTERNAL_TECHNOLOGY_SCOUTING_TEMPLATE.md`
- candidate final report supplied by user

Use Windows/PowerShell or repo-relative paths. Do not use `/mnt`, `/home/oai`, `sandbox:/`, or `C:\mnt`.

## Adoption Rules

Accept only changes that:

- are candidate-owned;
- do not overlap another module owner;
- preserve current accepted public contracts;
- preserve deterministic behavior;
- include external scouting for non-trivial subsystem work;
- include focused tests if code behavior was added;
- do not add live runtime provider/API/LLM/RAG/media dependency;
- do not require broad project file or solution changes.

Reject or stop if the candidate:

- changed active state docs directly;
- changed public `GamePackage` schema without explicit approval;
- changed `.sln` or `.csproj`;
- changed WinForms UI;
- changed Unity runtime/build entrypoints;
- changed generator-library;
- changed provider/LLM/RAG/media/Lua execution;
- changed `run-product-smoke.ps1` or `GeneratorPlanGamePackageAssembler.cs` without explicit kernel adoption scope;
- uses unclear external license terms;
- requires live runtime API/provider behavior.

## Required Behavior

1. Inspect candidate changed files from the user report and/or candidate working copy.
2. Classify changes:
   - accept;
   - accept with narrow adjustment;
   - reject;
   - stop for kernel decision.
3. Bring acceptable changes into `main` using narrow edits only.
4. Do not silently copy forbidden/shared changes.
5. Update `docs/CONTEXT_INDEX.md` only if accepted docs must become discoverable.
6. Update `docs/CURRENT_GENERATOR_STATE.md` and `docs/CURRENT_GENERATOR_STATE.json` only after acceptance is justified.
7. Add or update compact accepted proof artifacts only if the repo conventions require it.
8. Run focused tests and any candidate smoke scenario.
9. Run current state docs tests if state docs changed.
10. Run broader validation if feasible.
11. Stop on one manual adoption gate.

## Validation

Prefer:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~<candidate focused tests>"
```

If state docs changed:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

If a product smoke scenario was accepted:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario <scenario>
```

Then run:

```powershell
.\.devflow\scripts\check-all.ps1
```

If full validation is too expensive or blocked, report exactly why and list focused validation that did run.

## Stop Statuses

Use one:

```text
candidate_adopted_to_main_manual_gate_required
candidate_rejected
candidate_requires_split_or_kernel_decision
candidate_requires_license_decision
candidate_requires_user_decision
```

## Final Report

Include:

- candidate id;
- adoption decision;
- accepted files;
- rejected files;
- changed files in `main`;
- external technology decision;
- tests run;
- smoke run;
- remaining manual gate;
- exact next instruction for lane refresh, without performing branch management.

