-- Blueprint: river overlay generator.
function generate_overlay(ctx)
    local width = ctx.width or ctx.chunk_size or 32
    local height = ctx.height or ctx.chunk_size or 32
    local overlay = { tiles = {} }
    local base_x = ctx.start_x or math.floor(width / 3)
    for y = 0, height - 1 do
        local bend = math.floor(ctx:noise2d("river-bend", y * 0.09, 0) * 5)
        local river_x = base_x + bend
        for dx = -1, 1 do
            local x = river_x + dx
            if x >= 0 and x < width then table.insert(overlay.tiles, { x = x, y = y, tileId = "tile/water", layer = "ground" }) end
        end
    end
    return overlay
end
