using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.Gameplay;

public sealed class RulePackGameplayFamilyAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/rule-pack-gameplay-family-foundations";
    public const string ReportJsonFileName = "rule-pack-gameplay-family-report.json";
    public const string ReportMarkdownFileName = "rule-pack-gameplay-family-report.md";
    public const string VerificationMarkdownFileName = "rule-pack-gameplay-family-verification.md";
    public const string ManualGate = "rule_pack_gameplay_family_artifact_verification";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IRulePackGameplayFamilyRuntimeAdapter _runtimeAdapter;

    static RulePackGameplayFamilyAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public RulePackGameplayFamilyAcceptanceService(IRulePackGameplayFamilyRuntimeAdapter? runtimeAdapter = null)
    {
        _runtimeAdapter = runtimeAdapter ?? new UnavailableRulePackGameplayFamilyRuntimeAdapter();
    }

    public RulePackGameplayFamilyAcceptanceResult Build(string? projectRootPath = null)
    {
        var package = BuildPackage();
        var declarations = BuildDeclarations();
        var validSpecs = BuildValidSpecs();
        var invalidSpecs = BuildInvalidSpecs();

        var validScenarios = validSpecs.Select(spec => BuildValidScenario(package, declarations, spec)).ToList();
        var invalidScenarios = invalidSpecs.Select(spec => BuildInvalidScenario(package, declarations, spec)).ToList();
        var repeated = BuildValidScenario(package, declarations, validSpecs.Single(item => item.ScenarioId == "gameplay_combined_loop"));
        var scenarios = validScenarios.Concat(invalidScenarios).OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList();

        var validAccepted = validScenarios.All(item => item.ExpectedValid && item.ActualValid);
        var invalidRejected = invalidScenarios.All(item =>
            !item.ExpectedValid &&
            !item.ActualValid &&
            item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error"));
        var packageRuleBindingPassed = validScenarios.All(item => item.PackageBindingAudit.Passed);
        var runtimeExecutionPassed = validScenarios.All(item =>
            item.RuntimeEvidence.RuntimeAttempted &&
            item.RuntimeEvidence.RuntimeStartSucceeded &&
            item.RuntimeEvidence.RuntimeBoundary.UsedGameRuntimeService &&
            item.RuntimeEvidence.Commands.All(command => command.Succeeded) &&
            item.RuntimeEvidence.Commands.All(CommandHasStateDelta));
        var saveLoadPassed = validScenarios.All(item =>
            item.RuntimeEvidence.SaveLoadRoundtripPassed &&
            item.RuntimeEvidence.SaveLoadEvidence.UsedRuntimeStateSerializer &&
            item.RuntimeEvidence.SaveLoadEvidence.SerializedFullState &&
            DictionaryEquals(item.RuntimeEvidence.StateEvidence, item.RuntimeEvidence.RestoredStateEvidence));
        var deterministicReplayPassed =
            validScenarios.Single(item => item.ScenarioId == "gameplay_combined_loop").DeterministicHash == repeated.DeterministicHash &&
            validScenarios.Single(item => item.ScenarioId == "gameplay_combined_loop").RuntimeEvidence.RuntimeStateHash == repeated.RuntimeEvidence.RuntimeStateHash &&
            validScenarios.Single(item => item.ScenarioId == "gameplay_combined_loop").RuntimeEvidence.RestoredRuntimeStateHash == repeated.RuntimeEvidence.RestoredRuntimeStateHash;
        var fakeSuccessRejected = invalidScenarios.Any(item =>
            item.ScenarioId == "invalid_fake_runtime_success" &&
            !item.ActualValid &&
            item.Diagnostics.Any(diagnostic => diagnostic.Code == "gameplay_family.evidence.required_command_missing" || diagnostic.Code == "gameplay_family.evidence.state_delta_missing"));

        var diagnostics = new List<RulePackGameplayFamilyDiagnostic>
        {
            Diagnostic("info", "gameplay_family.goal007_gate_recorded", "connected_world_travel_state_artifact_verification", "User-confirmed Goal 007 connected-world artifact verification is recorded as passed."),
            Diagnostic("info", "gameplay_family.no_external_execution", "harness", "No LLM, RAG, provider, Lua, Unity or media execution was invoked."),
            Diagnostic(validAccepted ? "info" : "error", validAccepted ? "gameplay_family.valid_scenarios_accepted" : "gameplay_family.valid_scenarios_failed", "valid_scenarios", "All required valid gameplay-family scenarios must be accepted."),
            Diagnostic(invalidRejected ? "info" : "error", invalidRejected ? "gameplay_family.invalid_scenarios_rejected" : "gameplay_family.invalid_scenarios_not_rejected", "invalid_scenarios", "All required invalid gameplay-family scenarios must fail by real diagnostics or runtime evidence."),
            Diagnostic(packageRuleBindingPassed ? "info" : "error", packageRuleBindingPassed ? "gameplay_family.package_bindings_verified" : "gameplay_family.package_bindings_failed", "package_bindings", "Every selected declaration id must bind to exact package/runtime ids before runtime execution."),
            Diagnostic(runtimeExecutionPassed ? "info" : "error", runtimeExecutionPassed ? "gameplay_family.runtime_commands_executed" : "gameplay_family.runtime_commands_missing", "runtime_commands", "Runtime command evidence must contain successful commands and state deltas."),
            Diagnostic(saveLoadPassed ? "info" : "error", saveLoadPassed ? "gameplay_family.save_load_roundtrip_passed" : "gameplay_family.save_load_roundtrip_failed", "runtime_state", "Save/load must restore exact gameplay-family state evidence."),
            Diagnostic(deterministicReplayPassed ? "info" : "error", deterministicReplayPassed ? "gameplay_family.replay_stable" : "gameplay_family.replay_unstable", "gameplay_combined_loop", "Repeated command execution must produce stable scenario and runtime hashes."),
            Diagnostic(fakeSuccessRejected ? "info" : "error", fakeSuccessRejected ? "gameplay_family.fake_success_rejected" : "gameplay_family.fake_success_not_rejected", "invalid_fake_runtime_success", "Copied ids plus a success boolean must not satisfy acceptance.")
        };
        diagnostics.AddRange(scenarios.SelectMany(item => item.Diagnostics));

        var reportWithoutHash = new RulePackGameplayFamilyReport
        {
            Accepted = validAccepted &&
                       invalidRejected &&
                       packageRuleBindingPassed &&
                       runtimeExecutionPassed &&
                       saveLoadPassed &&
                       deterministicReplayPassed &&
                       fakeSuccessRejected,
            ManualGate = ManualGate,
            Goal007GateRecorded = true,
            CompletedSlices = ["S071", "S072", "S073", "S074", "S075", "S076", "S077", "S077A"],
            ScenarioCount = scenarios.Count,
            ValidScenarioCount = validScenarios.Count,
            InvalidScenarioCount = invalidScenarios.Count,
            ValidScenariosAccepted = validAccepted,
            InvalidScenariosRejected = invalidRejected,
            PackageRuleBindingAuditPassed = packageRuleBindingPassed,
            GameplayRuntimeExecutionPassed = runtimeExecutionPassed,
            SaveLoadRoundtripPassed = saveLoadPassed,
            DeterministicReplayPassed = deterministicReplayPassed,
            FakeRuntimeSuccessRejected = fakeSuccessRejected,
            PublicGamePackageSchemaChanged = false,
            ExternalExecution = new RulePackGameplayFamilyExternalExecutionFlags(),
            Declarations = declarations,
            Scenarios = scenarios,
            Diagnostics = SortDiagnostics(diagnostics),
            RemainingPrimitiveLimits =
            [
                "combat, factions, reputation, social, work and theft families remain for later goals",
                "prices, markets and vendor restocking remain deterministic declaration/runtime primitives only",
                "Unity presentation and Runtime Preview UI are not implemented in this goal",
                "Lua/provider/media execution remains locked"
            ]
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new RulePackGameplayFamilyAcceptanceResult
        {
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<RulePackGameplayFamilyWriteResult> WriteAsync(
        string projectRootPath,
        RulePackGameplayFamilyAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "rule-pack-gameplay-family-foundations"));
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

        return new RulePackGameplayFamilyWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<RulePackGameplayFamilyWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private RulePackGameplayFamilyScenario BuildValidScenario(
        GamePackageDefinition package,
        RulePackGameplayFamilyDeclarations declarations,
        GameplayFamilyScenarioSpec spec)
    {
        var bindingAudit = AuditBindings(package, declarations, spec);
        var diagnostics = new List<RulePackGameplayFamilyDiagnostic>(bindingAudit.Diagnostics);
        var runtimeEvidence = bindingAudit.Passed
            ? _runtimeAdapter.Run(new RulePackGameplayFamilyRuntimeRequest
            {
                ScenarioId = spec.ScenarioId,
                Seed = spec.Seed,
                Package = package,
                Declarations = declarations,
                Commands = spec.Commands,
                InitialInventoryAmounts = spec.InitialInventoryAmounts,
                CompletionFlagId = spec.CompletionFlagId
            })
            : new RulePackGameplayFamilyRuntimeEvidence();

        diagnostics.AddRange(runtimeEvidence.Diagnostics);
        var evidenceDiagnostics = ValidateRuntimeEvidence(spec, runtimeEvidence);
        diagnostics.AddRange(evidenceDiagnostics);
        var actualValid = IsScenarioAccepted(bindingAudit, runtimeEvidence, evidenceDiagnostics);

        var scenarioWithoutHash = new RulePackGameplayFamilyScenario
        {
            ScenarioId = spec.ScenarioId,
            Seed = spec.Seed,
            ExpectedValid = true,
            ActualValid = actualValid,
            SelectedGameplayFamilyIds = spec.FamilyIds,
            SourceDeclarationIds = spec.SourceDeclarationIds,
            PackageRuntimeIds = spec.PackageRuntimeIds,
            PackageBindingAudit = bindingAudit,
            RuntimeEvidence = runtimeEvidence,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        return scenarioWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(scenarioWithoutHash, JsonOptions))
        };
    }

    private RulePackGameplayFamilyScenario BuildInvalidScenario(
        GamePackageDefinition package,
        RulePackGameplayFamilyDeclarations declarations,
        GameplayFamilyScenarioSpec spec)
    {
        var invalidPackage = ClonePackage(package);
        var invalidDeclarations = CloneDeclarations(declarations);
        ApplyInvalidMutation(invalidPackage, invalidDeclarations, spec.InvalidKind);
        var bindingAudit = AuditBindings(invalidPackage, invalidDeclarations, spec);
        var diagnostics = new List<RulePackGameplayFamilyDiagnostic>(bindingAudit.Diagnostics);
        RulePackGameplayFamilyRuntimeEvidence runtimeEvidence;

        if (spec.InvalidKind == "fake_runtime_success")
        {
            runtimeEvidence = FakeRuntimeSuccess(spec);
            diagnostics.AddRange(runtimeEvidence.Diagnostics);
        }
        else if (!bindingAudit.Passed && !ShouldRunRuntimeForInvalidScenario(spec.InvalidKind))
        {
            runtimeEvidence = new RulePackGameplayFamilyRuntimeEvidence();
        }
        else
        {
            runtimeEvidence = _runtimeAdapter.Run(new RulePackGameplayFamilyRuntimeRequest
            {
                ScenarioId = spec.ScenarioId,
                Seed = spec.Seed,
                Package = invalidPackage,
                Declarations = invalidDeclarations,
                Commands = spec.Commands,
                InitialInventoryAmounts = spec.InitialInventoryAmounts,
                CompletionFlagId = spec.CompletionFlagId
            });
            diagnostics.AddRange(runtimeEvidence.Diagnostics);
            if (runtimeEvidence.Commands.Any(command => !command.Succeeded))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.runtime_command_failed", spec.ScenarioId, "Invalid runtime scenario failed during command execution."));
            }
        }

        var evidenceDiagnostics = ValidateRuntimeEvidence(spec, runtimeEvidence);
        diagnostics.AddRange(evidenceDiagnostics);
        var actualValid = IsScenarioAccepted(bindingAudit, runtimeEvidence, evidenceDiagnostics);

        var scenarioWithoutHash = new RulePackGameplayFamilyScenario
        {
            ScenarioId = spec.ScenarioId,
            Seed = spec.Seed,
            ExpectedValid = false,
            ActualValid = actualValid,
            InvalidKind = spec.InvalidKind,
            SelectedGameplayFamilyIds = spec.FamilyIds,
            SourceDeclarationIds = spec.SourceDeclarationIds,
            PackageRuntimeIds = spec.PackageRuntimeIds,
            PackageBindingAudit = bindingAudit,
            RuntimeEvidence = runtimeEvidence,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        return scenarioWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(scenarioWithoutHash, JsonOptions))
        };
    }

    private static bool ShouldRunRuntimeForInvalidScenario(string invalidKind) =>
        invalidKind is "equipment_slot_mismatch"
            or "crafting_missing_inputs"
            or "trade_insufficient_cost"
            or "status_duration_mismatch"
            or "save_load_mismatch";

    private static RulePackGameplayFamilyBindingAudit AuditBindings(
        GamePackageDefinition package,
        RulePackGameplayFamilyDeclarations declarations,
        GameplayFamilyScenarioSpec spec)
    {
        var diagnostics = new List<RulePackGameplayFamilyDiagnostic>();
        var packageItemIds = package.Game.Items.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var recipeIds = package.Game.Recipes.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var transactionIds = package.Game.Transactions.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var slotIds = package.Game.EquipmentSlots.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var statusIds = package.Game.Statuses.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);

        var allDeclarations = declarations.Items.Select(item => new DeclarationIndex(item.DeclarationId, item.FamilyId, "item", item.PackageItemId, "", "", "", "", item))
            .Concat(declarations.Equipment.Select(item => new DeclarationIndex(item.DeclarationId, item.FamilyId, "equipment", item.PackageItemId, item.PackageSlotId, "", "", "", item)))
            .Concat(declarations.Recipes.Select(item => new DeclarationIndex(item.DeclarationId, item.FamilyId, "recipe", "", "", item.PackageRecipeId, "", "", item)))
            .Concat(declarations.Transactions.Select(item => new DeclarationIndex(item.DeclarationId, item.FamilyId, "transaction", "", "", "", item.PackageTransactionId, "", item)))
            .Concat(declarations.Statuses.Select(item => new DeclarationIndex(item.DeclarationId, item.FamilyId, "status", "", "", "", "", item.PackageStatusId, item)))
            .ToList();
        var selectedDeclarations = new List<DeclarationIndex>();

        foreach (var sourceId in spec.SourceDeclarationIds)
        {
            var matches = allDeclarations.Where(item => item.DeclarationId == sourceId).ToList();
            if (matches.Count != 1)
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.unknown_source_declaration", sourceId, "Scenario selected declaration id must exist exactly once."));
                continue;
            }

            var selected = matches[0];
            selectedDeclarations.Add(selected);
            if (!spec.FamilyIds.Contains(selected.FamilyId, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.family_mismatch", sourceId, "Selected declaration family is not part of the scenario family set."));
            }
        }

        var selectedItems = selectedDeclarations.Where(item => item.Kind == "item").Select(item => (GameplayItemDeclaration)item.Source).ToList();
        var selectedEquipment = selectedDeclarations.Where(item => item.Kind == "equipment").Select(item => (GameplayEquipmentDeclaration)item.Source).ToList();
        var selectedRecipes = selectedDeclarations.Where(item => item.Kind == "recipe").Select(item => (GameplayRecipeDeclaration)item.Source).ToList();
        var selectedTransactions = selectedDeclarations.Where(item => item.Kind == "transaction").Select(item => (GameplayTransactionDeclaration)item.Source).ToList();
        var selectedStatuses = selectedDeclarations.Where(item => item.Kind == "status").Select(item => (GameplayStatusDeclaration)item.Source).ToList();

        foreach (var item in selectedItems)
        {
            var packageItem = package.Game.Items.FirstOrDefault(candidate => candidate.Id == item.PackageItemId);
            if (packageItem == null)
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.missing_item_ref", item.DeclarationId, "Gameplay item declaration references a missing package item."));
                continue;
            }

            foreach (var effect in packageItem.UseEffects)
            {
                var effectId = EffectId(effect);
                if (EffectKind(effect, "status"))
                {
                    var status = selectedStatuses.FirstOrDefault(candidate => candidate.PackageStatusId == effectId);
                    if (status == null || !statusIds.Contains(effectId))
                    {
                        diagnostics.Add(Diagnostic("error", "gameplay_family.audit.invalid_status_effect_binding", item.DeclarationId, "Item effect must reference a selected package status declaration."));
                    }
                    else if (ParseInt(effect.Args.GetValueOrDefault("amount")) != status.DurationTicks)
                    {
                        diagnostics.Add(Diagnostic("error", "gameplay_family.audit.status_duration_mismatch", item.DeclarationId, "Item effect duration must match the selected status declaration."));
                    }
                }
                else if (EffectKind(effect, "flag"))
                {
                    if (!spec.PackageRuntimeIds.Contains(effectId, StringComparer.Ordinal))
                    {
                        diagnostics.Add(Diagnostic("error", "gameplay_family.audit.command_target_not_declared", item.DeclarationId, "Item flag effect must be listed in the scenario package/runtime ids."));
                    }
                }
            }
        }

        foreach (var recipe in selectedRecipes)
        {
            var packageRecipe = package.Game.Recipes.FirstOrDefault(candidate => candidate.Id == recipe.PackageRecipeId);
            if (packageRecipe == null)
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.missing_recipe_ref", recipe.DeclarationId, "Gameplay recipe declaration references a missing package recipe."));
                continue;
            }

            var packageInputIds = packageRecipe.Inputs.Select(item => item.Id).OrderBy(item => item, StringComparer.Ordinal).ToList();
            var packageOutputIds = packageRecipe.Outputs.Where(item => item.Kind is "item" or "add_item").Select(item => item.Id).OrderBy(item => item, StringComparer.Ordinal).ToList();
            if (!recipe.InputItemIds.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(packageInputIds, StringComparer.Ordinal) ||
                !recipe.OutputItemIds.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(packageOutputIds, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.recipe_io_mismatch", recipe.DeclarationId, "Package recipe inputs/outputs must equal the selected recipe declaration."));
            }

            foreach (var itemId in recipe.InputItemIds.Concat(recipe.OutputItemIds))
            {
                if (!packageItemIds.Contains(itemId))
                {
                    diagnostics.Add(Diagnostic("error", "gameplay_family.audit.missing_item_ref", recipe.DeclarationId, "Gameplay recipe declaration references a missing package item."));
                }
            }
        }

        foreach (var transaction in selectedTransactions)
        {
            var packageTransaction = package.Game.Transactions.FirstOrDefault(candidate => candidate.Id == transaction.PackageTransactionId);
            if (packageTransaction == null)
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.missing_transaction_ref", transaction.DeclarationId, "Gameplay transaction declaration references a missing package transaction."));
                continue;
            }

            var packageCostIds = packageTransaction.Costs.Select(item => item.Id).OrderBy(item => item, StringComparer.Ordinal).ToList();
            var packageOutputIds = packageTransaction.Outputs.Where(item => item.Kind is "item" or "add_item").Select(item => item.Id).OrderBy(item => item, StringComparer.Ordinal).ToList();
            if (!transaction.CostItemIds.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(packageCostIds, StringComparer.Ordinal) ||
                !transaction.OutputItemIds.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(packageOutputIds, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.transaction_io_mismatch", transaction.DeclarationId, "Package transaction costs/outputs must equal the selected transaction declaration."));
            }

            foreach (var itemId in transaction.CostItemIds.Concat(transaction.OutputItemIds))
            {
                if (!packageItemIds.Contains(itemId))
                {
                    diagnostics.Add(Diagnostic("error", "gameplay_family.audit.missing_item_ref", transaction.DeclarationId, "Gameplay transaction declaration references a missing package item."));
                }
            }
        }

        foreach (var equipment in selectedEquipment)
        {
            var item = package.Game.Items.FirstOrDefault(candidate => candidate.Id == equipment.PackageItemId);
            var slot = package.Game.EquipmentSlots.FirstOrDefault(candidate => candidate.Id == equipment.PackageSlotId);
            if (item == null)
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.missing_item_ref", equipment.DeclarationId, "Gameplay equipment declaration references a missing package item."));
            }

            if (slot == null)
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.missing_equipment_slot_ref", equipment.DeclarationId, "Gameplay equipment declaration references a missing package slot."));
            }

            if (item != null && slot != null && !ItemMatchesSlot(item, slot))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.equipment_slot_mismatch", equipment.DeclarationId, "Equipment declaration item and slot must match package compatibility rules."));
            }
        }

        foreach (var status in selectedStatuses)
        {
            if (!statusIds.Contains(status.PackageStatusId))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.invalid_status_effect_binding", status.DeclarationId, "Gameplay status declaration is not bound to a package status."));
            }
        }

        foreach (var command in spec.Commands)
        {
            if (!CommandCoveredBySelectedDeclarations(command, selectedItems, selectedEquipment, selectedRecipes, selectedTransactions, selectedStatuses, spec))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.command_target_not_declared", command.CommandId, "Runtime command target must be covered by the scenario's selected declarations and exact package ids."));
            }
        }

        foreach (var id in spec.PackageRuntimeIds)
        {
            if (id.StartsWith("item/", StringComparison.Ordinal) && !packageItemIds.Contains(id))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.missing_item_ref", id, "Scenario references a missing package item."));
            }
            else if (id.StartsWith("recipe/", StringComparison.Ordinal) && !recipeIds.Contains(id))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.missing_recipe_ref", id, "Scenario references a missing package recipe."));
            }
            else if (id.StartsWith("transaction/", StringComparison.Ordinal) && !transactionIds.Contains(id))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.missing_transaction_ref", id, "Scenario references a missing package transaction."));
            }
            else if (id.StartsWith("slot/", StringComparison.Ordinal) && !slotIds.Contains(id))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.missing_equipment_slot_ref", id, "Scenario references a missing package equipment slot."));
            }
            else if (id.StartsWith("status/", StringComparison.Ordinal) && !statusIds.Contains(id))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.audit.invalid_status_effect_binding", id, "Scenario references a missing package status."));
            }
        }

        return new RulePackGameplayFamilyBindingAudit
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            PackageId = package.Manifest.PackageId,
            AuditedDeclarationIds = selectedDeclarations.Select(item => item.DeclarationId)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            AuditedPackageRuntimeIds = spec.PackageRuntimeIds.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static bool CommandCoveredBySelectedDeclarations(
        GameplayCommandSpec command,
        IReadOnlyList<GameplayItemDeclaration> selectedItems,
        IReadOnlyList<GameplayEquipmentDeclaration> selectedEquipment,
        IReadOnlyList<GameplayRecipeDeclaration> selectedRecipes,
        IReadOnlyList<GameplayTransactionDeclaration> selectedTransactions,
        IReadOnlyList<GameplayStatusDeclaration> selectedStatuses,
        GameplayFamilyScenarioSpec spec)
    {
        return command.CommandType switch
        {
            "gameplay/use_item" => selectedItems.Any(item => item.PackageItemId == command.TargetId) &&
                                   selectedStatuses.Count > 0,
            "gameplay/equip_item" => selectedEquipment.Any(item =>
                item.PackageItemId == command.TargetId &&
                item.PackageSlotId == command.SecondaryTargetId),
            "gameplay/craft_recipe" => selectedRecipes.Any(item => item.PackageRecipeId == command.TargetId),
            "gameplay/execute_transaction" => selectedTransactions.Any(item => item.PackageTransactionId == command.TargetId),
            "gameplay/set_flag" => !string.IsNullOrWhiteSpace(spec.CompletionFlagId) &&
                                   command.TargetId == spec.CompletionFlagId &&
                                   spec.PackageRuntimeIds.Contains(command.TargetId, StringComparer.Ordinal),
            _ => false
        };
    }

    private static bool ItemMatchesSlot(ItemDefinition item, EquipmentSlotDefinition slot)
    {
        var kindAllowed = slot.AllowedKinds.Count == 0 ||
                          slot.AllowedKinds.Any(kind => string.Equals(kind, item.Kind, StringComparison.OrdinalIgnoreCase));
        var tagAllowed = slot.AllowedTags.Count == 0 ||
                         slot.AllowedTags.Any(tag => item.Tags.Any(itemTag => string.Equals(itemTag, tag, StringComparison.OrdinalIgnoreCase)));
        return kindAllowed && tagAllowed;
    }

    private static bool EffectKind(EffectDefinition effect, string kind) =>
        string.Equals(effect.Type, kind, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(effect.Type, "add_" + kind, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(effect.Type, "set_" + kind, StringComparison.OrdinalIgnoreCase);

    private static string EffectId(EffectDefinition effect) =>
        effect.Args.GetValueOrDefault("id") ??
        effect.Args.GetValueOrDefault("itemId") ??
        effect.Args.GetValueOrDefault("resourceId") ??
        effect.Args.GetValueOrDefault("flagId") ??
        effect.Args.GetValueOrDefault("statusId") ??
        string.Empty;

    private static int? ParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;

    private static IReadOnlyList<RulePackGameplayFamilyDiagnostic> ValidateRuntimeEvidence(
        GameplayFamilyScenarioSpec spec,
        RulePackGameplayFamilyRuntimeEvidence evidence)
    {
        var diagnostics = new List<RulePackGameplayFamilyDiagnostic>();
        if (!evidence.RuntimeAttempted)
        {
            diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.runtime_not_attempted", spec.ScenarioId, "Runtime evidence was not attempted."));
            return diagnostics;
        }

        if (!evidence.RuntimeStartSucceeded)
        {
            diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.runtime_start_failed", spec.ScenarioId, "Runtime evidence did not start successfully."));
        }

        if (!evidence.RuntimeBoundary.UsedGameRuntimeService ||
            string.IsNullOrWhiteSpace(evidence.RuntimeBoundary.RuntimeServiceType) ||
            string.IsNullOrWhiteSpace(evidence.RuntimeBoundary.StateFactoryType))
        {
            diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.real_runtime_boundary_missing", spec.ScenarioId, "Runtime evidence did not prove execution through GameRuntimeService and the runtime state factory."));
        }

        foreach (var expected in spec.Commands)
        {
            var command = evidence.Commands.FirstOrDefault(item => item.CommandId == expected.CommandId);
            if (command == null)
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.required_command_missing", expected.CommandId, "Required runtime command evidence is missing."));
                continue;
            }

            if (!command.Succeeded)
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.required_command_failed", expected.CommandId, "Required runtime command did not succeed."));
            }

            if (!CommandHasStateDelta(command))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.state_delta_missing", expected.CommandId, "Runtime command did not prove an attributable state delta."));
            }

            if (command.RuntimeEventTypes.Count == 0 && command.RuntimeDiagnosticCodes.Count == 0)
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.runtime_events_missing", expected.CommandId, "Runtime command did not preserve real runtime events or diagnostics."));
            }

            if (!CategoryMatchesCommand(expected.CommandType, command))
            {
                diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.category_mismatch", expected.CommandId, "Runtime evidence populated unrelated command-family delta categories."));
            }
        }

        if (!evidence.SaveLoadEvidence.UsedRuntimeStateSerializer ||
            !evidence.SaveLoadEvidence.SerializedFullState ||
            string.IsNullOrWhiteSpace(evidence.SaveLoadEvidence.SerializedStateHash))
        {
            diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.serializer_not_used", spec.ScenarioId, "Save/load evidence did not use the runtime state serializer on the full GameRuntimeState."));
        }

        if (!DictionaryEquals(evidence.StateEvidence, evidence.RestoredStateEvidence) || !evidence.SaveLoadRoundtripPassed)
        {
            diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.save_load_mismatch", spec.ScenarioId, "Save/load did not restore exact gameplay-family state evidence."));
        }

        if (string.IsNullOrWhiteSpace(evidence.RuntimeStateHash) || evidence.RuntimeStateHash != evidence.RestoredRuntimeStateHash)
        {
            diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.restored_hash_mismatch", spec.ScenarioId, "Runtime and restored state hashes differ."));
        }

        if (!string.IsNullOrWhiteSpace(spec.CompletionFlagId) &&
            evidence.CompletionRewardEvidence.CompletionFlagAfter != "completed")
        {
            diagnostics.Add(Diagnostic("error", "gameplay_family.evidence.completion_missing", spec.CompletionFlagId, "Completion flag was not produced by runtime state."));
        }

        return diagnostics;
    }

    private static bool IsScenarioAccepted(
        RulePackGameplayFamilyBindingAudit bindingAudit,
        RulePackGameplayFamilyRuntimeEvidence runtimeEvidence,
        IReadOnlyList<RulePackGameplayFamilyDiagnostic> evidenceDiagnostics) =>
        bindingAudit.Passed &&
        runtimeEvidence.RuntimeAttempted &&
        runtimeEvidence.RuntimeStartSucceeded &&
        runtimeEvidence.RuntimeBoundary.UsedGameRuntimeService &&
        runtimeEvidence.RuntimeBoundary.UsedRuntimeStateFactory &&
        runtimeEvidence.Commands.All(item => item.Succeeded) &&
        runtimeEvidence.Commands.All(CommandHasStateDelta) &&
        runtimeEvidence.SaveLoadRoundtripPassed &&
        runtimeEvidence.SaveLoadEvidence.UsedRuntimeStateSerializer &&
        runtimeEvidence.SaveLoadEvidence.SerializedFullState &&
        evidenceDiagnostics.All(item => item.Severity != "error");

    private static bool CommandHasStateDelta(RulePackGameplayFamilyRuntimeCommandEvidence command) =>
        command.InventoryDelta.Changed ||
        command.EquipmentDelta.Changed ||
        command.CraftingDelta.Changed ||
        command.TradeDelta.Changed ||
        command.StatusDelta.Changed ||
        command.CompletionDelta.Changed;

    private static bool CategoryMatchesCommand(string expectedCommandType, RulePackGameplayFamilyRuntimeCommandEvidence command)
    {
        var craftAllowed = expectedCommandType == "gameplay/craft_recipe";
        var tradeAllowed = expectedCommandType == "gameplay/execute_transaction";
        var equipmentAllowed = expectedCommandType is "gameplay/equip_item" or "gameplay/unequip_item";
        var statusAllowed = expectedCommandType == "gameplay/use_item";
        var completionAllowed = expectedCommandType is "gameplay/use_item" or "gameplay/set_flag";

        return (!command.CraftingDelta.Changed || craftAllowed) &&
               (!command.CraftingDelta.Inputs.Any() || craftAllowed) &&
               (!command.CraftingDelta.Outputs.Any() || craftAllowed) &&
               (!command.TradeDelta.Changed || tradeAllowed) &&
               (!command.TradeDelta.Costs.Any() || tradeAllowed) &&
               (!command.TradeDelta.Outputs.Any() || tradeAllowed) &&
               (!command.EquipmentDelta.Changed || equipmentAllowed) &&
               (!command.StatusDelta.Changed || statusAllowed) &&
               (!command.CompletionDelta.Changed || completionAllowed);
    }

    private static IReadOnlyList<GameplayFamilyScenarioSpec> BuildValidSpecs() =>
    [
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "gameplay_inventory_item_use",
            Seed = "goal008-inventory-item-use-seed",
            FamilyIds = ["family/items", "family/inventory", "family/item_use", "family/status_effects"],
            SourceDeclarationIds = ["decl/item/field_ration", "decl/status/focused"],
            PackageRuntimeIds = ["item/field_ration", "status/focused", "flag/item_used"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/field_ration"] = 2 },
            CompletionFlagId = "flag/item_used",
            Commands =
            [
                GameplayCommandSpec.UseItem("01_use_field_ration", "item/field_ration", "inventory/player")
            ]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "gameplay_equipment_loadout",
            Seed = "goal008-equipment-loadout-seed",
            FamilyIds = ["family/equipment", "family/loadout"],
            SourceDeclarationIds = ["decl/equipment/scavenger_tool"],
            PackageRuntimeIds = ["item/scavenger_tool", "slot/tool"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/scavenger_tool"] = 1 },
            Commands =
            [
                GameplayCommandSpec.EquipItem("01_equip_scavenger_tool", "item/scavenger_tool", "slot/tool", "inventory/player")
            ]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "gameplay_crafting_recipe",
            Seed = "goal008-crafting-recipe-seed",
            FamilyIds = ["family/crafting", "family/resource_conversion"],
            SourceDeclarationIds = ["decl/recipe/repair_wrap"],
            PackageRuntimeIds = ["recipe/repair_wrap", "item/scrap", "item/thread", "item/repair_wrap"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal)
            {
                ["item/scrap"] = 2,
                ["item/thread"] = 1
            },
            Commands =
            [
                GameplayCommandSpec.CraftRecipe("01_craft_repair_wrap", "recipe/repair_wrap", "inventory/player")
            ]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "gameplay_trading_transaction",
            Seed = "goal008-trading-transaction-seed",
            FamilyIds = ["family/trading", "family/transaction"],
            SourceDeclarationIds = ["decl/transaction/buy_signal_charm"],
            PackageRuntimeIds = ["transaction/buy_signal_charm", "item/trade_token", "item/signal_charm"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/trade_token"] = 3 },
            Commands =
            [
                GameplayCommandSpec.ExecuteTransaction("01_buy_signal_charm", "transaction/buy_signal_charm", "inventory/player")
            ]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "gameplay_status_effect_chain",
            Seed = "goal008-status-effect-chain-seed",
            FamilyIds = ["family/status_effects", "family/item_use"],
            SourceDeclarationIds = ["decl/item/focus_tonic", "decl/status/focused"],
            PackageRuntimeIds = ["item/focus_tonic", "status/focused", "flag/status_chain"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/focus_tonic"] = 1 },
            CompletionFlagId = "flag/status_chain",
            Commands =
            [
                GameplayCommandSpec.UseItem("01_use_focus_tonic", "item/focus_tonic", "inventory/player")
            ]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "gameplay_combined_loop",
            Seed = "goal008-combined-loop-seed",
            FamilyIds = ["family/items", "family/inventory", "family/equipment", "family/crafting", "family/trading", "family/status_effects"],
            SourceDeclarationIds = ["decl/equipment/scavenger_tool", "decl/recipe/repair_wrap", "decl/transaction/buy_signal_charm", "decl/item/focus_tonic", "decl/status/focused"],
            PackageRuntimeIds = ["item/scavenger_tool", "slot/tool", "recipe/repair_wrap", "item/scrap", "item/thread", "item/repair_wrap", "transaction/buy_signal_charm", "item/trade_token", "item/signal_charm", "item/focus_tonic", "status/focused", "flag/status_chain", "flag/goal008_complete"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal)
            {
                ["item/scavenger_tool"] = 1,
                ["item/scrap"] = 2,
                ["item/thread"] = 1,
                ["item/trade_token"] = 3,
                ["item/focus_tonic"] = 1
            },
            CompletionFlagId = "flag/goal008_complete",
            Commands =
            [
                GameplayCommandSpec.EquipItem("01_equip_tool", "item/scavenger_tool", "slot/tool", "inventory/player"),
                GameplayCommandSpec.CraftRecipe("02_craft_repair_wrap", "recipe/repair_wrap", "inventory/player"),
                GameplayCommandSpec.ExecuteTransaction("03_buy_signal_charm", "transaction/buy_signal_charm", "inventory/player"),
                GameplayCommandSpec.UseItem("04_use_focus_tonic", "item/focus_tonic", "inventory/player"),
                GameplayCommandSpec.SetFlag("05_complete_loop", "flag/goal008_complete", "completed")
            ]
        }
    ];

    private static IReadOnlyList<GameplayFamilyScenarioSpec> BuildInvalidSpecs() =>
    [
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "invalid_missing_item_or_recipe_ref",
            Seed = "goal008-invalid-missing-ref",
            InvalidKind = "missing_item_or_recipe_ref",
            FamilyIds = ["family/crafting"],
            SourceDeclarationIds = ["decl/recipe/repair_wrap"],
            PackageRuntimeIds = ["recipe/repair_wrap", "item/missing_scrap"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/scrap"] = 2, ["item/thread"] = 1 },
            Commands = [GameplayCommandSpec.CraftRecipe("01_craft_missing_ref", "recipe/repair_wrap", "inventory/player")]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "invalid_equipment_slot_mismatch",
            Seed = "goal008-invalid-slot-mismatch",
            InvalidKind = "equipment_slot_mismatch",
            FamilyIds = ["family/equipment"],
            SourceDeclarationIds = ["decl/equipment/scavenger_tool"],
            PackageRuntimeIds = ["item/scavenger_tool", "slot/charm"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/scavenger_tool"] = 1 },
            Commands = [GameplayCommandSpec.EquipItem("01_equip_wrong_slot", "item/scavenger_tool", "slot/charm", "inventory/player")]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "invalid_crafting_missing_inputs",
            Seed = "goal008-invalid-crafting-inputs",
            InvalidKind = "crafting_missing_inputs",
            FamilyIds = ["family/crafting"],
            SourceDeclarationIds = ["decl/recipe/repair_wrap"],
            PackageRuntimeIds = ["recipe/repair_wrap", "item/scrap", "item/thread"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/scrap"] = 1 },
            Commands = [GameplayCommandSpec.CraftRecipe("01_craft_without_inputs", "recipe/repair_wrap", "inventory/player")]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "invalid_trade_insufficient_cost",
            Seed = "goal008-invalid-trade-cost",
            InvalidKind = "trade_insufficient_cost",
            FamilyIds = ["family/trading"],
            SourceDeclarationIds = ["decl/transaction/buy_signal_charm"],
            PackageRuntimeIds = ["transaction/buy_signal_charm", "item/trade_token", "item/signal_charm"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/trade_token"] = 1 },
            Commands = [GameplayCommandSpec.ExecuteTransaction("01_buy_without_cost", "transaction/buy_signal_charm", "inventory/player")]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "invalid_status_or_effect_binding",
            Seed = "goal008-invalid-status-binding",
            InvalidKind = "status_or_effect_binding",
            FamilyIds = ["family/status_effects"],
            SourceDeclarationIds = ["decl/status/focused"],
            PackageRuntimeIds = ["item/focus_tonic", "status/missing_focus"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/focus_tonic"] = 1 },
            Commands = [GameplayCommandSpec.UseItem("01_use_invalid_status", "item/focus_tonic", "inventory/player")]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "invalid_fake_runtime_success",
            Seed = "goal008-invalid-fake-success",
            InvalidKind = "fake_runtime_success",
            FamilyIds = ["family/items", "family/equipment", "family/crafting", "family/trading", "family/status_effects"],
            SourceDeclarationIds = ["decl/item/focus_tonic", "decl/status/focused"],
            PackageRuntimeIds = ["item/focus_tonic", "status/focused"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/focus_tonic"] = 1 },
            Commands = [GameplayCommandSpec.UseItem("01_use_fake_tonic", "item/focus_tonic", "inventory/player")]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "invalid_unknown_source_declaration",
            Seed = "goal008-invalid-unknown-source",
            InvalidKind = "unknown_source_declaration",
            FamilyIds = ["family/item_use", "family/status_effects"],
            SourceDeclarationIds = ["decl/item/focus_tonic", "decl/status/focused", "decl/missing/unknown"],
            PackageRuntimeIds = ["item/focus_tonic", "status/focused", "flag/status_chain"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/focus_tonic"] = 1 },
            Commands = [GameplayCommandSpec.UseItem("01_use_unknown_source_tonic", "item/focus_tonic", "inventory/player")]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "invalid_command_target_not_declared",
            Seed = "goal008-invalid-command-target",
            InvalidKind = "command_target_not_declared",
            FamilyIds = ["family/item_use", "family/status_effects"],
            SourceDeclarationIds = ["decl/item/focus_tonic", "decl/status/focused"],
            PackageRuntimeIds = ["item/field_ration", "item/focus_tonic", "status/focused", "flag/item_used"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/field_ration"] = 1 },
            Commands = [GameplayCommandSpec.UseItem("01_use_unselected_ration", "item/field_ration", "inventory/player")]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "invalid_status_duration_mismatch",
            Seed = "goal008-invalid-status-duration",
            InvalidKind = "status_duration_mismatch",
            FamilyIds = ["family/status_effects", "family/item_use"],
            SourceDeclarationIds = ["decl/item/focus_tonic", "decl/status/focused"],
            PackageRuntimeIds = ["item/focus_tonic", "status/focused", "flag/status_chain"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/focus_tonic"] = 1 },
            Commands = [GameplayCommandSpec.UseItem("01_use_duration_mismatch_tonic", "item/focus_tonic", "inventory/player")]
        },
        new GameplayFamilyScenarioSpec
        {
            ScenarioId = "invalid_save_load_mismatch",
            Seed = "goal008-invalid-save-load",
            InvalidKind = "save_load_mismatch",
            FamilyIds = ["family/status_effects", "family/item_use"],
            SourceDeclarationIds = ["decl/item/focus_tonic", "decl/status/focused"],
            PackageRuntimeIds = ["item/focus_tonic", "status/focused", "flag/status_chain"],
            InitialInventoryAmounts = new SortedDictionary<string, double>(StringComparer.Ordinal) { ["item/focus_tonic"] = 1 },
            Commands = [GameplayCommandSpec.UseItem("01_use_save_load_mismatch_tonic", "item/focus_tonic", "inventory/player")]
        }
    ];

    private static void ApplyInvalidMutation(
        GamePackageDefinition package,
        RulePackGameplayFamilyDeclarations declarations,
        string invalidKind)
    {
        switch (invalidKind)
        {
            case "missing_item_or_recipe_ref":
                declarations.Recipes = declarations.Recipes.Select(item => item.DeclarationId == "decl/recipe/repair_wrap"
                    ? item with { InputItemIds = ["item/missing_scrap", "item/thread"] }
                    : item).ToList();
                package.Game.Recipes.Single(item => item.Id == "recipe/repair_wrap").Inputs[0].Id = "item/missing_scrap";
                break;
            case "status_or_effect_binding":
                declarations.Statuses = declarations.Statuses.Select(item => item.DeclarationId == "decl/status/focused"
                    ? item with { PackageStatusId = "status/missing_focus" }
                    : item).ToList();
                package.Game.Items.Single(item => item.Id == "item/focus_tonic").UseEffects[0].Args["statusId"] = "status/missing_focus";
                break;
            case "status_duration_mismatch":
                package.Game.Items.Single(item => item.Id == "item/focus_tonic").UseEffects[0].Args["amount"] = "2";
                break;
        }
    }

    private static RulePackGameplayFamilyRuntimeEvidence FakeRuntimeSuccess(GameplayFamilyScenarioSpec spec)
    {
        var evidence = new RulePackGameplayFamilyRuntimeEvidence
        {
            RuntimeAttempted = true,
            RuntimeStartSucceeded = true,
            RuntimeStateOwner = "GameRuntimeState",
            PackageId = "game/goal008-gameplay-family",
            Commands = [],
            StateEvidence = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["scenarioId"] = spec.ScenarioId,
                ["copiedIds"] = string.Join("|", spec.PackageRuntimeIds.OrderBy(item => item, StringComparer.Ordinal))
            },
            RestoredStateEvidence = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["scenarioId"] = spec.ScenarioId,
                ["copiedIds"] = string.Join("|", spec.PackageRuntimeIds.OrderBy(item => item, StringComparer.Ordinal))
            },
            SaveLoadRoundtripPassed = true
        };

        var hash = ComputeHash(JsonSerializer.Serialize(evidence.StateEvidence, JsonOptions));
        return evidence with
        {
            RuntimeStateHash = hash,
            RestoredRuntimeStateHash = hash,
            RuntimeEvidenceHash = ComputeHash(JsonSerializer.Serialize(evidence, JsonOptions))
        };
    }

    private static GamePackageDefinition BuildPackage() => new()
    {
        Manifest = new GameManifest
        {
            PackageId = "game/goal008-gameplay-family",
            Title = "Goal 008 Gameplay Family Proof",
            StartMapId = "map/goal008-start"
        },
        Game = new GameDefinition
        {
            Maps = [new MapDefinition { Id = "map/goal008-start", Name = "Goal 008 Start", Width = 1, Height = 1 }],
            Items =
            [
                new ItemDefinition
                {
                    Id = "item/field_ration",
                    Name = "Field Ration",
                    Kind = "consumable",
                    Tags = ["consumable", "supply"],
                    Metadata = new Dictionary<string, string> { ["consumeOnUse"] = "true" },
                    UseEffects =
                    [
                        new EffectDefinition { Type = "status", Args = new Dictionary<string, string> { ["statusId"] = "status/focused", ["amount"] = "3" } },
                        new EffectDefinition { Type = "flag", Args = new Dictionary<string, string> { ["flagId"] = "flag/item_used", ["value"] = "completed" } }
                    ]
                },
                new ItemDefinition
                {
                    Id = "item/focus_tonic",
                    Name = "Focus Tonic",
                    Kind = "consumable",
                    Tags = ["consumable", "status-source"],
                    Metadata = new Dictionary<string, string> { ["consumeOnUse"] = "true" },
                    UseEffects =
                    [
                        new EffectDefinition { Type = "status", Args = new Dictionary<string, string> { ["statusId"] = "status/focused", ["amount"] = "3" } },
                        new EffectDefinition { Type = "flag", Args = new Dictionary<string, string> { ["flagId"] = "flag/status_chain", ["value"] = "completed" } }
                    ]
                },
                new ItemDefinition { Id = "item/scavenger_tool", Name = "Scavenger Tool", Kind = "tool", Tags = ["tool"], Metadata = new Dictionary<string, string> { ["equip_slot"] = "slot/tool" } },
                new ItemDefinition { Id = "item/scrap", Name = "Scrap", Kind = "material", Tags = ["material"] },
                new ItemDefinition { Id = "item/thread", Name = "Thread", Kind = "material", Tags = ["material"] },
                new ItemDefinition { Id = "item/repair_wrap", Name = "Repair Wrap", Kind = "crafted", Tags = ["crafted"] },
                new ItemDefinition { Id = "item/trade_token", Name = "Trade Token", Kind = "currency", Tags = ["currency"] },
                new ItemDefinition { Id = "item/signal_charm", Name = "Signal Charm", Kind = "trinket", Tags = ["charm"] }
            ],
            Statuses =
            [
                new StatusDefinition { Id = "status/focused", Name = "Focused", DurationMode = "turns", Kind = "buff" }
            ],
            EquipmentSlots =
            [
                new EquipmentSlotDefinition { Id = "slot/tool", Name = "Tool", AllowedKinds = ["tool"], AllowedTags = ["tool"] },
                new EquipmentSlotDefinition { Id = "slot/charm", Name = "Charm", AllowedKinds = ["trinket"], AllowedTags = ["charm"] }
            ],
            Recipes =
            [
                new RecipeDefinition
                {
                    Id = "recipe/repair_wrap",
                    Name = "Repair Wrap",
                    Inputs =
                    [
                        new CostDefinition { Kind = "item", Id = "item/scrap", Amount = 2 },
                        new CostDefinition { Kind = "item", Id = "item/thread", Amount = 1 }
                    ],
                    Outputs = [new OutputDefinition { Kind = "item", Id = "item/repair_wrap", Amount = 1 }]
                }
            ],
            Transactions =
            [
                new TransactionDefinition
                {
                    Id = "transaction/buy_signal_charm",
                    Name = "Buy Signal Charm",
                    Costs = [new CostDefinition { Kind = "item", Id = "item/trade_token", Amount = 3 }],
                    Outputs = [new OutputDefinition { Kind = "item", Id = "item/signal_charm", Amount = 1 }]
                }
            ],
            Inventories =
            [
                new InventoryDefinition
                {
                    Id = "inventory/player",
                    OwnerKind = "player",
                    OwnerId = "player"
                }
            ]
        }
    };

    private static RulePackGameplayFamilyDeclarations BuildDeclarations() => new()
    {
        RulePackId = "rule_pack/gameplay_family_foundations_v1",
        RulesVersion = "goal008_v1",
        Items =
        [
            new GameplayItemDeclaration { DeclarationId = "decl/item/field_ration", PackageItemId = "item/field_ration", FamilyId = "family/item_use" },
            new GameplayItemDeclaration { DeclarationId = "decl/item/focus_tonic", PackageItemId = "item/focus_tonic", FamilyId = "family/status_effects" }
        ],
        Equipment =
        [
            new GameplayEquipmentDeclaration { DeclarationId = "decl/equipment/scavenger_tool", PackageItemId = "item/scavenger_tool", PackageSlotId = "slot/tool", FamilyId = "family/equipment" }
        ],
        Recipes =
        [
            new GameplayRecipeDeclaration
            {
                DeclarationId = "decl/recipe/repair_wrap",
                PackageRecipeId = "recipe/repair_wrap",
                FamilyId = "family/crafting",
                InputItemIds = ["item/scrap", "item/thread"],
                OutputItemIds = ["item/repair_wrap"]
            }
        ],
        Transactions =
        [
            new GameplayTransactionDeclaration
            {
                DeclarationId = "decl/transaction/buy_signal_charm",
                PackageTransactionId = "transaction/buy_signal_charm",
                FamilyId = "family/trading",
                CostItemIds = ["item/trade_token"],
                OutputItemIds = ["item/signal_charm"]
            }
        ],
        Statuses =
        [
            new GameplayStatusDeclaration { DeclarationId = "decl/status/focused", PackageStatusId = "status/focused", FamilyId = "family/status_effects", DurationTicks = 3 }
        ]
    };

    private static string RenderReport(RulePackGameplayFamilyReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Rule-Pack Gameplay Family Report");
        builder.AppendLine();
        builder.AppendLine("- Accepted: " + report.Accepted.ToString().ToLowerInvariant());
        builder.AppendLine("- Manual gate: " + report.ManualGate);
        builder.AppendLine("- Goal 007 gate recorded: " + report.Goal007GateRecorded.ToString().ToLowerInvariant());
        builder.AppendLine("- Completed slices: " + string.Join(", ", report.CompletedSlices));
        builder.AppendLine("- Valid scenarios: " + report.ValidScenarioCount);
        builder.AppendLine("- Invalid scenarios: " + report.InvalidScenarioCount);
        builder.AppendLine("- Package/rule binding audit: " + report.PackageRuleBindingAuditPassed.ToString().ToLowerInvariant());
        builder.AppendLine("- Runtime execution: " + report.GameplayRuntimeExecutionPassed.ToString().ToLowerInvariant());
        builder.AppendLine("- Save/load exact state: " + report.SaveLoadRoundtripPassed.ToString().ToLowerInvariant());
        builder.AppendLine("- Deterministic replay: " + report.DeterministicReplayPassed.ToString().ToLowerInvariant());
        builder.AppendLine("- Public GamePackage schema changed: " + report.PublicGamePackageSchemaChanged.ToString().ToLowerInvariant());
        builder.AppendLine();
        foreach (var scenario in report.Scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal))
        {
            builder.AppendLine("## " + scenario.ScenarioId);
            builder.AppendLine();
            builder.AppendLine("- Expected valid: " + scenario.ExpectedValid.ToString().ToLowerInvariant());
            builder.AppendLine("- Actual valid: " + scenario.ActualValid.ToString().ToLowerInvariant());
            builder.AppendLine("- Families: " + string.Join(", ", scenario.SelectedGameplayFamilyIds));
            builder.AppendLine("- Runtime boundary: " + scenario.RuntimeEvidence.RuntimeBoundary.AdapterId + " / " + scenario.RuntimeEvidence.RuntimeBoundary.RuntimeServiceType);
            builder.AppendLine("- Commands: " + string.Join(" -> ", scenario.RuntimeEvidence.Commands.Select(item => item.CommandType + ":" + item.TargetId)));
            builder.AppendLine("- Diagnostics: " + string.Join(", ", scenario.Diagnostics.Select(item => item.Code).Distinct().OrderBy(item => item, StringComparer.Ordinal)));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string RenderVerification(RulePackGameplayFamilyReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Rule-Pack Gameplay Family Verification");
        builder.AppendLine();
        builder.AppendLine("Final gate:");
        builder.AppendLine();
        builder.AppendLine("```text");
        builder.AppendLine(report.ManualGate);
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("This report does not mark the manual gate passed.");
        builder.AppendLine();
        builder.AppendLine("- Accepted: " + report.Accepted.ToString().ToLowerInvariant());
        builder.AppendLine("- S071-S077 completed: " + string.Join(", ", report.CompletedSlices));
        builder.AppendLine("- Valid accepted: " + report.ValidScenariosAccepted.ToString().ToLowerInvariant());
        builder.AppendLine("- Invalid rejected: " + report.InvalidScenariosRejected.ToString().ToLowerInvariant());
        builder.AppendLine("- External execution flags all false: " + report.ExternalExecution.AllFalse.ToString().ToLowerInvariant());
        builder.AppendLine("- Next work created: false");
        return builder.ToString();
    }

    private static RulePackGameplayFamilyDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static IReadOnlyList<RulePackGameplayFamilyDiagnostic> SortDiagnostics(IEnumerable<RulePackGameplayFamilyDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static GamePackageDefinition ClonePackage(GamePackageDefinition package) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(JsonSerializer.Serialize(package, JsonOptions), JsonOptions) ?? new GamePackageDefinition();

    private static RulePackGameplayFamilyDeclarations CloneDeclarations(RulePackGameplayFamilyDeclarations declarations) =>
        JsonSerializer.Deserialize<RulePackGameplayFamilyDeclarations>(JsonSerializer.Serialize(declarations, JsonOptions), JsonOptions) ?? new RulePackGameplayFamilyDeclarations();

    private static bool DictionaryEquals(IReadOnlyDictionary<string, string> left, IReadOnlyDictionary<string, string> right) =>
        left.Count == right.Count && left.All(pair => right.TryGetValue(pair.Key, out var value) && value == pair.Value);

    private sealed record DeclarationIndex(
        string DeclarationId,
        string FamilyId,
        string Kind,
        string PackageItemId,
        string PackageSlotId,
        string PackageRecipeId,
        string PackageTransactionId,
        string PackageStatusId,
        object Source);

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Rule-pack gameplay-family output path must stay under the project root.");
        }
    }

    private sealed record GameplayFamilyScenarioSpec
    {
        public string ScenarioId { get; init; } = string.Empty;
        public string Seed { get; init; } = string.Empty;
        public string InvalidKind { get; init; } = string.Empty;
        public IReadOnlyList<string> FamilyIds { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> SourceDeclarationIds { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> PackageRuntimeIds { get; init; } = Array.Empty<string>();
        public IReadOnlyDictionary<string, double> InitialInventoryAmounts { get; init; } = new SortedDictionary<string, double>(StringComparer.Ordinal);
        public string CompletionFlagId { get; init; } = string.Empty;
        public IReadOnlyList<GameplayCommandSpec> Commands { get; init; } = Array.Empty<GameplayCommandSpec>();
    }

    public sealed class UnavailableRulePackGameplayFamilyRuntimeAdapter : IRulePackGameplayFamilyRuntimeAdapter
    {
        public RulePackGameplayFamilyRuntimeEvidence Run(RulePackGameplayFamilyRuntimeRequest request) => new()
        {
            RuntimeAttempted = false,
            RuntimeStartSucceeded = false,
            RuntimeStateOwner = "GameRuntimeState",
            PackageId = request.Package.Manifest.PackageId,
            RuntimeBoundary = new GameplayRuntimeBoundaryEvidence
            {
                AdapterId = nameof(UnavailableRulePackGameplayFamilyRuntimeAdapter),
                RuntimeServiceType = string.Empty,
                StateFactoryType = string.Empty,
                SerializerType = string.Empty,
                SnapshotStoreType = string.Empty,
                UsedGameRuntimeService = false,
                UsedRuntimeStateFactory = false
            },
            Diagnostics =
            [
                Diagnostic("error", "gameplay_family.runtime_adapter_unavailable", request.ScenarioId, "Rule-pack gameplay-family acceptance requires an injected real runtime adapter.")
            ]
        };
    }
}

public interface IRulePackGameplayFamilyRuntimeAdapter
{
    RulePackGameplayFamilyRuntimeEvidence Run(RulePackGameplayFamilyRuntimeRequest request);
}

public sealed record RulePackGameplayFamilyAcceptanceResult
{
    public RulePackGameplayFamilyReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record RulePackGameplayFamilyWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record RulePackGameplayFamilyReport
{
    public string SchemaVersion { get; init; } = "1";
    public string DeterministicHash { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public bool Goal007GateRecorded { get; init; }
    public IReadOnlyList<string> CompletedSlices { get; init; } = Array.Empty<string>();
    public int ScenarioCount { get; init; }
    public int ValidScenarioCount { get; init; }
    public int InvalidScenarioCount { get; init; }
    public bool ValidScenariosAccepted { get; init; }
    public bool InvalidScenariosRejected { get; init; }
    public bool PackageRuleBindingAuditPassed { get; init; }
    public bool GameplayRuntimeExecutionPassed { get; init; }
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool DeterministicReplayPassed { get; init; }
    public bool FakeRuntimeSuccessRejected { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public RulePackGameplayFamilyExternalExecutionFlags ExternalExecution { get; init; } = new();
    public RulePackGameplayFamilyDeclarations Declarations { get; init; } = new();
    public IReadOnlyList<RulePackGameplayFamilyScenario> Scenarios { get; init; } = Array.Empty<RulePackGameplayFamilyScenario>();
    public IReadOnlyList<RulePackGameplayFamilyDiagnostic> Diagnostics { get; init; } = Array.Empty<RulePackGameplayFamilyDiagnostic>();
    public IReadOnlyList<string> RemainingPrimitiveLimits { get; init; } = Array.Empty<string>();
}

public sealed record RulePackGameplayFamilyDeclarations
{
    public string RulePackId { get; init; } = string.Empty;
    public string RulesVersion { get; init; } = string.Empty;
    public IReadOnlyList<GameplayItemDeclaration> Items { get; set; } = Array.Empty<GameplayItemDeclaration>();
    public IReadOnlyList<GameplayEquipmentDeclaration> Equipment { get; set; } = Array.Empty<GameplayEquipmentDeclaration>();
    public IReadOnlyList<GameplayRecipeDeclaration> Recipes { get; set; } = Array.Empty<GameplayRecipeDeclaration>();
    public IReadOnlyList<GameplayTransactionDeclaration> Transactions { get; set; } = Array.Empty<GameplayTransactionDeclaration>();
    public IReadOnlyList<GameplayStatusDeclaration> Statuses { get; set; } = Array.Empty<GameplayStatusDeclaration>();
}

public sealed record GameplayItemDeclaration
{
    public string DeclarationId { get; init; } = string.Empty;
    public string PackageItemId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
}

public sealed record GameplayEquipmentDeclaration
{
    public string DeclarationId { get; init; } = string.Empty;
    public string PackageItemId { get; init; } = string.Empty;
    public string PackageSlotId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
}

public sealed record GameplayRecipeDeclaration
{
    public string DeclarationId { get; init; } = string.Empty;
    public string PackageRecipeId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public IReadOnlyList<string> InputItemIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> OutputItemIds { get; init; } = Array.Empty<string>();
}

public sealed record GameplayTransactionDeclaration
{
    public string DeclarationId { get; init; } = string.Empty;
    public string PackageTransactionId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public IReadOnlyList<string> CostItemIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> OutputItemIds { get; init; } = Array.Empty<string>();
}

public sealed record GameplayStatusDeclaration
{
    public string DeclarationId { get; init; } = string.Empty;
    public string PackageStatusId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public int DurationTicks { get; init; }
}

public sealed record RulePackGameplayFamilyScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string InvalidKind { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedGameplayFamilyIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SourceDeclarationIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PackageRuntimeIds { get; init; } = Array.Empty<string>();
    public RulePackGameplayFamilyBindingAudit PackageBindingAudit { get; init; } = new();
    public RulePackGameplayFamilyRuntimeEvidence RuntimeEvidence { get; init; } = new();
    public IReadOnlyList<RulePackGameplayFamilyDiagnostic> Diagnostics { get; init; } = Array.Empty<RulePackGameplayFamilyDiagnostic>();
}

public sealed record RulePackGameplayFamilyBindingAudit
{
    public bool Passed { get; init; }
    public string PackageId { get; init; } = string.Empty;
    public IReadOnlyList<string> AuditedDeclarationIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> AuditedPackageRuntimeIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<RulePackGameplayFamilyDiagnostic> Diagnostics { get; init; } = Array.Empty<RulePackGameplayFamilyDiagnostic>();
}

public sealed record RulePackGameplayFamilyRuntimeRequest
{
    public string ScenarioId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public GamePackageDefinition Package { get; init; } = new();
    public RulePackGameplayFamilyDeclarations Declarations { get; init; } = new();
    public IReadOnlyDictionary<string, double> InitialInventoryAmounts { get; init; } = new SortedDictionary<string, double>(StringComparer.Ordinal);
    public IReadOnlyList<GameplayCommandSpec> Commands { get; init; } = Array.Empty<GameplayCommandSpec>();
    public string CompletionFlagId { get; init; } = string.Empty;
}

public sealed record GameplayCommandSpec
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public string InventoryId { get; init; } = "inventory/player";
    public string Value { get; init; } = string.Empty;

    public static GameplayCommandSpec UseItem(string commandId, string itemId, string inventoryId) =>
        new() { CommandId = commandId, CommandType = "gameplay/use_item", TargetId = itemId, InventoryId = inventoryId };

    public static GameplayCommandSpec EquipItem(string commandId, string itemId, string slotId, string inventoryId) =>
        new() { CommandId = commandId, CommandType = "gameplay/equip_item", TargetId = itemId, SecondaryTargetId = slotId, InventoryId = inventoryId };

    public static GameplayCommandSpec CraftRecipe(string commandId, string recipeId, string inventoryId) =>
        new() { CommandId = commandId, CommandType = "gameplay/craft_recipe", TargetId = recipeId, InventoryId = inventoryId };

    public static GameplayCommandSpec ExecuteTransaction(string commandId, string transactionId, string inventoryId) =>
        new() { CommandId = commandId, CommandType = "gameplay/execute_transaction", TargetId = transactionId, InventoryId = inventoryId };

    public static GameplayCommandSpec SetFlag(string commandId, string flagId, string value) =>
        new() { CommandId = commandId, CommandType = "gameplay/set_flag", TargetId = flagId, Value = value };
}

public sealed record RulePackGameplayFamilyRuntimeEvidence
{
    public bool RuntimeAttempted { get; init; }
    public bool RuntimeStartSucceeded { get; init; }
    public string RuntimeStateOwner { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public GameplayRuntimeBoundaryEvidence RuntimeBoundary { get; init; } = new();
    public string RuntimeEvidenceHash { get; init; } = string.Empty;
    public IReadOnlyList<RulePackGameplayFamilyRuntimeCommandEvidence> Commands { get; init; } = Array.Empty<RulePackGameplayFamilyRuntimeCommandEvidence>();
    public IReadOnlyDictionary<string, string> InventoryBefore { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> InventoryAfter { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EquipmentBefore { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EquipmentAfter { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> StatusBefore { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> StatusAfter { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public GameplayCompletionRewardEvidence CompletionRewardEvidence { get; init; } = new();
    public string RuntimeStateHash { get; init; } = string.Empty;
    public string RestoredRuntimeStateHash { get; init; } = string.Empty;
    public bool SaveLoadRoundtripPassed { get; init; }
    public GameplaySaveLoadEvidence SaveLoadEvidence { get; init; } = new();
    public IReadOnlyDictionary<string, string> StateEvidence { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> RestoredStateEvidence { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<RulePackGameplayFamilyDiagnostic> Diagnostics { get; init; } = Array.Empty<RulePackGameplayFamilyDiagnostic>();
}

public sealed record RulePackGameplayFamilyRuntimeCommandEvidence
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public bool Succeeded { get; init; }
    public string DiagnosticCode { get; init; } = string.Empty;
    public string DiagnosticMessage { get; init; } = string.Empty;
    public IReadOnlyList<string> RuntimeEventTypes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RuntimeDiagnosticCodes { get; init; } = Array.Empty<string>();
    public GameplayInventoryDelta InventoryDelta { get; init; } = new();
    public GameplayEquipmentDelta EquipmentDelta { get; init; } = new();
    public GameplayCraftingDelta CraftingDelta { get; init; } = new();
    public GameplayTradeDelta TradeDelta { get; init; } = new();
    public GameplayStatusDelta StatusDelta { get; init; } = new();
    public GameplayCompletionDelta CompletionDelta { get; init; } = new();
}

public sealed record GameplayRuntimeBoundaryEvidence
{
    public string AdapterId { get; init; } = string.Empty;
    public string RuntimeServiceType { get; init; } = string.Empty;
    public string StateFactoryType { get; init; } = string.Empty;
    public string SerializerType { get; init; } = string.Empty;
    public string SnapshotStoreType { get; init; } = string.Empty;
    public bool UsedGameRuntimeService { get; init; }
    public bool UsedRuntimeStateFactory { get; init; }
}

public sealed record GameplaySaveLoadEvidence
{
    public bool UsedRuntimeStateSerializer { get; init; }
    public bool UsedRuntimeSnapshotStore { get; init; }
    public bool SerializedFullState { get; init; }
    public string SerializedStateHash { get; init; } = string.Empty;
    public string RestoredSerializedStateHash { get; init; } = string.Empty;
    public string SnapshotSlotName { get; init; } = string.Empty;
    public bool SnapshotSaveSucceeded { get; init; }
    public bool SnapshotLoadSucceeded { get; init; }
}

public sealed record GameplayInventoryDelta
{
    public bool Changed { get; init; }
    public IReadOnlyDictionary<string, string> Before { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> After { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record GameplayEquipmentDelta
{
    public bool Changed { get; init; }
    public IReadOnlyDictionary<string, string> Before { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> After { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record GameplayCraftingDelta
{
    public bool Changed { get; init; }
    public IReadOnlyList<GameplayItemAmountChange> Inputs { get; init; } = Array.Empty<GameplayItemAmountChange>();
    public IReadOnlyList<GameplayItemAmountChange> Outputs { get; init; } = Array.Empty<GameplayItemAmountChange>();
}

public sealed record GameplayTradeDelta
{
    public bool Changed { get; init; }
    public IReadOnlyList<GameplayItemAmountChange> Costs { get; init; } = Array.Empty<GameplayItemAmountChange>();
    public IReadOnlyList<GameplayItemAmountChange> Outputs { get; init; } = Array.Empty<GameplayItemAmountChange>();
}

public sealed record GameplayStatusDelta
{
    public bool Changed { get; init; }
    public IReadOnlyDictionary<string, string> Before { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> After { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record GameplayCompletionDelta
{
    public bool Changed { get; init; }
    public IReadOnlyDictionary<string, string> Before { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> After { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record GameplayItemAmountChange
{
    public string ItemId { get; init; } = string.Empty;
    public double AmountBefore { get; init; }
    public double AmountAfter { get; init; }
}

public sealed record GameplayCompletionRewardEvidence
{
    public string CompletionFlagId { get; init; } = string.Empty;
    public string CompletionFlagBefore { get; init; } = string.Empty;
    public string CompletionFlagAfter { get; init; } = string.Empty;
    public IReadOnlyList<string> RewardItemIds { get; init; } = Array.Empty<string>();
}

public sealed record RulePackGameplayFamilyExternalExecutionFlags
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

public sealed record RulePackGameplayFamilyDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
