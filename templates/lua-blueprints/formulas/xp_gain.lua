-- type: formula.lua
-- purpose: XP reward scaled by difficulty and level difference.
-- contract: function calculate(ctx) -> number

function calculate(ctx)
    local base = ctx:param("baseXp") or 10
    local difficulty = ctx:param("difficulty") or 1
    local actor_level = ctx:progression_level("player", "progression/character_level")
    local target_level = ctx:param("targetLevel") or actor_level

    local level_delta = target_level - actor_level
    local multiplier = 1.0 + level_delta * 0.12

    if multiplier < 0.25 then multiplier = 0.25 end
    if multiplier > 2.0 then multiplier = 2.0 end

    return math.floor(base * difficulty * multiplier)
end
