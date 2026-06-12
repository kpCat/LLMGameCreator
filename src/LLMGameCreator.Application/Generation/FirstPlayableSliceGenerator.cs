using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation;

public sealed partial class FirstPlayableSliceGenerator : IFirstPlayableSliceGenerator
{
    private const string Category = "GenerationDraft";
    private readonly IAppSettingsRepository _settingsRepository;
    private readonly ILlmChatClient _llmChatClient;
    private readonly ICurrentGamePackageService _currentGamePackageService;
    private readonly IGamePackageValidator _validator;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public FirstPlayableSliceGenerator(
        IAppSettingsRepository settingsRepository,
        ILlmChatClient llmChatClient,
        ICurrentGamePackageService currentGamePackageService,
        IGamePackageValidator validator)
    {
        _settingsRepository = settingsRepository;
        _llmChatClient = llmChatClient;
        _currentGamePackageService = currentGamePackageService;
        _validator = validator;
    }

    public async Task<GenerationResult> TestConnectionAsync(CancellationToken cancellationToken)
    {
        var profile = await LoadDefaultProfileAsync(cancellationToken).ConfigureAwait(false);
        var response = await _llmChatClient.CompleteAsync(profile, new LlmChatRequest
        {
            SystemPrompt = "Return ONLY valid JSON. No markdown.",
            UserPrompt = "{\"ok\":true}",
            MaxTokens = 64
        }, cancellationToken).ConfigureAwait(false);

        return new GenerationResult
        {
            Success = true,
            RawContent = response.Content,
            Message = "LM Studio ответил на /chat/completions.",
            ProfileTitle = profile.Title,
            Endpoint = response.Endpoint,
            Model = response.Model
        };
    }

    public async Task<GenerationResult> GenerateAsync(GenerationInterviewModel interview, CancellationToken cancellationToken)
    {
        var profile = await LoadDefaultProfileAsync(cancellationToken).ConfigureAwait(false);
        var response = await _llmChatClient.CompleteAsync(profile, new LlmChatRequest
        {
            SystemPrompt = BuildSystemPrompt(),
            UserPrompt = BuildUserPrompt(interview),
            Temperature = 0.2,
            MaxTokens = 6000
        }, cancellationToken).ConfigureAwait(false);

        var result = new GenerationResult
        {
            RawContent = response.Content,
            ProfileTitle = profile.Title,
            Endpoint = response.Endpoint,
            Model = response.Model
        };

        var json = ExtractStrictJson(response.Content);
        if (json == null)
        {
            result.Message = "LM Studio вернул не strict JSON. Markdown, пояснения и текст вокруг JSON запрещены.";
            result.DraftValidationReport.Issues.Add(CreateIssue("draft.json.strict", "Ответ должен быть одним JSON object без markdown и текста вокруг.", null));
            return result;
        }

        result.RawJson = json;
        FirstPlayableSliceDraft? draft;
        try
        {
            draft = JsonSerializer.Deserialize<FirstPlayableSliceDraft>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            result.Message = $"JSON не удалось разобрать: {ex.Message}";
            result.DraftValidationReport.Issues.Add(CreateIssue("draft.json.invalid", result.Message, null));
            return result;
        }

        if (draft == null)
        {
            result.Message = "JSON разобран в пустой draft.";
            result.DraftValidationReport.Issues.Add(CreateIssue("draft.empty", result.Message, null));
            return result;
        }

        var report = ValidateDraft(draft);
        result.Draft = draft;
        result.DraftValidationReport = report;
        result.Success = report.IsValid;
        result.Message = report.IsValid
            ? "Draft успешно получен и прошёл локальную проверку."
            : "Draft получен, но не прошёл локальную проверку.";
        return result;
    }

    public async Task<GenerationResult> AnalyzeBriefAsync(GenerationInterviewModel interview, CancellationToken cancellationToken)
    {
        var profile = await LoadDefaultProfileAsync(cancellationToken).ConfigureAwait(false);
        var response = await _llmChatClient.CompleteAsync(profile, new LlmChatRequest
        {
            SystemPrompt = BuildHelperSystemPrompt(),
            UserPrompt = BuildHelperUserPrompt(interview),
            Temperature = 0.35,
            MaxTokens = 1800
        }, cancellationToken).ConfigureAwait(false);

        return new GenerationResult
        {
            Success = true,
            RawContent = response.Content,
            Message = "ИИ вернул вопросы и варианты. Package не изменён.",
            ProfileTitle = profile.Title,
            Endpoint = response.Endpoint,
            Model = response.Model
        };
    }

