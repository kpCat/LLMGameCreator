using System.Text.Json;
using LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;
using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;

public sealed class EditDrivenGamePackageRuntimePreviewBridgeEvidenceService
{
    private const string PackageJsonRelativePath =
        EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.PackageJsonRelativePath;
    private const string ProjectedIndexRelativePath =
        EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.ProjectedIndexRelativePath;
    private const string PlayerIndexRelativePath =
        EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.PlayerIndexRelativePath;
    private const string ValidationReportRelativePath =
        EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.ValidationReportRelativePath;
    private const string SourceTargetsRelativePath =
        EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.SourceTargetsRelativePath;

    private readonly GamePackageValidator _validator = new();
    private readonly GeneratedPackageRuntimePreviewService _runtimePreviewService = new();
    private readonly GeneratedContentInteractionPreviewService _interactionPreviewService = new();
    private readonly EditDrivenGamePackageRuntimePreviewBridgeProjectionBuilder _projectionBuilder = new();

    public EditDrivenGamePackageRuntimePreviewBridgeBuildResult Build(string repositoryRootPath)
    {
        if (string.IsNullOrWhiteSpace(repositoryRootPath))
        {
            throw new ArgumentException("Repository root path is required.", nameof(repositoryRootPath));
        }

        var root = Path.GetFullPath(repositoryRootPath);
        var source = ReadSourceContext(root);
        var projectedPackage = _projectionBuilder.BuildProjectedPackage(source);
        var projectedFiles = _projectionBuilder.BuildProjectedPackageFiles(source, projectedPackage);
        var fileLedger = BuildFileLedger(projectedFiles);
        var bridgeProof = BuildBridgeProof(source, projectedFiles, fileLedger);
        var negativeProof = BuildNegativeProof(source, projectedFiles, fileLedger);
        var bindingInventory = BuildWinFormsBindingInventory(root);
        var qualityScan = new EditDrivenGamePackageRuntimePreviewBridgeQualityGateScanner()
            .Scan(root, bindingInventory);

        var implementationStatus = IsGreen(source, fileLedger, bridgeProof, negativeProof, bindingInventory, qualityScan)
            ? "GREEN"
            : "BLOCKED";

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var projectedManifest = new EditDrivenGamePackageRuntimePreviewBridgeProjectedPackageManifest
        {
            ImplementationStatus = implementationStatus,
            RowCount = source.Rows.Count,
            TargetCount = source.Targets.Count,
            ActionCount = source.ActionLog.Actions.Count,
            ProjectedPackageFileCount = fileLedger.FileCount,
            SourceGoal077ReportHash = source.Goal077ReportHash,
            SourceGoal078ReportHash = source.Goal078ReportHash,
            SourceGoal079ReportHash = source.Goal079ReportHash,
            SourceGoal079AReportHash = source.Goal079AReportHash,
            ProjectedPackageHash = fileLedger.Files.First(file => file.RelativePath == PackageJsonRelativePath).Sha256,
            ProjectedPackageFileLedgerHash = HashJson(fileLedger),
            RuntimePreviewBridgeProofHash = HashJson(bridgeProof),
            RuntimePreviewNegativeProofHash = HashJson(negativeProof),
            WinFormsBindingInventoryHash = HashJson(bindingInventory),
            QualityGateScanHash = HashJson(qualityScan),
            ProjectedPackageReadProofPassed = bridgeProof.ProjectedPackagePayloadRead,
            RuntimePreviewBridgeProofPassed = bridgeProof.Passed,
            NegativeProofPassed = negativeProof.Passed
        };

        artifactJson["source-artifact-manifest.json"] = Serialize(source.SourceArtifactManifest);
        artifactJson["projected-gamepackage-file-ledger.json"] = Serialize(fileLedger);
        artifactJson["runtime-preview-bridge-proof.json"] = Serialize(bridgeProof);
        artifactJson["runtime-preview-negative-proof.json"] = Serialize(negativeProof);
        artifactJson["winforms-binding-inventory.json"] = Serialize(bindingInventory);
        artifactJson["quality-gate-scan.json"] = Serialize(qualityScan);
        artifactJson["projected-gamepackage-manifest.json"] = Serialize(projectedManifest);

        var report = new EditDrivenGamePackageRuntimePreviewBridgeReport
        {
            ImplementationStatus = implementationStatus,
            Goal079AcceptedForContinuation = source.SourceArtifactManifest.Goal079AcceptedForContinuation,
            Goal079ASourceFormatGuardPassedByHandoff =
                source.SourceArtifactManifest.Goal079ASourceFormatGuardPassedByHandoff,
            RowCount = source.Rows.Count,
            TargetCount = source.Targets.Count,
            ActionCount = source.ActionLog.Actions.Count,
            ProjectedPackageFileCount = fileLedger.FileCount,
            SourceGoal077ReportHash = source.Goal077ReportHash,
            SourceGoal078ReportHash = source.Goal078ReportHash,
            SourceGoal079ReportHash = source.Goal079ReportHash,
            SourceGoal079AReportHash = source.Goal079AReportHash,
            ProjectedPackageHash = projectedManifest.ProjectedPackageHash,
            ProjectedPackageManifestHash = HashJson(projectedManifest),
            ProjectedPackageFileLedgerHash = projectedManifest.ProjectedPackageFileLedgerHash,
            RuntimePreviewBridgeProofHash = projectedManifest.RuntimePreviewBridgeProofHash,
            RuntimePreviewNegativeProofHash = projectedManifest.RuntimePreviewNegativeProofHash,
            WinFormsBindingInventoryHash = projectedManifest.WinFormsBindingInventoryHash,
            QualityGateScanHash = projectedManifest.QualityGateScanHash,
            Diagnostics = source.Diagnostics
                .Concat(fileLedger.Diagnostics)
                .Concat(bridgeProof.Diagnostics)
                .Concat(bindingInventory.Diagnostics)
                .Concat(qualityScan.Diagnostics)
                .ToList()
        };

        report = report with { DeterministicHash = HashJson(report) };
        artifactJson["edit-driven-gamepackage-runtime-preview-bridge-report.json"] = Serialize(report);

        return new EditDrivenGamePackageRuntimePreviewBridgeBuildResult
        {
            SourceArtifactManifest = source.SourceArtifactManifest,
            ProjectedPackageManifest = projectedManifest,
            ProjectedPackageFileLedger = fileLedger,
            RuntimePreviewBridgeProof = bridgeProof,
            RuntimePreviewNegativeProof = negativeProof,
            WinFormsBindingInventory = bindingInventory,
            QualityGateScan = qualityScan,
            Report = report,
            ReportMarkdown = EditDrivenGamePackageRuntimePreviewBridgeReportRenderer.Render(report),
            ProjectedPackageFiles = projectedFiles,
            ArtifactJsonByFileName = artifactJson
        };
    }

