# City-builder / simulation basics

Batch 016 adds deterministic Lua generator-library modules for city-builder and simulation baseline IR. The output is compact configuration data for later editor workflows, validators, UI modules and runtime adapters.

This is not a full runtime simulation engine. The modules do not execute live citizen state, job assignment, path search, economy loops or renderer logic. They produce JSON-serializable IR and diagnostics so the host can validate and later consume the data safely.

## Modules

### `lua/simulation/citizen_needs.lua`

Defines citizen need profile IR. A need contains:

- `id`: lowercase slash id such as `need/food`;
- `category`: broad grouping such as survival, housing, security or happiness;
- `priority` and `weight`: planner/balancing metadata;
- `decay_per_tick`: tick metadata only;
- `thresholds`: low, warning and satisfied values in range `0..1`;
- `satisfaction_sources`: service or building ids that can satisfy the need.

Diagnostics cover invalid ids, negative weights, invalid decay values, missing thresholds and threshold ordering.

### `lua/simulation/job_system_config.lua`

Defines job role and workplace assignment IR. A job contains:

- `id`: lowercase slash id such as `job/farmer`;
- `workplace_category` or `workplace_building_id`;
- `worker_capacity`;
- `required_tags` and `required_skills`;
- `shift` and tick metadata;
- `economy_hooks` such as wage or output references.

Diagnostics cover invalid ids, missing workplace references, invalid capacities and invalid shift metadata.

### `lua/simulation/building_catalog.lua`

Generates compact building catalog IR. A building contains:

- `id`;
- `category`: housing, production, service, storage or utility;
- `footprint` width and height;
- `build_costs` as resource amount metadata;
- `zone_tags`;
- hooks for services, jobs, housing, storage and economy references.

Diagnostics cover invalid footprint, missing category, invalid build amounts, duplicate ids and invalid zone metadata.

### `lua/simulation/service_coverage.lua`

Defines service coverage config IR. This module does not calculate real map coverage or solve paths. A service contains:

- `id`;
- provider building ids or categories;
- `radius` and `capacity` metadata;
- target tags;
- quality and priority metadata;
- optional need references.

Diagnostics cover invalid radius, capacity, provider references and duplicate services.

## Input, config and output shape

Every module accepts either a config table directly or `{ config = ... }`.

Every module returns:

- `ok`: boolean;
- `data`: generated IR when valid, otherwise empty table;
- `diagnostics`: array of diagnostic entries;
- `artifacts`: reserved array, currently empty.

Diagnostics use:

- `severity`;
- `code`;
- `message`;
- `target`.

Outputs use only JSON-serializable primitive values, arrays and dictionaries.

## Example use cases

### Small frontier town

Use citizen needs for food, rest and safety; buildings for houses, farms and clinics; jobs for builders, farmers and healers; and service coverage for markets and clinics.

### Industrial district

Use production buildings, worker jobs and service coverage metadata for storage, repairs and worker housing.

### Economy planning

Use building `build_costs`, job `economy_hooks` and service capacity metadata as inputs for later validation and balancing modules.

### Simulation tick planning

Use `tick_mode`, `decay_per_tick`, shifts and service metadata to describe how a future host simulation should interpret time. The modules do not run the tick loop.

## Compatibility with later batches

Batch 017 UI IR may already exist in the repository. Batch 016 does not depend on it. Later UI modules can consume city-builder metadata for build menus, need panels, service overlays and job screens. Later Unity, validation and orchestration batches can consume these same IR contracts without direct coupling here.
