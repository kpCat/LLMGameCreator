-- Blueprint: room based building generator.
-- Type: generator.lua
function generate_map(ctx)
    local width = ctx.width or 48
    local height = ctx.height or 32
    local map = { width = width, height = height, tiles = {}, entities = {}, triggers = {} }

    for y = 0, height - 1 do
        for x = 0, width - 1 do
            local border = x == 0 or y == 0 or x == width - 1 or y == height - 1
            table.insert(map.tiles, { x = x, y = y, tileId = border and "tile/wall" or "tile/wood_floor", layer = "ground" })
        end
    end

    local sx, sy = math.floor(width / 2), math.floor(height / 2)
    for y = 1, height - 2 do table.insert(map.tiles, { x = sx, y = y, tileId = "tile/wall", layer = "ground" }) end
    for x = 1, width - 2 do table.insert(map.tiles, { x = x, y = sy, tileId = "tile/wall", layer = "ground" }) end
    table.insert(map.tiles, { x = sx, y = math.floor(height / 4), tileId = "tile/wood_floor", layer = "ground" })
    table.insert(map.tiles, { x = math.floor(width / 4), y = sy, tileId = "tile/wood_floor", layer = "ground" })
    return map
end