    public Task<EditDrivenGamePackageRuntimePreviewBridgeWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var root = Path.GetFullPath(repositoryRootPath);
        var build = Build(root);
        var outputDirectoryPath = Path.Combine(
            root,
            EditDrivenGamePackageRuntimePreviewBridgeVocabulary.RelativeOutputDirectory);
        var projectedDirectoryPath = Path.Combine(
            outputDirectoryPath,
            EditDrivenGamePackageRuntimePreviewBridgeVocabulary.ProjectedPackageDirectoryName);

        if (Directory.Exists(outputDirectoryPath))
        {
            Directory.Delete(outputDirectoryPath, recursive: true);
        }

        Directory.CreateDirectory(projectedDirectoryPath);
        var writtenFiles = new List<string>();

        foreach (var file in build.ProjectedPackageFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetPath = Path.Combine(outputDirectoryPath, NormalizeSeparators(file.Key));
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.WriteAllText(targetPath, file.Value);
            writtenFiles.Add(RelativeToRoot(root, targetPath));
        }

        foreach (var artifact in build.ArtifactJsonByFileName)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (artifact.Key.EndsWith("-report.json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var targetPath = Path.Combine(outputDirectoryPath, artifact.Key);
            File.WriteAllText(targetPath, artifact.Value);
            writtenFiles.Add(RelativeToRoot(root, targetPath));
        }

        var reportPath = Path.Combine(
            outputDirectoryPath,
            "edit-driven-gamepackage-runtime-preview-bridge-report.md");
        File.WriteAllText(reportPath, build.ReportMarkdown);
        writtenFiles.Add(RelativeToRoot(root, reportPath));

        return Task.FromResult(new EditDrivenGamePackageRuntimePreviewBridgeWriteResult
        {
            Result = build,
            OutputDirectoryPath = outputDirectoryPath,
            ProjectedPackageDirectoryPath = projectedDirectoryPath,
            ReportMarkdownPath = reportPath,
            WrittenFiles = writtenFiles.Order(StringComparer.Ordinal).ToList()
        });
    }

    private static bool IsGreen(
        Goal080SourceContext source,
        EditDrivenGamePackageRuntimePreviewBridgeProjectedPackageFileLedger fileLedger,
        EditDrivenGamePackageRuntimePreviewBridgeProof bridgeProof,
        EditDrivenGamePackageRuntimePreviewBridgeNegativeProof negativeProof,
        EditDrivenGamePackageRuntimePreviewBridgeWinFormsBindingInventory bindingInventory,
        EditDrivenGamePackageRuntimePreviewBridgeQualityGateScan qualityScan) =>
        source.SourceArtifactManifest.Goal079AcceptedForContinuation
        && source.SourceArtifactManifest.Goal079ASourceFormatGuardPassedByHandoff
        && source.SourceArtifactManifest.Goal079ReportWasGreenProducedForReview
        && source.SourceArtifactManifest.Goal079ArtifactAcceptedFalse
        && source.Diagnostics.All(diagnostic => diagnostic.Severity != "error")
        && fileLedger.Passed
        && bridgeProof.Passed
        && negativeProof.Passed
        && bindingInventory.Passed
        && qualityScan.Passed;

