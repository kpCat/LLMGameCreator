-- Deterministic random helpers.
-- Runtime must provide ctx:random_int / ctx:random_float.

llmgc = llmgc or {}
llmgc.random = llmgc.random or {}

function llmgc.random.int(ctx, min, max)
    return ctx:random_int(min, max)
end

function llmgc.random.float(ctx)
    return ctx:random_float()
end

function llmgc.random.chance(ctx, probability)
    if probability <= 0 then return false end
    if probability >= 1 then return true end
    return ctx:random_float() < probability
end

function llmgc.random.pick(ctx, values)
    if values == nil or #values == 0 then
        return nil
    end
    return values[ctx:random_int(1, #values)]
end

function llmgc.random.weighted_pick(ctx, entries)
    local total = 0
    for _, entry in ipairs(entries or {}) do
        total = total + (entry.weight or 0)
    end

    if total <= 0 then return nil end

    local roll = ctx:random_float() * total
    local current = 0
    for _, entry in ipairs(entries) do
        current = current + (entry.weight or 0)
        if roll <= current then
            return entry.value or entry.id or entry
        end
    end

    return entries[#entries]
end
