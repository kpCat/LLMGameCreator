# Candidate World Region Climate Report

- Candidate id: candidate_world_region_climate_v1
- Base candidate id: candidate_world_biome_noise_v1
- Contract id: world_region_climate_contract_v1
- Final status: candidate_ready_for_serial_adoption
- Contract proof passed: true
- Score sampler: sha256_score_0_10000_v1_reused
- Biome classifier: world_biome_noise_contract_v1_classifier_reused
- Climate logic: latitude_like_coordinate_plus_sha256_variation_plus_elevation_cooling
- Deterministic hash: be531d231f26e522ed04ea447da8c72a153161fb481a50fc75a7037659f0fba2
- Global map materialized: false
- Settlement generation implemented: false
- Road generation implemented: false
- Faction generation implemented: false

## External Scouting Decisions

- H3: reference_only; No dependency; useful future reference for hex indexing.
- S2: reference_only; No dependency; useful future reference for spherical cell partitioning.
- AutoBiomes: reference_only; No dependency; useful future reference for authoring biome rules.
- PCG papers: reference_only; No dependency; used only as conceptual reference.

## Samples

| X | Y | Elevation | Moisture | Temperature | Ruggedness | Climate | Biome | Region | Settlement | Road cost |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| -9 | 11 | 0.849 | 0.2053 | 0 | 0.2145 | polar | biome/alpine | region/-1_0/cc8616f2 | 0 | 0.5409 |
| 0 | 0 | 0.0397 | 0.1637 | 0.0377 | 0.0139 | polar | biome/water | region/0_0/fc296e11 | 0 | 0.6328 |
| 7 | 3 | 0.796 | 0.1438 | 0 | 0.1875 | polar | biome/alpine | region/0_0/fc296e11 | 0 | 0.5207 |
| 16 | -4 | 0.923 | 0.367 | 0 | 0.7415 | polar | biome/alpine | region/1_-1/3088a9dc | 0 | 0.7784 |
| 32 | 32 | 0.2174 | 0.4869 | 0.3724 | 0.9939 | cold | biome/water | region/2_2/c92d8d2d | 0 | 0.9401 |
| 128 | -96 | 0.6269 | 0.6267 | 0.2294 | 0.8047 | cold | biome/plains | region/8_-6/9d14365c | 0.2553 | 0.4549 |

## Region Summaries

| Region | Dominant biome | Avg temp | Avg moisture | Settlement | Road suitability | Samples |
| --- | --- | --- | --- | --- | --- | --- |
| region/-1_0/cc8616f2 | biome/desert | 0.0787 | 0.2735 | 0.1242 | 0.4736 | 9 |
| region/0_0/fc296e11 | biome/alpine | 0.1026 | 0.3279 | 0.0748 | 0.3758 | 9 |
| region/1_-1/3088a9dc | biome/alpine | 0.0475 | 0.3261 | 0.049 | 0.3966 | 9 |
| region/2_2/c92d8d2d | biome/alpine | 0.4303 | 0.3191 | 0.0911 | 0.4548 | 9 |
| region/8_-6/9d14365c | biome/plains | 0.4834 | 0.5185 | 0.2176 | 0.5711 | 9 |

This candidate keeps huge-world behavior coordinate-derived: samples and summaries are calculated from seed plus coordinate only, without mutable global RNG state or full-map materialization.
Forbidden files remain outside this candidate proof: public GamePackage schema, project files, current-state handoff, UI, Unity/runtime/provider/LLM/RAG/media/Lua/generator-library.