    public FirstPlayableSliceApplyResult ApplyDraft(FirstPlayableSliceDraft draft)
    {
        var draftReport = ValidateDraft(draft);
        if (!draftReport.IsValid)
        {
            return new FirstPlayableSliceApplyResult
            {
                Success = false,
                Message = "Draft не применён: локальная проверка draft не пройдена.",
                ValidationReport = draftReport
            };
        }

        var current = _currentGamePackageService.CurrentPackage;
        if (current == null)
        {
            return new FirstPlayableSliceApplyResult
            {
                Success = false,
                Message = "Проект игры не открыт."
            };
        }

        var copy = ClonePackage(current);
        ApplyToCopy(copy, draft);

        var report = _validator.Validate(copy, _currentGamePackageService.CurrentFolder);
        if (!report.IsValid)
        {
            return new FirstPlayableSliceApplyResult
            {
                Success = false,
                Message = "Draft не применён: итоговый GamePackage не прошёл validation.",
                ValidationReport = report
            };
        }

        _currentGamePackageService.ReplaceCurrent(copy);
        return new FirstPlayableSliceApplyResult
        {
            Success = true,
            Message = "Draft применён к текущему package.",
            ValidationReport = report
        };
    }

    private async Task<LlmEndpointSettings> LoadDefaultProfileAsync(CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
        var profile = settings.LlmProfiles.FirstOrDefault(item => string.Equals(item.Id, settings.DefaultLlmProfileId, StringComparison.Ordinal))
            ?? settings.LlmProfiles.FirstOrDefault();
        if (profile == null)
        {
            throw new InvalidOperationException("В appsettings не найден ни один LLM profile.");
        }

        if (string.IsNullOrWhiteSpace(profile.Endpoint))
        {
            throw new InvalidOperationException($"LLM profile '{profile.Id}' не содержит endpoint.");
        }

        if (string.IsNullOrWhiteSpace(profile.Model))
        {
            throw new InvalidOperationException($"LLM profile '{profile.Id}' не содержит model.");
        }

        return profile;
    }

