using System.Text;

namespace LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;

public sealed class EditDrivenPlayableReviewPackageMaterializationEvidenceService
{
    public const string ReportMarkdownFileName = "edit-driven-review-package-materialization-report.md";
    public const string ReviewPackageManifestFileName = "review-package-manifest.json";
    public const string PackageFileLedgerFileName = "package-file-ledger.json";
    public const string PlayerReadablePackageIndexFileName = "player-readable-package-index.json";
    public const string PackageTargetCoverageFileName = "package-target-coverage.json";
    public const string StateLineageProofFileName = "state-lineage-proof.json";
    public const string TamperNegativeProofFileName = "tamper-negative-proof.json";
    public const string WinFormsBindingInventoryFileName = "winforms-binding-inventory.json";
    public const string QualityGateScanFileName = "quality-gate-scan.json";
    public const string SourceArtifactManifestFileName = "source-artifact-manifest.json";

    private const string ReviewManifestPath = "review-package/manifest.json";
    private const string PackageIndexPath = "review-package/package-index.json";
    private const string PlayerIndexPath = "review-package/player-readable-index.json";
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly EditDrivenPlayableReviewPackageMaterializationQualityGateScanner _qualityScanner;

    public EditDrivenPlayableReviewPackageMaterializationEvidenceService(
        EditDrivenPlayableReviewPackageMaterializationQualityGateScanner? qualityScanner = null)
    {
        _qualityScanner = qualityScanner ?? new EditDrivenPlayableReviewPackageMaterializationQualityGateScanner();
    }

    public EditDrivenPlayableReviewPackageMaterializationBuildResult Build(string projectRootPath)
    {
        var root = Path.GetFullPath(projectRootPath);
        var source = LoadSource(root);
        var sourceManifest = BuildSourceArtifactManifest(root, source);
        var targets = BuildTargetFiles(source, sourceManifest);
        var targetPayloads = BuildTargetPayloads(targets);
        var packageIndex = BuildPackageIndex(targets, targetPayloads);
        var playerIndex = BuildPlayerIndex(source.Handoff, targets, sourceManifest);
        var reviewPayloadsWithoutManifest = new SortedDictionary<string, string>(targetPayloads, StringComparer.Ordinal)
        {
            [PackageIndexPath] = Serialize(packageIndex),
            [PlayerIndexPath] = Serialize(playerIndex)
        };
        var reviewManifest = BuildReviewPackageManifest(
            sourceManifest,
            targets,
            EntriesFor(reviewPayloadsWithoutManifest, targets));
        reviewPayloadsWithoutManifest[ReviewManifestPath] = Serialize(reviewManifest);
        var reviewPackageFiles = CopySorted(reviewPayloadsWithoutManifest);
        var ledger = BuildPackageFileLedger(reviewPackageFiles, targets);
        var coverage = BuildCoverage(packageIndex);
        var lineage = BuildStateLineageProof(source.State, packageIndex);
        var stagedRead = EditDrivenPlayableReviewPackageMaterializationReadValidator.ValidateReviewPackagePayloads(
            reviewPackageFiles,
            ledger,
            sourceManifest.SourceGoal076ReportHash,
            sourceManifest.SourceGoal076ManifestHash);
        var negative = EditDrivenPlayableReviewPackageMaterializationReadValidator.BuildNegativeProof(
            reviewPackageFiles,
            ledger,
            sourceManifest,
            targets,
            playerIndex);
        var binding = _qualityScanner.BuildWinFormsBindingInventory(root);
        var preQualityArtifacts = BuildArtifactPayloads(
            sourceManifest,
            reviewManifest,
            ledger,
            playerIndex,
            coverage,
            lineage,
            negative,
            binding,
            quality: null);
        var evidencePayloads = CombinePayloads(reviewPackageFiles, preQualityArtifacts);
        var quality = _qualityScanner.Scan(root, targets.Count, evidencePayloads);
        var artifacts = BuildArtifactPayloads(
            sourceManifest,
            reviewManifest,
            ledger,
            playerIndex,
            coverage,
            lineage,
            negative,
            binding,
            quality);
        var reportWithoutHash = BuildReport(
            sourceManifest,
            reviewManifest,
            ledger,
            playerIndex,
            coverage,
            lineage,
            stagedRead,
            negative,
            binding,
            quality);
        var report = reportWithoutHash with { DeterministicHash = Hash(reportWithoutHash) };
        var reportMarkdown = RenderReport(report, stagedRead, negative, quality);

        return new EditDrivenPlayableReviewPackageMaterializationBuildResult
        {
            SourceArtifactManifest = sourceManifest,
            ReviewPackageManifest = reviewManifest,
            PackageFileLedger = ledger,
            PackageIndex = packageIndex,
            PlayerReadablePackageIndex = playerIndex,
            PackageTargetCoverage = coverage,
            StateLineageProof = lineage,
            StagedPackageReadProof = stagedRead,
            TamperNegativeProof = negative,
            WinFormsBindingInventory = binding,
            QualityGateScan = quality,
            Report = report,
            ReportMarkdown = reportMarkdown,
            ReviewPackageFiles = reviewPackageFiles,
            ArtifactJsonByFileName = artifacts
        };
    }

