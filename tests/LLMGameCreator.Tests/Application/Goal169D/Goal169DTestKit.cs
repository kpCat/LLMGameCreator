using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal164;

namespace LLMGameCreator.Tests.Application.Goal169D;

internal static class Goal169DTestKit
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Lazy<Goal169DRawState> RawFixture =
        new(CreateRaw);
    private static readonly Lazy<Goal169DQualifiedPortableState>
        QualifiedPortableFixture = new(CreateQualifiedPortable);

    internal static Goal169DRawState Raw => RawFixture.Value;
    internal static Goal169DQualifiedPortableState State =>
        QualifiedPortableFixture.Value;

    internal static string FileSha(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream))
            .ToLowerInvariant();
    }

    internal static string TreeSha(string path)
    {
        if (!Directory.Exists(path))
            return "<absent>";

        var builder = new StringBuilder();
        foreach (var file in Directory.EnumerateFiles(
                     path, "*", SearchOption.AllDirectories)
                 .OrderBy(item => item, StringComparer.OrdinalIgnoreCase))
        {
            builder.Append(Path.GetRelativePath(path, file)
                    .Replace('\\', '/'))
                .Append('|')
                .Append(FileSha(file))
                .AppendLine();
        }

        return Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(builder.ToString())))
            .ToLowerInvariant();
    }

    internal static IReadOnlyList<string> BuildHistoryFiles(
        string project) =>
        Directory.Exists(BuildHistoryRoot(project))
            ? Directory.EnumerateFiles(
                    BuildHistoryRoot(project),
                    "*.json",
                    SearchOption.TopDirectoryOnly)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList()
            : [];

    internal static string BuildHistoryRoot(string project) =>
        Path.Combine(
            project,
            UnifiedGameProjectWorkspaceVocabulary
                .BuildHistoryRelativeRoot.Replace(
                    '/', Path.DirectorySeparatorChar));

    internal static string AuthoringRoot(string project) =>
        Path.Combine(project, ".llmgc", "authoring");

    internal static string GenerationRoot(string project) =>
        Path.Combine(
            project,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));

    private static Goal169DRawState CreateRaw()
    {
        var project = Goal156TestKit.CoreOnly;
        var packagePath = Path.Combine(project.Path, "package.json");
        var sourcePath = Path.Combine(
            project.Path,
            SeededGeneratedProjectVocabulary.SourceRelativePath
                .Replace('/', Path.DirectorySeparatorChar));
        var packageHash = FileSha(packagePath);
        var sourceHash = FileSha(sourcePath);
        var authoringHash = TreeSha(AuthoringRoot(project.Path));
        var generationHash = TreeSha(GenerationRoot(project.Path));
        var source = Goal156TestKit.SourceService.Validate(project.Path);
        var authoring = Goal156TestKit.Authoring(project.Path);
        var snapshot = Goal156TestKit.OpenWorkspace(project.Path)
            .Snapshot();
        return new Goal169DRawState(
            project,
            Goal156TestKit.Load(project.Path),
            source,
            authoring,
            snapshot,
            packageHash,
            sourceHash,
            authoringHash,
            generationHash,
            BuildHistoryFiles(project.Path).Count,
            "CREATION_ONLY_NOT_QUALIFIED");
    }

    private static Goal169DQualifiedPortableState
        CreateQualifiedPortable()
    {
        var raw = Raw;
        var retainedBefore = Goal169CRetainedPublication.Capture();
        var hostRoot = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            ProjectStandaloneBuildVocabulary.HostCacheRootName);
        var hostBefore = TreeSha(hostRoot);

        const int qualifiedBuildInvocationCount = 1;
        var qualified = Goal164BuildFixture.Create(coreOnly: true);
        var qualifiedPackagePath = Path.Combine(
            qualified.Project.Path, "package.json");
        var qualifiedPackageHash = FileSha(qualifiedPackagePath);
        var qualifiedHistoryFiles =
            BuildHistoryFiles(qualified.Project.Path);
        var qualifiedHistory = ReadHistory(
            qualified.Build.BuildHistoryPath);
        var qualifiedAuthoringHash =
            TreeSha(AuthoringRoot(qualified.Project.Path));
        var qualifiedGenerationHash =
            TreeSha(GenerationRoot(qualified.Project.Path));
        var qualifiedSourceHash = FileSha(Path.Combine(
            qualified.Project.Path,
            SeededGeneratedProjectVocabulary.SourceRelativePath
                .Replace('/', Path.DirectorySeparatorChar)));
        var correlation =
            GeneratedCampaignRegionalEventCorrelationService.Validate(
                qualified.Package,
                qualifiedPackageHash,
                qualified.Build.GeneratedCampaignRegionalEvents
                ?? throw new InvalidOperationException(
                    "goal169d.qualified.events_missing"),
                qualified.Build.GeneratedCampaignRelationships
                ?? throw new InvalidOperationException(
                    "goal169d.qualified.relationships_missing"));

        var portable = Goal156TestKit.Copy(
            qualified.Project, "goal169d-qualified-core-portable");
        var builds = Path.Combine(portable.Path, "Builds");
        if (Directory.Exists(builds))
            Directory.Delete(builds, recursive: true);
        var portablePackagePath = Path.Combine(
            portable.Path, "package.json");
        var portableHistoryPath = Path.Combine(
            portable.Path,
            Path.GetRelativePath(
                qualified.Project.Path,
                qualified.Build.BuildHistoryPath));
        var portableBeforeOpen = CaptureProjectFiles(
            portable.Path, portableHistoryPath);
        var portableController =
            Goal156TestKit.OpenWorkspace(portable.Path);
        var portableSnapshot = portableController.Snapshot();
        var portableAfterOpen = CaptureProjectFiles(
            portable.Path, portableHistoryPath);
        var portableHistory = ReadHistory(portableHistoryPath);
        var portablePackage = Goal156TestKit.Load(portable.Path);
        var portableCorrelation =
            GeneratedCampaignRegionalEventCorrelationService.Validate(
                portablePackage,
                FileSha(portablePackagePath),
                portableSnapshot.GeneratedCampaignRegionalEvents
                ?? throw new InvalidOperationException(
                    "goal169d.portable.events_missing"),
                portableSnapshot.GeneratedCampaignRelationships
                ?? throw new InvalidOperationException(
                    "goal169d.portable.relationships_missing"));
        var pointer = new ProjectStandaloneOutputLocationService()
            .LoadCurrentOutput(
                portable.Path,
                portablePackage.Manifest.PackageId);

        var retainedAfter = Goal169CRetainedPublication.Capture();
        return new Goal169DQualifiedPortableState(
            raw,
            qualified,
            qualifiedHistory,
            qualifiedHistoryFiles,
            qualifiedBuildInvocationCount,
            qualifiedPackageHash,
            qualifiedSourceHash,
            qualifiedAuthoringHash,
            qualifiedGenerationHash,
            correlation,
            portable,
            portableSnapshot,
            portableHistory,
            portableBeforeOpen,
            portableAfterOpen,
            portableCorrelation,
            pointer,
            retainedBefore,
            retainedAfter,
            hostBefore,
            TreeSha(hostRoot),
            RealPlayerSmokeInvocationCount: 0,
            UnityEditorProcessStartCount: 0,
            UnityHostBuildCount: 0,
            CachedHostMutationCount: 0);
    }

    private static Goal169DProjectFileCapture CaptureProjectFiles(
        string project,
        string selectedHistoryPath) => new(
        FileSha(Path.Combine(project, "package.json")),
        FileSha(selectedHistoryPath),
        TreeSha(AuthoringRoot(project)),
        TreeSha(GenerationRoot(project)));

    private static GameProjectBuildHistoryEntry ReadHistory(
        string path) =>
        JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions)
        ?? throw new JsonException("goal169d.history_invalid");
}

