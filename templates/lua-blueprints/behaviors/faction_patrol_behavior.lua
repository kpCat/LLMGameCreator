-- Blueprint: faction patrol behavior.
function decide_action(ctx)
    local enemy = ctx:nearest_hostile_entity()
    if enemy ~= nil and enemy.distance <= (ctx.alert_radius or 6) then
        return { action = "move_towards", target = enemy.entityId }
    end
    local patrol = ctx:component("patrol")
    if patrol ~= nil and patrol.points ~= nil and #patrol.points > 0 then
        local p = patrol.points[(ctx:state_number("patrolIndex") % #patrol.points) + 1]
        return { action = "move_to", x = p.x, y = p.y }
    end
    return { action = "wait" }
end
