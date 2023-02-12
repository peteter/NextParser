using System;
using System.Collections.Generic;

namespace Gotoland.NextParser
{
    public static class IListExtensions
    {
        public static decimal Dec(this IList<object> list, int index)
        {
            return Convert.ToDecimal(list[index]);
        }

        public static decimal Dec(this IList<object> list)
        {
            try
            {
                return Convert.ToDecimal(list[0]);
            }
            catch (FormatException)
            {
                throw new InvalidParameterException($"The parameter could not be converted to Decimal! '{list[0]}'");
            }
        }

        public static double Double(this IList<object> list, int index)
        {
            return Convert.ToDouble(list[index]);
        }

        public static double Double(this IList<object> list)
        {
            return Convert.ToDouble(list[0]);
        }

        public static int Int(this IList<object> list, int index)
        {
            return Convert.ToInt32(list[index]);
        }

        public static int Int(this IList<object> list)
        {
            return Convert.ToInt32(list[0]);
        }
        
        public static DateTime DateTime(this IList<object> list, int index)
        {
            return Convert.ToDateTime(list[index]);
        }

        public static DateTime DateTime(this IList<object> list)
        {
            return Convert.ToDateTime(list[0]);
        }
        
        public static string Str(this IList<object> list, int index)
        {
            return Convert.ToString(list[index]);
        }

        public static string Str(this IList<object> list)
        {
            return Convert.ToString(list[0]);
        }
        
        public static bool Bool(this IList<object> list, int index)
        {
            return Convert.ToBoolean(list[index]);
        }

        public static bool Bool(this IList<object> list)
        {
            return Convert.ToBoolean(list[0]);
        }

        public static TVal Get<TVal>(this IList<object> list, int index)
        {
            return (TVal) list[index];
        }
    }
}