-- type: prototype.lua
-- purpose: Example encounter prototype for multiple enemies.

data:extend({
    {
        type = "encounter",
        id = "encounter/wolf_pack",
        name = "Стая волков",
        participants = {
            enemies = {
                { prototypeId = "prototype/enemy/wolf", min = 2, max = 4 }
            }
        },
        winEffects = {
            { type = "add_progress", args = { progressionId = "progression/survival", amount = 2 } }
        },
        loseEffects = {
            { type = "change_resource", args = { entityId = "player", resourceId = "resource/health", amount = -10 } }
        }
    }
})
