-- type: generator.lua
-- mode: finite_dungeon
-- purpose: Simple bounded room/corridor generator for small dungeons.
-- contract: function generate_chunk(ctx) -> GeneratedChunkDraft

function generate_chunk(ctx)
    local chunk = llmgc.chunks.new(ctx.chunk_size, ctx.chunk_size)
    llmgc.procedural.fill(chunk, "tile/stone_wall", "ground")

    -- Carve a simple room in each chunk.
    llmgc.procedural.rect(chunk, 2, 2, ctx.chunk_size - 3, ctx.chunk_size - 3, "tile/stone_floor", "ground")

    -- Corridors connecting neighbor chunks.
    local mid = math.floor(ctx.chunk_size / 2)
    llmgc.procedural.rect(chunk, mid - 1, 0, mid + 1, ctx.chunk_size - 1, "tile/stone_floor", "ground")
    llmgc.procedural.rect(chunk, 0, mid - 1, ctx.chunk_size - 1, mid + 1, "tile/stone_floor", "ground")

    -- Sparse objects.
    if ctx:random_float() < 0.25 then
        llmgc.chunks.add_entity(chunk, "prototype/object/dungeon_chest", ctx:random_int(3, ctx.chunk_size - 4), ctx:random_int(3, ctx.chunk_size - 4), {
            interactable = { interactionId = "interaction/open_dungeon_chest" }
        })
    end

    if ctx:random_float() < 0.18 then
        llmgc.chunks.add_entity(chunk, "prototype/enemy/skeleton", ctx:random_int(3, ctx.chunk_size - 4), ctx:random_int(3, ctx.chunk_size - 4), {
            combatant = { encounterId = "encounter/skeleton_basic" },
            behavior = { scriptId = "script/behavior/hostile_chase" }
        })
    end

    chunk.metadata = {
        generationMode = "finite_dungeon_map"
    }

    return chunk
end
