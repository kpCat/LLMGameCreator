using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedGameplaySavesSummaryService
{
    private readonly GeneratedGameplaySaveService _saveService;

    public GeneratedGameplaySavesSummaryService(GeneratedGameplaySaveService saveService)
    {
        _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
    }

    public GeneratedGameplaySavesSummary Read(string projectFolder)
    {
        if (!_saveService.IsGeneratedProject(projectFolder)) return new GeneratedGameplaySavesSummary();
        return FromList(_saveService.List(projectFolder));
    }

    internal GeneratedGameplaySavesSummary Read(
        string projectFolder,
        GameProjectOperationLease operationLease) =>
        FromList(_saveService.List(projectFolder, operationLease));

    public static IReadOnlyList<GeneratedGameplaySaveFact> StandaloneHumanFacts(
        GeneratedGameplaySavesSummary? summary) => summary is { LastMigration: not null }
        ? summary.HumanFacts
        : [];

    private static GeneratedGameplaySavesSummary FromList(GeneratedGameplaySaveListResult list)
    {
        var generated = list.Entries.Where(entry => !entry.LegacyRaw).ToList();
        var latest = generated.Where(entry => entry.Migration is not null)
            .Select(entry => entry.Migration).LastOrDefault();
        var facts = latest is null
            ? []
            : (IReadOnlyList<GeneratedGameplaySaveFact>)
            [
                new GeneratedGameplaySaveFact { Label = "Игровое сохранение", Value = "перенесено" },
                new GeneratedGameplaySaveFact { Label = "Мир сохранения", Value = "текущий" },
                new GeneratedGameplaySaveFact { Label = "Позиция", Value = latest.MapReset
                    ? "сброшена на старт" : "сохранена" },
                new GeneratedGameplaySaveFact { Label = "Сохранено данных", Value = latest.PreservedCounts.Values.Sum()
                    .ToString(System.Globalization.CultureInfo.InvariantCulture) },
                new GeneratedGameplaySaveFact { Label = "Сброшено данных", Value = latest.DroppedCounts.Values.Sum()
                    .ToString(System.Globalization.CultureInfo.InvariantCulture) },
                new GeneratedGameplaySaveFact { Label = "Проверка после загрузки", Value = "пройдена" }
            ];
        return new GeneratedGameplaySavesSummary
        {
            Present = list.Entries.Count > 0,
            Passed = list.Passed,
            SlotCount = generated.Count,
            CurrentCount = generated.Count(entry => entry.Status == GeneratedGameplaySaveStatus.CURRENT),
            MigrationRequiredCount = generated.Count(entry => entry.Status is
                GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED
                or GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED),
            InvalidCount = generated.Count(entry => entry.Status == GeneratedGameplaySaveStatus.INVALID),
            LegacyRawCount = list.Entries.Count(entry => entry.LegacyRaw),
            LastMigration = latest,
            Entries = list.Entries,
            HumanFacts = facts,
            Diagnostics = list.Diagnostics
        };
    }
}
