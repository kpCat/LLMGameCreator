# World Biome Noise Candidate Contract

Candidate id: `candidate_world_biome_noise_v1`  
Contract id: `world_biome_noise_contract_v1`  
Final candidate status: `candidate_ready_for_serial_adoption`  
Base accepted gate supplied by task: `modular_generator_kernel_parallel_readiness_verification passed`

## Status

This is candidate-only work. It does not claim production integration, an accepted manual gate, public `GamePackage` schema support, Unity runtime support, generator-library support, Lua execution or FastNoise Lite dependency adoption.

## Deterministic Noise Input Contract

Inputs:

- `seed`: required stable string, trimmed before hashing.
- `rulesVersion`: required stable string, for example `world_biome_noise_rules_v1`.
- `coordinateSpace`: one of `world_cell`, `chunk_cell`, `region_anchor`.
- `dimension`: `2d` for this candidate; `3d` is reserved for future adapter parity with FastNoise Lite.
- `channelId`: stable id such as `elevation`, `moisture`, `temperature` or `roughness`.
- `x`, `y`, optional `z`: signed integer coordinates after normalization.
- `normalizationVersion`: required stable string, for example `hash_score_0_10000_v1`.

Output:

- `score0To10000`: integer score from `0` to `10000`.
- `normalizedMinusOneToOne`: report-only decimal view derived from the score and rounded/serialized deterministically when needed.

The canonical comparison value is `score0To10000`. Raw external-library float output must be quantized to the same score range before validation or golden-file comparison.

## Coordinate Normalization Rules

- Inputs use integer world cells or chunk-local cells.
- Chunk-local sampling must derive world cell coordinates before sampling:

```text
world_x = chunk_x * chunk_size + local_x
world_y = chunk_y * chunk_size + local_y
```

- `chunkSize` must be positive.
- Coordinate origin is contract-specific and must be recorded in the consuming artifact.
- Same seed, rules version, channel and normalized coordinates must produce the same scores.
- Different seeds should produce visible variation without changing artifact shape.

## Biome Classification Contract

Biome classification consumes quantized channel scores, not raw provider noise:

| Biome id | Elevation score | Moisture score | Notes |
| --- | --- | --- | --- |
| `biome/water` | `0..2499` | any | Low elevation dominates. |
| `biome/alpine` | `7500..10000` | any | High elevation dominates. |
| `biome/desert` | `2500..7499` | `0..2999` | Dry midlands. |
| `biome/forest` | `2500..7499` | `6500..10000` | Wet midlands. |
| `biome/plains` | `2500..7499` | `3000..6499` | Default midland fallback. |

Tie/boundary behavior is inclusive and ordered exactly as listed above.

## Adapter Boundary

Recommended future serial-adoption seam:

```text
WorldBiomeNoiseInput
  -> ISeededNoiseSampler.Sample(input)
  -> BiomeClassifier.Classify(scores, biomeTable)
  -> biome/chunk/world artifacts
```

FastNoise Lite, if adopted, should be one `ISeededNoiseSampler` implementation. The classifier and artifacts should not depend on FastNoise Lite types or enums.

## Absence Behavior

If an external noise adapter is absent:

- use the project-owned deterministic fallback sampler;
- add diagnostic `world_biome_noise.external_adapter.absent_optional`;
- keep output deterministic;
- do not silently claim FastNoise-backed evidence.

If no deterministic sampler is available:

- reject with `world_biome_noise.sampler.missing`;
- do not generate biome assignments.

## Validation Strategy

Required focused validation:

- same seed and coordinates produce identical report hashes;
- different seed changes at least one score or biome while preserving sample count/shape;
- invalid empty seed is rejected;
- invalid coordinate space is rejected;
- classifier boundaries are stable for water/alpine/desert/forest/plains;
- report records `fastNoiseLiteDependencyAdopted=false`;
- report records no live LLM/RAG/provider/media/network/Lua/Unity execution.

## Serial Adoption Recommendation

Adopt the contract and fallback first. Then decide whether to add a FastNoise Lite adapter in a serial dependency task with:

- exact upstream version/source pin;
- MIT attribution handling;
- project-file/source-inclusion decision;
- quantized cross-platform golden fixtures;
- adapter replacement tests.

