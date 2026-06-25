# Product Slice 042 - Microgame Variation Acceptance

## Purpose

Close Goal 002 by proving that configurable generated microgames vary deterministically while remaining playable.

S042 should not add major new mechanics. It should verify that the S038-S041 runtime-backed configurable microgame path is not a one-seed illusion.

## Functional goal

Headless acceptance should run multiple deterministic variants:

- at least 3 seeds;
- at least 2 presets/options if S041 provides them;
- each accepted variant starts runtime;
- each accepted variant exposes active goal;
- each accepted variant has runtime-backed progress/reward/challenge/completion evidence;
- variants differ in meaningful generated ids/labels/maps/items/quests/encounters.

## Required artifacts

Write deterministic artifacts under:

```text
.llmgc/procedural/generated-microgame-variation/
```

Expected files:

- `generated-microgame-variation-report.json`
- `generated-microgame-variation-report.md`
- `manual-configurable-microgame-verification.md`

Report should include:

- seed/preset matrix;
- package ids/titles;
- active goals;
- challenge/reward/completion evidence;
- variation/difference summary;
- failures/diagnostics per variant;
- no external execution diagnostic.

## Required behavior

Minimum acceptable behavior:

- default variant passes;
- at least two non-default variants pass;
- variation report shows differences;
- deterministic re-run produces byte-stable report for the same matrix;
- state stops at manual configurable microgame verification.

If some variant fails, do not hide it. Either repair the generator deterministically or mark the goal blocked with exact failure evidence.

## Tests

Add focused acceptance tests:

- same variant matrix is byte-stable;
- variants differ in meaningful ids/labels;
- all accepted variants have runtime-backed or explicitly validated microgame evidence;
- no external execution.

Suggested filter:

```text
FullyQualifiedName~MicrogameVariationAcceptance
```

## Product smoke

Add scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-microgame-variation
```

Smoke must prove:

```text
variant matrix
-> accepted generated microgame loop for each required variant
-> variation report artifacts exist
```

## Docs/state

Update:

- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- manual verification docs.

After S042, recommended next work:

```text
manual_configurable_microgame_verification
```

Do not recommend S043 or another Codex feature slice until the user manually verifies configurable generated microgames.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~MicrogameVariationAcceptance"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-microgame-variation
.\.devflow\scripts\check-all.ps1
```

## Constraints

Do not add LLM/provider/Lua/Unity/media execution.

Do not add large new mechanics.

Do not redesign package/runtime/UI broadly.

Do not proceed to another feature slice after this; stop for manual verification.

