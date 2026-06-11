-- type: prototype.lua
-- purpose: Example biome-specific loot tables.

data:extend({
    {
        type = "loot_table",
        id = "loot/forest_gathering",
        rolls = 1,
        entries = {
            { itemId = "item/raw_herb", min = 1, max = 3, weight = 45 },
            { itemId = "item/mushroom", min = 1, max = 2, weight = 30 },
            { itemId = "item/branch", min = 1, max = 4, weight = 25 }
        }
    },
    {
        type = "loot_table",
        id = "loot/swamp_gathering",
        rolls = 1,
        entries = {
            { itemId = "item/swamp_flower", min = 1, max = 2, weight = 35 },
            { itemId = "item/toxic_mushroom", min = 1, max = 2, weight = 25 },
            { itemId = "item/mud_clay", min = 1, max = 3, weight = 40 }
        }
    }
})
