using System.Globalization;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

internal sealed class FeatureModuleEffectiveValueExpression
{
    private readonly string _text;
    private readonly Func<string, decimal> _resolveReference;
    private int _position;

    private FeatureModuleEffectiveValueExpression(string text, Func<string, decimal> resolveReference)
    {
        _text = text;
        _resolveReference = resolveReference;
    }

    public static decimal Evaluate(string text, Func<string, decimal> resolveReference)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("empty numeric expression rejected");
        var parser = new FeatureModuleEffectiveValueExpression(text, resolveReference);
        var value = parser.ParseExpression();
        parser.SkipWhitespace();
        if (parser._position != text.Length) throw new InvalidOperationException("invalid numeric expression rejected: " + text);
        return value;
    }

    private decimal ParseExpression()
    {
        var value = ParseTerm();
        while (true)
        {
            SkipWhitespace();
            if (Take('+')) value += ParseTerm();
            else if (Take('-')) value -= ParseTerm();
            else return value;
        }
    }

    private decimal ParseTerm()
    {
        var value = ParseUnary();
        while (true)
        {
            SkipWhitespace();
            if (Take('*')) value *= ParseUnary();
            else if (Take('/'))
            {
                var divisor = ParseUnary();
                if (divisor == 0) throw new InvalidOperationException("division by zero rejected");
                value /= divisor;
            }
            else return value;
        }
    }

    private decimal ParseUnary()
    {
        SkipWhitespace();
        if (Take('+')) return ParseUnary();
        if (Take('-')) return -ParseUnary();
        return ParsePrimary();
    }

    private decimal ParsePrimary()
    {
        SkipWhitespace();
        if (Take('('))
        {
            var value = ParseExpression();
            SkipWhitespace();
            if (!Take(')')) throw new InvalidOperationException("unclosed numeric expression parenthesis rejected");
            return value;
        }
        if (StartsWith("${"))
        {
            _position += 2;
            var end = _text.IndexOf('}', _position);
            if (end < 0) throw new InvalidOperationException("unclosed effective value reference rejected");
            var reference = _text[_position..end];
            _position = end + 1;
            return _resolveReference(reference);
        }

        var start = _position;
        while (_position < _text.Length && (char.IsDigit(_text[_position]) || _text[_position] == '.')) _position++;
        if (start == _position
            || !decimal.TryParse(_text[start.._position], NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var numeric))
            throw new InvalidOperationException("nonnumeric value in numeric expression rejected: " + _text);
        return numeric;
    }

    private bool Take(char value)
    {
        if (_position >= _text.Length || _text[_position] != value) return false;
        _position++;
        return true;
    }

    private bool StartsWith(string value) =>
        _text.AsSpan(_position).StartsWith(value, StringComparison.Ordinal);

    private void SkipWhitespace()
    {
        while (_position < _text.Length && char.IsWhiteSpace(_text[_position])) _position++;
    }
}
