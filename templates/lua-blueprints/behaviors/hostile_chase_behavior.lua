-- type: behavior.lua
-- purpose: Hostile enemy that chases visible player or waits.
-- contract: function decide_action(ctx) -> BehaviorActionDraft

function decide_action(ctx)
    if ctx:can_see("self", "player") then
        return {
            action = "move_towards",
            target = "player"
        }
    end

    if ctx:random_float() < 0.25 then
        return { action = "wander_near", radius = 3 }
    end

    return { action = "wait" }
end
