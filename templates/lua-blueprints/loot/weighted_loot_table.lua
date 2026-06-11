-- type: prototype.lua
-- purpose: Example weighted loot table declaration.

data:extend({
    {
        type = "loot_table",
        id = "loot/basic_chest",
        rolls = 2,
        entries = {
            { itemId = "item/old_coin", min = 3, max = 12, weight = 60 },
            { itemId = "item/healing_potion", min = 1, max = 1, weight = 20 },
            { itemId = "item/rusty_dagger", min = 1, max = 1, weight = 8 },
            { itemId = "item/blank_scroll", min = 1, max = 1, weight = 12 }
        }
    }
})
