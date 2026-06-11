-- Interaction helpers.

llmgc = llmgc or {}
llmgc.interactions = llmgc.interactions or {}

function llmgc.interactions.open_dialogue(dialogue_id)
    return llmgc.effects.open_dialogue(dialogue_id)
end

function llmgc.interactions.message(text)
    return {
        events = {
            { type = "message", text = text }
        }
    }
end

function llmgc.interactions.effects(effects)
    return llmgc.effects.many(effects)
end
