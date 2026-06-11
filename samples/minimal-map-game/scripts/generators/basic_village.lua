-- Тип: generator.lua
-- Контракт v0.1: функция generate_chunk(ctx) возвращает chunkDraft.
-- Реальный Lua engine пока не подключён; файл нужен как эталон будущего формата.

function generate_chunk(ctx)
    return {
        tiles = {},
        entities = {},
        triggers = {}
    }
end
