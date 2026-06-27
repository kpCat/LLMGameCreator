# Goal 022: Development Complexity Stabilization And Artifact Scope Governance

## Starting gate

This goal may start only after the user explicitly provides:

```text
generated_game_profile_contract_verification passed
```

Goal 021 / S177C must already be reviewed from the pushed repository. Do not re-open Goal 021 architecture unless a concrete pushed defect is found.

## Final gate

Stop at exactly one final gate:

```text
development_complexity_stabilization_verification
```

Leave this gate `required`, not `passed`.

Do not start the previous queued "Capability Bundle Selection To Pipeline Inputs" work in this goal. That work becomes the next product candidate after this stabilization goal is reviewed.

## Why this goal exists

The recent Goal 021 hotfix chain proved a process defect: valid product smoke and `check-all.ps1` can mutate tracked generated artifacts outside the active goal family. That turns every later goal into a forensic review and makes development complexity grow multiplicatively.

The user explicitly requested a stabilization goal before continuing generator capability work.

## Product / process outcome

The concrete improvement must be:

```text
Future Codex goals have a documented and automated artifact-scope governance path that prevents unrelated tracked generated artifacts from being silently mutated by tests, product smoke, check-all, or hotfix repair work.
```

This is not a gameplay feature goal. It is a bounded process/tooling goal that protects future gameplay goals from avoidable complexity growth.

The goal is successful only if it reduces future review load by adding repeatable checks, not merely by writing another policy document.

## Read first

Read these before editing:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_021_GENERATED_GAME_PROFILE_CONTRACT_REFRESH.md`
8. `docs/GAME_PROFILE_CONTRACT_V1.md`
9. `docs/CODEX_EXECUTION_DOCTRINE.md`
10. `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
11. `.devflow/scripts/check-all.ps1`
12. `.devflow/scripts/run-product-smoke.ps1`
13. product-smoke tests that use `LLMGC_PRODUCT_SMOKE_PROJECT_DIR` or write `.llmgc/procedural/**`

## Scope

Allowed:

- New policy doc: `docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md`.
- New artifact-scope configuration under `.devflow/artifact-scope/`.
- New scope guard script, preferably `.devflow/scripts/check-artifact-scope.ps1`.
- Minimal updates to `.devflow/scripts/check-all.ps1` to isolate full-test product-smoke writes under the check-all run directory and/or support a scope guard mode.
- Minimal updates to `.devflow/scripts/run-product-smoke.ps1` only if required to add a `development-complexity-stabilization` product smoke route or to avoid unintended root artifact mutation.
- Focused tests under `tests/LLMGameCreator.Tests/Devflow/`, `tests/LLMGameCreator.Tests/ProductSmoke/`, or another narrow existing test namespace.
- Compact root artifacts under `.llmgc/procedural/development-complexity-stabilization/`.
- State/routing docs:
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Forbidden:

- Do not implement Capability Bundle Selection To Pipeline Inputs in this goal.
- Do not start Goal 023 or S185.
- Do not change public `GamePackage` schema.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms/Runtime Preview UI work.
- Do not run or modify Unity player/build entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not edit `generator-library/**`.
- Do not delete, untrack or broadly clean old generated Unity outputs in this goal unless the user explicitly asks for that cleanup. This goal may inventory and guard them, but must not perform broad cleanup.
- Do not redesign the generator architecture.
- Do not add broad framework layers. Prefer small devflow scripts/tests and policy artifacts.

## Bounded git exception

This goal is specifically about tracked-file mutation governance. Bounded git inspection is allowed only for scope guard implementation and verification.

Allowed git commands:

```powershell
git diff --name-only
git diff --name-only -- <paths>
git status --porcelain
git ls-files
git check-ignore -v -- <path>
git restore -- <tracked generated artifact paths mutated by the current verification run only>
```

Rules:

- Do not commit, push, branch, merge, rebase, reset the repository, alter history, or restore broad path groups.
- `git restore` is allowed only as a local repair after a verification command mutates tracked generated artifacts, and only after the isolation defect is fixed.
- The final Codex report must list every git command used and why.
- If bounded git commands are unavailable, stop with a blocker and include exact command output.

## Required slices

### S178: Record accepted Goal 021 and insert stabilization goal

Record that the user accepted:

```text
generated_game_profile_contract_verification passed
```

Update state/queue docs so current work becomes Goal 022 stabilization and the current gate after this goal is:

```text
development_complexity_stabilization_verification required
```

Queue handling:

- Insert this stabilization goal as Goal 022.
- Move the previously queued `Capability Bundle Selection To Pipeline Inputs` to the next candidate slot, normally Goal 023.
- Do not start the capability-selection implementation.

### S179: Development complexity stabilization policy

Create `docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md`.

It must define, at minimum:

- artifact mutability classes:
  - source code/docs;
  - state/handoff docs;
  - current-goal compact review artifacts;
  - historical compact artifacts, normally read-only;
  - heavy generated build/runtime outputs, normally untracked/ignored;
  - task-pack docs;
