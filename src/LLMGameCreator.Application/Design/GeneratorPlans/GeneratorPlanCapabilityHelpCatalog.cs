namespace LLMGameCreator.Application.Design.GeneratorPlans;

public static class GeneratorPlanCapabilityHelpCatalog
{
    private static readonly IReadOnlyDictionary<string, GeneratorPlanCapabilityHelpMetadata> Entries = BuildEntries();
    private static readonly IReadOnlyList<GeneratorPlanCapabilityCompositionSeed> CompositionSeeds = BuildCompositionSeeds();

    public static GeneratorPlanCapabilityHelpMetadata Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return GeneratorPlanCapabilityHelpMetadata.Fallback(string.Empty);
        }

        return Entries.TryGetValue(id.Trim(), out var entry)
            ? entry
            : GeneratorPlanCapabilityHelpMetadata.Fallback(id.Trim());
    }

    public static IReadOnlyList<GeneratorPlanCapabilityCompositionSeed> ListCompositionSeeds()
    {
        return CompositionSeeds;
    }

    public static string MapDiagnosticCategory(string code)
    {
        return code switch
        {
            GeneratorPlanCapabilitySelectionDiagnosticCodes.IncompatiblePresentationWorld => GeneratorPlanCapabilitySelectionDiagnosticCategories.Impossible,
            GeneratorPlanCapabilitySelectionDiagnosticCodes.VariantExplicitlyIncompatible => GeneratorPlanCapabilitySelectionDiagnosticCategories.Impossible,
            GeneratorPlanCapabilitySelectionDiagnosticCodes.MissingArtifactContract => GeneratorPlanCapabilitySelectionDiagnosticCategories.UnsupportedYet,
            GeneratorPlanCapabilitySelectionDiagnosticCodes.CapabilityGap => GeneratorPlanCapabilitySelectionDiagnosticCategories.UnsupportedYet,
            GeneratorPlanCapabilitySelectionDiagnosticCodes.MissingValidator => GeneratorPlanCapabilitySelectionDiagnosticCategories.UnsupportedYet,
            GeneratorPlanCapabilitySelectionDiagnosticCodes.VariantNotRecommended => GeneratorPlanCapabilitySelectionDiagnosticCategories.Risky,
            GeneratorPlanCapabilitySelectionDiagnosticCodes.Loaded => GeneratorPlanCapabilitySelectionDiagnosticCategories.Info,
            _ => GeneratorPlanCapabilitySelectionDiagnosticCategories.Info
        };
    }

    private static IReadOnlyDictionary<string, GeneratorPlanCapabilityHelpMetadata> BuildEntries()
    {
        return new[]
        {
            Entry("presentation_mode/map_and_panel_rpg", "Карта + панельная RPG", "Map and panel RPG", "Игра с картой, регионами или узлами и панелями для сцен, диалогов, боя и событий.", "Хорошая основа для текстовой RPG, исследования регионов и сюжетных цепочек без прямого action-управления.", "Путешествие по графу регионов, события в локациях, диалоги и проверки навыков.", "Текстовые RPG, exploration RPG, narrative RPG.", "Обычно не подходит для direct first-person action или бесконечного tile streaming как главной модели.", "foundation_supported"),
            Entry("presentation_mode/first_person_grid_2d_textures", "Псевдо-3D сетка от первого лица", "First-person grid with 2D textures", "Игрок движется по клеткам и смотрит на мир от первого лица.", "Подходит для blobber/ dungeon crawler формы с дискретным перемещением и партийным интерфейсом.", "Dungeon crawler с клетками, дверями, ловушками и пошаговым боем.", "Партийные dungeon crawler игры.", "Требует аккуратной топологии и pathfinding профиля.", "foundation_supported"),
            Entry("presentation_mode/top_down_2d", "2D вид сверху", "Top-down 2D", "Классическая карта сверху с персонажем или группой на плоскости.", "Подходит для небольших карт, тактического перемещения и простых action/turn-based прототипов.", "Локация с интерактивными объектами, NPC и переходами.", "Компактные карты, tactical RPG, adventure.", "Не всякая party-blob логика хорошо ложится на эту форму.", "foundation_supported"),
            Entry("world_topology/region_graph", "Граф регионов", "Region graph", "Мир состоит из регионов и связей между ними.", "Внутри региона могут быть сцены, события, поселения, ресурсы и переходы.", "Регион старта, соседний город, опасная дорога, руины.", "Текстовые RPG, exploration RPG, большие миры без tile streaming.", "Для истинного бесконечного мира позже нужны chunk/rule контракты.", "foundation_planned"),
            Entry("world_topology/first_person_grid_dungeon", "Сеточный dungeon", "First-person grid dungeon", "Мир состоит из клеток, направлений и переходов.", "Ранний понятный формат для проверки party RPG и dungeon crawler связки.", "Комната, коридор, поворот, дверь, лестница.", "Blobber RPG и пошаговое исследование.", "Не является свободным 3D миром.", "foundation_supported"),
            Entry("world_topology/single_map", "Одна карта", "Single map", "Одна ограниченная карта без сложной генерации регионов.", "Простая топология для маленьких прототипов и smoke-сценариев.", "Деревня, арена, тестовая локация.", "Минимальные прототипы.", "Масштабирование в большой мир потребует другой топологии.", "foundation_supported"),
            Entry("actor_model/party_blob", "Партия как единый отряд", "Party blob", "Группа героев перемещается как один отряд.", "Классический режим для старых партийных RPG: у каждого героя могут быть характеристики, но позиция общая.", "Четыре героя исследуют dungeon одной группой.", "Party RPG и blobber combat.", "Не подходит для независимого перемещения каждого участника без расширения runtime.", "foundation_supported"),
            Entry("actor_model/single_player_character", "Один главный персонаж", "Single player character", "Игрок управляет одним главным персонажем.", "Самая простая модель для базовой проверки механик, квестов и инвентаря.", "Один герой принимает решения, сражается и получает предметы.", "Adventure, solo RPG, survival prototype.", "Party roster потребует отдельного модуля.", "foundation_supported"),
            Entry("inventory_model/grid_inventory", "Сеточный инвентарь", "Grid inventory", "Предметы занимают ячейки в сетке.", "Даёт понятную RPG-метафору инвентаря, но требует проверок размера и пересечений.", "Рюкзак 8x6, предмет 2x3, экипировка отдельно.", "RPG с предметами и лутом.", "Может усложнить ранний прототип.", "foundation_supported"),
            Entry("combat_model/blobber_party_turn_based", "Пошаговый бой партии", "Blobber party turn-based", "Партия сражается пошагово как единый отряд.", "Хорошо сочетается с first-person grid и party blob.", "Раунды, фронтлайн, способности героев.", "Dungeon crawler и party RPG.", "Realtime toggle пока отдельный будущий модуль.", "foundation_supported"),
            Entry("combat_model/turn_based", "Пошаговый бой", "Turn-based combat", "Бой строится по ходам.", "Базовая безопасная форма для строгих артефактов и будущей проверки баланса.", "Игрок выбирает действие, враг отвечает.", "Tactical RPG, text RPG, prototypes.", "Может быть слишком статичным для action-игр.", "foundation_supported"),
            Entry("combat_model/dialogue_combat", "Диалоговый бой", "Dialogue combat", "Бой представлен серией выборов, проверок и последствий.", "Это валидная дизайн-идея, но нуждается в отдельных контрактах диалогов/эффектов.", "Спор, дуэль аргументов, психологическая схватка.", "Narrative RPG и social combat.", "Может быть unsupported_yet в текущем наборе контрактов.", "foundation_planned"),
            Entry("progression_model/level_xp", "Уровни и опыт", "Level XP", "Персонажи получают опыт и уровни.", "Базовый progression core, который позже можно комбинировать с перками, навыками и репутацией.", "XP за квест, новый уровень, очки характеристик.", "Классическая RPG-прогрессия.", "Дерево перков и skill XP должны быть модулями, а не заменой core.", "foundation_supported"),
            Entry("pathfinding/first_person_grid_movement", "Шаги по сетке от первого лица", "First-person grid movement", "Перемещение клетка за клеткой с направлением взгляда.", "Поддерживает dungeon crawler топологию и проверку достижимости.", "Шаг вперёд, поворот, дверь, препятствие.", "First-person grid dungeon.", "Не заменяет произвольный navigation mesh.", "foundation_supported"),
            Entry("npc_behavior/static", "Статичные NPC", "Static NPC", "NPC не перемещаются сами и служат источниками диалогов, торговли или событий.", "Безопасная базовая модель для ранних артефактов.", "Торговец в городе, страж у ворот.", "Ранние RPG-сцены.", "Живой AI и расписания требуют будущих модулей.", "foundation_supported"),
            Entry("headless", "Headless runtime target", "Headless", "Цель для проверки логики без UI.", "Используется для контрактов, тестов и будущих smoke-проверок.", "Сервисная проверка package/runtime поведения.", "Автотесты и backend validation.", "Не означает готовый визуальный player.", "foundation_supported"),
            Entry("debug", "Debug preview target", "Debug", "Отладочная цель для редактора и диагностик.", "Помогает проверять артефакты без production export.", "WinForms preview или диагностика.", "Локальная отладка.", "Не является финальным player/export.", "foundation_supported"),
            Entry("feature_bundle/core_atlas_planning/v1", "Базовое планирование через atlas", "Core atlas planning", "Минимальный bundle для построения capability selection.", "Собирает базовые контракты, валидаторы и prompt context для M4 flow.", "Выбрать форму игры и сохранить latest selection.", "Любая первая генерация строгих артефактов.", "Сам по себе не создаёт GamePackage.", "foundation_supported"),
            Entry("feature_bundle/dialogue_choice_graph/v1", "Граф диалоговых выборов", "Dialogue choice graph", "Добавляет будущую структуру диалогов с выбором и последствиями.", "Полезно для narrative RPG, но требует валидаторов и артефактов диалогов.", "Вопрос, варианты ответа, последствия.", "Сюжетные и социальные RPG.", "Может быть unsupported_yet, если контракт не выбран.", "foundation_planned"),
            Entry("feature_bundle/faction_reputation/v1", "Репутация фракций", "Faction reputation", "Отношения игрока с фракциями влияют на мир.", "Будущая основа для реакций NPC, цен, квестов и доступа.", "Гильдия доверяет игроку, город вводит налог.", "RPG с фракциями и экономикой.", "Требует баланса и state/runtime поддержки.", "foundation_planned"),
            Entry("feature_bundle/world_region_chunk_generation/v1", "Генерация регионов и chunks", "World region/chunk generation", "Будущая генерация больших миров через регионы, правила и chunks.", "В этом slice это только понятная справка и metadata foundation.", "Seed -> region graph -> local nodes -> optional chunks.", "Большие procedural RPG.", "Истинный infinite streaming не реализуется здесь.", "foundation_planned"),
            Entry("feature_bundle/city_builder_production_conquest/v1", "Город, производство и завоевание", "City builder production/conquest", "Системы производства, поселений и контроля территорий.", "Сложный bundle для будущих симуляционных игр.", "Поселение добывает ресурс, строит здания, расширяется.", "City builder и strategy hybrids.", "Высокий риск scope expansion.", "future"),
            Entry("feature_bundle/inventory_panel_grid/v1", "Панель сеточного инвентаря", "Inventory panel grid", "UI/contract направление для сеточного инвентаря.", "Связано с предметами, размерами, слотами и проверкой пересечений.", "Рюкзак, предметы, drag/drop в будущем UI.", "RPG с loot/inventory.", "Не добавляет новый UI inventory в этом slice.", "foundation_planned"),
            Entry("feature_bundle/horror_content_overlay/v1", "Хоррор-оверлей", "Optional horror content overlay", "Опциональная атмосфера хоррора поверх базовой игры.", "Должна оставаться явным выбором и не включаться скрыто.", "Темные события, напряжение, ограниченные ресурсы.", "Horror RPG и survival.", "Не включать как дефолт.", "foundation_planned"),
            Entry("feature_bundle/party_roster_progression/v1", "Партия и развитие героев", "Party roster/progression", "Состав партии, роли героев и развитие.", "Будущая связка character cards, progression и combat.", "Воин, маг, лекарь получают уровни и навыки.", "Party RPG.", "Требует аккуратной композиции progression модулей.", "foundation_planned"),
            Entry("feature_bundle/combat_realtime_turn_hybrid/v1", "Гибридный бой realtime + turns", "Realtime/turn hybrid combat", "Бой может переключаться между реальным временем и пошаговым режимом.", "Важный будущий модуль для Might and Magic-подобного поведения.", "Свободное время до угрозы, затем пошаговый режим.", "Party RPG с гибридным боем.", "Не реализует runtime toggle в этом slice.", "foundation_planned"),
            Entry("feature_bundle/survival_sandbox/v1", "Survival sandbox", "Survival sandbox", "Выживание, ресурсы, крафт и опасности среды.", "Сильно зависит от economy, crafting, ресурсных узлов и баланса.", "Голод, погода, добыча ресурсов.", "Survival RPG.", "Высокий риск сложной симуляции.", "foundation_planned")
        }.ToDictionary(entry => entry.Id, StringComparer.OrdinalIgnoreCase);
    }

    private static GeneratorPlanCapabilityHelpMetadata Entry(
        string id,
        string displayNameRu,
        string displayNameEn,
        string shortDescriptionRu,
        string detailsRu,
        string examplesRu,
        string bestForRu,
        string warningsRu,
        string implementationStatus)
    {
        return new GeneratorPlanCapabilityHelpMetadata
        {
            Id = id,
            DisplayNameRu = displayNameRu,
            DisplayNameEn = displayNameEn,
            ShortDescriptionRu = shortDescriptionRu,
            DetailsRu = detailsRu,
            ExamplesRu = examplesRu,
            BestForRu = bestForRu,
            WarningsRu = warningsRu,
            ImplementationStatus = implementationStatus
        };
    }

    private static IReadOnlyList<GeneratorPlanCapabilityCompositionSeed> BuildCompositionSeeds()
    {
        return
        [
            Seed("module/progression/perk_tree", "module", "Дерево перков", "Открываемые ветки улучшений."),
            Seed("module/progression/level_up_stat_allocation", "module", "Очки характеристик за уровень", "Игрок распределяет параметры при повышении уровня."),
            Seed("module/progression/skill_xp", "module", "Опыт навыков", "Навыки растут от использования."),
            Seed("module/progression/class_tree", "module", "Классовое дерево", "Развитие через классы и специализации."),
            Seed("module/progression/faction_rank", "module", "Ранг фракции", "Прогрессия через доверие и статус во фракциях."),
            Seed("module/progression/metamodule_growth", "module", "Рост metamodule", "Долгосрочные улучшения за пределами обычного уровня."),
            Seed("modifier/combat/realtime", "modifier", "Бой в реальном времени", "Действия идут без фиксированных ходов."),
            Seed("modifier/combat/turn_based", "modifier", "Пошаговый бой", "Бой разделён на ходы."),
            Seed("modifier/combat/hybrid_realtime_turn_toggle", "modifier", "Гибрид realtime + turns", "Игрок может переключать режим боя."),
            Seed("module/combat/dialogue_combat", "module", "Диалоговый бой", "Конфликт через выборы и проверки."),
            Seed("module/combat/party_commands", "module", "Команды партии", "Игрок отдаёт команды нескольким героям."),
            Seed("module/world/region_graph", "module", "Граф регионов", "Мир как регионы и связи."),
            Seed("module/world/chunk_generation", "module", "Chunk generation", "Правила генерации больших областей."),
            Seed("module/world/biomes", "module", "Биомы", "Региональные типы окружения."),
            Seed("module/world/weather", "module", "Погода", "Состояние погоды влияет на события и мир."),
            Seed("module/world/time_of_day", "module", "Время суток", "День, ночь и связанные правила."),
            Seed("module/world/procedural_events", "module", "Процедурные события", "События выбираются по правилам и контексту."),
            Seed("module/world/settlements", "module", "Поселения", "Города, деревни и точки торговли."),
            Seed("module/economy/economy", "module", "Экономика", "Общая модель цен, ресурсов и торговли."),
            Seed("module/economy/trading", "module", "Торговля", "Покупка, продажа и рынки."),
            Seed("module/economy/price_policy", "module", "Политика цен", "Факторы цены: регион, редкость, фракции, спрос."),
            Seed("module/economy/supply_demand", "module", "Спрос и предложение", "Динамика доступности и стоимости."),
            Seed("module/balance/power_budget", "module", "Power budget", "Бюджеты силы предметов, врагов и наград."),
            Seed("module/balance/encounter_tiers", "module", "Тиры encounter", "Уровни опасности встреч.")
        ];
    }

    private static GeneratorPlanCapabilityCompositionSeed Seed(string id, string kind, string displayNameRu, string shortDescriptionRu)
    {
        return new GeneratorPlanCapabilityCompositionSeed
        {
            Id = id,
            Kind = kind,
            DisplayNameRu = displayNameRu,
            ShortDescriptionRu = shortDescriptionRu
        };
    }
}
