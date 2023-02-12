using System;

namespace Gotoland.NextParserV2
{
    internal static class CharSpanExtensions
    {
        public static ReadOnlySpan<T> Slice<T>(this ReadOnlySpan<T> span, Segment segment)
        {
            return span.Slice(segment.Start, Math.Min(segment.Lenght, span.Length - segment.Start));
        }

        public static bool StartsWith(this ReadOnlySpan<char> span, char c)
        {
            return span.Length > 0 && span[0] == c;
        }

        public static bool EndsWith(this ReadOnlySpan<char> span, char c)
        {
            return span.Length > 0 && span[^1] == c;
        }

        public static ReadOnlySpan<char> TrimOne(this ReadOnlySpan<char> text, char[] chars)
        {
            bool trimStart = false;
            bool trimEnd = false;

            foreach (var c in chars)
            {
                if (!trimStart && text.StartsWith(c))
                    trimStart = true;
                if (!trimEnd && text.EndsWith(c))
                    trimEnd = true;
            }

            if (trimStart && !trimEnd)
            {
                return text.Slice(1);
            }
            else if (trimStart && trimEnd)
            {
                return text.Slice(1, text.Length - 2);
            }
            else if (trimEnd)
            {
                return text.Slice(0, text.Length - 1);
            }
            return text;
        }
    }
}