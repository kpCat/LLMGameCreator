# Module Contract Manifest v1

Status: Goal 029 contract  
Final gate: `modular_generator_kernel_parallel_readiness_verification`

## Purpose

`module_contract_manifest_v1` describes a deterministic repository-local
generator module. It is a static manifest contract, not dynamic plugin loading,
runtime Lua execution, a provider surface, or a new gameplay package expansion.

The first supported module kind is `package_assembly`. Goal 029 registers only
existing package assembly shapes so future module work can declare ownership,
dependencies, tests and product-smoke scenarios without every task growing the
same shared files.

## Required Fields

- `moduleId`: stable unique id such as `package_assembly_world_entities`.
- `moduleKind`: module family, initially `package_assembly`.
- `version`: deterministic contract version string.
- `ownedSourceRoots`: source roots the module owns.
- `ownedArtifactRoot`: compact artifact root owned by the module.
- `inputContracts`: accepted artifact contracts consumed by the module.
- `outputContracts`: package/generated-content contracts produced by the
  module.
- `requiredKernelCapabilities`: static kernel capabilities needed by the
  module.
- `requiredDependencies`: module ids that must be present.
- `optionalDependencies`: module ids that may be absent.
- `absenceBehavior`: `required_module` or `optional_absent_allowed`.
- `validators`: validators or guard helpers required for proof.
- `focusedTestFilter`: focused test filter for module proof.
- `productSmokeScenario`: scenario id for integration smoke.
- `forbiddenRuntimeDependencies`: forbidden execution surfaces that must remain
  absent from the module.
- `deterministicHashRules`: ordering and path/timestamp rules used for stable
  artifacts.

## Validation

Validators must reject:

- missing required fields;
- duplicate `moduleId`;
- unknown input or output contract ids;
- missing required dependencies;
- required runtime dependency on LLM, RAG, provider/media, Unity, WinForms UI or
  arbitrary Lua execution;
- artifact roots outside `.llmgc/procedural/`;
- module-only proof that claims a product vertical gate.

Optional dependencies are allowed to be absent only when reported as
`absent_optional`.

## Non-Goals

- No public `GamePackage` schema change.
- No `.sln` or `.csproj` change.
- No WinForms UI, Unity build/player, provider/media/RAG/LLM/Lua execution.
- No generator-library mutation.
- No Goal 030/S234 work.
