-- type: behavior.lua
-- purpose: Neutral NPC wandering behavior.
-- contract: function decide_action(ctx) -> BehaviorActionDraft

function decide_action(ctx)
    if ctx:random_float() < 0.65 then
        return { action = "wait" }
    end

    local directions = { "north", "south", "west", "east" }
    return {
        action = "move",
        direction = directions[ctx:random_int(1, #directions)]
    }
end
