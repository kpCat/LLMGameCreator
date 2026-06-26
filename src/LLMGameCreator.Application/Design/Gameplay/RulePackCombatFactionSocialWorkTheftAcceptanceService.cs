using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.Gameplay;

public sealed class RulePackCombatFactionSocialWorkTheftAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/rule-pack-combat-faction-social-work-theft";
    public const string ReportJsonFileName = "rule-pack-combat-faction-social-work-theft-report.json";
    public const string ReportMarkdownFileName = "rule-pack-combat-faction-social-work-theft-report.md";
    public const string VerificationMarkdownFileName = "rule-pack-combat-faction-social-work-theft-verification.md";
    public const string ManualGate = "rule_pack_combat_faction_social_work_theft_artifact_verification";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IRulePackCombatFactionSocialWorkTheftRuntimeAdapter _runtimeAdapter;

    static RulePackCombatFactionSocialWorkTheftAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public RulePackCombatFactionSocialWorkTheftAcceptanceService(IRulePackCombatFactionSocialWorkTheftRuntimeAdapter? runtimeAdapter = null)
    {
        _runtimeAdapter = runtimeAdapter ?? new UnavailableRulePackCombatFactionSocialWorkTheftRuntimeAdapter();
    }

    public RulePackCombatFactionSocialWorkTheftAcceptanceResult Build(string? projectRootPath = null)
    {
        var package = BuildPackage();
        var declarations = BuildDeclarations();
        var validSpecs = BuildValidSpecs();
        var invalidSpecs = BuildInvalidSpecs();

        var validScenarios = validSpecs.Select(spec => BuildScenario(package, declarations, spec, expectedValid: true)).ToList();
        var invalidScenarios = invalidSpecs.Select(spec => BuildInvalidScenario(package, declarations, spec)).ToList();
        var repeated = BuildScenario(package, declarations, validSpecs.Single(item => item.ScenarioId == "combined_combat_social_work_theft_loop"), expectedValid: true);
        var scenarios = validScenarios.Concat(invalidScenarios).OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList();

        var validAccepted = validScenarios.All(item => item.ExpectedValid && item.ActualValid);
        var invalidRejected = invalidScenarios.All(item => !item.ExpectedValid && !item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error"));
        var bindingPassed = validScenarios.All(item => item.PackageBindingAudit.Passed);
        var runtimePassed = validScenarios.All(ScenarioRuntimePassed);
        var saveLoadPassed = validScenarios.All(item =>
            item.RuntimeEvidence.SaveLoadRoundtripPassed &&
            item.RuntimeEvidence.SaveLoadEvidence.UsedRuntimeStateSerializer &&
            item.RuntimeEvidence.SaveLoadEvidence.UsedRuntimeSnapshotStore &&
            item.RuntimeEvidence.SaveLoadEvidence.SerializedFullState &&
            DictionaryEquals(item.RuntimeEvidence.StateEvidence, item.RuntimeEvidence.RestoredStateEvidence));
        var deterministicReplayPassed =
            validScenarios.Single(item => item.ScenarioId == "combined_combat_social_work_theft_loop").DeterministicHash == repeated.DeterministicHash &&
            validScenarios.Single(item => item.ScenarioId == "combined_combat_social_work_theft_loop").RuntimeEvidence.RuntimeStateHash == repeated.RuntimeEvidence.RuntimeStateHash &&
            validScenarios.Single(item => item.ScenarioId == "combined_combat_social_work_theft_loop").RuntimeEvidence.RestoredRuntimeStateHash == repeated.RuntimeEvidence.RestoredRuntimeStateHash;
        var isolationPassed = validScenarios.All(item => item.RuntimeEvidence.ScenarioIsolationPassed) &&
                              invalidScenarios.Any(item => item.ScenarioId == "invalid_cross_scenario_state_leakage" && !item.ActualValid);
        var fakeSuccessRejected = invalidScenarios.Any(item =>
            item.ScenarioId == "invalid_fake_runtime_success" &&
            !item.ActualValid &&
            item.Diagnostics.Any(diagnostic => diagnostic.Code == "combat_family.evidence.required_command_missing" || diagnostic.Code == "combat_family.evidence.runtime_boundary_missing"));

        var diagnostics = new List<RulePackCombatFactionSocialWorkTheftDiagnostic>
        {
            Diagnostic("info", "combat_family.goal008_gate_recorded", "rule_pack_gameplay_family_artifact_verification", "User-confirmed Goal 008 rule-pack gameplay family artifact verification is recorded as passed."),
            Diagnostic("info", "combat_family.no_external_execution", "harness", "No LLM, RAG, provider, Lua, Unity or media execution was invoked."),
            Diagnostic(validAccepted ? "info" : "error", validAccepted ? "combat_family.valid_scenarios_accepted" : "combat_family.valid_scenarios_failed", "valid_scenarios", "All required valid combat/faction/social/work/theft scenarios must be accepted."),
            Diagnostic(invalidRejected ? "info" : "error", invalidRejected ? "combat_family.invalid_scenarios_rejected" : "combat_family.invalid_scenarios_not_rejected", "invalid_scenarios", "All required invalid scenarios must fail by causal diagnostics or runtime evidence."),
            Diagnostic(bindingPassed ? "info" : "error", bindingPassed ? "combat_family.package_bindings_verified" : "combat_family.package_bindings_failed", "package_bindings", "Every selected declaration id must bind to exact package/runtime ids before runtime execution."),
            Diagnostic(runtimePassed ? "info" : "error", runtimePassed ? "combat_family.runtime_commands_executed" : "combat_family.runtime_commands_missing", "runtime_commands", "Runtime command evidence must contain successful covered commands and state deltas."),
            Diagnostic(saveLoadPassed ? "info" : "error", saveLoadPassed ? "combat_family.save_load_roundtrip_passed" : "combat_family.save_load_roundtrip_failed", "runtime_state", "Save/load must restore exact combat-family state evidence."),
            Diagnostic(deterministicReplayPassed ? "info" : "error", deterministicReplayPassed ? "combat_family.replay_stable" : "combat_family.replay_unstable", "combined_combat_social_work_theft_loop", "Repeated command execution must produce stable scenario and runtime hashes."),
            Diagnostic(isolationPassed ? "info" : "error", isolationPassed ? "combat_family.isolation_passed" : "combat_family.isolation_failed", "scenario_isolation", "Scenario execution must not retain prior encounter, dialogue, faction, inventory, flags or command history."),
            Diagnostic(fakeSuccessRejected ? "info" : "error", fakeSuccessRejected ? "combat_family.fake_success_rejected" : "combat_family.fake_success_not_rejected", "invalid_fake_runtime_success", "Copied ids plus a success boolean must not satisfy acceptance.")
        };
        diagnostics.AddRange(scenarios.SelectMany(item => item.Diagnostics));

        var reportWithoutHash = new RulePackCombatFactionSocialWorkTheftReport
        {
            Accepted = validAccepted && invalidRejected && bindingPassed && runtimePassed && saveLoadPassed && deterministicReplayPassed && isolationPassed && fakeSuccessRejected,
            ManualGate = ManualGate,
            Goal008GateRecorded = true,
            CompletedSlices = ["S078", "S079", "S080", "S081", "S082", "S083", "S084"],
            ScenarioCount = scenarios.Count,
            ValidScenarioCount = validScenarios.Count,
            InvalidScenarioCount = invalidScenarios.Count,
            ValidScenariosAccepted = validAccepted,
            InvalidScenariosRejected = invalidRejected,
            PackageRuleBindingAuditPassed = bindingPassed,
            CombatFactionSocialWorkTheftRuntimeExecutionPassed = runtimePassed,
            SaveLoadRoundtripPassed = saveLoadPassed,
            DeterministicReplayPassed = deterministicReplayPassed,
            ScenarioIsolationPassed = isolationPassed,
            FakeRuntimeSuccessRejected = fakeSuccessRejected,
            PublicGamePackageSchemaChanged = false,
            ExternalExecution = new RulePackCombatFactionSocialWorkTheftExternalExecutionFlags(),
            BoundedWorkMeaning = "work means a data-driven interaction or transaction contract over existing requirement, output, item, flag and reputation primitives",
            BoundedTheftMeaning = "theft means a data-driven container transfer plus explicit rule-pack flag and reputation consequences; no dynamic stealth or detection AI is claimed",
            Declarations = declarations,
            Scenarios = scenarios,
            Diagnostics = SortDiagnostics(diagnostics),
            RemainingPrimitiveLimits =
            [
                "work has no schedules, employers, time wages or economy simulation",
                "theft has no witnesses, detection chance, law ownership model, stealth AI or relationship simulation",
                "social evidence is bounded to package dialogue choices and supported runtime consequences",
                "combat evidence is bounded to existing encounter turns, abilities, AI and reward outputs",
                "Unity presentation, Lua/provider/media execution and future content scale-up remain out of scope"
            ]
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new RulePackCombatFactionSocialWorkTheftAcceptanceResult
        {
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report)
        };
    }

    public async Task<RulePackCombatFactionSocialWorkTheftWriteResult> WriteAsync(
        string projectRootPath,
        RulePackCombatFactionSocialWorkTheftAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "rule-pack-combat-faction-social-work-theft"));
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

        return new RulePackCombatFactionSocialWorkTheftWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<RulePackCombatFactionSocialWorkTheftWriteResult> BuildAndWriteAsync(string projectRootPath, CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private RulePackCombatFactionSocialWorkTheftScenario BuildScenario(
        GamePackageDefinition package,
        CombatFamilyDeclarations declarations,
        CombatFamilyScenarioSpec spec,
        bool expectedValid)
    {
        var bindingAudit = AuditBindings(package, declarations, spec);
        var diagnostics = new List<RulePackCombatFactionSocialWorkTheftDiagnostic>(bindingAudit.Diagnostics);
        var runtimeEvidence = bindingAudit.Passed
            ? _runtimeAdapter.Run(new RulePackCombatFactionSocialWorkTheftRuntimeRequest
            {
                ScenarioId = spec.ScenarioId,
                Seed = spec.Seed,
                Package = package,
                Declarations = declarations,
                Commands = spec.Commands,
                ExpectedScenarioStateMarker = spec.ScenarioStateMarker
            })
            : new RulePackCombatFactionSocialWorkTheftRuntimeEvidence();

        diagnostics.AddRange(runtimeEvidence.Diagnostics);
        var evidenceDiagnostics = ValidateRuntimeEvidence(spec, runtimeEvidence);
        diagnostics.AddRange(evidenceDiagnostics);
        var actualValid = IsScenarioAccepted(bindingAudit, runtimeEvidence, evidenceDiagnostics);

        var scenarioWithoutHash = new RulePackCombatFactionSocialWorkTheftScenario
        {
            ScenarioId = spec.ScenarioId,
            Seed = spec.Seed,
            ExpectedValid = expectedValid,
            ActualValid = actualValid,
            InvalidKind = spec.InvalidKind,
            SelectedFamilyIds = spec.FamilyIds,
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

    private RulePackCombatFactionSocialWorkTheftScenario BuildInvalidScenario(
        GamePackageDefinition package,
        CombatFamilyDeclarations declarations,
        CombatFamilyScenarioSpec spec)
    {
        var invalidPackage = ClonePackage(package);
        var invalidDeclarations = CloneDeclarations(declarations);
        var invalidSpec = spec;
        ApplyInvalidMutation(invalidPackage, invalidDeclarations, ref invalidSpec);

        if (invalidSpec.InvalidKind == "fake_runtime_success")
        {
            var bindingAudit = AuditBindings(invalidPackage, invalidDeclarations, invalidSpec);
            var runtimeEvidence = FakeRuntimeSuccess(invalidSpec);
            var diagnostics = new List<RulePackCombatFactionSocialWorkTheftDiagnostic>(bindingAudit.Diagnostics);
            diagnostics.AddRange(runtimeEvidence.Diagnostics);
            var evidenceDiagnostics = ValidateRuntimeEvidence(invalidSpec, runtimeEvidence);
            diagnostics.AddRange(evidenceDiagnostics);
            var actualValid = IsScenarioAccepted(bindingAudit, runtimeEvidence, evidenceDiagnostics);
            return BuildInvalidScenarioRecord(invalidSpec, bindingAudit, runtimeEvidence, diagnostics, actualValid);
        }

        return BuildScenario(invalidPackage, invalidDeclarations, invalidSpec, expectedValid: false);
    }

    private static RulePackCombatFactionSocialWorkTheftScenario BuildInvalidScenarioRecord(
        CombatFamilyScenarioSpec spec,
        RulePackCombatFactionSocialWorkTheftBindingAudit bindingAudit,
        RulePackCombatFactionSocialWorkTheftRuntimeEvidence runtimeEvidence,
        List<RulePackCombatFactionSocialWorkTheftDiagnostic> diagnostics,
        bool actualValid)
    {
        var scenarioWithoutHash = new RulePackCombatFactionSocialWorkTheftScenario
        {
            ScenarioId = spec.ScenarioId,
            Seed = spec.Seed,
            ExpectedValid = false,
            ActualValid = actualValid,
            InvalidKind = spec.InvalidKind,
            SelectedFamilyIds = spec.FamilyIds,
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

    private static RulePackCombatFactionSocialWorkTheftBindingAudit AuditBindings(
        GamePackageDefinition package,
        CombatFamilyDeclarations declarations,
        CombatFamilyScenarioSpec spec)
    {
        var diagnostics = new List<RulePackCombatFactionSocialWorkTheftDiagnostic>();
        var allDeclarations = declarations.Encounters.Select(item => new DeclarationIndex(item.DeclarationId, item.FamilyId, "encounter", item.PackageEncounterId, item.PlayerParticipantId, item.EnemyParticipantId, item.PlayerAbilityId, string.Empty, string.Empty, string.Empty, item))
            .Concat(declarations.Factions.Select(item => new DeclarationIndex(item.DeclarationId, item.FamilyId, "faction", string.Empty, string.Empty, string.Empty, string.Empty, item.PackageFactionId, string.Empty, string.Empty, item)))
            .Concat(declarations.Dialogues.Select(item => new DeclarationIndex(item.DeclarationId, item.FamilyId, "dialogue", string.Empty, string.Empty, string.Empty, string.Empty, item.FactionId, item.PackageDialogueId, item.PackageChoiceId, item)))
            .Concat(declarations.WorkContracts.Select(item => new DeclarationIndex(item.DeclarationId, item.FamilyId, "work", string.Empty, string.Empty, string.Empty, string.Empty, item.FactionId, item.PackageInteractionId, item.PackageTransactionId, item)))
            .Concat(declarations.TheftConsequences.Select(item => new DeclarationIndex(item.DeclarationId, item.FamilyId, "theft", string.Empty, string.Empty, string.Empty, string.Empty, item.FactionId, item.ContainerInventoryId, item.ItemId, item)))
            .ToList();

        var selected = new List<DeclarationIndex>();
        foreach (var sourceId in spec.SourceDeclarationIds)
        {
            var matches = allDeclarations.Where(item => item.DeclarationId == sourceId).ToList();
            if (matches.Count != 1)
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.unknown_source_declaration", sourceId, "Scenario selected declaration id must exist exactly once."));
                continue;
            }

            selected.Add(matches[0]);
            if (!spec.FamilyIds.Contains(matches[0].FamilyId, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.family_mismatch", sourceId, "Selected declaration family is not part of the scenario family set."));
            }
        }

        var selectedDeclarationIds = selected.Select(item => item.DeclarationId).ToHashSet(StringComparer.Ordinal);
        foreach (var command in spec.Commands)
        {
            if (string.IsNullOrWhiteSpace(command.SourceDeclarationId) || !selectedDeclarationIds.Contains(command.SourceDeclarationId))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.command_not_covered_by_declaration", command.CommandId, "Every command must be covered by an exact selected declaration."));
            }
        }

        foreach (var encounter in selected.Where(item => item.Kind == "encounter").Select(item => (CombatEncounterDeclaration)item.Source))
        {
            var packageEncounter = package.Game.Encounters.SingleOrDefault(item => item.Id == encounter.PackageEncounterId);
            if (packageEncounter == null)
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_encounter_ref", encounter.PackageEncounterId, "Selected encounter must exist in the package."));
                continue;
            }

            if (packageEncounter.Participants.Count(item => item.Id == encounter.PlayerParticipantId || item.Id == encounter.EnemyParticipantId) != 2)
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_participant_ref", encounter.PackageEncounterId, "Selected encounter participants must exist exactly."));
            }

            if (!package.Game.Abilities.Any(item => item.Id == encounter.PlayerAbilityId) || !package.Game.Abilities.Any(item => item.Id == encounter.EnemyAbilityId))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_ability_ref", encounter.PackageEncounterId, "Selected encounter abilities must exist exactly."));
            }

            if (!package.Game.Resources.Any(item => item.Id == encounter.HealthResourceId) || !package.Game.Resources.Any(item => item.Id == encounter.FocusResourceId))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_resource_ref", encounter.PackageEncounterId, "Selected encounter resources must exist exactly."));
            }
        }

        foreach (var faction in selected.Where(item => item.Kind == "faction").Select(item => (FactionReputationDeclaration)item.Source))
        {
            var packageFaction = package.Game.Factions.SingleOrDefault(item => item.Id == faction.PackageFactionId);
            if (packageFaction == null)
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_faction_ref", faction.PackageFactionId, "Selected faction must exist in the package."));
                continue;
            }

            if (faction.Amount == 0 || faction.ExpectedAfter < packageFaction.MinReputation || faction.ExpectedAfter > packageFaction.MaxReputation)
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.invalid_reputation_amount", faction.DeclarationId, "Reputation amount and clamped expected value must be valid."));
            }
        }

        foreach (var dialogue in selected.Where(item => item.Kind == "dialogue").Select(item => (SocialDialogueDeclaration)item.Source))
        {
            var packageDialogue = package.Game.Dialogues.SingleOrDefault(item => item.Id == dialogue.PackageDialogueId);
            if (packageDialogue == null)
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_dialogue_ref", dialogue.PackageDialogueId, "Selected dialogue must exist in the package."));
                continue;
            }

            if (!packageDialogue.Nodes.SelectMany(item => item.Choices).Any(item => item.Id == dialogue.PackageChoiceId))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_dialogue_choice_ref", dialogue.PackageChoiceId, "Selected dialogue choice must exist exactly."));
            }
        }

        foreach (var work in selected.Where(item => item.Kind == "work").Select(item => (WorkContractDeclaration)item.Source))
        {
            if (!package.Game.Interactions.Any(item => item.Id == work.PackageInteractionId))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_work_interaction_ref", work.PackageInteractionId, "Selected work interaction must exist in the package."));
            }

            var transaction = package.Game.Transactions.SingleOrDefault(item => item.Id == work.PackageTransactionId);
            if (transaction == null)
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_work_transaction_ref", work.PackageTransactionId, "Selected work transaction must exist in the package."));
            }
            else if (!transaction.Requirements.Any(item => item.Id == work.RequiredItemId) || !transaction.Outputs.Any(item => item.Id == work.RewardItemId))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.invalid_work_outputs", work.PackageTransactionId, "Work requirement and reward ids must resolve."));
            }
        }

        foreach (var theft in selected.Where(item => item.Kind == "theft").Select(item => (TheftConsequenceDeclaration)item.Source))
        {
            var container = package.Game.Inventories.SingleOrDefault(item => item.Id == theft.ContainerInventoryId);
            if (container == null || !IsContainer(container))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_theft_container_ref", theft.ContainerInventoryId, "Selected theft inventory must be a package container."));
            }
            else if (!container.Stacks.Any(item => item.ItemId == theft.ItemId && item.Amount >= theft.Amount))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_theft_item_ref", theft.ItemId, "Selected theft item must exist in the container initial state."));
            }

            if (theft.Amount <= 0)
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.theft_amount_invalid", theft.DeclarationId, "Theft transfer amount must be positive."));
            }

            if (!package.Game.Factions.Any(item => item.Id == theft.FactionId))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.missing_theft_faction_ref", theft.FactionId, "Theft consequence faction must exist."));
            }
        }

        var auditedIds = selected.Select(item => item.DeclarationId).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList();
        var auditedRuntimeIds = selected
            .SelectMany(DeclarationRuntimeIds)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

        foreach (var id in spec.PackageRuntimeIds)
        {
            if (!auditedRuntimeIds.Contains(id, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.audit.package_runtime_id_not_audited", id, "Scenario package/runtime id must be part of the selected binding audit."));
            }
        }

        return new RulePackCombatFactionSocialWorkTheftBindingAudit
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            PackageId = package.Manifest.PackageId,
            AuditedDeclarationIds = auditedIds,
            AuditedPackageRuntimeIds = auditedRuntimeIds,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IEnumerable<string> DeclarationRuntimeIds(DeclarationIndex item)
    {
        foreach (var id in new[] { item.PrimaryId, item.SecondaryId, item.TertiaryId, item.QuaternaryId, item.FactionId, item.DialogueOrInteractionId, item.ChoiceOrTransactionOrItemId })
        {
            yield return id;
        }

        switch (item.Source)
        {
            case CombatEncounterDeclaration encounter:
                yield return encounter.EnemyAbilityId;
                yield return encounter.HealthResourceId;
                yield return encounter.FocusResourceId;
                yield return "item/victory_token";
                break;
            case SocialDialogueDeclaration dialogue:
                yield return dialogue.PackageNodeId;
                yield return dialogue.FlagId;
                yield return "item/contract_note";
                break;
            case WorkContractDeclaration work:
                yield return work.RequiredItemId;
                yield return work.RewardItemId;
                yield return work.CompletionFlagId;
                break;
            case TheftConsequenceDeclaration theft:
                yield return theft.TheftFlagId;
                break;
        }
    }

    private static IReadOnlyList<RulePackCombatFactionSocialWorkTheftDiagnostic> ValidateRuntimeEvidence(
        CombatFamilyScenarioSpec spec,
        RulePackCombatFactionSocialWorkTheftRuntimeEvidence evidence)
    {
        var diagnostics = new List<RulePackCombatFactionSocialWorkTheftDiagnostic>();
        if (!evidence.RuntimeAttempted)
        {
            diagnostics.Add(Diagnostic("error", "combat_family.evidence.runtime_not_attempted", spec.ScenarioId, "Runtime must be attempted through an injected real adapter."));
        }

        if (!evidence.RuntimeStartSucceeded || !evidence.RuntimeBoundary.UsedGameRuntimeService || !evidence.RuntimeBoundary.UsedRuntimeStateFactory)
        {
            diagnostics.Add(Diagnostic("error", "combat_family.evidence.runtime_boundary_missing", spec.ScenarioId, "Evidence must come from GameRuntimeService and GameRuntimeStateFactory."));
        }

        foreach (var command in spec.Commands)
        {
            var evidenceCommand = evidence.Commands.FirstOrDefault(item => item.CommandId == command.CommandId);
            if (evidenceCommand == null)
            {
                diagnostics.Add(Diagnostic("error", "combat_family.evidence.required_command_missing", command.CommandId, "Every declared command must have runtime command evidence."));
                continue;
            }

            if (!evidenceCommand.Succeeded)
            {
                diagnostics.Add(Diagnostic("error", "combat_family.evidence.command_failed", command.CommandId, "Required runtime command failed."));
            }

            if (evidenceCommand.SourceDeclarationId != command.SourceDeclarationId || evidenceCommand.TargetId != command.TargetId)
            {
                diagnostics.Add(Diagnostic("error", "combat_family.evidence.command_correlation_mismatch", command.CommandId, "Command evidence must correlate to the exact declaration and target."));
            }

            if (!CommandHasExpectedDelta(command, evidenceCommand))
            {
                diagnostics.Add(Diagnostic("error", "combat_family.evidence.command_delta_missing", command.CommandId, "Command evidence is missing command-specific state delta."));
            }
        }

        if (!evidence.SaveLoadRoundtripPassed || !DictionaryEquals(evidence.StateEvidence, evidence.RestoredStateEvidence))
        {
            diagnostics.Add(Diagnostic("error", "combat_family.evidence.save_load_mismatch", spec.ScenarioId, "Full runtime state save/load evidence must match exactly."));
        }

        if (!evidence.ScenarioIsolationPassed)
        {
            diagnostics.Add(Diagnostic("error", "combat_family.evidence.cross_scenario_state_leakage", spec.ScenarioId, "Runtime evidence contains prior-scenario leakage."));
        }

        return SortDiagnostics(diagnostics);
    }

    private static bool CommandHasExpectedDelta(CombatCommandSpec spec, CombatRuntimeCommandEvidence command) =>
        spec.CommandType switch
        {
            "combat/start_encounter" => command.EncounterDelta.Changed && command.RuntimeEventTypes.Contains("EncounterStarted"),
            "combat/use_ability" or "combat/basic_attack" or "combat/run_ai" => command.EncounterDelta.Changed && command.RuntimeEventTypes.Any(item => item is "AbilityUsed" or "DamageApplied" or "AiActionChosen" or "EncounterWon"),
            "faction/change_reputation" => command.FactionDelta.Changed && command.RuntimeEventTypes.Contains("FactionReputationChanged"),
            "social/open_dialogue" => command.DialogueDelta.Changed && command.RuntimeEventTypes.Contains("DialogueOpened"),
            "social/choose_dialogue" => command.DialogueDelta.Changed && command.RuntimeEventTypes.Contains("DialogueChoiceSelected") && (command.FactionDelta.Changed || command.FlagDelta.Changed || command.InventoryDelta.Changed),
            "work/execute_contract" => command.WorkDelta.Changed && command.InventoryDelta.Changed,
            "gameplay/set_flag" => command.FlagDelta.Changed,
            "theft/open_container" => command.ContainerDelta.Opened && command.RuntimeEventTypes.Contains("ContainerOpened"),
            "theft/take_from_container" => command.ContainerDelta.Changed && command.InventoryDelta.Changed && command.RuntimeEventTypes.Contains("ItemTransferred"),
            _ => false
        };

    private static bool IsScenarioAccepted(
        RulePackCombatFactionSocialWorkTheftBindingAudit bindingAudit,
        RulePackCombatFactionSocialWorkTheftRuntimeEvidence runtimeEvidence,
        IReadOnlyList<RulePackCombatFactionSocialWorkTheftDiagnostic> evidenceDiagnostics) =>
        bindingAudit.Passed &&
        runtimeEvidence.RuntimeAttempted &&
        runtimeEvidence.RuntimeStartSucceeded &&
        runtimeEvidence.RuntimeBoundary.UsedGameRuntimeService &&
        runtimeEvidence.RuntimeBoundary.UsedRuntimeStateFactory &&
        runtimeEvidence.Commands.Count > 0 &&
        runtimeEvidence.Commands.All(item => item.Succeeded) &&
        runtimeEvidence.SaveLoadRoundtripPassed &&
        runtimeEvidence.ScenarioIsolationPassed &&
        evidenceDiagnostics.All(item => item.Severity != "error");

    private static bool ScenarioRuntimePassed(RulePackCombatFactionSocialWorkTheftScenario scenario) =>
        scenario.RuntimeEvidence.RuntimeAttempted &&
        scenario.RuntimeEvidence.RuntimeStartSucceeded &&
        scenario.RuntimeEvidence.RuntimeBoundary.UsedGameRuntimeService &&
        scenario.RuntimeEvidence.RuntimeBoundary.UsedRuntimeStateFactory &&
        scenario.RuntimeEvidence.Commands.Count > 0 &&
        scenario.RuntimeEvidence.Commands.All(item => item.Succeeded) &&
        scenario.RuntimeEvidence.Commands.All(command =>
            command.EncounterDelta.Changed ||
            command.FactionDelta.Changed ||
            command.DialogueDelta.Changed ||
            command.WorkDelta.Changed ||
            command.ContainerDelta.Changed ||
            command.ContainerDelta.Opened ||
            command.InventoryDelta.Changed ||
            command.FlagDelta.Changed);

    private static GamePackageDefinition BuildPackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = "game/rule_pack_combat_family",
                Title = "Rule Pack Combat Family",
                Version = "0.9.0",
                FormatVersion = "0.1",
                StartMapId = "map/test_arena"
            },
            Game = new GameDefinition
            {
                TilePrototypes =
                [
                    new TilePrototypeDefinition { Id = "tile/floor", Name = "Floor", Walkable = true }
                ],
                Maps =
                [
                    new MapDefinition { Id = "map/test_arena", Name = "Test Arena", Width = 4, Height = 4, DefaultTileId = "tile/floor" }
                ],
                Items =
                [
                    new ItemDefinition { Id = "item/work_permit", Name = "Work Permit" },
                    new ItemDefinition { Id = "item/wage_scrip", Name = "Wage Scrip" },
                    new ItemDefinition { Id = "item/guard_badge", Name = "Guard Badge" },
                    new ItemDefinition { Id = "item/stolen_gem", Name = "Stolen Gem" },
                    new ItemDefinition { Id = "item/victory_token", Name = "Victory Token" },
                    new ItemDefinition { Id = "item/contract_note", Name = "Contract Note" }
                ],
                Resources =
                [
                    new ResourceDefinition { Id = "resource/health", Name = "Health", Kind = "health", MinValue = 0, MaxValue = 20, DefaultValue = 10, Tags = ["health"] },
                    new ResourceDefinition { Id = "resource/focus", Name = "Focus", Kind = "abstract", MinValue = 0, MaxValue = 10, DefaultValue = 5 }
                ],
                Abilities =
                [
                    new AbilityDefinition
                    {
                        Id = "ability/guard_strike",
                        Name = "Guard Strike",
                        Kind = "attack",
                        Power = 5,
                        ResourceId = "resource/health",
                        Costs = [new CostDefinition { Kind = "resource", Id = "resource/focus", Amount = 1 }],
                        Tags = ["basic_attack"]
                    },
                    new AbilityDefinition
                    {
                        Id = "ability/raider_cut",
                        Name = "Raider Cut",
                        Kind = "attack",
                        Power = 2,
                        ResourceId = "resource/health",
                        Tags = ["basic_attack"]
                    },
                    new AbilityDefinition
                    {
                        Id = "ability/finisher",
                        Name = "Finisher",
                        Kind = "attack",
                        Power = 8,
                        ResourceId = "resource/health",
                        Costs = [new CostDefinition { Kind = "resource", Id = "resource/focus", Amount = 1 }]
                    }
                ],
                Encounters =
                [
                    new EncounterDefinition
                    {
                        Id = "encounter/roadside_raider",
                        Name = "Roadside Raider",
                        Kind = "combat",
                        DefaultSeed = 9042,
                        Metadata = { ["default_attack_ability_id"] = "ability/guard_strike" },
                        Participants =
                        [
                            Participant("participant/player_guard", "Player Guard", "player", "ability/guard_strike", health: 14, focus: 5),
                            Participant("participant/raider", "Raider", "enemy", "ability/raider_cut", health: 9, focus: 3)
                        ],
                        Rewards = [new OutputDefinition { Kind = "item", Id = "item/victory_token", Amount = 1 }]
                    },
                    new EncounterDefinition
                    {
                        Id = "encounter/quick_resolution",
                        Name = "Quick Resolution",
                        Kind = "combat",
                        DefaultSeed = 9043,
                        Participants =
                        [
                            Participant("participant/player_guard", "Player Guard", "player", "ability/finisher", health: 14, focus: 5),
                            Participant("participant/raider", "Raider", "enemy", "ability/raider_cut", health: 6, focus: 3)
                        ],
                        Rewards = [new OutputDefinition { Kind = "item", Id = "item/victory_token", Amount = 1 }]
                    }
                ],
                Factions =
                [
                    new FactionDefinition { Id = "faction/settlement_watch", Name = "Settlement Watch", DefaultReputation = 0, MinReputation = -100, MaxReputation = 100 }
                ],
                Dialogues =
                [
                    new DialogueDefinition
                    {
                        Id = "dialogue/watch_captain",
                        Title = "Watch Captain",
                        StartNodeId = "node/start",
                        Nodes =
                        [
                            new DialogueNodeDefinition
                            {
                                Id = "node/start",
                                SpeakerId = "npc/watch_captain",
                                Text = "Contract available.",
                                Choices =
                                [
                                    new DialogueChoiceDefinition
                                    {
                                        Id = "choice/accept_contract",
                                        Text = "Accept contract.",
                                        CloseDialogue = true,
                                        Rewards =
                                        [
                                            new OutputDefinition { Kind = "flag", Id = "flag/work_contract_accepted", Amount = 1, Mode = "accepted" },
                                            new OutputDefinition { Kind = "reputation", Id = "faction/settlement_watch", Amount = 5 },
                                            new OutputDefinition { Kind = "item", Id = "item/contract_note", Amount = 1 }
                                        ]
                                    }
                                ]
                            }
                        ]
                    }
                ],
                Transactions =
                [
                    new TransactionDefinition
                    {
                        Id = "transaction/work_contract_reward",
                        Name = "Work Contract Reward",
                        Kind = "work_contract",
                        Requirements = [new RequirementDefinition { Kind = "has_item", Id = "item/work_permit", Amount = 1 }],
                        Outputs = [new OutputDefinition { Kind = "item", Id = "item/wage_scrip", Amount = 3 }]
                    }
                ],
                Interactions =
                [
                    new InteractionDefinition { Id = "interaction/work_contract_reward", Kind = "trade", Metadata = { ["transaction_id"] = "transaction/work_contract_reward" } },
                    new InteractionDefinition { Id = "interaction/watch_captain_talk", Kind = "talk", Metadata = { ["dialogue_id"] = "dialogue/watch_captain" } },
                    new InteractionDefinition { Id = "interaction/cache_container", Kind = "open_container", Metadata = { ["container_id"] = "inventory/merchant_cache" } }
                ],
                Inventories =
                [
                    new InventoryDefinition
                    {
                        Id = "inventory/player",
                        OwnerKind = "player",
                        OwnerId = "player",
                        Stacks = [new ItemStackDefinition { ItemId = "item/work_permit", Amount = 1 }]
                    },
                    new InventoryDefinition
                    {
                        Id = "inventory/merchant_cache",
                        OwnerKind = "container",
                        OwnerId = "entity/merchant_cache",
                        Tags = ["container"],
                        Metadata = { ["container"] = "true" },
                        Stacks = [new ItemStackDefinition { ItemId = "item/stolen_gem", Amount = 2 }]
                    }
                ]
            }
        };
    }

    private static EncounterParticipantDefinition Participant(string id, string name, string team, string abilityId, double health, double focus) => new()
    {
        Id = id,
        Name = name,
        Team = team,
        Kind = team,
        Abilities = [abilityId],
        Resources =
        [
            new OutputDefinition { Kind = "resource", Id = "resource/health", Amount = health },
            new OutputDefinition { Kind = "resource", Id = "resource/focus", Amount = focus }
        ]
    };

    private static CombatFamilyDeclarations BuildDeclarations() => new()
    {
        RulePackId = "rule_pack/combat_faction_social_work_theft",
        RulesVersion = "1",
        Encounters =
        [
            new CombatEncounterDeclaration
            {
                DeclarationId = "decl/combat/turn_based_encounter",
                FamilyId = "combat",
                PackageEncounterId = "encounter/roadside_raider",
                PlayerParticipantId = "participant/player_guard",
                EnemyParticipantId = "participant/raider",
                PlayerAbilityId = "ability/guard_strike",
                EnemyAbilityId = "ability/raider_cut",
                HealthResourceId = "resource/health",
                FocusResourceId = "resource/focus"
            },
            new CombatEncounterDeclaration
            {
                DeclarationId = "decl/combat/resolution_reward",
                FamilyId = "combat",
                PackageEncounterId = "encounter/quick_resolution",
                PlayerParticipantId = "participant/player_guard",
                EnemyParticipantId = "participant/raider",
                PlayerAbilityId = "ability/finisher",
                EnemyAbilityId = "ability/raider_cut",
                HealthResourceId = "resource/health",
                FocusResourceId = "resource/focus"
            }
        ],
        Factions =
        [
            new FactionReputationDeclaration { DeclarationId = "decl/faction/watch_reputation_gain", FamilyId = "faction", PackageFactionId = "faction/settlement_watch", Amount = 12, ExpectedAfter = 12 },
            new FactionReputationDeclaration { DeclarationId = "decl/faction/theft_penalty", FamilyId = "faction", PackageFactionId = "faction/settlement_watch", Amount = -20, ExpectedAfter = -20 }
        ],
        Dialogues =
        [
            new SocialDialogueDeclaration
            {
                DeclarationId = "decl/social/accept_watch_contract",
                FamilyId = "social",
                PackageDialogueId = "dialogue/watch_captain",
                PackageNodeId = "node/start",
                PackageChoiceId = "choice/accept_contract",
                FactionId = "faction/settlement_watch",
                FlagId = "flag/work_contract_accepted"
            }
        ],
        WorkContracts =
        [
            new WorkContractDeclaration
            {
                DeclarationId = "decl/work/contract_reward",
                FamilyId = "work",
                PackageInteractionId = "interaction/work_contract_reward",
                PackageTransactionId = "transaction/work_contract_reward",
                RequiredItemId = "item/work_permit",
                RewardItemId = "item/wage_scrip",
                CompletionFlagId = "flag/work_contract_completed",
                FactionId = "faction/settlement_watch"
            }
        ],
        TheftConsequences =
        [
            new TheftConsequenceDeclaration
            {
                DeclarationId = "decl/theft/cache_gem_penalty",
                FamilyId = "theft",
                ContainerInventoryId = "inventory/merchant_cache",
                ItemId = "item/stolen_gem",
                Amount = 1,
                TheftFlagId = "flag/theft_reported",
                FactionId = "faction/settlement_watch",
                ReputationPenalty = -20
            }
        ]
    };

    private static IReadOnlyList<CombatFamilyScenarioSpec> BuildValidSpecs() =>
    [
        new()
        {
            ScenarioId = "combat_turn_based_encounter",
            Seed = "s079-combat-turn",
            FamilyIds = ["combat"],
            SourceDeclarationIds = ["decl/combat/turn_based_encounter"],
            PackageRuntimeIds = ["encounter/roadside_raider", "participant/player_guard", "participant/raider", "ability/guard_strike", "ability/raider_cut", "resource/health", "resource/focus"],
            ScenarioStateMarker = "combat_turn_based_encounter",
            Commands =
            [
                CombatCommandSpec.StartEncounter("cmd/start_roadside_raider", "decl/combat/turn_based_encounter", "encounter/roadside_raider"),
                CombatCommandSpec.UseAbility("cmd/player_guard_strike", "decl/combat/turn_based_encounter", "ability/guard_strike", "participant/player_guard", "participant/raider"),
                CombatCommandSpec.RunAi("cmd/raider_ai_turn", "decl/combat/turn_based_encounter")
            ]
        },
        new()
        {
            ScenarioId = "combat_resolution_reward",
            Seed = "s079-combat-resolution",
            FamilyIds = ["combat"],
            SourceDeclarationIds = ["decl/combat/resolution_reward"],
            PackageRuntimeIds = ["encounter/quick_resolution", "participant/player_guard", "participant/raider", "ability/finisher", "item/victory_token"],
            ScenarioStateMarker = "combat_resolution_reward",
            Commands =
            [
                CombatCommandSpec.StartEncounter("cmd/start_quick_resolution", "decl/combat/resolution_reward", "encounter/quick_resolution"),
                CombatCommandSpec.UseAbility("cmd/player_finisher", "decl/combat/resolution_reward", "ability/finisher", "participant/player_guard", "participant/raider")
            ]
        },
        new()
        {
            ScenarioId = "faction_reputation_change",
            Seed = "s080-faction",
            FamilyIds = ["faction"],
            SourceDeclarationIds = ["decl/faction/watch_reputation_gain"],
            PackageRuntimeIds = ["faction/settlement_watch"],
            ScenarioStateMarker = "faction_reputation_change",
            Commands = [CombatCommandSpec.ChangeReputation("cmd/watch_reputation_gain", "decl/faction/watch_reputation_gain", "faction/settlement_watch", 12)]
        },
        new()
        {
            ScenarioId = "social_dialogue_reputation_consequence",
            Seed = "s080-social",
            FamilyIds = ["social"],
            SourceDeclarationIds = ["decl/social/accept_watch_contract"],
            PackageRuntimeIds = ["dialogue/watch_captain", "node/start", "choice/accept_contract", "faction/settlement_watch", "flag/work_contract_accepted", "item/contract_note"],
            ScenarioStateMarker = "social_dialogue_reputation_consequence",
            Commands =
            [
                CombatCommandSpec.OpenDialogue("cmd/open_watch_dialogue", "decl/social/accept_watch_contract", "dialogue/watch_captain"),
                CombatCommandSpec.ChooseDialogue("cmd/accept_watch_contract", "decl/social/accept_watch_contract", "choice/accept_contract")
            ]
        },
        new()
        {
            ScenarioId = "work_contract_reward",
            Seed = "s081-work",
            FamilyIds = ["work"],
            SourceDeclarationIds = ["decl/work/contract_reward"],
            PackageRuntimeIds = ["interaction/work_contract_reward", "transaction/work_contract_reward", "item/work_permit", "item/wage_scrip", "flag/work_contract_completed"],
            ScenarioStateMarker = "work_contract_reward",
            Commands =
            [
                CombatCommandSpec.ExecuteWork("cmd/work_contract_reward", "decl/work/contract_reward", "interaction/work_contract_reward"),
                CombatCommandSpec.SetFlag("cmd/work_contract_completed", "decl/work/contract_reward", "flag/work_contract_completed", "completed")
            ]
        },
        new()
        {
            ScenarioId = "theft_container_reputation_consequence",
            Seed = "s081-theft",
            FamilyIds = ["theft"],
            SourceDeclarationIds = ["decl/theft/cache_gem_penalty"],
            PackageRuntimeIds = ["inventory/merchant_cache", "item/stolen_gem", "flag/theft_reported", "faction/settlement_watch"],
            ScenarioStateMarker = "theft_container_reputation_consequence",
            Commands =
            [
                CombatCommandSpec.OpenContainer("cmd/open_cache", "decl/theft/cache_gem_penalty", "inventory/merchant_cache"),
                CombatCommandSpec.TakeFromContainer("cmd/take_stolen_gem", "decl/theft/cache_gem_penalty", "inventory/merchant_cache", "item/stolen_gem", 1),
                CombatCommandSpec.SetFlag("cmd/theft_reported", "decl/theft/cache_gem_penalty", "flag/theft_reported", "true"),
                CombatCommandSpec.ChangeReputation("cmd/theft_reputation_penalty", "decl/theft/cache_gem_penalty", "faction/settlement_watch", -20)
            ]
        },
        new()
        {
            ScenarioId = "combined_combat_social_work_theft_loop",
            Seed = "s082-combined",
            FamilyIds = ["social", "work", "combat", "theft", "faction"],
            SourceDeclarationIds = ["decl/social/accept_watch_contract", "decl/work/contract_reward", "decl/combat/resolution_reward", "decl/theft/cache_gem_penalty", "decl/faction/theft_penalty"],
            PackageRuntimeIds = ["dialogue/watch_captain", "choice/accept_contract", "interaction/work_contract_reward", "encounter/quick_resolution", "ability/finisher", "item/victory_token", "inventory/merchant_cache", "item/stolen_gem", "flag/theft_reported", "faction/settlement_watch"],
            ScenarioStateMarker = "combined_combat_social_work_theft_loop",
            Commands =
            [
                CombatCommandSpec.OpenDialogue("cmd/combined_open_dialogue", "decl/social/accept_watch_contract", "dialogue/watch_captain"),
                CombatCommandSpec.ChooseDialogue("cmd/combined_accept_contract", "decl/social/accept_watch_contract", "choice/accept_contract"),
                CombatCommandSpec.ExecuteWork("cmd/combined_work_reward", "decl/work/contract_reward", "interaction/work_contract_reward"),
                CombatCommandSpec.StartEncounter("cmd/combined_start_encounter", "decl/combat/resolution_reward", "encounter/quick_resolution"),
                CombatCommandSpec.UseAbility("cmd/combined_finisher", "decl/combat/resolution_reward", "ability/finisher", "participant/player_guard", "participant/raider"),
                CombatCommandSpec.SetFlag("cmd/combined_work_complete", "decl/work/contract_reward", "flag/work_contract_completed", "completed"),
                CombatCommandSpec.OpenContainer("cmd/combined_open_cache", "decl/theft/cache_gem_penalty", "inventory/merchant_cache"),
                CombatCommandSpec.TakeFromContainer("cmd/combined_take_gem", "decl/theft/cache_gem_penalty", "inventory/merchant_cache", "item/stolen_gem", 1),
                CombatCommandSpec.SetFlag("cmd/combined_theft_flag", "decl/theft/cache_gem_penalty", "flag/theft_reported", "true"),
                CombatCommandSpec.ChangeReputation("cmd/combined_theft_penalty", "decl/faction/theft_penalty", "faction/settlement_watch", -20)
            ]
        }
    ];

    private static IReadOnlyList<CombatFamilyScenarioSpec> BuildInvalidSpecs() =>
    [
        Invalid("invalid_missing_encounter_or_participant_ref", "missing_encounter_or_participant_ref", BuildValidSpecs()[0]),
        Invalid("invalid_missing_ability_or_resource_ref", "missing_ability_or_resource_ref", BuildValidSpecs()[0]),
        Invalid("invalid_combat_wrong_turn_or_target", "combat_wrong_turn_or_target", BuildValidSpecs()[0]),
        Invalid("invalid_missing_faction_ref", "missing_faction_ref", BuildValidSpecs()[2]),
        Invalid("invalid_dialogue_or_choice_ref", "dialogue_or_choice_ref", BuildValidSpecs()[3]),
        Invalid("invalid_work_requirement_unmet", "work_requirement_unmet", BuildValidSpecs()[4]),
        Invalid("invalid_theft_container_or_item_ref", "theft_container_or_item_ref", BuildValidSpecs()[5]),
        Invalid("invalid_theft_nonpositive_amount", "theft_nonpositive_amount", BuildValidSpecs()[5]),
        Invalid("invalid_command_not_covered_by_declaration", "command_not_covered_by_declaration", BuildValidSpecs()[2]),
        Invalid("invalid_fake_runtime_success", "fake_runtime_success", BuildValidSpecs()[6]),
        Invalid("invalid_save_load_mismatch", "save_load_mismatch", BuildValidSpecs()[6]),
        Invalid("invalid_cross_scenario_state_leakage", "cross_scenario_state_leakage", BuildValidSpecs()[6])
    ];

    private static CombatFamilyScenarioSpec Invalid(string scenarioId, string kind, CombatFamilyScenarioSpec source) => source with
    {
        ScenarioId = scenarioId,
        Seed = "s083-" + scenarioId,
        InvalidKind = kind,
        ScenarioStateMarker = scenarioId
    };

    private static void ApplyInvalidMutation(GamePackageDefinition package, CombatFamilyDeclarations declarations, ref CombatFamilyScenarioSpec spec)
    {
        switch (spec.InvalidKind)
        {
            case "missing_encounter_or_participant_ref":
                package.Game.Encounters.RemoveAll(item => item.Id == "encounter/roadside_raider");
                break;
            case "missing_ability_or_resource_ref":
                package.Game.Abilities.RemoveAll(item => item.Id == "ability/guard_strike");
                break;
            case "combat_wrong_turn_or_target":
                spec = spec with
                {
                    Commands =
                    [
                        CombatCommandSpec.StartEncounter("cmd/start_roadside_raider", "decl/combat/turn_based_encounter", "encounter/roadside_raider"),
                        CombatCommandSpec.UseAbility("cmd/enemy_acts_on_player_turn", "decl/combat/turn_based_encounter", "ability/raider_cut", "participant/raider", "participant/player_guard")
                    ]
                };
                break;
            case "missing_faction_ref":
                package.Game.Factions.RemoveAll(item => item.Id == "faction/settlement_watch");
                break;
            case "dialogue_or_choice_ref":
                package.Game.Dialogues.Single(item => item.Id == "dialogue/watch_captain").Nodes[0].Choices.Clear();
                break;
            case "work_requirement_unmet":
                package.Game.Inventories.Single(item => item.Id == "inventory/player").Stacks.Clear();
                break;
            case "theft_container_or_item_ref":
                package.Game.Inventories.Single(item => item.Id == "inventory/merchant_cache").Stacks.Clear();
                break;
            case "theft_nonpositive_amount":
                declarations.TheftConsequences = declarations.TheftConsequences.Select(item => item with { Amount = 0 }).ToList();
                spec = spec with
                {
                    Commands =
                    [
                        CombatCommandSpec.OpenContainer("cmd/open_cache", "decl/theft/cache_gem_penalty", "inventory/merchant_cache"),
                        CombatCommandSpec.TakeFromContainer("cmd/take_zero_gem", "decl/theft/cache_gem_penalty", "inventory/merchant_cache", "item/stolen_gem", 0)
                    ]
                };
                break;
            case "command_not_covered_by_declaration":
                spec = spec with
                {
                    Commands = [CombatCommandSpec.SetFlag("cmd/undeclared_flag", "decl/not/selected", "flag/undeclared", "true")]
                };
                break;
            case "save_load_mismatch":
                spec = spec with { ScenarioStateMarker = "invalid_save_load_mismatch" };
                break;
            case "cross_scenario_state_leakage":
                spec = spec with { ScenarioStateMarker = "leak:previous_scenario" };
                break;
        }
    }

    private static RulePackCombatFactionSocialWorkTheftRuntimeEvidence FakeRuntimeSuccess(CombatFamilyScenarioSpec spec) => new()
    {
        RuntimeAttempted = true,
        RuntimeStartSucceeded = true,
        RuntimeStateOwner = "GameRuntimeState",
        PackageId = "game/rule_pack_combat_family",
        RuntimeStateHash = "copied",
        RestoredRuntimeStateHash = "copied",
        SaveLoadRoundtripPassed = true,
        ScenarioIsolationPassed = true,
        StateEvidence = new Dictionary<string, string>(StringComparer.Ordinal) { ["scenarioId"] = spec.ScenarioId },
        RestoredStateEvidence = new Dictionary<string, string>(StringComparer.Ordinal) { ["scenarioId"] = spec.ScenarioId },
        Diagnostics = [Diagnostic("info", "combat_family.fake_runtime_success_fixture", spec.ScenarioId, "Fixture copies ids and success booleans without command evidence.")]
    };

    private static string RenderReport(RulePackCombatFactionSocialWorkTheftReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Rule-Pack Combat, Faction, Social, Work And Theft Report");
        builder.AppendLine();
        builder.AppendLine($"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- Manual gate: {report.ManualGate}");
        builder.AppendLine($"- Goal 008 gate recorded: {report.Goal008GateRecorded.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- Completed slices: {string.Join(", ", report.CompletedSlices)}");
        builder.AppendLine($"- Valid scenarios: {report.ValidScenarioCount}");
        builder.AppendLine($"- Invalid scenarios: {report.InvalidScenarioCount}");
        builder.AppendLine($"- Runtime execution: {report.CombatFactionSocialWorkTheftRuntimeExecutionPassed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- Save/load: {report.SaveLoadRoundtripPassed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- Deterministic replay: {report.DeterministicReplayPassed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- Isolation: {report.ScenarioIsolationPassed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- Fake success rejected: {report.FakeRuntimeSuccessRejected.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- Hash: {report.DeterministicHash}");
        builder.AppendLine();
        builder.AppendLine("## Bounded Semantics");
        builder.AppendLine();
        builder.AppendLine($"- Work: {report.BoundedWorkMeaning}");
        builder.AppendLine($"- Theft: {report.BoundedTheftMeaning}");
        builder.AppendLine();
        builder.AppendLine("## Remaining Primitive Limits");
        foreach (var limit in report.RemainingPrimitiveLimits)
        {
            builder.AppendLine($"- {limit}");
        }
        builder.AppendLine();
        builder.AppendLine("## Scenarios");
        foreach (var scenario in report.Scenarios)
        {
            builder.AppendLine($"- {scenario.ScenarioId}: expected={scenario.ExpectedValid.ToString().ToLowerInvariant()}, actual={scenario.ActualValid.ToString().ToLowerInvariant()}, commands={scenario.RuntimeEvidence.Commands.Count}");
        }

        return builder.ToString();
    }

    private static string RenderVerification(RulePackCombatFactionSocialWorkTheftReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Rule-Pack Combat, Faction, Social, Work And Theft Verification");
        builder.AppendLine();
        builder.AppendLine("Stop at this manual gate:");
        builder.AppendLine();
        builder.AppendLine("```text");
        builder.AppendLine(ManualGate);
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("- Do not mark this gate passed in this run.");
        builder.AppendLine("- Do not create post-goal work.");
        builder.AppendLine($"- Report accepted by automated evidence: {report.Accepted.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- Report hash: {report.DeterministicHash}");
        return builder.ToString();
    }

    private static IReadOnlyList<RulePackCombatFactionSocialWorkTheftDiagnostic> SortDiagnostics(IEnumerable<RulePackCombatFactionSocialWorkTheftDiagnostic> diagnostics) =>
        diagnostics.OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static RulePackCombatFactionSocialWorkTheftDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static bool IsContainer(InventoryDefinition inventory) =>
        string.Equals(inventory.OwnerKind, "container", StringComparison.OrdinalIgnoreCase) ||
        inventory.Tags.Any(tag => string.Equals(tag, "container", StringComparison.OrdinalIgnoreCase)) ||
        inventory.Metadata.TryGetValue("container", out var value) && value.Equals("true", StringComparison.OrdinalIgnoreCase);

    private static GamePackageDefinition ClonePackage(GamePackageDefinition package) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(JsonSerializer.Serialize(package, JsonOptions), JsonOptions)!;

    private static CombatFamilyDeclarations CloneDeclarations(CombatFamilyDeclarations declarations) =>
        JsonSerializer.Deserialize<CombatFamilyDeclarations>(JsonSerializer.Serialize(declarations, JsonOptions), JsonOptions)!;

    private static bool DictionaryEquals(IReadOnlyDictionary<string, string> left, IReadOnlyDictionary<string, string> right) =>
        left.Count == right.Count && left.All(pair => right.TryGetValue(pair.Key, out var value) && value == pair.Value);

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
            throw new InvalidOperationException("Rule-pack combat/faction/social/work/theft output path must stay under the project root.");
        }
    }

    private sealed record DeclarationIndex(
        string DeclarationId,
        string FamilyId,
        string Kind,
        string PrimaryId,
        string SecondaryId,
        string TertiaryId,
        string QuaternaryId,
        string FactionId,
        string DialogueOrInteractionId,
        string ChoiceOrTransactionOrItemId,
        object Source);

    public sealed class UnavailableRulePackCombatFactionSocialWorkTheftRuntimeAdapter : IRulePackCombatFactionSocialWorkTheftRuntimeAdapter
    {
        public RulePackCombatFactionSocialWorkTheftRuntimeEvidence Run(RulePackCombatFactionSocialWorkTheftRuntimeRequest request) => new()
        {
            RuntimeAttempted = false,
            RuntimeStartSucceeded = false,
            RuntimeStateOwner = "GameRuntimeState",
            PackageId = request.Package.Manifest.PackageId,
            RuntimeBoundary = new CombatRuntimeBoundaryEvidence
            {
                AdapterId = nameof(UnavailableRulePackCombatFactionSocialWorkTheftRuntimeAdapter),
                RuntimeServiceType = string.Empty,
                StateFactoryType = string.Empty,
                SerializerType = string.Empty,
                SnapshotStoreType = string.Empty,
                UsedGameRuntimeService = false,
                UsedRuntimeStateFactory = false
            },
            Diagnostics =
            [
                Diagnostic("error", "combat_family.runtime_adapter_unavailable", request.ScenarioId, "Rule-pack combat/faction/social/work/theft acceptance requires an injected real runtime adapter.")
            ]
        };
    }
}

