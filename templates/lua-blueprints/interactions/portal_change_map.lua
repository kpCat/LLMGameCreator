-- type: interaction.lua
-- purpose: Transfer player to another map/spawn.
-- contract: function on_interact(ctx) -> InteractionResultDraft

function on_interact(ctx)
    return llmgc.interactions.effects({
        {
            type = "change_map",
            args = {
                mapId = "map/forest",
                spawnId = "spawn/south_gate"
            }
        }
    })
end
