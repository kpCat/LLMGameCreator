# Capability Bundle Pipeline Inputs Contract v1

Status: Goal 023 planning contract  
Final gate: `capability_bundle_pipeline_inputs_verification`

## Purpose

`capability_bundle_pipeline_inputs_v1` turns accepted `game_profile_v1` files into deterministic capability-bundle selections and concrete generator pipeline input records.

This contract is planning data only. It must not mutate public `GamePackage` schema, assemble packages, run Unity, call LLM/RAG/providers/media generation, execute arbitrary Lua, or edit `generator-library`.

## Required Provenance

Every pipeline input record must include:

- source accepted profile id;
- source profile path and source profile hash;
- previous accepted gate: `development_complexity_stabilization_verification passed`;
- provenance to accepted Goal 021 profile artifacts;
- provenance to Goal 022 artifact-scope policy;
- final manual gate: `capability_bundle_pipeline_inputs_verification`.

## Required Selection Fields

For each accepted profile, the selection must preserve explicit ids for:

- presentation mode;
- world topology;
- actor model;
- inventory model;
- combat model;
- progression model;
- pathfinding profile;
- NPC behavior model;
- selected feature bundle ids;
- runtime target ids.

Derived ids are allowed only when they are deterministic from the accepted profile. Accepted profiles and atlas files must not be rewritten to hide mismatches.

## Required Resolved Fields

Each selection and pipeline input record must include:

- resolved capability ids;
- resolved artifact contract ids;
- resolved validator ids;
- resolved prompt context template ids;
- resolved runtime target ids;
- selected feature bundle ids;
- expected downstream generation stages;
- package assembly candidate inputs expressed as ids/contracts only.

## Support Separation

The contract must explicitly separate:

- `supported_now` capability ids;
- `future_required` capability ids;
- `blocked_gap` ids for impossible or incompatible current atlas/profile combinations.

Future-required capabilities from Goal 021 must remain future-required. Atlas incompatibilities or missing atlas target ids must be visible as gaps, not treated as completed support.

## Required Artifacts

Goal 023 compact artifacts are written under:

```text
.llmgc/procedural/capability-bundle-pipeline-inputs/
```

Required files:

- `capability-bundle-pipeline-inputs-profile-requests.json`;
- `capability-bundle-pipeline-inputs-selection.json`;
- `capability-bundle-pipeline-inputs-generator-inputs.json`;
- `capability-bundle-pipeline-inputs-gap-report.json`;
- `capability-bundle-pipeline-inputs-invalid-matrix.json`;
- `capability-bundle-pipeline-inputs-report.json`;
- `capability-bundle-pipeline-inputs-report.md`;
- `capability-bundle-pipeline-inputs-verification.md`.

## Report Requirements

The final valid report must include:

```text
accepted=false
finalStatus=capability_bundle_pipeline_inputs_verification
manualGate=capability_bundle_pipeline_inputs_verification
previousAcceptedGate=development_complexity_stabilization_verification passed
profileCount=3
pipelineInputCount=3
capabilitySelectionStarted=true
packageAssemblyExecuted=false
publicGamePackageSchemaChanged=false
projectFilesChanged=false
generatorLibraryChanged=false
unityBuildExecuted=false
llmRagProviderMediaLuaExecuted=false
scopeGuardPassed=true
invalidMatrix.passed=true
```

Top-level diagnostics must contain no `severity=error` when `contractProofPassed=true`.

## Invalid/Fake/Leak Matrix

The matrix must reject at least:

- missing accepted Goal 022 gate;
- missing accepted Goal 021 profile artifacts;
- copied capability-selection report without profile files;
- unknown or duplicate profile ids;
- unknown feature bundle or runtime target ids;
- presentation/topology incompatibility treated as complete;
- future capability marked supported-now;
- package assembly, Unity build, LLM/RAG/provider/media/Lua execution claims;
- public GamePackage schema or generator-library mutation claims;
- cross-family leakage into frontier-only ids;
- historical Goal 021/020 artifact mutation.
