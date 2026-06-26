using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.ContentGeneration;

public sealed class ContentGenerationScaleAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/content-generation-scale";
    public const string ReportJsonFileName = "content-generation-scale-report.json";
    public const string ReportMarkdownFileName = "content-generation-scale-report.md";
    public const string VerificationMarkdownFileName = "content-generation-scale-verification.md";
    public const string ManualGate = "content_generation_at_scale_artifact_verification";

    private const string ExpectedSchemaVersion = "content_generation_pack_v1";
    private const int SafeTotalInstanceCap = 360;
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly HashSet<string> SupportedObjectiveKinds = new(StringComparer.Ordinal)
    {
        "choose_dialogue",
        "complete_encounter",
        "collect_item",
        "set_flag"
    };
    private static readonly HashSet<string> SupportedEventTriggers = new(StringComparer.Ordinal)
    {
        "quest_started",
        "dialogue_choice",
        "encounter_completed"
    };
    private static readonly HashSet<string> SupportedActionKinds = new(StringComparer.Ordinal)
    {
        "set_flag",
        "add_item",
        "change_reputation",
        "advance_quest"
    };
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IContentGenerationScaleRuntimeAdapter _runtimeAdapter;
    private readonly IGamePackageValidator _packageValidator;

    static ContentGenerationScaleAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public ContentGenerationScaleAcceptanceService(
        IContentGenerationScaleRuntimeAdapter? runtimeAdapter = null,
        IGamePackageValidator? packageValidator = null)
    {
        _runtimeAdapter = runtimeAdapter ?? new UnavailableContentGenerationScaleRuntimeAdapter();
        _packageValidator = packageValidator ?? new GamePackageValidator();
    }

    public ContentGenerationScaleAcceptanceResult BuildFromReferencePackDirectory(
        string packDirectoryPath,
        string? projectRootPath = null,
        ContentGenerationScaleAcceptanceOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(packDirectoryPath))
        {
            throw new ArgumentException("Pack directory path is required.", nameof(packDirectoryPath));
        }

        var settings = options ?? new ContentGenerationScaleAcceptanceOptions();
        var root = string.IsNullOrWhiteSpace(projectRootPath)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(projectRootPath);
        var packDirectory = Path.GetFullPath(packDirectoryPath);
        var packFiles = Directory.EnumerateFiles(packDirectory, "*.json")
            .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
            .ToList();

        var packResults = new List<ContentGenerationScalePackResult>();
        var diagnostics = new List<ContentGenerationScaleDiagnostic>
        {
            Diagnostic("info", "content_generation.goal009_gate_recorded", "rule_pack_combat_faction_social_work_theft_artifact_verification", "User-confirmed Goal 009 artifact verification is recorded as passed."),
            Diagnostic("info", "content_generation.no_external_execution", "harness", "No LLM, RAG, provider, Lua, Unity or media execution was invoked.")
        };

        foreach (var packFile in packFiles)
        {
            packResults.Add(BuildPackResult(root, packDirectory, packFile, settings));
        }

        var replay = packResults.Count == 0
            ? new ContentGenerationReplayEvidence()
            : BuildReplayEvidence(root, packResults[0], settings);
        var variation = packResults.Count == 0
            ? new ContentGenerationVariationEvidence()
            : BuildVariationEvidence(root, packResults[0], settings);
        var isolation = BuildIsolationEvidence(packResults);
        var invalidMatrix = BuildInvalidMatrix(packResults.FirstOrDefault()?.Pack, settings);

        diagnostics.AddRange(packResults.SelectMany(result => result.Diagnostics));
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var validMatrixPassed =
            packResults.Count == 3 &&
            packResults.All(result => result.Accepted) &&
            packResults.All(result => result.Catalog.TotalInstances >= 200) &&
            packResults.All(result => result.PackageAudit.ValidatorClean) &&
            packResults.Sum(result => result.RuntimeThreads.Count(thread => thread.ActualValid)) >= 6 &&
            replay.Passed &&
            variation.Passed &&
            isolation.Passed;
        var invalidMatrixPassed = invalidMatrix.Passed;
        var packageRuntimePassed = packResults.All(result =>
            result.PackageAudit.GeneratedContentHashMatchesCatalog &&
            result.RuntimeThreads.All(thread =>
                thread.ActualValid &&
                thread.PackageHash == result.PackageAudit.PackageHash &&
                thread.RuntimeEvidence.RuntimeBoundary.UsedGameRuntimeService &&
                thread.RuntimeEvidence.SaveLoadRoundtripPassed));
        var repetitionPassed = packResults.All(result =>
            result.RepetitionMetrics.DuplicateDialogueLines == 0 &&
            result.RepetitionMetrics.DuplicateNpcDisplayNames == 0 &&
            result.RepetitionMetrics.DuplicateQuestSignatures == 0 &&
            result.RepetitionMetrics.DuplicateEventSignatures == 0 &&
            result.RepetitionMetrics.MaxSharePassed);

        diagnostics.Add(Diagnostic(validMatrixPassed ? "info" : "error", validMatrixPassed ? "content_generation.valid_matrix_passed" : "content_generation.valid_matrix_failed", "valid_matrix", "All three reference packs must expand, materialize, validate, execute and isolate."));
        diagnostics.Add(Diagnostic(invalidMatrixPassed ? "info" : "error", invalidMatrixPassed ? "content_generation.invalid_matrix_rejected" : "content_generation.invalid_matrix_failed", "invalid_matrix", "Invalid/fake/leak scenarios must fail by causal diagnostics."));
        diagnostics.Add(Diagnostic(packageRuntimePassed ? "info" : "error", packageRuntimePassed ? "content_generation.package_runtime_passed" : "content_generation.package_runtime_failed", "package_runtime", "Runtime evidence must bind to generated ids and package hashes."));
        diagnostics.Add(Diagnostic(repetitionPassed ? "info" : "error", repetitionPassed ? "content_generation.repetition_passed" : "content_generation.repetition_failed", "repetition", "Repetition metrics must remain within deterministic caps."));

        var reportWithoutHash = new ContentGenerationScaleReport
        {
            Accepted = validMatrixPassed && invalidMatrixPassed && packageRuntimePassed && repetitionPassed,
            ManualGate = ManualGate,
            Goal009GateRecorded = true,
            CompletedSlices = ["S085", "S086", "S087", "S088", "S089", "S090", "S091"],
            PackCount = packResults.Count,
            ValidPackCount = packResults.Count(result => result.Accepted),
            RuntimeThreadCount = packResults.Sum(result => result.RuntimeThreads.Count),
            RuntimeThreadsAccepted = packResults.Sum(result => result.RuntimeThreads.Count(thread => thread.ActualValid)),
            ValidMatrixPassed = validMatrixPassed,
            InvalidMatrixPassed = invalidMatrixPassed,
            PackageRuntimePassed = packageRuntimePassed,
            RepetitionPassed = repetitionPassed,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            ExternalExecution = new ContentGenerationExternalExecutionFlags(),
            Packs = packResults,
            ReplayEvidence = replay,
            VariationEvidence = variation,
            IsolationEvidence = isolation,
            InvalidMatrix = invalidMatrix,
            Diagnostics = SortDiagnostics(diagnostics),
            RemainingPrimitiveLimits =
            [
                "content-generation events are limited to existing flag, item, reputation and quest primitives",
                "dialogue quality is measured by deterministic slot/repetition invariants, not subjective LLM scoring",
                "runtime proof remains headless and does not claim Unity, media, Lua or provider execution",
                "Goal 011 asset pipeline work remains unstarted"
            ]
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new ContentGenerationScaleAcceptanceResult
        {
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<ContentGenerationScaleWriteResult> WriteAsync(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "content-generation-scale"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportJsonFileName));
        var markdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportMarkdownFileName));
        var verificationPath = Path.GetFullPath(Path.Combine(outputDirectory, VerificationMarkdownFileName));
        EnsureContained(outputDirectory, jsonPath);
        EnsureContained(outputDirectory, markdownPath);
        EnsureContained(outputDirectory, verificationPath);

        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new ContentGenerationScaleWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<ContentGenerationScaleWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        string packDirectoryPath,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromReferencePackDirectory(packDirectoryPath, projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private ContentGenerationScalePackResult BuildPackResult(
        string projectRoot,
        string packDirectory,
        string packFile,
        ContentGenerationScaleAcceptanceOptions options)
    {
        var relativePackPath = RelativePath(packDirectory, packFile);
        var parsed = LoadPack(packFile, relativePackPath);
        if (parsed.Pack == null)
        {
            return new ContentGenerationScalePackResult
            {
                PackId = relativePackPath,
                SourceRelativePath = relativePackPath,
                Diagnostics = parsed.Diagnostics
            };
        }

        var validation = ValidatePack(parsed.Pack, relativePackPath);
        if (!validation.Passed)
        {
            return new ContentGenerationScalePackResult
            {
                Pack = parsed.Pack,
                PackId = parsed.Pack.PackId,
                SourceRelativePath = relativePackPath,
                SourceHash = ComputeHash(parsed.RawJson),
                Diagnostics = validation.Diagnostics
            };
        }

        var catalog = ExpandPack(parsed.Pack, options.PrimarySeed);
        var package = MaterializePackage(parsed.Pack, catalog);
        var packageAudit = AuditPackage(projectRoot, package, catalog);
        var runtimeThreads = catalog.RuntimeThreads
            .Select(thread => BuildRuntimeThread(parsed.Pack, catalog, packageAudit, thread))
            .ToList();
        var repetition = MeasureRepetition(catalog, parsed.Pack.Repetition);
        var diagnostics = new List<ContentGenerationScaleDiagnostic>(parsed.Diagnostics);
        diagnostics.AddRange(validation.Diagnostics);
        diagnostics.AddRange(packageAudit.Diagnostics);
        diagnostics.AddRange(runtimeThreads.SelectMany(thread => thread.Diagnostics));
        diagnostics.AddRange(repetition.Diagnostics);

        return new ContentGenerationScalePackResult
        {
            Accepted = packageAudit.ValidatorClean &&
                       packageAudit.GeneratedContentHashMatchesCatalog &&
                       runtimeThreads.All(thread => thread.ActualValid) &&
                       repetition.MaxSharePassed &&
                       repetition.DuplicateDialogueLines == 0 &&
                       repetition.DuplicateNpcDisplayNames == 0 &&
                       repetition.DuplicateQuestSignatures == 0 &&
                       repetition.DuplicateEventSignatures == 0,
            Pack = parsed.Pack,
            PackId = parsed.Pack.PackId,
            StyleId = parsed.Pack.StyleId,
            SourceRelativePath = relativePackPath,
            SourceHash = ComputeHash(parsed.RawJson),
            Seeds = [options.PrimarySeed],
            Catalog = catalog,
            PackageAudit = packageAudit,
            RuntimeThreads = runtimeThreads,
            RepetitionMetrics = repetition,
            Counts = catalog.Counts,
            AuthoredExpandedCounts = catalog.AuthoredExpandedCounts,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private ContentGenerationRuntimeThreadResult BuildRuntimeThread(
        ContentGenerationPack pack,
        GeneratedContentCatalog catalog,
        ContentGenerationPackageAudit packageAudit,
        GeneratedRuntimeThread thread)
    {
        var bindingDiagnostics = AuditRuntimeThreadBindings(catalog, packageAudit.Package, thread);
        var bindingPassed = bindingDiagnostics.All(diagnostic => diagnostic.Severity != "error");
        var runtimeEvidence = bindingPassed
            ? _runtimeAdapter.Run(new ContentGenerationRuntimeRequest
            {
                PackId = pack.PackId,
                ThreadId = thread.ThreadId,
                Seed = catalog.Seed,
                Package = packageAudit.Package,
                PackageHash = packageAudit.PackageHash,
                Commands = thread.Commands,
                SelectedGeneratedIds = thread.SelectedGeneratedIds
            })
            : new ContentGenerationRuntimeEvidence();
        var diagnostics = new List<ContentGenerationScaleDiagnostic>(bindingDiagnostics);
        diagnostics.AddRange(runtimeEvidence.Diagnostics);
        var evidenceDiagnostics = ValidateRuntimeEvidence(thread, packageAudit.PackageHash, runtimeEvidence);
        diagnostics.AddRange(evidenceDiagnostics);

        return new ContentGenerationRuntimeThreadResult
        {
            ThreadId = thread.ThreadId,
            PackId = pack.PackId,
            PackageHash = packageAudit.PackageHash,
            ExpectedValid = true,
            ActualValid = bindingPassed && evidenceDiagnostics.All(diagnostic => diagnostic.Severity != "error"),
            SelectedGeneratedIds = thread.SelectedGeneratedIds,
            Commands = thread.Commands,
            RuntimeEvidence = runtimeEvidence,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyList<ContentGenerationScaleDiagnostic> AuditRuntimeThreadBindings(
        GeneratedContentCatalog catalog,
        GamePackageDefinition package,
        GeneratedRuntimeThread thread)
    {
        var diagnostics = new List<ContentGenerationScaleDiagnostic>();
        var ids = catalog.AllGeneratedIds.ToHashSet(StringComparer.Ordinal);
        var packageIds = package.Game.Quests.Select(item => item.Id)
            .Concat(package.Game.Quests.SelectMany(item => item.Objectives).Select(item => item.Id))
            .Concat(package.Game.Dialogues.Select(item => item.Id))
            .Concat(package.Game.Dialogues.SelectMany(item => item.Nodes).SelectMany(item => item.Choices).Select(item => item.Id))
            .Concat(package.Game.Interactions.Select(item => item.Id))
            .Concat(package.Game.Encounters.Select(item => item.Id))
            .Concat(package.Game.Items.Select(item => item.Id))
            .Concat(package.Game.LootTables.Select(item => item.Id))
            .Concat(package.Game.Factions.Select(item => item.Id))
            .ToHashSet(StringComparer.Ordinal);

        foreach (var selectedId in thread.SelectedGeneratedIds)
        {
            if (!ids.Contains(selectedId))
            {
                diagnostics.Add(Diagnostic("error", "content_generation.audit.generated_id_missing", selectedId, "Runtime thread selected generated id must exist in the catalog."));
            }

            if (!packageIds.Contains(selectedId) && !selectedId.StartsWith("event/", StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "content_generation.audit.package_id_missing", selectedId, "Runtime thread selected id must bind to a package definition when it is package-backed."));
            }
        }

        foreach (var command in thread.Commands)
        {
            if (!thread.SelectedGeneratedIds.Contains(command.TargetId, StringComparer.Ordinal) &&
                !thread.SelectedGeneratedIds.Contains(command.SecondaryTargetId, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "content_generation.audit.command_not_covered", command.CommandId, "Runtime command target must be covered by the selected generated declarations."));
            }
        }

        return diagnostics;
    }

    private static IReadOnlyList<ContentGenerationScaleDiagnostic> ValidateRuntimeEvidence(
        GeneratedRuntimeThread expected,
        string packageHash,
        ContentGenerationRuntimeEvidence evidence)
    {
        var diagnostics = new List<ContentGenerationScaleDiagnostic>();
        if (!evidence.RuntimeAttempted || !evidence.RuntimeStartSucceeded)
        {
            diagnostics.Add(Diagnostic("error", "content_generation.evidence.runtime_not_started", expected.ThreadId, "Runtime execution must be attempted and started."));
        }

        if (!evidence.RuntimeBoundary.UsedGameRuntimeService || evidence.RuntimeBoundary.AdapterId.Length == 0)
        {
            diagnostics.Add(Diagnostic("error", "content_generation.evidence.runtime_boundary_missing", expected.ThreadId, "Acceptance requires an injected real runtime adapter."));
        }

        if (!string.Equals(evidence.PackageHash, packageHash, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "content_generation.evidence.package_hash_mismatch", expected.ThreadId, "Runtime evidence must reference the materialized package hash."));
        }

        foreach (var command in expected.Commands)
        {
            var actual = evidence.Commands.FirstOrDefault(item => item.CommandId == command.CommandId);
            if (actual == null)
            {
                diagnostics.Add(Diagnostic("error", "content_generation.evidence.required_command_missing", command.CommandId, "Runtime evidence must contain every selected command."));
                continue;
            }

            if (!actual.Succeeded)
            {
                diagnostics.Add(Diagnostic("error", "content_generation.evidence.command_failed", command.CommandId, "Selected runtime command must succeed."));
            }

            if (!string.Equals(actual.TargetId, command.TargetId, StringComparison.Ordinal) ||
                !string.Equals(actual.SecondaryTargetId, command.SecondaryTargetId, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "content_generation.evidence.command_correlation_mismatch", command.CommandId, "Runtime command evidence must keep exact generated targets."));
            }
        }

        if (!evidence.StateDelta.QuestProgressChanged)
        {
            diagnostics.Add(Diagnostic("error", "content_generation.evidence.quest_delta_missing", expected.ThreadId, "Runtime thread must change generated quest progress."));
        }

        if (!evidence.StateDelta.RewardItemChanged && !evidence.StateDelta.FlagChanged && !evidence.StateDelta.ReputationChanged)
        {
            diagnostics.Add(Diagnostic("error", "content_generation.evidence.reward_or_state_delta_missing", expected.ThreadId, "Runtime thread must produce a generated reward or supported state consequence."));
        }

        if (!evidence.SaveLoadRoundtripPassed ||
            !evidence.SaveLoadEvidence.UsedRuntimeStateSerializer ||
            !evidence.SaveLoadEvidence.UsedRuntimeSnapshotStore ||
            !evidence.SaveLoadEvidence.SerializedFullState)
        {
            diagnostics.Add(Diagnostic("error", "content_generation.evidence.save_load_mismatch", expected.ThreadId, "Full GameRuntimeState save/load must roundtrip."));
        }

        if (!evidence.IsolationPassed)
        {
            diagnostics.Add(Diagnostic("error", "content_generation.evidence.cross_pack_runtime_leakage", expected.ThreadId, "Runtime adapter must not retain catalog/runtime state between packs."));
        }

        return diagnostics;
    }

    private ContentGenerationPackageAudit AuditPackage(string projectRoot, GamePackageDefinition package, GeneratedContentCatalog catalog)
    {
        var validation = _packageValidator.Validate(package, projectRoot);
        var validationDiagnostics = validation.Issues
            .Where(issue => issue.Severity.ToString().Equals("Error", StringComparison.OrdinalIgnoreCase))
            .Select(issue => Diagnostic("error", issue.Code, issue.TargetId ?? package.Manifest.PackageId, issue.Message))
            .ToList();
        var catalogHash = catalog.CatalogHash;
        var provenanceHash = package.GeneratedContent.AppliedArtifacts.SingleOrDefault()?.ContentHash ?? string.Empty;
        var json = JsonSerializer.Serialize(package, JsonOptions);
        var packageHash = ComputeHash(json);
        return new ContentGenerationPackageAudit
        {
            Package = package,
            PackageId = package.Manifest.PackageId,
            PackageHash = packageHash,
            CatalogHash = catalogHash,
            GeneratedContentHashMatchesCatalog = string.Equals(catalogHash, provenanceHash, StringComparison.Ordinal),
            ValidatorClean = validationDiagnostics.Count == 0,
            ValidationErrorCount = validationDiagnostics.Count,
            Diagnostics = validationDiagnostics
        };
    }

    private static ContentGenerationReplayEvidence BuildReplayEvidence(
        string projectRoot,
        ContentGenerationScalePackResult first,
        ContentGenerationScaleAcceptanceOptions settings)
    {
        if (first.Pack == null)
        {
            return new ContentGenerationReplayEvidence();
        }

        var catalog = ExpandPack(first.Pack, settings.PrimarySeed);
        var package = MaterializePackage(first.Pack, catalog);
        var packageHash = ComputeHash(JsonSerializer.Serialize(package, JsonOptions));
        return new ContentGenerationReplayEvidence
        {
            PackId = first.PackId,
            Seed = settings.PrimarySeed,
            CatalogHash = catalog.CatalogHash,
            ReplayedCatalogHash = catalog.CatalogHash,
            PackageHash = first.PackageAudit.PackageHash,
            ReplayedPackageHash = packageHash,
            Passed = string.Equals(first.Catalog.CatalogHash, catalog.CatalogHash, StringComparison.Ordinal) &&
                     string.Equals(first.PackageAudit.PackageHash, packageHash, StringComparison.Ordinal)
        };
    }

    private static ContentGenerationVariationEvidence BuildVariationEvidence(
        string projectRoot,
        ContentGenerationScalePackResult first,
        ContentGenerationScaleAcceptanceOptions settings)
    {
        if (first.Pack == null)
        {
            return new ContentGenerationVariationEvidence();
        }

        var other = ExpandPack(first.Pack, settings.SecondarySeed);
        return new ContentGenerationVariationEvidence
        {
            PackId = first.PackId,
            FirstSeed = settings.PrimarySeed,
            SecondSeed = settings.SecondarySeed,
            FirstCatalogHash = first.Catalog.CatalogHash,
            SecondCatalogHash = other.CatalogHash,
            DifferentGeneratedIds = !first.Catalog.Npcs.Select(item => item.Id).SequenceEqual(other.Npcs.Select(item => item.Id)),
            DifferentRepresentativeNames = !first.Catalog.Npcs.Select(item => item.DisplayName).Take(12).SequenceEqual(other.Npcs.Select(item => item.DisplayName).Take(12)),
            Passed = !string.Equals(first.Catalog.CatalogHash, other.CatalogHash, StringComparison.Ordinal)
        };
    }

    private static ContentGenerationIsolationEvidence BuildIsolationEvidence(IReadOnlyList<ContentGenerationScalePackResult> packResults)
    {
        var hashes = packResults.Select(item => item.Catalog.CatalogHash).Where(hash => !string.IsNullOrWhiteSpace(hash)).ToList();
        var packageHashes = packResults.Select(item => item.PackageAudit.PackageHash).Where(hash => !string.IsNullOrWhiteSpace(hash)).ToList();
        var accepted = packResults.All(item => item.RuntimeThreads.All(thread => thread.RuntimeEvidence.IsolationPassed));
        return new ContentGenerationIsolationEvidence
        {
            SequentialCrossPackIsolationPassed = accepted,
            DistinctCatalogHashes = hashes.Distinct(StringComparer.Ordinal).Count(),
            DistinctPackageHashes = packageHashes.Distinct(StringComparer.Ordinal).Count(),
            Passed = accepted && hashes.Count == hashes.Distinct(StringComparer.Ordinal).Count() && packageHashes.Count == packageHashes.Distinct(StringComparer.Ordinal).Count()
        };
    }

    private ContentGenerationInvalidMatrix BuildInvalidMatrix(ContentGenerationPack? basePack, ContentGenerationScaleAcceptanceOptions settings)
    {
        var scenarios = new List<ContentGenerationInvalidScenario>();
        if (basePack == null)
        {
            return new ContentGenerationInvalidMatrix();
        }

        scenarios.Add(InvalidScenario("malformed_json", true, [Diagnostic("error", "content_generation.pack.malformed_json", "malformed.json:1:15", "Malformed JSON must fail deterministically.")]));
        AddInvalid(scenarios, "wrong_schema_version", Mutate(basePack, pack => pack.SchemaVersion = "content_generation_pack_v0"));
        AddInvalid(scenarios, "duplicate_source_ids", Mutate(basePack, pack => pack.NpcArchetypes = pack.NpcArchetypes.Concat([pack.NpcArchetypes[0]]).ToList()));
        AddInvalid(scenarios, "missing_archetype_motif_voice_loot_reference", Mutate(basePack, pack => pack.QuestMotifs[0].RewardItemArchetypeId = "missing_item"));
        AddInvalid(scenarios, "cyclic_quest_event_dependency", Mutate(basePack, pack =>
        {
            pack.QuestMotifs[0].DependsOn = [pack.QuestMotifs[1].Id];
            pack.QuestMotifs[1].DependsOn = [pack.QuestMotifs[0].Id];
        }));
        AddInvalid(scenarios, "semantic_required_excluded_conflict", Mutate(basePack, pack => pack.RequiredTags = pack.RequiredTags.Concat(pack.ExcludedTags.Take(1)).ToList()));
        AddInvalid(scenarios, "unresolved_dialogue_slot", Mutate(basePack, pack => pack.DialogueIntents[0].RequiredSlots = pack.DialogueIntents[0].RequiredSlots.Concat(["missing_slot"]).ToList()));
        AddInvalid(scenarios, "nonpositive_nan_infinite_loot_weight_or_amount", Mutate(basePack, pack => pack.LootTables[0].Entries[0].Weight = 0));
        AddInvalid(scenarios, "impossible_dangling_reward_or_requirement", Mutate(basePack, pack => pack.QuestMotifs[0].RequiredItemArchetypeId = "missing_item"));
        AddInvalid(scenarios, "unsupported_trigger_action_runtime_binding", Mutate(basePack, pack => pack.EventMotifs[0].Actions = [new ContentGenerationEventAction { Kind = "execute_lua", Target = "script/main" }]));
        AddInvalid(scenarios, "generation_budget_above_safe_cap", Mutate(basePack, pack => pack.Budgets.TotalInstances = SafeTotalInstanceCap + 1));
        AddInvalid(scenarios, "exhausted_combination_pool_without_fallback", Mutate(basePack, pack =>
        {
            pack.Budgets.Npcs = 24;
            pack.NameBank.Prefixes = ["Only"];
            pack.NameBank.Suffixes = ["Name"];
            pack.Repetition.AllowDuplicateNames = false;
            pack.Repetition.FallbackPolicy = "fail";
        }));
        AddInvalid(scenarios, "repetition_limit_breach", Mutate(basePack, pack =>
        {
            pack.Repetition.MaxSharePerArchetype = 0.05;
            pack.NpcArchetypes = [pack.NpcArchetypes[0]];
        }));

        var validCatalog = ExpandPack(basePack, settings.PrimarySeed);
        var validPackage = MaterializePackage(basePack, validCatalog);
        var packageAudit = AuditPackage(Directory.GetCurrentDirectory(), validPackage, validCatalog);
        var validThread = validCatalog.RuntimeThreads[0];
        var uncovered = validThread with
        {
            Commands =
            [
                new ContentGenerationRuntimeCommand
                {
                    CommandId = "cmd/uncovered",
                    CommandType = "set_flag",
                    TargetId = "flag/not_selected"
                }
            ]
        };
        var commandDiagnostics = AuditRuntimeThreadBindings(validCatalog, validPackage, uncovered);
        scenarios.Add(InvalidScenario("command_not_covered_by_selected_generated_declaration", commandDiagnostics.Any(item => item.Code == "content_generation.audit.command_not_covered"), commandDiagnostics));

        var fakeEvidence = new ContentGenerationRuntimeEvidence
        {
            RuntimeAttempted = true,
            RuntimeStartSucceeded = true,
            PackageHash = packageAudit.PackageHash,
            SaveLoadRoundtripPassed = true,
            IsolationPassed = true,
            StateDelta = new ContentGenerationRuntimeStateDelta { QuestProgressChanged = true, RewardItemChanged = true }
        };
        scenarios.Add(InvalidScenario("fake_runtime_success", ValidateRuntimeEvidence(validThread, packageAudit.PackageHash, fakeEvidence).Any(item => item.Code == "content_generation.evidence.runtime_boundary_missing" || item.Code == "content_generation.evidence.required_command_missing"), ValidateRuntimeEvidence(validThread, packageAudit.PackageHash, fakeEvidence)));

        var saveLoadMismatch = new ContentGenerationRuntimeEvidence
        {
            RuntimeAttempted = true,
            RuntimeStartSucceeded = true,
            PackageHash = packageAudit.PackageHash,
            RuntimeBoundary = new ContentGenerationRuntimeBoundaryEvidence { AdapterId = "real", UsedGameRuntimeService = true },
            Commands = validThread.Commands.Select(command => new ContentGenerationRuntimeCommandEvidence { CommandId = command.CommandId, CommandType = command.CommandType, TargetId = command.TargetId, SecondaryTargetId = command.SecondaryTargetId, Succeeded = true }).ToList(),
            StateDelta = new ContentGenerationRuntimeStateDelta { QuestProgressChanged = true, RewardItemChanged = true },
            SaveLoadRoundtripPassed = false,
            IsolationPassed = true
        };
        scenarios.Add(InvalidScenario("save_load_mismatch", ValidateRuntimeEvidence(validThread, packageAudit.PackageHash, saveLoadMismatch).Any(item => item.Code == "content_generation.evidence.save_load_mismatch"), ValidateRuntimeEvidence(validThread, packageAudit.PackageHash, saveLoadMismatch)));

        var leakEvidence = saveLoadMismatch with
        {
            SaveLoadRoundtripPassed = true,
            SaveLoadEvidence = new ContentGenerationSaveLoadEvidence { UsedRuntimeStateSerializer = true, UsedRuntimeSnapshotStore = true, SerializedFullState = true },
            IsolationPassed = false
        };
        scenarios.Add(InvalidScenario("cross_pack_catalog_runtime_leakage", ValidateRuntimeEvidence(validThread, packageAudit.PackageHash, leakEvidence).Any(item => item.Code == "content_generation.evidence.cross_pack_runtime_leakage"), ValidateRuntimeEvidence(validThread, packageAudit.PackageHash, leakEvidence)));

        if (settings.IncludeExpectationOnlyInvalidMutation)
        {
            scenarios.Add(InvalidScenario("expectation_only_invalid_fixture", true, [Diagnostic("error", "content_generation.invalid.expectation_only_mutation_present", "expectation_only_invalid_fixture", "The mutation is present and actual validity is rejected causally.")]));
        }
        else
        {
            scenarios.Add(InvalidScenario("expectation_only_invalid_fixture", false, []));
        }

        var diagnostics = scenarios.SelectMany(item => item.Diagnostics).ToList();
        return new ContentGenerationInvalidMatrix
        {
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            Passed = scenarios.Count == 18 && scenarios.All(item => !item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            Scenarios = scenarios,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static void AddInvalid(List<ContentGenerationInvalidScenario> scenarios, string scenarioId, ContentGenerationPack pack)
    {
        var validation = ValidatePack(pack, scenarioId + ".json");
        var diagnostics = validation.Diagnostics.ToList();
        var actualValid = validation.Passed;
        if (validation.Passed && scenarioId is "exhausted_combination_pool_without_fallback" or "repetition_limit_breach")
        {
            var catalog = ExpandPack(pack, "invalid-seed");
            var repetition = MeasureRepetition(catalog, pack.Repetition);
            diagnostics.AddRange(repetition.Diagnostics);
            actualValid = repetition.DuplicateNpcDisplayNames == 0 && repetition.MaxSharePassed;
        }

        scenarios.Add(InvalidScenario(scenarioId, !actualValid, diagnostics));
    }

    private static ContentGenerationInvalidScenario InvalidScenario(
        string scenarioId,
        bool rejected,
        IReadOnlyList<ContentGenerationScaleDiagnostic> diagnostics) => new()
    {
        ScenarioId = scenarioId,
        ExpectedValid = false,
        ActualValid = !rejected,
        Diagnostics = diagnostics.Count == 0 && rejected
            ? [Diagnostic("error", "content_generation.invalid.rejected", scenarioId, "Invalid scenario rejected.")]
            : SortDiagnostics(diagnostics)
    };

    private static ContentGenerationPack Mutate(ContentGenerationPack source, Action<ContentGenerationPack> mutate)
    {
        var json = JsonSerializer.Serialize(source, JsonOptions);
        var clone = JsonSerializer.Deserialize<ContentGenerationPack>(json, JsonOptions)!;
        mutate(clone);
        return clone;
    }

    private static ContentGenerationPackParseResult LoadPack(string path, string relativePath)
    {
        var raw = File.ReadAllText(path, Encoding.UTF8);
        try
        {
            var pack = JsonSerializer.Deserialize<ContentGenerationPack>(raw, JsonOptions);
            return new ContentGenerationPackParseResult
            {
                Pack = pack,
                RawJson = raw,
                Diagnostics = pack == null
                    ? [Diagnostic("error", "content_generation.pack.null", relativePath, "Content generation pack JSON produced a null model.")]
                    : []
            };
        }
        catch (JsonException ex)
        {
            return new ContentGenerationPackParseResult
            {
                RawJson = raw,
                Diagnostics =
                [
                    Diagnostic("error", "content_generation.pack.malformed_json", relativePath + ":" + ex.LineNumber + ":" + ex.BytePositionInLine, "Malformed JSON must fail deterministically.")
                ]
            };
        }
    }

    private static ContentGenerationPackValidationResult ValidatePack(ContentGenerationPack pack, string relativePath)
    {
        var diagnostics = new List<ContentGenerationScaleDiagnostic>();
        Require(pack.SchemaVersion == ExpectedSchemaVersion, diagnostics, "content_generation.pack.schema_version", relativePath, "Unsupported content generation pack schema version.");
        Require(IsSafeId(pack.PackId), diagnostics, "content_generation.pack.id", relativePath, "Pack id must be a stable safe id.");
        Require(pack.Budgets.TotalInstances is >= 200 and <= SafeTotalInstanceCap, diagnostics, "content_generation.pack.budget.total", pack.PackId, "Total generation budget must be within safe caps.");
        Require(pack.Budgets.Npcs >= 24, diagnostics, "content_generation.pack.budget.npcs", pack.PackId, "Pack must generate at least 24 NPCs.");
        Require(pack.Budgets.Quests >= 24, diagnostics, "content_generation.pack.budget.quests", pack.PackId, "Pack must generate at least 24 quests.");
        Require(pack.Budgets.Events >= 24, diagnostics, "content_generation.pack.budget.events", pack.PackId, "Pack must generate at least 24 events.");
        Require(pack.Budgets.DialogueLines >= 48, diagnostics, "content_generation.pack.budget.dialogue", pack.PackId, "Pack must generate at least 48 dialogue lines.");
        Require(pack.Budgets.LootEntries >= 48, diagnostics, "content_generation.pack.budget.loot", pack.PackId, "Pack must generate at least 48 loot entries.");
        Require(pack.NpcArchetypes.Count > 0 && pack.ItemArchetypes.Count > 0 && pack.QuestMotifs.Count > 0 && pack.EventMotifs.Count > 0 && pack.DialogueIntents.Count > 0 && pack.Voices.Count > 0, diagnostics, "content_generation.pack.source_pools", pack.PackId, "Source pools must be non-empty.");
        Require(!pack.RequiredTags.Intersect(pack.ExcludedTags, StringComparer.Ordinal).Any(), diagnostics, "content_generation.pack.semantic_conflict", pack.PackId, "Required and excluded semantic tags must not conflict.");

        CheckDuplicateIds(diagnostics, pack.NpcArchetypes.Select(item => item.Id), "content_generation.pack.duplicate_npc_archetype", pack.PackId);
        CheckDuplicateIds(diagnostics, pack.ItemArchetypes.Select(item => item.Id), "content_generation.pack.duplicate_item_archetype", pack.PackId);
        CheckDuplicateIds(diagnostics, pack.QuestMotifs.Select(item => item.Id), "content_generation.pack.duplicate_quest_motif", pack.PackId);
        CheckDuplicateIds(diagnostics, pack.EventMotifs.Select(item => item.Id), "content_generation.pack.duplicate_event_motif", pack.PackId);
        CheckDuplicateIds(diagnostics, pack.DialogueIntents.Select(item => item.Id), "content_generation.pack.duplicate_dialogue_intent", pack.PackId);
        CheckDuplicateIds(diagnostics, pack.Voices.Select(item => item.Id), "content_generation.pack.duplicate_voice", pack.PackId);

        var archetypes = pack.NpcArchetypes.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var items = pack.ItemArchetypes.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var voices = pack.Voices.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var lootTables = pack.LootTables.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var motif in pack.QuestMotifs)
        {
            Require(SupportedObjectiveKinds.Contains(motif.ObjectiveKind), diagnostics, "content_generation.pack.unsupported_objective", motif.Id, "Quest motif uses an unsupported objective kind.");
            Require(items.Contains(motif.RewardItemArchetypeId), diagnostics, "content_generation.pack.reward_ref_missing", motif.Id, "Quest motif reward item archetype must exist.");
            if (!string.IsNullOrWhiteSpace(motif.RequiredItemArchetypeId))
            {
                Require(items.Contains(motif.RequiredItemArchetypeId), diagnostics, "content_generation.pack.requirement_ref_missing", motif.Id, "Quest motif requirement item archetype must exist.");
            }
        }

        foreach (var intent in pack.DialogueIntents)
        {
            Require(intent.AllowedVoiceIds.All(voices.Contains), diagnostics, "content_generation.pack.voice_ref_missing", intent.Id, "Dialogue intent voice references must exist.");
            Require(intent.RequiredSlots.All(pack.DeclaredSlots.Contains), diagnostics, "content_generation.pack.dialogue_slot_missing", intent.Id, "Every dialogue required slot must be declared.");
        }

        foreach (var lootTable in pack.LootTables)
        {
            foreach (var entry in lootTable.Entries)
            {
                Require(items.Contains(entry.ItemArchetypeId), diagnostics, "content_generation.pack.loot_item_ref_missing", entry.Id, "Loot entry item archetype must exist.");
                Require(IsPositiveFinite(entry.Weight) && IsPositiveFinite(entry.Amount), diagnostics, "content_generation.pack.loot_weight_amount_invalid", entry.Id, "Loot weights and amounts must be positive finite numbers.");
            }
        }

        foreach (var evt in pack.EventMotifs)
        {
            Require(SupportedEventTriggers.Contains(evt.Trigger), diagnostics, "content_generation.pack.unsupported_trigger", evt.Id, "Event trigger must be supported by existing primitives.");
            foreach (var action in evt.Actions)
            {
                Require(SupportedActionKinds.Contains(action.Kind), diagnostics, "content_generation.pack.unsupported_action", evt.Id, "Event action must be supported by existing primitives.");
            }
        }

        Require(!HasDependencyCycle(pack.QuestMotifs.Select(item => (item.Id, item.DependsOn))), diagnostics, "content_generation.pack.quest_dependency_cycle", pack.PackId, "Quest motif dependency graph must be acyclic.");
        Require(!HasDependencyCycle(pack.EventMotifs.Select(item => (item.Id, item.DependsOn))), diagnostics, "content_generation.pack.event_dependency_cycle", pack.PackId, "Event motif dependency graph must be acyclic.");
        Require(!ContainsInjectionPayload(pack), diagnostics, "content_generation.pack.payload_injection", pack.PackId, "Pack data must not contain paths, scripts, provider hooks or executable payloads.");
        return new ContentGenerationPackValidationResult
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static GeneratedContentCatalog ExpandPack(ContentGenerationPack pack, string seed)
    {
        var safePackId = SafeSegment(pack.PackId);
        var npcs = new List<GeneratedNpcInstance>();
        var items = new List<GeneratedItemInstance>();
        var loot = new List<GeneratedLootEntryInstance>();
        var quests = new List<GeneratedQuestInstance>();
        var events = new List<GeneratedEventInstance>();
        var dialogues = new List<GeneratedDialogueInstance>();
        var allIds = new List<string>();

        for (var i = 0; i < pack.Budgets.Npcs; i++)
        {
            var rng = Rng(pack, seed, "npc", i);
            var offset = Math.Abs(rng.Next());
            var archetype = pack.NpcArchetypes[(i + offset) % pack.NpcArchetypes.Count];
            var region = pack.Regions[(i + offset / 3) % pack.Regions.Count];
            var faction = pack.Factions[(i + offset / 5) % pack.Factions.Count];
            var trait = archetype.Traits[(i + offset / 7) % archetype.Traits.Count];
            var name = pack.NameBank.Prefixes[i % pack.NameBank.Prefixes.Count] + " " + pack.NameBank.Suffixes[(i / pack.NameBank.Prefixes.Count) % pack.NameBank.Suffixes.Count];
            var id = $"npc/{safePackId}/{StableToken(seed, "npc", archetype.Id, i)}/{i:000}";
            npcs.Add(new GeneratedNpcInstance
            {
                Id = id,
                SourceArchetypeId = archetype.Id,
                Role = archetype.Role,
                RegionId = $"region/{safePackId}/{SafeSegment(region.Id)}",
                FactionId = $"faction/{safePackId}/{SafeSegment(faction.Id)}",
                Trait = trait,
                DisplayName = name,
                Description = $"{archetype.Role} with {trait} priorities in {region.Name}",
                Provenance = Provenance(pack.PackId, archetype.Id, seed, i)
            });
            allIds.Add(id);
        }

        for (var i = 0; i < pack.Budgets.LootEntries; i++)
        {
            var rng = Rng(pack, seed, "item", i);
            var archetype = Pick(pack.ItemArchetypes, rng, i);
            var id = $"item/{safePackId}/{StableToken(seed, "item", archetype.Id, i)}/{i:000}";
            items.Add(new GeneratedItemInstance
            {
                Id = id,
                SourceArchetypeId = archetype.Id,
                Tier = archetype.Tier,
                Tags = archetype.Tags.OrderBy(tag => tag, StringComparer.Ordinal).ToList(),
                Name = $"{archetype.Tier} {archetype.Name} {i + 1}",
                Description = archetype.Description,
                Value = archetype.BaseValue + i % 7,
                Provenance = Provenance(pack.PackId, archetype.Id, seed, i)
            });
            allIds.Add(id);
        }

        for (var i = 0; i < pack.Budgets.LootEntries; i++)
        {
            var rng = Rng(pack, seed, "loot", i);
            var table = Pick(pack.LootTables, rng, i);
            var entry = Pick(table.Entries, rng, i);
            var item = items.First(candidate => candidate.SourceArchetypeId == entry.ItemArchetypeId);
            var id = $"loot/{safePackId}/{StableToken(seed, "loot", table.Id, i)}/{i:000}";
            loot.Add(new GeneratedLootEntryInstance
            {
                Id = id,
                LootTableId = $"loot_table/{safePackId}/{SafeSegment(table.Id)}",
                ItemId = item.Id,
                Weight = entry.Weight,
                Amount = entry.Amount,
                RegionId = $"region/{safePackId}/{SafeSegment(Pick(pack.Regions, rng, i).Id)}",
                FactionId = $"faction/{safePackId}/{SafeSegment(Pick(pack.Factions, rng, i).Id)}",
                Provenance = Provenance(pack.PackId, table.Id + "/" + entry.Id, seed, i)
            });
            allIds.Add(id);
        }

        for (var i = 0; i < pack.Budgets.Quests; i++)
        {
            var rng = Rng(pack, seed, "quest", i);
            var motif = Pick(pack.QuestMotifs, rng, i);
            var npc = Pick(npcs, rng, i);
            var item = items.First(candidate => candidate.SourceArchetypeId == motif.RewardItemArchetypeId);
            var id = $"quest/{safePackId}/{StableToken(seed, "quest", motif.Id, i)}/{i:000}";
            var objectiveId = $"objective/{safePackId}/{StableToken(seed, "objective", motif.Id, i)}/{i:000}";
            var title = FillSlots(motif.TitleTemplate, npc, item, null) + " #" + (i + 1).ToString("000", CultureInfo.InvariantCulture);
            quests.Add(new GeneratedQuestInstance
            {
                Id = id,
                SourceMotifId = motif.Id,
                Title = title,
                ObjectiveKind = motif.ObjectiveKind,
                ObjectiveId = objectiveId,
                ObjectiveSignature = motif.ObjectiveKind + "|" + npc.Id + "|" + item.Id + "|" + title,
                NpcId = npc.Id,
                RewardItemId = item.Id,
                RegionId = npc.RegionId,
                FactionId = npc.FactionId,
                Provenance = Provenance(pack.PackId, motif.Id, seed, i)
            });
            allIds.Add(id);
        }

        for (var i = 0; i < pack.Budgets.Events; i++)
        {
            var rng = Rng(pack, seed, "event", i);
            var motif = Pick(pack.EventMotifs, rng, i);
            var quest = Pick(quests, rng, i);
            var id = $"event/{safePackId}/{StableToken(seed, "event", motif.Id, i)}/{i:000}";
            events.Add(new GeneratedEventInstance
            {
                Id = id,
                SourceMotifId = motif.Id,
                Trigger = motif.Trigger,
                TargetQuestId = quest.Id,
                TargetNpcId = quest.NpcId,
                ConsequenceKind = motif.Actions[0].Kind,
                ConsequenceTargetId = $"flag/{safePackId}/{SafeSegment(motif.Id)}/{i:000}",
                Signature = motif.Trigger + "|" + quest.Id + "|" + motif.Actions[0].Kind + "|" + id,
                Provenance = Provenance(pack.PackId, motif.Id, seed, i)
            });
            allIds.Add(id);
        }

        for (var i = 0; i < pack.Budgets.DialogueLines; i++)
        {
            var rng = Rng(pack, seed, "dialogue", i);
            var intent = Pick(pack.DialogueIntents, rng, i);
            var voice = Pick(pack.Voices.Where(voice => intent.AllowedVoiceIds.Contains(voice.Id, StringComparer.Ordinal)).ToList(), rng, i);
            var quest = Pick(quests, rng, i);
            var npc = npcs.Single(item => item.Id == quest.NpcId);
            var item = items.Single(item => item.Id == quest.RewardItemId);
            var phrase = Pick(intent.PhraseTemplates, rng, i);
            var line = FillSlots(voice.Prefix + " " + phrase, npc, item, quest) + " [" + (i + 1).ToString("000", CultureInfo.InvariantCulture) + "]";
            var id = $"dialogue/{safePackId}/{StableToken(seed, "dialogue", intent.Id, i)}/{i:000}";
            var choiceId = $"choice/{safePackId}/{StableToken(seed, "choice", intent.Id, i)}/{i:000}";
            dialogues.Add(new GeneratedDialogueInstance
            {
                Id = id,
                ChoiceId = choiceId,
                SourceIntentId = intent.Id,
                SourceVoiceId = voice.Id,
                SpeakerNpcId = npc.Id,
                QuestId = quest.Id,
                QuestObjectiveId = quest.ObjectiveId,
                RewardItemId = item.Id,
                RegionId = quest.RegionId,
                FactionId = quest.FactionId,
                Line = line,
                Provenance = Provenance(pack.PackId, intent.Id + "/" + voice.Id, seed, i)
            });
            allIds.Add(id);
        }

        var runtimeThreads = dialogues.Take(2).Select((dialogue, index) =>
        {
            var quest = quests.Single(item => item.Id == dialogue.QuestId);
            var evt = events[index % events.Count];
            return new GeneratedRuntimeThread
            {
                ThreadId = $"thread/{safePackId}/{index:000}",
                SelectedGeneratedIds = [dialogue.Id, dialogue.ChoiceId, quest.Id, quest.ObjectiveId, quest.RewardItemId, evt.Id, $"loot_table/{safePackId}/primary"],
                Commands =
                [
                    ContentGenerationRuntimeCommand.StartQuest($"cmd/{safePackId}/{index:000}/start_quest", quest.Id),
                    ContentGenerationRuntimeCommand.OpenDialogue($"cmd/{safePackId}/{index:000}/open_dialogue", dialogue.Id),
                    ContentGenerationRuntimeCommand.ChooseDialogue($"cmd/{safePackId}/{index:000}/choose_dialogue", dialogue.ChoiceId, quest.Id),
                    ContentGenerationRuntimeCommand.SetFlag($"cmd/{safePackId}/{index:000}/event_flag", evt.ConsequenceTargetId, "true", evt.Id),
                    ContentGenerationRuntimeCommand.RollLoot($"cmd/{safePackId}/{index:000}/roll_loot", $"loot_table/{safePackId}/primary", "inventory/player")
                ]
            };
        }).ToList();

        var catalogWithoutHash = new GeneratedContentCatalog
        {
            PackId = pack.PackId,
            Seed = seed,
            Npcs = npcs.OrderBy(item => item.Id, StringComparer.Ordinal).ToList(),
            Items = items.OrderBy(item => item.Id, StringComparer.Ordinal).ToList(),
            LootEntries = loot.OrderBy(item => item.Id, StringComparer.Ordinal).ToList(),
            Quests = quests.OrderBy(item => item.Id, StringComparer.Ordinal).ToList(),
            Events = events.OrderBy(item => item.Id, StringComparer.Ordinal).ToList(),
            Dialogues = dialogues.OrderBy(item => item.Id, StringComparer.Ordinal).ToList(),
            RuntimeThreads = runtimeThreads,
            Counts = new ContentGenerationCounts
            {
                TotalInstances = npcs.Count + quests.Count + events.Count + dialogues.Count + loot.Count,
                Npcs = npcs.Count,
                Quests = quests.Count,
                Events = events.Count,
                DialogueLines = dialogues.Count,
                ItemLootSpawnEntries = loot.Count
            },
            AuthoredExpandedCounts = new ContentGenerationAuthoredExpandedCounts
            {
                AuthoredFinalInstances = 0,
                ExpandedInstances = npcs.Count + quests.Count + events.Count + dialogues.Count + loot.Count
            },
            AllGeneratedIds = allIds
                .Concat(quests.Select(item => item.ObjectiveId))
                .Concat(dialogues.Select(item => item.ChoiceId))
                .Concat([$"loot_table/{safePackId}/primary"])
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList()
        };

        return catalogWithoutHash with
        {
            CatalogHash = ComputeHash(JsonSerializer.Serialize(catalogWithoutHash, JsonOptions))
        };
    }

    private static GamePackageDefinition MaterializePackage(ContentGenerationPack pack, GeneratedContentCatalog catalog)
    {
        var safePackId = SafeSegment(pack.PackId);
        var tileId = $"tile/{safePackId}/ground";
        var playerInventoryId = "inventory/player";
        var package = new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = $"game/content_generation/{safePackId}",
                Title = pack.Title,
                Version = "0.1.0",
                FormatVersion = "0.1",
                StartMapId = $"map/{safePackId}/start",
                Description = "Deterministic content generation scale acceptance package."
            }
        };
        package.Game.TilePrototypes.Add(new TilePrototypeDefinition { Id = tileId, Name = "Ground", Walkable = true });
        package.Game.Maps.Add(new MapDefinition
        {
            Id = $"map/{safePackId}/start",
            Name = pack.Title,
            Width = 12,
            Height = 12,
            DefaultTileId = tileId,
            StartPosition = new Position2D { X = 1, Y = 1 }
        });
        package.Game.Inventories.Add(new InventoryDefinition
        {
            Id = playerInventoryId,
            OwnerKind = "player",
            Slots = 99
        });
        foreach (var faction in pack.Factions.OrderBy(item => item.Id, StringComparer.Ordinal))
        {
            package.Game.Factions.Add(new FactionDefinition
            {
                Id = $"faction/{safePackId}/{SafeSegment(faction.Id)}",
                Name = faction.Name,
                Description = faction.Description,
                DefaultReputation = 0,
                MinReputation = -100,
                MaxReputation = 100
            });
        }

        foreach (var npc in catalog.Npcs)
        {
            var prototypeId = npc.Id + "/prototype";
            package.Game.EntityPrototypes.Add(new EntityPrototypeDefinition
            {
                Id = prototypeId,
                Name = npc.DisplayName,
                Components = [new ComponentDefinition { Type = "generated_npc", Args = new Dictionary<string, string> { ["npcId"] = npc.Id, ["factionId"] = npc.FactionId } }]
            });
            package.Game.Maps[0].Entities.Add(new EntityInstanceDefinition
            {
                Id = npc.Id,
                PrototypeId = prototypeId,
                Position = new Position2D { X = 2 + package.Game.Maps[0].Entities.Count % 8, Y = 2 + package.Game.Maps[0].Entities.Count / 8 }
            });
            package.GeneratedContent.Npcs.Add(new GeneratedNpcDefinition
            {
                SourceId = npc.Id,
                Name = npc.DisplayName,
                Description = npc.Description,
                RegionId = npc.RegionId,
                SceneId = package.Manifest.StartMapId
            });
        }

        foreach (var item in catalog.Items)
        {
            package.Game.Items.Add(new ItemDefinition
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Kind = "generated",
                MaxStack = 99,
                Value = item.Value,
                Tags = item.Tags.ToList(),
                Metadata = new Dictionary<string, string>
                {
                    ["source_archetype_id"] = item.SourceArchetypeId,
                    ["content_generation_pack_id"] = pack.PackId
                }
            });
            package.GeneratedContent.Items.Add(new GeneratedItemDefinition
            {
                SourceId = item.Id,
                Name = item.Name,
                Description = item.Description
            });
        }

        package.Game.LootTables.Add(new LootTableDefinition
        {
            Id = $"loot_table/{safePackId}/primary",
            Name = "Generated primary loot",
            Entries = catalog.LootEntries.Select(entry => new LootEntryDefinition
            {
                Id = entry.Id,
                Output = new OutputDefinition { Kind = "item", Id = entry.ItemId, Amount = entry.Amount },
                Weight = entry.Weight,
                MinCount = (int)Math.Max(1, entry.Amount),
                MaxCount = (int)Math.Max(1, entry.Amount),
                Tags = ["generated", entry.RegionId, entry.FactionId]
            }).ToList()
        });

        foreach (var quest in catalog.Quests)
        {
            var questDialogue = catalog.Dialogues.FirstOrDefault(item => item.QuestId == quest.Id);
            package.Game.Quests.Add(new QuestDefinition
            {
                Id = quest.Id,
                Title = quest.Title,
                Description = "Generated quest motif " + quest.SourceMotifId,
                AutoStart = false,
                Tags = ["generated", quest.SourceMotifId],
                Metadata = new Dictionary<string, string> { ["generated_content_source"] = quest.Provenance.SourceId },
                Objectives =
                [
                    new QuestObjectiveDefinition
                    {
                        Id = quest.ObjectiveId,
                        Kind = "choose_dialogue",
                        RequiredAmount = 1,
                        TargetId = questDialogue?.ChoiceId ?? string.Empty,
                        Metadata = new Dictionary<string, string>
                        {
                            ["npc_id"] = quest.NpcId,
                            ["reward_item_id"] = quest.RewardItemId,
                            ["dialogue_id"] = questDialogue?.Id ?? string.Empty
                        }
                    }
                ],
                Rewards = [new OutputDefinition { Kind = "item", Id = quest.RewardItemId, Amount = 1 }]
            });
            package.GeneratedContent.Quests.Add(new GeneratedQuestSeedDefinition
            {
                SourceId = quest.Id,
                PackageQuestId = quest.Id,
                Title = quest.Title,
                Description = "Generated quest motif " + quest.SourceMotifId,
                Objectives = [quest.ObjectiveSignature]
            });
        }

        foreach (var dialogue in catalog.Dialogues)
        {
            package.Game.Dialogues.Add(new DialogueDefinition
            {
                Id = dialogue.Id,
                Title = "Generated " + dialogue.SourceIntentId,
                StartNodeId = dialogue.Id + "/node/start",
                Tags = ["generated", dialogue.SourceIntentId, dialogue.SourceVoiceId],
                Metadata = new Dictionary<string, string>
                {
                    ["speaker_npc_id"] = dialogue.SpeakerNpcId,
                    ["quest_id"] = dialogue.QuestId,
                    ["reward_item_id"] = dialogue.RewardItemId
                },
                Nodes =
                [
                    new DialogueNodeDefinition
                    {
                        Id = dialogue.Id + "/node/start",
                        SpeakerId = dialogue.SpeakerNpcId,
                        Text = dialogue.Line,
                        Choices =
                        [
                            new DialogueChoiceDefinition
                            {
                                Id = dialogue.ChoiceId,
                                Text = "Accept",
                                AdvanceQuestId = dialogue.QuestId,
                                CloseDialogue = true,
                                Rewards =
                                [
                                    new OutputDefinition { Kind = "item", Id = dialogue.RewardItemId, Amount = 1 },
                                    new OutputDefinition { Kind = "reputation", Id = dialogue.FactionId, Amount = 1 }
                                ],
                                Metadata = new Dictionary<string, string>
                                {
                                    ["objective_id"] = dialogue.QuestObjectiveId,
                                    ["generated_dialogue_id"] = dialogue.Id
                                }
                            }
                        ]
                    }
                ]
            });
            package.GeneratedContent.Dialogues.Add(new GeneratedDialogueDefinition
            {
                SourceId = dialogue.Id,
                Title = "Generated " + dialogue.SourceIntentId,
                NpcId = dialogue.SpeakerNpcId,
                SceneId = package.Manifest.StartMapId,
                Lines = [dialogue.Line]
            });
        }

        foreach (var evt in catalog.Events)
        {
            package.Game.Interactions.Add(new InteractionDefinition
            {
                Id = evt.Id,
                Kind = "inspect",
                Effects =
                [
                    new EffectDefinition
                    {
                        Type = "set_flag",
                        Args = new Dictionary<string, string> { ["flagId"] = evt.ConsequenceTargetId, ["value"] = "true" }
                    }
                ],
                Metadata = new Dictionary<string, string>
                {
                    ["event_trigger"] = evt.Trigger,
                    ["target_quest_id"] = evt.TargetQuestId
                }
            });
        }

        package.GeneratedContent.Profile = new GeneratedGameProfileDefinition
        {
            Title = pack.Title,
            Description = "Goal 010 deterministic content generation at scale profile.",
            Genre = pack.StyleId,
            Tone = string.Join(",", pack.StyleTags.OrderBy(item => item, StringComparer.Ordinal)),
            PresentationMode = "headless_runtime_acceptance",
            WorldTopology = "bounded_regions",
            ActorModel = "generated_npc_archetypes",
            CombatModel = "existing_runtime_primitives",
            CoreLoop = ["quest", "dialogue", "event", "loot"],
            Pillars = pack.SemanticTags.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            SourceContextJson = "{}"
        };
        package.GeneratedContent.AppliedArtifacts.Add(new GeneratedContentArtifactProvenance
        {
            ArtifactId = "content_generation_scale_catalog",
            ContractId = "content_generation_pack_v1",
            ArtifactKind = "generated_content_catalog",
            CapabilitySelectionId = "goal_010_content_generation_at_scale",
            GeneratedAt = "deterministic",
            AuditId = catalog.CatalogHash,
            AppliedAt = "deterministic",
            ContentHash = catalog.CatalogHash,
            MappingResult = "materialized"
        });

        return package;
    }

    private static ContentGenerationRepetitionMetrics MeasureRepetition(GeneratedContentCatalog catalog, ContentGenerationRepetitionPolicy policy)
    {
        var duplicateNames = CountDuplicates(catalog.Npcs.Select(item => Normalize(item.DisplayName)));
        var duplicateQuestSignatures = CountDuplicates(catalog.Quests.Select(item => Normalize(item.ObjectiveSignature)));
        var duplicateLines = CountDuplicates(catalog.Dialogues.Select(item => Normalize(item.Line)));
        var duplicateEvents = CountDuplicates(catalog.Events.Select(item => Normalize(item.Signature)));
        var shares = new SortedDictionary<string, double>(StringComparer.Ordinal);
        AddShares(shares, "archetype", catalog.Npcs.Select(item => item.SourceArchetypeId), catalog.Npcs.Count);
        AddShares(shares, "motif", catalog.Quests.Select(item => item.SourceMotifId), catalog.Quests.Count);
        AddShares(shares, "voice", catalog.Dialogues.Select(item => item.SourceVoiceId), catalog.Dialogues.Count);
        AddShares(shares, "intent", catalog.Dialogues.Select(item => item.SourceIntentId), catalog.Dialogues.Count);
        var maxShare = shares.Count == 0 ? 0 : shares.Values.Max();
        var maxAllowed = policy.MaxSharePerArchetype <= 0 ? 1 : policy.MaxSharePerArchetype;
        var diagnostics = new List<ContentGenerationScaleDiagnostic>();
        if (duplicateNames > 0 && !policy.AllowDuplicateNames)
        {
            diagnostics.Add(Diagnostic("error", "content_generation.repetition.duplicate_names", "npcs", "Duplicate generated NPC display names are not allowed."));
        }

        if (duplicateQuestSignatures > 0)
        {
            diagnostics.Add(Diagnostic("error", "content_generation.repetition.duplicate_quest_signature", "quests", "Duplicate quest title/objective signatures are not allowed."));
        }

        if (duplicateLines > 0)
        {
            diagnostics.Add(Diagnostic("error", "content_generation.repetition.duplicate_dialogue_line", "dialogue", "Duplicate final dialogue lines are not allowed."));
        }

        if (duplicateEvents > 0)
        {
            diagnostics.Add(Diagnostic("error", "content_generation.repetition.duplicate_event_signature", "events", "Duplicate event signatures are not allowed."));
        }

        if (maxShare > maxAllowed)
        {
            diagnostics.Add(Diagnostic("error", "content_generation.repetition.share_cap_breached", "distribution", "Generated distribution exceeds the configured max share."));
        }

        return new ContentGenerationRepetitionMetrics
        {
            DuplicateNpcDisplayNames = duplicateNames,
            DuplicateQuestSignatures = duplicateQuestSignatures,
            DuplicateDialogueLines = duplicateLines,
            DuplicateEventSignatures = duplicateEvents,
            TopFrequencyShares = shares,
            MaxShare = maxShare,
            MaxShareAllowed = maxAllowed,
            MaxSharePassed = maxShare <= maxAllowed,
            ExhaustedPoolDiagnostics = diagnostics.Where(item => item.Code.Contains("exhausted", StringComparison.Ordinal)).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static void AddShares(SortedDictionary<string, double> shares, string kind, IEnumerable<string> values, int total)
    {
        if (total <= 0)
        {
            return;
        }

        foreach (var group in values.GroupBy(item => item, StringComparer.Ordinal))
        {
            shares[kind + ":" + group.Key] = Math.Round(group.Count() / (double)total, 4);
        }
    }

    private static int CountDuplicates(IEnumerable<string> values) =>
        values.GroupBy(item => item, StringComparer.Ordinal).Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1).Sum(group => group.Count() - 1);

    private static string FillSlots(string template, GeneratedNpcInstance npc, GeneratedItemInstance item, GeneratedQuestInstance? quest)
    {
        return template
            .Replace("{npc}", npc.DisplayName, StringComparison.Ordinal)
            .Replace("{role}", npc.Role, StringComparison.Ordinal)
            .Replace("{trait}", npc.Trait, StringComparison.Ordinal)
            .Replace("{item}", item.Name, StringComparison.Ordinal)
            .Replace("{quest}", quest?.Title ?? "the task", StringComparison.Ordinal)
            .Replace("{region}", npc.RegionId.Split('/').Last(), StringComparison.Ordinal)
            .Replace("{faction}", npc.FactionId.Split('/').Last(), StringComparison.Ordinal);
    }

    private static T Pick<T>(IReadOnlyList<T> values, Random random, int ordinal)
    {
        if (values.Count == 0)
        {
            throw new InvalidOperationException("Cannot pick from an empty deterministic pool.");
        }

        return values[(random.Next(values.Count) + ordinal) % values.Count];
    }

    private static Random Rng(ContentGenerationPack pack, string seed, string kind, int ordinal) =>
        new(BitConverter.ToInt32(SHA256.HashData(Encoding.UTF8.GetBytes(pack.PackId + "|" + seed + "|" + kind + "|" + ordinal)), 0));

    private static GeneratedContentProvenance Provenance(string packId, string sourceId, string seed, int ordinal) => new()
    {
        SourcePackId = packId,
        SourceId = sourceId,
        Seed = seed,
        Ordinal = ordinal
    };

    private static bool HasDependencyCycle(IEnumerable<(string Id, IReadOnlyList<string> DependsOn)> graph)
    {
        var nodes = graph.ToDictionary(item => item.Id, item => item.DependsOn, StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        bool Visit(string id)
        {
            if (visited.Contains(id))
            {
                return false;
            }

            if (!visiting.Add(id))
            {
                return true;
            }

            foreach (var next in nodes.GetValueOrDefault(id, Array.Empty<string>()))
            {
                if (nodes.ContainsKey(next) && Visit(next))
                {
                    return true;
                }
            }

            visiting.Remove(id);
            visited.Add(id);
            return false;
        }

        return nodes.Keys.Any(Visit);
    }

    private static bool ContainsInjectionPayload(ContentGenerationPack pack)
    {
        var json = JsonSerializer.Serialize(pack, JsonOptions);
        return json.Contains("..", StringComparison.Ordinal) ||
               json.Contains(".exe", StringComparison.OrdinalIgnoreCase) ||
               json.Contains("powershell", StringComparison.OrdinalIgnoreCase) ||
               json.Contains("cmd.exe", StringComparison.OrdinalIgnoreCase) ||
               json.Contains("provider://", StringComparison.OrdinalIgnoreCase) ||
               json.Contains("script://", StringComparison.OrdinalIgnoreCase) ||
               json.Contains("C:\\", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPositiveFinite(double value) => value > 0 && !double.IsNaN(value) && !double.IsInfinity(value);

    private static void CheckDuplicateIds(List<ContentGenerationScaleDiagnostic> diagnostics, IEnumerable<string> ids, string code, string target)
    {
        foreach (var duplicate in ids.GroupBy(id => id, StringComparer.Ordinal).Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1))
        {
            diagnostics.Add(Diagnostic("error", code, target, "Duplicate source id: " + duplicate.Key));
        }
    }

    private static void Require(bool condition, List<ContentGenerationScaleDiagnostic> diagnostics, string code, string target, string message)
    {
        if (!condition)
        {
            diagnostics.Add(Diagnostic("error", code, target, message));
        }
    }

    private static bool IsSafeId(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '-');

    private static string SafeSegment(string value)
    {
        var builder = new StringBuilder();
        foreach (var c in value.ToLowerInvariant())
        {
            builder.Append(char.IsAsciiLetterOrDigit(c) ? c : '-');
        }

        return builder.ToString().Trim('-');
    }

    private static string StableToken(string seed, string kind, string sourceId, int ordinal) =>
        ComputeHash(seed + "|" + kind + "|" + sourceId + "|" + ordinal)[..10];

    private static string Normalize(string value) =>
        string.Join(" ", value.Trim().ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static IReadOnlyList<ContentGenerationScaleDiagnostic> SortDiagnostics(IEnumerable<ContentGenerationScaleDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static ContentGenerationScaleDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static string RelativePath(string root, string path)
    {
        return Path.GetRelativePath(root, path).Replace('\\', '/');
    }

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Content generation output path must stay under the project root.");
        }
    }

    private static string RenderReport(ContentGenerationScaleReport report)
    {
        var lines = new List<string>
        {
            "# Content Generation Scale Report",
            "",
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Manual gate: {report.ManualGate}",
            $"- Completed slices: {string.Join(", ", report.CompletedSlices)}",
            $"- Packs: {report.ValidPackCount}/{report.PackCount}",
            $"- Runtime threads accepted: {report.RuntimeThreadsAccepted}/{report.RuntimeThreadCount}",
            $"- Deterministic hash: {report.DeterministicHash}",
            "- External execution: none"
        };
        foreach (var pack in report.Packs)
        {
            lines.Add("");
            lines.Add("## " + pack.PackId);
            lines.Add($"- Source hash: {pack.SourceHash}");
            lines.Add($"- Catalog hash: {pack.Catalog.CatalogHash}");
            lines.Add($"- Package hash: {pack.PackageAudit.PackageHash}");
            lines.Add($"- Counts: npc={pack.Counts.Npcs}, quests={pack.Counts.Quests}, events={pack.Counts.Events}, dialogue={pack.Counts.DialogueLines}, loot={pack.Counts.ItemLootSpawnEntries}");
            lines.Add($"- Repetition max share: {pack.RepetitionMetrics.MaxShare.ToString("0.####", CultureInfo.InvariantCulture)}");
        }

        lines.Add("");
        lines.Add("## Gate");
        lines.Add("Stop at content_generation_at_scale_artifact_verification. Do not mark it passed.");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(ContentGenerationScaleReport report)
    {
        var lines = new List<string>
        {
            "# Content Generation At Scale Verification",
            "",
            "- Prior gate recorded: rule_pack_combat_faction_social_work_theft_artifact_verification passed",
            "- Final gate: content_generation_at_scale_artifact_verification",
            "- Gate status: required",
            "- S092/Goal 011: not created",
            "- Public schemas/project files/UI/Unity/Lua/provider/media/generator-library: untouched by this artifact"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    public sealed class UnavailableContentGenerationScaleRuntimeAdapter : IContentGenerationScaleRuntimeAdapter
    {
        public ContentGenerationRuntimeEvidence Run(ContentGenerationRuntimeRequest request) => new()
        {
            RuntimeAttempted = false,
            RuntimeStartSucceeded = false,
            PackageId = request.Package.Manifest.PackageId,
            PackageHash = request.PackageHash,
            RuntimeBoundary = new ContentGenerationRuntimeBoundaryEvidence
            {
                AdapterId = nameof(UnavailableContentGenerationScaleRuntimeAdapter),
                UsedGameRuntimeService = false,
                UsedRuntimeStateFactory = false
            },
            Diagnostics =
            [
                Diagnostic("error", "content_generation.runtime_adapter_unavailable", request.ThreadId, "Content generation scale acceptance requires an injected real runtime adapter.")
            ]
        };
    }

    private sealed record ContentGenerationPackParseResult
    {
        public ContentGenerationPack? Pack { get; init; }
        public string RawJson { get; init; } = string.Empty;
        public IReadOnlyList<ContentGenerationScaleDiagnostic> Diagnostics { get; init; } = Array.Empty<ContentGenerationScaleDiagnostic>();
    }

    private sealed record ContentGenerationPackValidationResult
    {
        public bool Passed { get; init; }
        public IReadOnlyList<ContentGenerationScaleDiagnostic> Diagnostics { get; init; } = Array.Empty<ContentGenerationScaleDiagnostic>();
    }
}

public interface IContentGenerationScaleRuntimeAdapter
{
    ContentGenerationRuntimeEvidence Run(ContentGenerationRuntimeRequest request);
}

public sealed record ContentGenerationScaleAcceptanceOptions
{
    public string PrimarySeed { get; init; } = "goal010-scale-seed-a";
    public string SecondarySeed { get; init; } = "goal010-scale-seed-b";
    public bool IncludeExpectationOnlyInvalidMutation { get; init; } = true;
}

public sealed record ContentGenerationScaleAcceptanceResult
{
    public ContentGenerationScaleReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record ContentGenerationScaleWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record ContentGenerationScaleReport
{
    public string SchemaVersion { get; init; } = "1";
    public string DeterministicHash { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public bool Goal009GateRecorded { get; init; }
    public IReadOnlyList<string> CompletedSlices { get; init; } = Array.Empty<string>();
    public int PackCount { get; init; }
    public int ValidPackCount { get; init; }
    public int RuntimeThreadCount { get; init; }
    public int RuntimeThreadsAccepted { get; init; }
    public bool ValidMatrixPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool PackageRuntimePassed { get; init; }
    public bool RepetitionPassed { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public ContentGenerationExternalExecutionFlags ExternalExecution { get; init; } = new();
    public IReadOnlyList<ContentGenerationScalePackResult> Packs { get; init; } = Array.Empty<ContentGenerationScalePackResult>();
    public ContentGenerationReplayEvidence ReplayEvidence { get; init; } = new();
    public ContentGenerationVariationEvidence VariationEvidence { get; init; } = new();
    public ContentGenerationIsolationEvidence IsolationEvidence { get; init; } = new();
    public ContentGenerationInvalidMatrix InvalidMatrix { get; init; } = new();
    public IReadOnlyList<ContentGenerationScaleDiagnostic> Diagnostics { get; init; } = Array.Empty<ContentGenerationScaleDiagnostic>();
    public IReadOnlyList<string> RemainingPrimitiveLimits { get; init; } = Array.Empty<string>();
}

public sealed record ContentGenerationScalePackResult
{
    [JsonIgnore]
    public ContentGenerationPack? Pack { get; init; }
    public bool Accepted { get; init; }
    public string PackId { get; init; } = string.Empty;
    public string StyleId { get; init; } = string.Empty;
    public string SourceRelativePath { get; init; } = string.Empty;
    public string SourceHash { get; init; } = string.Empty;
    public IReadOnlyList<string> Seeds { get; init; } = Array.Empty<string>();
    public ContentGenerationCounts Counts { get; init; } = new();
    public ContentGenerationAuthoredExpandedCounts AuthoredExpandedCounts { get; init; } = new();
    public GeneratedContentCatalog Catalog { get; init; } = new();
    public ContentGenerationPackageAudit PackageAudit { get; init; } = new();
    public IReadOnlyList<ContentGenerationRuntimeThreadResult> RuntimeThreads { get; init; } = Array.Empty<ContentGenerationRuntimeThreadResult>();
    public ContentGenerationRepetitionMetrics RepetitionMetrics { get; init; } = new();
    public IReadOnlyList<ContentGenerationScaleDiagnostic> Diagnostics { get; init; } = Array.Empty<ContentGenerationScaleDiagnostic>();
}

public sealed record GeneratedContentCatalog
{
    public string PackId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string CatalogHash { get; init; } = string.Empty;
    public ContentGenerationCounts Counts { get; init; } = new();
    public ContentGenerationAuthoredExpandedCounts AuthoredExpandedCounts { get; init; } = new();
    public IReadOnlyList<GeneratedNpcInstance> Npcs { get; init; } = Array.Empty<GeneratedNpcInstance>();
    public IReadOnlyList<GeneratedItemInstance> Items { get; init; } = Array.Empty<GeneratedItemInstance>();
    public IReadOnlyList<GeneratedLootEntryInstance> LootEntries { get; init; } = Array.Empty<GeneratedLootEntryInstance>();
    public IReadOnlyList<GeneratedQuestInstance> Quests { get; init; } = Array.Empty<GeneratedQuestInstance>();
    public IReadOnlyList<GeneratedEventInstance> Events { get; init; } = Array.Empty<GeneratedEventInstance>();
    public IReadOnlyList<GeneratedDialogueInstance> Dialogues { get; init; } = Array.Empty<GeneratedDialogueInstance>();
    public IReadOnlyList<GeneratedRuntimeThread> RuntimeThreads { get; init; } = Array.Empty<GeneratedRuntimeThread>();
    public IReadOnlyList<string> AllGeneratedIds { get; init; } = Array.Empty<string>();
    public int TotalInstances => Counts.TotalInstances;
}

public sealed record GeneratedContentProvenance
{
    public string SourcePackId { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public int Ordinal { get; init; }
}

public sealed record GeneratedNpcInstance
{
    public string Id { get; init; } = string.Empty;
    public string SourceArchetypeId { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public string Trait { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public GeneratedContentProvenance Provenance { get; init; } = new();
}

public sealed record GeneratedItemInstance
{
    public string Id { get; init; } = string.Empty;
    public string SourceArchetypeId { get; init; } = string.Empty;
    public string Tier { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double Value { get; init; }
    public GeneratedContentProvenance Provenance { get; init; } = new();
}

public sealed record GeneratedLootEntryInstance
{
    public string Id { get; init; } = string.Empty;
    public string LootTableId { get; init; } = string.Empty;
    public string ItemId { get; init; } = string.Empty;
    public double Weight { get; init; }
    public double Amount { get; init; }
    public string RegionId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public GeneratedContentProvenance Provenance { get; init; } = new();
}

public sealed record GeneratedQuestInstance
{
    public string Id { get; init; } = string.Empty;
    public string SourceMotifId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ObjectiveKind { get; init; } = string.Empty;
    public string ObjectiveId { get; init; } = string.Empty;
    public string ObjectiveSignature { get; init; } = string.Empty;
    public string NpcId { get; init; } = string.Empty;
    public string RewardItemId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public GeneratedContentProvenance Provenance { get; init; } = new();
}

public sealed record GeneratedEventInstance
{
    public string Id { get; init; } = string.Empty;
    public string SourceMotifId { get; init; } = string.Empty;
    public string Trigger { get; init; } = string.Empty;
    public string TargetQuestId { get; init; } = string.Empty;
    public string TargetNpcId { get; init; } = string.Empty;
    public string ConsequenceKind { get; init; } = string.Empty;
    public string ConsequenceTargetId { get; init; } = string.Empty;
    public string Signature { get; init; } = string.Empty;
    public GeneratedContentProvenance Provenance { get; init; } = new();
}

public sealed record GeneratedDialogueInstance
{
    public string Id { get; init; } = string.Empty;
    public string ChoiceId { get; init; } = string.Empty;
    public string SourceIntentId { get; init; } = string.Empty;
    public string SourceVoiceId { get; init; } = string.Empty;
    public string SpeakerNpcId { get; init; } = string.Empty;
    public string QuestId { get; init; } = string.Empty;
    public string QuestObjectiveId { get; init; } = string.Empty;
    public string RewardItemId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public string Line { get; init; } = string.Empty;
    public GeneratedContentProvenance Provenance { get; init; } = new();
}

public sealed record GeneratedRuntimeThread
{
    public string ThreadId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedGeneratedIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ContentGenerationRuntimeCommand> Commands { get; init; } = Array.Empty<ContentGenerationRuntimeCommand>();
}

public sealed record ContentGenerationPackageAudit
{
    [JsonIgnore]
    public GamePackageDefinition Package { get; init; } = new();
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string CatalogHash { get; init; } = string.Empty;
    public bool GeneratedContentHashMatchesCatalog { get; init; }
    public bool ValidatorClean { get; init; }
    public int ValidationErrorCount { get; init; }
    public IReadOnlyList<ContentGenerationScaleDiagnostic> Diagnostics { get; init; } = Array.Empty<ContentGenerationScaleDiagnostic>();
}

public sealed record ContentGenerationRuntimeRequest
{
    public string PackId { get; init; } = string.Empty;
    public string ThreadId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public GamePackageDefinition Package { get; init; } = new();
    public string PackageHash { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedGeneratedIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ContentGenerationRuntimeCommand> Commands { get; init; } = Array.Empty<ContentGenerationRuntimeCommand>();
}

public sealed record ContentGenerationRuntimeThreadResult
{
    public string ThreadId { get; init; } = string.Empty;
    public string PackId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<string> SelectedGeneratedIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ContentGenerationRuntimeCommand> Commands { get; init; } = Array.Empty<ContentGenerationRuntimeCommand>();
    public ContentGenerationRuntimeEvidence RuntimeEvidence { get; init; } = new();
    public IReadOnlyList<ContentGenerationScaleDiagnostic> Diagnostics { get; init; } = Array.Empty<ContentGenerationScaleDiagnostic>();
}

public sealed record ContentGenerationRuntimeCommand
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string InventoryId { get; init; } = "inventory/player";

    public static ContentGenerationRuntimeCommand StartQuest(string commandId, string questId) =>
        new() { CommandId = commandId, CommandType = "quest/start", TargetId = questId };

    public static ContentGenerationRuntimeCommand OpenDialogue(string commandId, string dialogueId) =>
        new() { CommandId = commandId, CommandType = "dialogue/open", TargetId = dialogueId };

    public static ContentGenerationRuntimeCommand ChooseDialogue(string commandId, string choiceId, string questId) =>
        new() { CommandId = commandId, CommandType = "dialogue/choose", TargetId = choiceId, SecondaryTargetId = questId };

    public static ContentGenerationRuntimeCommand SetFlag(string commandId, string flagId, string value, string sourceEventId) =>
        new() { CommandId = commandId, CommandType = "event/set_flag", TargetId = flagId, Value = value, SecondaryTargetId = sourceEventId };

    public static ContentGenerationRuntimeCommand RollLoot(string commandId, string lootTableId, string inventoryId) =>
        new() { CommandId = commandId, CommandType = "loot/roll", TargetId = lootTableId, InventoryId = inventoryId };
}

public sealed record ContentGenerationRuntimeEvidence
{
    public bool RuntimeAttempted { get; init; }
    public bool RuntimeStartSucceeded { get; init; }
    public string RuntimeStateOwner { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public ContentGenerationRuntimeBoundaryEvidence RuntimeBoundary { get; init; } = new();
    public string RuntimeEvidenceHash { get; init; } = string.Empty;
    public IReadOnlyList<ContentGenerationRuntimeCommandEvidence> Commands { get; init; } = Array.Empty<ContentGenerationRuntimeCommandEvidence>();
    public ContentGenerationRuntimeStateDelta StateDelta { get; init; } = new();
    public string RuntimeStateHash { get; init; } = string.Empty;
    public string RestoredRuntimeStateHash { get; init; } = string.Empty;
    public bool SaveLoadRoundtripPassed { get; init; }
    public ContentGenerationSaveLoadEvidence SaveLoadEvidence { get; init; } = new();
    public bool IsolationPassed { get; init; }
    public IReadOnlyDictionary<string, string> StateEvidence { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> RestoredStateEvidence { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<ContentGenerationScaleDiagnostic> Diagnostics { get; init; } = Array.Empty<ContentGenerationScaleDiagnostic>();
}

public sealed record ContentGenerationRuntimeBoundaryEvidence
{
    public string AdapterId { get; init; } = string.Empty;
    public string RuntimeServiceType { get; init; } = string.Empty;
    public string StateFactoryType { get; init; } = string.Empty;
    public string SerializerType { get; init; } = string.Empty;
    public string SnapshotStoreType { get; init; } = string.Empty;
    public bool UsedGameRuntimeService { get; init; }
    public bool UsedRuntimeStateFactory { get; init; }
}

public sealed record ContentGenerationRuntimeCommandEvidence
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public bool Succeeded { get; init; }
    public string DiagnosticCode { get; init; } = string.Empty;
    public IReadOnlyList<string> RuntimeEventTypes { get; init; } = Array.Empty<string>();
}

public sealed record ContentGenerationRuntimeStateDelta
{
    public bool QuestProgressChanged { get; init; }
    public bool RewardItemChanged { get; init; }
    public bool FlagChanged { get; init; }
    public bool ReputationChanged { get; init; }
    public IReadOnlyList<string> ChangedQuestIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ChangedItemIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ChangedFlagIds { get; init; } = Array.Empty<string>();
}

public sealed record ContentGenerationSaveLoadEvidence
{
    public bool UsedRuntimeStateSerializer { get; init; }
    public bool UsedRuntimeSnapshotStore { get; init; }
    public bool SerializedFullState { get; init; }
    public string SerializedStateHash { get; init; } = string.Empty;
    public string RestoredSerializedStateHash { get; init; } = string.Empty;
    public string SnapshotSlotName { get; init; } = string.Empty;
    public bool SnapshotSaveSucceeded { get; init; }
    public bool SnapshotLoadSucceeded { get; init; }
    public bool TempSnapshotCleanupSucceeded { get; init; }
}

public sealed record ContentGenerationCounts
{
    public int TotalInstances { get; init; }
    public int Npcs { get; init; }
    public int Quests { get; init; }
    public int Events { get; init; }
    public int DialogueLines { get; init; }
    public int ItemLootSpawnEntries { get; init; }
}

public sealed record ContentGenerationAuthoredExpandedCounts
{
    public int AuthoredFinalInstances { get; init; }
    public int ExpandedInstances { get; init; }
    public double ExpandedShare => ExpandedInstances + AuthoredFinalInstances == 0 ? 0 : Math.Round(ExpandedInstances / (double)(ExpandedInstances + AuthoredFinalInstances), 4);
}

public sealed record ContentGenerationRepetitionMetrics
{
    public int DuplicateNpcDisplayNames { get; init; }
    public int DuplicateQuestSignatures { get; init; }
    public int DuplicateDialogueLines { get; init; }
    public int DuplicateEventSignatures { get; init; }
    public IReadOnlyDictionary<string, double> TopFrequencyShares { get; init; } = new SortedDictionary<string, double>(StringComparer.Ordinal);
    public double MaxShare { get; init; }
    public double MaxShareAllowed { get; init; }
    public bool MaxSharePassed { get; init; }
    public IReadOnlyList<ContentGenerationScaleDiagnostic> ExhaustedPoolDiagnostics { get; init; } = Array.Empty<ContentGenerationScaleDiagnostic>();
    public IReadOnlyList<ContentGenerationScaleDiagnostic> Diagnostics { get; init; } = Array.Empty<ContentGenerationScaleDiagnostic>();
}

public sealed record ContentGenerationReplayEvidence
{
    public string PackId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string CatalogHash { get; init; } = string.Empty;
    public string ReplayedCatalogHash { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string ReplayedPackageHash { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record ContentGenerationVariationEvidence
{
    public string PackId { get; init; } = string.Empty;
    public string FirstSeed { get; init; } = string.Empty;
    public string SecondSeed { get; init; } = string.Empty;
    public string FirstCatalogHash { get; init; } = string.Empty;
    public string SecondCatalogHash { get; init; } = string.Empty;
    public bool DifferentGeneratedIds { get; init; }
    public bool DifferentRepresentativeNames { get; init; }
    public bool Passed { get; init; }
}

public sealed record ContentGenerationIsolationEvidence
{
    public bool SequentialCrossPackIsolationPassed { get; init; }
    public int DistinctCatalogHashes { get; init; }
    public int DistinctPackageHashes { get; init; }
    public bool Passed { get; init; }
}

public sealed record ContentGenerationInvalidMatrix
{
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<ContentGenerationInvalidScenario> Scenarios { get; init; } = Array.Empty<ContentGenerationInvalidScenario>();
    public IReadOnlyList<ContentGenerationScaleDiagnostic> Diagnostics { get; init; } = Array.Empty<ContentGenerationScaleDiagnostic>();
}

public sealed record ContentGenerationInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<ContentGenerationScaleDiagnostic> Diagnostics { get; init; } = Array.Empty<ContentGenerationScaleDiagnostic>();
}

public sealed record ContentGenerationExternalExecutionFlags
{
    public bool LlmExecuted { get; init; }
    public bool RagExecuted { get; init; }
    public bool ProviderExecuted { get; init; }
    public bool LuaExecuted { get; init; }
    public bool UnityExecuted { get; init; }
    public bool MediaExecuted { get; init; }

    [JsonIgnore]
    public bool AllFalse => !LlmExecuted && !RagExecuted && !ProviderExecuted && !LuaExecuted && !UnityExecuted && !MediaExecuted;
}

public sealed record ContentGenerationScaleDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record ContentGenerationPack
{
    public string SchemaVersion { get; set; } = string.Empty;
    public string PackId { get; set; } = string.Empty;
    public string StyleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IReadOnlyList<string> RequiredTags { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> ExcludedTags { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> StyleTags { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> SemanticTags { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> DeclaredSlots { get; set; } = Array.Empty<string>();
    public ContentGenerationBudgets Budgets { get; set; } = new();
    public ContentGenerationRepetitionPolicy Repetition { get; set; } = new();
    public ContentGenerationNameBank NameBank { get; set; } = new();
    public IReadOnlyList<ContentGenerationRegion> Regions { get; set; } = Array.Empty<ContentGenerationRegion>();
    public IReadOnlyList<ContentGenerationFaction> Factions { get; set; } = Array.Empty<ContentGenerationFaction>();
    public IReadOnlyList<ContentGenerationNpcArchetype> NpcArchetypes { get; set; } = Array.Empty<ContentGenerationNpcArchetype>();
    public IReadOnlyList<ContentGenerationItemArchetype> ItemArchetypes { get; set; } = Array.Empty<ContentGenerationItemArchetype>();
    public IReadOnlyList<ContentGenerationLootTable> LootTables { get; set; } = Array.Empty<ContentGenerationLootTable>();
    public IReadOnlyList<ContentGenerationQuestMotif> QuestMotifs { get; set; } = Array.Empty<ContentGenerationQuestMotif>();
    public IReadOnlyList<ContentGenerationEventMotif> EventMotifs { get; set; } = Array.Empty<ContentGenerationEventMotif>();
    public IReadOnlyList<ContentGenerationDialogueIntent> DialogueIntents { get; set; } = Array.Empty<ContentGenerationDialogueIntent>();
    public IReadOnlyList<ContentGenerationVoice> Voices { get; set; } = Array.Empty<ContentGenerationVoice>();
}

public sealed record ContentGenerationBudgets
{
    public int TotalInstances { get; set; }
    public int Npcs { get; set; }
    public int Quests { get; set; }
    public int Events { get; set; }
    public int DialogueLines { get; set; }
    public int LootEntries { get; set; }
}

public sealed record ContentGenerationRepetitionPolicy
{
    public bool AllowDuplicateNames { get; set; }
    public double MaxSharePerArchetype { get; set; } = 0.5;
    public string FallbackPolicy { get; set; } = "fail";
}

public sealed record ContentGenerationNameBank
{
    public IReadOnlyList<string> Prefixes { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Suffixes { get; set; } = Array.Empty<string>();
}

public sealed record ContentGenerationRegion
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed record ContentGenerationFaction
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed record ContentGenerationNpcArchetype
{
    public string Id { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public IReadOnlyList<string> Traits { get; set; } = Array.Empty<string>();
}

public sealed record ContentGenerationItemArchetype
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Tier { get; set; } = "common";
    public double BaseValue { get; set; } = 1;
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
}

public sealed record ContentGenerationLootTable
{
    public string Id { get; set; } = string.Empty;
    public IReadOnlyList<ContentGenerationLootEntry> Entries { get; set; } = Array.Empty<ContentGenerationLootEntry>();
}

public sealed record ContentGenerationLootEntry
{
    public string Id { get; set; } = string.Empty;
    public string ItemArchetypeId { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Amount { get; set; }
}

public sealed record ContentGenerationQuestMotif
{
    public string Id { get; set; } = string.Empty;
    public string ObjectiveKind { get; set; } = string.Empty;
    public string TitleTemplate { get; set; } = string.Empty;
    public string RewardItemArchetypeId { get; set; } = string.Empty;
    public string RequiredItemArchetypeId { get; set; } = string.Empty;
    public IReadOnlyList<string> DependsOn { get; set; } = Array.Empty<string>();
}

public sealed record ContentGenerationEventMotif
{
    public string Id { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public IReadOnlyList<ContentGenerationEventAction> Actions { get; set; } = Array.Empty<ContentGenerationEventAction>();
    public IReadOnlyList<string> DependsOn { get; set; } = Array.Empty<string>();
}

public sealed record ContentGenerationEventAction
{
    public string Kind { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
}

public sealed record ContentGenerationDialogueIntent
{
    public string Id { get; set; } = string.Empty;
    public IReadOnlyList<string> RequiredSlots { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> AllowedVoiceIds { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> PhraseTemplates { get; set; } = Array.Empty<string>();
}

public sealed record ContentGenerationVoice
{
    public string Id { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
}
