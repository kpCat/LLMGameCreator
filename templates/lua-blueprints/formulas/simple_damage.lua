-- type: formula.lua
-- purpose: Basic weapon damage formula.
-- contract: function calculate(ctx) -> number

function calculate(ctx)
    local base = ctx:weapon_damage("actor")
    local strength = ctx:stat("actor", "stat/strength")
    local roll = ctx:random_int(1, 4)

    return math.floor(base + strength * 1.2 + roll)
end
