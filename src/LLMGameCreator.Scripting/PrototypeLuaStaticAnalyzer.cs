using System.Text;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Scripting;

public sealed partial class PrototypeLuaStaticAnalyzer
{
    private static readonly ForbiddenPattern[] ForbiddenPatterns =
    {
        new("io", "lua.prototype.forbidden_api", "The io API is not available in Prototype Lua."),
        new("os", "lua.prototype.forbidden_api", "The os API is not available in Prototype Lua."),
        new("debug", "lua.prototype.forbidden_api", "The debug API is not available in Prototype Lua."),
        new("dofile", "lua.prototype.forbidden_loader", "dofile is not available in Prototype Lua."),
        new("loadfile", "lua.prototype.forbidden_loader", "loadfile is not available in Prototype Lua."),
        new("load", "lua.prototype.forbidden_loader", "load is not available in Prototype Lua."),
        new("require", "lua.prototype.forbidden_loader", "require is not available in Prototype Lua."),
        new("package", "lua.prototype.forbidden_package", "package is not available in Prototype Lua."),
        new("collectgarbage", "lua.prototype.forbidden_api", "collectgarbage is not available in Prototype Lua."),
        new("setfenv", "lua.prototype.forbidden_api", "setfenv is not available in Prototype Lua."),
        new("getfenv", "lua.prototype.forbidden_api", "getfenv is not available in Prototype Lua."),
        new("newproxy", "lua.prototype.forbidden_api", "newproxy is not available in Prototype Lua."),
        new("coroutine", "lua.prototype.forbidden_api", "coroutine is not available in Prototype Lua."),
        new("while", "lua.prototype.forbidden_control_flow", "Loops are not available in Prototype Lua declarations."),
        new("repeat", "lua.prototype.forbidden_control_flow", "Loops are not available in Prototype Lua declarations."),
        new("for", "lua.prototype.forbidden_control_flow", "Loops are not available in Prototype Lua declarations.")
    };

    public IReadOnlyList<PrototypeLuaDiagnostic> Analyze(string source, string target)
    {
        var diagnostics = new List<PrototypeLuaDiagnostic>();
        var searchable = RemoveCommentsAndStrings(source ?? string.Empty);
        foreach (var pattern in ForbiddenPatterns)
        {
            if (!IdentifierRegex(pattern.Token).IsMatch(searchable))
            {
                continue;
            }

            diagnostics.Add(Error(pattern.Code, pattern.Message, target));
        }

        if (MathRandomRegex.IsMatch(searchable) || MathRandomSeedRegex.IsMatch(searchable))
        {
            diagnostics.Add(Error("lua.prototype.forbidden_random", "math.random and math.randomseed are not available in Prototype Lua.", target));
        }

        return diagnostics;
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

    private static string RemoveCommentsAndStrings(string source)
    {
        var result = new StringBuilder(source.Length);
        var index = 0;
        while (index < source.Length)
        {
            var current = source[index];
            var next = index + 1 < source.Length ? source[index + 1] : '\0';
            if (current == '-' && next == '-')
            {
                index += 2;
                if (index < source.Length && source[index] == '[' && index + 1 < source.Length && source[index + 1] == '[')
                {
                    index += 2;
                    while (index + 1 < source.Length && !(source[index] == ']' && source[index + 1] == ']'))
                    {
                        result.Append(' ');
                        index++;
                    }

                    index = Math.Min(source.Length, index + 2);
                    continue;
                }

                while (index < source.Length && source[index] != '\r' && source[index] != '\n')
                {
                    result.Append(' ');
                    index++;
                }

                continue;
            }

            if (current == '"' || current == '\'')
            {
                var quote = current;
                result.Append(' ');
                index++;
                while (index < source.Length)
                {
                    if (source[index] == '\\')
                    {
                        result.Append(' ');
                        index += Math.Min(2, source.Length - index);
                        continue;
                    }

                    var end = source[index] == quote;
                    result.Append(source[index] == '\r' || source[index] == '\n' ? source[index] : ' ');
                    index++;
                    if (end)
                    {
                        break;
                    }
                }

                continue;
            }

            result.Append(current);
            index++;
        }

        return result.ToString();
    }

    private static Regex IdentifierRegex(string token)
    {
        return new Regex($@"(?<![A-Za-z0-9_]){Regex.Escape(token)}(?![A-Za-z0-9_])", RegexOptions.CultureInvariant);
    }

    private static readonly Regex MathRandomRegex = new Regex(
        @"(?<![A-Za-z0-9_])math\s*\.\s*random(?![A-Za-z0-9_])",
        RegexOptions.CultureInvariant);

    private static readonly Regex MathRandomSeedRegex = new Regex(
        @"(?<![A-Za-z0-9_])math\s*\.\s*randomseed(?![A-Za-z0-9_])",
        RegexOptions.CultureInvariant);

    private sealed class ForbiddenPattern
    {
        public ForbiddenPattern(string token, string code, string message)
        {
            Token = token;
            Code = code;
            Message = message;
        }

        public string Token { get; }
        public string Code { get; }
        public string Message { get; }
    }
}
