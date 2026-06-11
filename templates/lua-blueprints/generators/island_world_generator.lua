-- Blueprint: finite island overworld generator.
-- Type: generator.lua
function generate_map(ctx)
    local width = ctx.width or 96
    local height = ctx.height or 96
    local center_x = width / 2
    local center_y = height / 2
    local max_dist = math.min(width, height) / 2
    local map = { width = width, height = height, tiles = {}, entities = {}, triggers = {} }

    for y = 0, height - 1 do
        for x = 0, width - 1 do
            local dx = (x - center_x) / max_dist
            local dy = (y - center_y) / max_dist
            local dist = math.sqrt(dx * dx + dy * dy)
            local noise = ctx:noise2d("island-height", x * 0.06, y * 0.06)
            local h = 1.0 - dist + noise * 0.22
            local tile = "tile/water"
            if h > 0.06 then tile = "tile/sand" end
            if h > 0.16 then tile = "tile/grass" end
            if h > 0.42 then tile = "tile/forest" end
            if h > 0.68 then tile = "tile/mountain" end
            table.insert(map.tiles, { x = x, y = y, tileId = tile, layer = "ground" })
        end
    end

    table.insert(map.entities, { prototypeId = "entity-prototype/player_start", x = math.floor(center_x), y = math.floor(center_y), components = {} })
    return map
end
