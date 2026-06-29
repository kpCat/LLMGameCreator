using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;
using LLMGameCreator.Application.Design.LuaSandboxExecutionGate;
using LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;

namespace LLMGameCreator.Application.Design.HybridDraftLuaExpansion;

public static partial class HybridDraftLuaExpansionCatalog
{
    private static readonly IReadOnlyList<string> ForbiddenBoundaryGroups =
    [
        "file_system",
        "filesystem",
        "network",
        "process",
        "os_process",
        "reflection",
        "thread",
        "threading",
        "time",
        "random",
        "native_interop",
        "runtime_mutation",
        "runtime_direct_mutation",
        "ui",
        "ui_winforms",
        "unity",
        "unity_direct_call",
        "gamepackage_schema_mutation",
        "provider_llm",
        "provider_llm_rag",
        "rag",
        "arbitrary_code_generation",
        "implicit_lua_execution",
        "lua_source_generation"
    ];

    public static HybridExecutorAdapterSelection BuildAdapterSelection() =>
        new()
        {
            LocalRestoreProbeSucceeded = true,
            TransitiveSourceGeneratorPackageObserved = true,
            SourceGeneratorAnalyzersExcludedByPackageMetadata = true,
            SafeApiIsolationProven = true,
            CapabilityFlags = new HybridExecutorCapabilityFlags
            {
                RealLuaExecution = true,
                RepoOwnedFixtureOnly = true,
                ArbitraryUserLuaAllowed = false,
                StandardLibrariesOpened = false,
                FilesystemExposed = false,
                NetworkExposed = false,
                ProcessExposed = false,
                ReflectionExposed = false,
                ThreadingExposed = false,
                WallClockTimeExposed = false,
                RandomExposed = false,
                NativeInteropExposed = false,
                RuntimeUiUnityGamePackageProviderLlmRagExposed = false,
                CancellationTokenSupported = true,
                InstructionCountHookSupported = false,
                DeclarativeFixtureRestrictionRequired = true
            },
            RiskNotes =
            [
                "LuaCSharp 0.5.5 restored in a disposable net8.0 probe and declares MIT license in the package nuspec.",
                "The package declares LuaCSharp.SourceGenerator transitively, but package metadata excludes Build and Analyzers assets for that dependency; no explicit source-generator package is added by this repo.",
                "LuaCSharp does not expose a Goal037-proven instruction-count hook, so accepted scripts are restricted to repo-owned declarative fixtures and loop/import/global boundary tokens are rejected before execution.",
                "The adapter never calls OpenStandardLibraries and never exposes host functions, .NET objects, filesystem, network, process, reflection, threading, wall-clock time, random, native interop, Runtime, UI, Unity, provider, LLM or RAG surfaces."
            ],
            Diagnostics =
            [
                Diagnostic("info", "hybrid.adapter.package.selected", "LuaCSharp/0.5.5", "Selected one pinned MIT package for a bounded Application-layer executor adapter."),
                Diagnostic("warning", "hybrid.adapter.instruction_count.unavailable", "LuaCSharp/0.5.5", "Instruction-count cancellation is not proven; declarative repo-owned fixtures are required.")
            ]
        };

    public static IReadOnlyList<HybridPipelineStep> BuildPipelineSteps() =>
    [
        new() { Ordinal = 1, StepId = "goal034_draft_request_candidate", SourceGoal = "Goal034", Responsibility = "Use strict draft request/candidate identifiers only; no live LLM call." },
        new() { Ordinal = 2, StepId = "goal035_lua_manifest_selection", SourceGoal = "Goal035", Responsibility = "Select reviewed manifest ids and dependency order from the manifest registry." },
        new() { Ordinal = 3, StepId = "goal036_sandbox_gate_decision", SourceGoal = "Goal036", Responsibility = "Require deny-first sandbox decision evidence before any bounded executor attempt." },
        new() { Ordinal = 4, StepId = "bounded_lua_expansion_request", SourceGoal = "Goal037", Responsibility = "Map draft/manifest/sandbox evidence to repo-owned deterministic fixture requests." },
        new() { Ordinal = 5, StepId = "executor_adapter_result", SourceGoal = "Goal037", Responsibility = "Run LuaCSharp without standard libraries or host APIs only for declarative fixtures." },
        new() { Ordinal = 6, StepId = "csharp_output_validator", SourceGoal = "Goal037", Responsibility = "Validate structured IR shape, budgets, traces and forbidden boundary claims." },
        new() { Ordinal = 7, StepId = "promotion_decision", SourceGoal = "Goal037", Responsibility = "Accept only validated IR for future review; never self-promote the manual gate." }
    ];

