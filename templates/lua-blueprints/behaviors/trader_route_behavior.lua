-- Blueprint: trader route behavior.
function decide_action(ctx)
    local route = ctx:component("tradeRoute")
    if route == nil or route.stops == nil or #route.stops == 0 then return { action = "wait" } end
    local stop = route.stops[(ctx:state_number("routeStopIndex") % #route.stops) + 1]
    if ctx:is_at(stop.x, stop.y) then return { action = "run_interaction", interactionId = "interaction/trader_arrived_at_stop" } end
    return { action = "move_to", x = stop.x, y = stop.y }
end
