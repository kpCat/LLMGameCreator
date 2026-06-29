# External scouting — Goal 035 Lua Module Manifest Registry

## Decision

Do **not** add external dependencies in Goal 035.

Goal 035 is not a Lua execution goal. It is a BCL-only Application-layer manifest/registry/review contract goal. The purpose is to make future Lua/manual/import/LLM module input safe before any interpreter, generator-library import, runtime binding, or Unity integration is allowed.

## Checked options

### Lua language / official runtime

Lua itself is MIT-licensed and broadly suitable for commercial use, but adopting a runtime/interpreter is not needed for this goal.

Observed current official Lua information:
- Lua 5.4 readme/license states Lua is free software under the MIT license and can be used commercially with copyright notice preservation.
- Lua download page currently advertises Lua 5.5.0 as current. This matters only for future runtime selection, not for this manifest-only goal.

Risk:
- Selecting a concrete Lua version now would prematurely constrain future script semantics.
- A manifest registry should support declared target dialects without executing code.

Goal 035 decision:
- Model target dialect as metadata: `lua_5_2`, `lua_5_4`, `lua_5_5_candidate_or_later`, `manifest_only`.
- Default all seed manifests to `manifest_only` or `lua_5_4_future`.
- No interpreter or parser.

### MoonSharp

MoonSharp is a Lua interpreter written entirely in C# for .NET/Mono/Unity. The project is historically popular, Unity-friendly, and its license is permissive/new-BSD style.

Pros:
- Pure C# style is attractive for sandboxing and portability.
- Unity history is useful.
- No native DLLs.

Risks:
- Older NuGet package history and possible maintenance/performance concerns.
- Lua dialect is historically Lua 5.2-like, not current Lua 5.4/5.5.
- Taking it now would turn Goal 035 into runtime selection rather than manifest governance.

Goal 035 decision:
- Consider as a future optional adapter only.
- Do not add dependency.

### NLua / KeraLua

NLua is a bridge between Lua and .NET; KeraLua provides native bindings around Lua 5.4 and is MIT-licensed.

Pros:
- Closer to official Lua runtime behavior.
- KeraLua targets Lua 5.4 and multiple platforms.

Risks:
- Native binding/runtime packaging complexity.
- .NET interop can be powerful enough to become unsafe unless sandbox policy is strict.
- Not needed for manifest-only registry.

Goal 035 decision:
- Consider as future optional adapter only after API surface contracts exist.
- Do not add dependency.

### Lua-CSharp

Lua-CSharp is a newer C# Lua interpreter with MIT license, .NET/Unity support, async API, and performance positioning.

Pros:
- Pure C# and modern .NET/Unity positioning.
- MIT license.
- Could be evaluated later for a sandboxed executor.

Risks:
- Still an interpreter decision and not required for registry.
- Thread-safety/runtime behavior/sandbox limitations must be tested before adoption.
- Would expand scope.

Goal 035 decision:
- Future candidate for optional executor adapter.
- Do not add dependency.

## Architectural implication

Goal 035 must create a manifest registry that is interpreter-agnostic:

- module family and version;
- target dialect declaration, but no execution;
- allowed/denied host API groups;
- dependency graph;
- semantic/artifact contract bindings;
- request/candidate/review provenance;
- side-effect policy;
- resource budget metadata;
- promotion status;
- diagnostics and evidence.

Future executor goals may adapt MoonSharp/NLua/KeraLua/Lua-CSharp behind this registry, but the registry must not depend on any of them.
