using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignEventPresenter
{
    public IReadOnlyList<string> Present(UnifiedRuntimeResult result) => result.MapEvents.Select(e => e.Message)
        .Concat(result.GameplayEvents.Select(e => e.Message)).Where(message => !string.IsNullOrWhiteSpace(message))
        .Concat(result.Diagnostics.Select(d => d.Message)).TakeLast(12).ToList();
}
