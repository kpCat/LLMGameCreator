using System.Text;

namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveProviderJobPlanService
{
    private static readonly UnityArchiveRequestProviderKind[] JobProviderKinds =
    [
        UnityArchiveRequestProviderKind.manual_import,
        UnityArchiveRequestProviderKind.comfyui_future,
        UnityArchiveRequestProviderKind.suno_future,
        UnityArchiveRequestProviderKind.local_audio_future,
        UnityArchiveRequestProviderKind.procedural_future
    ];

    public UnityArchiveProviderJobPlanResult BuildPlan(UnityArchiveProviderJobPlanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.RequestPipeline);
        ArgumentNullException.ThrowIfNull(request.ArchiveManifest);
        ArgumentNullException.ThrowIfNull(request.DesignBrief);
        ArgumentNullException.ThrowIfNull(request.TargetProfile);
        if (string.IsNullOrWhiteSpace(request.ProjectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(request));
        }

        var assetRequests = Order(request.RequestPipeline.AssetRequests, item => item.RequestId);
        var audioRequests = Order(request.RequestPipeline.AudioRequests, item => item.RequestId);
        var luaRequests = Order(request.RequestPipeline.LuaModuleRequests, item => item.ModuleId);
        var assetSlots = assetRequests.Select(CreateAssetSlot).ToList();
        var audioSlots = audioRequests.Select(CreateAudioSlot).ToList();
        var luaSlots = luaRequests.Select(CreateLuaSlot).ToList();

        var jobs = new List<UnityArchiveProviderJob>();
        jobs.AddRange(assetRequests.Zip(assetSlots, CreateJob));
        jobs.AddRange(audioRequests.Zip(audioSlots, CreateJob));
        jobs.AddRange(luaRequests.Zip(luaSlots, CreateJob));
        jobs = jobs
            .Where(job => job.ProviderKind != UnityArchiveRequestProviderKind.none)
            .OrderBy(job => job.JobId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(job => job.JobId, StringComparer.Ordinal)
            .ToList();

        var batches = JobProviderKinds.Select(provider => new UnityArchiveProviderJobBatch
        {
            ProviderKind = provider,
            ExecutionEnabled = false,
            Jobs = jobs.Where(job => job.ProviderKind == provider).ToList()
        }).ToList();

        var fulfillmentSlots = assetSlots.Select(ToFulfillmentSlot)
            .Concat(audioSlots.Select(ToFulfillmentSlot))
            .Concat(luaSlots.Select(ToFulfillmentSlot))
            .OrderBy(slot => slot.SlotId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(slot => slot.SlotId, StringComparer.Ordinal)
            .ToList();

        var diagnostics = Validate(request.RequestPipeline, fulfillmentSlots, jobs, batches);
        var readiness = diagnostics.Any(item => item.Severity == UnityArchiveExportDiagnosticSeverity.Error)
            ? UnityArchiveProviderPlanReadiness.BlockedByErrors
            : diagnostics.Any(item => item.Severity == UnityArchiveExportDiagnosticSeverity.Warning)
                ? UnityArchiveProviderPlanReadiness.ReadyWithWarnings
                : UnityArchiveProviderPlanReadiness.Ready;
        var providerReadiness = batches.Select(batch => new UnityArchiveProviderJobReadinessEntry
        {
            ProviderKind = batch.ProviderKind,
            JobCount = batch.Jobs.Count,
            Readiness = UnityArchiveProviderJobReadiness.planned_not_executed,
            ExecutionEnabled = false
        }).ToList();

        var report = new UnityArchiveProviderReadinessReport
        {
            Readiness = readiness,
            AssetSlotCount = assetSlots.Count,
            AudioSlotCount = audioSlots.Count,
            LuaModuleSlotCount = luaSlots.Count,
            ProviderJobCount = jobs.Count,
            Providers = providerReadiness,
            Diagnostics = diagnostics
        };

        return new UnityArchiveProviderJobPlanResult
        {
            FulfillmentPlan = new UnityArchiveFulfillmentPlan
            {
                GameId = request.ArchiveManifest.GameId.Trim(),
                DesignBriefId = request.DesignBrief.BriefId.Trim(),
                TargetProfileId = request.TargetProfile.TargetProfileId.Trim(),
                Slots = fulfillmentSlots
            },
            AssetSlots = new UnityArchiveAssetSlotIndex { Slots = assetSlots },
            AudioSlots = new UnityArchiveAudioSlotIndex { Slots = audioSlots },
            LuaModuleSlots = new UnityArchiveLuaModuleSlotIndex { Slots = luaSlots },
            ProviderJobs = new UnityArchiveProviderJobIndex { Batches = batches },
            ReadinessReport = report,
            Diagnostics = diagnostics,
            Readiness = readiness
        };
    }

    public static bool IsSafeExpectedOutputRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath) ||
            relativePath.Contains('\\') || relativePath.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        var segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length > 1 &&
               segments.Length == relativePath.Split('/').Length &&
               segments.All(segment => segment is not "." and not "..") &&
               !relativePath.Contains(':');
    }

    private static UnityArchiveAssetSlot CreateAssetSlot(UnityArchiveAssetRequest request)
    {
        return new UnityArchiveAssetSlot
        {
            SlotId = $"asset-slot.{SafeToken(request.RequestId)}",
            RequestId = request.RequestId,
            AssetId = request.AssetId,
            AssetKind = request.AssetKind,
            ProviderKind = request.ProviderKind,
            ExpectedOutputRelativePath = $"assets/generated/{request.AssetKind.ToString().ToLowerInvariant()}/{SafeToken(request.AssetId)}.png",
            Required = true,
            SourceRef = request.SourceRef
        };
    }

    private static UnityArchiveAudioSlot CreateAudioSlot(UnityArchiveAudioRequest request)
    {
        return new UnityArchiveAudioSlot
        {
            SlotId = $"audio-slot.{SafeToken(request.RequestId)}",
            RequestId = request.RequestId,
            AudioId = request.AudioId,
            AudioKind = request.AudioKind,
            ProviderKind = request.ProviderKind,
            ExpectedOutputRelativePath = $"audio/generated/{request.AudioKind.ToString().ToLowerInvariant()}/{SafeToken(request.AudioId)}.wav",
            Required = true,
            SourceRef = request.SourceRef
        };
    }

    private static UnityArchiveLuaModuleSlot CreateLuaSlot(UnityArchiveLuaModuleRequest request)
    {
        return new UnityArchiveLuaModuleSlot
        {
            SlotId = $"lua-slot.{SafeToken(request.ModuleId)}",
            ModuleId = request.ModuleId,
            ModuleKind = request.ModuleKind,
            ProviderKind = request.ProviderKind,
            ExpectedOutputRelativePath = $"lua/generated/{SafeToken(request.ModuleId)}.lua",
            Required = true,
            SourceRef = request.SourceRef
        };
    }

    private static UnityArchiveProviderJob CreateJob(UnityArchiveAssetRequest request, UnityArchiveAssetSlot slot)
    {
        return NewJob(request.ProviderKind, request.RequestId, slot.SlotId, slot.ExpectedOutputRelativePath,
            request.PromptOrInstruction, request.SourceRef, request.StyleTags, request.Metadata);
    }

    private static UnityArchiveProviderJob CreateJob(UnityArchiveAudioRequest request, UnityArchiveAudioSlot slot)
    {
        return NewJob(request.ProviderKind, request.RequestId, slot.SlotId, slot.ExpectedOutputRelativePath,
            request.PromptOrInstruction, request.SourceRef, Array.Empty<string>(), request.Metadata);
    }

    private static UnityArchiveProviderJob CreateJob(UnityArchiveLuaModuleRequest request, UnityArchiveLuaModuleSlot slot)
    {
        return NewJob(request.ProviderKind, request.ModuleId, slot.SlotId, slot.ExpectedOutputRelativePath,
            request.PromptOrInstruction, request.SourceRef, Array.Empty<string>(), request.Metadata);
    }

    private static UnityArchiveProviderJob NewJob(
        UnityArchiveRequestProviderKind providerKind,
        string requestId,
        string slotId,
        string expectedPath,
        string instruction,
        UnityArchiveRequestSourceRef sourceRef,
        IReadOnlyList<string> tags,
        IReadOnlyDictionary<string, string> metadata)
    {
        return new UnityArchiveProviderJob
        {
            JobId = $"provider-job.{providerKind.ToString().ToLowerInvariant()}.{SafeToken(requestId)}",
            ProviderKind = providerKind,
            RequestId = requestId,
            SlotId = slotId,
            ExpectedOutputRelativePath = expectedPath,
            PromptOrInstruction = instruction,
            SourceRef = sourceRef,
            Tags = tags.OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase).ThenBy(tag => tag, StringComparer.Ordinal).ToList(),
            Metadata = metadata.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.Key, StringComparer.Ordinal)
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal),
            Readiness = UnityArchiveProviderJobReadiness.planned_not_executed,
            ExecutionEnabled = false
        };
    }

    private static IReadOnlyList<UnityArchiveProviderJobPlanDiagnostic> Validate(
        UnityArchiveRequestPipelineResult pipeline,
        IReadOnlyList<UnityArchiveFulfillmentSlot> slots,
        IReadOnlyList<UnityArchiveProviderJob> jobs,
        IReadOnlyList<UnityArchiveProviderJobBatch> batches)
    {
        var diagnostics = new List<UnityArchiveProviderJobPlanDiagnostic>();
        AddDuplicates(slots.Select(slot => slot.SlotId), "provider_plan.duplicate_slot_id", "slot", diagnostics);
        AddDuplicates(jobs.Select(job => job.JobId), "provider_plan.duplicate_job_id", "job", diagnostics);

        foreach (var slot in slots.Where(slot => !IsSafeExpectedOutputRelativePath(slot.ExpectedOutputRelativePath)))
        {
            diagnostics.Add(Error("provider_plan.unsafe_expected_output_path", $"Unsafe expected output path '{slot.ExpectedOutputRelativePath}'.", slot.SlotId));
        }

        foreach (var provider in slots.Select(slot => slot.ProviderKind).Concat(jobs.Select(job => job.ProviderKind)).Distinct())
        {
            if (!Enum.IsDefined(provider))
            {
                diagnostics.Add(Error("provider_plan.unknown_provider_kind", $"Unknown provider kind '{provider}'.", provider.ToString()));
            }
            else if (UnityArchiveRequestDiagnosticsBuilder.IsFutureProvider(provider))
            {
                diagnostics.Add(Warning("provider_plan.future_provider_planned", $"Future provider '{provider}' is planned but not executed.", provider.ToString()));
            }
        }

        var requestIds = pipeline.AssetRequests.Select(item => item.RequestId)
            .Concat(pipeline.AudioRequests.Select(item => item.RequestId))
            .Concat(pipeline.LuaModuleRequests.Select(item => item.ModuleId));
        foreach (var requestId in requestIds.Where(requestId => !slots.Any(slot => string.Equals(slot.RequestId, requestId, StringComparison.OrdinalIgnoreCase))))
        {
            diagnostics.Add(Error("provider_plan.missing_slot_for_request", $"Missing fulfillment slot for request '{requestId}'.", requestId));
        }

        foreach (var job in jobs.Where(job => job.ProviderKind == UnityArchiveRequestProviderKind.none))
        {
            diagnostics.Add(Error("provider_plan.job_with_none_provider", $"Provider job '{job.JobId}' cannot use provider 'none'.", job.JobId));
        }

        foreach (var batch in batches.Where(batch => batch.ExecutionEnabled))
        {
            diagnostics.Add(Error("provider_plan.executable_provider_claim", $"Provider batch '{batch.ProviderKind}' must remain non-executable.", batch.ProviderKind.ToString()));
        }
        foreach (var job in jobs.Where(job => job.ExecutionEnabled || job.Readiness != UnityArchiveProviderJobReadiness.planned_not_executed))
        {
            diagnostics.Add(Error("provider_plan.executable_provider_claim", $"Provider job '{job.JobId}' must remain planned and non-executable.", job.JobId));
        }

        return diagnostics
            .OrderBy(item => UnityArchiveRequestDiagnosticsBuilder.SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.TargetId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void AddDuplicates(
        IEnumerable<string> values,
        string code,
        string label,
        ICollection<UnityArchiveProviderJobPlanDiagnostic> diagnostics)
    {
        foreach (var value in values.GroupBy(item => item, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1)
                     .Select(group => group.Key)
                     .OrderBy(item => item, StringComparer.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error(code, $"Duplicate {label} id '{value}'.", value));
        }
    }

    private static string SafeToken(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return "unknown";
        }

        var builder = new StringBuilder(trimmed.Length);
        foreach (var character in trimmed)
        {
            var normalized = character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '.' or '-' or '_'
                ? char.ToLowerInvariant(character)
                : '-';
            builder.Append(normalized == '.' && builder.Length > 0 && builder[^1] == '.' ? '-' : normalized);
        }

        var result = builder.ToString().Trim('.', '-', '_');
        return result.Length == 0 ? "unknown" : result;
    }

    private static UnityArchiveFulfillmentSlot ToFulfillmentSlot(UnityArchiveAssetSlot slot) =>
        new()
        {
            SlotId = slot.SlotId,
            RequestId = slot.RequestId,
            ProviderKind = slot.ProviderKind,
            ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
            Required = slot.Required,
            Status = slot.Status,
            SourceRef = slot.SourceRef
        };

    private static UnityArchiveFulfillmentSlot ToFulfillmentSlot(UnityArchiveAudioSlot slot) =>
        new()
        {
            SlotId = slot.SlotId,
            RequestId = slot.RequestId,
            ProviderKind = slot.ProviderKind,
            ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
            Required = slot.Required,
            Status = slot.Status,
            SourceRef = slot.SourceRef
        };

    private static UnityArchiveFulfillmentSlot ToFulfillmentSlot(UnityArchiveLuaModuleSlot slot) =>
        new()
        {
            SlotId = slot.SlotId,
            RequestId = slot.ModuleId,
            ProviderKind = slot.ProviderKind,
            ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
            Required = slot.Required,
            Status = slot.Status,
            SourceRef = slot.SourceRef
        };

    private static IReadOnlyList<T> Order<T>(IEnumerable<T> values, Func<T, string> idSelector)
    {
        return values.OrderBy(idSelector, StringComparer.OrdinalIgnoreCase)
            .ThenBy(idSelector, StringComparer.Ordinal)
            .ToList();
    }

    private static UnityArchiveProviderJobPlanDiagnostic Error(string code, string message, string targetId) =>
        new() { Severity = UnityArchiveExportDiagnosticSeverity.Error, Code = code, Message = message, TargetId = targetId };

    private static UnityArchiveProviderJobPlanDiagnostic Warning(string code, string message, string targetId) =>
        new() { Severity = UnityArchiveExportDiagnosticSeverity.Warning, Code = code, Message = message, TargetId = targetId };
}
