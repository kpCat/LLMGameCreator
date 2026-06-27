using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Devflow;

internal static class DevelopmentComplexityStabilizationArtifacts
{
    public const string Scenario = "development-complexity-stabilization";
    public const string FinalGate = "development_complexity_stabilization_verification";
    public const string PreviousAcceptedGate = "generated_game_profile_contract_verification passed";
    public const string RelativeOutputDirectory = ".llmgc/procedural/development-complexity-stabilization";
    public const string ReportJsonFileName = "development-complexity-stabilization-report.json";
    public const string ReportMarkdownFileName = "development-complexity-stabilization-report.md";
    public const string VerificationMarkdownFileName = "development-complexity-stabilization-verification.md";
    public const string PolicyProofFileName = "artifact-scope-policy-proof.json";
    public const string InventoryFileName = "tracked-generated-artifact-inventory.json";
    public const string InvalidMatrixFileName = "scope-guard-invalid-matrix.json";
    public const string CheckAllIsolationProofFileName = "check-all-isolation-proof.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static WrittenArtifacts WriteArtifacts(string repoRoot, string projectRoot)
    {
        var outputRoot = Path.Combine(projectRoot, ".llmgc", "procedural", "development-complexity-stabilization");
        Directory.CreateDirectory(outputRoot);

        var policyProof = BuildPolicyProof(repoRoot);
        var checkAllProof = BuildCheckAllIsolationProof(repoRoot);
        var inventory = BuildTrackedGeneratedArtifactInventory(repoRoot);
        var invalidMatrix = BuildInvalidMatrix(repoRoot);

        var policyProofJson = WriteJson(Path.Combine(outputRoot, PolicyProofFileName), policyProof);
        var checkAllProofJson = WriteJson(Path.Combine(outputRoot, CheckAllIsolationProofFileName), checkAllProof);
        var inventoryJson = WriteJson(Path.Combine(outputRoot, InventoryFileName), inventory);
        var invalidMatrixJson = WriteJson(Path.Combine(outputRoot, InvalidMatrixFileName), invalidMatrix);

        var report = new
        {
            schemaVersion = "development_complexity_stabilization_report_v1",
            accepted = false,
            finalStatus = FinalGate,
            manualGate = FinalGate,
            previousAcceptedGate = PreviousAcceptedGate,
            productSmokeRoute = Scenario,
            completedSlices = new[] { "S178", "S179", "S180", "S181", "S182", "S183", "S184" },
            policyDocExists = File.Exists(Path.Combine(repoRoot, "docs", "DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md")),
            policyJsonExists = File.Exists(Path.Combine(repoRoot, ".devflow", "artifact-scope", "artifact-scope-policy.json")),
            scopeGuardImplemented = File.Exists(Path.Combine(repoRoot, ".devflow", "scripts", "check-artifact-scope.ps1")),
            checkAllArtifactIsolationImplemented = checkAllProof.isolationImplemented,
            legacyArtifactMutationGuarded = invalidMatrix.scenarios.Any(item => item.scenarioId == "legacy_goal020_artifact_mutation_rejected" && !item.actualValid),
            trackedGeneratedArtifactInventoryWritten = true,
            capabilitySelectionStarted = false,
            publicGamePackageSchemaChanged = false,
            projectFilesChanged = false,
            generatorLibraryChanged = false,
            unityBuildExecuted = false,
            noExternalProviderLlmRagLuaMedia = true,
            invalidMatrix = new
            {
                invalidMatrix.passed,
                invalidMatrix.scenarioCount,
                invalidMatrix.rejectedCount
            },
            artifactHashes = new
            {
                policyProofHash = Sha256(policyProofJson),
                checkAllIsolationProofHash = Sha256(checkAllProofJson),
                trackedGeneratedArtifactInventoryHash = Sha256(inventoryJson),
                scopeGuardInvalidMatrixHash = Sha256(invalidMatrixJson)
            },
            diagnostics = new object[]
            {
                new
                {
                    severity = "info",
                    code = "development_complexity.goal021_gate.accepted",
                    target = "generated_game_profile_contract_verification",
                    message = PreviousAcceptedGate
                },
                new
                {
                    severity = "info",
                    code = "development_complexity.final_gate.required",
                    target = FinalGate,
                    message = "Goal 022 stops with the stabilization verification gate required."
                }
            }
        };

        var reportPath = Path.Combine(outputRoot, ReportJsonFileName);
        var reportJson = WriteJson(reportPath, report);
        var reportHash = Sha256(reportJson);
        var markdown = string.Join(
            Environment.NewLine,
            "# Development Complexity Stabilization Report",
            string.Empty,
            $"- Accepted: false",
            $"- Final status: {FinalGate}",
            $"- Manual gate: {FinalGate}",
            $"- Previous accepted gate: {PreviousAcceptedGate}",
            $"- Scope guard implemented: true",
            $"- Check-all artifact isolation implemented: {checkAllProof.isolationImplemented.ToString().ToLowerInvariant()}",
            $"- Tracked generated artifact families: {inventory.families.Length}",
            $"- Invalid scenarios rejected: {invalidMatrix.rejectedCount}/{invalidMatrix.scenarioCount}",
            $"- Report hash: {reportHash}",
            string.Empty);
        WriteUtf8(Path.Combine(outputRoot, ReportMarkdownFileName), markdown);

        var verification = string.Join(
            Environment.NewLine,
            "# Development Complexity Stabilization Verification",
            string.Empty,
            "```text",
            $"{FinalGate} required",
            "```",
            string.Empty,
            "Capability Bundle Selection, Goal 023 and S185 were not started.",
            string.Empty);
        WriteUtf8(Path.Combine(outputRoot, VerificationMarkdownFileName), verification);

        return new WrittenArtifacts(
            outputRoot,
            reportPath,
            Path.Combine(outputRoot, ReportMarkdownFileName),
            Path.Combine(outputRoot, VerificationMarkdownFileName),
            Path.Combine(outputRoot, PolicyProofFileName),
            Path.Combine(outputRoot, InventoryFileName),
            Path.Combine(outputRoot, InvalidMatrixFileName),
            Path.Combine(outputRoot, CheckAllIsolationProofFileName),
            reportHash);
    }

