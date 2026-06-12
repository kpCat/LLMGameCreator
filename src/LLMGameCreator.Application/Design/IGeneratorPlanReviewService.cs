namespace LLMGameCreator.Application.Design;

public interface IGeneratorPlanReviewService
{
    Task<GeneratorPlanReviewResult> RevalidatePlanAsync(string planId, CancellationToken cancellationToken);
    Task<GeneratorPlanStatusUpdateResult> ApprovePlanAsync(string planId, string? note, CancellationToken cancellationToken);
    Task<GeneratorPlanStatusUpdateResult> RejectPlanAsync(string planId, string? note, CancellationToken cancellationToken);
    Task<GeneratorPlanStatusUpdateResult> ArchivePlanAsync(string planId, string? note, CancellationToken cancellationToken);
}
