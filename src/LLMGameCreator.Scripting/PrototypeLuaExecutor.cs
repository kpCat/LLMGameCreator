using System.Diagnostics;
using System.Text.Json.Nodes;
using MoonSharp.Interpreter;

namespace LLMGameCreator.Scripting;

public sealed class PrototypeLuaExecutor : IPrototypeLuaExecutor
{
    private static readonly HashSet<string> SupportedDeclarationTypes = new(StringComparer.Ordinal)
    {
        "tile",
        "map",
        "entity_prototype",
        "manifest_update",
        "item",
        "resource",
        "status",
        "recipe",
        "loot_table",
        "transaction",
        "resource_network",
        "resource_node",
        "inventory",
        "equipment_slot",
        "stat",
        "progression",
        "encounter",
        "ability",
        "quest",
        "dialogue",
        "faction"
    };

    private readonly PrototypeLuaStaticAnalyzer _staticAnalyzer;
    private readonly PrototypeLuaSandboxOptions _options;

    public PrototypeLuaExecutor(PrototypeLuaStaticAnalyzer staticAnalyzer)
        : this(staticAnalyzer, new PrototypeLuaSandboxOptions())
    {
    }

    public PrototypeLuaExecutor(PrototypeLuaStaticAnalyzer staticAnalyzer, PrototypeLuaSandboxOptions options)
    {
        _staticAnalyzer = staticAnalyzer;
        _options = options;
    }

    public Task<PrototypeLuaExecutionResult> ExecuteAsync(PrototypeLuaExecutionRequest request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new PrototypeLuaExecutionResult();
        var target = string.IsNullOrWhiteSpace(request.SourcePath) ? request.ScriptId : request.SourcePath!;

        if (string.IsNullOrWhiteSpace(request.Source))
        {
            result.Diagnostics.Add(Error("lua.prototype.source.empty", "Prototype Lua source is required.", target));
            result.ElapsedMs = stopwatch.ElapsedMilliseconds;
            return Task.FromResult(result);
        }

        result.Diagnostics.AddRange(_staticAnalyzer.Analyze(request.Source, target));
        if (result.Diagnostics.Any(IsError))
        {
            result.ElapsedMs = stopwatch.ElapsedMilliseconds;
            return Task.FromResult(result);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var maxDeclarations = request.MaxDeclarations.GetValueOrDefault(_options.DefaultMaxDeclarations);
            var declarations = new List<PrototypeLuaDeclaration>();
            var script = new Script(CoreModules.None);
            script.Globals["data"] = BuildDataApi(script, declarations, maxDeclarations);
            script.Globals["llmgc"] = BuildLlmgcTable(script);

            script.DoString(request.Source, codeFriendlyName: target);
            result.Declarations = declarations;
            result.Success = !result.Diagnostics.Any(IsError);
        }
        catch (ScriptRuntimeException ex)
        {
            result.Diagnostics.Add(Error("lua.prototype.runtime_error", ex.DecoratedMessage ?? ex.Message, target));
        }
        catch (SyntaxErrorException ex)
        {
            result.Diagnostics.Add(Error("lua.prototype.syntax_error", ex.DecoratedMessage ?? ex.Message, target));
        }
        catch (TimeoutException ex)
        {
            result.Diagnostics.Add(Error("lua.prototype.timeout", ex.Message, target));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            result.Diagnostics.Add(Error("lua.prototype.cancelled", "Prototype Lua execution was cancelled.", target));
        }
        catch (Exception ex)
        {
            result.Diagnostics.Add(Error("lua.prototype.execution_failed", ex.Message, target));
        }
        finally
        {
            result.ElapsedMs = stopwatch.ElapsedMilliseconds;
            result.Success = result.Success && !result.Diagnostics.Any(IsError);
        }

        return Task.FromResult(result);
    }

    private Table BuildDataApi(Script script, List<PrototypeLuaDeclaration> declarations, int maxDeclarations)
    {
        var data = new Table(script);
        data["extend"] = DynValue.NewCallback((_, args) =>
        {
            var declarationsArgument = args.Count > 1 && args[0].Type == DataType.Table ? args[1] : args[0];
            if (declarationsArgument.Type != DataType.Table)
            {
                throw new ScriptRuntimeException("data:extend expects a table of declarations.");
            }

            CaptureDeclarations(declarationsArgument.Table, declarations, maxDeclarations);
            return DynValue.Nil;
        });
        return data;
    }

