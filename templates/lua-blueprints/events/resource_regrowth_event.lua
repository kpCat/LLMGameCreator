-- Blueprint: resource regrowth tick.
function on_world_tick(ctx)
    local effects = {}
    for _, node in ipairs(ctx:entities_with_component("resourceNode")) do
        if node.depleted and ctx:random_float() < (node.regrowthChance or 0.03) then
            table.insert(effects, { type = "restore_resource_node", args = { entityId = node.entityId } })
        end
    end
    return { effects = effects }
end
