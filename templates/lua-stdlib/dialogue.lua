-- Dialogue helpers.

llmgc = llmgc or {}
llmgc.dialogue = llmgc.dialogue or {}

function llmgc.dialogue.line(speaker_id, expression, text)
    return {
        speakerId = speaker_id,
        expression = expression or "neutral",
        text = text
    }
end

function llmgc.dialogue.option(id, text, target_node_id, effects)
    return {
        id = id,
        text = text,
        targetNodeId = target_node_id,
        effects = effects or {}
    }
end
