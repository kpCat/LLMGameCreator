namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveAssetAudioLuaRequestService
{
    public UnityArchiveRequestPipelineResult BuildRequests(UnityArchiveRequestPipelineRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.DesignBrief);
        ArgumentNullException.ThrowIfNull(request.TargetProfile);
        ArgumentNullException.ThrowIfNull(request.ArchiveManifest);
        if (string.IsNullOrWhiteSpace(request.ProjectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(request));
        }

        var context = new UnityArchiveRequestBuildContext(request);
        var diagnostics = new List<UnityArchiveRequestDiagnostic>();

        // Build asset requests
        var assetBuilder = new UnityArchiveAssetRequestBuilder(context);
        var (assetRequests, assetDiagnostics) = assetBuilder.Build();
        diagnostics.AddRange(assetDiagnostics);

        // Build audio requests
        var audioBuilder = new UnityArchiveAudioRequestBuilder(context);
        var audioRequests = audioBuilder.Build();

        // Build Lua module requests
        var luaBuilder = new UnityArchiveLuaModuleRequestBuilder(context);
        var (luaModuleRequests, luaDiagnostics) = luaBuilder.Build();
        diagnostics.AddRange(luaDiagnostics);

        // Validate duplicate IDs
        ValidateAssetIds(assetRequests, diagnostics);
        ValidateAudioIds(audioRequests, diagnostics);
        ValidateLuaIds(luaModuleRequests, diagnostics);

        // Aggregate future provider warnings for assets
        var assetFutureProviderGroups = CountFutureProviders(assetRequests);
        foreach (var provider in assetFutureProviderGroups.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase).ThenBy(key => key, StringComparer.Ordinal))
        {
            diagnostics.Add(UnityArchiveRequestDiagnosticsBuilder.Warning(
                $"request.diagnostic.future_provider_kind.asset.{provider.ToLowerInvariant()}",
                $"Asset requests use future provider '{provider}' for {assetFutureProviderGroups[provider]} request(s).",
                "asset_requests"));
        }

        // Aggregate future provider warnings for audio
        var audioFutureProviderGroups = CountFutureProviders(audioRequests);
        foreach (var provider in audioFutureProviderGroups.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase).ThenBy(key => key, StringComparer.Ordinal))
        {
            diagnostics.Add(UnityArchiveRequestDiagnosticsBuilder.Warning(
                $"request.diagnostic.future_provider_kind.audio.{provider.ToLowerInvariant()}",
                $"Audio requests use future provider '{provider}' for {audioFutureProviderGroups[provider]} request(s).",
                "audio_requests"));
        }

        // Sort deterministically
        assetRequests.Sort((a, b) => UnityArchiveRequestDiagnosticsBuilder.CompareRequests(a.RequestId, b.RequestId));
        audioRequests.Sort((a, b) => UnityArchiveRequestDiagnosticsBuilder.CompareRequests(a.RequestId, b.RequestId));
        luaModuleRequests.Sort((a, b) => UnityArchiveRequestDiagnosticsBuilder.CompareRequests(a.ModuleId, b.ModuleId));

        // Calculate readiness
        var hasErrors = diagnostics.Any(d => d.Severity == UnityArchiveExportDiagnosticSeverity.Error);
        var hasWarnings = diagnostics.Any(d => d.Severity == UnityArchiveExportDiagnosticSeverity.Warning);
        var readiness = hasErrors
            ? UnityArchiveRequestReadiness.BlockedByErrors
            : hasWarnings
                ? UnityArchiveRequestReadiness.ReadyWithWarnings
                : UnityArchiveRequestReadiness.Ready;

        return new UnityArchiveRequestPipelineResult
        {
            AssetRequests = assetRequests,
            AudioRequests = audioRequests,
            LuaModuleRequests = luaModuleRequests,
            Diagnostics = diagnostics.OrderBy(d => UnityArchiveRequestDiagnosticsBuilder.SeverityOrder(d.Severity))
                .ThenBy(d => d.Code, StringComparer.OrdinalIgnoreCase)
                .ThenBy(d => d.TargetId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(d => d.Message, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Readiness = readiness
        };
    }

    private static void ValidateAssetIds(IReadOnlyList<UnityArchiveAssetRequest> assetRequests, List<UnityArchiveRequestDiagnostic> diagnostics)
    {
        foreach (var duplicateId in assetRequests
                     .GroupBy(r => r.RequestId, StringComparer.OrdinalIgnoreCase)
                     .Where(g => g.Count() > 1)
                     .Select(g => g.Key)
                     .OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
        {
            diagnostics.Add(UnityArchiveRequestDiagnosticsBuilder.Error("request.diagnostic.duplicate_asset_request_id", $"Duplicate asset request id '{duplicateId}'.", duplicateId));
        }
    }

    private static void ValidateAudioIds(IReadOnlyList<UnityArchiveAudioRequest> audioRequests, List<UnityArchiveRequestDiagnostic> diagnostics)
    {
        foreach (var duplicateId in audioRequests
                     .GroupBy(r => r.RequestId, StringComparer.OrdinalIgnoreCase)
                     .Where(g => g.Count() > 1)
                     .Select(g => g.Key)
                     .OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
        {
            diagnostics.Add(UnityArchiveRequestDiagnosticsBuilder.Error("request.diagnostic.duplicate_audio_request_id", $"Duplicate audio request id '{duplicateId}'.", duplicateId));
        }
    }

    private static void ValidateLuaIds(IReadOnlyList<UnityArchiveLuaModuleRequest> luaModuleRequests, List<UnityArchiveRequestDiagnostic> diagnostics)
    {
        foreach (var duplicateId in luaModuleRequests
                     .GroupBy(r => r.ModuleId, StringComparer.OrdinalIgnoreCase)
                     .Where(g => g.Count() > 1)
                     .Select(g => g.Key)
                     .OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
        {
            diagnostics.Add(UnityArchiveRequestDiagnosticsBuilder.Error("request.diagnostic.duplicate_lua_request_id", $"Duplicate Lua module request id '{duplicateId}'.", duplicateId));
        }
    }

    private static Dictionary<string, int> CountFutureProviders(IReadOnlyList<UnityArchiveAssetRequest> requests)
    {
        var groups = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var req in requests)
        {
            if (!UnityArchiveRequestDiagnosticsBuilder.IsFutureProvider(req.ProviderKind))
            {
                continue;
            }

            var provider = req.ProviderKind.ToString();
            if (groups.ContainsKey(provider))
            {
                groups[provider]++;
            }
            else
            {
                groups[provider] = 1;
            }
        }
        return groups;
    }

    private static Dictionary<string, int> CountFutureProviders(IReadOnlyList<UnityArchiveAudioRequest> requests)
    {
        var groups = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var req in requests)
        {
            if (!UnityArchiveRequestDiagnosticsBuilder.IsFutureProvider(req.ProviderKind))
            {
                continue;
            }

            var provider = req.ProviderKind.ToString();
            if (groups.ContainsKey(provider))
            {
                groups[provider]++;
            }
            else
            {
                groups[provider] = 1;
            }
        }
        return groups;
    }
}