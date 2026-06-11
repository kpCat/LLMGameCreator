-- Example formula.lua file.
-- Intended entry point: calculate(ctx)

require_lualib("core")
require_lualib("random")

function calculate(ctx)
    local strength = ctx:stat("actor", "stat/strength") or 1
    local weapon = ctx:stat("actor", "stat/weapon_damage") or 1
    return weapon + strength * 1.25 + llmgc.random.int(ctx, 1, 4)
end
