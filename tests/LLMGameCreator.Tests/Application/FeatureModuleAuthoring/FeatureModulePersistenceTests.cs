using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using Xunit;

namespace LLMGameCreator.Tests.Application.FeatureModuleAuthoring;

public sealed class FeatureModulePersistenceTests
{
    [Fact]
    public void Create_save_load_clone_delete_and_staleness_are_deterministic()
    {
        var library = FeatureModuleLibraryAndParameterTests.Load();
        var workspace = Temp("persistence");
        var clock = new FixedClock(new DateTimeOffset(2026, 7, 11, 8, 30, 0, TimeSpan.Zero));
        try
        {
            var service = new FeatureModuleCompositionPersistenceService(workspace, clock);
            var created = service.CreateNew("goal147-test", "Goal147 Test", "Roundtrip", library);
            var saved = service.Save(created, library);
            var loaded = service.Load(created.CompositionId, library);
            Assert.Equal(1, saved.Revision);
            Assert.Equal(FeatureModuleCompositionPersistenceService.SerializeCanonical(saved),
                FeatureModuleCompositionPersistenceService.SerializeCanonical(loaded));
            var clone = service.Clone(saved.CompositionId, "goal147-test-clone", "Goal147 Clone", library);
            Assert.Equal(1, clone.Revision);
            Assert.Equal(2, service.List(library).CompositionCount);
            service.Delete(clone.CompositionId);
            Assert.Single(service.List(library).Compositions);
            Assert.Throws<InvalidOperationException>(() => service.SaveAs(saved, saved.CompositionId, "Duplicate", library));
            Assert.Throws<InvalidOperationException>(() => service.CreateNew("../escape", "Bad", "Bad", library));

            var changed = library with { CatalogFingerprint = new string('a', 64) };
            var stale = new FeatureModuleCompositionStalenessService().Evaluate(loaded, changed);
            Assert.False(stale.Stale);
            Assert.True(stale.AdditiveCompatible);
            Assert.Equal("ADDITIVE_COMPATIBLE", stale.Status);
            Assert.True(stale.CatalogFingerprintChanged);
        }
        finally { if (Directory.Exists(workspace)) Directory.Delete(workspace, true); }
    }

    [Fact]
    public void Corrupt_document_is_rejected_without_overwriting_existing_bytes()
    {
        var library = FeatureModuleLibraryAndParameterTests.Load();
        var workspace = Temp("corrupt");
        try
        {
            Directory.CreateDirectory(workspace);
            var path = Path.Combine(workspace, "corrupt.featurecomposition.json");
            File.WriteAllText(path, "{not-json");
            var before = File.ReadAllBytes(path);
            var service = new FeatureModuleCompositionPersistenceService(workspace);
            Assert.Throws<InvalidOperationException>(() => service.Load("corrupt", library));
            Assert.Equal(before, File.ReadAllBytes(path));
            var index = service.List(library);
            Assert.Equal(1, index.CorruptDocumentCount);
            Assert.Equal("CORRUPT", Assert.Single(index.Compositions).Status);
        }
        finally { if (Directory.Exists(workspace)) Directory.Delete(workspace, true); }
    }

    [Fact]
    public void Missing_selected_module_is_reported_unresolved_without_fallback()
    {
        var library = FeatureModuleLibraryAndParameterTests.Load();
        var document = new FeatureModuleCompositionDocument
        {
            CompositionId = "missing-module",
            SelectedModuleIds = ["feature.profile.missing"],
            CatalogFingerprint = library.CatalogFingerprint
        };
        var stale = new FeatureModuleCompositionStalenessService().Evaluate(document, library);
        Assert.True(stale.Unresolved);
        Assert.Equal("UNRESOLVED", stale.Status);
        Assert.Contains("feature.profile.missing", stale.MissingModuleIds);
    }

    private static string Temp(string suffix) => Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal147-" + suffix + "-" + Guid.NewGuid().ToString("N"));

    private sealed class FixedClock(DateTimeOffset value) : IFeatureModuleAuthoringClock
    {
        public DateTimeOffset UtcNow => value;
    }
}
