-- Blueprint: escort quest template.
function create_quest(ctx)
    return {
        id = ctx.quest_id or "quest/escort_npc",
        type = "escort",
        objectives = {
            { type = "meet_entity", entityId = ctx.escort_entity_id },
            { type = "reach_location", locationId = ctx.target_location_id },
            { type = "keep_entity_alive", entityId = ctx.escort_entity_id }
        },
        failureConditions = { { type = "entity_dead", entityId = ctx.escort_entity_id } },
        rewards = ctx.rewards or {}
    }
end