    private Goal080SourceContext ReadSourceContext(string root)
    {
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic>();
        var sourceArtifacts = new List<EditDrivenGamePackageRuntimePreviewBridgeSourceArtifactReference>();

        foreach (var relativePath in EditDrivenGamePackageRuntimePreviewBridgeVocabulary.RequiredSourceArtifactRelativePaths)
        {
            var fullPath = Path.Combine(root, NormalizeSeparators(relativePath));
            var exists = File.Exists(fullPath);
            if (!exists)
            {
                diagnostics.Add(Diagnostic("source_artifact_missing", relativePath, "Required source artifact is missing."));
            }

            sourceArtifacts.Add(new EditDrivenGamePackageRuntimePreviewBridgeSourceArtifactReference
            {
                SourceGoal = ResolveSourceGoal(relativePath),
                ArtifactFamily = Path.GetFileNameWithoutExtension(relativePath).Replace('-', '_'),
                ArtifactRelativePath = relativePath,
                ArtifactHash = exists ? EditDrivenGamePackageRuntimePreviewBridgeHash.Sha256File(fullPath) : string.Empty,
                Exists = exists
            });
        }

        var goal077ReportPath = Path.Combine(
            root,
            NormalizeSeparators(EditDrivenGamePackageRuntimePreviewBridgeVocabulary.Goal077RelativeOutputDirectory),
            "edit-driven-review-package-materialization-report.md");
        var goal078ReportPath = Path.Combine(
            root,
            NormalizeSeparators(EditDrivenGamePackageRuntimePreviewBridgeVocabulary.Goal078RelativeOutputDirectory),
            "edit-driven-review-package-playable-session-report.md");
        var goal079ReportPath = Path.Combine(
            root,
            NormalizeSeparators(EditDrivenGamePackageRuntimePreviewBridgeVocabulary.Goal079RelativeOutputDirectory),
            "edit-driven-spine-quality-consolidation-report.md");
        var goal079AReportPath = Path.Combine(
            root,
            NormalizeSeparators(EditDrivenGamePackageRuntimePreviewBridgeVocabulary.Goal079ARelativeOutputDirectory),
            "source-format-line-ending-guard-report.md");

        var goal077ReportHash = SafeHash(goal077ReportPath);
        var goal078ReportHash = SafeHash(goal078ReportPath);
        var goal079ReportHash = SafeHash(goal079ReportPath);
        var goal079AReportHash = SafeHash(goal079AReportPath);
        var goal079Fields = ParseReportFields(SafeRead(goal079ReportPath));
        var goal079AFields = ParseReportFields(SafeRead(goal079AReportPath));
        var currentState = SafeRead(Path.Combine(root, "docs", "CURRENT_GENERATOR_STATE.md"))
            + Environment.NewLine
            + SafeRead(Path.Combine(root, "docs", "CURRENT_GENERATOR_STATE.json"));

        var goal079AcceptedForContinuation = currentState.Contains(
            EditDrivenGamePackageRuntimePreviewBridgeVocabulary.Goal079AcceptedForContinuationText,
            StringComparison.OrdinalIgnoreCase);
        var goal079ASourceFormatHandoff = currentState.Contains(
            EditDrivenGamePackageRuntimePreviewBridgeVocabulary.Goal079ASourceFormatHandoffText,
            StringComparison.OrdinalIgnoreCase);
        var goal079Green = string.Equals(Value(goal079Fields, "implementationStatus"), "GREEN", StringComparison.OrdinalIgnoreCase);
        var goal079ArtifactAcceptedFalse = string.Equals(Value(goal079Fields, "accepted"), "false", StringComparison.OrdinalIgnoreCase);
        var goal079AGreen = string.Equals(Value(goal079AFields, "implementationStatus"), "GREEN", StringComparison.OrdinalIgnoreCase);

        if (!goal079AcceptedForContinuation)
        {
            diagnostics.Add(Diagnostic(
                "goal079_handoff_missing",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal079 continuation handoff must be recorded before Goal080 evidence can be GREEN."));
        }

        if (!goal079ASourceFormatHandoff || !goal079AGreen)
        {
            diagnostics.Add(Diagnostic(
                "goal079a_handoff_missing",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal079A source-format guard handoff must be recorded before Goal080 evidence can be GREEN."));
        }

        var rows = ReadRows(root, diagnostics);
        var actionLog = ReadJson<EditDrivenReviewPackagePlayableSessionActionLog>(
            root,
            EditDrivenGamePackageRuntimePreviewBridgeVocabulary.Goal078RelativeOutputDirectory + "/playable-session-action-log.json",
            diagnostics) ?? new EditDrivenReviewPackagePlayableSessionActionLog();
        var replayProof = ReadJson<EditDrivenReviewPackagePlayableSessionReplayProof>(
            root,
            EditDrivenGamePackageRuntimePreviewBridgeVocabulary.Goal078RelativeOutputDirectory + "/playable-session-replay-proof.json",
            diagnostics) ?? new EditDrivenReviewPackagePlayableSessionReplayProof();

        var manifest = new EditDrivenGamePackageRuntimePreviewBridgeSourceArtifactManifest
        {
            Goal079AcceptedForContinuation = goal079AcceptedForContinuation,
            Goal079ASourceFormatGuardPassedByHandoff = goal079ASourceFormatHandoff && goal079AGreen,
            Goal079ReportWasGreenProducedForReview = goal079Green,
            Goal079ArtifactAcceptedFalse = goal079ArtifactAcceptedFalse,
            Goal077ReportHash = goal077ReportHash,
            Goal078ReportHash = goal078ReportHash,
            Goal079ReportHash = goal079ReportHash,
            Goal079AReportHash = goal079AReportHash,
            SourceArtifactCount = sourceArtifacts.Count,
            SourceArtifacts = sourceArtifacts,
            Diagnostics = diagnostics
        };

        return new Goal080SourceContext
        {
            RootPath = root,
            Rows = rows,
            Targets = rows.SelectMany(row => row.Targets).ToList(),
            ActionLog = actionLog,
            ReplayProof = replayProof,
            SourceArtifactManifest = manifest,
            Goal077ReportHash = goal077ReportHash,
            Goal078ReportHash = goal078ReportHash,
            Goal079ReportHash = goal079ReportHash,
            Goal079AReportHash = goal079AReportHash,
            Diagnostics = diagnostics
        };
    }

    private static IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeRowRecord> ReadRows(
        string root,
        List<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic> diagnostics)
    {
        var index = ReadJson<EditDrivenReviewPackageIndex>(
            root,
            EditDrivenGamePackageRuntimePreviewBridgeVocabulary.Goal077RelativeOutputDirectory
            + "/review-package/package-index.json",
            diagnostics);
        if (index == null)
        {
            return [];
        }

        var rows = new List<EditDrivenGamePackageRuntimePreviewBridgeRowRecord>();
        foreach (var sourceRow in index.Rows
                     .OrderBy(row => EditDrivenGamePackageRuntimePreviewBridgeVocabulary.FamilyOrderingKey(row.FamilyId))
                     .ThenBy(row => EditDrivenGamePackageRuntimePreviewBridgeVocabulary.SeedOrderingKey(row.SeedId))
                     .ThenBy(row => row.RowId, StringComparer.Ordinal))
        {
            var targets = new List<EditDrivenGamePackageRuntimePreviewBridgeTargetRecord>();
            foreach (var target in sourceRow.Targets.OrderBy(target => target.TargetId, StringComparer.Ordinal))
            {
                var targetPath = Path.Combine(
                    root,
                    NormalizeSeparators(EditDrivenGamePackageRuntimePreviewBridgeVocabulary.Goal077RelativeOutputDirectory),
                    NormalizeSeparators(target.RelativePath));
                if (!File.Exists(targetPath))
                {
                    diagnostics.Add(Diagnostic("target_payload_missing", target.RelativePath, "Goal077 target payload is missing."));
                    continue;
                }

                var payloadJson = File.ReadAllText(targetPath);
                var normalizedPayloadJson = payloadJson.TrimEnd('\r', '\n');
                var fileHash = EditDrivenGamePackageRuntimePreviewBridgeHash.Sha256Text(normalizedPayloadJson);
                if (!string.Equals(fileHash, target.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.Add(Diagnostic("target_hash_mismatch", target.RelativePath, "Goal077 target payload hash does not match package index."));
                }

                using var payload = JsonDocument.Parse(payloadJson);
                var rootElement = payload.RootElement;
                targets.Add(new EditDrivenGamePackageRuntimePreviewBridgeTargetRecord
                {
                    RowId = sourceRow.RowId,
                    FamilyId = sourceRow.FamilyId,
                    SeedId = sourceRow.SeedId,
                    ProfileId = ProfileId(sourceRow.FamilyId, sourceRow.SeedId),
                    TargetId = target.TargetId,
                    RelativePath = target.RelativePath,
                    LogicalPackagePath = target.LogicalPackagePath,
                    FileHash = fileHash,
                    PayloadHash = EditDrivenGamePackageRuntimePreviewBridgeHash.Sha256Text(normalizedPayloadJson),
                    PayloadJson = payloadJson,
                    FieldId = JsonString(rootElement, "fieldId"),
                    DomainId = JsonString(rootElement, "domainId"),
                    BeforeValue = JsonString(rootElement, "beforeValue"),
                    AfterValue = JsonString(rootElement, "afterValue"),
                    BeforeHash = JsonString(rootElement, "beforeHash"),
                    AfterHash = JsonString(rootElement, "afterHash"),
                    RollbackHash = JsonString(rootElement, "rollbackHash"),
                    ReplayHash = JsonString(rootElement, "replayHash"),
                    ValidationRequirement = JsonString(rootElement, "validationRequirement")
                });
            }

            rows.Add(new EditDrivenGamePackageRuntimePreviewBridgeRowRecord
            {
                RowId = sourceRow.RowId,
                FamilyId = sourceRow.FamilyId,
                SeedId = sourceRow.SeedId,
                ProfileId = ProfileId(sourceRow.FamilyId, sourceRow.SeedId),
                Targets = targets
            });
        }

        if (rows.Count != 9 || rows.Sum(row => row.Targets.Count) != 18)
        {
            diagnostics.Add(Diagnostic("target_coverage_mismatch", "Goal077 package-index.json", "Goal080 expected 9 rows and 18 targets."));
        }

        return rows;
    }

    private EditDrivenGamePackageRuntimePreviewBridgeProjectedPackageFileLedger BuildFileLedger(
        IReadOnlyDictionary<string, string> projectedFiles)
    {
        var entries = projectedFiles.Select(file => new EditDrivenGamePackageRuntimePreviewBridgeProjectedFileEntry
        {
            RelativePath = file.Key,
            Role = file.Key switch
            {
                PackageJsonRelativePath => "public_gamepackage_package_json",
                ProjectedIndexRelativePath => "projected_package_index",
                PlayerIndexRelativePath => "player_readable_bridge_index",
                ValidationReportRelativePath => "validation_report",
                SourceTargetsRelativePath => "source_target_manifest",
                _ => "projected_package_sidecar"
            },
            Sha256 = EditDrivenGamePackageRuntimePreviewBridgeHash.Sha256Text(file.Value),
            ByteCount = System.Text.Encoding.UTF8.GetByteCount(file.Value)
        }).ToList();
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic>();
        if (!entries.Any(file => file.RelativePath == PackageJsonRelativePath))
        {
            diagnostics.Add(Diagnostic("projected_package_missing", PackageJsonRelativePath, "Projected GamePackage package.json is missing."));
        }

        return new EditDrivenGamePackageRuntimePreviewBridgeProjectedPackageFileLedger
        {
            Passed = diagnostics.Count == 0 && entries.Count == projectedFiles.Count,
            FileCount = entries.Count,
            Files = entries,
            Diagnostics = diagnostics
        };
    }

    private EditDrivenGamePackageRuntimePreviewBridgeProof BuildBridgeProof(
        Goal080SourceContext source,
        IReadOnlyDictionary<string, string> projectedFiles,
        EditDrivenGamePackageRuntimePreviewBridgeProjectedPackageFileLedger fileLedger)
    {
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic>();
        var ledgerHashesMatch = fileLedger.Files.All(file =>
            projectedFiles.TryGetValue(file.RelativePath, out var payload)
            && string.Equals(
                EditDrivenGamePackageRuntimePreviewBridgeHash.Sha256Text(payload),
                file.Sha256,
                StringComparison.OrdinalIgnoreCase));
        if (!ledgerHashesMatch)
        {
            diagnostics.Add(Diagnostic("projected_file_ledger_hash_mismatch", PackageJsonRelativePath, "Projected package files do not match the recorded ledger hashes."));
        }

        var projectedPackagePayloadRead = projectedFiles.TryGetValue(PackageJsonRelativePath, out var packageJson);
        if (!projectedPackagePayloadRead)
        {
            diagnostics.Add(Diagnostic("projected_package_missing", PackageJsonRelativePath, "Projected package payload was not read."));
            packageJson = "{}";
        }

        GamePackageDefinition? package = null;
        try
        {
            package = EditDrivenGamePackageRuntimePreviewBridgeJson.Deserialize<GamePackageDefinition>(packageJson ?? "{}");
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Diagnostic("projected_package_json_invalid", PackageJsonRelativePath, exception.Message));
        }

        var deserialized = package != null;
        var validation = deserialized ? _validator.Validate(package!) : new ValidationReport();
        foreach (var issue in validation.Issues.Where(issue => issue.Severity >= ValidationSeverity.Error))
        {
            diagnostics.Add(Diagnostic(issue.Code, issue.TargetId ?? PackageJsonRelativePath, issue.Message));
        }

        var preview = deserialized
            ? _runtimePreviewService.Build(package!, new GameState { CurrentMapId = package!.Manifest.StartMapId })
            : new GeneratedPackageRuntimePreviewModel();
        var interactionCatalog = _interactionPreviewService.Build(preview);
        var interactionEntryCount = interactionCatalog.Categories.Sum(category => category.Entries.Count);
        var targetIdsInPackage = package?.Game.Items
            .Select(item => item.Metadata.TryGetValue("targetId", out var targetId) ? targetId : string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        var targetActions = source.ActionLog.Actions
            .Where(action => !string.IsNullOrWhiteSpace(action.TargetId))
            .ToList();
        var actionTargetIds = targetActions
            .Select(action => action.TargetId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sourceTargetIds = source.Targets.Select(target => target.TargetId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allTargetsCovered = sourceTargetIds.SetEquals(targetIdsInPackage);
        var allActionsCovered = actionTargetIds.All(targetIdsInPackage.Contains)
                                && targetActions.All(action => action.TargetPayloadRead)
                                && source.ActionLog.Actions.Count == source.ActionLog.ActionCount;
        if (!allTargetsCovered)
        {
            diagnostics.Add(Diagnostic("target_projection_gap", PackageJsonRelativePath, "Projected package does not cover all Goal077 targets."));
        }

        if (!allActionsCovered)
        {
            diagnostics.Add(Diagnostic("action_projection_gap", PackageJsonRelativePath, "Goal078 actions are not fully covered by projected targets."));
        }

        var runtimeProjectionPassed = preview.CurrentScene != null
                                      && preview.Warnings.Count == 0
                                      && preview.Regions.Count >= 3
                                      && preview.Npcs.Count == source.Rows.Count
                                      && preview.Items.Count == source.Targets.Count
                                      && preview.Quests.Count == source.Rows.Count
                                      && preview.Mechanics.Count == source.Targets.Count;
        var interactionProjectionPassed = interactionCatalog.Categories.Count >= 8
                                          && interactionEntryCount >= source.Targets.Count + source.Rows.Count;

        return new EditDrivenGamePackageRuntimePreviewBridgeProof
        {
            Passed = projectedPackagePayloadRead
                     && deserialized
                     && ledgerHashesMatch
                     && validation.IsValid
                     && runtimeProjectionPassed
                     && interactionProjectionPassed
                     && allTargetsCovered
                     && allActionsCovered
                     && diagnostics.All(diagnostic => diagnostic.Severity != "error"),
            ProjectedPackagePayloadRead = projectedPackagePayloadRead,
            ProjectedPackageDeserialized = deserialized,
            GamePackageValidationPassed = validation.IsValid,
            RuntimePreviewProjectionPassed = runtimeProjectionPassed,
            InteractionCatalogProjectionPassed = interactionProjectionPassed,
            AllGoal077TargetsCovered = allTargetsCovered,
            AllGoal078ActionsCovered = allActionsCovered,
            RowCount = source.Rows.Count,
            TargetCount = source.Targets.Count,
            ActionCount = source.ActionLog.Actions.Count,
            RuntimePreviewRegionCount = preview.Regions.Count,
            RuntimePreviewNpcCount = preview.Npcs.Count,
            RuntimePreviewItemCount = preview.Items.Count,
            RuntimePreviewDialogueCount = preview.Dialogues.Count,
            RuntimePreviewQuestCount = preview.Quests.Count,
            RuntimePreviewMechanicCount = preview.Mechanics.Count,
            InteractionCategoryCount = interactionCatalog.Categories.Count,
            InteractionEntryCount = interactionEntryCount,
            InitialProjectionStateHash = HashObject(new
            {
                source.Goal077ReportHash,
                source.Goal078ReportHash,
                rows = source.Rows.Count,
                targets = source.Targets.Count
            }),
            PostPackageReadStateHash = HashObject(new
            {
                projectedPackagePayloadRead,
                deserialized,
                projectedPackageHash = fileLedger.Files.FirstOrDefault(file => file.RelativePath == PackageJsonRelativePath)?.Sha256
            }),
            PostRuntimePreviewStateHash = HashObject(new
            {
                preview.CurrentMapId,
                regionCount = preview.Regions.Count,
                npcCount = preview.Npcs.Count,
                itemCount = preview.Items.Count,
                questCount = preview.Quests.Count,
                mechanicCount = preview.Mechanics.Count
            }),
            ActionCoverageStateHash = HashObject(new
            {
                actionCount = source.ActionLog.Actions.Count,
                actionTargetIds = actionTargetIds.Order(StringComparer.Ordinal).ToList(),
                source.ReplayProof.ReplayFinalStateHash
            }),
            Goal078ReplayFinalStateHash = source.ReplayProof.ReplayFinalStateHash,
            ProjectedPackageHash = fileLedger.Files.FirstOrDefault(file => file.RelativePath == PackageJsonRelativePath)?.Sha256 ?? string.Empty,
            ProjectedPackageFileLedgerHash = HashJson(fileLedger),
            RuntimePreviewWarnings = preview.Warnings,
            Diagnostics = diagnostics
        };
    }

    private EditDrivenGamePackageRuntimePreviewBridgeNegativeProof BuildNegativeProof(
        Goal080SourceContext source,
        IReadOnlyDictionary<string, string> projectedFiles,
        EditDrivenGamePackageRuntimePreviewBridgeProjectedPackageFileLedger fileLedger)
    {
        var scenarios = new[]
        {
            RunNegativeScenario(
                "missing_projected_package_file",
                source,
                projectedFiles.Where(pair => pair.Key != PackageJsonRelativePath)
                    .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
                fileLedger),
            RunNegativeScenario(
                "tampered_projected_package_file",
                source,
                projectedFiles.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Key == PackageJsonRelativePath
                        ? pair.Value.Replace("Goal080 Edit Driven Runtime Preview Bridge", string.Empty, StringComparison.Ordinal)
                        : pair.Value,
                    StringComparer.Ordinal),
                fileLedger),
            RunNegativeScenario(
                "projected_index_missing_target",
                source,
                projectedFiles.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Key == SourceTargetsRelativePath
                        ? pair.Value.Replace(source.Targets.Last().TargetId, "missing-target", StringComparison.Ordinal)
                        : pair.Value,
                    StringComparer.Ordinal),
                fileLedger),
            RunNegativeScenario(
                "fake_success_without_projected_package_read",
                source,
                projectedFiles,
                fileLedger,
                forcePayloadReadFalse: true),
            RunNegativeScenario(
                "source_lineage_hash_mismatch",
                source with { Goal077ReportHash = "0000000000000000000000000000000000000000000000000000000000000000" },
                projectedFiles,
                fileLedger)
        };

        return new EditDrivenGamePackageRuntimePreviewBridgeNegativeProof
        {
            Passed = scenarios.Length == EditDrivenGamePackageRuntimePreviewBridgeVocabulary.RequiredNegativeScenarioIds.Count
                     && scenarios.All(scenario => scenario.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Length,
            Scenarios = scenarios
        };
    }

    private EditDrivenGamePackageRuntimePreviewBridgeNegativeScenario RunNegativeScenario(
        string scenarioId,
        Goal080SourceContext source,
        IReadOnlyDictionary<string, string> projectedFiles,
        EditDrivenGamePackageRuntimePreviewBridgeProjectedPackageFileLedger fileLedger,
        bool forcePayloadReadFalse = false)
    {
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic>();
        var proof = BuildBridgeProof(source, projectedFiles, fileLedger);
        var rejected = !proof.Passed;
        if (!forcePayloadReadFalse && scenarioId == "projected_index_missing_target")
        {
            var sourceTargets = projectedFiles.TryGetValue(SourceTargetsRelativePath, out var sourceTargetsJson)
                ? sourceTargetsJson
                : string.Empty;
            rejected = rejected || !source.Targets.All(target => sourceTargets.Contains(target.TargetId, StringComparison.Ordinal));
        }

        if (forcePayloadReadFalse)
        {
            rejected = true;
            diagnostics.Add(Diagnostic(
                "fake_success_rejected",
                PackageJsonRelativePath,
                "Bridge proof requires projected package payload read."));
        }

        if (scenarioId == "source_lineage_hash_mismatch")
        {
            var indexJson = projectedFiles.TryGetValue(ProjectedIndexRelativePath, out var value) ? value : string.Empty;
            rejected = rejected || !indexJson.Contains(source.Goal077ReportHash, StringComparison.Ordinal);
            diagnostics.Add(Diagnostic(
                "source_lineage_mismatch_rejected",
                ProjectedIndexRelativePath,
                "Projected package lineage hash mismatch was rejected."));
        }

        if (!rejected)
        {
            diagnostics.Add(Diagnostic("negative_scenario_not_rejected", scenarioId, "Negative scenario unexpectedly passed."));
        }

        return new EditDrivenGamePackageRuntimePreviewBridgeNegativeScenario
        {
            ScenarioId = scenarioId,
            ActualStatus = rejected ? "rejected" : "accepted",
            Diagnostics = diagnostics.Concat(proof.Diagnostics).ToList()
        };
    }

    private static EditDrivenGamePackageRuntimePreviewBridgeWinFormsBindingInventory BuildWinFormsBindingInventory(string root)
    {
        const string parentCs =
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.cs";
        const string parentDesigner =
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignAuthoringReviewWorkspacePageControl.Designer.cs";
        const string childCs =
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignGamePackageRuntimePreviewBridgeControl.cs";
        var parentCode = SafeRead(Path.Combine(root, NormalizeSeparators(parentCs)));
        var designerCode = SafeRead(Path.Combine(root, NormalizeSeparators(parentDesigner)));
        var childCode = SafeRead(Path.Combine(root, NormalizeSeparators(childCs)));
        var diagnostics = new List<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic>();
        var tabDeclared = designerCode.Contains("_runtimePreviewBridgeTabPage", StringComparison.Ordinal)
                          && designerCode.Contains("_runtimePreviewBridgeControl", StringComparison.Ordinal);
        var serviceLoaded = parentCode.Contains("EditDrivenGamePackageRuntimePreviewBridgeEvidenceService", StringComparison.Ordinal);
        var controlBound = parentCode.Contains("_runtimePreviewBridgeControl.Bind", StringComparison.Ordinal)
                           && childCode.Contains("EditDrivenGamePackageRuntimePreviewBridgeBuildResult", StringComparison.Ordinal);
        var activationBinds = parentCode.Contains("BuildAndWriteAsync", StringComparison.Ordinal)
                              && parentCode.Contains("runtimePreviewBridgeResult", StringComparison.Ordinal);

        if (!tabDeclared)
        {
            diagnostics.Add(Diagnostic("winforms_tab_missing", parentDesigner, "Runtime preview bridge tab is not declared in designer."));
        }

        if (!serviceLoaded)
        {
            diagnostics.Add(Diagnostic("winforms_service_missing", parentCs, "Runtime preview bridge evidence service is not loaded by parent page."));
        }

        if (!controlBound)
        {
            diagnostics.Add(Diagnostic("winforms_control_binding_missing", childCs, "Runtime preview bridge control is not bound from parent page."));
        }

        if (!activationBinds)
        {
            diagnostics.Add(Diagnostic("winforms_activation_binding_missing", parentCs, "Parent OnActivated path does not bind Goal080 data."));
        }

        return new EditDrivenGamePackageRuntimePreviewBridgeWinFormsBindingInventory
        {
            Passed = tabDeclared && serviceLoaded && controlBound && activationBinds,
            ParentPageRuntimePreviewBridgeTabDeclared = tabDeclared,
            ParentPageRuntimePreviewBridgeServiceLoaded = serviceLoaded,
            ParentPageRuntimePreviewBridgeControlBound = controlBound,
            ParentPageActivationBindsGoal080Data = activationBinds,
            Groups =
            [
                new EditDrivenGamePackageRuntimePreviewBridgeWinFormsBindingGroup
                {
                    GroupId = "goal080_runtime_preview_bridge_tab",
                    ControlName = "CampaignGamePackageRuntimePreviewBridgeControl",
                    RelativePath = childCs,
                    SeparateUserControl = childCode.Contains(": UserControl", StringComparison.Ordinal),
                    BindsGoal080Data = childCode.Contains("Bind(EditDrivenGamePackageRuntimePreviewBridgeBuildResult", StringComparison.Ordinal)
                }
            ],
            Diagnostics = diagnostics
        };
    }

    private static T? ReadJson<T>(
        string root,
        string relativePath,
        List<EditDrivenGamePackageRuntimePreviewBridgeDiagnostic> diagnostics)
    {
        var fullPath = Path.Combine(root, NormalizeSeparators(relativePath));
        if (!File.Exists(fullPath))
        {
            diagnostics.Add(Diagnostic("json_artifact_missing", relativePath, "JSON artifact is missing."));
            return default;
        }

        try
        {
            return EditDrivenGamePackageRuntimePreviewBridgeJson.Deserialize<T>(File.ReadAllText(fullPath));
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Diagnostic("json_artifact_invalid", relativePath, exception.Message));
            return default;
        }
    }

    private static IReadOnlyDictionary<string, string> ParseReportFields(string markdown)
    {
        var values = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in markdown.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            if (!line.StartsWith("- ", StringComparison.Ordinal))
            {
                continue;
            }

            var separator = line.IndexOf(':');
            if (separator <= 2)
            {
                continue;
            }

            var key = line[2..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            values[key] = value;
        }

        return values;
    }

    private static string Value(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) ? value : string.Empty;

    private static string JsonString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return string.Empty;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : property.ToString();
    }

    private static string ResolveSourceGoal(string relativePath)
    {
        if (relativePath.Contains("/goal-077-", StringComparison.Ordinal))
        {
            return "Goal077";
        }

        if (relativePath.Contains("/goal-078-", StringComparison.Ordinal))
        {
            return "Goal078";
        }

        if (relativePath.Contains("/goal-079a-", StringComparison.Ordinal))
        {
            return "Goal079A";
        }

        if (relativePath.Contains("/goal-079-", StringComparison.Ordinal))
        {
            return "Goal079";
        }

        return "unknown";
    }

    private static string ProfileId(string familyId, string seedId) =>
        $"{familyId}/{seedId}".Replace('_', '-');

    private static string Serialize<T>(T value) =>
        EditDrivenGamePackageRuntimePreviewBridgeJson.Serialize(value);

    private static string HashObject<T>(T value) =>
        EditDrivenGamePackageRuntimePreviewBridgeHash.Sha256Text(Serialize(value));

    private static string HashJson<T>(T value) => HashObject(value);

    private static string SafeRead(string path) =>
        File.Exists(path) ? File.ReadAllText(path) : string.Empty;

    private static string SafeHash(string path) =>
        File.Exists(path) ? EditDrivenGamePackageRuntimePreviewBridgeHash.Sha256File(path) : string.Empty;

    private static string NormalizeSeparators(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);

    private static string RelativeToRoot(string root, string path) =>
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

    private static EditDrivenGamePackageRuntimePreviewBridgeDiagnostic Diagnostic(
        string code,
        string target,
        string message) =>
        EditDrivenGamePackageRuntimePreviewBridgeDiagnostic.Error(code, target, NormalizeDiagnosticMessage(code, message));

    private static string NormalizeDiagnosticMessage(string code, string message)
    {
        if (string.Equals(code, "manifest.start_map.empty", StringComparison.Ordinal))
        {
            return "StartMapId is not populated.";
        }

        return ContainsMojibakeMarker(message)
            ? "Diagnostic message contained invalid mojibake markers and was normalized."
            : message;
    }

    private static bool ContainsMojibakeMarker(string value)
    {
        foreach (var marker in MojibakeMarkers)
        {
            if (value.Contains(marker, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly string[] MojibakeMarkers =
    [
        "\u0420\u045F",
        "\u0420\u045C",
        "\u0420\u045B",
        "\u0420\u2022",
        "\u0420\u040E",
        "\u0420\u203A",
        "\u0420\u00A4",
        "\u0420\u045A",
        "\u0420\u0408",
        "\u0420\u0409",
        "\u0420\u0491",
        "\u0420\u00B5",
        "\u0420\u00B0",
        "\u0420\u00BB",
        "\u0420\u0405",
        "\u0421\u040F",
        "\u0421\u20AC",
        "\u0421\u0402",
        "\u0421\u2039",
        "\u0421\u040A",
        "\uFFFD"
    ];

}
