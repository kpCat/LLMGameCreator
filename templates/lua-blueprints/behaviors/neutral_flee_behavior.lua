-- type: behavior.lua
-- purpose: Neutral creature that flees if player is too close.
-- contract: function decide_action(ctx) -> BehaviorActionDraft

function decide_action(ctx)
    local distance = ctx:distance_to("self", "player")

    if distance <= 3 then
        return {
            action = "move_away_from",
            target = "player"
        }
    end

    return { action = "wander_near", radius = 5 }
end