    private string BuildUserPrompt(GenerationInterviewModel interview)
    {
        var package = _currentGamePackageService.CurrentPackage;
        var builder = new StringBuilder();
        builder.AppendLine("Generate one first playable slice draft for the current GamePackage.");
        builder.AppendLine();
        builder.AppendLine("1. Current package context");
        builder.AppendLine($"packageId: {package?.Manifest.PackageId ?? "not_open"}");
        builder.AppendLine($"title: {package?.Manifest.Title ?? "not_open"}");
        builder.AppendLine($"current startMapId: {package?.Manifest.StartMapId ?? "not_open"}");
        builder.AppendLine("Use current package identity; do not change project id.");
        builder.AppendLine();
        builder.AppendLine("2. Core idea");
        builder.AppendLine(EmptyAsDash(interview.GameIdea));
        builder.AppendLine();
        builder.AppendLine("3. Steering fields");
        builder.AppendLine($"genre: {interview.Genre}");
        builder.AppendLine($"tone: {interview.Tone}");
        builder.AppendLine($"camera/view: {interview.CameraView}");
        builder.AppendLine($"setting: {interview.Setting}");
        builder.AppendLine($"first location: {interview.FirstLocation}");
        builder.AppendLine($"first conflict: {interview.FirstConflict}");
        builder.AppendLine($"player role: {interview.PlayerRole}");
        builder.AppendLine($"required NPC: {interview.RequiredNpc}");
        builder.AppendLine($"map size preference: {interview.MapWidth}x{interview.MapHeight}");
        builder.AppendLine($"generation mode: {interview.GenerationMode}");
        builder.AppendLine();
        builder.AppendLine("4. Lore");
        builder.AppendLine(EmptyAsDash(interview.LoreNotes));
        builder.AppendLine();
        builder.AppendLine("5. Hard constraints");
        builder.AppendLine(EmptyAsDash(interview.HardConstraints));
        builder.AppendLine();
        builder.AppendLine("6. Must include");
        builder.AppendLine(EmptyAsDash(interview.MustInclude));
        builder.AppendLine();
        builder.AppendLine("7. Must avoid");
        builder.AppendLine(EmptyAsDash(interview.MustAvoid));
        builder.AppendLine();
        builder.AppendLine("8. Player fantasy");
        builder.AppendLine(EmptyAsDash(interview.PlayerFantasy));
        builder.AppendLine();
        builder.AppendLine("9. Gameplay logic wishes / Lua notes");
        builder.AppendLine(EmptyAsDash(interview.GameplayLogicNotes));
        builder.AppendLine();
        builder.AppendLine("10. Scope controls");
        builder.AppendLine($"MaxTileOverrides: {Math.Clamp(interview.MaxTileOverrides, 10, 160)}");
        builder.AppendLine($"TargetNpcCount: {Math.Clamp(interview.TargetNpcCount, 1, 6)}");
        builder.AppendLine($"TargetEntityInstanceCount: {Math.Clamp(interview.TargetEntityInstanceCount, 0, 20)}");
        builder.AppendLine($"TargetQuestCount: {Math.Clamp(interview.TargetQuestCount, 0, 3)}");
        builder.AppendLine($"TargetDialogueCount: {Math.Clamp(interview.TargetDialogueCount, 0, 3)}");
        builder.AppendLine($"DetailMode: {interview.DetailMode}");
        builder.AppendLine($"LogicMode: {interview.LogicMode}");
        builder.AppendLine("Use at most MaxTileOverrides tile overrides.");
        builder.AppendLine("Do not enumerate full map borders cell-by-cell.");
        builder.AppendLine("Use sparse tile overrides.");
        builder.AppendLine("Keep JSON compact in compact/balanced modes.");
        builder.AppendLine("Use detail mode to control amount of generated text.");
        builder.AppendLine("Respect target counts as approximate targets, but remain valid.");
        builder.AppendLine("Respect LogicMode.");
        builder.AppendLine(GetLogicModeInstruction(interview.LogicMode));
        builder.AppendLine();
        builder.AppendLine("11. Required JSON schema example");
        builder.AppendLine("Use this exact JSON shape and no other fields:");
        builder.AppendLine(GetSchemaExample(interview.MapWidth, interview.MapHeight));
        return builder.ToString();
    }

    private static string BuildSystemPrompt()
    {
        return string.Join(Environment.NewLine, new[]
        {
            "You are generating data for LLMGameCreator.",
            "Return ONLY valid JSON object.",
            "No markdown.",
            "No explanations outside JSON.",
            "Do not invent fields outside schema.",
            "Use current package identity; do not change project id.",
            "IDs are lowercase slash ids.",
            "Every tileId must reference tilePrototypes.",
            "Every prototypeId must reference entityPrototypes.",
            "startMapId must reference an existing map.",
            "startPosition must be inside map bounds and on a walkable tile.",
            "Use DefaultTileId plus sparse tile overrides.",
            "Do not enumerate full map borders cell-by-cell.",
            "Use at most requested MaxTileOverrides.",
            "Keep map visually meaningful: road, obstacle, landmark, NPC position.",
            "No Lua code.",
            "Do not generate Lua code.",
            "Do not create Lua files.",
            "No Unity.",
            "No asset generation.",
            "Prefer data-driven quests/dialogues/interactions/effects.",
            "If logic would need Lua later, output ScriptPlans only.",
            "ScriptPlans describe trigger, target, purpose, future entry point and capabilities.",
            "ScriptPlans are planning notes, not executable code.",
            "Respect lore, hard constraints, must include, must avoid, player fantasy.",
            "Treat Genre/Tone/Camera/Setting/FirstConflict as steering labels, not rigid enums.",
            "Keep all components empty arrays for this goal unless existing component shape is explicitly known.",
            "Output limits: exactly 1 start map; map width 12-40; map height 8-30; 3-8 tile prototypes; 1-6 entity prototypes; 0-20 entity instances; 0-3 dialogues; 0-3 quests; sparse tile overrides only."
        });
    }

