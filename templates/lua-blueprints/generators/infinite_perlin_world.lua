-- type: generator.lua
-- mode: infinite_chunk
-- purpose: Infinite chunk generation using deterministic noise.
-- contract: function generate_chunk(ctx) -> GeneratedChunkDraft

function generate_chunk(ctx)
    local chunk = llmgc.chunks.new(ctx.chunk_size, ctx.chunk_size)

    local biome_rules = {
        { min = 0.00, max = 0.25, biomeId = "biome/water" },
        { min = 0.25, max = 0.38, biomeId = "biome/sand" },
        { min = 0.38, max = 0.72, biomeId = "biome/grassland" },
        { min = 0.72, max = 1.00, biomeId = "biome/forest" }
    }

    for y = 0, ctx.chunk_size - 1 do
        for x = 0, ctx.chunk_size - 1 do
            local wx = llmgc.procedural.world_x(ctx, x)
            local wy = llmgc.procedural.world_y(ctx, y)

            local height = ctx:noise2d(ctx.world_seed, wx, wy, 0.025)
            local biome_id = llmgc.procedural.pick_biome_by_noise(height, biome_rules, "biome/grassland")

            if biome_id == "biome/water" then
                llmgc.chunks.add_tile(chunk, x, y, "tile/water", "ground")
            elseif biome_id == "biome/sand" then
                llmgc.chunks.add_tile(chunk, x, y, "tile/sand", "ground")
            elseif biome_id == "biome/forest" then
                llmgc.chunks.add_tile(chunk, x, y, "tile/forest_floor", "ground")
                if ctx:noise2d(ctx.world_seed + 17, wx, wy, 0.12) > 0.68 then
                    llmgc.chunks.add_entity(chunk, "prototype/object/tree", x, y, {
                        collidable = { blocksMovement = true },
                        harvestable = { lootTableId = "loot/wood_basic" }
                    })
                end
            else
                llmgc.chunks.add_tile(chunk, x, y, "tile/grass", "ground")
            end
        end
    end

    -- Rare points of interest. Keep this deterministic and sparse.
    if ctx:noise2d(ctx.world_seed + 101, ctx.chunk_x, ctx.chunk_y, 1.0) > 0.92 then
        local px = ctx:random_int(2, ctx.chunk_size - 3)
        local py = ctx:random_int(2, ctx.chunk_size - 3)
        llmgc.chunks.add_entity(chunk, "prototype/object/ancient_shrine", px, py, {
            interactable = { interactionId = "interaction/inspect_ancient_shrine" }
        })
    end

    chunk.metadata = {
        generationMode = "infinite_perlin_world",
        chunkX = ctx.chunk_x,
        chunkY = ctx.chunk_y
    }

    return chunk
end
