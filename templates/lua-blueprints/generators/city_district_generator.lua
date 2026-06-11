-- Blueprint: finite city district generator.
-- Type: generator.lua
function generate_map(ctx)
    local width = ctx.width or 96
    local height = ctx.height or 96
    local map = { width = width, height = height, tiles = {}, entities = {}, triggers = {} }

    for y = 0, height - 1 do
        for x = 0, width - 1 do
            local tile = (x % 16 == 0 or y % 16 == 0) and "tile/road" or "tile/city_ground"
            table.insert(map.tiles, { x = x, y = y, tileId = tile, layer = "ground" })
        end
    end

    for by = 8, height - 16, 16 do
        for bx = 8, width - 16, 16 do
            if ctx:random_float() < 0.75 then
                table.insert(map.entities, { prototypeId = "entity-prototype/building/simple_house", x = bx, y = by, components = { interactable = { interactionId = "interaction/enter_building" } } })
            end
        end
    end

    return map
end
