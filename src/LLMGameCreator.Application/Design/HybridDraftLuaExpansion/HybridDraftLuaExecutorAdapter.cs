using Lua;

namespace LLMGameCreator.Application.Design.HybridDraftLuaExpansion;

public sealed class HybridDraftLuaExecutorAdapter
{
    public async Task<HybridExecutorAdapterResult> ExecuteAsync(
        HybridDraftLuaExpansionRequest request,
        HybridLuaFixture fixture,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(fixture);

        var diagnostics = HybridDraftLuaExpansionCatalog
            .ValidateRequest(request)
            .Concat(HybridDraftLuaExpansionCatalog.ValidateFixture(fixture))
            .ToList();

        if (request.FixtureId != fixture.FixtureId)
        {
            diagnostics.Add(HybridDraftLuaExpansionCatalog.Diagnostic("error", "hybrid.fixture.mismatch", fixture.FixtureId, "Fixture id does not match request."));
        }

        if (request.ScenarioId != fixture.ScenarioId || request.ProducedArtifactFamily != fixture.ProducedArtifactFamily)
        {
            diagnostics.Add(HybridDraftLuaExpansionCatalog.Diagnostic("error", "hybrid.fixture.scenario_family_mismatch", fixture.FixtureId, "Fixture scenario/family does not match request."));
        }

        var sortedPreflight = HybridDraftLuaExpansionCatalog.SortDiagnostics(diagnostics);
        if (sortedPreflight.Any(item => item.Severity == "error"))
        {
            return new HybridExecutorAdapterResult
            {
                ExecutionRequestId = request.ExecutionRequestId,
                ScenarioId = request.ScenarioId,
                FixtureId = fixture.FixtureId,
                Status = "rejected",
                LuaExecuted = false,
                Diagnostics = sortedPreflight
            };
        }

        try
        {
            var state = LuaState.Create();
            var results = await state.DoStringAsync(
                fixture.ScriptText,
                chunkName: fixture.FixtureId,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (results.Length == 0)
            {
                return Rejected(request, fixture, "hybrid.executor.empty_result", "Lua fixture returned no values.");
            }

            var root = results[0].Read<LuaTable>();
            var output = HybridDraftLuaExpansionCatalog.BuildOutputFromFixture(
                request,
                ReadString(root, "stableId"),
                ReadSlots(root, "slots", request.OutputBudget),
                ReadTags(root, "tags"),
                ReadRelations(root, "relations"),
                ReadDiagnostics(root, "diagnostics"));
            var validation = HybridDraftLuaExpansionCatalog.ValidateOutput(request, output);
            var status = validation.Any(item => item.Severity == "error") ? "rejected" : "accepted";

            return new HybridExecutorAdapterResult
            {
                ExecutionRequestId = request.ExecutionRequestId,
                ScenarioId = request.ScenarioId,
                FixtureId = fixture.FixtureId,
                Status = status,
                LuaExecuted = true,
                Output = output,
                Diagnostics = validation
            };
        }
        catch (Exception ex) when (ex is LuaParseException
                                       or LuaCompileException
                                       or LuaRuntimeException
                                       or LuaAssertionException
                                       or LuaCanceledException
                                       or LuaModuleNotFoundException
                                       or InvalidOperationException
                                       or ArgumentException
                                   || ex.GetType().Namespace == "Lua")
        {
            return Rejected(request, fixture, "hybrid.executor.exception", ex.GetType().Name);
        }
    }

    private static HybridExecutorAdapterResult Rejected(
        HybridDraftLuaExpansionRequest request,
        HybridLuaFixture fixture,
        string code,
        string message) =>
        new()
        {
            ExecutionRequestId = request.ExecutionRequestId,
            ScenarioId = request.ScenarioId,
            FixtureId = fixture.FixtureId,
            Status = "rejected",
            LuaExecuted = false,
            Diagnostics =
            [
                HybridDraftLuaExpansionCatalog.Diagnostic("error", code, request.ExecutionRequestId, message)
            ]
        };

    private static IReadOnlyList<HybridExpansionSlot> ReadSlots(LuaTable root, string key, int budget)
    {
        var table = root[key].Read<LuaTable>();
        var items = new List<HybridExpansionSlot>();
        for (var index = 1; index <= budget + 1; index++)
        {
            var value = table[index];
            if (value.Type == LuaValueType.Nil)
            {
                break;
            }

            var item = value.Read<LuaTable>();
            items.Add(new HybridExpansionSlot
            {
                SlotId = ReadString(item, "id"),
                SlotKind = ReadString(item, "kind"),
                Weight = ReadInt(item, "weight"),
                Tags = ReadStringArray(item, "tags"),
                RelationIds = ReadStringArray(item, "relations")
            });
        }

        return items.OrderBy(item => item.SlotId, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<HybridWeightedTag> ReadTags(LuaTable root, string key)
    {
        var table = root[key].Read<LuaTable>();
        var items = new List<HybridWeightedTag>();
        for (var index = 1; ; index++)
        {
            var value = table[index];
            if (value.Type == LuaValueType.Nil)
            {
                break;
            }

            var item = value.Read<LuaTable>();
            items.Add(new HybridWeightedTag
            {
                TagId = ReadString(item, "id"),
                Weight = ReadInt(item, "weight")
            });
        }

        return items.OrderBy(item => item.TagId, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<HybridExpansionRelation> ReadRelations(LuaTable root, string key)
    {
        var table = root[key].Read<LuaTable>();
        var items = new List<HybridExpansionRelation>();
        for (var index = 1; ; index++)
        {
            var value = table[index];
            if (value.Type == LuaValueType.Nil)
            {
                break;
            }

            var item = value.Read<LuaTable>();
            items.Add(new HybridExpansionRelation
            {
                RelationId = ReadString(item, "id"),
                SourceId = ReadString(item, "source"),
                TargetId = ReadString(item, "target"),
                RelationKind = ReadString(item, "kind")
            });
        }

        return items.OrderBy(item => item.RelationId, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<HybridDraftLuaDiagnostic> ReadDiagnostics(LuaTable root, string key)
    {
        var table = root[key].Read<LuaTable>();
        var items = new List<HybridDraftLuaDiagnostic>();
        for (var index = 1; ; index++)
        {
            var value = table[index];
            if (value.Type == LuaValueType.Nil)
            {
                break;
            }

            var item = value.Read<LuaTable>();
            items.Add(new HybridDraftLuaDiagnostic
            {
                Severity = ReadString(item, "severity"),
                Code = ReadString(item, "code"),
                Target = ReadString(item, "target"),
                Message = ReadString(item, "message")
            });
        }

        return HybridDraftLuaExpansionCatalog.SortDiagnostics(items);
    }

    private static IReadOnlyList<string> ReadStringArray(LuaTable root, string key)
    {
        var table = root[key].Read<LuaTable>();
        var items = new List<string>();
        for (var index = 1; ; index++)
        {
            var value = table[index];
            if (value.Type == LuaValueType.Nil)
            {
                break;
            }

            items.Add(value.Read<string>());
        }

        return items.Order(StringComparer.Ordinal).ToList();
    }

    private static string ReadString(LuaTable table, string key) => table[key].Read<string>();

    private static int ReadInt(LuaTable table, string key) => table[key].Read<int>();
}
