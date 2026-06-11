-- Example interaction.lua file.
-- Intended entry point: on_interact(ctx)

require_lualib("core")
require_lualib("effects")
require_lualib("interactions")

function on_interact(ctx)
    if ctx:has_flag("flag/chest_opened") then
        return llmgc.interactions.message("Сундук уже пуст.")
    end

    if not ctx:has_item("player", "item/rusty_key") then
        return llmgc.interactions.message("Сундук заперт. Нужен ключ.")
    end

    return llmgc.effects.many({
        { type = "set_flag", args = { flagId = "flag/chest_opened", value = true } },
        { type = "add_item", args = { entityId = "player", itemId = "item/old_coin", count = 3 } },
        { type = "play_sfx", args = { assetId = "asset/sfx/chest_open" } }
    })
end
