-- Blueprint: relationship based dialogue.
function on_interact(ctx)
    local rel = ctx:relationship("player", ctx:target_entity_id())
    if rel >= 70 then return { effects = { llmgc.effects.open_dialogue("dialogue/friendly") } } end
    if rel <= 20 then return { effects = { llmgc.effects.open_dialogue("dialogue/hostile") } } end
    return { effects = { llmgc.effects.open_dialogue("dialogue/neutral") } }
end
