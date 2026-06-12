using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Generation;

public interface IFirstPlayableSliceGenerator
{
    Task<GenerationResult> GenerateAsync(GenerationInterviewModel interview, CancellationToken cancellationToken);
    Task<GenerationResult> AnalyzeBriefAsync(GenerationInterviewModel interview, CancellationToken cancellationToken);
    Task<GenerationResult> TestConnectionAsync(CancellationToken cancellationToken);
    FirstPlayableSliceApplyResult ApplyDraft(FirstPlayableSliceDraft draft);
}

public sealed class GenerationInterviewModel
{
    public string GameIdea { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string CameraView { get; set; } = string.Empty;
    public string Setting { get; set; } = string.Empty;
    public string FirstLocation { get; set; } = string.Empty;
    public string FirstConflict { get; set; } = string.Empty;
    public string PlayerRole { get; set; } = string.Empty;
    public string RequiredNpc { get; set; } = string.Empty;
    public int MapWidth { get; set; } = 24;
    public int MapHeight { get; set; } = 16;
    public string GenerationMode { get; set; } = "first_playable_slice";
    public string LoreNotes { get; set; } = string.Empty;
    public string HardConstraints { get; set; } = string.Empty;
    public string MustInclude { get; set; } = string.Empty;
    public string MustAvoid { get; set; } = string.Empty;
    public string PlayerFantasy { get; set; } = string.Empty;
    public string GameplayLogicNotes { get; set; } = string.Empty;
    public string LogicMode { get; set; } = "data-only";
    public int MaxTileOverrides { get; set; } = 40;
    public int TargetNpcCount { get; set; } = 2;
    public int TargetEntityInstanceCount { get; set; } = 3;
    public int TargetQuestCount { get; set; } = 1;
    public int TargetDialogueCount { get; set; } = 1;
    public string DetailMode { get; set; } = "balanced";
}

public sealed class GenerationResult
{
    public bool Success { get; set; }
    public string RawContent { get; set; } = string.Empty;
    public string RawJson { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ProfileTitle { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public FirstPlayableSliceDraft? Draft { get; set; }
    public ValidationReport DraftValidationReport { get; set; } = new ValidationReport();
}

public sealed class FirstPlayableSliceApplyResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ValidationReport ValidationReport { get; set; } = new ValidationReport();
}

public sealed class FirstPlayableSliceDraft
{
    public string Title { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StartMapId { get; set; } = string.Empty;
    public List<TilePrototypeDefinition> TilePrototypes { get; set; } = new List<TilePrototypeDefinition>();
    public List<EntityPrototypeDefinition> EntityPrototypes { get; set; } = new List<EntityPrototypeDefinition>();
    public List<MapDefinition> Maps { get; set; } = new List<MapDefinition>();
    public List<DialogueDefinition> Dialogues { get; set; } = new List<DialogueDefinition>();
    public List<QuestDefinition> Quests { get; set; } = new List<QuestDefinition>();
    public string? LogicNotes { get; set; }
    public List<ScriptPlanModel> ScriptPlans { get; set; } = new List<ScriptPlanModel>();
}

public sealed class ScriptPlanModel
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string SuggestedEntryPoint { get; set; } = string.Empty;
    public List<string> RequiredCapabilities { get; set; } = new List<string>();
    public List<string> UsedBy { get; set; } = new List<string>();
    public string Notes { get; set; } = string.Empty;
}
