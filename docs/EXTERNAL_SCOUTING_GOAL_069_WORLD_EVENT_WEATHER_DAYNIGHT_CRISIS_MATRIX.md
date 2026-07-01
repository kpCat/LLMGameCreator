# External Scouting — Goal 069 World Event / Weather / Day-Night / Crisis Matrix

## Decision

Do not add external dependencies in Goal 069.

The goal needs a deterministic game-world environment/event pressure model, not a real-time calendar library or procedural-noise dependency.

## Considered

### NodaTime

NodaTime is a strong .NET date/time library under Apache-2.0 and is useful for real-world date/time correctness. It is not needed for Goal 069 because the game should use a deterministic fantasy/game clock rather than real local time/time zones.

Decision: defer. Use BCL-only deterministic records.

### FastNoiseLite

FastNoiseLite is MIT and useful for procedural noise/terrain/weather field generation. It is promising for future map/weather/climate fields, but Goal 069 should not introduce a dependency for the first world-event/weather proof.

Decision: defer as optional future adapter.

### libnoise variants

Useful historically for coherent noise, but licensing and ports vary. Not needed for this goal.

Decision: do not use.

### Unity weather/sky assets

Not appropriate here. Goal 069 is a generator proof and Alpha marker proof, not visual weather rendering.

Decision: do not use.

## Future candidates

Future world/climate/map detail goals may consider:

- FastNoiseLite for deterministic climate/noise fields.
- WFC/MarkovJunior/TextureSynthesis cluster for spatial local detail.
- Voxel/ray-traced audio accessibility ideas after geometry/rooms/openings/weather direction become stable.

## Current goal policy

Implement a BCL-only Application seam:

- deterministic world clock/calendar policy
- day-night phase states
- weather/hazard catalog
- crisis/global event catalog
- cross-system effects
- save/load/replay proof
- Unity Alpha marker proof

No provider, no LLM, no real weather, no real-time dependency.