internal sealed record Goal169DRawState(
    GeneratedProject Project,
    LLMGameCreator.GamePackage.GamePackageDefinition Package,
    SeededGeneratedProjectSourceValidationResult Source,
    GameProjectAuthoringState Authoring,
    UnifiedGameProjectWorkspaceSnapshot Snapshot,
    string PackageSha256,
    string SourceSha256,
    string AuthoringSha256,
    string GenerationSha256,
    int BuildInvocationCount,
    string Status);

internal sealed record Goal169DQualifiedPortableState(
    Goal169DRawState Raw,
    Goal164BuildFixture Qualified,
    GameProjectBuildHistoryEntry QualifiedHistory,
    IReadOnlyList<string> QualifiedHistoryFiles,
    int QualifiedBuildInvocationCount,
    string QualifiedPackageSha256,
    string QualifiedSourceSha256,
    string QualifiedAuthoringSha256,
    string QualifiedGenerationSha256,
    GeneratedCampaignRegionalEventCorrelationResult QualifiedCorrelation,
    GeneratedProject Portable,
    UnifiedGameProjectWorkspaceSnapshot PortableSnapshot,
    GameProjectBuildHistoryEntry PortableHistory,
    Goal169DProjectFileCapture PortableBeforeOpen,
    Goal169DProjectFileCapture PortableAfterOpen,
    GeneratedCampaignRegionalEventCorrelationResult PortableCorrelation,
    ProjectStandaloneCurrentOutputReadResult PortablePointer,
    Goal169CRetainedPublicationCapture RetainedBefore,
    Goal169CRetainedPublicationCapture RetainedAfter,
    string HostBeforeSha256,
    string HostAfterSha256,
    int RealPlayerSmokeInvocationCount,
    int UnityEditorProcessStartCount,
    int UnityHostBuildCount,
    int CachedHostMutationCount)
{
    internal GameProjectGeneratedCampaignRelationshipSummary
        Relationships => Qualified.Build.GeneratedCampaignRelationships
                         ?? throw new InvalidOperationException(
                             "goal169d.relationships_missing");

    internal GameProjectGeneratedCampaignRegionalEventSummary Events =>
        Qualified.Build.GeneratedCampaignRegionalEvents
        ?? throw new InvalidOperationException(
            "goal169d.events_missing");

    internal int AvailableBranchCount =>
        Relationships.BranchQualifications.Count(item =>
            item.Available);
}

