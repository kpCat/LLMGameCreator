# External scouting — Goal 036 Lua Sandbox Execution Gate

Status: internet-scouted before task creation.

Goal 036 must not pick a production Lua runtime yet. The target is an Application-layer execution **gate**: sandbox policy, host API binding, budget planning, dry-run trace, rejection/repair decisions, and evidence. Real Lua parsing/execution/source generation remains out of scope unless a later explicit executor-adapter goal chooses a runtime.

## Candidates reviewed

### MoonSharp

- Kind: managed C# Lua interpreter for .NET/Mono/Xamarin/Unity.
- Fit: strong future candidate for a managed optional executor adapter because it is pure C# and Unity-friendly.
- Risk: Lua 5.2-like surface, not Lua 5.4; object exposure has sharp edges if public members are exposed too broadly; an executor adapter must register a narrow whitelist only.
- Decision for Goal 036: do not add dependency; model it as a future optional adapter candidate.

### Lua-CSharp

- Kind: high-performance Lua interpreter implemented in C# for .NET and Unity.
- Fit: future managed optional executor adapter candidate; MIT license.
- Risk: newer/smaller ecosystem than MoonSharp; integration/sandbox behavior needs a dedicated spike; still an executor decision.
- Decision for Goal 036: do not add dependency; keep as future optional adapter candidate.

### NLua / KeraLua

- Kind: .NET bridge/native bindings for Lua 5.4.
- Fit: useful future candidate if exact Lua 5.4 compatibility is more important than fully managed deployment.
- Risk: native runtime packaging; platform-specific binaries; .NET object bridge can be dangerous if not fenced; Unity/IL2CPP implications need careful adapter work.
- Decision for Goal 036: do not add dependency.

### Lua.NET

- Kind: native Lua bindings supporting several Lua versions/LuaJIT.
- Fit: possible future compatibility/performance candidate.
- Risk: newer/smaller adoption surface; native binding and sandbox risks; too early for core.
- Decision for Goal 036: do not add dependency.

### Relua

- Kind: Lua parser/transformation tooling.
- Fit: useful later for source review/import tooling if Lua source files become a real artifact.
- Risk: parser/source-transformation is explicitly not the Goal 036 target.
- Decision for Goal 036: do not add dependency.

## Goal 036 dependency decision

No external dependencies now.

Implement BCL-only contracts and deterministic gates:

- sandbox policy model;
- host API binding policy;
- execution request and budget records;
- dry-run/probe plan records;
- deterministic deny-first decision engine;
- repair planner;
- compact evidence.

The implementation must make a future executor adapter safer, but must not become the executor itself.
