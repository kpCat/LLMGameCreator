namespace LLMGameCreator.Domain.Definitions;

public sealed class DialogueDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string StartNodeId { get; set; } = string.Empty;
    public string? BackgroundAssetId { get; set; }
    public List<DialogueNodeDefinition> Nodes { get; set; } = new List<DialogueNodeDefinition>();
}

public sealed class DialogueNodeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string SpeakerId { get; set; } = string.Empty;
    public string Expression { get; set; } = "neutral";
    public string Text { get; set; } = string.Empty;
    public List<DialogueChoiceDefinition> Choices { get; set; } = new List<DialogueChoiceDefinition>();
}

public sealed class DialogueChoiceDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? TargetNodeId { get; set; }
    public List<ConditionDefinition> Conditions { get; set; } = new List<ConditionDefinition>();
    public List<EffectDefinition> Effects { get; set; } = new List<EffectDefinition>();
}