    public static IReadOnlyList<HybridDraftLuaExpansionRequest> BuildDefaultRequests()
    {
        var draftRequests = StrictLlmDraftArtifactLoopCatalog.BuildDefaultRequestSets()
            .SelectMany(item => item.Requests)
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .ToList();
        var manifestPlans = new LuaModuleManifestPlanner().PlanDefaultScenarios()
            .ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);
        var sandboxRequests = LuaSandboxExecutionGateCatalog.BuildDefaultRequests();
        var sandboxDecisions = sandboxRequests
            .Select(item => LuaSandboxExecutionGateValidator.Decide(item, LuaModuleManifestRegistryCatalog.BuildDefaultManifests()))
            .ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);
        var scenarios = BuildScenarioRequestSpecs();

        return scenarios
            .Select(spec => BuildRequest(spec, draftRequests, manifestPlans[spec.ScenarioId], sandboxDecisions[spec.ScenarioId]))
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyDictionary<string, HybridLuaFixture> BuildFixtures(IReadOnlyList<HybridDraftLuaExpansionRequest> requests) =>
        requests
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(request => BuildFixture(request))
            .ToDictionary(item => item.FixtureId, StringComparer.Ordinal);

    public static HybridDraftToLuaRequestMap BuildDraftToLuaRequestMap(IReadOnlyList<HybridDraftLuaExpansionRequest> requests)
    {
        var rows = requests
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(item => new HybridDraftToLuaRequestMapRow
            {
                ScenarioId = item.ScenarioId,
                ExecutionRequestId = item.ExecutionRequestId,
                SourceDraftRequestId = item.SourceDraftRequestId,
                SourceManifestId = item.SourceManifestId,
                SandboxDecisionId = item.SandboxDecisionId,
                ProducedArtifactFamily = item.ProducedArtifactFamily,
                FixtureId = item.FixtureId,
                OutputBudget = item.OutputBudget,
                SandboxApprovedForGoal037Executor = item.SandboxApprovedForGoal037Executor
            })
            .ToList();

        return new HybridDraftToLuaRequestMap
        {
            RequestCount = rows.Count,
            Rows = rows
        };
    }

    public static HybridSandboxApprovedExpansionMatrix BuildSandboxApprovedExpansionMatrix(IReadOnlyList<HybridDraftLuaExpansionRequest> requests)
    {
        var rows = requests
            .GroupBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(group =>
            {
                var request = group.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).First();
                var rejectedOrRepair = request.Goal036DecisionStatus is "rejected" or "needs_repair";
                return new HybridSandboxApprovalRow
                {
                    ScenarioId = request.ScenarioId,
                    SandboxDecisionId = request.SandboxDecisionId,
                    Goal036DecisionStatus = request.Goal036DecisionStatus,
                    Goal036RejectedOrRepairRequired = rejectedOrRepair,
                    Goal037AdapterAvailable = true,
                    ApprovedForRepoOwnedFixtureExecution = request.SandboxApprovedForGoal037Executor,
                    ApprovalReason = request.SandboxApprovedForGoal037Executor
                        ? "Goal036 decision is not rejected/repair_required and Goal037 provides a repo-owned fixture-only adapter."
                        : "Goal036 decision does not allow a bounded Goal037 fixture execution attempt."
                };
            })
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();

        return new HybridSandboxApprovedExpansionMatrix
        {
            RowCount = rows.Count,
            ApprovedCount = rows.Count(item => item.ApprovedForRepoOwnedFixtureExecution),
            Rows = rows
        };
    }

    public static HybridExpansionOutput BuildOutputFromFixture(
        HybridDraftLuaExpansionRequest request,
        string stableId,
        IReadOnlyList<HybridExpansionSlot> slots,
        IReadOnlyList<HybridWeightedTag> tags,
        IReadOnlyList<HybridExpansionRelation> relations,
        IReadOnlyList<HybridDraftLuaDiagnostic> diagnostics)
    {
        var orderedSlots = slots.OrderBy(item => item.SlotId, StringComparer.Ordinal).ToList();
        var orderedTags = tags.OrderBy(item => item.TagId, StringComparer.Ordinal).ToList();
        var orderedRelations = relations.OrderBy(item => item.RelationId, StringComparer.Ordinal).ToList();
        var traceSummary = $"{request.ExecutionRequestId}|scenario={request.ScenarioId}|family={request.ProducedArtifactFamily}|slots={orderedSlots.Count}|tags={orderedTags.Count}|relations={orderedRelations.Count}|luaExecuted=true";

        return new HybridExpansionOutput
        {
            StableId = stableId,
            ScenarioId = request.ScenarioId,
            SourceDraftRequestId = request.SourceDraftRequestId,
            SourceManifestId = request.SourceManifestId,
            SandboxDecisionId = request.SandboxDecisionId,
            ProducedArtifactFamily = request.ProducedArtifactFamily,
            Slots = orderedSlots,
            Tags = orderedTags,
            Relations = orderedRelations,
            Diagnostics = SortDiagnostics(diagnostics),
            PromotionStatus = "accepted",
            StructuralTraceSummary = traceSummary,
            TraceHash = ComputeHash(traceSummary),
            LuaExecuted = true
        };
    }

    public static IReadOnlyList<HybridDraftLuaDiagnostic> ValidateRequest(HybridDraftLuaExpansionRequest request)
    {
        var diagnostics = new List<HybridDraftLuaDiagnostic>();
        if (!StableIdPattern().IsMatch(request.ExecutionRequestId))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.request_id.invalid", request.ExecutionRequestId, "Execution request id must be stable."));
        }

        if (!HybridDraftLuaExpansionVocabulary.Scenarios.Contains(request.ScenarioId))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.scenario.fake", request.ScenarioId, "Request references an unknown scenario."));
        }

        if (!string.Equals(request.ProfileId, request.ScenarioId, StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.profile.wrong_scenario", request.ProfileId, "Request profile must match the selected scenario profile."));
        }

        if (!request.SourceDraftRequestId.StartsWith($"draft-request/{request.ScenarioId}/", StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.goal034_draft.fake", request.SourceDraftRequestId, "Request must reference a Goal034 draft request for the same scenario."));
        }

        if (!request.SourceManifestId.StartsWith($"lua-module/{ScenarioSegment(request.ScenarioId)}/", StringComparison.Ordinal)
            && !request.SourceManifestId.StartsWith("lua-module/metamodule/", StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.goal035_manifest.fake", request.SourceManifestId, "Request must reference a Goal035 selected manifest for the same scenario."));
        }

        if (!request.SandboxDecisionId.StartsWith($"goal036-sandbox-decision/{request.ScenarioId}", StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.goal036_sandbox_decision.fake", request.SandboxDecisionId, "Request must reference a Goal036 sandbox decision for the same scenario."));
        }

        if (request.SourceCategory != "repo_owned_fixture")
        {
            diagnostics.Add(Diagnostic("error", "hybrid.source_category.forbidden", request.SourceCategory, "Only repo-owned deterministic fixtures may be executed."));
        }

        if (!HybridDraftLuaExpansionVocabulary.ArtifactFamilies.Contains(request.ProducedArtifactFamily))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.artifact_family.unknown", request.ProducedArtifactFamily, "Produced artifact family is not allowed for Goal037."));
        }

        if (request.OutputBudget <= 0)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.budget.missing", request.ExecutionRequestId, "Output budget must be positive."));
        }

        if (!request.SandboxApprovedForGoal037Executor && request.ExecutorAttempted)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.sandbox_denied.executor_attempted", request.ExecutionRequestId, "Executor cannot run when sandbox approval is false."));
        }

        if (request.SelfPromoted)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.self_promotion.forbidden", request.ExecutionRequestId, "Goal037 output cannot promote itself or pass its manual gate."));
        }

        if (request.ClaimsGamePackageMutation)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.gamepackage_mutation.forbidden", request.ExecutionRequestId, "Goal037 must not mutate or claim mutation of GamePackage schema/content."));
        }

        foreach (var group in request.RequestedBoundaryGroups.Order(StringComparer.Ordinal))
        {
            if (ForbiddenBoundaryGroups.Contains(group, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", $"hybrid.boundary.{NormalizeCodeSegment(group)}.forbidden", group, "Request crosses a forbidden boundary for Goal037."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<HybridDraftLuaDiagnostic> ValidateFixture(HybridLuaFixture fixture)
    {
        var diagnostics = new List<HybridDraftLuaDiagnostic>();
        if (fixture.SourceCategory != "repo_owned_fixture")
        {
            diagnostics.Add(Diagnostic("error", "hybrid.fixture.source_category.forbidden", fixture.FixtureId, "Only repo-owned fixtures are accepted."));
        }

        if (!fixture.DeclarativeOnly)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.fixture.declarative.required", fixture.FixtureId, "Fixture must be declarative-only."));
        }

        var forbiddenTokens = new[]
        {
            " for ",
            " while ",
            " repeat ",
            " function ",
            " require",
            " dofile",
            " load",
            " io.",
            " os.",
            " debug.",
            " coroutine.",
            " math.random",
            " package."
        };
        var padded = $" {fixture.ScriptText.Replace(Environment.NewLine, " ", StringComparison.Ordinal)} ";
        foreach (var token in forbiddenTokens)
        {
            if (padded.Contains(token, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "hybrid.fixture.token.forbidden", fixture.FixtureId, $"Fixture contains forbidden token '{token.Trim()}'."));
            }
        }

        if (!fixture.ScriptText.TrimStart().StartsWith("return", StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.fixture.return_table.required", fixture.FixtureId, "Fixture must return a structured table."));
        }

        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<HybridDraftLuaDiagnostic> ValidateOutput(HybridDraftLuaExpansionRequest request, HybridExpansionOutput? output)
    {
        var diagnostics = new List<HybridDraftLuaDiagnostic>();
        diagnostics.AddRange(ValidateRequest(request));
        if (output == null)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.malformed", request.ExecutionRequestId, "Executor did not return a structured output."));
            return SortDiagnostics(diagnostics);
        }

        if (!StableIdPattern().IsMatch(output.StableId))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output_id.invalid", output.StableId, "Output stable id is invalid."));
        }

        if (output.ScenarioId != request.ScenarioId)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.scenario_mismatch", output.ScenarioId, "Output scenario does not match request."));
        }

        if (output.SourceDraftRequestId != request.SourceDraftRequestId)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.goal034_draft_mismatch", output.SourceDraftRequestId, "Output draft request id does not match request."));
        }

        if (output.SourceManifestId != request.SourceManifestId)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.goal035_manifest_mismatch", output.SourceManifestId, "Output manifest id does not match request."));
        }

        if (output.SandboxDecisionId != request.SandboxDecisionId)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.goal036_decision_mismatch", output.SandboxDecisionId, "Output sandbox decision id does not match request."));
        }

        if (output.ProducedArtifactFamily != request.ProducedArtifactFamily)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.family_mismatch", output.ProducedArtifactFamily, "Output artifact family does not match request."));
        }

        if (!output.LuaExecuted)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.lua_not_executed", output.StableId, "GREEN Goal037 evidence must include a real bounded Lua execution result."));
        }

        if (output.Slots.Count == 0 || output.Slots.Count > request.OutputBudget)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.budget.exceeded", output.StableId, "Output slot count must be positive and within request budget."));
        }

        if (!output.Slots.Select(item => item.SlotId).SequenceEqual(output.Slots.Select(item => item.SlotId).Order(StringComparer.Ordinal), StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.order.nondeterministic", output.StableId, "Output slots must use stable ordering."));
        }

        if (output.Slots.Select(item => item.SlotId).Distinct(StringComparer.Ordinal).Count() != output.Slots.Count)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.slot.duplicate", output.StableId, "Output slots must have unique ids."));
        }

        if (string.IsNullOrWhiteSpace(output.TraceHash) || string.IsNullOrWhiteSpace(output.StructuralTraceSummary))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.trace.missing", output.StableId, "Output must include a structural trace summary and trace hash."));
        }

        if (!HybridDraftLuaExpansionVocabulary.PromotionStatuses.Contains(output.PromotionStatus))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.output.promotion_status.unknown", output.PromotionStatus, "Output promotion status is unknown."));
        }

        if (ContainsFinalProse(output))
        {
            diagnostics.Add(Diagnostic("error", "hybrid.final_prose.forbidden", output.StableId, "Output must be structured IR and must not contain final prose payloads."));
        }

        return SortDiagnostics(diagnostics);
    }

    public static HybridPromotionDecision DecidePromotion(HybridDraftLuaExpansionRequest request, HybridExpansionOutput? output)
    {
        var diagnostics = ValidateOutput(request, output);
        var hasErrors = diagnostics.Any(item => item.Severity == "error");
        var status = hasErrors ? "rejected" : "accepted";

        return new HybridPromotionDecision
        {
            DecisionId = $"hybrid-promotion/{request.ScenarioId}/{StableSuffix(request.ExecutionRequestId)}",
            ExecutionRequestId = request.ExecutionRequestId,
            StableOutputId = output?.StableId ?? string.Empty,
            ScenarioId = request.ScenarioId,
            PromotionStatus = status,
            Promoted = !hasErrors,
            Reasons = hasErrors
                ? diagnostics.Where(item => item.Severity == "error").Select(item => item.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
                : ["validated_structured_ir", "bounded_lua_executor_result", "manual_gate_remains_required"],
            Diagnostics = diagnostics
        };
    }

    public static HybridInvalidMatrix BuildInvalidMatrix(
        IReadOnlyList<HybridDraftLuaExpansionRequest> validRequests,
        IReadOnlyDictionary<string, HybridExpansionOutput> validOutputs,
        HybridExecutorAdapterSelection adapterSelection)
    {
        var valid = validRequests.First(item => item.ScenarioId == "frontier_survival");
        var validOutput = validOutputs[valid.ExecutionRequestId];
        var invalidCases = new List<HybridInvalidScenario>
        {
            Invalid("fake_goal034_draft_id", "fake Goal034 draft id", "rejected", valid with { SourceDraftRequestId = "draft-request/fake/missing/001" }, validOutput),
            Invalid("fake_goal035_manifest_id", "fake Goal035 manifest id", "rejected", valid with { SourceManifestId = "lua-module/fake/missing" }, validOutput),
            Invalid("fake_goal036_sandbox_decision_id", "fake Goal036 sandbox decision id", "rejected", valid with { SandboxDecisionId = "goal036-sandbox-decision/fake" }, validOutput),
            Invalid("sandbox_denied_executor_attempted", "sandbox denied but executor attempted", "rejected", valid with { SandboxApprovedForGoal037Executor = false, ExecutorAttempted = true }, validOutput),
            Invalid("wrong_scenario_profile", "wrong scenario/profile", "rejected", valid with { ProfileId = "gothic_intrigue" }, validOutput),
            Invalid("final_prose_payload", "final prose payload", "rejected", valid, validOutput with { Tags = validOutput.Tags.Concat([new HybridWeightedTag { TagId = "final prose sentence with spaces", Weight = 1 }]).ToList() }),
            Invalid("gamepackage_mutation_claim", "GamePackage mutation claim", "rejected", valid with { ClaimsGamePackageMutation = true }, validOutput),
            Invalid("runtime_ui_unity_provider_llm_rag_lua_source_generation_leak", "Runtime/UI/Unity/provider/LLM/RAG/Lua source generation leakage", "rejected", valid with { RequestedBoundaryGroups = ["runtime_mutation", "ui", "unity", "provider_llm", "rag", "lua_source_generation"] }, validOutput),
            Invalid("filesystem_network_process_reflection_thread_time_random_native_interop_request", "filesystem/network/process/reflection/thread/time/random/native interop request", "rejected", valid with { RequestedBoundaryGroups = ["file_system", "network", "process", "reflection", "threading", "time", "random", "native_interop"] }, validOutput),
            Invalid("over_budget_output", "over-budget output", "rejected", valid with { OutputBudget = 1 }, validOutput),
            Invalid("nondeterministic_output_order", "nondeterministic output order", "rejected", valid, validOutput with { Slots = validOutput.Slots.Reverse().ToList() }),
            Invalid("missing_trace", "missing trace", "rejected", valid, validOutput with { TraceHash = string.Empty, StructuralTraceSummary = string.Empty }),
            Invalid("self_promotion", "self-promotion", "rejected", valid with { SelfPromoted = true }, validOutput),
            Invalid(
                "dependency_unavailable_unsafe_adapter_blocker_path",
                "dependency unavailable / unsafe adapter blocker path",
                "blocked",
                valid,
                validOutput,
                adapterSelection with
                {
                    Status = "blocked_dependency_unavailable_or_unsafe",
                    DependencyUnavailableOrUnsafe = true,
                    SafeApiIsolationProven = false,
                    BlockerReason = "LuaCSharp dependency or sandbox API isolation could not be proven."
                }),
            Invalid("malformed_executor_output", "malformed executor output", "rejected", valid, null)
        };

        return new HybridInvalidMatrix
        {
            ScenarioCount = invalidCases.Count,
            MatchedExpectationCount = invalidCases.Count(item => item.ExpectedStatus == item.ActualStatus),
            RejectedCount = invalidCases.Count(item => item.ActualStatus == "rejected"),
            RepairRequiredCount = invalidCases.Count(item => item.ActualStatus == "repair_required"),
            BlockedCount = invalidCases.Count(item => item.ActualStatus == "blocked"),
            Passed = invalidCases.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            Scenarios = invalidCases.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static HybridDraftLuaDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    public static IReadOnlyList<HybridDraftLuaDiagnostic> SortDiagnostics(IEnumerable<HybridDraftLuaDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static string ComputeHash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static HybridDraftLuaExpansionRequest BuildRequest(
        ScenarioRequestSpec spec,
        IReadOnlyList<StrictLlmDraftRequest> draftRequests,
        LuaModuleSelectionPlan manifestPlan,
        LuaSandboxExecutionDecision sandboxDecision)
    {
        var draft = draftRequests.First(item => item.ScenarioId == spec.ScenarioId && item.TargetDraftFamily == spec.DraftFamilyId);
        var manifest = manifestPlan.SelectedManifests.First(item => item.FamilyId == spec.ManifestFamilyId);
        var approved = sandboxDecision.DecisionStatus is not "rejected" and not "needs_repair";
        var requestId = $"hybrid-expansion-request/{spec.ScenarioId}/{spec.ArtifactFamily}";

        return new HybridDraftLuaExpansionRequest
        {
            ExecutionRequestId = requestId,
            ScenarioId = spec.ScenarioId,
            ProfileId = spec.ScenarioId,
            SourceDraftRequestId = draft.RequestId,
            SourceManifestId = manifest.ModuleId,
            SandboxDecisionId = $"goal036-sandbox-decision/{spec.ScenarioId}/{sandboxDecision.DecisionStatus}",
            Goal036DecisionStatus = sandboxDecision.DecisionStatus,
            ProducedArtifactFamily = spec.ArtifactFamily,
            OutputBudget = spec.OutputBudget,
            FixtureId = $"repo-fixture/{spec.ScenarioId}/{spec.ArtifactFamily}",
            SandboxApprovedForGoal037Executor = approved,
            ExecutorAttempted = approved,
            DeterministicOrderingKey = $"{spec.ScenarioId}|{spec.ArtifactFamily}|{requestId}"
        };
    }

    private static IReadOnlyList<ScenarioRequestSpec> BuildScenarioRequestSpecs() =>
    [
        new("frontier_survival", "npc_role_personality_draft", "npc_species_archetype_rules", "npc_species_archetype_expansion_hints", 8),
        new("frontier_survival", "quest_motive_objective_draft", "quest_objective_reward_rules", "quest_event_intent_expansion_hints", 6),
        new("gothic_intrigue", "faction_relation_draft", "faction_reputation_social_relation_rules", "region_faction_kingdom_expansion_hints", 7),
        new("gothic_intrigue", "dialogue_act_template_slot_draft", "quest_objective_reward_rules", "quest_event_intent_expansion_hints", 6),
        new("caravan_trade", "economy_item_resource_hint_draft", "item_resource_recipe_loot_economy_rules", "economy_combat_settlement_expansion_hints", 7),
        new("caravan_trade", "quest_motive_objective_draft", "quest_objective_reward_rules", "quest_event_intent_expansion_hints", 6),
        new("metamodule_kingdoms", "species_archetype_feature_draft", "metamodule_species_archetype_expansion_rules", "metamodule_species_archetype_slot_expansion", 128),
        new("metamodule_kingdoms", "faction_relation_draft", "faction_reputation_social_relation_rules", "region_faction_kingdom_expansion_hints", 8)
    ];

    private static HybridLuaFixture BuildFixture(HybridDraftLuaExpansionRequest request)
    {
        var slots = request.ProducedArtifactFamily == "metamodule_species_archetype_slot_expansion"
            ? BuildMetamoduleSlots(request)
            : BuildScenarioSlots(request);
        var tags = BuildTags(request);
        var relations = BuildRelations(request, slots);
        var script = RenderFixtureScript(request, slots, tags, relations);

        return new HybridLuaFixture
        {
            FixtureId = request.FixtureId,
            ScenarioId = request.ScenarioId,
            ProducedArtifactFamily = request.ProducedArtifactFamily,
            ScriptText = script,
            ScriptHash = ComputeHash(script),
            DeclarativeOnly = true
        };
    }

    private static IReadOnlyList<HybridExpansionSlot> BuildScenarioSlots(HybridDraftLuaExpansionRequest request)
    {
        var prefix = $"{request.ScenarioId}/{request.ProducedArtifactFamily}";
        return Enumerable.Range(1, Math.Min(3, request.OutputBudget))
            .Select(index => new HybridExpansionSlot
            {
                SlotId = $"slot/{prefix}/{index:000}",
                SlotKind = request.ProducedArtifactFamily,
                Weight = 10 - index,
                Tags = [$"scenario:{request.ScenarioId}", $"family:{request.ProducedArtifactFamily}", $"fixture:{index:000}"],
                RelationIds = [$"relation/{prefix}/{index:000}"]
            })
            .OrderBy(item => item.SlotId, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<HybridExpansionSlot> BuildMetamoduleSlots(HybridDraftLuaExpansionRequest request)
    {
        var selectedSlots = new LuaModuleManifestPlanner().PlanDefaultScenarios()
            .Single(item => item.ScenarioId == "metamodule_kingdoms")
            .SelectedManifests
            .Where(item => item.ModuleId.StartsWith("lua-module/metamodule/species-archetype-slot/", StringComparison.Ordinal))
            .OrderBy(item => item.ModuleId, StringComparer.Ordinal)
            .Take(request.OutputBudget)
            .Select((manifest, index) => new HybridExpansionSlot
            {
                SlotId = $"slot/metamodule/species-archetype/{index + 1:000}",
                SlotKind = "metamodule_species_archetype_slot",
                Weight = 1000 - index,
                Tags = [$"manifest:{manifest.ModuleId}", "metamodule:kingdoms", "species_archetype_slot"],
                RelationIds = [$"relation/metamodule/species-archetype/{index + 1:000}"]
            })
            .ToList();

        return selectedSlots;
    }

    private static IReadOnlyList<HybridWeightedTag> BuildTags(HybridDraftLuaExpansionRequest request) =>
    [
        new() { TagId = $"scenario:{request.ScenarioId}", Weight = 100 },
        new() { TagId = $"artifact_family:{request.ProducedArtifactFamily}", Weight = 90 },
        new() { TagId = "source:repo_owned_fixture", Weight = 80 }
    ];

    private static IReadOnlyList<HybridExpansionRelation> BuildRelations(HybridDraftLuaExpansionRequest request, IReadOnlyList<HybridExpansionSlot> slots) =>
        slots
            .Take(Math.Min(slots.Count, 8))
            .Select(slot => new HybridExpansionRelation
            {
                RelationId = slot.RelationIds.First(),
                SourceId = request.SourceManifestId,
                TargetId = slot.SlotId,
                RelationKind = "expands"
            })
            .OrderBy(item => item.RelationId, StringComparer.Ordinal)
            .ToList();

    private static string RenderFixtureScript(
        HybridDraftLuaExpansionRequest request,
        IReadOnlyList<HybridExpansionSlot> slots,
        IReadOnlyList<HybridWeightedTag> tags,
        IReadOnlyList<HybridExpansionRelation> relations)
    {
        var lines = new List<string>
        {
            "return {",
            $"  stableId = '{EscapeLua($"hybrid-expansion/{request.ScenarioId}/{request.ProducedArtifactFamily}")}',",
            "  slots = {"
        };
        lines.AddRange(slots.Select(slot =>
            $"    {{ id = '{EscapeLua(slot.SlotId)}', kind = '{EscapeLua(slot.SlotKind)}', weight = {slot.Weight}, tags = {{{string.Join(", ", slot.Tags.Select(tag => $"'{EscapeLua(tag)}'"))}}}, relations = {{{string.Join(", ", slot.RelationIds.Select(relation => $"'{EscapeLua(relation)}'"))}}} }},"));
        lines.Add("  },");
        lines.Add("  tags = {");
        lines.AddRange(tags.Select(tag => $"    {{ id = '{EscapeLua(tag.TagId)}', weight = {tag.Weight} }},"));
        lines.Add("  },");
        lines.Add("  relations = {");
        lines.AddRange(relations.Select(relation => $"    {{ id = '{EscapeLua(relation.RelationId)}', source = '{EscapeLua(relation.SourceId)}', target = '{EscapeLua(relation.TargetId)}', kind = '{EscapeLua(relation.RelationKind)}' }},"));
        lines.Add("  },");
        lines.Add("  diagnostics = {");
        lines.Add($"    {{ severity = 'info', code = 'hybrid.fixture.executed', target = '{EscapeLua(request.ExecutionRequestId)}', message = 'repo owned deterministic fixture executed' }}");
        lines.Add("  }");
        lines.Add("}");
        return string.Join(Environment.NewLine, lines);
    }

    private static HybridInvalidScenario Invalid(
        string scenarioId,
        string kind,
        string expectedStatus,
        HybridDraftLuaExpansionRequest request,
        HybridExpansionOutput? output,
        HybridExecutorAdapterSelection? adapterSelection = null)
    {
        var selection = adapterSelection ?? BuildAdapterSelection();
        var diagnostics = new List<HybridDraftLuaDiagnostic>();
        if (selection.DependencyUnavailableOrUnsafe || !selection.SafeApiIsolationProven)
        {
            diagnostics.Add(Diagnostic("error", "hybrid.adapter.blocked", selection.PackageId, selection.BlockerReason));
            return new HybridInvalidScenario
            {
                ScenarioId = scenarioId,
                MutatedEvidenceKind = kind,
                ExpectedStatus = expectedStatus,
                ActualStatus = "blocked",
                ExpectedValid = false,
                ActualValid = false,
                Diagnostics = SortDiagnostics(diagnostics)
            };
        }

        diagnostics.AddRange(ValidateOutput(request, output));
        var actualStatus = diagnostics.Any(item => item.Severity == "error") ? "rejected" : "accepted";
        return new HybridInvalidScenario
        {
            ScenarioId = scenarioId,
            MutatedEvidenceKind = kind,
            ExpectedStatus = expectedStatus,
            ActualStatus = actualStatus,
            ExpectedValid = false,
            ActualValid = actualStatus == "accepted",
            Diagnostics = SortDiagnostics(diagnostics.Where(item => item.Severity == "error"))
        };
    }

    private static bool ContainsFinalProse(HybridExpansionOutput output)
    {
        static bool LooksLikeSentence(string value) => value.Contains(" ", StringComparison.Ordinal) && !value.Contains(':', StringComparison.Ordinal);
        return output.Tags.Any(item => LooksLikeSentence(item.TagId))
            || output.Slots.SelectMany(item => item.Tags).Any(LooksLikeSentence)
            || output.Diagnostics.Any(item => item.Code.Contains("final_prose", StringComparison.Ordinal));
    }

    private static string ScenarioSegment(string scenarioId) =>
        scenarioId switch
        {
            "frontier_survival" => "frontier",
            "gothic_intrigue" => "gothic",
            "caravan_trade" => "caravan",
            "metamodule_kingdoms" => "metamodule",
            _ => scenarioId
        };

    private static string StableSuffix(string value)
    {
        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? "unknown" : parts[^1].Replace('_', '-');
    }

    private static string NormalizeCodeSegment(string value) => value.Replace('.', '_').Replace('-', '_');

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static string EscapeLua(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal);

    [GeneratedRegex("^[a-z0-9][a-z0-9_./:-]*[a-z0-9]$")]
    private static partial Regex StableIdPattern();

    private sealed record ScenarioRequestSpec(
        string ScenarioId,
        string DraftFamilyId,
        string ManifestFamilyId,
        string ArtifactFamily,
        int OutputBudget);
}
