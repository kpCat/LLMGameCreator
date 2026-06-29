# External Scouting — Goal 037 Hybrid LLM Draft Plus Lua Deterministic Expansion

## Decision

Goal 037 may use one explicitly bounded external dependency only if the local repo restore/build proves it works and the dependency remains isolated behind an Application-layer executor adapter.

Preferred candidate for a real executor adapter:

- `LuaCSharp` NuGet package, pinned to a concrete version discovered during implementation, preferably the latest stable available locally.
- Rationale: pure C# Lua interpreter for .NET/Unity, MIT license, .NET Standard 2.1/.NET 6+ compatible, no native DLL packaging surface.

Rejected/deferred for this goal:

- MoonSharp: pure C# and proven historically, but older Lua 5.2-like surface and broader historical API exposure. Useful as a future adapter only.
- NLua/KeraLua: MIT, Lua 5.4/native bindings, but native packaging and bridge surface are too risky for this first bounded generator executor.
- Lua.NET: MIT and modern, but native/multi-version binding surface is not needed for a first sandboxed generator expansion slice.
- Luau/NuLua: interesting future family, but not the same contract as the existing Lua manifest registry and too broad for this goal.

## Dependency rule

Do not add any package unless all of the following are true:

1. License is MIT or otherwise clearly acceptable for this repo.
2. `dotnet restore` and `dotnet build` are green after adding it.
3. The dependency is referenced only where required for the bounded Application-layer executor adapter.
4. The implementation keeps deny-first sandbox policy and does not expose arbitrary .NET objects, filesystem, network, process, reflection, threading, wall-clock time, random, native interop or Runtime/GamePackage mutation.
5. If real execution cannot be implemented safely, commit/push a `BLOCKED Goal 037 ...` state with evidence and do not fake execution.

## Architecture bias

The goal is not “LLM writes content” and not “Lua owns the game”. The intended pipeline is:

```text
Strict Goal034 draft candidate
 -> Goal035 Lua module manifest selection
 -> Goal036 sandbox execution decision
 -> bounded deterministic Lua expansion adapter
 -> C# output validator
 -> promoted deterministic expansion artifact
```

LLM remains offline/draft-only. Lua remains deterministic expansion-only. C# remains the authority for validation, promotion, evidence and future materialization.
