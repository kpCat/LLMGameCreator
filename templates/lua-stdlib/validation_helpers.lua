-- Validation helpers for generated Lua.

llmgc = llmgc or {}
llmgc.validation = llmgc.validation or {}

function llmgc.validation.require_id(value, prefix)
    if type(value) ~= "string" or value == "" then
        error("id must be a non-empty string")
    end

    if prefix ~= nil and string.sub(value, 1, #prefix) ~= prefix then
        error("id must start with " .. prefix)
    end

    return value
end

function llmgc.validation.require_asset(asset_id)
    return llmgc.validation.require_id(asset_id, "asset/")
end

function llmgc.validation.require_effect(effect)
    if type(effect) ~= "table" then
        error("effect must be a table")
    end
    if type(effect.type) ~= "string" or effect.type == "" then
        error("effect.type is required")
    end
    return effect
end
