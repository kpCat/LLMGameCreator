-- Combat helpers. This is only a contract baseline, not a full combat system.

llmgc = llmgc or {}
llmgc.combat = llmgc.combat or {}

function llmgc.combat.deal_damage(target_id, damage_type, amount)
    return {
        type = "deal_damage",
        args = {
            targetId = target_id,
            damageType = damage_type,
            amount = amount
        }
    }
end

function llmgc.combat.start_encounter(encounter_id)
    return llmgc.effects.one("start_encounter", { encounterId = encounter_id })
end
