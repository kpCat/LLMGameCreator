-- Standard effect constructors.

llmgc = llmgc or {}
llmgc.effects = llmgc.effects or {}

function llmgc.effects.one(effect_type, args)
    return { effects = { { type = effect_type, args = args or {} } } }
end

function llmgc.effects.many(effects)
    return { effects = effects or {} }
end

function llmgc.effects.open_dialogue(dialogue_id)
    return llmgc.effects.one("open_dialogue", { dialogueId = dialogue_id })
end

function llmgc.effects.set_flag(flag_id, value)
    return llmgc.effects.one("set_flag", { flagId = flag_id, value = value ~= false })
end

function llmgc.effects.change_resource(entity_id, resource_id, amount)
    return llmgc.effects.one("change_resource", {
        entityId = entity_id,
        resourceId = resource_id,
        amount = amount
    })
end

function llmgc.effects.add_item(entity_id, item_id, count)
    return llmgc.effects.one("add_item", {
        entityId = entity_id,
        itemId = item_id,
        count = count or 1
    })
end

function llmgc.effects.play_sfx(asset_id)
    return llmgc.effects.one("play_sfx", { assetId = asset_id })
end

function llmgc.effects.play_music(asset_id)
    return llmgc.effects.one("play_music", { assetId = asset_id })
end