internal sealed record Goal169DProjectFileCapture(
    string PackageSha256,
    string SelectedHistorySha256,
    string AuthoringSha256,
    string GenerationSha256);

internal static class Goal169CRetainedPublication
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    internal static Goal169CRetainedPublicationCapture Capture()
    {
        var root = Goal156TestKit.RepositoryRoot;
        using var publication = ReadProof(
            root, "immutable-run-publication-proof.json");
        using var correlation = ReadProof(
            root,
            "immutable-payload-history-package-correlation-proof.json");
        using var rc = ReadProof(root, "rc-portability-proof.json");
        var publicationRoot = publication.RootElement;
        var correlationRoot = correlation.RootElement;
        var rcRoot = rc.RootElement;
        var pointerPath = RequiredString(
            publicationRoot, "currentPointerPath");
        var runStatusPath = RequiredString(
            publicationRoot, "runStatusPath");
        var selectedHistoryPath = RequiredString(
            correlationRoot, "selectedHistoryPath");
        var releaseCandidatePath = RequiredString(
            rcRoot, "releaseCandidatePath");
        var runRoot = Directory.GetParent(runStatusPath)?.FullName
                      ?? throw new InvalidOperationException(
                          "goal169d.retained.run_root_missing");
        var projectRoot = Directory.GetParent(
                              Directory.GetParent(
                                  Directory.GetParent(
                                      selectedHistoryPath)!.FullName)!
                                  .FullName)!
                          .FullName;
        var payloadRoot = Path.Combine(
            runRoot,
            "g_Data",
            "StreamingAssets",
            "LLMGameCreatorProject");
        var packagePath = Path.Combine(projectRoot, "package.json");
        var standaloneHistoryPath = Path.Combine(
            projectRoot,
            ProjectStandaloneBuildVocabulary.HistoryRelativePath
                .Replace('/', Path.DirectorySeparatorChar));
        var pointer = JsonSerializer.Deserialize<
                          ProjectStandaloneCurrentPointer>(
                          File.ReadAllText(pointerPath, Encoding.UTF8),
                          JsonOptions)
                      ?? throw new JsonException(
                          "goal169d.retained.pointer_invalid");
        var status = JsonSerializer.Deserialize<
                         ProjectStandaloneRunStatus>(
                         File.ReadAllText(runStatusPath, Encoding.UTF8),
                         JsonOptions)
                     ?? throw new JsonException(
                         "goal169d.retained.run_status_invalid");
        var history = JsonSerializer.Deserialize<
                          GameProjectBuildHistoryEntry>(
                          File.ReadAllText(
                              selectedHistoryPath, Encoding.UTF8),
                          JsonOptions)
                      ?? throw new JsonException(
                          "goal169d.retained.history_invalid");

        return new Goal169CRetainedPublicationCapture(
            pointerPath,
            runRoot,
            runStatusPath,
            payloadRoot,
            standaloneHistoryPath,
            selectedHistoryPath,
            releaseCandidatePath,
            packagePath,
            Goal169DTestKit.FileSha(pointerPath),
            Goal169DTestKit.TreeSha(runRoot),
            Goal169DTestKit.FileSha(runStatusPath),
            Goal169DTestKit.TreeSha(payloadRoot),
            Goal169DTestKit.FileSha(standaloneHistoryPath),
            Goal169DTestKit.FileSha(selectedHistoryPath),
            Goal169DTestKit.FileSha(releaseCandidatePath),
            Goal169DTestKit.FileSha(packagePath),
            pointer,
            status,
            history,
            RequiredString(
                publicationRoot, "currentPointerSha256"),
            RequiredString(
                publicationRoot, "runStatusSha256"),
            RequiredString(
                correlationRoot, "selectedHistorySha256"),
            RequiredString(
                rcRoot, "releaseCandidateSha256"),
            RequiredString(
                correlationRoot, "actualPayloadPackageSha256"),
            RequiredString(publicationRoot, "finalStateHash"));
    }

    private static JsonDocument ReadProof(
        string root,
        string fileName) => JsonDocument.Parse(File.ReadAllText(
        Path.Combine(
            root,
            ".llmgc",
            "procedural",
            "goal-169c-post-fix-immutable-standalone-rc-and-portable-closure",
            fileName),
        Encoding.UTF8));

    private static string RequiredString(
        JsonElement element,
        string propertyName) =>
        element.GetProperty(propertyName).GetString()
        ?? throw new JsonException(
            "goal169d.retained." + propertyName + "_missing");
}

internal sealed record Goal169CRetainedPublicationCapture(
    string CurrentPointerPath,
    string RunRoot,
    string RunStatusPath,
    string PayloadRoot,
    string StandaloneHistoryPath,
    string SelectedHistoryPath,
    string ReleaseCandidatePath,
    string PackagePath,
    string CurrentPointerSha256,
    string RunTreeSha256,
    string RunStatusSha256,
    string PayloadTreeSha256,
    string StandaloneHistorySha256,
    string SelectedHistorySha256,
    string ReleaseCandidateSha256,
    string PackageSha256,
    ProjectStandaloneCurrentPointer Pointer,
    ProjectStandaloneRunStatus RunStatus,
    GameProjectBuildHistoryEntry SelectedHistory,
    string ExpectedCurrentPointerSha256,
    string ExpectedRunStatusSha256,
    string ExpectedSelectedHistorySha256,
    string ExpectedReleaseCandidateSha256,
    string ExpectedPackageSha256,
    string ExpectedFinalStateHash);
