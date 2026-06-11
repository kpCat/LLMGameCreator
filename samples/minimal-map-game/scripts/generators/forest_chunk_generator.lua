-- Example generator.lua file.
-- Intended entry point: generate_chunk(ctx)

require_lualib("core")
require_lualib("random")
require_lualib("noise")
require_lualib("chunks")

function generate_chunk(ctx)
    local width = ctx.chunk_width or 16
    local height = ctx.chunk_height or 16
    local chunk = llmgc.chunks.new(width, height)

    for y = 0, height - 1 do
        for x = 0, width - 1 do
            local world_x = ctx.origin_x + x
            local world_y = ctx.origin_y + y
            local h = llmgc.noise.value2d(ctx, world_x, world_y, 0.08)

            local tile_id = "tile/grass"
            if h < 0.20 then
                tile_id = "tile/water"
            elseif h > 0.75 then
                tile_id = "tile/forest"
            elseif h > 0.60 then
                tile_id = "tile/stone"
            end

            llmgc.chunks.add_tile(chunk, x, y, tile_id)
        end
    end

    if llmgc.random.chance(ctx, 0.15) then
        llmgc.chunks.add_entity(chunk, "npc/old_guard", llmgc.random.int(ctx, 1, width - 2), llmgc.random.int(ctx, 1, height - 2))
    end

    return chunk
end
