-- Entity prototype helpers.

llmgc = llmgc or {}
llmgc.entities = llmgc.entities or {}

function llmgc.entities.npc(id, name, asset_id, dialogue_id, extra)
    local proto = {
        type = "npc",
        id = id,
        name = name,
        components = {
            renderable = { assetId = asset_id },
            interactable = { interactionId = "interaction/talk/" .. id },
            dialogue = { dialogueId = dialogue_id }
        }
    }
    return llmgc.merge_tables(proto, extra or {})
end

function llmgc.entities.object(id, name, asset_id, blocks_movement, interaction_id, extra)
    local proto = {
        type = "object",
        id = id,
        name = name,
        components = {
            renderable = { assetId = asset_id },
            collidable = { blocksMovement = blocks_movement },
            interactable = { interactionId = interaction_id }
        }
    }
    return llmgc.merge_tables(proto, extra or {})
end
