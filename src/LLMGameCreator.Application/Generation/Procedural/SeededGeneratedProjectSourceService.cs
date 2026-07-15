using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class SeededGeneratedProjectSourceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly IReadOnlySet<string> ExactSourceProperties = new HashSet<string>(StringComparer.Ordinal)
    {
        "schemaVersion", "creationKind", "seed", "mode", "presetId", "styleHintIds", "variantIds",
        "mechanicsProfileId", "planId", "planSha256", "rulePackId", "rulePackSha256",
        "tinyLoopStateSha256", "generatedMvpPackageSha256", "generatedOverlaySha256",
        "generatedBasePackageSha256", "goal142BaselinePackageSha256", "generatedStartMapId",
        "counts", "tinyLoop", "sidecarSha256"
    };

    private readonly IGamePackageValidator _validator;
    private readonly GenerationPresetOptionsService _presetOptions;
    private readonly GeneratedProjectOverlayService _overlayService;

    public SeededGeneratedProjectSourceService(
        IGamePackageValidator? validator = null,
        GenerationPresetOptionsService? presetOptions = null,
        GeneratedProjectOverlayService? overlayService = null)
    {
        _validator = validator ?? new GamePackageValidator();
        _presetOptions = presetOptions ?? new GenerationPresetOptionsService();
        _overlayService = overlayService ?? new GeneratedProjectOverlayService(_validator);
    }

    public SeededGeneratedProjectSourceValidationResult Validate(string projectFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFolder);
        var root = Path.GetFullPath(projectFolder);
        var sourcePath = Resolve(root, SeededGeneratedProjectVocabulary.SourceRelativePath);
        if (!File.Exists(sourcePath)) return new SeededGeneratedProjectSourceValidationResult
        {
            Present = false,
            Passed = true,
            Status = "ABSENT",
            SourcePath = sourcePath
        };

        var diagnostics = new List<string>();
        try
        {
            var sourceJson = File.ReadAllText(sourcePath, Encoding.UTF8);
            using var sourceDocument = JsonDocument.Parse(sourceJson);
            var actualProperties = sourceDocument.RootElement.EnumerateObject()
                .Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
            if (!actualProperties.SetEquals(ExactSourceProperties))
                diagnostics.Add("generated_source.unsupported_schema");
            var source = JsonSerializer.Deserialize<SeededGeneratedProjectSourceRecord>(sourceJson, JsonOptions);
            if (source is null)
                return Failed(sourcePath, diagnostics.Append("generated_source.invalid_json"));

            ValidateVocabulary(source, diagnostics);
            var generationRoot = Resolve(root, SeededGeneratedProjectVocabulary.GenerationRelativeRoot);
            ValidateSidecars(generationRoot, source, diagnostics);
            if (diagnostics.Count > 0) return Failed(sourcePath, diagnostics, source);

            var planJson = Read(generationRoot, SeededGeneratedProjectVocabulary.PlanJsonFileName);
            var rulePackJson = Read(generationRoot, SeededGeneratedProjectVocabulary.RulePackJsonFileName);
            var tinyStateJson = Read(generationRoot, SeededGeneratedProjectVocabulary.TinyLoopStateJsonFileName);
            var tinyReportMarkdown = Read(generationRoot, SeededGeneratedProjectVocabulary.TinyLoopReportMarkdownFileName);
            var generatedMvpJson = Read(generationRoot, SeededGeneratedProjectVocabulary.GeneratedMvpPackageJsonFileName);
            var overlayJson = Read(generationRoot, SeededGeneratedProjectVocabulary.GeneratedOverlayJsonFileName);
            var generatedBaseJson = Read(generationRoot, SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName);

            var plan = Deserialize<ProceduralGeneratedGamePlan>(planJson, "generated_source.plan_invalid_json");
            var rulePack = Deserialize<FormulaEffectActionRulePack>(rulePackJson, "generated_source.rule_pack_invalid_json");
            var tinyState = Deserialize<TinyGeneratedRuntimeState>(tinyStateJson, "generated_source.tiny_state_invalid_json");
            var generatedMvp = Deserialize<GamePackageDefinition>(generatedMvpJson, "generated_source.mvp_invalid_json");
            var overlay = Deserialize<GeneratedProjectOverlayDocument>(overlayJson, "generated_source.overlay_invalid_json");
            var generatedBase = Deserialize<GamePackageDefinition>(generatedBaseJson, "generated_source.base_invalid_json");

            ValidateCounts(source.Counts, plan, diagnostics);
            if (!string.Equals(source.PlanId, plan.PlanId, StringComparison.Ordinal)
                || !string.Equals(source.RulePackId, rulePack.Metadata.RulePackId, StringComparison.Ordinal))
                diagnostics.Add("generated_source.identity_mismatch");
            if (!string.Equals(source.GeneratedStartMapId, overlay.GeneratedStartMapId, StringComparison.Ordinal)
                || !generatedBase.Game.Maps.Any(map => map.Id == source.GeneratedStartMapId))
                diagnostics.Add("generated_source.generated_start_map_missing");
            if (!string.Equals(source.Goal142BaselinePackageSha256, overlay.Goal142BaselinePackageSha256, StringComparison.Ordinal)
                || !string.Equals(source.GeneratedBasePackageSha256, overlay.GeneratedBasePackageSha256, StringComparison.Ordinal))
                diagnostics.Add("generated_source.overlay_hash_chain_mismatch");

            var regeneratedRulePack = new FormulaEffectActionRegistryService().Generate(
                new FormulaEffectActionRegistryRequest { SourcePlan = plan });
            if (!string.Equals(regeneratedRulePack.Json, rulePackJson, StringComparison.Ordinal))
                diagnostics.Add("generated_source.rule_pack_regeneration_mismatch");
            var regeneratedTinyLoop = new TinyGeneratedRuntimeLoopService().Run(new TinyGeneratedRuntimeLoopRequest
            {
                SourcePlan = plan,
                RulePack = regeneratedRulePack.RulePack,
                RulePackValidationReport = regeneratedRulePack.ValidationReport
            });
            if (!string.Equals(regeneratedTinyLoop.StateJson, tinyStateJson, StringComparison.Ordinal)
                || !string.Equals(regeneratedTinyLoop.ReportMarkdown, tinyReportMarkdown, StringComparison.Ordinal))
                diagnostics.Add("generated_source.tiny_loop_regeneration_mismatch");
            var expectedTiny = BuildTinyLoopFacts(plan, regeneratedRulePack.RulePack, regeneratedTinyLoop);
            if (expectedTiny != source.TinyLoop
                || !string.Equals(tinyState.DeterministicHash, source.TinyLoop.FinalStateHash, StringComparison.Ordinal)
                || !source.TinyLoop.Passed)
                diagnostics.Add("generated_source.tiny_loop_failed");

            var regeneratedMvp = new GeneratedPackageMvpService(_validator).Generate(new GeneratedPackageMvpRequest
            {
                SourcePlan = plan,
                RulePack = regeneratedRulePack.RulePack,
                RulePackValidationReport = regeneratedRulePack.ValidationReport,
                TinyLoopResult = regeneratedTinyLoop
            });
            var regeneratedNamespacedMvp = _overlayService.NamespaceGeneratedPackage(regeneratedMvp.PackageJson);
            if (!string.Equals(regeneratedNamespacedMvp, generatedMvpJson, StringComparison.Ordinal))
                diagnostics.Add("generated_source.mvp_regeneration_mismatch");

            diagnostics.AddRange(_overlayService.ValidatePackageRecords(generatedBaseJson, overlay, includeBaseline: true));
            var validation = _validator.Validate(generatedBase);
            diagnostics.AddRange(validation.Issues
                .Where(issue => issue.Severity is Domain.Validation.ValidationSeverity.Error
                    or Domain.Validation.ValidationSeverity.Critical)
                .Select(issue => "generated_source.package_invalid:" + issue.Code));
            if (!string.Equals(HashText(generatedMvpJson), source.GeneratedMvpPackageSha256, StringComparison.Ordinal)
                || !string.Equals(HashText(generatedBaseJson), source.GeneratedBasePackageSha256, StringComparison.Ordinal)
                || !string.Equals(HashText(overlayJson), source.GeneratedOverlaySha256, StringComparison.Ordinal))
                diagnostics.Add("generated_source.sidecar_hash_mismatch");

            diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
            return new SeededGeneratedProjectSourceValidationResult
            {
                Present = true,
                Passed = diagnostics.Count == 0,
                Status = diagnostics.Count == 0 ? "CURRENT" : "INVALID",
                SourcePath = sourcePath,
                Source = source,
                Overlay = overlay,
                GeneratedBasePackage = generatedBase,
                GeneratedMvpPackage = generatedMvp,
                Diagnostics = diagnostics
            };
        }
        catch (JsonException)
        {
            return Failed(sourcePath, diagnostics.Append("generated_source.invalid_json"));
        }
        catch (IOException exception)
        {
            return Failed(sourcePath, diagnostics.Append("generated_source.unreadable:" + exception.GetType().Name));
        }
        catch (InvalidOperationException exception)
        {
            return Failed(sourcePath, diagnostics.Append(exception.Message));
        }
    }

    public static GeneratedProjectCounts Counts(ProceduralGeneratedGamePlan plan) => new()
    {
        Regions = plan.World.Regions.Count,
        Factions = plan.Factions.Count,
        Actors = plan.ActorSeeds.Count,
        ItemsAndResources = plan.ItemResourceSeeds.Count,
        Encounters = plan.EncounterSeeds.Count,
        QuestEvents = plan.QuestEventSeeds.Count
    };

    public static GeneratedProjectTinyLoopFacts BuildTinyLoopFacts(
        ProceduralGeneratedGamePlan plan,
        FormulaEffectActionRulePack rulePack,
        TinyGeneratedRuntimeLoopResult tinyLoop)
    {
        var initial = HashText("seeded_generated_project_initial_state_v1\n"
                               + plan.Metadata.DeterministicHash + "\n" + rulePack.Metadata.DeterministicHash);
        var rewardOrCost = tinyLoop.State.InventoryItemCounts.Values.Any(value => value != 0)
                           || tinyLoop.State.FactionReputationDeltas.Values.Any(value => value != 0)
                           || tinyLoop.State.QuestEventStates.Count > 0;
        return new GeneratedProjectTinyLoopFacts
        {
            Passed = !tinyLoop.Report.HasErrors && tinyLoop.Report.Steps.Count > 0,
            InitialStateHash = initial,
            FinalStateHash = tinyLoop.State.DeterministicHash,
            StepCount = tinyLoop.Report.Steps.Count,
            RewardOrCostObserved = rewardOrCost,
            StateChangeObserved = tinyLoop.Report.Steps.Count > 0
                                  && !string.Equals(initial, tinyLoop.State.DeterministicHash, StringComparison.Ordinal)
        };
    }

    private void ValidateVocabulary(SeededGeneratedProjectSourceRecord source, ICollection<string> diagnostics)
    {
        if (!string.Equals(source.SchemaVersion, SeededGeneratedProjectVocabulary.SourceSchemaVersion, StringComparison.Ordinal)
            || !string.Equals(source.CreationKind, GameProjectCreationKinds.SeededGenerated, StringComparison.Ordinal))
            diagnostics.Add("generated_source.unsupported_schema");
        if (string.IsNullOrWhiteSpace(source.Seed)) diagnostics.Add("generated_source.seed_missing");
        if (!ProceduralGameGenerationModes.Supported.Contains(source.Mode)) diagnostics.Add("generated_source.mode_unsupported");
        if (!_presetOptions.GetPresets().Any(preset => preset.PresetId == source.PresetId)) diagnostics.Add("generated_source.preset_unknown");
        if (!GeneratedProjectMechanicsProfiles.Supported.Contains(source.MechanicsProfileId, StringComparer.Ordinal))
            diagnostics.Add("generated_source.profile_unknown");
    }

    private static void ValidateSidecars(
        string generationRoot,
        SeededGeneratedProjectSourceRecord source,
        ICollection<string> diagnostics)
    {
        foreach (var fileName in SeededGeneratedProjectVocabulary.RequiredSidecarFileNames)
        {
            var path = Resolve(generationRoot, fileName);
            if (!File.Exists(path))
            {
                diagnostics.Add("generated_source.sidecar_missing:" + fileName);
                continue;
            }
            if (!source.SidecarSha256.TryGetValue(fileName, out var expected)
                || !string.Equals(HashFile(path), expected, StringComparison.Ordinal))
                diagnostics.Add("generated_source.sidecar_hash_mismatch:" + fileName);
        }
        if (source.SidecarSha256.Count != SeededGeneratedProjectVocabulary.RequiredSidecarFileNames.Count)
            diagnostics.Add("generated_source.sidecar_inventory_mismatch");
        Match(source.PlanSha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.PlanJsonFileName, diagnostics);
        Match(source.RulePackSha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.RulePackJsonFileName, diagnostics);
        Match(source.TinyLoopStateSha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.TinyLoopStateJsonFileName, diagnostics);
        Match(source.GeneratedMvpPackageSha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.GeneratedMvpPackageJsonFileName, diagnostics);
        Match(source.GeneratedOverlaySha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.GeneratedOverlayJsonFileName, diagnostics);
        Match(source.GeneratedBasePackageSha256, source.SidecarSha256, SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName, diagnostics);
    }

    private static void Match(
        string directHash,
        IReadOnlyDictionary<string, string> sidecars,
        string fileName,
        ICollection<string> diagnostics)
    {
        if (!sidecars.TryGetValue(fileName, out var value)
            || !string.Equals(directHash, value, StringComparison.Ordinal))
            diagnostics.Add("generated_source.sidecar_hash_mismatch:" + fileName);
    }

    private static void ValidateCounts(
        GeneratedProjectCounts expected,
        ProceduralGeneratedGamePlan plan,
        ICollection<string> diagnostics)
    {
        if (expected != Counts(plan)) diagnostics.Add("generated_source.count_mismatch");
    }

    private static T Deserialize<T>(string json, string code) =>
        JsonSerializer.Deserialize<T>(json, JsonOptions)
        ?? throw new InvalidOperationException(code);

    private static string Read(string root, string fileName) => File.ReadAllText(Resolve(root, fileName), Encoding.UTF8);

    private static string Resolve(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.Equals(fullRoot, comparison) && !path.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("generated_source.path_escape");
        return path;
    }

    internal static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    internal static string HashText(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static SeededGeneratedProjectSourceValidationResult Failed(
        string sourcePath,
        IEnumerable<string> diagnostics,
        SeededGeneratedProjectSourceRecord? source = null) => new()
    {
        Present = true,
        Passed = false,
        Status = "INVALID",
        SourcePath = sourcePath,
        Source = source,
        Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList()
    };
}
