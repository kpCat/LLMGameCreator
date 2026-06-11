-- type: event.lua
-- purpose: Random event while moving through wilderness.
-- contract: function on_player_moved(ctx) -> RuntimeEventDraft

function on_player_moved(ctx)
    if ctx:current_biome() == "biome/village" then
        return { effects = {} }
    end

    if ctx:random_float() > 0.04 then
        return { effects = {} }
    end

    local event_id = ctx:weighted_pick("table/random_travel_events")

    return {
        effects = {
            {
                type = "trigger_event",
                args = { eventId = event_id }
            }
        }
    }
end
