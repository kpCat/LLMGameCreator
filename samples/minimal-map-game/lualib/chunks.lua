-- Chunk draft helpers.

llmgc = llmgc or {}
llmgc.chunks = llmgc.chunks or {}

function llmgc.chunks.new(width, height)
    return {
        width = width,
        height = height,
        tiles = {},
        entities = {},
        triggers = {},
        events = {}
    }
end

function llmgc.chunks.add_tile(chunk, x, y, tile_id, layer)
    table.insert(chunk.tiles, {
        x = x,
        y = y,
        tileId = tile_id,
        layer = layer or "ground"
    })
    return chunk
end

function llmgc.chunks.add_entity(chunk, prototype_id, x, y, components)
    table.insert(chunk.entities, {
        prototypeId = prototype_id,
        x = x,
        y = y,
        components = components or {}
    })
    return chunk
end

function llmgc.chunks.add_trigger(chunk, x, y, interaction_id)
    table.insert(chunk.triggers, {
        x = x,
        y = y,
        interactionId = interaction_id
    })
    return chunk
end
