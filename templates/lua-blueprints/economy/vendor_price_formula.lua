-- Blueprint: vendor price formula.
function calculate(ctx)
    local base = ctx:param_number("basePrice")
    local reputation = ctx:stat("player", "stat/reputation/vendor")
    local scarcity = ctx:param_number("scarcity") or 1.0
    local discount = math.min(0.3, reputation * 0.01)
    return math.max(1, math.floor(base * scarcity * (1.0 - discount)))
end
