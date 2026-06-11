-- Example prototype.lua file for entity prototypes.

require_lualib("core")
require_lualib("entities")

data:extend({
    llmgc.entities.npc(
        "npc/old_guard",
        "Старый стражник",
        "asset/npc/old_guard/spritesheet",
        "dialogue/old_guard_intro",
        {
            tags = { "npc", "guard", "village" },
            portraitSetAssetId = "asset/npc/old_guard/portrait_set"
        }
    ),
    llmgc.entities.object(
        "object/locked_chest",
        "Запертый сундук",
        "asset/object/locked_chest",
        true,
        "interaction/open_locked_chest",
        {
            tags = { "container", "locked" }
        }
    )
})
