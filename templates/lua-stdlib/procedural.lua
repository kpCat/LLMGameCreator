-- Procedural generation helpers for LLMGameCreator Lua scripts.
-- This file is intentionally small and deterministic-friendly.

llmgc = llmgc or {}
llmgc.procedural = llmgc.procedural or {}

function llmgc.procedural.chunk_key(chunk_x, chunk_y)
    return tostring(chunk_x) .. ":" .. tostring(chunk_y)
end

function llmgc.procedural.world_x(ctx, local_x)
    return ctx.chunk_x * ctx.chunk_size + local_x
end

function llmgc.procedural.world_y(ctx, local_y)
    return ctx.chunk_y * ctx.chunk_size + local_y
end

function llmgc.procedural.pick_biome_by_noise(value, rules, fallback_biome_id)
    for _, rule in ipairs(rules) do
        if value >= rule.min and value < rule.max then
            return rule.biomeId
        end
    end

    return fallback_biome_id
end

function llmgc.procedural.fill(chunk, tile_id, layer)
    for y = 0, chunk.height - 1 do
        for x = 0, chunk.width - 1 do
            llmgc.chunks.add_tile(chunk, x, y, tile_id, layer)
        end
    end

    return chunk
end

function llmgc.procedural.rect(chunk, x1, y1, x2, y2, tile_id, layer)
    for y = y1, y2 do
        for x = x1, x2 do
            llmgc.chunks.add_tile(chunk, x, y, tile_id, layer)
        end
    end

    return chunk
end

function llmgc.procedural.border(chunk, tile_id, layer)
    local max_x = chunk.width - 1
    local max_y = chunk.height - 1

    for x = 0, max_x do
        llmgc.chunks.add_tile(chunk, x, 0, tile_id, layer)
        llmgc.chunks.add_tile(chunk, x, max_y, tile_id, layer)
    end

    for y = 1, max_y - 1 do
        llmgc.chunks.add_tile(chunk, 0, y, tile_id, layer)
        llmgc.chunks.add_tile(chunk, max_x, y, tile_id, layer)
    end

    return chunk
end

function llmgc.procedural.maybe_spawn(ctx, chunk, chance, prototype_id, x, y, components)
    if ctx:random_float() <= chance then
        llmgc.chunks.add_entity(chunk, prototype_id, x, y, components or {})
        return true
    end

    return false
end
