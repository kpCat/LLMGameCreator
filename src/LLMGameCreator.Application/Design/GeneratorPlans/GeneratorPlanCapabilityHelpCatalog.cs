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
            GeneratorPlanCapabilitySelectionDiagnosticCodes.UnknownComposableCapabilityId => GeneratorPlanCapabilitySelectionDiagnosticCategories.UnsupportedYet,
            GeneratorPlanCapabilitySelectionDiagnosticCodes.ComposableCapabilityUnsupportedYet => GeneratorPlanCapabilitySelectionDiagnosticCategories.UnsupportedYet,
            GeneratorPlanCapabilitySelectionDiagnosticCodes.VariantNotRecommended => GeneratorPlanCapabilitySelectionDiagnosticCategories.Risky,
            GeneratorPlanCapabilitySelectionDiagnosticCodes.ComposableCapabilityRisky => GeneratorPlanCapabilitySelectionDiagnosticCategories.Risky,
            GeneratorPlanCapabilitySelectionDiagnosticCodes.ComposableCapabilityInfo => GeneratorPlanCapabilitySelectionDiagnosticCategories.Info,
            GeneratorPlanCapabilitySelectionDiagnosticCodes.Loaded => GeneratorPlanCapabilitySelectionDiagnosticCategories.Info,
            _ => GeneratorPlanCapabilitySelectionDiagnosticCategories.Info
        };
    }

    public static string MapDiagnosticCategoryDisplayName(string category)
    {
        return category switch
        {
            GeneratorPlanCapabilitySelectionDiagnosticCategories.Impossible => "\u041d\u0435\u043b\u044c\u0437\u044f \u0441\u043e\u0432\u043c\u0435\u0441\u0442\u0438\u0442\u044c",
            GeneratorPlanCapabilitySelectionDiagnosticCategories.UnsupportedYet => "\u0418\u0434\u0435\u044f \u0432\u043e\u0437\u043c\u043e\u0436\u043d\u0430, \u043d\u043e \u043f\u043e\u0434\u0434\u0435\u0440\u0436\u043a\u0430 \u0435\u0449\u0451 \u043d\u0435 \u0434\u043e\u0434\u0435\u043b\u0430\u043d\u0430",
            GeneratorPlanCapabilitySelectionDiagnosticCategories.Risky => "\u041c\u043e\u0436\u043d\u043e, \u043d\u043e \u0435\u0441\u0442\u044c \u0440\u0438\u0441\u043a/\u043d\u0443\u0436\u043d\u0430 \u043e\u0441\u0442\u043e\u0440\u043e\u0436\u043d\u043e\u0441\u0442\u044c",
            GeneratorPlanCapabilitySelectionDiagnosticCategories.Info => "\u0418\u043d\u0444\u043e\u0440\u043c\u0430\u0446\u0438\u044f",
            _ => string.IsNullOrWhiteSpace(category) ? "\u0418\u043d\u0444\u043e\u0440\u043c\u0430\u0446\u0438\u044f" : category
        };
    }

    private static IReadOnlyDictionary<string, GeneratorPlanCapabilityHelpMetadata> BuildEntries()
    {
        return new[]
        {
            Entry(
                "presentation_mode/map_and_panel_rpg",
                "Карта + панельная RPG",
                "Map and panel RPG",
                "Игра с картой, регионами или узлами и панелями для сцен, диалогов, боя и событий.",
                "Хорошая основа для текстовой RPG, исследования регионов и сюжетных цепочек без прямого action-управления.",
                "Путешествие по графу регионов, события в локациях, диалоги и проверки навыков.",
                "Текстовые RPG, exploration RPG, narrative RPG.",
                "Обычно не подходит для direct first-person action или бесконечного tile streaming как главной модели.",
                "foundation_supported"),
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
            Entry(
                "feature_bundle/core_atlas_planning/v1",
                "\u041e\u0431\u044f\u0437\u0430\u0442\u0435\u043b\u044c\u043d\u0430\u044f \u0442\u0435\u0445\u043d\u0438\u0447\u0435\u0441\u043a\u0430\u044f \u0431\u0430\u0437\u0430 \u0433\u0435\u043d\u0435\u0440\u0430\u0446\u0438\u0438",
                "Core atlas planning",
                "\u042d\u0442\u043e \u043d\u0435 \u0438\u0433\u0440\u043e\u0432\u0430\u044f \u043c\u0435\u0445\u0430\u043d\u0438\u043a\u0430.",
                "\u041d\u0443\u0436\u043d\u043e, \u0447\u0442\u043e\u0431\u044b \u0433\u0435\u043d\u0435\u0440\u0430\u0442\u043e\u0440 \u0437\u043d\u0430\u043b \u0431\u0430\u0437\u043e\u0432\u044b\u0435 \u043a\u043e\u043d\u0442\u0440\u0430\u043a\u0442\u044b/\u0432\u0430\u043b\u0438\u0434\u0430\u0442\u043e\u0440\u044b \u0434\u043b\u044f \u0441\u043e\u0437\u0434\u0430\u043d\u0438\u044f \u0430\u0440\u0442\u0435\u0444\u0430\u043a\u0442\u043e\u0432. " +
                "\u041e\u0431\u044b\u0447\u043d\u043e \u043e\u0441\u0442\u0430\u0432\u043b\u044f\u0439 \u0432\u043a\u043b\u044e\u0447\u0451\u043d\u043d\u044b\u043c.",
                "\u0412\u044b\u0431\u043e\u0440 \u0444\u043e\u0440\u043c\u044b \u0438\u0433\u0440\u044b, \u0431\u0430\u0437\u043e\u0432\u044b\u0445 \u043a\u043e\u043d\u0442\u0440\u0430\u043a\u0442\u043e\u0432 \u0438 \u0441\u0442\u0440\u043e\u0433\u0438\u0445 \u0430\u0440\u0442\u0435\u0444\u0430\u043a\u0442\u043e\u0432.",
                "\u041b\u044e\u0431\u0430\u044f \u043f\u0435\u0440\u0432\u0430\u044f \u0433\u0435\u043d\u0435\u0440\u0430\u0446\u0438\u044f \u0441\u0442\u0440\u043e\u0433\u0438\u0445 \u0430\u0440\u0442\u0435\u0444\u0430\u043a\u0442\u043e\u0432.",
                "\u0421\u0430\u043c \u043f\u043e \u0441\u0435\u0431\u0435 \u043d\u0435 \u0441\u043e\u0437\u0434\u0430\u0451\u0442 GamePackage \u0438 \u043d\u0435 \u0434\u043e\u0431\u0430\u0432\u043b\u044f\u0435\u0442 gameplay.",
                "foundation_supported"),
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
            Seed("module/balance/encounter_tiers", "module", "Тиры encounter", "Уровни опасности встреч."),
            Seed("constraint/balance/no_player_rubberbanding", "constraint", "Без прямой подстройки мира под игрока", "Мир балансируется через зоны, фракции, редкость и прогрессию, а не скрытую подгонку всего под героя."),
            Seed("constraint/world/safe_start_region_required", "constraint", "Безопасный стартовый регион", "Стартовая область должна давать понятный вход, базовые ресурсы и выходы без смертельной ловушки."),
            Seed("constraint/economy/no_infinite_money_loops", "constraint", "Без бесконечных денежных циклов", "Экономические правила не должны создавать очевидную бесконечную прибыль без риска или расхода."),
            Seed("constraint/combat/enemy_counterplay_required", "constraint", "У врагов должна быть контригра", "Каждая опасная угроза должна иметь понятный ответ игрока или подготовку."),
            Seed("runtime_requirement/requires_region_graph", "runtime_requirement", "Нужен граф регионов", "Runtime должен уметь хранить регионы и переходы между ними."),
            Seed("runtime_requirement/requires_chunk_streaming", "runtime_requirement", "Нужен chunk streaming", "Runtime должен поддерживать подгрузку или генерацию больших областей частями."),
            Seed("runtime_requirement/requires_day_night_cycle", "runtime_requirement", "Нужен цикл день/ночь", "Runtime должен хранить и обновлять время суток."),
            Seed("runtime_requirement/requires_weather_state", "runtime_requirement", "Нужно состояние погоды", "Runtime должен хранить погоду и её влияние на события."),
            Seed("runtime_requirement/requires_trade_market_state", "runtime_requirement", "Нужно состояние рынков", "Runtime должен хранить цены, доступность товаров и торговые изменения."),
            Seed("runtime_requirement/requires_turn_toggle", "runtime_requirement", "Нужно переключение realtime/turns", "Runtime должен поддерживать гибридный режим боя."),
            Seed("runtime_requirement/requires_party_state", "runtime_requirement", "Нужно состояние партии", "Runtime должен хранить состав, роли и развитие героев.")
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
