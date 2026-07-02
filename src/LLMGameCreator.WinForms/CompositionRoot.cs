using DryIoc;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;
using LLMGameCreator.Application.Editing;
using LLMGameCreator.Application.Generation;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.AssetPipeline;
using LLMGameCreator.Infrastructure.Generation;
using LLMGameCreator.Infrastructure.Logging;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Scripting;
using LLMGameCreator.WinForms.Pages;
using LLMGameCreator.WinForms.Pages.CompositionWorkbench;
using LLMGameCreator.WinForms.Pages.UnityArchiveReview;
using Microsoft.Extensions.Logging;

namespace LLMGameCreator.WinForms;

public sealed class CompositionRoot : IDisposable
{
    private readonly Container _container;
    private readonly ILoggerFactory _loggerFactory;

    public CompositionRoot()
    {
        _container = new Container();

        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator");
        var logsPath = Path.Combine(appData, "Logs");
        var logFilePath = Path.Combine(logsPath, "LLMGameCreator.log");
        var settingsPath = Path.Combine(appData, "appsettings.json");

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new SimpleFileLoggerProvider(logFilePath));
        });

        _container.RegisterDelegate<ILoggerFactory>(_ => _loggerFactory, Reuse.Singleton);
        _container.RegisterDelegate<IAppSettingsRepository>(_ => new JsonAppSettingsRepository(settingsPath), Reuse.Singleton);
        _container.Register<IGamePackageRepository, JsonGamePackageRepository>(Reuse.Singleton);
        _container.Register<SqliteDesignDatabase>(Reuse.Singleton);
        _container.RegisterDelegate<IDesignDatabaseInitializer>(resolver => resolver.Resolve<SqliteDesignDatabase>(), Reuse.Singleton);
        _container.RegisterDelegate<IDesignKnowledgeRepository>(resolver => resolver.Resolve<SqliteDesignDatabase>(), Reuse.Singleton);
        _container.RegisterDelegate<IGeneratorLibraryRegistry>(resolver => resolver.Resolve<SqliteDesignDatabase>(), Reuse.Singleton);
        _container.RegisterDelegate<IGeneratorPlanRepository>(resolver => resolver.Resolve<SqliteDesignDatabase>(), Reuse.Singleton);
        _container.RegisterDelegate<IGeneratedArtifactRepository>(resolver => resolver.Resolve<SqliteDesignDatabase>(), Reuse.Singleton);
        _container.Register<IGeneratorLibraryImporter, GeneratorLibraryImportService>(Reuse.Singleton);
        _container.Register<IGeneratorLibraryIntegrityValidator, GeneratorLibraryIntegrityValidator>(Reuse.Singleton);
        _container.Register<GamePackagePatchOperationValidator>(Reuse.Singleton);
        _container.Register<GeneratorPlanValidator>(Reuse.Singleton);
        _container.Register<IGeneratorPlanDraftService, GeneratorPlanDraftService>(Reuse.Singleton);
        _container.Register<IGeneratorPlanReviewService, GeneratorPlanReviewService>(Reuse.Singleton);
        _container.Register<IGeneratorPlanPreviewService, LLMGameCreator.Application.Design.GeneratorPlanPreviewService>(Reuse.Singleton);
        _container.Register<IGamePackagePatchService, GamePackagePatchService>(Reuse.Singleton);
        _container.Register<IGeneratorPlanPipelineService, GeneratorPlanPipelineService>(Reuse.Singleton);
        _container.Register<PrototypeLuaStaticAnalyzer>(Reuse.Singleton);
        _container.Register<PrototypeLuaDeclarationMapper>(Reuse.Singleton);
        _container.RegisterDelegate<IPrototypeLuaExecutor>(resolver => new PrototypeLuaExecutor(
            resolver.Resolve<PrototypeLuaStaticAnalyzer>()), Reuse.Singleton);
        _container.Register<IPrototypeLuaPatchArtifactService, PrototypeLuaPatchArtifactService>(Reuse.Singleton);
        _container.Register<ICurrentGamePackageService, CurrentGamePackageService>(Reuse.Singleton);
        _container.Register<AssembledGamePackageActivationService>(Reuse.Singleton);
        _container.Register<ContentLanguagePolicyService>(Reuse.Singleton);
        _container.Register<NewGamePackageFactory>(Reuse.Singleton);
        _container.Register<IGameProjectService, GameProjectService>(Reuse.Singleton);
        _container.Register<IGamePackageValidator, GamePackageValidator>(Reuse.Singleton);
        _container.Register<GameBlueprintPresetProvider>(Reuse.Singleton);
        _container.RegisterDelegate<CapabilityRegistry>(_ => BuiltInCapabilityRegistry.Create(), Reuse.Singleton);
        _container.RegisterDelegate<GeneratorCatalog>(_ => BuiltInGeneratorCatalog.Create(), Reuse.Singleton);
        _container.Register<GameBlueprintCompositionValidator>(Reuse.Singleton);
        _container.Register<GeneratorCatalogValidator>(Reuse.Singleton);
        _container.Register<GeneratorPlanResolver>(Reuse.Singleton);
        _container.Register<GameCompositionDiagnosticsService>(Reuse.Singleton);
        _container.Register<GameCompositionDiagnosticsMarkdownRenderer>(Reuse.Singleton);
        _container.Register<GameCompositionDiagnosticsExportService>(Reuse.Singleton);
        _container.Register<CompositionWorkbenchPresenter>(Reuse.Singleton);
        _container.Register<UnityArchiveManualImportTemplateService>(Reuse.Singleton);
        _container.Register<UnityArchiveManualProviderImportMarkdownRenderer>(Reuse.Singleton);
        _container.Register<UnityArchiveManualProviderImportService>(Reuse.Singleton);
        _container.Register<UnityArchiveReviewPresenter>(Reuse.Singleton);
        _container.Register<SchemaDrivenCampaignWorkspaceEvidenceService>(Reuse.Singleton);
        _container.Register<SchemaDrivenCampaignEditEvidenceService>(Reuse.Singleton);
        _container.Register<EditDrivenPlayablePreviewRefreshEvidenceService>(Reuse.Singleton);
        _container.RegisterDelegate<GeneratorPlanDraftArtifactProductionService>(_ => new GeneratorPlanDraftArtifactProductionService(), Reuse.Singleton);
        _container.Register<GeneratorPlanDraftArtifactApprovalValidator>(Reuse.Singleton);
        _container.Register<GeneratorPlanDraftArtifactApprovalMarkdownRenderer>(Reuse.Singleton);
        _container.RegisterDelegate<GeneratorPlanDraftArtifactApprovalService>(resolver => new GeneratorPlanDraftArtifactApprovalService(
            resolver.Resolve<GeneratorPlanDraftArtifactProductionService>(),
            resolver.Resolve<GeneratorPlanDraftArtifactApprovalValidator>(),
            resolver.Resolve<GeneratorPlanDraftArtifactApprovalMarkdownRenderer>()), Reuse.Singleton);
        _container.Register<GeneratorPlanDraftArtifactApprovalArtifactService>(Reuse.Singleton);
        _container.Register<GeneratorPlanDraftArtifactApprovalArtifactReader>(Reuse.Singleton);
        _container.Register<GeneratorPlanDraftArtifactReviewService>(Reuse.Singleton);
        _container.Register<GeneratorPlanGamePackageAssembler>(Reuse.Singleton);
        _container.Register<GeneratorPlanGamePackageAssemblyValidator>(Reuse.Singleton);
        _container.Register<GeneratorPlanGamePackageAssemblyMarkdownRenderer>(Reuse.Singleton);
        _container.Register<GeneratorPlanApprovedArtifactSetReader>(Reuse.Singleton);
        _container.RegisterDelegate<GeneratorPlanGamePackageAssemblyService>(resolver => new GeneratorPlanGamePackageAssemblyService(
            resolver.Resolve<GeneratorPlanGamePackageAssembler>(),
            resolver.Resolve<IGamePackageValidator>(),
            resolver.Resolve<GeneratorPlanGamePackageAssemblyValidator>(),
            resolver.Resolve<GeneratorPlanGamePackageAssemblyMarkdownRenderer>(),
            resolver.Resolve<IGamePackageRepository>(),
            resolver.Resolve<GeneratorPlanDraftArtifactApprovalArtifactReader>(),
            resolver.Resolve<GeneratorPlanApprovedArtifactSetReader>()), Reuse.Singleton);
        _container.Register<GeneratorPlanGamePackageAssemblyArtifactService>(Reuse.Singleton);
        _container.Register<GeneratorPlanPackageExportRunMarkdownRenderer>(Reuse.Singleton);
        _container.Register<GeneratorPlanPackageExportRunService>(Reuse.Singleton);
        _container.Register<GeneratorPlanPackageExportRunArtifactReader>(Reuse.Singleton);
        _container.Register<GeneratorPlanExampleTemplateCatalog>(Reuse.Singleton);
        _container.Register<GeneratorPlanExampleTemplateService>(Reuse.Singleton);
        _container.Register<GeneratorPlanCapabilitySelectionAtlasReader>(Reuse.Singleton);
        _container.Register<GeneratorPlanCapabilitySelectionService>(Reuse.Singleton);
        _container.Register<GeneratorPlanCapabilitySelectionArtifactService>(Reuse.Singleton);
        _container.Register<GeneratorPlanCapabilitySelectionArtifactReader>(Reuse.Singleton);
        _container.Register<GeneratorPlanStrictLlmArtifactContractCatalog>(Reuse.Singleton);
        _container.Register<ContentLanguagePromptInstructionProvider>(Reuse.Singleton);
        _container.Register<ContentLanguageDiagnosticService>(Reuse.Singleton);
        _container.RegisterDelegate<GeneratorPlanStrictLlmArtifactPromptBuilder>(resolver =>
            new GeneratorPlanStrictLlmArtifactPromptBuilder(
                resolver.Resolve<ContentLanguagePromptInstructionProvider>()), Reuse.Singleton);
        _container.Register<GeneratorPlanStrictJsonResponseParser>(Reuse.Singleton);
        _container.Register<GeneratorPlanStrictLlmArtifactValidator>(Reuse.Singleton);
        _container.RegisterDelegate<GeneratorPlanStrictLlmArtifactRepairPromptBuilder>(resolver =>
            new GeneratorPlanStrictLlmArtifactRepairPromptBuilder(
                resolver.Resolve<ContentLanguagePromptInstructionProvider>()), Reuse.Singleton);
        _container.Register<GeneratorPlanStrictLlmArtifactGenerationArtifactService>(Reuse.Singleton);
        _container.Register<GeneratorPlanStrictLlmArtifactGenerationArtifactReader>(Reuse.Singleton);
        _container.Register<GeneratorPlanStrictLlmArtifactGenerationService>(Reuse.Singleton);
        _container.Register<GeneratorPlanStrictLlmEvaluationMarkdownRenderer>(Reuse.Singleton);
        _container.Register<GeneratorPlanStrictLlmEvaluationArtifactService>(Reuse.Singleton);
        _container.Register<GeneratorPlanStrictLlmEvaluationArtifactReader>(Reuse.Singleton);
        _container.Register<GeneratorPlanStrictLlmEvaluationService>(Reuse.Singleton);
        _container.Register<IPackageEditorService, PackageEditorService>(Reuse.Singleton);
        _container.RegisterDelegate<ILlmChatClient>(_ => new OpenAiCompatibleLlmChatClient(), Reuse.Singleton);
        _container.Register<IFirstPlayableSliceGenerator, FirstPlayableSliceGenerator>(Reuse.Singleton);
        _container.Register<GenerationPresetOptionsService>(Reuse.Singleton);
        _container.RegisterDelegate<IVisibleGeneratedPlayableRuntimeAdapter>(resolver => new GeneratedPlayableRuntimePreviewAdapter(
            resolver.Resolve<IGameRuntime>()), Reuse.Singleton);
        _container.RegisterDelegate<VisibleGeneratedPlayablePreviewService>(resolver => new VisibleGeneratedPlayablePreviewService(
            generationOptionsService: resolver.Resolve<GenerationPresetOptionsService>(),
            runtimeAdapter: resolver.Resolve<IVisibleGeneratedPlayableRuntimeAdapter>()), Reuse.Singleton);
        _container.Register<GeneratedMicrogameGoalPreviewService>(Reuse.Singleton);
        _container.Register<GeneratedMicrogameChallengePreviewService>(Reuse.Singleton);
        _container.RegisterDelegate<RuntimeBackedMicrogameStateAcceptanceService>(resolver => new RuntimeBackedMicrogameStateAcceptanceService(
            resolver.Resolve<IRuntimeStateSerializer>(),
            resolver.Resolve<IRuntimeSnapshotStore>()), Reuse.Singleton);
        _container.RegisterDelegate<OneClickGeneratedPreviewWorkflowService>(resolver => new OneClickGeneratedPreviewWorkflowService(
            visiblePreviewService: resolver.Resolve<VisibleGeneratedPlayablePreviewService>(),
            runtimeBackedStateAcceptanceService: resolver.Resolve<RuntimeBackedMicrogameStateAcceptanceService>(),
            generationOptionsService: resolver.Resolve<GenerationPresetOptionsService>(),
            currentGamePackageService: resolver.Resolve<ICurrentGamePackageService>()), Reuse.Singleton);
        _container.Register<GeneratedPackageRuntimePreviewService>(Reuse.Singleton);
        _container.Register<GeneratedContentInteractionPreviewService>(Reuse.Singleton);
        _container.Register<GeneratedQuestDialoguePreviewService>(Reuse.Singleton);
        _container.Register<GeneratedMapPlacementPreviewService>(Reuse.Singleton);
        _container.Register<IGameRuntime, DefaultGameRuntime>(Reuse.Singleton);
        _container.Register<IGameRuntimeStateFactory, GameRuntimeStateFactory>(Reuse.Singleton);
        _container.Register<IRequirementEvaluator, RequirementEvaluator>(Reuse.Singleton);
        _container.Register<ICostConsumer, CostConsumer>(Reuse.Singleton);
        _container.Register<IOutputApplier, OutputApplier>(Reuse.Singleton);
        _container.Register<IRecipeRuntimeService, RecipeRuntimeService>(Reuse.Singleton);
        _container.Register<ILootRuntimeService, LootRuntimeService>(Reuse.Singleton);
        _container.Register<ITransactionRuntimeService, TransactionRuntimeService>(Reuse.Singleton);
        _container.Register<IResourceNetworkRuntimeService, ResourceNetworkRuntimeService>(Reuse.Singleton);
        _container.Register<IEquipmentRuntimeService, EquipmentRuntimeService>(Reuse.Singleton);
        _container.Register<IContainerRuntimeService, ContainerRuntimeService>(Reuse.Singleton);
        _container.Register<IHarvestRuntimeService, HarvestRuntimeService>(Reuse.Singleton);
        _container.Register<IEncounterRuntimeService, EncounterRuntimeService>(Reuse.Singleton);
        _container.Register<IEncounterAiService, EncounterAiService>(Reuse.Singleton);
        _container.Register<IFactionRuntimeService, FactionRuntimeService>(Reuse.Singleton);
        _container.Register<IQuestRuntimeService, QuestRuntimeService>(Reuse.Singleton);
        _container.Register<IDialogueRuntimeService, DialogueRuntimeService>(Reuse.Singleton);
        _container.Register<IQuestObjectiveTracker, QuestObjectiveTracker>(Reuse.Singleton);
        _container.Register<IUseItemRuntimeService, UseItemRuntimeService>(Reuse.Singleton);
        _container.Register<IInteractionRuntimeService, InteractionRuntimeService>(Reuse.Singleton);
        _container.Register<IGameRuntimeService, GameRuntimeService>(Reuse.Singleton);
        _container.Register<IUnifiedGameRuntimeService, UnifiedGameRuntimeService>(Reuse.Singleton);
        _container.Register<IRuntimeStateSerializer, RuntimeStateSerializer>(Reuse.Singleton);
        _container.Register<IRuntimeSnapshotStore, RuntimeSnapshotStore>(Reuse.Singleton);
        _container.Register<IScriptEngine, NullScriptEngine>(Reuse.Singleton);
        _container.Register<IAssetGenerationProvider, NullAssetGenerationProvider>(Reuse.Singleton);

        _container.RegisterDelegate<DashboardPageControl>(resolver => new DashboardPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IPackageEditorService>()), Reuse.Singleton);

        _container.RegisterDelegate<ProjectsPageControl>(resolver => new ProjectsPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IAppSettingsRepository>(),
            resolver.Resolve<IGameProjectService>(),
            resolver.Resolve<IGamePackageValidator>()), Reuse.Singleton);

        _container.RegisterDelegate<GenerationPageControl>(resolver => new GenerationPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IFirstPlayableSliceGenerator>(),
            resolver.Resolve<IGamePackageValidator>(),
            resolver.Resolve<IAppSettingsRepository>()), Reuse.Singleton);

        _container.RegisterDelegate<ValidationPageControl>(resolver => new ValidationPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IGamePackageValidator>()), Reuse.Singleton);

        _container.RegisterDelegate<RuntimePreviewPageControl>(resolver => new RuntimePreviewPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IGameRuntime>(),
            resolver.Resolve<OneClickGeneratedPreviewWorkflowService>(),
            resolver.Resolve<GeneratedPackageRuntimePreviewService>(),
            resolver.Resolve<GeneratedContentInteractionPreviewService>(),
            resolver.Resolve<GeneratedQuestDialoguePreviewService>(),
            resolver.Resolve<GeneratedMicrogameGoalPreviewService>(),
            resolver.Resolve<GeneratedMicrogameChallengePreviewService>(),
            resolver.Resolve<GeneratedMapPlacementPreviewService>(),
            resolver.Resolve<GenerationPresetOptionsService>()), Reuse.Singleton);

        _container.RegisterDelegate<RuntimeSimulatorPageControl>(resolver => new RuntimeSimulatorPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IGameRuntimeService>(),
            resolver.Resolve<IUnifiedGameRuntimeService>(),
            resolver.Resolve<IRuntimeStateSerializer>(),
            resolver.Resolve<IRuntimeSnapshotStore>()), Reuse.Singleton);

        _container.RegisterDelegate<GeneratorLibraryPageControl>(resolver => new GeneratorLibraryPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IDesignDatabaseInitializer>(),
            resolver.Resolve<IGeneratorLibraryImporter>(),
            resolver.Resolve<IGeneratorLibraryRegistry>(),
            resolver.Resolve<IGeneratorLibraryIntegrityValidator>(),
            resolver.Resolve<IGeneratorPlanDraftService>(),
            resolver.Resolve<IGeneratorPlanRepository>(),
            resolver.Resolve<IGeneratorPlanReviewService>(),
            resolver.Resolve<IGeneratorPlanPreviewService>(),
            resolver.Resolve<IGeneratorPlanPipelineService>(),
            resolver.Resolve<IGeneratedArtifactRepository>(),
            resolver.Resolve<IGamePackagePatchService>(),
            resolver.Resolve<IPrototypeLuaExecutor>(),
            resolver.Resolve<IPrototypeLuaPatchArtifactService>()), Reuse.Singleton);

        _container.RegisterDelegate<PackageExportPageControl>(resolver => new PackageExportPageControl(
            resolver.Resolve<GeneratorPlanPackageExportRunService>(),
            resolver.Resolve<GeneratorPlanPackageExportRunArtifactReader>(),
            resolver.Resolve<IDesignDatabaseInitializer>(),
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<GeneratorPlanExampleTemplateService>()), Reuse.Singleton);

        _container.RegisterDelegate<ArtifactReviewPageControl>(resolver => new ArtifactReviewPageControl(
            resolver.Resolve<GeneratorPlanDraftArtifactReviewService>(),
            resolver.Resolve<GeneratorPlanGamePackageAssemblyService>(),
            resolver.Resolve<GeneratorPlanGamePackageAssemblyArtifactService>(),
            resolver.Resolve<AssembledGamePackageActivationService>(),
            resolver.Resolve<IDesignDatabaseInitializer>(),
            resolver.Resolve<ICurrentGamePackageService>()), Reuse.Singleton);

        _container.RegisterDelegate<CapabilityPickerPageControl>(resolver => new CapabilityPickerPageControl(
            resolver.Resolve<GeneratorPlanCapabilitySelectionService>(),
            resolver.Resolve<GeneratorPlanCapabilitySelectionArtifactService>(),
            resolver.Resolve<GeneratorPlanCapabilitySelectionArtifactReader>(),
            resolver.Resolve<IDesignDatabaseInitializer>(),
            resolver.Resolve<ICurrentGamePackageService>()), Reuse.Singleton);

        _container.RegisterDelegate<StrictLlmArtifactsPageControl>(resolver => new StrictLlmArtifactsPageControl(
            resolver.Resolve<IAppSettingsRepository>(),
            resolver.Resolve<GeneratorPlanCapabilitySelectionArtifactReader>(),
            resolver.Resolve<GeneratorPlanStrictLlmArtifactContractCatalog>(),
            resolver.Resolve<GeneratorPlanStrictLlmArtifactGenerationService>(),
            resolver.Resolve<GeneratorPlanStrictLlmArtifactGenerationArtifactReader>(),
            resolver.Resolve<IDesignDatabaseInitializer>(),
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<ContentLanguagePolicyService>()), Reuse.Singleton);

        _container.RegisterDelegate<StrictLlmEvaluationPageControl>(resolver => new StrictLlmEvaluationPageControl(
            resolver.Resolve<IAppSettingsRepository>(),
            resolver.Resolve<GeneratorPlanStrictLlmArtifactContractCatalog>(),
            resolver.Resolve<GeneratorPlanStrictLlmArtifactGenerationArtifactReader>(),
            resolver.Resolve<GeneratorPlanStrictLlmEvaluationService>(),
            resolver.Resolve<GeneratorPlanStrictLlmEvaluationArtifactReader>(),
            resolver.Resolve<IDesignDatabaseInitializer>(),
            resolver.Resolve<ICurrentGamePackageService>()), Reuse.Singleton);

        _container.RegisterDelegate<CompositionWorkbenchPageControl>(resolver => new CompositionWorkbenchPageControl(
            resolver.Resolve<CompositionWorkbenchPresenter>(),
            resolver.Resolve<ICurrentGamePackageService>()), Reuse.Singleton);

        _container.RegisterDelegate<UnityArchiveReviewPageControl>(resolver => new UnityArchiveReviewPageControl(
            resolver.Resolve<UnityArchiveReviewPresenter>(),
            resolver.Resolve<ICurrentGamePackageService>()), Reuse.Singleton);

        _container.RegisterDelegate<CampaignAuthoringReviewWorkspacePageControl>(resolver =>
            new CampaignAuthoringReviewWorkspacePageControl(
                resolver.Resolve<SchemaDrivenCampaignWorkspaceEvidenceService>(),
                resolver.Resolve<SchemaDrivenCampaignEditEvidenceService>(),
                resolver.Resolve<EditDrivenPlayablePreviewRefreshEvidenceService>()), Reuse.Singleton);

        _container.RegisterDelegate<AssetsPageControl>(resolver => new AssetsPageControl(
            resolver.Resolve<ICurrentGamePackageService>()), Reuse.Singleton);

        _container.RegisterDelegate<SettingsPageControl>(resolver => new SettingsPageControl(
            resolver.Resolve<IAppSettingsRepository>()), Reuse.Singleton);

        _container.RegisterDelegate<IEditorPageRegistry>(resolver => new EditorPageRegistry(new IEditorPage[]
        {
            resolver.Resolve<DashboardPageControl>(),
            resolver.Resolve<ProjectsPageControl>(),
            resolver.Resolve<GenerationPageControl>(),
            resolver.Resolve<PackageExportPageControl>(),
            resolver.Resolve<CapabilityPickerPageControl>(),
            resolver.Resolve<StrictLlmArtifactsPageControl>(),
            resolver.Resolve<StrictLlmEvaluationPageControl>(),
            resolver.Resolve<CampaignAuthoringReviewWorkspacePageControl>(),
            resolver.Resolve<ArtifactReviewPageControl>(),
            resolver.Resolve<CompositionWorkbenchPageControl>(),
            resolver.Resolve<ValidationPageControl>(),
            resolver.Resolve<UnityArchiveReviewPageControl>(),
            resolver.Resolve<GeneratorLibraryPageControl>(),
            resolver.Resolve<RuntimePreviewPageControl>(),
            resolver.Resolve<RuntimeSimulatorPageControl>(),
            resolver.Resolve<AssetsPageControl>(),
            resolver.Resolve<SettingsPageControl>()
        }), Reuse.Singleton);

        _container.RegisterDelegate<MainForm>(resolver => new MainForm(
    resolver.Resolve<IEditorPageRegistry>(),
    resolver.Resolve<ICurrentGamePackageService>(),
    resolver.Resolve<ILoggerFactory>()), Reuse.Singleton);
    }

    public MainForm ResolveMainForm() => _container.Resolve<MainForm>();

    public IEditorPageRegistry ResolveEditorPageRegistry() => _container.Resolve<IEditorPageRegistry>();

    public void Dispose()
    {
        _container.Dispose();
        _loggerFactory.Dispose();
    }
}
