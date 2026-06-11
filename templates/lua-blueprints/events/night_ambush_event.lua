-- type: event.lua
-- purpose: Night ambush chance after movement/wait.
-- contract: function on_world_step(ctx) -> RuntimeEventDraft

function on_world_step(ctx)
    if not ctx:is_night() then
        return { effects = {} }
    end

    if ctx:current_tile_has_tag("safe_zone") then
        return { effects = {} }
    end

    if ctx:random_float() < 0.03 then
        return {
            effects = {
                {
                    type = "start_encounter",
                    args = { encounterId = "encounter/night_ambush" }
                }
            }
        }
    end

    return { effects = {} }
end
