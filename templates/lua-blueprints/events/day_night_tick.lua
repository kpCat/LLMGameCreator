-- Blueprint: day/night tick event.
function on_time_tick(ctx)
    local hour = ctx:hour()
    local isNight = hour >= 22 or hour < 6
    if isNight and not ctx:flag("flag/world/is_night") then
        return { effects = { llmgc.effects.set_flag("flag/world/is_night", true) } }
    end
    if (not isNight) and ctx:flag("flag/world/is_night") then
        return { effects = { llmgc.effects.set_flag("flag/world/is_night", false) } }
    end
    return { effects = {} }
end
