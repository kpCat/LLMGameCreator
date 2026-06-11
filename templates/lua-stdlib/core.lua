-- LLMGameCreator Lua standard library: core helpers.
-- This file is intentionally small and sandbox-friendly.

llmgc = llmgc or {}
llmgc.version = "0.1.1"

function llmgc.assert_string(value, name)
    if type(value) ~= "string" or value == "" then
        error((name or "value") .. " must be a non-empty string")
    end
    return value
end

function llmgc.assert_table(value, name)
    if type(value) ~= "table" then
        error((name or "value") .. " must be a table")
    end
    return value
end

function llmgc.clone_table(source)
    if type(source) ~= "table" then
        return source
    end

    local result = {}
    for key, value in pairs(source) do
        result[key] = llmgc.clone_table(value)
    end
    return result
end

function llmgc.merge_tables(base, overlay)
    local result = llmgc.clone_table(base or {})
    for key, value in pairs(overlay or {}) do
        result[key] = value
    end
    return result
end

function llmgc.id(prefix, name)
    llmgc.assert_string(prefix, "prefix")
    llmgc.assert_string(name, "name")
    return prefix .. "/" .. name
end
