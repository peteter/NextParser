using System;
using System.Collections.Generic;

namespace Gotoland.NextParser
{
    public static class DictionaryExtensions
    {
        public static bool ContainsKey<T>(this Dictionary<string, T> dict, ReadOnlySpan<char> searched)
        {
            foreach (var key in dict.Keys)
            {
                bool found = true;
                if (key.Length != searched.Length)
                {
                    continue;
                }
                for (int i = 0; i < searched.Length; i++)
                {
                    if (key[i] != searched[i])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return true;
                }
            }
            return false;
        }
    }
}