using System;

namespace Gotoland.NextParser
{
    public static class StringExtensions
    {
        public static string TrimOne(this string text, char[] chars)
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
                return text.Substring(1);
            }
            else if (trimStart && trimEnd)
            {
                return text.Substring(1, text.Length - 2);
            }
            else if (trimEnd)
            {
                return text.Substring(0, text.Length - 1);
            }
            return text;
        }

        public static ReadOnlySpan<char> TrimOneSpan(this ReadOnlySpan<char> text, char[] chars)
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