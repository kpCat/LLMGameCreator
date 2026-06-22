using System.Text;

namespace LLMGameCreator.Application.Composition;

internal static class UnityArchiveRequestDiagnosticsBuilder
{
    public static UnityArchiveRequestDiagnostic Error(string code, string message, string targetId)
    {
        return new UnityArchiveRequestDiagnostic
        {
            Severity = UnityArchiveExportDiagnosticSeverity.Error,
            Code = code,
            Message = message,
            TargetId = targetId
        };
    }

    public static UnityArchiveRequestDiagnostic Warning(string code, string message, string targetId)
    {
        return new UnityArchiveRequestDiagnostic
        {
            Severity = UnityArchiveExportDiagnosticSeverity.Warning,
            Code = code,
            Message = message,
            TargetId = targetId
        };
    }

    public static string NormalizeId(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return "unknown";
        }

        var sb = new StringBuilder(trimmed.Length);
        foreach (var ch in trimmed)
        {
            if (ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_')
            {
                sb.Append(ch);
            }
            else
            {
                sb.Append('-');
            }
        }

        return sb.ToString().ToLowerInvariant();
    }

    public static bool IsFutureProvider(UnityArchiveRequestProviderKind kind)
    {
        return kind is UnityArchiveRequestProviderKind.comfyui_future
            or UnityArchiveRequestProviderKind.suno_future
            or UnityArchiveRequestProviderKind.local_audio_future
            or UnityArchiveRequestProviderKind.procedural_future;
    }

    public static int SeverityOrder(UnityArchiveExportDiagnosticSeverity severity)
    {
        return severity switch
        {
            UnityArchiveExportDiagnosticSeverity.Error => 0,
            UnityArchiveExportDiagnosticSeverity.Warning => 1,
            _ => 2
        };
    }

    public static int CompareRequests(string a, string b)
    {
        var kindComparison = string.Compare(GetRequestKind(a), GetRequestKind(b), StringComparison.OrdinalIgnoreCase);
        if (kindComparison != 0)
        {
            return kindComparison;
        }

        var idComparison = string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        if (idComparison != 0)
        {
            return idComparison;
        }

        return string.Compare(a, b, StringComparison.Ordinal);
    }

    private static string GetRequestKind(string requestId)
    {
        var segments = requestId.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 2 ? segments[0].ToLowerInvariant() : requestId.ToLowerInvariant();
    }
}