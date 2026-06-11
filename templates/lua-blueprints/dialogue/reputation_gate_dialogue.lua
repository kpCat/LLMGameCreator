-- Blueprint: reputation gate dialogue.
function on_interact(ctx)
    local reputation = ctx:stat("player", ctx:param_string("reputationStatId"))
    if reputation >= (ctx:param_number("required") or 50) then
        return { effects = { llmgc.effects.open_dialogue(ctx:param_string("successDialogueId")) } }
    end
    return { effects = { llmgc.effects.open_dialogue(ctx:param_string("failureDialogueId")) } }
end
