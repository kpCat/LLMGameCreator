-- Blueprint: mage enemy combat behavior.
function decide_combat_action(ctx)
    local target = ctx:best_enemy_target()
    if target == nil then return { action = "wait" } end
    if ctx:resource("self", "resource/mana") >= 10 then
        return { action = "use_ability", abilityId = "ability/firebolt", target = target.entityId }
    end
    return { action = "use_ability", abilityId = "ability/basic_staff_attack", target = target.entityId }
end
