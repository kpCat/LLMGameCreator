using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal160;

[Collection(Goal160Collection.Name)]
public sealed class Goal160CandidateSealTests
{
    [Fact]
    public void Behavioral_preview_publication_writes_exact_seal()
    {
        using var fixture = CandidateSealFixture.Create();
        Assert.True(File.Exists(Path.Combine(fixture.Root, ".llmgc", "regeneration-candidate", "seal.json")));
        Assert.Equal(64, fixture.Seal.SealSha256.Length);
        Assert.Contains(fixture.Seal.SealSha256, fixture.Service.Serialize(fixture.Seal), StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_unchanged_candidate_seal_validates()
    {
        using var fixture = CandidateSealFixture.Create();
        var result = fixture.Verify();
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
    }

    [Fact]
    public void Behavioral_caller_modified_diff_is_rejected_by_diff_hash()
    {
        using var fixture = CandidateSealFixture.Create();
        var result = fixture.Service.Verify(fixture.Root, fixture.Seal, fixture.Build, fixture.Snapshot,
            fixture.Diff with { NewSeed = "caller-tamper" }, fixture.Authoring);
        Assert.Contains("regeneration.candidate_seal_mismatch", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_root_identity_substitution_is_rejected()
    {
        using var fixture = CandidateSealFixture.Create();
        var expected = fixture.Seal with { CandidateRootIdentity = Guid.NewGuid().ToString("N") };
        var result = fixture.Service.Verify(fixture.Root, expected, fixture.Build, fixture.Snapshot,
            fixture.Diff, fixture.Authoring);
        Assert.Contains("regeneration.candidate_seal_mismatch", result.Diagnostics);
    }

    [Theory]
    [InlineData("package", "regeneration.candidate_package_changed")]
    [InlineData("authoring", "regeneration.candidate_authoring_changed")]
    [InlineData("identity", "regeneration.candidate_tampered")]
    [InlineData("generation", "regeneration.candidate_tampered")]
    [InlineData("history", "regeneration.candidate_history_changed")]
    [InlineData("support", "regeneration.candidate_tampered")]
    [InlineData("rc", "regeneration.candidate_tampered")]
    public void Behavioral_candidate_file_tamper_is_rejected_before_mutation(
        string target,
        string expectedDiagnostic)
    {
        using var fixture = CandidateSealFixture.Create();
        File.AppendAllText(fixture.PathFor(target), "tamper", Encoding.UTF8);
        var result = fixture.Verify();
        Assert.Contains(expectedDiagnostic, result.Diagnostics);
    }

    [Fact]
    public void Behavioral_selected_history_name_change_is_rejected()
    {
        using var fixture = CandidateSealFixture.Create();
        var result = fixture.Service.Verify(fixture.Root,
            fixture.Seal with { SelectedBuildHistoryFileName = "other.json", SealSha256 = new string('0', 64) },
            fixture.Build, fixture.Snapshot, fixture.Diff, fixture.Authoring);
        Assert.Contains("regeneration.candidate_seal_mismatch", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_build_hash_change_is_rejected()
    {
        using var fixture = CandidateSealFixture.Create();
        var result = fixture.Service.Verify(fixture.Root, fixture.Seal,
            fixture.Build with { FinalStateHash = new string('0', 64) },
            fixture.Snapshot, fixture.Diff, fixture.Authoring);
        Assert.Contains("regeneration.candidate_package_changed", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_failed_seal_validation_leaves_separate_authority_unchanged()
    {
        using var fixture = CandidateSealFixture.Create();
        using var authority = OperationProject.Create();
        var marker = Path.Combine(authority.Path, "authority.txt");
        File.WriteAllText(marker, "before", Encoding.UTF8);
        File.AppendAllText(fixture.PathFor("package"), "tamper", Encoding.UTF8);
        Assert.False(fixture.Verify().Passed);
        Assert.Equal("before", File.ReadAllText(marker, Encoding.UTF8));
    }
}

internal sealed class CandidateSealFixture : IDisposable
{
    private CandidateSealFixture(
        string root,
        GameProjectSeedRegenerationCandidateSealService service,
        GameProjectSeedRegenerationCandidateSeal seal,
        GameProjectBuildResult build,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        GameProjectSeedRegenerationDiff diff,
        GameProjectAuthoringState authoring)
    {
        Root = root;
        Service = service;
        Seal = seal;
        Build = build;
        Snapshot = snapshot;
        Diff = diff;
        Authoring = authoring;
    }

    public string Root { get; }
    public GameProjectSeedRegenerationCandidateSealService Service { get; }
    public GameProjectSeedRegenerationCandidateSeal Seal { get; }
    public GameProjectBuildResult Build { get; }
    public UnifiedGameProjectWorkspaceSnapshot Snapshot { get; }
    public GameProjectSeedRegenerationDiff Diff { get; }
    public GameProjectAuthoringState Authoring { get; }

    public static CandidateSealFixture Create()
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal160Seal", Guid.NewGuid().ToString("N"));
        Write(root, ".llmgc/generation/seeded-project-source.json", "source");
        Write(root, ".llmgc/generation/generated-base-package.json", "base");
        Write(root, "package.json", "package");
        Write(root, ".llmgc/authoring/document.json", "authoring");
        Write(root, ".llmgc/project-identity.json", "identity");
        Write(root, ".llmgc/build-history/selected.json", "history");
        Write(root, "assets/asset.txt", "asset");
        Write(root, "scripts/script.txt", "script");
        Write(root, UnifiedGameProjectWorkspaceVocabulary.ReleaseCandidateRecordRelativePath, "rc");
        var parameter = JsonSerializer.SerializeToElement(3);
        var document = new FeatureModuleCompositionDocument
        {
            SelectedModuleIds = ["feature.test"],
            ParameterValues = [new FeatureModuleParameterValue
            {
                ModuleId = "feature.test",
                ParameterId = "count",
                Value = parameter
            }]
        };
        var authoring = new GameProjectAuthoringState
        {
            ProjectFolder = root,
            Document = document,
            Identity = new GameProjectIdentityDocument { PackageId = "test", Title = "Test" }
        };
        var build = new GameProjectBuildResult
        {
            Passed = true,
            PackageSha256 = Hash(Path.Combine(root, "package.json")),
            CompositionPackageSha256 = new string('c', 64),
            FinalStateHash = new string('f', 64),
            QualifiedAuthoringFingerprint = new string('a', 64)
        };
        var snapshot = new UnifiedGameProjectWorkspaceSnapshot
        {
            GeneratedWorld = new GameProjectGeneratedWorldSummary
            {
                Present = true,
                Passed = true,
                Status = "TRAVEL_CURRENT",
                PlanSha256 = new string('1', 64),
                OverlaySha256 = new string('2', 64),
                GeneratedBasePackageSha256 = new string('3', 64)
            }
        };
        var diff = new GameProjectSeedRegenerationDiff
        {
            OldSeed = "old",
            NewSeed = "new",
            NewSourceRequestSha256 = new string('4', 64),
            NewPlanSha256 = new string('1', 64),
            NewOverlaySha256 = new string('2', 64),
            NewGeneratedBaseSha256 = new string('3', 64),
            GameplayChanged = true,
            AuthoringPreserved = true,
            ProjectIdentityPreserved = true
        };
        var service = new GameProjectSeedRegenerationCandidateSealService();
        var seal = service.Create(root, Guid.NewGuid().ToString("N"), "attempt", "selected.json",
            build, snapshot, diff, authoring);
        return new CandidateSealFixture(root, service, seal, build, snapshot, diff, authoring);
    }

    public GameProjectSeedRegenerationCandidateSealResult Verify() =>
        Service.Verify(Root, Seal, Build, Snapshot, Diff, Authoring);

    public string PathFor(string target) => target switch
    {
        "package" => Path.Combine(Root, "package.json"),
        "authoring" => Path.Combine(Root, ".llmgc", "authoring", "document.json"),
        "identity" => Path.Combine(Root, ".llmgc", "project-identity.json"),
        "generation" => Path.Combine(Root, ".llmgc", "generation", "generated-base-package.json"),
        "history" => Path.Combine(Root, ".llmgc", "build-history", "selected.json"),
        "support" => Path.Combine(Root, "assets", "asset.txt"),
        "rc" => Path.Combine(Root, UnifiedGameProjectWorkspaceVocabulary.ReleaseCandidateRecordRelativePath
            .Replace('/', Path.DirectorySeparatorChar)),
        _ => throw new InvalidOperationException(target)
    };

    private static void Write(string root, string relative, string value)
    {
        var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, value, new UTF8Encoding(false));
    }

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant();
    }

    public void Dispose()
    {
        if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
    }
}
