# Rich Package Assembly Coverage Audit v1

Status: Goal 024 audit contract  
Final gate: `rich_package_assembly_coverage_audit_verification`

## Purpose

`rich_package_assembly_coverage_audit_v1` consumes accepted Goal 023 `capability_bundle_pipeline_inputs_v1` artifacts and audits current package assembly coverage before any package expansion work starts.

This contract is audit and planning data only. It must not mutate public `GamePackage` schema, assemble a new package for review, run Unity, call LLM/RAG/providers/media generation, execute arbitrary Lua, edit `generator-library`, or start Goal 025/S199.

## Required Provenance

Every audit report must record:

- previous accepted gate: `capability_bundle_pipeline_inputs_verification passed`;
- Goal 023 report path and hash;
- Goal 023 generator inputs path and hash;
- Goal 023 gap report path and hash;
- physical validation that Goal 023 report has `manualGate == capability_bundle_pipeline_inputs_verification`, `contractProofPassed == true`, `pipelineInputCount == 3`, and no top-level `severity=error` diagnostics;
- final manual gate: `rich_package_assembly_coverage_audit_verification`.

## Coverage Domains

The coverage matrix must include at least:

- `world`;
- `entities`;
- `quests`;
- `dialogue_interactions`;
- `items_inventory_economy`;
- `combat_progression`;
- `factions_social_work_theft_schedules`;
- `assets_runtime_export`.

Each domain records related profile ids, related Goal 023 pipeline input ids, candidate artifact contract ids, package schema areas or explicit absence, validator ids or explicit absence, runtime smoke evidence or explicit absence, support status, gap ids, and recommended next action.

## Evidence Classes

Coverage evidence must use one of these classes:

- `package_schema_field`;
- `package_validator`;
- `package_assembly_mapping`;
- `runtime_smoke`;
- `previous_goal_artifact`;
- `sidecar_only`;
- `future_required`;
- `blocked_gap`.

Docs-only mentions do not prove package support.

## Support Statuses

Allowed support statuses are:

- `package_supported`;
- `package_supported_partial`;
- `sidecar_only`;
- `future_required`;
- `blocked_gap`;
- `unsupported`.

Future-required and blocked-gap entries from Goal 023 must stay future-required or blocked. They must not be treated as package-supported.

## Required Artifacts

Goal 024 compact artifacts are written under:

```text
.llmgc/procedural/rich-package-assembly-coverage-audit/
```

Required files:

- `rich-package-assembly-coverage-matrix.json`;
- `rich-package-assembly-coverage-gap-report.json`;
- `rich-package-assembly-next-slice-plan.json`;
- `rich-package-assembly-coverage-invalid-matrix.json`;
- `rich-package-assembly-coverage-audit-report.json`;
- `rich-package-assembly-coverage-audit-report.md`;
- `rich-package-assembly-coverage-audit-verification.md`.

## Report Requirements

The final valid report must include:

```text
accepted=false
finalStatus=rich_package_assembly_coverage_audit_verification
manualGate=rich_package_assembly_coverage_audit_verification
previousAcceptedGate=capability_bundle_pipeline_inputs_verification passed
goal023EvidenceVerified=true
coverageDomainCount>=8
coverageMatrixWritten=true
gapReportWritten=true
nextSlicePlanWritten=true
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

- missing accepted Goal 023 report;
- stale or mismatched previous gate;
- copied coverage report without Goal 023 generator input artifact;
- fewer than three Goal 023 pipeline inputs;
- top-level error diagnostics in Goal 023 report;
- docs-only GamePackage mentions treated as package support;
- future-required capability treated as package-supported;
- blocked gap treated as ready for package assembly;
- public GamePackage schema mutation claim;
- package assembly execution claim;
- Unity build, LLM, RAG, provider, media or Lua execution claim;
- generator-library mutation claim;
- historical Goal 020/021/022/023 artifact mutation;
- duplicate coverage domain id;
- missing required coverage domain;
- Goal 025/S199 started marker.
