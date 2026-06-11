-- Blueprint: crafting recipe check.
function can_craft(ctx)
    local recipe = ctx:recipe()
    for _, ingredient in ipairs(recipe.ingredients) do
        if not ctx:has_item("player", ingredient.itemId, ingredient.count) then return false end
    end
    return true
end

function craft(ctx)
    if not can_craft(ctx) then return { effects = { llmgc.effects.log("Недостаточно материалов.") } } end
    local effects = {}
    for _, ingredient in ipairs(ctx:recipe().ingredients) do
        table.insert(effects, llmgc.effects.remove_item("player", ingredient.itemId, ingredient.count))
    end
    table.insert(effects, llmgc.effects.add_item("player", ctx:recipe().resultItemId, ctx:recipe().resultCount or 1))
    return { effects = effects }
end
