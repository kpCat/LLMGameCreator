-- type: generator.lua
-- mode: road_overlay
-- purpose: Add deterministic road/path overlay through chunks.
-- contract: function apply_road_overlay(ctx, chunk) -> GeneratedChunkDraft

function apply_road_overlay(ctx, chunk)
    local mid = math.floor(ctx.chunk_size / 2)

    -- East-west road every chunk on the main latitude.
    if ctx.chunk_y == 0 then
        llmgc.procedural.rect(chunk, 0, mid - 1, ctx.chunk_size - 1, mid + 1, "tile/road", "ground")
    end

    -- North-south road every 5 chunks.
    if ctx.chunk_x % 5 == 0 then
        llmgc.procedural.rect(chunk, mid - 1, 0, mid + 1, ctx.chunk_size - 1, "tile/road", "ground")
    end

    return chunk
end
