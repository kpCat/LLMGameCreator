-- type: interaction.lua
-- purpose: Harvest resource node once.
-- contract: function on_interact(ctx) -> InteractionResultDraft

function on_interact(ctx)
    local node_id = ctx:self_id()
    local flag_id = "flag/harvested/" .. node_id

    if ctx:get_flag(flag_id) then
        return llmgc.interactions.message("Здесь уже ничего не осталось.")
    end

    return llmgc.interactions.effects({
        llmgc.effects.add_item("player", "item/raw_herb", ctx:random_int(1, 3)),
        llmgc.effects.set_flag(flag_id, true),
        llmgc.effects.play_sound("asset/sfx/harvest_herb")
    })
end