    private static string BuildHelperSystemPrompt()
    {
        return string.Join(Environment.NewLine, new[]
        {
            "Analyze this game idea and return compact JSON with clarifying questions, suggested variants, recommended field values, and generation risks.",
            "Do not generate GamePackage.",
            "Do not generate Lua code.",
            "Keep response short.",
            "Return JSON only."
        });
    }

    private string BuildHelperUserPrompt(GenerationInterviewModel interview)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Current fields and brief:");
        builder.AppendLine(BuildUserPrompt(interview));
        builder.AppendLine();
        builder.AppendLine("Return this compact helper shape:");
        builder.AppendLine("""
{
  "questions": [
    {
      "id": "tone",
      "question": "Какой уровень мрачности нужен?",
      "why": "Это влияет на NPC/dialogue/quest tone.",
      "options": ["dark fantasy", "grimdark", "dark fairy tale"]
    }
  ],
  "suggestions": [
    {
      "title": "Village under quarantine",
      "description": "..."
    }
  ],
  "recommendedFields": {
    "genre": "rpg",
    "tone": "dark fantasy",
    "setting": "cursed village",
    "firstConflict": "blocked road"
  },
  "risks": [
    "Current idea does not define player goal after talking to elder."
  ]
}
""");
        return builder.ToString();
    }

    private static string GetSchemaExample(int width, int height)
    {
        return $$"""
{
  "title": "Generated Game",
  "packageId": "game/generated",
  "description": "Short playable slice.",
  "startMapId": "map/start",
  "tilePrototypes": [
    { "id": "tile/grass", "name": "Grass", "walkable": true, "movementCost": 1.0, "assetId": null },
    { "id": "tile/wall", "name": "Wall", "walkable": false, "movementCost": 999.0, "assetId": null },
    { "id": "tile/road", "name": "Road", "walkable": true, "movementCost": 0.8, "assetId": null }
  ],
  "entityPrototypes": [
    { "id": "prototype/npc/elder", "name": "Village Elder", "assetId": null, "components": [] }
  ],
  "maps": [
    {
      "id": "map/start",
      "name": "Start Village",
      "width": {{Math.Clamp(width, 12, 40)}},
      "height": {{Math.Clamp(height, 8, 30)}},
      "defaultTileId": "tile/grass",
      "startPosition": { "x": 2, "y": 2 },
      "tiles": [
        { "x": 0, "y": 0, "tileId": "tile/wall" },
        { "x": 1, "y": 0, "tileId": "tile/wall" },
        { "x": 2, "y": 1, "tileId": "tile/road" }
      ],
      "entities": [
        { "id": "entity/elder", "prototypeId": "prototype/npc/elder", "position": { "x": 5, "y": 5 }, "components": [] }
      ]
    }
  ],
  "dialogues": [
    {
      "id": "dialogue/elder_intro",
      "title": "Elder Intro",
      "startNodeId": "start",
      "backgroundAssetId": null,
      "nodes": [
        {
          "id": "start",
          "speakerId": "prototype/npc/elder",
          "expression": "neutral",
          "text": "Welcome.",
          "choices": [
            { "id": "continue", "text": "Continue", "targetNodeId": null, "conditions": [], "effects": [] }
          ]
        }
      ]
    }
  ],
  "quests": [
    {
      "id": "quest/first_task",
      "title": "First Task",
      "description": "Find the blocked road.",
      "stages": [
        { "id": "stage/start", "text": "Talk to the elder.", "completeConditions": [] }
      ]
    }
  ],
  "logicNotes": "Short data-driven behavior summary. No Lua code.",
  "scriptPlans": [
    {
      "id": "script-plan/blocked_road_interaction",
      "kind": "interaction",
      "trigger": "player inspects blocked road",
      "targetId": "entity/blocked_road",
      "purpose": "Future hook for richer blocked road interaction.",
      "suggestedEntryPoint": "on_interact",
      "requiredCapabilities": ["return_effects"],
      "usedBy": ["entity/blocked_road"],
      "notes": "Planning only. Do not create Lua source in this draft."
    }
  ]
}
""";
    }

    private static string GetLogicModeInstruction(string logicMode)
    {
        return logicMode switch
        {
            "no-scripts" => "LogicMode no-scripts: do not output scriptPlans.",
            "data-plus-script-plan" => "LogicMode data-plus-script-plan: include scriptPlans for future hooks where useful, but no Lua code.",
            _ => "LogicMode data-only: express behavior through quests/dialogues/interactions only, no scriptPlans unless unavoidable."
        };
    }

    private static string EmptyAsDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static string? ExtractStrictJson(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal) || !trimmed.StartsWith("{", StringComparison.Ordinal) || !trimmed.EndsWith("}", StringComparison.Ordinal))
        {
            return null;
        }

        return trimmed;
    }

    private static ValidationReport ValidateDraft(FirstPlayableSliceDraft draft)
    {
        var report = new ValidationReport();
        RequireText(report, draft.Title, "draft.title.empty", "Title обязателен.", "title");
        RequireText(report, draft.StartMapId, "draft.start_map.empty", "StartMapId обязателен.", "startMapId");

        CheckCount(report, draft.TilePrototypes.Count, 3, 8, "draft.tiles.count", "TilePrototypes должно быть 3-8.", "tilePrototypes");
        CheckCount(report, draft.EntityPrototypes.Count, 1, 6, "draft.entity_prototypes.count", "EntityPrototypes должно быть 1-6.", "entityPrototypes");
        CheckCount(report, draft.Maps.Count, 1, 1, "draft.maps.count", "Для этой цели нужна ровно 1 карта.", "maps");
        CheckCount(report, draft.Dialogues.Count, 0, 3, "draft.dialogues.count", "Dialogues должно быть 0-3.", "dialogues");
        CheckCount(report, draft.Quests.Count, 0, 3, "draft.quests.count", "Quests должно быть 0-3.", "quests");

        var tileIds = CheckIds(report, draft.TilePrototypes.Select(tile => tile.Id), "tile");
        var entityPrototypeIds = CheckIds(report, draft.EntityPrototypes.Select(entity => entity.Id), "entity_prototype");
        CheckIds(report, draft.Maps.Select(map => map.Id), "map");
        CheckIds(report, draft.Dialogues.Select(dialogue => dialogue.Id), "dialogue");
        CheckIds(report, draft.Quests.Select(quest => quest.Id), "quest");

        var map = draft.Maps.FirstOrDefault();
        if (map == null)
        {
            return report;
        }

        if (!string.Equals(map.Id, draft.StartMapId, StringComparison.Ordinal))
        {
            report.Issues.Add(CreateIssue("draft.start_map.missing", "StartMapId должен ссылаться на единственную карту draft.", draft.StartMapId));
        }

        if (map.Width < 12 || map.Width > 40)
        {
            report.Issues.Add(CreateIssue("draft.map.width", "Ширина карты должна быть 12-40.", map.Id));
        }

        if (map.Height < 8 || map.Height > 30)
        {
            report.Issues.Add(CreateIssue("draft.map.height", "Высота карты должна быть 8-30.", map.Id));
        }

        if (!tileIds.Contains(map.DefaultTileId))
        {
            report.Issues.Add(CreateIssue("draft.map.default_tile", $"DefaultTileId не найден: {map.DefaultTileId}", map.Id));
        }

        var distinctMapTileIds = new HashSet<string>(StringComparer.Ordinal) { map.DefaultTileId };
        foreach (var tile in map.Tiles)
        {
            if (!tileIds.Contains(tile.TileId))
            {
                report.Issues.Add(CreateIssue("draft.map.tile_ref", $"TileId не найден: {tile.TileId}", map.Id));
            }

            if (tile.X < 0 || tile.Y < 0 || tile.X >= map.Width || tile.Y >= map.Height)
            {
                report.Issues.Add(CreateIssue("draft.map.tile_bounds", $"Tile override вне границ: {tile.X},{tile.Y}", map.Id));
            }

            distinctMapTileIds.Add(tile.TileId);
        }

        if (map.Tiles.Count == 0 || distinctMapTileIds.Count < 2)
        {
            report.Issues.Add(CreateIssue("draft.map.mixed_tiles", "Карта должна использовать DefaultTileId и tile overrides минимум с одним отличающимся tileId.", map.Id));
        }

        if (map.StartPosition.X < 0 || map.StartPosition.Y < 0 || map.StartPosition.X >= map.Width || map.StartPosition.Y >= map.Height)
        {
            report.Issues.Add(CreateIssue("draft.map.start_bounds", "StartPosition вне границ карты.", map.Id));
        }
        else if (!IsWalkableAtStart(draft.TilePrototypes, map))
        {
            report.Issues.Add(CreateIssue("draft.map.start_walkable", "StartPosition должен быть на walkable tile.", map.Id));
        }

        foreach (var entity in map.Entities)
        {
            if (!entityPrototypeIds.Contains(entity.PrototypeId))
            {
                report.Issues.Add(CreateIssue("draft.entity.prototype", $"PrototypeId не найден: {entity.PrototypeId}", entity.Id));
            }

            if (entity.Position.X < 0 || entity.Position.Y < 0 || entity.Position.X >= map.Width || entity.Position.Y >= map.Height)
            {
                report.Issues.Add(CreateIssue("draft.entity.bounds", $"Entity вне границ карты: {entity.Position.X},{entity.Position.Y}", entity.Id));
            }
        }

        CheckCount(report, map.Entities.Count, 0, 20, "draft.entities.count", "Entity instances должно быть 0-20.", map.Id);
        return report;
    }

    private static HashSet<string> CheckIds(ValidationReport report, IEnumerable<string> ids, string group)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var id in ids)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Issues.Add(CreateIssue($"draft.{group}.id.empty", "Id не должен быть пустым.", group));
                continue;
            }

            if (!SlashIdRegex().IsMatch(id))
            {
                report.Issues.Add(CreateIssue($"draft.{group}.id.format", $"Id должен быть lowercase slash id: {id}", id));
            }

            if (!set.Add(id))
            {
                report.Issues.Add(CreateIssue($"draft.{group}.id.duplicate", $"Дублирующийся id: {id}", id));
            }
        }

        return set;
    }

    private static bool IsWalkableAtStart(IReadOnlyList<TilePrototypeDefinition> tiles, MapDefinition map)
    {
        var startTileId = map.Tiles.LastOrDefault(tile => tile.X == map.StartPosition.X && tile.Y == map.StartPosition.Y)?.TileId
            ?? map.DefaultTileId;
        return tiles.FirstOrDefault(tile => string.Equals(tile.Id, startTileId, StringComparison.Ordinal))?.Walkable == true;
    }

    private static void RequireText(ValidationReport report, string? text, string code, string message, string targetId)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            report.Issues.Add(CreateIssue(code, message, targetId));
        }
    }

    private static void CheckCount(ValidationReport report, int count, int min, int max, string code, string message, string targetId)
    {
        if (count < min || count > max)
        {
            report.Issues.Add(CreateIssue(code, message, targetId));
        }
    }

    private static ValidationIssue CreateIssue(string code, string message, string? targetId)
    {
        return new ValidationIssue
        {
            Code = code,
            Severity = ValidationSeverity.Error,
            Message = message,
            TargetId = targetId,
            Category = Category
        };
    }

    private static GamePackageDefinition ClonePackage(GamePackageDefinition package)
    {
        var json = JsonSerializer.Serialize(package, JsonOptions);
        return JsonSerializer.Deserialize<GamePackageDefinition>(json, JsonOptions)
            ?? throw new InvalidOperationException("Не удалось создать копию текущего package.");
    }

    private static void ApplyToCopy(GamePackageDefinition package, FirstPlayableSliceDraft draft)
    {
        package.Manifest.Title = draft.Title.Trim();
        package.Manifest.Description = string.IsNullOrWhiteSpace(draft.Description) ? null : draft.Description.Trim();
        package.Manifest.StartMapId = draft.StartMapId.Trim();
        package.Manifest.FormatVersion = string.IsNullOrWhiteSpace(package.Manifest.FormatVersion) ? "0.1" : package.Manifest.FormatVersion;
        package.Game.TilePrototypes = draft.TilePrototypes;
        package.Game.EntityPrototypes = draft.EntityPrototypes;
        package.Game.Maps = draft.Maps;
        package.Game.Dialogues = draft.Dialogues;
        package.Game.Quests = draft.Quests;
    }

    [GeneratedRegex("^[a-z0-9]+(/[a-z0-9][a-z0-9_-]*)+$", RegexOptions.CultureInvariant)]
    private static partial Regex SlashIdRegex();
}