- rule: a goal may write only its declared artifact root and declared state docs unless a bounded hotfix explicitly lists restoration paths;
- rule: `check-all.ps1` and ordinary tests must not mutate tracked historical artifacts;
- rule: product-smoke routes that intentionally regenerate repo-root compact artifacts must declare that behavior and artifact family;
- rule: after every hotfix and after every goal final verification, scope must be checked before the gate is accepted;
- rule: invalid/fake/leak matrices should use shared mutation/validation helpers where possible instead of bespoke report-only diagnostics;
- rule: if more than 8-10 files or more than one artifact family must change, split the task or document a bounded exception;
- rule: housekeeping/cleanup tasks must not be mixed into product goals unless explicitly requested;
- recommended cadence: after every 5 accepted goals, run a stabilization audit; after every 10 accepted goals, run an architecture/process audit.

The policy must be practical and enforceable by scripts/tests, not only prose.

### S180: Artifact scope policy config and guard script

Add a machine-readable policy file under `.devflow/artifact-scope/`, for example:

```text
.devflow/artifact-scope/artifact-scope-policy.json
```

It must include at least:

- stable schema version;
- default mutable roots for devflow runs;
- allowed current-goal artifact roots;
- historical artifact roots that should be read-only during unrelated goals;
- tracked-heavy-output patterns that should not be newly introduced;
- default allowed state docs;
- standard forbidden broad paths such as `.sln`, `.csproj`, `generator-library/**`, Unity build entrypoints, public GamePackage schema paths, unless a task explicitly allows them.

Add `.devflow/scripts/check-artifact-scope.ps1`.

Minimum behavior:

- Normalize path separators.
- Read the machine-readable policy file.
- Inspect tracked changed files using bounded git commands.
- Accept explicit allowed exact paths and allowed path prefixes from parameters.
- Reject changed tracked paths outside the allowed set.
- Emit deterministic JSON and Markdown reports when requested.
- Return non-zero exit code on violations.
- Distinguish:
  - allowed current-goal changes;
  - allowed task-doc changes;
  - disallowed legacy artifact mutation;
  - disallowed project/schema/generator-library/UI/Unity entrypoint mutation;
  - tracked ignored/heavy-output warnings.

Suggested parameters:

```powershell
-PolicyPath
-Scenario
-BaselineRef
-AllowedPath
-AllowedPathPrefix
-ReportDirectory
-FailOnTrackedIgnored
```

The script must not depend on the user's absolute local path.

### S181: Isolate full-test artifact writes

Repair the discovered failure mode where full test/check-all execution can mutate tracked repo-root artifacts.

At minimum:

- Update `.devflow/scripts/check-all.ps1` so the test phase sets product-smoke/test artifact output environment variables to paths under the current check-all run directory, then restores previous environment values in `finally`.
- Use names already used by product-smoke tests where possible:
  - `LLMGC_PRODUCT_SMOKE_PROJECT_DIR`
  - `LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR`
- If additional test artifact environment variables are needed, document them in the new policy and keep them local to devflow/test execution.
- Do not change scenario-specific `run-product-smoke.ps1` behavior that intentionally regenerates current review artifacts in the repository root for explicit review scenarios.

If `check-all.ps1` still mutates historical tracked artifacts after this repair, the goal is not complete.

### S182: Inventory tracked generated artifacts without broad cleanup

Write a deterministic inventory artifact under:

```text
.llmgc/procedural/development-complexity-stabilization/
```

Suggested file:

```text
tracked-generated-artifact-inventory.json
```

The inventory must include:

- tracked `.llmgc/procedural/**` files grouped by artifact family;
- whether each family is current-goal mutable, historical compact review artifact, or heavy/generated output;
- tracked ignored/heavy Unity output findings as warnings, not automatic cleanup;
- explicit note that broad cleanup/untracking is deferred unless separately requested.

Do not delete or untrack old generated outputs in this goal.

### S183: Product smoke, focused tests, and invalid matrix

Add product smoke route:

```text
development-complexity-stabilization
```

The product smoke must verify, at minimum:

- policy doc exists;
- policy JSON exists and parses;
- scope guard script exists;
- check-all isolation is present;
- compact stabilization report exists;
- no LLM/provider/media/Lua/Unity build execution was invoked;
- final report is `accepted=false`;
- final manual gate is `development_complexity_stabilization_verification`.

Add focused tests for:

- policy JSON parsing and required classes;
- scope guard allowlist accepts declared current-goal paths;
- scope guard rejects legacy artifact mutation;
- scope guard rejects `.sln` / `.csproj` changes unless explicitly allowed;
- scope guard rejects public GamePackage schema path changes unless explicitly allowed;
- check-all script sets/restores test artifact env vars;
- state docs record Goal 021 accepted before Goal 022;
- queue docs put Capability Bundle Selection after this stabilization goal.

Invalid/fake/leak matrix minimum scenarios:

