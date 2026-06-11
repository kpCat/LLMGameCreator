-- Quest effect helpers.

llmgc = llmgc or {}
llmgc.quests = llmgc.quests or {}

function llmgc.quests.start(quest_id)
    return llmgc.effects.one("start_quest", { questId = quest_id })
end

function llmgc.quests.complete(quest_id)
    return llmgc.effects.one("complete_quest", { questId = quest_id })
end

function llmgc.quests.set_stage(quest_id, stage_id)
    return llmgc.effects.one("set_quest_stage", { questId = quest_id, stageId = stage_id })
end
