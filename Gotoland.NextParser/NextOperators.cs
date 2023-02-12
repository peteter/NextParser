using System;

namespace Gotoland.NextParser
{
    /// <summary>
    /// Holds the base operators for the formula parser.
    /// </summary>
    public static class NextOperators
    {
        public static readonly Operator Add = new Operator(
            "+",
            10,
            (left, right) => Convert.ToDecimal(left) + Convert.ToDecimal(right)
        );

        public static readonly Operator Concat = new Operator(
            "&",
            15,
            (left, right) => (Convert.ToString(left)) + (Convert.ToString(right))
        );

        public static readonly Operator Div = new Operator(
            "/",
            7,
            (left, right) => Convert.ToDecimal(left) / Convert.ToDecimal(right)
        );

        public static readonly Operator Mul = new Operator(
            "*",
            8,
            (left, right) => Convert.ToDecimal(left) * Convert.ToDecimal(right)
        );

        public static readonly Operator Sub = new Operator(
            "-",
            9,
            (left, right) =>
                {
                    if (left == null)
                        return -1 * (decimal)right;
                    if (right == null)
                        throw new BadFormulaException("Right operand missing for operator '-'!");
                    if (left is DateTime && right is DateTime)
                        return (DateTime)left - (DateTime)right;
                    return (decimal)left - (decimal)right;
                },
            allowNullParams: OperatorParameters.PreferRight
        );

        public static readonly Operator LessThan = new Operator(
            "<",
            10,
            (left, right) => Convert.ToDecimal(left) < Convert.ToDecimal(right)
        );

        public static readonly Operator LessThanOrEqual = new Operator(
             "<=",
             11,
             (left, right) => Convert.ToDecimal(left) <= Convert.ToDecimal(right)
         );

        public static readonly Operator GreaterThan = new Operator(
             ">",
             12,
             (left, right) => Convert.ToDecimal(left) > Convert.ToDecimal(right)
         );

        public static readonly Operator GreaterThanOrEqual = new Operator(
             ">=",
             13,
             (left, right) => Convert.ToDecimal(left) >= Convert.ToDecimal(right)
         );

        public static readonly Operator NotEqual = new Operator(
            "<>",
            14,
            (left, right) => Convert.ToDecimal(left) != Convert.ToDecimal(right)
        );

        public static readonly Operator Power = new Operator(
            "^",
            2,
            (left, right) => (decimal)Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right))
        );

        public static readonly Operator Negate = new Operator(
            "!",
            3,
            (left, right) => !Convert.ToBoolean(right),
            allowNullParams: OperatorParameters.PreferRight);

        public static readonly Operator Percent = new Operator(
            "%",
            4,
            (left, right) => Convert.ToDecimal(left) / 100m,
            allowNullParams: OperatorParameters.PreferLeft);


        /// <summary>
        /// An array of all the operators in this class.
        /// </summary>
        public static readonly Operator[] AllOperators = new[] {
            Add,
            Concat,
            Div,
            GreaterThan,
            GreaterThanOrEqual,
            LessThan,
            LessThanOrEqual,
            Mul,
            NotEqual,
            Power,
            Sub,
            Negate,
            Percent,
        };
    }
}