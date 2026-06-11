-- type: prototype.lua
-- purpose: Example encounter prototype with humanoid enemies.

data:extend({
    {
        type = "encounter",
        id = "encounter/bandit_patrol",
        name = "Бандитский патруль",
        participants = {
            enemies = {
                { prototypeId = "prototype/enemy/bandit", min = 1, max = 3 },
                { prototypeId = "prototype/enemy/bandit_archer", min = 0, max = 1 }
            }
        },
        tags = { "human", "hostile", "road_event" },
        startConditions = {
            { type = "not_flag", args = { flagId = "flag/bandit_patrol_defeated" } }
        },
        winEffects = {
            { type = "set_flag", args = { flagId = "flag/bandit_patrol_defeated", value = true } },
            { type = "roll_loot", args = { lootTableId = "loot/bandit_basic" } }
        }
    }
})
