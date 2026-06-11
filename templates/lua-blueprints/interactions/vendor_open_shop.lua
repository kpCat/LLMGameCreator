-- type: interaction.lua
-- purpose: Open vendor UI by vendor catalog id.
-- contract: function on_interact(ctx) -> InteractionResultDraft

function on_interact(ctx)
    return {
        kind = "effects",
        effects = {
            {
                type = "open_shop",
                args = {
                    vendorId = ctx:self_id(),
                    catalogId = "shop/general_goods"
                }
            }
        }
    }
end
