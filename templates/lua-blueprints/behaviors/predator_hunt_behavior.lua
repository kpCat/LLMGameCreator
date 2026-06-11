-- Blueprint: predator hunt behavior.
function decide_action(ctx)
    local prey = ctx:nearest_entity_with_tag("prey")
    if prey ~= nil and prey.distance <= 8 then return { action = "move_towards", target = prey.entityId } end
    local player = ctx:player()
    if player ~= nil and player.distance <= 5 and ctx:random_float() < 0.35 then
        return { action = "start_encounter", encounterId = "encounter/predator_attack", target = "player" }
    end
    return { action = "move_random", radius = 5 }
end
