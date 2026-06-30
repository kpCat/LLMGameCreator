# External scouting - Goal 058 Full Media-Bound Generator Campaign

## Decision

Do not add external dependencies in Goal 058.

Goal 058 is an orchestration/proof goal. The repository already has the necessary seams:

- strict LLM draft artifact loop;
- Lua manifest registry;
- Lua sandbox gate;
- bounded LuaCSharp expansion path;
- world/chunk/runtime delta proofs;
- multi-family simulatable loop proof;
- full generator without media dry-run;
- media campaign/materialization/review package proof;
- Unity Alpha media-bound and multi-family player proof.

The next value is not a new library. The next value is a single deterministic campaign runner that consumes these accepted/proven artifacts and produces a unified review/playable package proof.

## Considered options

### System.CommandLine

Potential use: a future CLI entrypoint for one-click generator campaigns.

Decision: defer.

Reason: Goal 058 can expose an Application-layer campaign runner and product smoke first. A CLI can be added later when the contract is stable. Adding CLI infrastructure now increases maintenance and does not prove the generator loop better than product smoke.

### Microsoft.Extensions.DependencyInjection / hosting abstractions

Potential use: orchestrating pipeline stages.

Decision: do not add.

Reason: the repository already has local Application seams and tests. Goal 058 should stay BCL-only and deterministic.

### Pipeline/workflow libraries

Potential use: state machine or workflow runner.

Decision: do not add.

Reason: the domain needs causal diagnostics, evidence hashes, source manifests and replayable JSON proof. Generic workflow libraries do not reduce the hard part.

### Unity packages

Potential use: richer UI, Addressables, media loading helpers.

Decision: do not add.

Reason: Goal 056/057 already proved the narrow Unity Alpha route. Goal 058 should reuse the existing player/manifest path, not broaden Unity dependencies.

## Recommended future adapters, not for Goal 058

- CLI adapter for running generator campaigns from command line.
- Optional Unity Addressables/export adapter after media package shape stabilizes.
- Optional real media provider adapters after license/provenance/import rules are mature.
- Optional authoring UI after the campaign contract is stable enough to edit.

## Goal 058 dependency stance

Allowed:

- BCL-only Application code.
- Existing repository Unity Alpha route, only if needed for proof.
- Existing LuaCSharp dependency introduced by Goal 037.
- Existing evidence/product smoke patterns.

Forbidden:

- New NuGet dependencies.
- Provider/LLM/RAG calls.
- Real media generation/import/network.
- Broad Unity/UI/Runtime/GamePackage schema rewrites.
