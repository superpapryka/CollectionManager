using System.Text;

namespace CollectionManager.Services;

public static class TextCodec
{
    public static string Escape(string? value)
    {
        value ??= string.Empty;
        return value
            .Replace("\\", "\\\\")
            .Replace("|", "\\p")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    public static List<string> SplitLine(string line)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        bool escaping = false;

        foreach (var ch in line)
        {
            if (escaping)
            {
                current.Append(ch switch
                {
                    'p' => '|',
                    'r' => '\r',
                    'n' => '\n',
                    '\\' => '\\',
                    _ => ch
                });
                escaping = false;
                continue;
            }

            if (ch == '\\')
            {
                escaping = true;
                continue;
            }

            if (ch == '|')
            {
                parts.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        if (escaping)
            current.Append('\\');

        parts.Add(current.ToString());
        return parts;
    }
}