-- type: event.lua
-- purpose: Periodically update weather using deterministic random.
-- contract: function on_tick(ctx) -> RuntimeEventDraft

function on_tick(ctx)
    local hour = ctx:get_time_hour()

    -- Example: roll weather only every 6 hours.
    if hour % 6 ~= 0 then
        return { effects = {} }
    end

    local roll = ctx:random_float()
    local weather = "weather/clear"

    if roll < 0.12 then
        weather = "weather/rain"
    elseif roll < 0.18 then
        weather = "weather/storm"
    elseif roll > 0.88 then
        weather = "weather/fog"
    end

    return {
        effects = {
            {
                type = "set_weather",
                args = { weatherId = weather }
            }
        }
    }
end
