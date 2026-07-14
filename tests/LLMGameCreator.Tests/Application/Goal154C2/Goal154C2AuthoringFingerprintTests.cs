using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154C2;

public sealed class Goal154C2AuthoringFingerprintTests
{
    [Fact] public void Behavioral_fingerprint_is_deterministic_when_selected_values_and_fingerprints_are_reordered()
    {
        var (library, document) = SocialDocument();
        var reordered = document with { SelectedModuleIds = document.SelectedModuleIds.Reverse().ToList(), ParameterValues = document.ParameterValues.Reverse().ToList(), ModuleFingerprints = document.ModuleFingerprints.Reverse().ToDictionary(x => x.Key, x => x.Value) };
        Assert.Equal(Fingerprint(document, library).Sha256, Fingerprint(reordered, library).Sha256);
    }
    [Fact] public void Behavioral_omitted_default_equals_explicit_default()
    {
        var (library, document) = SocialDocument();
        Assert.Equal(Fingerprint(document with { ParameterValues = [] }, library).Sha256, Fingerprint(document, library).Sha256);
    }
    [Fact] public void Behavioral_revision_timestamps_hashes_and_status_do_not_change_fingerprint()
    {
        var (library, document) = SocialDocument();
        var changed = document with { Revision = 99, CreatedAtUtc = DateTimeOffset.MinValue, UpdatedAtUtc = DateTimeOffset.MaxValue, LastQualificationStatus = "FAILED", LastMaterializedPackageSha256 = "x", LastQualifiedFinalStateHash = "y" };
        Assert.Equal(Fingerprint(document, library).Sha256, Fingerprint(changed, library).Sha256);
    }
    [Fact] public void Behavioral_parameter_change_changes_fingerprint()
    {
        var (library, document) = SocialDocument();
        Assert.NotEqual(Fingerprint(document, library).Sha256, Fingerprint(With(document, Dialogue, "trustedGoldReward", 9), library).Sha256);
    }
    [Fact] public void Behavioral_selected_module_change_changes_fingerprint()
    {
        var (library, document) = SocialDocument();
        Assert.NotEqual(Fingerprint(document, library).Sha256, Fingerprint(document with { SelectedModuleIds = [Faction, Quest] }, library).Sha256);
    }
    [Fact] public void Behavioral_selected_module_fingerprint_change_changes_fingerprint()
    {
        var (library, document) = SocialDocument();
        var fingerprints = library.ModuleFingerprints.ToDictionary(x => x.Key, x => x.Value); fingerprints[Dialogue] = "changed";
        Assert.NotEqual(Fingerprint(document, library).Sha256, Fingerprint(document, library with { ModuleFingerprints = fingerprints }).Sha256);
    }
    [Fact] public void Behavioral_unselected_optional_module_fingerprint_change_does_not_change_fingerprint()
    {
        var (library, document) = SocialDocument();
        var other = library.Catalog.Modules.First(module => !module.Required && !document.SelectedModuleIds.Contains(module.ModuleId)).ModuleId;
        var fingerprints = library.ModuleFingerprints.ToDictionary(x => x.Key, x => x.Value); fingerprints[other] = "changed";
        Assert.Equal(Fingerprint(document, library).Sha256, Fingerprint(document, library with { ModuleFingerprints = fingerprints }).Sha256);
    }
    [Fact] public void Behavioral_invalid_duplicate_and_unselected_parameters_are_causal()
    {
        var (library, document) = SocialDocument();
        var invalid = document with { ParameterValues = document.ParameterValues.Concat([new FeatureModuleParameterValue { ModuleId = Dialogue, ParameterId = "trustedGoldReward", Value = JsonSerializer.SerializeToElement(101) }, new FeatureModuleParameterValue { ModuleId = "feature.profile.alchemy_focus", ParameterId = "healingPotionOutput", Value = JsonSerializer.SerializeToElement(3) }]).ToList() };
        var result = Fingerprint(invalid, library);
        Assert.False(result.Passed); Assert.Contains(result.Diagnostics, value => value.Contains("duplicate", StringComparison.Ordinal)); Assert.Contains(result.Diagnostics, value => value.Contains("unselected", StringComparison.Ordinal));
    }
    [Fact] public void Behavioral_unknown_selected_module_is_causal() { var (l, d) = SocialDocument(); var r = Fingerprint(d with { SelectedModuleIds = d.SelectedModuleIds.Append("missing").ToList() }, l); Assert.False(r.Passed); Assert.Contains(r.Diagnostics, x => x.Contains("unknown_selected", StringComparison.Ordinal)); }
    [Fact] public void Behavioral_missing_selected_fingerprint_is_causal() { var (l, d) = SocialDocument(); var map = l.ModuleFingerprints.Where(x => x.Key != Dialogue).ToDictionary(x => x.Key, x => x.Value); var r = Fingerprint(d, l with { ModuleFingerprints = map }); Assert.False(r.Passed); Assert.Contains(r.Diagnostics, x => x.Contains("missing_module_fingerprint", StringComparison.Ordinal)); }
    [Fact] public void Behavioral_history_matching_fingerprint_is_current()
    {
        var (library, document) = SocialDocument(); var fingerprint = Fingerprint(document, library).Sha256;
        using var temp = new TempDirectory(); Write(temp.Path, Entry(fingerprint));
        var result = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, document, library);
        Assert.Equal("CURRENT", result.SocialConfigurationStatus); Assert.True(result.MatchesCurrentConfiguration);
    }
    [Fact] public void Behavioral_history_mismatched_fingerprint_is_last_success()
    {
        var (library, document) = SocialDocument(); using var temp = new TempDirectory(); Write(temp.Path, Entry("other"));
        Assert.Equal("LAST_SUCCESS", new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, document, library).SocialConfigurationStatus);
    }
    [Fact] public void Behavioral_old_history_without_fingerprint_is_unknown_never_current()
    {
        var (library, document) = SocialDocument(); using var temp = new TempDirectory(); Write(temp.Path, Entry(string.Empty));
        Assert.Equal("UNKNOWN", new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, document, library).SocialConfigurationStatus);
    }
    [Fact] public void Behavioral_history_rejects_false_social_replay_flags()
    {
        var (library, document) = SocialDocument(); using var temp = new TempDirectory(); Write(temp.Path, Entry(Fingerprint(document, library).Sha256) with { Social = new GameProjectSocialSummary { Present = true, Passed = true, CheckpointReplayPassed = false, FullReplayEquivalent = true } });
        Assert.Null(new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, document, library).LastSuccessfulBuild);
    }

    private const string Faction = "feature.faction.reputation_standing", Quest = "feature.quest.faction_reputation_consequences", Dialogue = "feature.dialogue.reputation_gated_reward";
    private static FeatureModuleAuthoringFingerprintResult Fingerprint(FeatureModuleCompositionDocument d, FeatureModuleLibrarySnapshot l) => new FeatureModuleAuthoringFingerprintService().Calculate(d, l);
    private static (FeatureModuleLibrarySnapshot, FeatureModuleCompositionDocument) SocialDocument()
    {
        var root = FindRoot(); var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var values = new[] { V(Faction,"startingReputation",0), V(Quest,"questReputationReward",10), V(Quest,"questFailurePenalty",5), V(Dialogue,"trustedReputationThreshold",10), V(Dialogue,"trustedGoldReward",7) };
        return (library, new FeatureModuleCompositionDocument { BaseCandidateId = "minimal-map-game-balanced-baseline", SelectedModuleIds = [Faction, Quest, Dialogue], ParameterValues = values, LastActivatedProjectPackageSha256 = "package", LastCompositionPackageSha256 = "composition", LastQualifiedFinalStateHash = "final" });
    }
    private static FeatureModuleParameterValue V(string m,string p,int value) => new() { ModuleId=m, ParameterId=p, Value=JsonSerializer.SerializeToElement(value) };
    private static FeatureModuleCompositionDocument With(FeatureModuleCompositionDocument d,string m,string p,int value) => d with { ParameterValues=d.ParameterValues.Where(x => x.ModuleId != m || x.ParameterId != p).Append(V(m,p,value)).ToList() };
    private static GameProjectBuildHistoryEntry Entry(string fingerprint) => new() { Status="GREEN",AttemptStatus="GREEN",PackageSha256="package",ActivatedProjectPackageSha256="package",CompositionPackageSha256="composition",FinalStateHash="final",CheckpointReloadPassed=true,FullReplayEquivalent=true,ActionBindingPassed=true,CompletedAtUtc=DateTimeOffset.UtcNow,QualifiedAuthoringFingerprint=fingerprint,Social=new GameProjectSocialSummary { Present=true,Passed=true,CheckpointReplayPassed=true,FullReplayEquivalent=true } };
    private static void Write(string root,GameProjectBuildHistoryEntry entry) { var path=Path.Combine(root,".llmgc","build-history"); Directory.CreateDirectory(path); File.WriteAllText(Path.Combine(path,"entry.json"),JsonSerializer.Serialize(entry)); }
    private static string FindRoot() { for(var d=new DirectoryInfo(AppContext.BaseDirectory);d is not null;d=d.Parent) if(File.Exists(Path.Combine(d.FullName,"LLMGameCreator.sln"))) return d.FullName; throw new DirectoryNotFoundException(); }
    private sealed class TempDirectory:IDisposable { public TempDirectory(){Path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Path);} public string Path{get;} public void Dispose(){if(Directory.Exists(Path))Directory.Delete(Path,true);} }
}
