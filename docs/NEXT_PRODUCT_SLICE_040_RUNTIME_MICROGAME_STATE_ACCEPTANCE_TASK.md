# Product Slice 040 - Runtime Microgame State Acceptance

## Purpose

Close Goal 002 by proving that the generated microgame loop has runtime-backed state evidence.

This is not a UI redesign. It is an acceptance slice for the runtime-backed microgame state foundation created by S038-S039.

## Functional goal

Headless acceptance should prove:

- one-click generation works;
- runtime starts;
- generated active goal is visible;
- interaction advances runtime-owned goal progress;
- challenge/reward/completion has runtime-owned or explicitly validated state evidence;
- selected state path survives existing snapshot/save/reload mechanism if available;
- Runtime Preview displays the runtime-backed evidence.

## Required acceptance artifacts

Write deterministic artifacts under:

```text
.llmgc/procedural/runtime-backed-microgame-state/
```

Expected files:

- `runtime-backed-microgame-state-snapshot.json`
- `runtime-backed-microgame-state-report.md`
- `manual-runtime-backed-microgame-verification.md`

Snapshot/report should include:

- package id/title;
- active goal id/title;
- runtime-owned progress evidence;
- challenge/reward/completion evidence;
- state persistence/snapshot evidence if available;
- fallback diagnostics if any feature remains projection-backed;
- no external execution diagnostic.

## Save/reload/snapshot requirement

Use existing runtime snapshot/store/serialization facilities if they fit.

If existing facilities cannot serialize the selected microgame state without broad redesign:

- do not redesign broadly;
- report exact blocker;
- still produce deterministic acceptance evidence for current runtime state.

## Tests

Add focused acceptance tests:

- runtime-backed state snapshot is deterministic;
- progress/reward/challenge/completion evidence is present;
- fallback diagnostics are explicit when used;
- snapshot/save/reload evidence is present or blocker is explicit;
- manual verification doc is updated;
- no external execution.

Suggested filter:

```text
FullyQualifiedName~RuntimeMicrogameStateAcceptance
```

## Product smoke

Add scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario runtime-backed-microgame-state
```

Smoke must prove:

```text
generate
-> runtime start
-> interaction
-> runtime-backed progress/reward/challenge/completion evidence
-> acceptance artifacts exist
```

## Docs/state

Update:

- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- manual verification doc.

After S040, recommended next work:

```text
manual_runtime_backed_microgame_verification
```

Do not recommend S041 or a new Codex feature slice until the user manually verifies the runtime-backed microgame loop.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~RuntimeMicrogameStateAcceptance"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario runtime-backed-microgame-state
.\.devflow\scripts\check-all.ps1
```

## Constraints

Do not add LLM/provider/Lua/Unity/media execution.

Do not build a final UI.

Do not unlock broad runtime/schema systems.

Do not proceed to another feature slice after this; stop for manual verification.

