namespace LLMGameCreator.Domain.Definitions;

public sealed class DialogueDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string StartNodeId { get; set; } = string.Empty;
    public string? BackgroundAssetId { get; set; }
    public List<RequirementDefinition> Conditions { get; set; } = new List<RequirementDefinition>();
    public List<OutputDefinition> EnterEffects { get; set; } = new List<OutputDefinition>();
    public List<OutputDefinition> ExitEffects { get; set; } = new List<OutputDefinition>();
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public List<DialogueNodeDefinition> Nodes { get; set; } = new List<DialogueNodeDefinition>();
}

public sealed class DialogueNodeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string SpeakerId { get; set; } = string.Empty;
    public string Expression { get; set; } = "neutral";
    public string Text { get; set; } = string.Empty;
    public List<RequirementDefinition> Conditions { get; set; } = new List<RequirementDefinition>();
    public List<OutputDefinition> EnterEffects { get; set; } = new List<OutputDefinition>();
    public List<OutputDefinition> ExitEffects { get; set; } = new List<OutputDefinition>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public List<DialogueChoiceDefinition> Choices { get; set; } = new List<DialogueChoiceDefinition>();
}

public sealed class DialogueChoiceDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? TargetNodeId { get; set; }
    public List<ConditionDefinition> Conditions { get; set; } = new List<ConditionDefinition>();
    public List<RequirementDefinition> Requirements { get; set; } = new List<RequirementDefinition>();
    public List<CostDefinition> Costs { get; set; } = new List<CostDefinition>();
    public List<EffectDefinition> Effects { get; set; } = new List<EffectDefinition>();
    public List<OutputDefinition> Rewards { get; set; } = new List<OutputDefinition>();
    public bool CloseDialogue { get; set; }
    public string? StartQuestId { get; set; }
    public string? AdvanceQuestId { get; set; }
    public string? SetQuestStageId { get; set; }
    public string? OpenTransactionId { get; set; }
    public string? StartEncounterId { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
