-- Example prototype.lua file.
-- Allowed API: data:extend only + lualib constructors.

require_lualib("core")
require_lualib("tiles")

data:extend({
    llmgc.tiles.prototype("tile/grass", "Трава", true, "asset/tile/grass", { movementCost = 1.0 }),
    llmgc.tiles.prototype("tile/stone", "Камень", true, "asset/tile/stone", { movementCost = 1.2 }),
    llmgc.tiles.prototype("tile/water", "Вода", false, "asset/tile/water", { movementCost = 999.0 }),
    llmgc.tiles.prototype("tile/forest", "Густой лес", false, "asset/tile/forest", { blocksSight = true })
})
