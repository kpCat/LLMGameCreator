using System.Security.Cryptography;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignSessionTruthService
{
    private readonly ICurrentGamePackageService _currentProject;
    private readonly GeneratedGameplaySaveValidator _validator;
    private readonly IGameProjectOperationCoordinator _operations;

    public GeneratedCampaignSessionTruthService(ICurrentGamePackageService currentProject, GeneratedGameplaySaveValidator validator, IGameProjectOperationCoordinator operations)
    { _currentProject = currentProject; _validator = validator; _operations = operations; }

    public (GeneratedCampaignSessionStatus Status, GeneratedCampaignProjectTruth? Truth, IReadOnlyList<string> Diagnostics) Capture()
    {
        if (string.IsNullOrWhiteSpace(_currentProject.CurrentFolder)) return (GeneratedCampaignSessionStatus.NO_PROJECT, null, ["campaign.no_project"]);
        var folder = _currentProject.CurrentFolder;
        using var lease = _operations.TryAcquire(folder, GameProjectOperationKinds.GameplayLoad);
        if (!lease.Acquired) return (GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null, [lease.Diagnostic]);
        var captured = _validator.CaptureProjectTruth(folder, lease);
        if (!captured.Passed || captured.Truth is null)
        {
            var generated = captured.Diagnostics.Any(x => x == "generated_save.not_generated_project");
            return (generated ? GeneratedCampaignSessionStatus.PROJECT_NOT_GENERATED : GeneratedCampaignSessionStatus.PROJECT_NOT_READY, null,
                captured.Diagnostics.Select(x => x.Replace("generated_save.", "campaign.", StringComparison.Ordinal)).ToList());
        }
        var truth = captured.Truth;
        var source = truth.StrictGeneratedSource.Source!;
        return (GeneratedCampaignSessionStatus.READY, new GeneratedCampaignProjectTruth
        {
            ProjectFolder = truth.ProjectFolder,
            ProjectIdentityFingerprint = truth.IdentityFingerprint,
            WorldId = truth.WorldId,
            SourceRecordSha256 = Hash(Path.Combine(truth.ProjectFolder, SeededGeneratedProjectVocabulary.SourceRelativePath.Replace('/', Path.DirectorySeparatorChar))),
            SourceRequestSha256 = GameProjectSeedRegenerationDiffService.RequestSha256(source.GenerationRequest),
            PlanSha256 = source.PlanSha256,
            GeneratedBasePackageSha256 = source.GeneratedBasePackageSha256,
            PackageSha256 = truth.PackageSha256,
            CompositionPackageSha256 = truth.CompositionPackageSha256,
            FinalStateHash = truth.SelectedBuildHistorySha256,
            QualifiedAuthoringFingerprint = truth.QualifiedAuthoringFingerprint,
            SelectedBuildHistoryFileName = truth.SelectedBuildHistoryFileName,
            GeneratedStartMapId = truth.GeneratedStartMapId,
            RegionMapBindings = truth.GeneratedRegionMapBindings
        }, []);
    }

    public static bool Same(GeneratedCampaignProjectTruth left, GeneratedCampaignProjectTruth right) =>
        left.ProjectIdentityFingerprint == right.ProjectIdentityFingerprint && left.WorldId == right.WorldId
        && left.PackageSha256 == right.PackageSha256 && left.CompositionPackageSha256 == right.CompositionPackageSha256
        && left.QualifiedAuthoringFingerprint == right.QualifiedAuthoringFingerprint;

    private static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
}
