-- type: behavior.lua
-- purpose: Simple schedule-based NPC behavior.
-- contract: function decide_action(ctx) -> BehaviorActionDraft

function decide_action(ctx)
    local hour = ctx:get_time_hour()

    if hour >= 22 or hour < 6 then
        return { action = "go_to_anchor", anchorId = "anchor/home" }
    end

    if hour >= 8 and hour < 18 then
        return { action = "go_to_anchor", anchorId = "anchor/work" }
    end

    return { action = "wander_near", radius = 4 }
end
