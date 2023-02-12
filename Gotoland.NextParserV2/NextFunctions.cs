using System;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global

namespace Gotoland.NextParserV2
{
    public static class NextFunctions
    {
        public static readonly Function Max = new Function(
            "MAX",
            1,
            ValueType.Number,
            para => para.Max(p => (decimal)p),
            ValueType.AnyNumber, ValueType.Number
        );

        public static readonly Function If = new Function(
            "IF",
            2,
            ValueType.Any,
            para => Convert.ToBoolean(para[0]) ? para[1] : para[2],
            ValueType.Any, ValueType.Any, ValueType.Any
        );

        public static readonly Function Ifs = new Function(
             "IFS",
             2002,
             ValueType.Any,
             para =>
             {
                 var ifCount = 2 * (para.Count / 2) - 1; // The last un-even index
                 for (int i = 0; i < ifCount; i += 2)
                     if (Convert.ToBoolean(para[i]))
                         return para[i + 1];
                 if (para.Count > 1 && para.Count % 2 == 1)
                     return para.Last();

                 return null;
             },
             elems => elems.Count < 2 ? throw new InvalidParameterException("Function IFS takes a minimum of 2 parameters!") : true,
             ValueType.AnyNumber, ValueType.Any
         );

        public static readonly Function Or = new Function(
            "OR",
            3,
            ValueType.Any,
            para => para.Any(Convert.ToBoolean),
            ValueType.Any, ValueType.Any
        );

        public static readonly Function Pi = new Function(
            "PI",
            4,
            ValueType.Number,
            para => Convert.ToDecimal(Math.PI)
        );

        public static readonly Function Count = new Function(
            "COUNT",
            5,
            ValueType.Number,
            para => (decimal)para.Count,
            ValueType.AnyNumber, ValueType.Any
        );

        public static readonly Function Neg = new Function(
            "NEG",
            7,
            ValueType.Number,
            ps => -Math.Abs(ps.Dec()),
            ValueType.Number
        );

        public static readonly Function Abs = new Function(
            "ABS",
            8,
            ValueType.Number,
            ps => Math.Abs(ps.Dec()),
            ValueType.Number
        );

        public static readonly Function And = new Function(
            "AND",
            9,
            ValueType.Boolean,
            ps => ps.All(Convert.ToBoolean),
            ValueType.AnyNumber, ValueType.Boolean
        );

        public static readonly Function Concatenate = new Function(
            "CONCATENATE",
            10,
            ValueType.Text,
            ps => string.Join("", ps.Select(Convert.ToString)),
            ValueType.Text, ValueType.Text
        );

        public static readonly Function Date = new Function(
            "DATE",
            11,
            ValueType.Date,
            ps => new DateTime(ps.Int(0), ps.Int(1), ps.Int(2)),
            ValueType.Number, ValueType.Number, ValueType.Number
        );

        public static readonly Function Day = new Function(
            "DAY",
            12,
            ValueType.Number,
            ps => ps.DateTime().Day,
            ValueType.Date
        );

        public static readonly Function Decimals = new Function(
            "DECIMALS",
            13,
            ValueType.Number,
            ps =>
            {
                var d = ps.Dec();
                return d - Math.Truncate(d);
            },
            ValueType.Number
        );

        public static readonly Function Eom = new Function(
            "EOM",
            14,
            ValueType.Date,
            _ =>
            {
                var today = DateTime.Now;
                var first = new DateTime(today.Year, today.Month, 1);
                first = first.AddMonths(1);
                first = first.AddDays(-1);
                return first;
            },
            ValueType.None
        );

        public static readonly Function False = new Function(
            "FALSE",
            15,
            ValueType.Boolean,
            (ps) => false,
            ValueType.Boolean
        );

        public static readonly Function Left = new Function(
            "LEFT",
            16,
            ValueType.Text,
            ps => Convert.ToString(ps.First()).Substring(0, ps.Int(1)),
            ValueType.Text, ValueType.Number
        );

        public static readonly Function Right = new Function(
            "RIGHT",
            17,
            ValueType.Text,
            ps => Convert.ToString(ps.First()).Substring(ps.Int(1)),
            ValueType.Text, ValueType.Number
        );

        public static readonly Function Substitute = new Function(
            "SUBSTITUTE",
            18,
            ValueType.Text,
            ps => ps.Str(0).Replace(ps.Str(1), ps.Str(2)),
            ValueType.Text, ValueType.Text, ValueType.Text
        );

        public static readonly Function Upper = new Function(
            "UPPER",
            19,
            ValueType.Text,
            (ps, culture) => ps.Str(0).ToUpper(culture),
            ValueType.Text
        );

        public static readonly Function Lower = new Function(
            "LOWER",
            20,
            ValueType.Text,
            (ps, culture) => ps.Str(0).ToLower(culture),
            ValueType.Text
        );

        public static readonly Function Not = new Function(
            "NOT",
            21,
            ValueType.Boolean,
            (ps) => !ps.Bool(),
            ValueType.Boolean
        );

        public static readonly Function Pow = new Function(
            "POW",
            22,
            ValueType.Number,
            (ps) => Math.Pow(ps.Double(0), ps.Double(1)),
            ValueType.Number, ValueType.Number
        );

        public static readonly Function Year = new Function(
            "YEAR",
            23,
            ValueType.Number,
            (ps, culture) => ps.DateTime().Year,
            ValueType.Number
        );
        
        public static readonly Function Sum = new Function(
            "SUM",
            24,
            ValueType.Number,
            (ps, culture) => ps.Select(p => Convert.ToDecimal(p)).Sum(),
            ValueType.AnyNumber
        );

        public static readonly Function[] AllFunctions =
        {
            Abs, And, Concatenate, Count, Date, Day, Decimals, Eom, False, If, Ifs, Left, Lower, Max, Neg, Not, Or, Pi, Pow, Right, Substitute, Upper, Year, Sum,
        };
    }

    /*
     *   Abs, AccPeriod, AccYear, Acos, And, Asin, Atan, Atan2, Concatenate, Cos, Cosh, 
  CurrentResource, CurrentUser, Date, Day, Ddb, Decimals, Eomonth, Exact, Exp, 
  False, Find, Formula, Fv, If, Int, Left, Len, Log, Log10, Lower, Mapping, Mid, 
  Mod, Month, Neg, Not, Or, Pi, Pmt, Proper, Pv, Rate, Rept, Right, Round, Sign, 
  Sin, Sinh, Sqrt, Substitute, Switch, Tan, Tanh, Tarvar, Today, Trim, True, Upper, 
  ValueViaTypeText, Year

     */
}
