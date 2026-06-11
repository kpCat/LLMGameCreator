-- Blueprint: bounty quest template.
function create_quest(ctx)
    return {
        id = ctx.quest_id or "quest/bounty",
        type = "bounty",
        objectives = {
            { type = "defeat_entity_or_group", targetId = ctx.target_id },
            { type = "return_to_entity", entityId = ctx.giver_id }
        },
        rewards = ctx.rewards or {}
    }
end
