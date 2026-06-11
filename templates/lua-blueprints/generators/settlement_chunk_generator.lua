-- type: generator.lua
-- mode: settlement_chunk
-- purpose: Generate simple settlement blocks: paths, houses, NPC markers.
-- contract: function generate_chunk(ctx) -> GeneratedChunkDraft

function generate_chunk(ctx)
    local chunk = llmgc.chunks.new(ctx.chunk_size, ctx.chunk_size)
    llmgc.procedural.fill(chunk, "tile/dirt", "ground")

    local mid = math.floor(ctx.chunk_size / 2)
    llmgc.procedural.rect(chunk, 0, mid - 1, ctx.chunk_size - 1, mid + 1, "tile/road", "ground")
    llmgc.procedural.rect(chunk, mid - 1, 0, mid + 1, ctx.chunk_size - 1, "tile/road", "ground")

    local house_positions = {
        { x = 3, y = 3 },
        { x = ctx.chunk_size - 5, y = 3 },
        { x = 3, y = ctx.chunk_size - 5 },
        { x = ctx.chunk_size - 5, y = ctx.chunk_size - 5 }
    }

    for _, p in ipairs(house_positions) do
        if ctx:random_float() < 0.65 then
            llmgc.chunks.add_entity(chunk, "prototype/building/simple_house", p.x, p.y, {
                collidable = { blocksMovement = true },
                interactable = { interactionId = "interaction/enter_simple_house" }
            })
        end
    end

    if ctx:random_float() < 0.5 then
        llmgc.chunks.add_entity(chunk, "prototype/npc/villager", mid + 2, mid, {
            interactable = { interactionId = "interaction/talk_villager" },
            behavior = { scriptId = "script/behavior/npc_wander" }
        })
    end

    chunk.metadata = {
        generationMode = "settlement_chunk_generator"
    }

    return chunk
end
