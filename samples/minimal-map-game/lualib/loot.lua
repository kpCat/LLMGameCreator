-- Loot helpers.

llmgc = llmgc or {}
llmgc.loot = llmgc.loot or {}

function llmgc.loot.roll_table(ctx, entries)
    local picked = llmgc.random.weighted_pick(ctx, entries)
    if picked == nil then
        return { effects = {} }
    end

    return llmgc.effects.add_item("player", picked.itemId or picked, picked.count or 1)
end