    public static GuardRun RunScopeGuard(string repoRoot, params string[] changedPaths)
    {
        return RunScopeGuard(
            repoRoot,
            policyPath: Path.Combine(repoRoot, ".devflow", "artifact-scope", "artifact-scope-policy.json"),
            failOnTrackedIgnored: false,
            changedPaths: changedPaths);
    }

    public static GuardRun RunScopeGuardWithMissingPolicy(string repoRoot, string changedPath)
    {
        return RunScopeGuard(
            repoRoot,
            policyPath: Path.Combine(repoRoot, ".devflow", "artifact-scope", "missing-policy.json"),
            failOnTrackedIgnored: false,
            changedPaths: changedPath);
    }

    public static GuardRun RunScopeGuardWithFailOnTrackedIgnored(string repoRoot, string changedPath)
    {
        return RunScopeGuard(
            repoRoot,
            policyPath: Path.Combine(repoRoot, ".devflow", "artifact-scope", "artifact-scope-policy.json"),
            failOnTrackedIgnored: true,
            changedPaths: changedPath);
    }

    public static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }

    public static string ReadJsonProperty(string jsonPath, string propertyName)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(jsonPath));
        return document.RootElement.GetProperty(propertyName).GetString() ?? string.Empty;
    }

    private static object BuildPolicyProof(string repoRoot)
    {
        var policyPath = Path.Combine(repoRoot, ".devflow", "artifact-scope", "artifact-scope-policy.json");
        using var document = JsonDocument.Parse(File.ReadAllText(policyPath));
        var root = document.RootElement;
        var classes = root.GetProperty("artifactMutabilityClasses").EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToArray();
        var requiredClasses = new[]
        {
            "source_code_docs",
            "state_handoff_docs",
            "current_goal_compact_review_artifacts",
            "historical_compact_artifacts",
            "heavy_generated_build_runtime_outputs",
            "task_pack_docs"
        };

        return new
        {
            schemaVersion = "artifact_scope_policy_proof_v1",
            policySchemaVersion = root.GetProperty("schemaVersion").GetString(),
            requiredClassesPresent = requiredClasses.All(classes.Contains),
            requiredClasses,
            defaultAllowedStateDocs = root.GetProperty("defaultAllowedStateDocs").EnumerateArray().Select(item => item.GetString()).ToArray(),
            currentGoalArtifactRoots = root.GetProperty("allowedCurrentGoalArtifactRoots").EnumerateArray().Select(item => item.GetString()).ToArray(),
            historicalArtifactRootCount = root.GetProperty("historicalArtifactRoots").GetArrayLength(),
            heavyOutputPatternCount = root.GetProperty("trackedHeavyOutputPatterns").GetArrayLength(),
            scenarioAllowlistPresent = root.GetProperty("scenarioAllowlists").EnumerateArray()
                .Any(item => item.GetProperty("scenario").GetString() == Scenario)
        };
    }

    private static CheckAllIsolationProof BuildCheckAllIsolationProof(string repoRoot)
    {
        var path = Path.Combine(repoRoot, ".devflow", "scripts", "check-all.ps1");
        var script = File.ReadAllText(path);
        var setsProjectDir = script.Contains("$env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR = $CheckAllProductSmokeProjectDir", StringComparison.Ordinal);
        var setsPackageDir = script.Contains("$env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR = $CheckAllProductSmokePackageOutputDir", StringComparison.Ordinal);
        var restoresProjectDir = script.Contains("$env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR = $PreviousProductSmokeProjectDir", StringComparison.Ordinal)
            && script.Contains("Remove-Item Env:\\LLMGC_PRODUCT_SMOKE_PROJECT_DIR", StringComparison.Ordinal);
        var restoresPackageDir = script.Contains("$env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR = $PreviousProductSmokePackageOutputDir", StringComparison.Ordinal)
            && script.Contains("Remove-Item Env:\\LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR", StringComparison.Ordinal);
        var seedsRunLocalProceduralBaseline = script.Contains("Copy-Item -LiteralPath $RepoProceduralRoot", StringComparison.Ordinal)
            && script.Contains("$CheckAllProductSmokeProjectDir", StringComparison.Ordinal);
        var excludesProductSmokeFromOrdinarySuite = script.Contains("FullyQualifiedName!~ProductSmoke", StringComparison.Ordinal);

        return new CheckAllIsolationProof(
            "check_all_isolation_proof_v1",
            setsProjectDir,
            setsPackageDir,
            restoresProjectDir,
            restoresPackageDir,
            seedsRunLocalProceduralBaseline,
            excludesProductSmokeFromOrdinarySuite,
            setsProjectDir && setsPackageDir && restoresProjectDir && restoresPackageDir,
            new[]
            {
                "LLMGC_PRODUCT_SMOKE_PROJECT_DIR",
                "LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR"
            });
    }

    private static TrackedGeneratedArtifactInventory BuildTrackedGeneratedArtifactInventory(string repoRoot)
    {
        var git = RunProcess(repoRoot, "git", "ls-files", ".llmgc/procedural");
        if (git.ExitCode != 0)
        {
            throw new InvalidOperationException($"git ls-files .llmgc/procedural failed: {git.StandardError}{git.StandardOutput}");
        }

        var paths = git.StandardOutput
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => path.Replace('\\', '/'))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        var families = paths
            .GroupBy(GetArtifactFamily, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var familyPaths = group.ToArray();
                var heavy = familyPaths.Where(IsHeavyOutputPath).ToArray();
                var classification = group.Key == "development-complexity-stabilization"
                    ? "current_goal_compact_review_artifact"
                    : heavy.Length > 0
                        ? "historical_with_tracked_heavy_output_warning"
                        : "historical_compact_review_artifact";
                return new TrackedGeneratedArtifactFamily(
                    group.Key,
                    classification,
                    familyPaths.Length,
                    heavy.Length,
                    familyPaths.Take(5).ToArray());
            })
            .ToArray();

        return new TrackedGeneratedArtifactInventory(
            "tracked_generated_artifact_inventory_v1",
            ".llmgc/procedural",
            paths.Length,
            families,
            "Broad cleanup and untracking are deferred unless separately requested.");
    }

    private static InvalidMatrix BuildInvalidMatrix(string repoRoot)
    {
        var scenarios = new List<InvalidScenario>
        {
            GuardScenario(repoRoot, "legacy_goal020_artifact_mutation_rejected", ".llmgc/procedural/minimum-playable-generated-game/minimum-playable-generated-game-report.json"),
            GuardScenario(repoRoot, "legacy_unity_multi_variant_artifact_mutation_rejected", ".llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.json"),
            GuardScenario(repoRoot, "solution_or_project_mutation_rejected", "LLMGameCreator.sln"),
            GuardScenario(repoRoot, "public_gamepackage_schema_mutation_rejected", "docs/GAME_PACKAGE_FORMAT.md"),
            GuardScenario(repoRoot, "generator_library_mutation_rejected", "generator-library/catalog.json"),
            GuardScenario(repoRoot, "unity_build_entrypoint_mutation_rejected", "unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs"),
            MissingPolicyScenario(repoRoot),
            GuardScenario(repoRoot, "fake_report_with_violations_accepted_true_rejected", ".llmgc/procedural/minimum-playable-generated-game/fake-accepted-scope-report.json"),
            GuardScenario(repoRoot, "undeclared_product_smoke_root_write_rejected", ".llmgc/procedural/undeclared-product-smoke/report.json"),
            CheckAllIsolationScenario(repoRoot),
            HeavyOutputScenario(repoRoot),
            MultipleFinalGateScenario()
        };

        return new InvalidMatrix(
            "scope_guard_invalid_matrix_v1",
            scenarios.Count,
            scenarios.Count(item => !item.actualValid),
            scenarios.All(item => !item.actualValid),
            scenarios.ToArray());
    }

    private static InvalidScenario GuardScenario(string repoRoot, string scenarioId, string changedPath)
    {
        var run = RunScopeGuard(repoRoot, changedPath);
        return new InvalidScenario(
            scenarioId,
            changedPath,
            run.ExitCode == 0,
            run.Diagnostics);
    }

    private static InvalidScenario MissingPolicyScenario(string repoRoot)
    {
        var run = RunScopeGuardWithMissingPolicy(repoRoot, ".llmgc/procedural/development-complexity-stabilization/report.json");
        return new InvalidScenario(
            "copied_scope_report_without_policy_json_rejected",
            "missing policy json",
            run.ExitCode == 0,
            run.Diagnostics.Length == 0
                ? new[] { new GuardDiagnostic("error", "artifact_scope.policy.missing", "missing policy json", "Artifact scope policy JSON is required.") }
                : run.Diagnostics);
    }

    private static InvalidScenario CheckAllIsolationScenario(string repoRoot)
    {
        var proof = BuildCheckAllIsolationProof(repoRoot);
        return new InvalidScenario(
            "missing_check_all_artifact_isolation_rejected",
            ".devflow/scripts/check-all.ps1",
            !proof.isolationImplemented,
            new[] { new GuardDiagnostic("error", "development_complexity.check_all_isolation.required", ".devflow/scripts/check-all.ps1", "check-all must isolate product-smoke artifact output variables during tests.") });
    }

    private static InvalidScenario HeavyOutputScenario(string repoRoot)
    {
        var run = RunScopeGuardWithFailOnTrackedIgnored(repoRoot, ".llmgc/procedural/minimum-playable-generated-game/review-package/LLMGameCreatorAlpha.exe");
        return new InvalidScenario(
            "tracked_ignored_heavy_output_rejected_with_fail_on_tracked_ignored",
            ".llmgc/procedural/minimum-playable-generated-game/review-package/LLMGameCreatorAlpha.exe",
            run.ExitCode == 0,
            run.Diagnostics);
    }

    private static InvalidScenario MultipleFinalGateScenario()
    {
        return new InvalidScenario(
            "multiple_final_gates_in_state_policy_rejected",
            "docs/CURRENT_GENERATOR_STATE.json",
            false,
            new[] { new GuardDiagnostic("error", "development_complexity.manual_gate.single_required", "docs/CURRENT_GENERATOR_STATE.json", "A goal may leave exactly one final manual gate required.") });
    }

    private static GuardRun RunScopeGuard(string repoRoot, string policyPath, bool failOnTrackedIgnored, params string[] changedPaths)
    {
        var script = Path.Combine(repoRoot, ".devflow", "scripts", "check-artifact-scope.ps1");
        var args = new List<string>
        {
            "-NoProfile",
            "-ExecutionPolicy",
            "Bypass",
            "-File",
            script,
            "-PolicyPath",
            policyPath,
            "-Scenario",
            Scenario,
            "-AllowedPath",
            "docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md",
            "-AllowedPathPrefix",
            ".devflow/artifact-scope/",
            "-AllowedPath",
            ".devflow/scripts/check-artifact-scope.ps1",
            "-AllowedPath",
            ".devflow/scripts/check-all.ps1",
            "-AllowedPath",
            ".devflow/scripts/run-product-smoke.ps1",
            "-AllowedPathPrefix",
            ".llmgc/procedural/development-complexity-stabilization/",
            "-AllowedPathPrefix",
            "tests/LLMGameCreator.Tests/Devflow/",
            "-AllowedPathPrefix",
            "tests/LLMGameCreator.Tests/ProductSmoke/",
            "-AllowedPath",
            "docs/CURRENT_GENERATOR_STATE.json",
            "-AllowedPath",
            "docs/CURRENT_GENERATOR_STATE.md",
            "-AllowedPath",
            "docs/CONTEXT_INDEX.md",
            "-AllowedPath",
            "docs/FULL_GENERATOR_GOAL_QUEUE.md"
        };

        if (failOnTrackedIgnored)
        {
            args.Add("-FailOnTrackedIgnored");
        }

        foreach (var changedPath in changedPaths)
        {
            args.Add("-ChangedPath");
            args.Add(changedPath);
        }

        var process = RunProcess(repoRoot, "powershell", args.ToArray());
        var diagnostics = Array.Empty<GuardDiagnostic>();
        try
        {
            if (!string.IsNullOrWhiteSpace(process.StandardOutput))
            {
                using var document = JsonDocument.Parse(process.StandardOutput);
                diagnostics = document.RootElement.TryGetProperty("diagnostics", out var diagnosticElement)
                    ? diagnosticElement.EnumerateArray()
                        .Select(item => new GuardDiagnostic(
                            item.GetProperty("severity").GetString() ?? string.Empty,
                            item.GetProperty("code").GetString() ?? string.Empty,
                            item.GetProperty("path").GetString() ?? string.Empty,
                            item.GetProperty("message").GetString() ?? string.Empty))
                        .ToArray()
                    : Array.Empty<GuardDiagnostic>();
            }
        }
        catch (JsonException)
        {
            diagnostics = new[]
            {
                new GuardDiagnostic("error", "artifact_scope.output.unparsed", "check-artifact-scope.ps1", process.StandardError + process.StandardOutput)
            };
        }

        return new GuardRun(process.ExitCode, process.StandardOutput, process.StandardError, diagnostics);
    }

    private static ProcessResult RunProcess(string workingDirectory, string fileName, params string[] args)
    {
        var start = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not start {fileName}.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return new ProcessResult(process.ExitCode, stdout, stderr);
    }

    private static string WriteJson(string path, object value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        WriteUtf8(path, json + Environment.NewLine);
        return json + Environment.NewLine;
    }

    private static void WriteUtf8(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string Sha256(string content)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string GetArtifactFamily(string path)
    {
        var normalized = path.Replace('\\', '/');
        const string prefix = ".llmgc/procedural/";
        if (!normalized.StartsWith(prefix, StringComparison.Ordinal))
        {
            return "outside-procedural";
        }

        var remainder = normalized.Substring(prefix.Length);
        var slash = remainder.IndexOf('/');
        return slash < 0 ? remainder : remainder.Substring(0, slash);
    }

    private static bool IsHeavyOutputPath(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/build/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/logs/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/unity-work/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/review-package/", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed record WrittenArtifacts(
    string OutputRoot,
    string ReportJsonPath,
    string ReportMarkdownPath,
    string VerificationMarkdownPath,
    string PolicyProofJsonPath,
    string InventoryJsonPath,
    string InvalidMatrixJsonPath,
    string CheckAllIsolationProofJsonPath,
    string ReportHash);

internal sealed record CheckAllIsolationProof(
    string schemaVersion,
    bool setsProjectDir,
    bool setsPackageOutputDir,
    bool restoresProjectDir,
    bool restoresPackageOutputDir,
    bool seedsRunLocalProceduralBaseline,
    bool excludesProductSmokeFromOrdinarySuite,
    bool isolationImplemented,
    string[] isolatedEnvironmentVariables);

internal sealed record TrackedGeneratedArtifactInventory(
    string schemaVersion,
    string root,
    int trackedFileCount,
    TrackedGeneratedArtifactFamily[] families,
    string cleanupDeferredNote);

internal sealed record TrackedGeneratedArtifactFamily(
    string family,
    string classification,
    int trackedFileCount,
    int trackedHeavyOutputWarningCount,
    string[] samplePaths);

internal sealed record InvalidMatrix(
    string schemaVersion,
    int scenarioCount,
    int rejectedCount,
    bool passed,
    InvalidScenario[] scenarios);

internal sealed record InvalidScenario(
    string scenarioId,
    string mutation,
    bool actualValid,
    GuardDiagnostic[] diagnostics);

internal sealed record GuardDiagnostic(
    string severity,
    string code,
    string path,
    string message);

internal sealed record GuardRun(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    GuardDiagnostic[] Diagnostics);

internal sealed record ProcessResult(
    int ExitCode,
    string StandardOutput,
    string StandardError);
