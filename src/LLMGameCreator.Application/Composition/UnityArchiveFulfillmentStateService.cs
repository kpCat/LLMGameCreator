using System.Text;

namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveFulfillmentStateService
{
    private const string ExpectedAssetExtension = ".png";
    private const string ExpectedAudioExtension = ".wav";
    private const string ExpectedLuaExtension = ".lua";

    public UnityArchiveFulfillmentStateResult Scan(UnityArchiveFulfillmentStateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ProviderJobPlan);
        if (string.IsNullOrWhiteSpace(request.OutputDirectoryPath))
        {
            throw new ArgumentException("Output directory path is required.", nameof(request));
        }

        var entries = new List<UnityArchiveFulfillmentStateEntry>();
        var fulfilledAssets = new List<UnityArchiveFulfilledAssetEntry>();
        var fulfilledAudio = new List<UnityArchiveFulfilledAudioEntry>();
        var fulfilledLua = new List<UnityArchiveFulfilledLuaEntry>();
        var invalidOutputs = new List<UnityArchiveInvalidOutputEntry>();
        var diagnostics = Validate(request.ProviderJobPlan).ToList();

        ProcessAssetSlots(request.ProviderJobPlan.AssetSlots.Slots, request.OutputDirectoryPath, entries, fulfilledAssets, invalidOutputs);
        ProcessAudioSlots(request.ProviderJobPlan.AudioSlots.Slots, request.OutputDirectoryPath, entries, fulfilledAudio, invalidOutputs);
        ProcessLuaSlots(request.ProviderJobPlan.LuaModuleSlots.Slots, request.OutputDirectoryPath, entries, fulfilledLua, invalidOutputs);

        foreach (var invalidOutput in invalidOutputs.Where(item => item.Reason is "is_directory" or "empty_file"))
        {
            diagnostics.Add(Error(
                "fulfillment_state.invalid_existing_output",
                $"Existing output '{invalidOutput.ExpectedOutputRelativePath}' is invalid: {invalidOutput.Reason}.",
                invalidOutput.SlotId));
        }

        var orderedDiagnostics = diagnostics
            .OrderBy(d => d.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(d => d.Code, StringComparer.Ordinal)
            .ThenBy(d => d.TargetId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(d => d.TargetId, StringComparer.Ordinal)
            .ThenBy(d => d.Message, StringComparer.OrdinalIgnoreCase)
            .ThenBy(d => d.Message, StringComparer.Ordinal)
            .ToList();

        return new UnityArchiveFulfillmentStateResult
        {
            FulfillmentState = new UnityArchiveFulfillmentStateReport
            {
                TotalSlotCount = entries.Count,
                MissingCount = entries.Count(e => e.Status == UnityArchiveFulfillmentStatus.missing),
                AvailableCount = entries.Count(e => e.Status == UnityArchiveFulfillmentStatus.available),
                InvalidCount = entries.Count(e => e.Status == UnityArchiveFulfillmentStatus.invalid),
                Entries = entries.OrderBy(e => e.SlotId, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(e => e.SlotId, StringComparer.Ordinal)
                    .ToList(),
                Diagnostics = orderedDiagnostics
            },
            FulfilledAssets = new UnityArchiveFulfilledAssetsIndex
            {
                Assets = fulfilledAssets.OrderBy(a => a.SlotId, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.SlotId, StringComparer.Ordinal)
                    .ToList()
            },
            FulfilledAudio = new UnityArchiveFulfilledAudioIndex
            {
                Audio = fulfilledAudio.OrderBy(a => a.SlotId, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.SlotId, StringComparer.Ordinal)
                    .ToList()
            },
            FulfilledLua = new UnityArchiveFulfilledLuaIndex
            {
                Lua = fulfilledLua.OrderBy(l => l.SlotId, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(l => l.SlotId, StringComparer.Ordinal)
                    .ToList()
            },
            InvalidOutputs = new UnityArchiveInvalidOutputsReport
            {
                InvalidOutputs = invalidOutputs.OrderBy(io => io.SlotId, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(io => io.SlotId, StringComparer.Ordinal)
                    .ToList()
            },
            Diagnostics = orderedDiagnostics
        };
    }

    private static void ProcessAssetSlots(
        IReadOnlyList<UnityArchiveAssetSlot> slots,
        string outputDirectory,
        List<UnityArchiveFulfillmentStateEntry> entries,
        List<UnityArchiveFulfilledAssetEntry> fulfilled,
        List<UnityArchiveInvalidOutputEntry> invalidOutputs)
    {
        foreach (var slot in slots)
        {
            var reportedPath = GetJsonSafeExpectedOutputRelativePath(slot.ExpectedOutputRelativePath);
            var status = CheckFileStatus(slot.ExpectedOutputRelativePath, ExpectedAssetExtension, outputDirectory);
            if (status == UnityArchiveFulfillmentStatus.available)
            {
                _ = TryGetContainedOutputPath(slot.ExpectedOutputRelativePath, outputDirectory, out var fullPath);
                var fileInfo = new FileInfo(fullPath);
                entries.Add(new UnityArchiveFulfillmentStateEntry
                {
                    SlotId = slot.SlotId,
                    RequestId = slot.RequestId,
                    ProviderKind = slot.ProviderKind,
                    ExpectedOutputRelativePath = reportedPath,
                    Status = status,
                    FileSizeBytes = fileInfo.Length
                });
                fulfilled.Add(new UnityArchiveFulfilledAssetEntry
                {
                    SlotId = slot.SlotId,
                    AssetId = slot.AssetId,
                    AssetKind = slot.AssetKind,
                    ExpectedOutputRelativePath = reportedPath,
                    FileSizeBytes = fileInfo.Length
                });
            }
            else
            {
                entries.Add(new UnityArchiveFulfillmentStateEntry
                {
                    SlotId = slot.SlotId,
                    RequestId = slot.RequestId,
                    ProviderKind = slot.ProviderKind,
                    ExpectedOutputRelativePath = reportedPath,
                    Status = status,
                    FileSizeBytes = 0
                });
                if (status == UnityArchiveFulfillmentStatus.invalid)
                {
                    invalidOutputs.Add(new UnityArchiveInvalidOutputEntry
                    {
                        SlotId = slot.SlotId,
                        ExpectedOutputRelativePath = reportedPath,
                        Reason = GetInvalidReason(slot.ExpectedOutputRelativePath, ExpectedAssetExtension, outputDirectory)
                    });
                }
            }
        }
    }

    private static void ProcessAudioSlots(
        IReadOnlyList<UnityArchiveAudioSlot> slots,
        string outputDirectory,
        List<UnityArchiveFulfillmentStateEntry> entries,
        List<UnityArchiveFulfilledAudioEntry> fulfilled,
        List<UnityArchiveInvalidOutputEntry> invalidOutputs)
    {
        foreach (var slot in slots)
        {
            var reportedPath = GetJsonSafeExpectedOutputRelativePath(slot.ExpectedOutputRelativePath);
            var status = CheckFileStatus(slot.ExpectedOutputRelativePath, ExpectedAudioExtension, outputDirectory);
            if (status == UnityArchiveFulfillmentStatus.available)
            {
                _ = TryGetContainedOutputPath(slot.ExpectedOutputRelativePath, outputDirectory, out var fullPath);
                var fileInfo = new FileInfo(fullPath);
                entries.Add(new UnityArchiveFulfillmentStateEntry
                {
                    SlotId = slot.SlotId,
                    RequestId = slot.RequestId,
                    ProviderKind = slot.ProviderKind,
                    ExpectedOutputRelativePath = reportedPath,
                    Status = status,
                    FileSizeBytes = fileInfo.Length
                });
                fulfilled.Add(new UnityArchiveFulfilledAudioEntry
                {
                    SlotId = slot.SlotId,
                    AudioId = slot.AudioId,
                    AudioKind = slot.AudioKind,
                    ExpectedOutputRelativePath = reportedPath,
                    FileSizeBytes = fileInfo.Length
                });
            }
            else
            {
                entries.Add(new UnityArchiveFulfillmentStateEntry
                {
                    SlotId = slot.SlotId,
                    RequestId = slot.RequestId,
                    ProviderKind = slot.ProviderKind,
                    ExpectedOutputRelativePath = reportedPath,
                    Status = status,
                    FileSizeBytes = 0
                });
                if (status == UnityArchiveFulfillmentStatus.invalid)
                {
                    invalidOutputs.Add(new UnityArchiveInvalidOutputEntry
                    {
                        SlotId = slot.SlotId,
                        ExpectedOutputRelativePath = reportedPath,
                        Reason = GetInvalidReason(slot.ExpectedOutputRelativePath, ExpectedAudioExtension, outputDirectory)
                    });
                }
            }
        }
    }

    private static void ProcessLuaSlots(
        IReadOnlyList<UnityArchiveLuaModuleSlot> slots,
        string outputDirectory,
        List<UnityArchiveFulfillmentStateEntry> entries,
        List<UnityArchiveFulfilledLuaEntry> fulfilled,
        List<UnityArchiveInvalidOutputEntry> invalidOutputs)
    {
        foreach (var slot in slots)
        {
            var reportedPath = GetJsonSafeExpectedOutputRelativePath(slot.ExpectedOutputRelativePath);
            var status = CheckFileStatus(slot.ExpectedOutputRelativePath, ExpectedLuaExtension, outputDirectory);
            if (status == UnityArchiveFulfillmentStatus.available)
            {
                _ = TryGetContainedOutputPath(slot.ExpectedOutputRelativePath, outputDirectory, out var fullPath);
                var fileInfo = new FileInfo(fullPath);
                entries.Add(new UnityArchiveFulfillmentStateEntry
                {
                    SlotId = slot.SlotId,
                    RequestId = slot.ModuleId,
                    ProviderKind = slot.ProviderKind,
                    ExpectedOutputRelativePath = reportedPath,
                    Status = status,
                    FileSizeBytes = fileInfo.Length
                });
                fulfilled.Add(new UnityArchiveFulfilledLuaEntry
                {
                    SlotId = slot.SlotId,
                    ModuleId = slot.ModuleId,
                    ModuleKind = slot.ModuleKind,
                    ExpectedOutputRelativePath = reportedPath,
                    FileSizeBytes = fileInfo.Length
                });
            }
            else
            {
                entries.Add(new UnityArchiveFulfillmentStateEntry
                {
                    SlotId = slot.SlotId,
                    RequestId = slot.ModuleId,
                    ProviderKind = slot.ProviderKind,
                    ExpectedOutputRelativePath = reportedPath,
                    Status = status,
                    FileSizeBytes = 0
                });
                if (status == UnityArchiveFulfillmentStatus.invalid)
                {
                    invalidOutputs.Add(new UnityArchiveInvalidOutputEntry
                    {
                        SlotId = slot.SlotId,
                        ExpectedOutputRelativePath = reportedPath,
                        Reason = GetInvalidReason(slot.ExpectedOutputRelativePath, ExpectedLuaExtension, outputDirectory)
                    });
                }
            }
        }
    }

    private static UnityArchiveFulfillmentStatus CheckFileStatus(string relativePath, string expectedExt, string outputDirectory)
    {
        if (!TryGetContainedOutputPath(relativePath, outputDirectory, out var fullPath))
        {
            return UnityArchiveFulfillmentStatus.invalid;
        }

        if (!relativePath.EndsWith(expectedExt, StringComparison.OrdinalIgnoreCase))
        {
            return UnityArchiveFulfillmentStatus.invalid;
        }

        if (Directory.Exists(fullPath))
        {
            return UnityArchiveFulfillmentStatus.invalid;
        }

        if (!File.Exists(fullPath))
        {
            return UnityArchiveFulfillmentStatus.missing;
        }

        var fileInfo = new FileInfo(fullPath);
        if (fileInfo.Length == 0)
        {
            return UnityArchiveFulfillmentStatus.invalid;
        }

        return UnityArchiveFulfillmentStatus.available;
    }

    private static string GetInvalidReason(string relativePath, string expectedExt, string outputDirectory)
    {
        if (!TryGetContainedOutputPath(relativePath, outputDirectory, out var fullPath))
        {
            return "unsafe_path";
        }

        if (!relativePath.EndsWith(expectedExt, StringComparison.OrdinalIgnoreCase))
        {
            return "wrong_extension";
        }

        if (Directory.Exists(fullPath))
        {
            return "is_directory";
        }

        var fileInfo = new FileInfo(fullPath);
        if (fileInfo.Length == 0)
        {
            return "empty_file";
        }

        return "unknown";
    }

    private static bool TryGetContainedOutputPath(string relativePath, string outputDirectory, out string fullPath)
    {
        fullPath = string.Empty;
        if (!UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(relativePath))
        {
            return false;
        }

        try
        {
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(outputDirectory));
            var candidate = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) ||
                !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            fullPath = candidate;
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static string GetJsonSafeExpectedOutputRelativePath(string relativePath)
    {
        return UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(relativePath)
            ? relativePath
            : string.Empty;
    }

    private static IReadOnlyList<UnityArchiveFulfillmentStateDiagnostic> Validate(UnityArchiveProviderJobPlanResult plan)
    {
        var diagnostics = new List<UnityArchiveFulfillmentStateDiagnostic>();

        var slotIds = plan.AssetSlots.Slots.Select(s => s.SlotId)
            .Concat(plan.AudioSlots.Slots.Select(s => s.SlotId))
            .Concat(plan.LuaModuleSlots.Slots.Select(s => s.SlotId));
        AddDuplicates(slotIds, "fulfillment_state.duplicate_slot_id", diagnostics);

        var expectedPaths = plan.AssetSlots.Slots.Select(s => s.ExpectedOutputRelativePath)
            .Concat(plan.AudioSlots.Slots.Select(s => s.ExpectedOutputRelativePath))
            .Concat(plan.LuaModuleSlots.Slots.Select(s => s.ExpectedOutputRelativePath));
        AddDuplicates(
            expectedPaths.Where(UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath),
            "fulfillment_state.duplicate_expected_output_path",
            diagnostics);

        foreach (var slot in plan.AssetSlots.Slots.Where(s => !s.ExpectedOutputRelativePath.EndsWith(ExpectedAssetExtension, StringComparison.OrdinalIgnoreCase)))
        {
            diagnostics.Add(Error("fulfillment_state.wrong_extension", $"Expected output path must end with {ExpectedAssetExtension}", slot.SlotId));
        }

        foreach (var slot in plan.AudioSlots.Slots.Where(s => !s.ExpectedOutputRelativePath.EndsWith(ExpectedAudioExtension, StringComparison.OrdinalIgnoreCase)))
        {
            diagnostics.Add(Error("fulfillment_state.wrong_extension", $"Expected output path must end with {ExpectedAudioExtension}", slot.SlotId));
        }

        foreach (var slot in plan.LuaModuleSlots.Slots.Where(s => !s.ExpectedOutputRelativePath.EndsWith(ExpectedLuaExtension, StringComparison.OrdinalIgnoreCase)))
        {
            diagnostics.Add(Error("fulfillment_state.wrong_extension", $"Expected output path must end with {ExpectedLuaExtension}", slot.SlotId));
        }

        foreach (var slot in plan.AssetSlots.Slots.Where(s => !UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(s.ExpectedOutputRelativePath)))
        {
            diagnostics.Add(Error("fulfillment_state.unsafe_expected_output_path", "Unsafe expected output path.", slot.SlotId));
        }
        foreach (var slot in plan.AudioSlots.Slots.Where(s => !UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(s.ExpectedOutputRelativePath)))
        {
            diagnostics.Add(Error("fulfillment_state.unsafe_expected_output_path", "Unsafe expected output path.", slot.SlotId));
        }
        foreach (var slot in plan.LuaModuleSlots.Slots.Where(s => !UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(s.ExpectedOutputRelativePath)))
        {
            diagnostics.Add(Error("fulfillment_state.unsafe_expected_output_path", "Unsafe expected output path.", slot.SlotId));
        }

        return diagnostics;
    }

    private static void AddDuplicates(IEnumerable<string> values, string code, ICollection<UnityArchiveFulfillmentStateDiagnostic> diagnostics)
    {
        foreach (var value in values.GroupBy(item => item, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1)
                     .Select(group => group.Key)
                     .OrderBy(item => item, StringComparer.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error(code, $"Duplicate {code.Split('.')[1].Replace("_", " ")} '{value}'.", value));
        }
    }

    private static UnityArchiveFulfillmentStateDiagnostic Error(string code, string message, string targetId) =>
        new() { Severity = UnityArchiveExportDiagnosticSeverity.Error, Code = code, Message = message, TargetId = targetId };
}