public interface IRulePackCombatFactionSocialWorkTheftRuntimeAdapter
{
    RulePackCombatFactionSocialWorkTheftRuntimeEvidence Run(RulePackCombatFactionSocialWorkTheftRuntimeRequest request);
}

public sealed record RulePackCombatFactionSocialWorkTheftAcceptanceResult
{
    public RulePackCombatFactionSocialWorkTheftReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record RulePackCombatFactionSocialWorkTheftWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record RulePackCombatFactionSocialWorkTheftReport
{
    public string SchemaVersion { get; init; } = "1";
    public string DeterministicHash { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public bool Goal008GateRecorded { get; init; }
    public IReadOnlyList<string> CompletedSlices { get; init; } = Array.Empty<string>();
    public int ScenarioCount { get; init; }
    public int ValidScenarioCount { get; init; }
    public int InvalidScenarioCount { get; init; }
    public bool ValidScenariosAccepted { get; init; }
    public bool InvalidScenariosRejected { get; init; }
    public bool PackageRuleBindingAuditPassed { get; init; }
    public bool CombatFactionSocialWorkTheftRuntimeExecutionPassed { get; init; }
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool DeterministicReplayPassed { get; init; }
    public bool ScenarioIsolationPassed { get; init; }
    public bool FakeRuntimeSuccessRejected { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public RulePackCombatFactionSocialWorkTheftExternalExecutionFlags ExternalExecution { get; init; } = new();
    public string BoundedWorkMeaning { get; init; } = string.Empty;
    public string BoundedTheftMeaning { get; init; } = string.Empty;
    public CombatFamilyDeclarations Declarations { get; init; } = new();
    public IReadOnlyList<RulePackCombatFactionSocialWorkTheftScenario> Scenarios { get; init; } = Array.Empty<RulePackCombatFactionSocialWorkTheftScenario>();
    public IReadOnlyList<RulePackCombatFactionSocialWorkTheftDiagnostic> Diagnostics { get; init; } = Array.Empty<RulePackCombatFactionSocialWorkTheftDiagnostic>();
    public IReadOnlyList<string> RemainingPrimitiveLimits { get; init; } = Array.Empty<string>();
}

public sealed record CombatFamilyDeclarations
{
    public string RulePackId { get; init; } = string.Empty;
    public string RulesVersion { get; init; } = string.Empty;
    public IReadOnlyList<CombatEncounterDeclaration> Encounters { get; init; } = Array.Empty<CombatEncounterDeclaration>();
    public IReadOnlyList<FactionReputationDeclaration> Factions { get; init; } = Array.Empty<FactionReputationDeclaration>();
    public IReadOnlyList<SocialDialogueDeclaration> Dialogues { get; init; } = Array.Empty<SocialDialogueDeclaration>();
    public IReadOnlyList<WorkContractDeclaration> WorkContracts { get; init; } = Array.Empty<WorkContractDeclaration>();
    public IReadOnlyList<TheftConsequenceDeclaration> TheftConsequences { get; set; } = Array.Empty<TheftConsequenceDeclaration>();
}

public sealed record CombatEncounterDeclaration
{
    public string DeclarationId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string PackageEncounterId { get; init; } = string.Empty;
    public string PlayerParticipantId { get; init; } = string.Empty;
    public string EnemyParticipantId { get; init; } = string.Empty;
    public string PlayerAbilityId { get; init; } = string.Empty;
    public string EnemyAbilityId { get; init; } = string.Empty;
    public string HealthResourceId { get; init; } = string.Empty;
    public string FocusResourceId { get; init; } = string.Empty;
}

public sealed record FactionReputationDeclaration
{
    public string DeclarationId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string PackageFactionId { get; init; } = string.Empty;
    public double Amount { get; init; }
    public double ExpectedAfter { get; init; }
}

public sealed record SocialDialogueDeclaration
{
    public string DeclarationId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string PackageDialogueId { get; init; } = string.Empty;
    public string PackageNodeId { get; init; } = string.Empty;
    public string PackageChoiceId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public string FlagId { get; init; } = string.Empty;
}

public sealed record WorkContractDeclaration
{
    public string DeclarationId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string PackageInteractionId { get; init; } = string.Empty;
    public string PackageTransactionId { get; init; } = string.Empty;
    public string RequiredItemId { get; init; } = string.Empty;
    public string RewardItemId { get; init; } = string.Empty;
    public string CompletionFlagId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
}

public sealed record TheftConsequenceDeclaration
{
    public string DeclarationId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string ContainerInventoryId { get; init; } = string.Empty;
    public string ItemId { get; init; } = string.Empty;
    public double Amount { get; init; }
    public string TheftFlagId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public double ReputationPenalty { get; init; }
}

public sealed record RulePackCombatFactionSocialWorkTheftScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string InvalidKind { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedFamilyIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SourceDeclarationIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PackageRuntimeIds { get; init; } = Array.Empty<string>();
    public RulePackCombatFactionSocialWorkTheftBindingAudit PackageBindingAudit { get; init; } = new();
    public RulePackCombatFactionSocialWorkTheftRuntimeEvidence RuntimeEvidence { get; init; } = new();
    public IReadOnlyList<RulePackCombatFactionSocialWorkTheftDiagnostic> Diagnostics { get; init; } = Array.Empty<RulePackCombatFactionSocialWorkTheftDiagnostic>();
}

public sealed record RulePackCombatFactionSocialWorkTheftBindingAudit
{
    public bool Passed { get; init; }
    public string PackageId { get; init; } = string.Empty;
    public IReadOnlyList<string> AuditedDeclarationIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> AuditedPackageRuntimeIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<RulePackCombatFactionSocialWorkTheftDiagnostic> Diagnostics { get; init; } = Array.Empty<RulePackCombatFactionSocialWorkTheftDiagnostic>();
}

public sealed record RulePackCombatFactionSocialWorkTheftRuntimeRequest
{
    public string ScenarioId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public GamePackageDefinition Package { get; init; } = new();
    public CombatFamilyDeclarations Declarations { get; init; } = new();
    public IReadOnlyList<CombatCommandSpec> Commands { get; init; } = Array.Empty<CombatCommandSpec>();
    public string ExpectedScenarioStateMarker { get; init; } = string.Empty;
}

public sealed record CombatCommandSpec
{
    public string CommandId { get; init; } = string.Empty;
    public string SourceDeclarationId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public double Amount { get; init; }
    public string Value { get; init; } = string.Empty;
    public string InventoryId { get; init; } = "inventory/player";

    public static CombatCommandSpec StartEncounter(string commandId, string declarationId, string encounterId) =>
        new() { CommandId = commandId, SourceDeclarationId = declarationId, CommandType = "combat/start_encounter", TargetId = encounterId };

    public static CombatCommandSpec UseAbility(string commandId, string declarationId, string abilityId, string sourceId, string targetId) =>
        new() { CommandId = commandId, SourceDeclarationId = declarationId, CommandType = "combat/use_ability", TargetId = abilityId, ActorId = sourceId, SecondaryTargetId = targetId };

    public static CombatCommandSpec RunAi(string commandId, string declarationId) =>
        new() { CommandId = commandId, SourceDeclarationId = declarationId, CommandType = "combat/run_ai" };

    public static CombatCommandSpec ChangeReputation(string commandId, string declarationId, string factionId, double amount) =>
        new() { CommandId = commandId, SourceDeclarationId = declarationId, CommandType = "faction/change_reputation", TargetId = factionId, Amount = amount };

    public static CombatCommandSpec OpenDialogue(string commandId, string declarationId, string dialogueId) =>
        new() { CommandId = commandId, SourceDeclarationId = declarationId, CommandType = "social/open_dialogue", TargetId = dialogueId };

    public static CombatCommandSpec ChooseDialogue(string commandId, string declarationId, string choiceId) =>
        new() { CommandId = commandId, SourceDeclarationId = declarationId, CommandType = "social/choose_dialogue", TargetId = choiceId };

    public static CombatCommandSpec ExecuteWork(string commandId, string declarationId, string interactionId) =>
        new() { CommandId = commandId, SourceDeclarationId = declarationId, CommandType = "work/execute_contract", TargetId = interactionId };

    public static CombatCommandSpec OpenContainer(string commandId, string declarationId, string containerId) =>
        new() { CommandId = commandId, SourceDeclarationId = declarationId, CommandType = "theft/open_container", TargetId = containerId };

    public static CombatCommandSpec TakeFromContainer(string commandId, string declarationId, string containerId, string itemId, double amount) =>
        new() { CommandId = commandId, SourceDeclarationId = declarationId, CommandType = "theft/take_from_container", TargetId = containerId, SecondaryTargetId = itemId, Amount = amount };

    public static CombatCommandSpec SetFlag(string commandId, string declarationId, string flagId, string value) =>
        new() { CommandId = commandId, SourceDeclarationId = declarationId, CommandType = "gameplay/set_flag", TargetId = flagId, Value = value };
}

public sealed record RulePackCombatFactionSocialWorkTheftRuntimeEvidence
{
    public bool RuntimeAttempted { get; init; }
    public bool RuntimeStartSucceeded { get; init; }
    public string RuntimeStateOwner { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public CombatRuntimeBoundaryEvidence RuntimeBoundary { get; init; } = new();
    public string RuntimeEvidenceHash { get; init; } = string.Empty;
    public IReadOnlyList<CombatRuntimeCommandEvidence> Commands { get; init; } = Array.Empty<CombatRuntimeCommandEvidence>();
    public CombatEncounterEvidence EncounterBefore { get; init; } = new();
    public CombatEncounterEvidence EncounterAfter { get; init; } = new();
    public IReadOnlyDictionary<string, string> FactionReputationBefore { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> FactionReputationAfter { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public CombatDialogueEvidence DialogueBefore { get; init; } = new();
    public CombatDialogueEvidence DialogueAfter { get; init; } = new();
    public CombatWorkEvidence WorkEvidence { get; init; } = new();
    public CombatTheftEvidence TheftEvidence { get; init; } = new();
    public string RuntimeStateHash { get; init; } = string.Empty;
    public string RestoredRuntimeStateHash { get; init; } = string.Empty;
    public bool SaveLoadRoundtripPassed { get; init; }
    public CombatSaveLoadEvidence SaveLoadEvidence { get; init; } = new();
    public bool ScenarioIsolationPassed { get; init; }
    public IReadOnlyDictionary<string, string> StateEvidence { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> RestoredStateEvidence { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<RulePackCombatFactionSocialWorkTheftDiagnostic> Diagnostics { get; init; } = Array.Empty<RulePackCombatFactionSocialWorkTheftDiagnostic>();
}

public sealed record CombatRuntimeCommandEvidence
{
    public string CommandId { get; init; } = string.Empty;
    public string SourceDeclarationId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public bool Succeeded { get; init; }
    public string DiagnosticCode { get; init; } = string.Empty;
    public string DiagnosticMessage { get; init; } = string.Empty;
    public IReadOnlyList<string> RuntimeEventTypes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RuntimeDiagnosticCodes { get; init; } = Array.Empty<string>();
    public CombatEncounterDelta EncounterDelta { get; init; } = new();
    public CombatFactionDelta FactionDelta { get; init; } = new();
    public CombatDialogueDelta DialogueDelta { get; init; } = new();
    public CombatWorkDelta WorkDelta { get; init; } = new();
    public CombatContainerDelta ContainerDelta { get; init; } = new();
    public CombatInventoryDelta InventoryDelta { get; init; } = new();
    public CombatFlagDelta FlagDelta { get; init; } = new();
}

public sealed record CombatRuntimeBoundaryEvidence
{
    public string AdapterId { get; init; } = string.Empty;
    public string RuntimeServiceType { get; init; } = string.Empty;
    public string StateFactoryType { get; init; } = string.Empty;
    public string SerializerType { get; init; } = string.Empty;
    public string SnapshotStoreType { get; init; } = string.Empty;
    public bool UsedGameRuntimeService { get; init; }
    public bool UsedRuntimeStateFactory { get; init; }
    public bool UsedEncounterRuntimeService { get; init; }
    public bool UsedEncounterAiService { get; init; }
    public bool UsedFactionRuntimeService { get; init; }
    public bool UsedDialogueRuntimeService { get; init; }
    public bool UsedInteractionRuntimeService { get; init; }
    public bool UsedContainerRuntimeService { get; init; }
}

public sealed record CombatSaveLoadEvidence
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

public sealed record CombatEncounterEvidence
{
    public string EncounterId { get; init; } = string.Empty;
    public bool Active { get; init; }
    public bool Resolved => !Active && !string.IsNullOrWhiteSpace(EncounterId);
    public int Round { get; init; }
    public int TurnIndex { get; init; }
    public IReadOnlyDictionary<string, string> Participants { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<string> ActionHistory { get; init; } = Array.Empty<string>();
}

public sealed record CombatDialogueEvidence
{
    public string DialogueId { get; init; } = string.Empty;
    public string CurrentNodeId { get; init; } = string.Empty;
    public bool Open { get; init; }
    public IReadOnlyList<string> History { get; init; } = Array.Empty<string>();
}

public sealed record CombatWorkEvidence
{
    public string ContractInteractionId { get; init; } = string.Empty;
    public string RewardItemId { get; init; } = string.Empty;
    public double RewardAmountAfter { get; init; }
    public string CompletionFlagId { get; init; } = string.Empty;
    public string CompletionFlagAfter { get; init; } = string.Empty;
}

public sealed record CombatTheftEvidence
{
    public string ContainerInventoryId { get; init; } = string.Empty;
    public string ItemId { get; init; } = string.Empty;
    public double ContainerAmountAfter { get; init; }
    public double PlayerAmountAfter { get; init; }
    public string TheftFlagId { get; init; } = string.Empty;
    public string TheftFlagAfter { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public string ReputationAfter { get; init; } = string.Empty;
}

public sealed record CombatEncounterDelta { public bool Changed { get; init; } public CombatEncounterEvidence Before { get; init; } = new(); public CombatEncounterEvidence After { get; init; } = new(); }
public sealed record CombatFactionDelta { public bool Changed { get; init; } public IReadOnlyDictionary<string, string> Before { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal); public IReadOnlyDictionary<string, string> After { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal); }
public sealed record CombatDialogueDelta { public bool Changed { get; init; } public CombatDialogueEvidence Before { get; init; } = new(); public CombatDialogueEvidence After { get; init; } = new(); }
public sealed record CombatWorkDelta { public bool Changed { get; init; } public CombatWorkEvidence Before { get; init; } = new(); public CombatWorkEvidence After { get; init; } = new(); }
public sealed record CombatContainerDelta { public bool Opened { get; init; } public bool Changed { get; init; } public IReadOnlyDictionary<string, string> Before { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal); public IReadOnlyDictionary<string, string> After { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal); }
public sealed record CombatInventoryDelta { public bool Changed { get; init; } public IReadOnlyDictionary<string, string> Before { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal); public IReadOnlyDictionary<string, string> After { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal); }
public sealed record CombatFlagDelta { public bool Changed { get; init; } public IReadOnlyDictionary<string, string> Before { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal); public IReadOnlyDictionary<string, string> After { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal); }

public sealed record RulePackCombatFactionSocialWorkTheftExternalExecutionFlags
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

public sealed record RulePackCombatFactionSocialWorkTheftDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

internal sealed record CombatFamilyScenarioSpec
{
    public string ScenarioId { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string InvalidKind { get; init; } = string.Empty;
    public IReadOnlyList<string> FamilyIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SourceDeclarationIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PackageRuntimeIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<CombatCommandSpec> Commands { get; init; } = Array.Empty<CombatCommandSpec>();
    public string ScenarioStateMarker { get; init; } = string.Empty;
}
