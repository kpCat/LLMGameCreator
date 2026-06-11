-- Blueprint: melee enemy combat behavior.
function decide_combat_action(ctx)
    local target = ctx:best_enemy_target()
    if target == nil then return { action = "wait" } end
    if target.distance > 1 then return { action = "move_towards", target = target.entityId } end
    return { action = "use_ability", abilityId = "ability/basic_melee_attack", target = target.entityId }
end