1. legacy Goal 020 artifact mutation is rejected;
2. legacy Unity multi-variant artifact mutation is rejected;
3. `.sln` or `.csproj` mutation is rejected;
4. public GamePackage schema mutation is rejected;
5. generator-library mutation is rejected;
6. Unity build/player entrypoint mutation is rejected;
7. copied scope report with no policy JSON is rejected;
8. fake report with violations but `accepted=true` is rejected;
9. product-smoke root write without declared scenario root is rejected;
10. missing check-all artifact isolation is rejected;
11. tracked ignored/heavy output introduced as new current artifact is rejected or recorded as blocking if `FailOnTrackedIgnored` is used;
12. multiple final gates in state policy are rejected.

Invalid cases should flow through the guard/policy validator where possible. Do not manually append diagnostics when a real helper can produce them.

### S184: State handoff and final review artifacts

Write compact artifacts under:

```text
.llmgc/procedural/development-complexity-stabilization/
```

Suggested files:

```text
development-complexity-stabilization-report.json
development-complexity-stabilization-report.md
development-complexity-stabilization-verification.md
artifact-scope-policy-proof.json
tracked-generated-artifact-inventory.json
scope-guard-invalid-matrix.json
check-all-isolation-proof.json
```

The final report must include:

```text
accepted=false
finalStatus=development_complexity_stabilization_verification
manualGate=development_complexity_stabilization_verification
previousAcceptedGate=generated_game_profile_contract_verification passed
scopeGuardImplemented=true
checkAllArtifactIsolationImplemented=true
legacyArtifactMutationGuarded=true
trackedGeneratedArtifactInventoryWritten=true
capabilitySelectionStarted=false
publicGamePackageSchemaChanged=false
projectFilesChanged=false
generatorLibraryChanged=false
unityBuildExecuted=false
noExternalProviderLlmRagLuaMedia=true
```

Top-level diagnostics must contain no `severity=error`.

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Current next work after Goal 022 must be:

```text
development_complexity_stabilization_verification
```

Do not mark it passed.

## Anti-false-positive review

Before final response, perform and report these checks:

- Final `development-complexity-stabilization-report.json` has `accepted=false`.
- `finalStatus` and `manualGate` equal `development_complexity_stabilization_verification`.
- `previousAcceptedGate` equals `generated_game_profile_contract_verification passed`.
- `capabilitySelectionStarted=false`.
- Scope guard rejects at least the required invalid mutation cases.
- `check-all.ps1` no longer writes product-smoke artifacts to repository root during full test execution.
- Running `check-artifact-scope.ps1` after product smoke/check-all shows only allowed changed files for Goal 022.
- Historical Goal 020 and Unity multi-variant compact artifacts are not mutated by final verification.
- State docs record Goal 021 accepted before Goal 022.
- `CONTEXT_INDEX.md` current next work is `development_complexity_stabilization_verification`.
- `FULL_GENERATOR_GOAL_QUEUE.md` shows Capability Bundle Selection as the next candidate after this stabilization goal, not started.
- No local absolute paths, timestamps, GUID-like nondeterminism, temp/user paths in compact deterministic artifacts except devflow run reports where timestamps already exist by design.
- Mojibake markers absent in changed text files.

## Required verification

Run, at minimum:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~DevelopmentComplexityStabilization|FullyQualifiedName~ArtifactScope|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario development-complexity-stabilization
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-022-final -AllowedPath docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md -AllowedPathPrefix .devflow/artifact-scope/ -AllowedPath .devflow/scripts/check-artifact-scope.ps1 -AllowedPath .devflow/scripts/check-all.ps1 -AllowedPath .devflow/scripts/run-product-smoke.ps1 -AllowedPathPrefix .llmgc/procedural/development-complexity-stabilization/ -AllowedPathPrefix tests/LLMGameCreator.Tests/Devflow/ -AllowedPathPrefix tests/LLMGameCreator.Tests/ProductSmoke/ -AllowedPath docs/CURRENT_GENERATOR_STATE.json -AllowedPath docs/CURRENT_GENERATOR_STATE.md -AllowedPath docs/CONTEXT_INDEX.md -AllowedPath docs/FULL_GENERATOR_GOAL_QUEUE.md -AllowedPath docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-022-development-complexity-stabilization-and-artifact-scope-governance-CODEX_GOAL.md -AllowedPath docs/GOAL_022_DEVELOPMENT_COMPLEXITY_STABILIZATION_AND_ARTIFACT_SCOPE_GOVERNANCE.md
```

If `check-all.ps1` fails because of real mutation or scope problems, fix within this goal. If it fails because of an environmental blocker, stop and report exact logs.

## Final response requirements

The final Codex response must include:

- changed files;
- new policy/script/config paths;
- compact artifact paths;
- report hashes;
- tracked generated artifact inventory summary;
- focused/product-smoke/check-all/scope-guard verification results;
- whether final valid report has zero top-level error diagnostics;
- confirmation that `development_complexity_stabilization_verification` remains required, not passed;
- confirmation that Capability Bundle Selection / Goal 023 was not started;
- exact bounded git commands used, or confirmation none were needed.
