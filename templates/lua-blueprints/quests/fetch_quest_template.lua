-- Blueprint: fetch quest template.
function create_quest(ctx)
    return {
        id = ctx.quest_id or "quest/fetch_item",
        type = "fetch",
        objectives = {
            { type = "obtain_item", itemId = ctx.item_id, count = ctx.count or 1 },
            { type = "return_to_entity", entityId = ctx.giver_id }
        },
        rewards = ctx.rewards or {}
    }
end
