-- type: formula.lua
-- purpose: Resource regeneration per tick/rest.
-- contract: function calculate(ctx) -> number

function calculate(ctx)
    local base = ctx:param("base") or 1
    local stat_bonus_id = ctx:param("statBonusId") or "stat/endurance"
    local stat_bonus = ctx:stat("actor", stat_bonus_id)
    local multiplier = ctx:param("multiplier") or 0.1

    return math.floor(base + stat_bonus * multiplier)
end
