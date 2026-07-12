using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public sealed class GameProjectWorkspaceStatusPresenter
{
    private static readonly IReadOnlyDictionary<string, (string Title, string Description, string Category)> Presentation =
        new Dictionary<string, (string, string, string)>(StringComparer.Ordinal)
        {
            ["feature.world.grid_navigation"] = ("Навигация по миру", "Перемещение по клеточной карте и границам мира.", "Мир"),
            ["feature.interaction.basic"] = ("Взаимодействия", "Базовые действия с объектами и персонажами.", "Игровой процесс"),
            ["feature.dialogue.basic"] = ("Диалоги", "Разговоры и выбор реплик.", "Сюжет"),
            ["feature.quest.objective_chain"] = ("Задания", "Цепочки целей и состояние заданий.", "Сюжет"),
            ["feature.inventory.basic"] = ("Инвентарь", "Хранение и использование предметов.", "Предметы"),
            ["feature.crafting.recipes"] = ("Создание предметов", "Рецепты и преобразование ресурсов.", "Предметы"),
            ["feature.resources.harvest"] = ("Сбор ресурсов", "Получение ресурсов из объектов мира.", "Ресурсы"),
            ["feature.economy.transaction"] = ("Обмен и торговля", "Проверяемые игровые транзакции.", "Экономика"),
            ["feature.combat.turn_based_encounter"] = ("Пошаговый бой", "Встречи, атаки и состояние участников боя.", "Бой"),
            ["feature.player_adapter.runtime_summary"] = ("Отображение состояния игры", "Показывает подтверждённое Runtime-состояние игроку.", "Представление"),
            ["feature.profile.alchemy_focus"] = ("Углублённая алхимия", "Расширяет приготовление зелий и стартовые алхимические ресурсы.", "Алхимия"),
            ["feature.profile.combat_focus"] = ("Усиленный бой", "Настраивает силу атак и выносливость противников.", "Бой"),
            ["feature.profile.exploration_resource_focus"] = ("Расширенный сбор ресурсов", "Добавляет более богатый сбор и полезные результаты обмена.", "Исследование")
        };

    public IReadOnlyList<GameProjectMechanicPresentation> Mechanics(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyCollection<string> selectedModuleIds)
    {
        var selected = selectedModuleIds.ToHashSet(StringComparer.Ordinal);
        var titleById = catalog.Modules.ToDictionary(module => module.ModuleId, FriendlyTitle, StringComparer.Ordinal);
        return catalog.Modules
            .OrderByDescending(module => module.Required)
            .ThenBy(module => FriendlyTitle(module), StringComparer.CurrentCulture)
            .Select(module =>
            {
                var metadata = Metadata(module);
                return new GameProjectMechanicPresentation
                {
                    ModuleId = module.ModuleId,
                    Title = metadata.Title,
                    Description = metadata.Description,
                    Category = metadata.Category,
                    Required = module.Required,
                    Selected = module.Required || selected.Contains(module.ModuleId),
                    DependencyTitles = module.Dependencies.Select(id => titleById.GetValueOrDefault(id, id)).ToList(),
                    ConflictTitles = module.Conflicts.Select(id => titleById.GetValueOrDefault(id, id)).ToList()
                };
            })
            .ToList();
    }

    public string AuthoringStatus(bool dirty, bool valid, bool missingDependencies)
    {
        if (missingDependencies) return "Не хватает модулей";
        if (!valid) return "Есть ошибки";
        if (dirty) return "Есть несохранённые изменения";
        return "Готово";
    }

    public string PackageStatus(string lastQualificationStatus, bool dirty)
    {
        if (dirty && string.Equals(lastQualificationStatus, "GREEN", StringComparison.Ordinal)) return "Требуется пересборка";
        if (string.Equals(lastQualificationStatus, "GREEN", StringComparison.Ordinal)) return "Готово";
        if (string.Equals(lastQualificationStatus, "FAILED", StringComparison.Ordinal)) return "Есть ошибки";
        return "Проверка ещё не запускалась";
    }

    public string FriendlyTitle(FeatureModuleDefinition module) => Metadata(module).Title;

    private static (string Title, string Description, string Category) Metadata(FeatureModuleDefinition module) =>
        Presentation.TryGetValue(module.ModuleId, out var value)
            ? value
            : (module.Title,
                string.IsNullOrWhiteSpace(module.Description) ? "Механика из каталога проекта." : module.Description,
                module.Category);
}
