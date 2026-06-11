-- Noise helpers. Runtime should provide deterministic noise functions.

llmgc = llmgc or {}
llmgc.noise = llmgc.noise or {}

function llmgc.noise.value2d(ctx, x, y, scale)
    scale = scale or 1.0
    return ctx:noise2d(x * scale, y * scale)
end

function llmgc.noise.band(value, bands)
    for _, band in ipairs(bands or {}) do
        if value >= band.min and value < band.max then
            return band.value
        end
    end
    return nil
end
