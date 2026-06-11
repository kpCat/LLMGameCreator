-- type: generator.lua
-- mode: biome_chunk
-- purpose: Generate one chunk according to an already selected biome.
-- contract: function generate_chunk(ctx) -> GeneratedChunkDraft

function generate_chunk(ctx)
    local chunk = llmgc.chunks.new(ctx.chunk_size, ctx.chunk_size)
    local biome_id = ctx.biome_id or "biome/grassland"

    local ground_tile = "tile/grass"
    local tree_prototype = "prototype/object/tree"
    local resource_chance = 0.03

    if biome_id == "biome/swamp" then
        ground_tile = "tile/swamp_mud"
        tree_prototype = "prototype/object/dead_tree"
        resource_chance = 0.05
    elseif biome_id == "biome/snow" then
        ground_tile = "tile/snow"
        tree_prototype = "prototype/object/pine_tree"
        resource_chance = 0.02
    elseif biome_id == "biome/desert" then
        ground_tile = "tile/sand"
        tree_prototype = "prototype/object/cactus"
        resource_chance = 0.015
    elseif biome_id == "biome/forest" then
        ground_tile = "tile/forest_floor"
        tree_prototype = "prototype/object/tree"
        resource_chance = 0.04
    end

    for y = 0, ctx.chunk_size - 1 do
        for x = 0, ctx.chunk_size - 1 do
            llmgc.chunks.add_tile(chunk, x, y, ground_tile, "ground")

            local wx = llmgc.procedural.world_x(ctx, x)
            local wy = llmgc.procedural.world_y(ctx, y)
            if ctx:noise2d(ctx.world_seed + 33, wx, wy, 0.14) > 0.78 then
                llmgc.chunks.add_entity(chunk, tree_prototype, x, y, {
                    collidable = { blocksMovement = true },
                    harvestable = { lootTableId = "loot/wood_basic" }
                })
            elseif ctx:random_float() < resource_chance then
                llmgc.chunks.add_entity(chunk, "prototype/object/resource_node", x, y, {
                    interactable = { interactionId = "interaction/harvest_resource_node" }
                })
            end
        end
    end

    chunk.metadata = {
        generationMode = "biome_chunk_generator",
        biomeId = biome_id
    }

    return chunk
end
