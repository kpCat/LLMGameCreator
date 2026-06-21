namespace LLMGameCreator.Application.Composition;

public sealed class UnityTargetContractValidator
{
    public UnityTargetContractValidationResult ValidateTargetProfile(
        UnityTargetProfile profile,
        IReadOnlyList<UnityRuntimeModuleContract> runtimeModules)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(runtimeModules);

        var diagnostics = ValidateRuntimeModuleCatalog(runtimeModules);
        if (string.IsNullOrWhiteSpace(profile.TargetProfileId))
        {
            diagnostics.Add(Diagnostic(
                UnityTargetContractDiagnosticSeverity.Error,
                UnityTargetContractDiagnosticCodes.BlankId,
                "Unity target profile id must not be blank.",
                "target_profile"));
        }

        AddRuntimeModuleReferenceDiagnostics(
            profile.RequiredRuntimeModuleIds.Concat(profile.OptionalRuntimeModuleIds),
            runtimeModules,
            profile.TargetProfileId,
            diagnostics);

        return Result(diagnostics);
    }

    public UnityTargetContractValidationResult ValidateArchive(
        UnityGameArchiveManifest archive,
        IReadOnlyList<UnityTargetProfile> targetProfiles,
        IReadOnlyList<UnityRuntimeModuleContract> runtimeModules)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(targetProfiles);
        ArgumentNullException.ThrowIfNull(runtimeModules);

        var diagnostics = ValidateRuntimeModuleCatalog(runtimeModules);
        if (string.IsNullOrWhiteSpace(archive.GameId))
        {
            diagnostics.Add(Diagnostic(
                UnityTargetContractDiagnosticSeverity.Error,
                UnityTargetContractDiagnosticCodes.BlankId,
                "Unity archive game id must not be blank.",
                "archive"));
        }
        else if (!IsSafeArchiveId(archive.GameId))
        {
            diagnostics.Add(Diagnostic(
                UnityTargetContractDiagnosticSeverity.Error,
                UnityTargetContractDiagnosticCodes.UnsafeArchiveId,
                $"Unity archive game id '{archive.GameId}' is not filename-safe ASCII.",
                archive.GameId));
        }

        var targetProfile = targetProfiles.FirstOrDefault(profile =>
            string.Equals(profile.TargetProfileId, archive.TargetProfileId, StringComparison.OrdinalIgnoreCase));
        if (targetProfile is null)
        {
            diagnostics.Add(Diagnostic(
                UnityTargetContractDiagnosticSeverity.Error,
                UnityTargetContractDiagnosticCodes.UnknownTargetProfile,
                $"Unity archive references unknown target profile '{archive.TargetProfileId}'.",
                archive.GameId,
                archive.TargetProfileId));
        }
        else
        {
            diagnostics.AddRange(ValidateTargetProfile(targetProfile, runtimeModules).Diagnostics);
        }

        AddRuntimeModuleReferenceDiagnostics(
            archive.RuntimeModuleIds,
            runtimeModules,
            archive.GameId,
            diagnostics);
        AddUiDiagnostics(archive, diagnostics);
        AddRequestDiagnostics(archive, diagnostics);
        AddLargeWorldDiagnostics(archive, diagnostics);

        return Result(diagnostics);
    }

    private static List<UnityTargetContractDiagnostic> ValidateRuntimeModuleCatalog(
        IReadOnlyList<UnityRuntimeModuleContract> runtimeModules)
    {
        var diagnostics = new List<UnityTargetContractDiagnostic>();
        foreach (var module in runtimeModules.Where(module => string.IsNullOrWhiteSpace(module.ModuleId)))
        {
            diagnostics.Add(Diagnostic(
                UnityTargetContractDiagnosticSeverity.Error,
                UnityTargetContractDiagnosticCodes.BlankId,
                "Unity runtime module id must not be blank.",
                "runtime_module"));
        }

        foreach (var duplicateId in runtimeModules
                     .Where(module => !string.IsNullOrWhiteSpace(module.ModuleId))
                     .GroupBy(module => module.ModuleId.Trim(), StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1)
                     .Select(group => group.Key)
                     .OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
        {
            diagnostics.Add(Diagnostic(
                UnityTargetContractDiagnosticSeverity.Error,
                UnityTargetContractDiagnosticCodes.DuplicateRuntimeModuleId,
                $"Unity runtime module id '{duplicateId}' is duplicated.",
                duplicateId));
        }

        return diagnostics;
    }

    private static void AddRuntimeModuleReferenceDiagnostics(
        IEnumerable<string> moduleIds,
        IReadOnlyList<UnityRuntimeModuleContract> runtimeModules,
        string targetId,
        ICollection<UnityTargetContractDiagnostic> diagnostics)
    {
        var modulesById = runtimeModules
            .Where(module => !string.IsNullOrWhiteSpace(module.ModuleId))
            .GroupBy(module => module.ModuleId.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var moduleId in Normalize(moduleIds))
        {
            if (!modulesById.TryGetValue(moduleId, out var module))
            {
                diagnostics.Add(Diagnostic(
                    UnityTargetContractDiagnosticSeverity.Error,
                    UnityTargetContractDiagnosticCodes.UnknownRuntimeModule,
                    $"Unity target references unknown runtime module '{moduleId}'.",
                    targetId,
                    moduleId));
                continue;
            }

            if (module.Maturity == UnityContractMaturity.PlannedFuture)
            {
                diagnostics.Add(Diagnostic(
                    UnityTargetContractDiagnosticSeverity.Warning,
                    UnityTargetContractDiagnosticCodes.FutureRuntimeModule,
                    $"Unity target requests planned future runtime module '{moduleId}'.",
                    targetId,
                    moduleId));
            }
        }
    }

    private static void AddUiDiagnostics(
        UnityGameArchiveManifest archive,
        ICollection<UnityTargetContractDiagnostic> diagnostics)
    {
        foreach (var layout in archive.UiLayouts)
        {
            if (string.IsNullOrWhiteSpace(layout.LayoutId))
            {
                diagnostics.Add(Diagnostic(
                    UnityTargetContractDiagnosticSeverity.Error,
                    UnityTargetContractDiagnosticCodes.BlankId,
                    "Unity UI layout id must not be blank.",
                    archive.GameId));
            }

            foreach (var binding in layout.Bindings.Where(binding => string.IsNullOrWhiteSpace(binding.SourcePath)))
            {
                diagnostics.Add(Diagnostic(
                    UnityTargetContractDiagnosticSeverity.Error,
                    UnityTargetContractDiagnosticCodes.BlankUiBindingPath,
                    $"Unity UI binding '{binding.BindingId}' must have a source path.",
                    layout.LayoutId,
                    binding.BindingId));
            }
        }
    }

    private static void AddRequestDiagnostics(
        UnityGameArchiveManifest archive,
        ICollection<UnityTargetContractDiagnostic> diagnostics)
    {
        AddDuplicateRequestDiagnostics(
            archive.AssetRequests.Select(request => request.RequestId),
            UnityTargetContractDiagnosticCodes.DuplicateAssetRequestId,
            "asset",
            archive.GameId,
            diagnostics);
        AddDuplicateRequestDiagnostics(
            archive.AudioRequests.Select(request => request.RequestId),
            UnityTargetContractDiagnosticCodes.DuplicateAudioRequestId,
            "audio",
            archive.GameId,
            diagnostics);
    }

    private static void AddDuplicateRequestDiagnostics(
        IEnumerable<string> requestIds,
        string code,
        string requestKind,
        string targetId,
        ICollection<UnityTargetContractDiagnostic> diagnostics)
    {
        foreach (var requestId in requestIds.Where(string.IsNullOrWhiteSpace))
        {
            diagnostics.Add(Diagnostic(
                UnityTargetContractDiagnosticSeverity.Error,
                UnityTargetContractDiagnosticCodes.BlankId,
                $"Unity {requestKind} request id must not be blank.",
                targetId));
        }

        foreach (var duplicateId in requestIds
                     .Where(id => !string.IsNullOrWhiteSpace(id))
                     .GroupBy(id => id.Trim(), StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1)
                     .Select(group => group.Key)
                     .OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
        {
            diagnostics.Add(Diagnostic(
                UnityTargetContractDiagnosticSeverity.Error,
                code,
                $"Unity {requestKind} request id '{duplicateId}' is duplicated.",
                targetId,
                duplicateId));
        }
    }

    private static void AddLargeWorldDiagnostics(
        UnityGameArchiveManifest archive,
        ICollection<UnityTargetContractDiagnostic> diagnostics)
    {
        var policy = archive.WorldStreamingPolicy;
        if (policy.WorldScale is not (UnityWorldScale.Large or UnityWorldScale.Infinite))
        {
            return;
        }

        var consistent = policy.StoreSeedRulesAndTemplates &&
                         policy.MaterializeActiveChunksOnly &&
                         policy.PersistDirtyDeltas &&
                         policy.GenerateNpcsLazily &&
                         policy.GenerateQuestsLazily &&
                         policy.SeparateAuthoredAndGeneratedPopulation &&
                         policy.ActiveNpcBudget > 0 &&
                         policy.ChunkSize > 0 &&
                         policy.ActiveRadius >= 0 &&
                         policy.NpcMaterializationPolicy == UnityMaterializationPolicy.AuthoredImportantAndLazyGenerated &&
                         policy.QuestMaterializationPolicy == UnityMaterializationPolicy.LazyOnDemand;
        if (!consistent)
        {
            diagnostics.Add(Diagnostic(
                UnityTargetContractDiagnosticSeverity.Error,
                UnityTargetContractDiagnosticCodes.InconsistentLargeWorldStreaming,
                "Large Unity worlds must store seed/rules/templates, materialize active chunks only, persist dirty deltas, lazily generate NPCs/quests, cap active NPCs and separate authored from generated population.",
                archive.GameId));
        }
    }

    private static UnityTargetContractValidationResult Result(
        IEnumerable<UnityTargetContractDiagnostic> diagnostics)
    {
        var ordered = diagnostics
            .Distinct()
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.TargetId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.RelatedId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new UnityTargetContractValidationResult
        {
            Ok = ordered.All(diagnostic => diagnostic.Severity != UnityTargetContractDiagnosticSeverity.Error),
            Diagnostics = ordered
        };
    }

    private static bool IsSafeArchiveId(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length > 0 &&
               !trimmed.Contains("..", StringComparison.Ordinal) &&
               trimmed.All(character =>
                   character is >= 'a' and <= 'z' or
                   >= 'A' and <= 'Z' or
                   >= '0' and <= '9' or
                   '-' or '_' or '.');
    }

    private static IReadOnlyList<string> Normalize(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static UnityTargetContractDiagnostic Diagnostic(
        UnityTargetContractDiagnosticSeverity severity,
        string code,
        string message,
        string targetId,
        string relatedId = "")
    {
        return new UnityTargetContractDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            TargetId = targetId,
            RelatedId = relatedId
        };
    }

    private static int SeverityOrder(UnityTargetContractDiagnosticSeverity severity)
    {
        return severity switch
        {
            UnityTargetContractDiagnosticSeverity.Error => 0,
            UnityTargetContractDiagnosticSeverity.Warning => 1,
            _ => 2
        };
    }
}