    public async Task<EditDrivenPlayableReviewPackageMaterializationWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<EditDrivenPlayableReviewPackageMaterializationWriteResult> WriteAsync(
        string projectRootPath,
        EditDrivenPlayableReviewPackageMaterializationBuildResult result,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(projectRootPath);
        var outputDirectory = Resolve(root, EditDrivenPlayableReviewPackageMaterializationVocabulary.RelativeOutputDirectory);
        ResetDirectory(outputDirectory);
        var written = new List<string>();

        foreach (var file in result.ReviewPackageFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.GetFullPath(Path.Combine(outputDirectory, file.Key.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, file.Value + Environment.NewLine, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(path);
        }

        foreach (var artifact in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, artifact.Key);
            await File.WriteAllTextAsync(path, artifact.Value + Environment.NewLine, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(reportPath);

        return new EditDrivenPlayableReviewPackageMaterializationWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReviewPackageDirectoryPath = Path.Combine(
                outputDirectory,
                EditDrivenPlayableReviewPackageMaterializationVocabulary.ReviewPackageDirectoryName),
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    public EditDrivenReviewPackageStagedReadProof ReadStagedReviewPackage(
        string outputDirectoryPath,
        string expectedSourceGoal076ReportHash,
        string expectedSourceGoal076ManifestHash)
    {
        var output = Path.GetFullPath(outputDirectoryPath);
        var ledgerPath = Path.Combine(output, PackageFileLedgerFileName);
        if (!File.Exists(ledgerPath))
        {
            return EditDrivenPlayableReviewPackageMaterializationReadValidator.BuildReadProof(
                manifestExists: File.Exists(Path.Combine(output, ReviewManifestPath.Replace('/', Path.DirectorySeparatorChar))),
                packageIndexExists: File.Exists(Path.Combine(output, PackageIndexPath.Replace('/', Path.DirectorySeparatorChar))),
                playerIndexExists: File.Exists(Path.Combine(output, PlayerIndexPath.Replace('/', Path.DirectorySeparatorChar))),
                allLedgerFilesExist: false,
                allFileHashesMatch: false,
                allExpectedRowsPresent: false,
                allExpectedTargetsPresent: false,
                sourceGoal076HashesMatch: false,
                stateLineageValid: false,
                rowCount: 0,
                targetCount: 0,
                diagnostics:
                [
                    Error(
                        "goal077.read.ledger_missing",
                        PackageFileLedgerFileName,
                        "The package file ledger must exist before staged read proof can pass.")
                ]);
        }

        var ledgerJson = File.ReadAllText(ledgerPath, Encoding.UTF8);
        var ledger = Deserialize<EditDrivenReviewPackageFileLedger>(ledgerJson) ?? new EditDrivenReviewPackageFileLedger();
        var payloads = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in ledger.Files)
        {
            var path = Path.GetFullPath(Path.Combine(output, entry.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (path.StartsWith(output, StringComparison.OrdinalIgnoreCase) && File.Exists(path))
            {
                payloads[entry.RelativePath] = File.ReadAllText(path, Encoding.UTF8).TrimEnd('\r', '\n');
            }
        }

        return EditDrivenPlayableReviewPackageMaterializationReadValidator.ValidateReviewPackagePayloads(
            payloads,
            ledger,
            expectedSourceGoal076ReportHash,
            expectedSourceGoal076ManifestHash);
    }

    public static IReadOnlyList<string> RequiredArtifactNames() =>
    [
        ReportMarkdownFileName,
        ReviewPackageManifestFileName,
        PackageFileLedgerFileName,
        PlayerReadablePackageIndexFileName,
        PackageTargetCoverageFileName,
        StateLineageProofFileName,
        TamperNegativeProofFileName,
        WinFormsBindingInventoryFileName,
        QualityGateScanFileName,
        SourceArtifactManifestFileName
    ];

    private static EditDrivenReviewPackageSourceArtifactManifest BuildSourceArtifactManifest(
        string projectRoot,
        SourceBundle source)
    {
        var diagnostics = new List<EditDrivenPlayableReviewPackageDiagnostic>();
        foreach (var missing in source.Artifacts.Where(item => !item.Exists))
        {
            diagnostics.Add(Error(
                "goal077.source.goal076_artifact_missing",
                missing.ArtifactRelativePath,
                "Goal 077 consumes real Goal 076 evidence and requires this artifact."));
        }

        var stateDocs = ReadOptional(projectRoot, "docs/CURRENT_GENERATOR_STATE.md")
            + Environment.NewLine
            + ReadOptional(projectRoot, "docs/CURRENT_GENERATOR_STATE.json")
            + Environment.NewLine
            + ReadOptional(projectRoot, "docs/CONTEXT_INDEX.md")
            + Environment.NewLine
            + ReadOptional(projectRoot, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var handoff = stateDocs.Contains(
            EditDrivenPlayableReviewPackageMaterializationVocabulary.Goal076AcceptedHandoffText,
            StringComparison.Ordinal);
        if (!handoff)
        {
            diagnostics.Add(Error(
                "goal077.preflight.goal076_handoff_missing",
                "docs/CURRENT_GENERATOR_STATE.*",
                "Goal 076 user handoff must be recorded before Goal 077."));
        }

        var reportGreen = source.ReportMarkdown.Contains("implementationStatus: GREEN", StringComparison.Ordinal)
            || source.ReportMarkdown.Contains("implementationStatus=GREEN", StringComparison.Ordinal);
        if (!reportGreen || source.PreviewManifest.ImplementationStatus != "GREEN")
        {
            diagnostics.Add(Error(
                "goal077.preflight.goal076_not_green",
                "Goal076.report",
                "Goal 076 evidence must be GREEN before Goal 077 can materialize a review package."));
        }

        var acceptedFalse = source.ReportMarkdown.Contains("accepted: false", StringComparison.Ordinal)
            && !source.PreviewManifest.Accepted;
        if (!acceptedFalse)
        {
            diagnostics.Add(Error(
                "goal077.preflight.goal076_artifact_acceptance_mutated",
                "Goal076.report",
                "Goal 076 artifact must remain accepted=false; only current-state handoff records acceptance."));
        }

        if (!source.RefreshPlan.Passed || source.RefreshPlan.RowCount != 9 || source.RefreshPlan.TargetCount != 18)
        {
            diagnostics.Add(Error(
                "goal077.preflight.goal076_refresh_plan_invalid",
                "gamepackage-refresh-plan.json",
                "Goal 076 refresh plan must have 9 rows and 18 package targets."));
        }

        return new EditDrivenReviewPackageSourceArtifactManifest
        {
            Goal076AcceptedByUserHandoff = handoff,
            Goal076ReportWasGreenProducedForReview = reportGreen,
            Goal076ArtifactAcceptedFalse = acceptedFalse,
            SourceGoal076ReportHash = source.ReportHash,
            SourceGoal076ManifestHash = source.PreviewManifestHash,
            SourceGoal076RefreshPlanHash = source.RefreshPlanHash,
            SourceGoal076HandoffManifestHash = source.HandoffManifestHash,
            SourceArtifactCount = source.Artifacts.Count,
            SourceArtifacts = source.Artifacts,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyList<EditDrivenReviewPackageTargetFile> BuildTargetFiles(
        SourceBundle source,
        EditDrivenReviewPackageSourceArtifactManifest sourceManifest)
    {
        var stateByRow = source.State.Rows.ToDictionary(row => row.RowId, row => row, StringComparer.Ordinal);
        var index = 0;
        return source.RefreshPlan.Rows
            .OrderBy(row => EditDrivenPlayableReviewPackageMaterializationVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => EditDrivenPlayableReviewPackageMaterializationVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .SelectMany(row =>
            {
                stateByRow.TryGetValue(row.RowId, out var state);
                return row.Targets
                    .OrderBy(target => target.LogicalPackagePath, StringComparer.Ordinal)
                    .ThenBy(target => target.FieldId, StringComparer.Ordinal)
                    .Select(target =>
                    {
                        index++;
                        return new EditDrivenReviewPackageTargetFile
                        {
                            TargetId = "target-" + index.ToString("000"),
                            SourceRowId = row.RowId,
                            FamilyId = row.FamilyId,
                            SeedId = row.SeedId,
                            FieldId = target.FieldId,
                            DomainId = target.DomainId,
                            LogicalPackagePath = target.LogicalPackagePath,
                            BeforeValue = target.BeforeValue,
                            AfterValue = target.AfterValue,
                            BeforeHash = state?.BeforeHash ?? string.Empty,
                            AfterHash = state?.AfterHash ?? row.SourceAfterHash,
                            RollbackHash = state?.RollbackHash ?? string.Empty,
                            ReplayHash = state?.ReplayHash ?? row.SourceAfterHash,
                            ValidationRequirement = target.ValidationRequirement,
                            SourceGoal076ReportHash = sourceManifest.SourceGoal076ReportHash,
                            SourceGoal076ManifestHash = sourceManifest.SourceGoal076ManifestHash,
                            SourceGoal076RefreshPlanHash = sourceManifest.SourceGoal076RefreshPlanHash,
                            SourceGoal076HandoffManifestHash = sourceManifest.SourceGoal076HandoffManifestHash
                        };
                    });
            })
            .ToList();
    }

    private static SortedDictionary<string, string> BuildTargetPayloads(
        IReadOnlyList<EditDrivenReviewPackageTargetFile> targets)
    {
        var payloads = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var target in targets)
        {
            payloads[TargetRelativePath(target)] = Serialize(target);
        }

        return payloads;
    }

    private static EditDrivenReviewPackageIndex BuildPackageIndex(
        IReadOnlyList<EditDrivenReviewPackageTargetFile> targets,
        IReadOnlyDictionary<string, string> targetPayloads)
    {
        var rows = targets
            .GroupBy(target => target.SourceRowId, StringComparer.Ordinal)
            .Select(group =>
            {
                var first = group.OrderBy(item => item.TargetId, StringComparer.Ordinal).First();
                return new EditDrivenReviewPackageIndexRow
                {
                    RowId = first.SourceRowId,
                    FamilyId = first.FamilyId,
                    SeedId = first.SeedId,
                    Targets = group
                        .OrderBy(item => item.TargetId, StringComparer.Ordinal)
                        .Select(target =>
                        {
                            var relativePath = TargetRelativePath(target);
                            return new EditDrivenReviewPackageIndexTarget
                            {
                                TargetId = target.TargetId,
                                RelativePath = relativePath,
                                LogicalPackagePath = target.LogicalPackagePath,
                                Sha256 = Hash(targetPayloads[relativePath])
                            };
                        })
                        .ToList()
                };
            })
            .OrderBy(row => EditDrivenPlayableReviewPackageMaterializationVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => EditDrivenPlayableReviewPackageMaterializationVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .ToList();

        return new EditDrivenReviewPackageIndex
        {
            Passed = rows.Count == 9 && rows.Sum(row => row.Targets.Count) == 18,
            RowCount = rows.Count,
            TargetCount = rows.Sum(row => row.Targets.Count),
            Rows = rows
        };
    }

    private static EditDrivenPlayerReadablePackageIndex BuildPlayerIndex(
        EditDrivenGoal076HandoffManifest handoff,
        IReadOnlyList<EditDrivenReviewPackageTargetFile> targets,
        EditDrivenReviewPackageSourceArtifactManifest sourceManifest)
    {
        var targetsByRow = targets
            .GroupBy(target => target.SourceRowId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.OrderBy(item => item.TargetId, StringComparer.Ordinal).ToList(), StringComparer.Ordinal);
        var scenarios = handoff.Rows
            .OrderBy(row => EditDrivenPlayableReviewPackageMaterializationVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => EditDrivenPlayableReviewPackageMaterializationVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row =>
            {
                targetsByRow.TryGetValue(row.RowId, out var rowTargets);
                rowTargets ??= [];
                var targetIds = rowTargets.Select(target => target.TargetId).ToList();
                return new EditDrivenPlayerScenarioMapping
                {
                    ScenarioId = row.FamilyId + "/" + row.SeedId,
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    SeedId = row.SeedId,
                    TargetIds = targetIds,
                    TargetFileRefs = rowTargets.Select(TargetRelativePath).ToList(),
                    PlayerMarkers = row.ExpectedPlayerMarkers
                        .Select(marker => new EditDrivenPlayerMarkerReference
                        {
                            Marker = marker,
                            RowId = row.RowId,
                            TargetIds = targetIds
                        })
                        .ToList()
                };
            })
            .ToList();
        var targetCount = scenarios.Sum(item => item.TargetIds.Count);
        var markerCount = scenarios.Sum(item => item.PlayerMarkers.Count);

        return new EditDrivenPlayerReadablePackageIndex
        {
            Passed = scenarios.Count == 9 && targetCount == 18 && markerCount > 0,
            SourceGoal076ReportHash = sourceManifest.SourceGoal076ReportHash,
            SourceGoal076HandoffManifestHash = sourceManifest.SourceGoal076HandoffManifestHash,
            ScenarioCount = scenarios.Count,
            RowCount = scenarios.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count(),
            TargetCount = targetCount,
            AllScenarioIdsMapped = scenarios.Count == handoff.PlayerFacingScenarioIds.Count,
            AllPlayerMarkersResolved = scenarios.All(item => item.PlayerMarkers.Count > 0 && item.TargetIds.Count > 0),
            AllRowsRepresented = scenarios.Select(item => item.RowId).Distinct(StringComparer.Ordinal).Count() == 9,
            AllTargetsRepresented = targetCount == 18,
            Scenarios = scenarios
        };
    }

    private static EditDrivenReviewPackageManifest BuildReviewPackageManifest(
        EditDrivenReviewPackageSourceArtifactManifest source,
        IReadOnlyList<EditDrivenReviewPackageTargetFile> targets,
        IReadOnlyList<EditDrivenReviewPackageFileEntry> files)
    {
        var packageHash = Hash(files.Select(file => file.RelativePath + ":" + file.Sha256).Order(StringComparer.Ordinal).ToArray());
        return new EditDrivenReviewPackageManifest
        {
            SourceGoal076ReportHash = source.SourceGoal076ReportHash,
            SourceGoal076ManifestHash = source.SourceGoal076ManifestHash,
            PackageHash = packageHash,
            RowCount = targets.Select(item => item.SourceRowId).Distinct(StringComparer.Ordinal).Count(),
            TargetCount = targets.Count,
            FileCount = files.Count,
            Files = files
        };
    }

    private static EditDrivenReviewPackageFileLedger BuildPackageFileLedger(
        IReadOnlyDictionary<string, string> reviewPackageFiles,
        IReadOnlyList<EditDrivenReviewPackageTargetFile> targets)
    {
        var entries = EntriesFor(reviewPackageFiles, targets);
        return new EditDrivenReviewPackageFileLedger
        {
            Passed = entries.Count > 0 && entries.All(item => !string.IsNullOrWhiteSpace(item.Sha256)),
            FileCount = entries.Count,
            Files = entries
        };
    }

    private static IReadOnlyList<EditDrivenReviewPackageFileEntry> EntriesFor(
        IReadOnlyDictionary<string, string> files,
        IReadOnlyList<EditDrivenReviewPackageTargetFile> targets)
    {
        var targetByPath = targets.ToDictionary(TargetRelativePath, target => target, StringComparer.Ordinal);
        return files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item =>
            {
                targetByPath.TryGetValue(item.Key, out var target);
                return new EditDrivenReviewPackageFileEntry
                {
                    RelativePath = item.Key,
                    Role = target is null ? RoleFor(item.Key) : "target",
                    Sha256 = Hash(item.Value),
                    ByteCount = Encoding.UTF8.GetByteCount(item.Value),
                    RowId = target?.SourceRowId ?? string.Empty,
                    TargetId = target?.TargetId ?? string.Empty
                };
            })
            .ToList();
    }

    private static EditDrivenPackageTargetCoverage BuildCoverage(EditDrivenReviewPackageIndex packageIndex)
    {
        var rows = packageIndex.Rows
            .Select(row => new EditDrivenPackageTargetCoverageRow
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                TargetCount = row.Targets.Count,
                TargetIds = row.Targets.Select(target => target.TargetId).ToList()
            })
            .ToList();
        return new EditDrivenPackageTargetCoverage
        {
            Passed = rows.Count == 9 && rows.Sum(row => row.TargetCount) == 18,
            RowCount = rows.Count,
            TargetCount = rows.Sum(row => row.TargetCount),
            Rows = rows
        };
    }

    private static EditDrivenReviewPackageStateLineageProof BuildStateLineageProof(
        EditDrivenGoal076StateTransitionProof state,
        EditDrivenReviewPackageIndex packageIndex)
    {
        var rowTargetCounts = packageIndex.Rows.ToDictionary(row => row.RowId, row => row.Targets.Count, StringComparer.Ordinal);
        var rows = state.Rows
            .OrderBy(row => EditDrivenPlayableReviewPackageMaterializationVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => EditDrivenPlayableReviewPackageMaterializationVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row => new EditDrivenReviewPackageStateLineageRow
            {
                RowId = row.RowId,
                BeforeHash = row.BeforeHash,
                AfterHash = row.AfterHash,
                RollbackHash = row.RollbackHash,
                ReplayHash = row.ReplayHash,
                StateChanged = row.StateChanged,
                RollbackRestored = row.RollbackHash == row.BeforeHash,
                ReplayRestoredAfter = row.ReplayHash == row.AfterHash
            })
            .ToList();
        return new EditDrivenReviewPackageStateLineageProof
        {
            Passed = rows.Count == 9
                && rows.All(row => row.StateChanged && row.RollbackRestored && row.ReplayRestoredAfter)
                && rowTargetCounts.Values.Sum() == 18,
            RowCount = rows.Count,
            TargetCount = rowTargetCounts.Values.Sum(),
            Rows = rows
        };
    }

    private static EditDrivenReviewPackageMaterializationReport BuildReport(
        EditDrivenReviewPackageSourceArtifactManifest source,
        EditDrivenReviewPackageManifest manifest,
        EditDrivenReviewPackageFileLedger ledger,
        EditDrivenPlayerReadablePackageIndex playerIndex,
        EditDrivenPackageTargetCoverage coverage,
        EditDrivenReviewPackageStateLineageProof lineage,
        EditDrivenReviewPackageStagedReadProof readProof,
        EditDrivenReviewPackageNegativeProof negative,
        EditDrivenReviewPackageWinFormsBindingInventory binding,
        EditDrivenReviewPackageQualityGateScan quality)
    {
        var diagnostics = SortDiagnostics(
            source.Diagnostics
                .Concat(readProof.Diagnostics)
                .Concat(negative.Scenarios.SelectMany(item => item.Diagnostics).Where(item => item.Severity == "critical"))
                .Concat(binding.Diagnostics)
                .Concat(quality.Diagnostics));
        var green = diagnostics.All(item => item.Severity != "error")
            && source.Goal076AcceptedByUserHandoff
            && source.Goal076ReportWasGreenProducedForReview
            && source.Goal076ArtifactAcceptedFalse
            && manifest.TargetCount == 18
            && ledger.Passed
            && playerIndex.Passed
            && coverage.Passed
            && lineage.Passed
            && readProof.Passed
            && negative.Passed
            && binding.Passed
            && quality.Passed;

        return new EditDrivenReviewPackageMaterializationReport
        {
            ImplementationStatus = green ? "GREEN" : "BLOCKED",
            Accepted = false,
            Goal076AcceptedByUserHandoff = source.Goal076AcceptedByUserHandoff,
            Goal076ImplementationGreen = source.Goal076ReportWasGreenProducedForReview,
            RowCount = coverage.RowCount,
            TargetCount = coverage.TargetCount,
            ReviewPackageFileCount = ledger.FileCount,
            SourceGoal076ReportHash = source.SourceGoal076ReportHash,
            SourceGoal076ManifestHash = source.SourceGoal076ManifestHash,
            ReviewPackageManifestHash = Hash(manifest),
            PackageFileLedgerHash = Hash(ledger),
            PlayerReadablePackageIndexHash = Hash(playerIndex),
            PackageTargetCoverageHash = Hash(coverage),
            StateLineageProofHash = Hash(lineage),
            StagedPackageReadProofHash = Hash(readProof),
            TamperNegativeProofHash = Hash(negative),
            WinFormsBindingInventoryHash = Hash(binding),
            QualityGateScanHash = Hash(quality),
            Diagnostics = diagnostics
        };
    }

    private static string RenderReport(
        EditDrivenReviewPackageMaterializationReport report,
        EditDrivenReviewPackageStagedReadProof readProof,
        EditDrivenReviewPackageNegativeProof negative,
        EditDrivenReviewPackageQualityGateScan quality)
    {
        var lines = new List<string>
        {
            "# Goal 077 Edit-Driven Review Package Materialization",
            string.Empty,
            "- gate: " + EditDrivenPlayableReviewPackageMaterializationVocabulary.FinalGate + " required",
            "- accepted: false",
            "- implementationStatus: " + report.ImplementationStatus,
            "- goal076Handoff: " + report.Goal076AcceptedByUserHandoff,
            "- goal076ImplementationGreen: " + report.Goal076ImplementationGreen,
            "- rowCount: " + report.RowCount,
            "- targetCount: " + report.TargetCount,
            "- reviewPackageFileCount: " + report.ReviewPackageFileCount,
            "- sourceGoal076ReportHash: " + report.SourceGoal076ReportHash,
            "- sourceGoal076ManifestHash: " + report.SourceGoal076ManifestHash,
            "- reviewPackageManifestHash: " + report.ReviewPackageManifestHash,
            "- packageFileLedgerHash: " + report.PackageFileLedgerHash,
            "- playerReadablePackageIndexHash: " + report.PlayerReadablePackageIndexHash,
            "- stateLineageProofHash: " + report.StateLineageProofHash,
            "- stagedPackageReadProofHash: " + report.StagedPackageReadProofHash,
            "- tamperNegativeProofHash: " + report.TamperNegativeProofHash,
            "- qualityGateScanHash: " + report.QualityGateScanHash,
            "- reportHash: " + report.DeterministicHash,
            string.Empty,
            "## Proof",
            "- stagedPackageReadProofPassed: " + readProof.Passed,
            "- allLedgerFilesExist: " + readProof.AllLedgerFilesExist,
            "- allFileHashesMatch: " + readProof.AllFileHashesMatch,
            "- allExpectedRowsPresent: " + readProof.AllExpectedRowsPresent,
            "- allExpectedTargetsPresent: " + readProof.AllExpectedTargetsPresent,
            "- sourceGoal076HashesMatch: " + readProof.SourceGoal076HashesMatch,
            "- stateLineageValid: " + readProof.StateLineageValid,
            string.Empty,
            "## Negative Proof"
        };
        lines.AddRange(negative.Scenarios.Select(item => "- " + item.ScenarioId + ": " + item.ActualStatus));
        lines.AddRange(
        [
            string.Empty,
            "## Quality",
            "- maxLineLength: " + quality.MaxLineLength,
            "- minifiedSourceFileCount: " + quality.MinifiedSourceFileCount,
            "- filesOver1000LinesCount: " + quality.FilesOver1000LinesCount,
            "- alphaRuntimeBootstrapLineCount: " + quality.AlphaRuntimeBootstrapLineCount,
            "- alphaRuntimeBootstrapHash: " + quality.AlphaRuntimeBootstrapHash,
            "- alphaRuntimeBootstrapNoChangeStatus: " + quality.AlphaRuntimeBootstrapNoChangeStatus,
            "- absoluteLocalPaths: " + quality.EvidenceContainsAbsoluteLocalPaths,
            "- timestampLikeValues: " + quality.EvidenceContainsTimestampLikeValues,
            "- heavyLogs: " + quality.EvidenceContainsHeavyLogs,
            "- scratchTamperFiles: " + quality.EvidenceContainsScratchTamperFiles,
            string.Empty,
            "## Diagnostics"
        ]);
        lines.AddRange(report.Diagnostics.Count == 0
            ? ["- none"]
            : report.Diagnostics.Select(item => "- " + item.Severity + ": " + item.Code + " [" + item.Target + "]"));
        lines.Add(string.Empty);
        lines.Add(EditDrivenPlayableReviewPackageMaterializationVocabulary.FinalGate + " required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static SortedDictionary<string, string> BuildArtifactPayloads(
        EditDrivenReviewPackageSourceArtifactManifest source,
        EditDrivenReviewPackageManifest manifest,
        EditDrivenReviewPackageFileLedger ledger,
        EditDrivenPlayerReadablePackageIndex playerIndex,
        EditDrivenPackageTargetCoverage coverage,
        EditDrivenReviewPackageStateLineageProof lineage,
        EditDrivenReviewPackageNegativeProof negative,
        EditDrivenReviewPackageWinFormsBindingInventory binding,
        EditDrivenReviewPackageQualityGateScan? quality)
    {
        var artifacts = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceArtifactManifestFileName] = Serialize(source),
            [ReviewPackageManifestFileName] = Serialize(manifest),
            [PackageFileLedgerFileName] = Serialize(ledger),
            [PlayerReadablePackageIndexFileName] = Serialize(playerIndex),
            [PackageTargetCoverageFileName] = Serialize(coverage),
            [StateLineageProofFileName] = Serialize(lineage),
            [TamperNegativeProofFileName] = Serialize(negative),
            [WinFormsBindingInventoryFileName] = Serialize(binding)
        };
        if (quality is not null)
        {
            artifacts[QualityGateScanFileName] = Serialize(quality);
        }

        return artifacts;
    }

    private SourceBundle LoadSource(string projectRoot)
    {
        var artifacts = EditDrivenPlayableReviewPackageMaterializationVocabulary.RequiredSourceArtifactNames
            .Select(name => ReadGoal076Artifact(projectRoot, name))
            .OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
            .ToList();
        var report = ReadText(projectRoot, "edit-driven-playable-preview-refresh-report.md");
        var manifestJson = ReadText(projectRoot, "playable-preview-refresh-manifest.json");
        var planJson = ReadText(projectRoot, "gamepackage-refresh-plan.json");
        var handoffJson = ReadText(projectRoot, "unity-player-handoff-manifest.json");
        var stateJson = ReadText(projectRoot, "state-transition-proof.json");
        return new SourceBundle(
            report,
            Deserialize<EditDrivenGoal076PreviewManifest>(manifestJson) ?? new EditDrivenGoal076PreviewManifest(),
            Deserialize<EditDrivenGoal076RefreshPlan>(planJson) ?? new EditDrivenGoal076RefreshPlan(),
            Deserialize<EditDrivenGoal076HandoffManifest>(handoffJson) ?? new EditDrivenGoal076HandoffManifest(),
            Deserialize<EditDrivenGoal076StateTransitionProof>(stateJson) ?? new EditDrivenGoal076StateTransitionProof(),
            Hash(report),
            Hash(manifestJson),
            Hash(planJson),
            Hash(handoffJson),
            artifacts);
    }

    private static string ReadText(string projectRoot, string fileName)
    {
        var path = Resolve(
            projectRoot,
            EditDrivenPlayableReviewPackageMaterializationVocabulary.Goal076RelativeOutputDirectory + "/" + fileName);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8).TrimEnd('\r', '\n') : string.Empty;
    }

    private static EditDrivenReviewPackageSourceArtifactReference ReadGoal076Artifact(string projectRoot, string fileName)
    {
        var relativePath = EditDrivenPlayableReviewPackageMaterializationVocabulary.Goal076RelativeOutputDirectory
            + "/" + fileName;
        var path = Resolve(projectRoot, relativePath);
        if (!File.Exists(path))
        {
            return new EditDrivenReviewPackageSourceArtifactReference
            {
                ArtifactFamily = Path.GetFileNameWithoutExtension(fileName),
                ArtifactRelativePath = relativePath,
                Exists = false
            };
        }

        return new EditDrivenReviewPackageSourceArtifactReference
        {
            ArtifactFamily = Path.GetFileNameWithoutExtension(fileName),
            ArtifactRelativePath = relativePath,
            ArtifactHash = EditDrivenPlayableReviewPackageMaterializationHash.Sha256(File.ReadAllBytes(path)),
            Exists = true
        };
    }

    private static SortedDictionary<string, string> CopySorted(IReadOnlyDictionary<string, string> values)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            result[value.Key] = value.Value;
        }

        return result;
    }

    private static SortedDictionary<string, string> CombinePayloads(
        IReadOnlyDictionary<string, string> reviewPackageFiles,
        IReadOnlyDictionary<string, string> artifacts)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in reviewPackageFiles)
        {
            result[item.Key] = item.Value;
        }

        foreach (var item in artifacts)
        {
            result[item.Key] = item.Value;
        }

        return result;
    }

    private static string TargetRelativePath(EditDrivenReviewPackageTargetFile target) =>
        "review-package/targets/"
        + EditDrivenPlayableReviewPackageMaterializationHash.SafeSegment(target.FamilyId) + "/"
        + EditDrivenPlayableReviewPackageMaterializationHash.SafeSegment(target.SourceRowId) + "/"
        + EditDrivenPlayableReviewPackageMaterializationHash.SafeSegment(target.TargetId + "-" + target.DomainId + "-" + target.FieldId)
        + ".json";

    private static string RoleFor(string relativePath) =>
        relativePath switch
        {
            ReviewManifestPath => "manifest",
            PackageIndexPath => "package-index",
            PlayerIndexPath => "player-readable-index",
            _ => "review-package-file"
        };

    private static string Serialize<T>(T value) =>
        EditDrivenPlayableReviewPackageMaterializationHash.Serialize(value);

    private static T? Deserialize<T>(string json) =>
        EditDrivenPlayableReviewPackageMaterializationHash.Deserialize<T>(json);

    private static string Hash<T>(T value) =>
        EditDrivenPlayableReviewPackageMaterializationHash.Sha256(Serialize(value));

    private static string Hash(string text) =>
        EditDrivenPlayableReviewPackageMaterializationHash.Sha256(text);

    private static IReadOnlyList<EditDrivenPlayableReviewPackageDiagnostic> SortDiagnostics(
        IEnumerable<EditDrivenPlayableReviewPackageDiagnostic> diagnostics) =>
        EditDrivenPlayableReviewPackageMaterializationQualityGateScanner.SortDiagnostics(diagnostics);

    private static string ReadOptional(string projectRoot, string relativePath)
    {
        var path = Resolve(projectRoot, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Resolve(string projectRoot, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, path);
        return path;
    }

    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static void EnsureContained(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + normalizedPath);
        }
    }

    private static EditDrivenPlayableReviewPackageDiagnostic Error(string code, string target, string message) =>
        EditDrivenPlayableReviewPackageDiagnostic.Error(code, target, message);

    private sealed record SourceBundle(
        string ReportMarkdown,
        EditDrivenGoal076PreviewManifest PreviewManifest,
        EditDrivenGoal076RefreshPlan RefreshPlan,
        EditDrivenGoal076HandoffManifest Handoff,
        EditDrivenGoal076StateTransitionProof State,
        string ReportHash,
        string PreviewManifestHash,
        string RefreshPlanHash,
        string HandoffManifestHash,
        IReadOnlyList<EditDrivenReviewPackageSourceArtifactReference> Artifacts);
}
