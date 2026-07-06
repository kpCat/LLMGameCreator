namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string GenericSystemsStatus { get; init; } = string.Empty;
    public string GenericSystemsSamplePackagePath { get; init; } = string.Empty;
    public string GenericSystemsPackageId { get; init; } = string.Empty;
    public bool GenericSystemsRecipePreviewPresent { get; init; }
    public bool GenericSystemsRecipeApplyPassed { get; init; }
    public bool GenericSystemsHarvestPreviewPresent { get; init; }
    public bool GenericSystemsHarvestApplyPassed { get; init; }
    public bool GenericSystemsTransactionPreviewPresent { get; init; }
    public bool GenericSystemsEncounterPreviewPresent { get; init; }
    public bool GenericSystemsCombatRoundPreviewPresent { get; init; }
    public bool GenericSystemsInventorySummaryPresent { get; init; }
    public bool GenericSystemsResourceSummaryPresent { get; init; }
    public bool GenericSystemsEventLogPresent { get; init; }
    public string GenericSystemsUnitySmokeStatus { get; init; } = string.Empty;
    public bool GenericSystemsCleanupScriptAvailable { get; init; }
    public string GenericSystemsCleanupCommand { get; init; } = string.Empty;
    public bool GenericSystemsGoal124StillGreen { get; init; }
    public bool GenericSystemsProjectionOnly { get; init; }
    public bool GenericSystemsSamplePackageReadOnly { get; init; }
    public string GenericSystemsEvidencePath { get; init; } = string.Empty;
    public string GenericSystemsExportPath { get; init; } = string.Empty;
    public bool GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary { get; init; }
    public bool GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysGenericGamePackageSystems { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool GenericGamePackageSystemsGroupPresent { get; init; }
    public string GenericSystemsStatus { get; init; } = string.Empty;
    public string GenericSystemsSamplePackagePath { get; init; } = string.Empty;
    public string GenericSystemsPackageId { get; init; } = string.Empty;
    public bool GenericSystemsRecipePreviewPresent { get; init; }
    public bool GenericSystemsRecipeApplyPassed { get; init; }
    public bool GenericSystemsHarvestPreviewPresent { get; init; }
    public bool GenericSystemsHarvestApplyPassed { get; init; }
    public bool GenericSystemsTransactionPreviewPresent { get; init; }
    public bool GenericSystemsEncounterPreviewPresent { get; init; }
    public bool GenericSystemsCombatRoundPreviewPresent { get; init; }
    public bool GenericSystemsInventorySummaryPresent { get; init; }
    public bool GenericSystemsResourceSummaryPresent { get; init; }
    public bool GenericSystemsEventLogPresent { get; init; }
    public string GenericSystemsUnitySmokeStatus { get; init; } = string.Empty;
    public bool GenericSystemsCleanupScriptAvailable { get; init; }
    public string GenericSystemsCleanupCommand { get; init; } = string.Empty;
    public bool GenericSystemsGoal124StillGreen { get; init; }
    public bool GenericSystemsProjectionOnly { get; init; }
    public bool GenericSystemsSamplePackageReadOnly { get; init; }
    public string GenericSystemsEvidencePath { get; init; } = string.Empty;
    public string GenericSystemsExportPath { get; init; } = string.Empty;
    public bool GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary { get; init; }
    public bool GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; }
    public bool GenericGamePackageSystemsQualityGatePassed { get; init; }
    public bool Goal125FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsGenericGamePackageSystemsBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string GenericSystemsStatus { get; init; } = string.Empty;
    public string GenericSystemsSamplePackagePath { get; init; } = string.Empty;
    public string GenericSystemsPackageId { get; init; } = string.Empty;
    public bool GenericSystemsRecipePreviewPresent { get; init; }
    public bool GenericSystemsRecipeApplyPassed { get; init; }
    public bool GenericSystemsHarvestPreviewPresent { get; init; }
    public bool GenericSystemsHarvestApplyPassed { get; init; }
    public bool GenericSystemsTransactionPreviewPresent { get; init; }
    public bool GenericSystemsEncounterPreviewPresent { get; init; }
    public bool GenericSystemsCombatRoundPreviewPresent { get; init; }
    public bool GenericSystemsInventorySummaryPresent { get; init; }
    public bool GenericSystemsResourceSummaryPresent { get; init; }
    public bool GenericSystemsEventLogPresent { get; init; }
    public string GenericSystemsUnitySmokeStatus { get; init; } = string.Empty;
    public bool GenericSystemsCleanupScriptAvailable { get; init; }
    public string GenericSystemsCleanupCommand { get; init; } = string.Empty;
    public bool GenericSystemsGoal124StillGreen { get; init; }
    public bool GenericSystemsProjectionOnly { get; init; }
    public bool GenericSystemsSamplePackageReadOnly { get; init; }
    public string GenericSystemsEvidencePath { get; init; } = string.Empty;
    public string GenericSystemsExportPath { get; init; } = string.Empty;
    public bool GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary { get; init; }
    public bool GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets { get; init; }
    public bool GenericGamePackageSystemsQualityGatePassed { get; init; }
    public bool Goal125FilesDiscoveredByRelativePaths { get; init; }
}
