using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Scripting;

namespace LLMGameCreator.Application.Design;

public sealed class PrototypeLuaPatchArtifactService : IPrototypeLuaPatchArtifactService
{
    private const int SchemaVersion = 1;

    private static readonly JsonSerializerOptions PatchJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IPrototypeLuaExecutor _executor;
    private readonly PrototypeLuaDeclarationMapper _mapper;
    private readonly GamePackagePatchOperationValidator _operationValidator;
    private readonly IGeneratedArtifactRepository _artifactRepository;
    private readonly IGamePackagePatchService _patchService;

    public PrototypeLuaPatchArtifactService(
        IPrototypeLuaExecutor executor,
        PrototypeLuaDeclarationMapper mapper,
        GamePackagePatchOperationValidator operationValidator,
        IGeneratedArtifactRepository artifactRepository,
        IGamePackagePatchService patchService)
    {
        _executor = executor;
        _mapper = mapper;
        _operationValidator = operationValidator;
        _artifactRepository = artifactRepository;
        _patchService = patchService;
    }

    public async Task<PrototypeLuaPatchArtifactResult> CreatePatchArtifactFromPrototypeLuaAsync(
        PrototypeLuaPatchArtifactRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Source))
        {
            var result = ValidationResult("artifact/prototype-lua/missing", "error", "lua.prototype.source.empty", "Prototype Lua source is required.", "source", 0);
            return new PrototypeLuaPatchArtifactResult(null, new[] { result }, null, false, "Prototype Lua source is required.");
        }

        var execution = await _executor.ExecuteAsync(new PrototypeLuaExecutionRequest
        {
            ScriptId = NormalizeSourceId(request.ScriptId),
            Source = request.Source,
            SourcePath = request.SourcePath,
            TimeoutMs = request.TimeoutMs,
            MaxDeclarations = request.MaxDeclarations,
            MaxInstructionCount = request.MaxInstructionCount
        }, cancellationToken).ConfigureAwait(false);

        var validationResults = ToValidationResults(BuildArtifactId(request), execution.Diagnostics);
        if (validationResults.Any(IsError) || !execution.Success)
        {
            return new PrototypeLuaPatchArtifactResult(null, validationResults, null, false, "Prototype Lua execution failed; patch artifact was not saved.");
        }

        var mapping = _mapper.MapToPackageOperations(execution.Declarations);
        validationResults = validationResults.Concat(ToValidationResults(BuildArtifactId(request), mapping.Diagnostics)).ToList();
        if (validationResults.Any(IsError) || !mapping.Success)
        {
            return new PrototypeLuaPatchArtifactResult(null, validationResults, null, false, "Prototype Lua declarations could not be mapped to package operations.");
        }

        var operationsValidation = _operationValidator.ValidatePackageOperationsJson(mapping.OperationsJson, BuildArtifactId(request));
        validationResults = validationResults.Concat(operationsValidation.ValidationResults).ToList();
        if (validationResults.Any(IsError))
        {
            return new PrototypeLuaPatchArtifactResult(null, validationResults, null, false, "Prototype Lua package operations failed patch validation; patch artifact was not saved.");
        }

        var artifactId = BuildArtifactId(request);
        var documentJson = BuildPatchDocumentJson(request, artifactId, mapping.OperationsJson);
        var patchValidation = _operationValidator.ValidatePatchJson(artifactId, documentJson);
        validationResults = validationResults
            .Concat(patchValidation)
            .Concat(new[]
            {
                ValidationResult(
                    artifactId,
                    "warning",
                    "lua.prototype.policy.patch_only",
                    "Prototype Lua output is saved as a game_package_patch_v1 artifact only; apply remains explicit through the patch pipeline.",
                    artifactId,
                    validationResults.Count + patchValidation.Count)
            })
            .ToList();
        if (validationResults.Any(IsError))
        {
            return new PrototypeLuaPatchArtifactResult(null, validationResults, null, false, "Prototype Lua patch artifact failed final validation; artifact was not saved.");
        }

        var artifact = new GeneratedArtifactRecord(
            artifactId,
            GamePackagePatchArtifactKinds.PatchV1,
            $"design-db://generated-artifacts/{artifactId}",
            documentJson,
            NormalizePlanId(request),
            ValidationState(validationResults),
            JsonSerializer.Serialize(new
            {
                created_utc = DateTimeOffset.UtcNow.ToString("O"),
                source = "prototype_lua",
                script_id = NormalizeSourceId(request.ScriptId),
                source_path = request.SourcePath,
                source_artifact_id = request.SourceArtifactId,
                declaration_count = execution.Declarations.Count,
                elapsed_ms = execution.ElapsedMs,
                dry_run_requested = request.DryRun
            }, PatchJsonOptions));

        await _artifactRepository.SaveGeneratedArtifactAsync(artifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(artifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        GamePackagePatchDryRunResult? dryRunResult = null;
        if (request.DryRun)
        {
            dryRunResult = await _patchService.DryRunPatchArtifactAsync(artifact.Id, cancellationToken).ConfigureAwait(false);
        }

        return new PrototypeLuaPatchArtifactResult(
            artifact,
            validationResults,
            dryRunResult,
            true,
            request.DryRun && dryRunResult != null ? dryRunResult.Message : $"Patch artifact saved: {artifact.Path}");
    }

    private static string BuildPatchDocumentJson(PrototypeLuaPatchArtifactRequest request, string artifactId, string operationsJson)
    {
        var operations = JsonNode.Parse(operationsJson) as JsonArray ?? new JsonArray();
        var document = new JsonObject
        {
            ["kind"] = GamePackagePatchArtifactKinds.PatchV1,
            ["schema_version"] = SchemaVersion,
            ["source"] = new JsonObject
            {
                ["plan_id"] = NormalizePlanId(request),
                ["preview_artifact_id"] = string.IsNullOrWhiteSpace(request.SourceArtifactId) ? artifactId + "/source" : request.SourceArtifactId!.Trim()
            },
            ["operations"] = operations
        };

        return document.ToJsonString(PatchJsonOptions);
    }

    private static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToValidationResults(string artifactId, IReadOnlyList<PrototypeLuaDiagnostic> diagnostics)
    {
        return diagnostics
            .Select((diagnostic, index) => ValidationResult(
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.Target,
                index))
            .ToList();
    }

    private static GeneratedArtifactValidationResultRecord ValidationResult(
        string artifactId,
        string severity,
        string code,
        string message,
        string target,
        int index)
    {
        return new GeneratedArtifactValidationResultRecord(
            BuildValidationResultId(artifactId, code, index),
            artifactId,
            severity,
            code,
            message,
            target,
            "{}");
    }

    private static string BuildArtifactId(PrototypeLuaPatchArtifactRequest request)
    {
        var source = NormalizeSourceId(request.ScriptId);
        return $"artifact/prototype-lua-patch/{source.Replace("/", "-", StringComparison.OrdinalIgnoreCase)}";
    }

    private static string NormalizePlanId(PrototypeLuaPatchArtifactRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.PlanId))
        {
            return request.PlanId.Trim();
        }

        var source = NormalizeSourceId(request.ScriptId);
        return $"prototype-lua/{source}";
    }

    private static string NormalizeSourceId(string? scriptId)
    {
        return string.IsNullOrWhiteSpace(scriptId) ? "inline" : scriptId.Trim();
    }

    private static bool IsError(GeneratedArtifactValidationResultRecord result)
    {
        return result.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)
            || result.Severity.Equals("critical", StringComparison.OrdinalIgnoreCase);
    }

    private static string ValidationState(IReadOnlyList<GeneratedArtifactValidationResultRecord> validationResults)
    {
        if (validationResults.Any(IsError))
        {
            return "invalid";
        }

        return validationResults.Any(result => result.Severity.Equals("warning", StringComparison.OrdinalIgnoreCase))
            ? "warning"
            : "valid";
    }

    private static string BuildValidationResultId(string artifactId, string code, int index)
    {
        return $"{artifactId}/validation/{index.ToString("D3", CultureInfo.InvariantCulture)}/{code}";
    }
}

