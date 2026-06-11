-- Blueprint: biome picker.
function pick_biome(ctx, world_x, world_y)
    local t = ctx:noise2d("temperature", world_x * 0.005, world_y * 0.005)
    local h = ctx:noise2d("humidity", world_x * 0.005, world_y * 0.005)
    local e = ctx:noise2d("elevation", world_x * 0.004, world_y * 0.004)
    if e > 0.72 then return "biome/mountains" end
    if h > 0.65 and t > 0.45 then return "biome/swamp" end
    if h > 0.55 then return "biome/forest" end
    if t < 0.25 then return "biome/tundra" end
    if t > 0.75 and h < 0.35 then return "biome/desert" end
    return "biome/plains"
end
