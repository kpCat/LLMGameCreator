-- type: interaction.lua
-- purpose: Open locked chest with key, otherwise show message.
-- contract: function on_interact(ctx) -> InteractionResultDraft

function on_interact(ctx)
    if not ctx:has_item("player", "item/rusty_key", 1) then
        return llmgc.interactions.message("Сундук заперт. Нужен ключ.")
    end

    return llmgc.interactions.effects({
        llmgc.effects.remove_item("player", "item/rusty_key", 1),
        llmgc.effects.add_item("player", "item/old_coin", 10),
        llmgc.effects.set_flag("flag/opened_locked_chest", true),
        llmgc.effects.play_sound("asset/sfx/chest_open")
    })
end
