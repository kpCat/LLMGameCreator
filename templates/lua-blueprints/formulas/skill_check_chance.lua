-- type: formula.lua
-- purpose: Convert stat + difficulty to success chance.
-- contract: function calculate(ctx) -> number between 0 and 1

function calculate(ctx)
    local skill = ctx:stat("actor", ctx:param("skillId"))
    local difficulty = ctx:param("difficulty") or 10
    local chance = 0.5 + (skill - difficulty) * 0.05

    if chance < 0.05 then return 0.05 end
    if chance > 0.95 then return 0.95 end
    return chance
end
