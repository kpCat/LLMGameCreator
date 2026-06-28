# External Technology Scouting - World Biome Noise

Subsystem: deterministic world biome/noise generation  
Candidate id: candidate_world_biome_noise_v1  
Date: 2026-06-29  
Agent: Codex

## Search Scope

- Libraries: FastNoise Lite upstream repository and C# source.
- Algorithms: seeded value/simplex/perlin-style noise, deterministic integer-hash fallback.
- Existing .NET packages: repo-local `*.csproj`, `*.props` and `*.targets` package references.
- Existing repo-local helpers: `docs/PROCEDURAL_WORLD_GENERATION.md`, `docs/WORLD_TOPOLOGY_AND_CHUNKING_CONTRACTS.md`, `Design/World/ConnectedWorldTravelAcceptanceService.cs`, Goal 029 modular kernel manifests and package assembly module patterns.

Local search found no existing FastNoise, Perlin, Simplex, OpenSimplex or general .NET noise package dependency in `src` or `tests`. Existing repo guidance already requires deterministic seed-based random/noise and names future `biome_pack_v1`, `chunk_rule_pack_v1`, `world_profile_v1`, `world_chunk_config_v1` and `runtime_chunk_delta_v1` contracts.

## Candidates Reviewed

| Candidate | Type | License | Runtime dependency? | Offline usable? | Deterministic? | Decision | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| FastNoise Lite | Portable noise library | MIT according to upstream LICENSE | Optional only if adopted later | Yes | Seeded API; float/double stability must be pinned by adapter tests | reference_only | Upstream README lists C# support, 2D/3D sampling, multiple algorithms and float/double support. Current candidate does not add it as dependency because `.csproj` edits are forbidden. |
| Repo-local noise utilities | Existing code/helpers | Project-owned | No new dependency | Yes | Not found as implementation | defer | Repo has world/chunk docs and connected-world deterministic evidence, but no reusable numeric noise helper. |
| Deterministic integer-hash value field | Built-in fallback/reference | Project-owned | No | Yes | Yes, byte-stable via SHA-256 input key | adapt_behind_adapter | Good baseline for contract tests and absence behavior; not a rich terrain-noise replacement. |

## FastNoise Lite Findings

- Upstream repository: https://github.com/Auburn/FastNoiseLite
- License: MIT license in upstream `LICENSE`.
- C# support: upstream README lists C# among supported languages.
- 2D/3D support: upstream README lists 2D and 3D sampling; C# source exposes 2D and 3D `GetNoise` overloads.
- Seed behavior: C# source has constructor seed and `SetSeed(int seed)`.
- Float/double concern: C# source uses a `FNLfloat` alias that defaults to `System.Single` with a commented `System.Double` alternative. LLMGameCreator should not persist raw unchecked float outputs as cross-platform golden data without quantization.
- Build/package impact: direct adoption would require adding source or package/project references. This candidate does not do that.
- Network/provider dependency: none indicated by upstream code/library; no live API is required.

## Accepted/Adapted/Reference/Rejected Decision

- Accepted: none.
- Adapted behind adapter: project-owned deterministic integer-hash value field as fallback/reference.
- Used as reference only: FastNoise Lite.
- Rejected: direct FastNoise Lite dependency in this candidate because project file changes are forbidden and serial adoption should decide dependency shape.
- Deferred: richer FastNoise Lite adapter, pinned version, attribution file and cross-platform quantized fixtures.

## Adapter Boundary

- LLMGameCreator contract: `world_biome_noise_contract_v1`.
- Adapter name: `ISeededNoiseSampler` in future serial adoption, with inputs equivalent to seed, rules version, channel id, coordinate space, integer coordinates, dimension and normalization version.
- External dependency boundary: FastNoise Lite may sit behind the sampler only after adoption; generated packages must store LLMGameCreator contract fields, not FastNoise-specific config as public schema.
- Replacement plan: keep contract tests against deterministic samples and biome classifications; swap the sampler implementation from fallback to FastNoise Lite without changing classifier inputs/outputs.

## Risk Notes

- License/attribution: MIT is permissive but requires retaining copyright/license notice if source is copied or distributed.
- Runtime footprint: FastNoise Lite C# source is compact, but direct source inclusion or package adoption still affects build inventory and attribution.
- Build impact: direct adoption needs `.csproj` or source inclusion, both outside this candidate.
- Testability: fallback can be tested with exact integer scores; FastNoise adapter should quantize outputs before golden assertions.
- Determinism: integer-hash fallback is stable by construction; FastNoise should be pinned to version/config and tested for same-seed/same-coordinate stability.
- Maintenance: upstream appears mature, but adoption should pin exact version/source and record update policy.
- Security: no live network path is needed.
- Paid/proprietary/API dependency: none.

## Conclusion

FastNoise Lite is a strong future adapter candidate, not a hard dependency for this parallel candidate. The safe serial-adoption path is:

```text
world_biome_noise_contract_v1
  -> ISeededNoiseSampler
  -> project-owned deterministic fallback
  -> optional FastNoise Lite adapter after serial dependency review
```

