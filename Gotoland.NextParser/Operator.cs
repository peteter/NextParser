using System;

namespace Gotoland.NextParser
{
    /// <summary>
    /// Operator element, with name logic etc.
    /// </summary>
    public class Operator
    {
        /// <summary>
        /// Just the name of this operator.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// How this operator ranks against other operators in evaluation order.
        /// </summary>
        internal int Rank { get; }

        /// <summary>
        /// The logic of the operator.
        /// </summary>
        internal Func<object, object, object> Op { get; }

        /// <summary>
        /// The value type of the operand(s).
        /// </summary>
        internal ValueType ValueType { get; }

        internal OperatorParameters ParameterPreference { get; private set; } = OperatorParameters.OnlyTwo;

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="name">Name of the operator.</param>
        /// <param name="rank">Rank versus other operators.</param>
        /// <param name="op">The logic function.</param>
        public Operator(string name, int rank, Func<object, object, object> op, OperatorParameters allowNullParams = OperatorParameters.OnlyTwo)
        {
            Name = name;
            Op = op;
            Rank = rank;
            ValueType = ValueType.FromParameter;
            ParameterPreference = allowNullParams;
        }

        /// <summary>
        /// Evaluates the operator with given operand(s).
        /// </summary>
        /// <param name="left">The left operand (if any).</param>
        /// <param name="right">The right operand (if any).</param>
        /// <returns>The result from the operation.</returns>
        internal object Evaluate(object left, object right)
        {
            if (ParameterPreference == OperatorParameters.OnlyTwo && (left == null || right == null))
            {
                throw new InvalidParameterException($"The operator '{Name}' does not allow null parameters. Params: {ParamToStringOrNull(left)}, {ParamToStringOrNull(right)}");
            }
            else if (left == null && right == null)
            {
                throw new InvalidParameterException($"The operator '{Name}' has no paramters! Both are null.");
            }
            return Op(left, right);
        }

        private static string ParamToStringOrNull(object param)
        {
            if (param == null) return "null";
            return param.ToString();
        }

        internal void ValidateParameter(Elem left)
        {
            if ((ParameterPreference == OperatorParameters.OnlyTwo || ParameterPreference == OperatorParameters.PreferLeft) && left == null)
            {
                throw new InvalidParameterException($"The operator '{Name}' does not allow null parameters. Left param was null!");
            }
        }

        internal void ValidateParameters(Elem elem)
        {
            if (ParameterPreference == OperatorParameters.OnlyTwo && (elem.Left == null || elem.Right == null))
            {
                throw new InvalidParameterException($"The operator '{Name}' does not allow null parameters. Params: {ParamToStringOrNull(elem.Left)}, {ParamToStringOrNull(elem.Right)}");
            }
        }

        public override string ToString()
        {
            return $"({Name}@{ValueType})";
        }
    }

    public enum OperatorParameters
    {
        Invalid = 0,
        OnlyTwo = 1,
        PreferLeft = 2,
        PreferRight = 3,
        Any = 4,
    }
}