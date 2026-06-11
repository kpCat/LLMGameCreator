-- type: generator.lua
-- mode: finite_world
-- purpose: Generate a bounded overworld map by chunk.
-- contract: function generate_chunk(ctx) -> GeneratedChunkDraft

function generate_chunk(ctx)
    local chunk = llmgc.chunks.new(ctx.chunk_size, ctx.chunk_size)

    local world_width_chunks = ctx.world_width_chunks or 8
    local world_height_chunks = ctx.world_height_chunks or 8

    local is_border_chunk = ctx.chunk_x == 0
        or ctx.chunk_y == 0
        or ctx.chunk_x == world_width_chunks - 1
        or ctx.chunk_y == world_height_chunks - 1

    for y = 0, ctx.chunk_size - 1 do
        for x = 0, ctx.chunk_size - 1 do
            local wx = llmgc.procedural.world_x(ctx, x)
            local wy = llmgc.procedural.world_y(ctx, y)
            local n = ctx:noise2d(ctx.world_seed, wx, wy, 0.04)

            if is_border_chunk and n < 0.35 then
                llmgc.chunks.add_tile(chunk, x, y, "tile/mountain", "ground")
            elseif n < 0.18 then
                llmgc.chunks.add_tile(chunk, x, y, "tile/water", "ground")
            elseif n > 0.76 then
                llmgc.chunks.add_tile(chunk, x, y, "tile/forest_floor", "ground")
            else
                llmgc.chunks.add_tile(chunk, x, y, "tile/grass", "ground")
            end
        end
    end

    -- Example fixed story anchor near the center of a finite map.
    local center_x = math.floor(world_width_chunks / 2)
    local center_y = math.floor(world_height_chunks / 2)
    if ctx.chunk_x == center_x and ctx.chunk_y == center_y then
        llmgc.chunks.add_entity(chunk, "prototype/location/village_marker", 7, 7, {
            portal = { mapId = "map/village", spawnId = "spawn/main_gate" },
            interactable = { interactionId = "interaction/enter_village" }
        })
    end

    chunk.metadata = {
        generationMode = "finite_overworld_map"
    }

    return chunk
end