    private static Table BuildLlmgcTable(Script script)
    {
        var llmgc = new Table(script);
        llmgc["version"] = "0.1";
        return llmgc;
    }

    private static void CaptureDeclarations(Table table, List<PrototypeLuaDeclaration> declarations, int maxDeclarations)
    {
        foreach (var pair in table.Pairs.OrderBy(pair => NumericKeyOrMax(pair.Key)))
        {
            if (pair.Value.Type != DataType.Table)
            {
                throw new ScriptRuntimeException("data:extend entries must be tables.");
            }

            if (declarations.Count >= maxDeclarations)
            {
                throw new ScriptRuntimeException($"Prototype Lua declaration limit exceeded: {maxDeclarations}.");
            }

            var json = ToJsonObject(pair.Value.Table);
            var type = ReadRequiredString(json, "type", declarations.Count);
            if (!SupportedDeclarationTypes.Contains(type))
            {
                throw new ScriptRuntimeException($"Unsupported Prototype Lua declaration type: {type}.");
            }

            declarations.Add(new PrototypeLuaDeclaration
            {
                Type = type,
                Id = ReadOptionalString(json, "id"),
                Json = json,
                SourceIndex = declarations.Count
            });
        }
    }

    private static JsonObject ToJsonObject(Table table)
    {
        var json = new JsonObject();
        foreach (var pair in table.Pairs)
        {
            var key = pair.Key.Type == DataType.String
                ? pair.Key.String
                : pair.Key.Type == DataType.Number
                    ? pair.Key.Number.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : string.Empty;
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            json[key] = ToJsonNode(pair.Value);
        }

        return json;
    }

    private static JsonNode? ToJsonNode(DynValue value)
    {
        return value.Type switch
        {
            DataType.Nil or DataType.Void => null,
            DataType.Boolean => JsonValue.Create(value.Boolean),
            DataType.Number => NumberToJson(value.Number),
            DataType.String => JsonValue.Create(value.String),
            DataType.Table => TableToJsonNode(value.Table),
            _ => throw new ScriptRuntimeException($"Unsupported declaration value type: {value.Type}.")
        };
    }

    private static JsonNode TableToJsonNode(Table table)
    {
        var pairs = table.Pairs.ToList();
        var numericKeys = pairs
            .Select(pair => NumericKeyOrNull(pair.Key))
            .Where(number => number.HasValue)
            .Select(number => number!.Value)
            .OrderBy(number => number)
            .ToList();
        if (numericKeys.Count == pairs.Count && numericKeys.Count > 0 && numericKeys[0] == 1 && numericKeys[^1] == numericKeys.Count)
        {
            var array = new JsonArray();
            foreach (var pair in pairs.OrderBy(pair => NumericKeyOrMax(pair.Key)))
            {
                array.Add(ToJsonNode(pair.Value));
            }

            return array;
        }

        return ToJsonObject(table);
    }

    private static JsonValue NumberToJson(double value)
    {
        return Math.Abs(value % 1) < double.Epsilon && value <= int.MaxValue && value >= int.MinValue
            ? JsonValue.Create((int)value)
            : JsonValue.Create(value);
    }

    private static string ReadRequiredString(JsonObject json, string propertyName, int index)
    {
        var value = ReadOptionalString(json, propertyName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ScriptRuntimeException($"Prototype Lua declaration at index {index} must include {propertyName}.");
        }

        return value;
    }

    private static string ReadOptionalString(JsonObject json, string propertyName)
    {
        return json[propertyName]?.GetValue<string>()?.Trim() ?? string.Empty;
    }

    private static int NumericKeyOrMax(DynValue value)
    {
        return NumericKeyOrNull(value) ?? int.MaxValue;
    }

    private static int? NumericKeyOrNull(DynValue value)
    {
        if (value.Type != DataType.Number)
        {
            return null;
        }

        var number = value.Number;
        return Math.Abs(number % 1) < double.Epsilon && number >= 1 && number <= int.MaxValue
            ? (int)number
            : null;
    }

    private static PrototypeLuaDiagnostic Error(string code, string message, string target)
    {
        return new PrototypeLuaDiagnostic
        {
            Severity = "error",
            Code = code,
            Message = message,
            Target = target
        };
    }

    private static bool IsError(PrototypeLuaDiagnostic diagnostic)
    {
        return diagnostic.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)
            || diagnostic.Severity.Equals("critical", StringComparison.OrdinalIgnoreCase);
    }
}
