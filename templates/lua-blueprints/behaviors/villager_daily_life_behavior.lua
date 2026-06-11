-- Blueprint: villager daily life behavior.
function decide_action(ctx)
    local hour = ctx:hour()
    if hour >= 6 and hour < 9 then return { action = "move_to_tag", tag = "workplace" } end
    if hour >= 9 and hour < 18 then return { action = "perform_activity", activity = "work" } end
    if hour >= 18 and hour < 22 then return { action = "move_to_tag", tag = "tavern_or_home" } end
    return { action = "move_to_tag", tag = "home" }
end
