namespace LLMGameCreator.Application.Design;

public interface IDesignDatabaseInitializer
{
    Task InitializeAsync(string databasePath, CancellationToken cancellationToken);
    Task<DesignDatabaseInfo> GetInfoAsync(CancellationToken cancellationToken);
}

public interface IDesignKnowledgeRepository
{
    Task UpsertKnowledgeItemAsync(DesignKnowledgeItem item, CancellationToken cancellationToken);
    Task<IReadOnlyList<DesignKnowledgeItem>> ListKnowledgeItemsAsync(CancellationToken cancellationToken);
    Task UpsertDecisionAsync(DesignDecision decision, CancellationToken cancellationToken);
    Task<IReadOnlyList<DesignDecision>> ListDecisionsAsync(CancellationToken cancellationToken);
    Task UpsertConstraintAsync(DesignConstraint constraint, CancellationToken cancellationToken);
    Task<IReadOnlyList<DesignConstraint>> ListConstraintsAsync(CancellationToken cancellationToken);
}

public interface IGeneratorLibraryRegistry
{
    Task SaveImportedLibraryAsync(GeneratorLibraryImportData data, CancellationToken cancellationToken);
    Task<IReadOnlyList<CapabilityModuleRecord>> ListCapabilitiesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<GeneratorModuleRecord>> ListModulesAsync(CancellationToken cancellationToken);
    Task<GeneratorModuleRecord?> GetModuleByIdAsync(string moduleId, CancellationToken cancellationToken);
    Task<IReadOnlyList<GeneratorModuleRecord>> ListModulesByCapabilityAsync(string capabilityId, CancellationToken cancellationToken);
    Task<IReadOnlyList<GeneratorLibraryImportIssue>> ListImportIssuesAsync(CancellationToken cancellationToken);
}

public interface IGeneratorLibraryImporter
{
    Task<GeneratorLibraryImportReport> ImportGeneratorLibraryAsync(string repositoryRootOrLibraryRoot, CancellationToken cancellationToken);
}

public interface IGeneratorPlanRepository
{
    Task SaveGeneratorPlanAsync(
        GeneratorPlanRecord plan,
        IReadOnlyList<GeneratorPlanStepRecord> steps,
        PromptContextPackRecord? contextPack,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<GeneratorPlanRecord>> ListGeneratorPlansAsync(CancellationToken cancellationToken);
    Task<GeneratorPlanRecord?> GetGeneratorPlanByIdAsync(string planId, CancellationToken cancellationToken);
    Task<IReadOnlyList<GeneratorPlanStepRecord>> GetGeneratorPlanStepsAsync(string planId, CancellationToken cancellationToken);
    Task<bool> UpdateGeneratorPlanStatusAsync(string planId, string status, string? note, CancellationToken cancellationToken);
}

public interface IGeneratedArtifactRepository
{
    Task SaveGeneratedArtifactAsync(GeneratedArtifactRecord artifact, CancellationToken cancellationToken);
    Task<IReadOnlyList<GeneratedArtifactRecord>> ListGeneratedArtifactsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<GeneratedArtifactRecord>> ListGeneratedArtifactsByPlanAsync(string planId, CancellationToken cancellationToken);
    Task<GeneratedArtifactRecord?> GetGeneratedArtifactByIdAsync(string artifactId, CancellationToken cancellationToken);
    Task SaveValidationResultsAsync(string artifactId, IReadOnlyList<GeneratedArtifactValidationResultRecord> results, CancellationToken cancellationToken);
    Task<IReadOnlyList<GeneratedArtifactValidationResultRecord>> ListValidationResultsByArtifactAsync(string artifactId, CancellationToken cancellationToken);
}
