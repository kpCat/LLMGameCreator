-- type: interaction.lua
-- purpose: Pick dialogue by flags/quest state.
-- contract: function on_interact(ctx) -> InteractionResultDraft

function on_interact(ctx)
    if ctx:get_flag("flag/helped_old_guard") then
        return llmgc.interactions.open_dialogue("dialogue/old_guard_grateful")
    end

    if ctx:quest_state("quest/find_gate_key") == "active" then
        return llmgc.interactions.open_dialogue("dialogue/old_guard_about_key")
    end

    return llmgc.interactions.open_dialogue("dialogue/old_guard_intro")
end
