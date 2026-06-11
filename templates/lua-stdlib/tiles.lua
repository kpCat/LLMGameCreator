-- Tile prototype helpers.

llmgc = llmgc or {}
llmgc.tiles = llmgc.tiles or {}

function llmgc.tiles.prototype(id, name, walkable, asset_id, extra)
    local proto = {
        type = "tile",
        id = id,
        name = name,
        walkable = walkable,
        assetId = asset_id
    }
    return llmgc.merge_tables(proto, extra or {})
end
