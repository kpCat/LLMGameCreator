-- Blueprint: finite cave network generator.
-- Type: generator.lua
function generate_map(ctx)
    local width = ctx.width or 80
    local height = ctx.height or 80
    local map = { width = width, height = height, tiles = {}, entities = {}, triggers = {} }

    for y = 0, height - 1 do
        for x = 0, width - 1 do
            table.insert(map.tiles, { x = x, y = y, tileId = "tile/cave_wall", layer = "ground" })
        end
    end

    local rooms = {}
    for i = 1, ctx.room_count or 14 do
        local rw = ctx:random_int(5, 12)
        local rh = ctx:random_int(5, 10)
        local rx = ctx:random_int(2, width - rw - 3)
        local ry = ctx:random_int(2, height - rh - 3)
        table.insert(rooms, { x = rx, y = ry, w = rw, h = rh })
        for y = ry, ry + rh do
            for x = rx, rx + rw do
                table.insert(map.tiles, { x = x, y = y, tileId = "tile/cave_floor", layer = "ground" })
            end
        end
    end

    for i = 2, #rooms do
        local a, b = rooms[i - 1], rooms[i]
        local ax, ay = math.floor(a.x + a.w / 2), math.floor(a.y + a.h / 2)
        local bx, by = math.floor(b.x + b.w / 2), math.floor(b.y + b.h / 2)
        for x = math.min(ax, bx), math.max(ax, bx) do table.insert(map.tiles, { x = x, y = ay, tileId = "tile/cave_floor", layer = "ground" }) end
        for y = math.min(ay, by), math.max(ay, by) do table.insert(map.tiles, { x = bx, y = y, tileId = "tile/cave_floor", layer = "ground" }) end
    end

    return map
end
