# Product Slice 037 - Microgame Acceptance + Playability Polish

## Purpose

Close Goal 001 by making the generated preview manually playable as a small 5-minute microgame.

This is not a broad UI redesign. It is an acceptance/polish slice for the loop created in S035-S036.

## Functional goal

The user should be able to:

1. Open WinForms.
2. Go to Runtime Preview.
3. Click `Generate Preview`.
4. Click `Start`.
5. Read the active goal.
6. Move to/interact with generated NPC/object/item.
7. Resolve the generated challenge/encounter or equivalent obstacle.
8. See reward/progress/completion state.

## Required polish

Improve only what is needed for manual playability:

- clear active goal panel/summary;
- readable current objective;
- readable interaction/challenge/reward labels;
- clearer log entries for progress/completion;
- generated content summary remains useful;
- no large layout redesign.

## Acceptance model

Add a deterministic microgame acceptance report/snapshot if useful:

```text
.llmgc/procedural/generated-microgame-loop/
```

Expected evidence:

- package id/title;
- active goal id/title;
- objective id/title;
- challenge id/title;
- required interaction/item/NPC;
- reward/progress/completion state;
- runtime start/move/interact evidence;
- diagnostics/warnings.

## Tests

Add focused acceptance tests:

- one-click generated package contains active goal, challenge, reward/completion evidence;
- preview projection has readable labels;
- acceptance report is deterministic;
- manual verification doc is updated;
- no external execution.

Suggested filter:

```text
FullyQualifiedName~GeneratedMicrogameAcceptance
```

## Product smoke

Add scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-microgame-loop
```

Smoke must prove the full headless loop:

```text
generate
-> runtime start
-> active goal visible
-> interaction/challenge step
-> reward/progress/completion evidence
```

## Docs/state

Update:

- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- manual verification doc.

After S037, recommended next work:

```text
manual_microgame_loop_verification
```

Do not recommend S038 or a new Codex feature slice until the user manually verifies the generated microgame loop.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GeneratedMicrogameAcceptance"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-microgame-loop
.\.devflow\scripts\check-all.ps1
```

## Constraints

Do not add LLM/provider/Lua/Unity/media execution.

Do not build a final UI.

Do not unlock broad runtime/schema systems.

Do not proceed to another feature slice after this; stop for manual verification.

