using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictJsonResponseParser
{
    public GeneratorPlanStrictJsonParseResult Parse(string response, string contractId = "")
    {
        var diagnostics = new List<GeneratorPlanStrictLlmArtifactDiagnostic>();
        var trimmed = (response ?? string.Empty).Trim();

        if (trimmed.Length == 0)
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid, "Response is empty.", "response", contractId));
            return Failed(diagnostics);
        }

        if (trimmed.StartsWith("```", StringComparison.Ordinal) || trimmed.Contains("```", StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonMarkdownFence, "Response must not contain Markdown code fences.", "response", contractId));
            return Failed(diagnostics);
        }

        if (trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonRootNotObject, "Response JSON root must be an object.", "response", contractId));
            return Failed(diagnostics);
        }

        if (!trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonTextWrapper, "Response must be exactly one JSON object with no text before or after it.", "response", contractId));
            return Failed(diagnostics);
        }

        if (!trimmed.EndsWith("}", StringComparison.Ordinal) && trimmed.LastIndexOf('}') >= 0)
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonTextWrapper, "Response must be exactly one JSON object with no text before or after it.", "response", contractId));
            return Failed(diagnostics);
        }

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonRootNotObject, "Response JSON root must be an object.", "response", contractId));
                return Failed(diagnostics);
            }

            return new GeneratorPlanStrictJsonParseResult
            {
                Ok = true,
                Json = trimmed
            };
        }
        catch (JsonException ex)
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid, ex.Message, "response", contractId));
            return Failed(diagnostics);
        }
    }

    private static GeneratorPlanStrictJsonParseResult Failed(IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics)
    {
        return new GeneratorPlanStrictJsonParseResult
        {
            Ok = false,
            Diagnostics = diagnostics
        };
    }

    private static GeneratorPlanStrictLlmArtifactDiagnostic Diagnostic(string severity, string code, string message, string target, string contractId)
    {
        return new GeneratorPlanStrictLlmArtifactDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            Target = target,
            ContractId = contractId
        };
    }
}
