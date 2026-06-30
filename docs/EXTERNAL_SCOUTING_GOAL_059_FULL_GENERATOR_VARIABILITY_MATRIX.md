# External Scouting — Goal 059 Full Generator Variability Regression Matrix

## Decision

Do not add external dependencies for Goal 059.

Goal 059 needs deterministic seed/profile matrix planning, replayability proofs, variance metrics, bounded matrix reduction, artifact evidence, and Unity Alpha player proof. This can be implemented with BCL-only C# and existing repository seams.

## Reviewed options

### FsCheck

- Use case: property-based testing for .NET.
- Value: can generate many inputs and check invariants.
- Risk now: new dependency surface, test-style shift, and weaker traceability than the repository's existing deterministic artifact matrix pattern.
- Decision: defer. A future testing-hardening goal may introduce property-based tests, but Goal 059 should preserve deterministic matrix rows and compact evidence.

### Microsoft PICT

- Use case: pairwise combinatorial test generation.
- Value: reduces large variant matrices to effective pairwise sets.
- Risk now: external tool dependency and different runtime/install assumptions.
- Decision: do not add. Implement a small deterministic in-house pairwise-ish matrix selector for known bounded dimensions if needed.

### Bogus

- Use case: fake/sample data generation for .NET.
- Value: useful for broad fake datasets.
- Risk now: generated fake content can hide domain causality. The generator needs semantic/domain-derived variation, not generic random names/data.
- Decision: do not add. Use existing semantic/profile/family evidence and deterministic seed derivation.

## Goal 059 dependency posture

- BCL-only Application seam.
- No new NuGet packages.
- No provider/LLM/RAG/media generation.
- No new Lua execution surface beyond consumed accepted evidence.
- Unity Alpha proof may be extended narrowly through existing `AlphaRuntimeBootstrap.cs` only if needed for matrix markers.

## Preferred implementation principle

Do not generate random-looking data. Produce deterministic, explainable variation from:

- accepted Goal 058 campaign facts;
- family ids;
- seed ids;
- semantic pack/style/lore knobs already present in evidence;
- media binding inventory;
- runtime/chunk/family-loop facts.

Every matrix row must be replayable and must record why it differs from the baseline.
