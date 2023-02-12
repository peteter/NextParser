using System;
using System.Collections.Generic;
using System.Globalization;

namespace Gotoland.NextParser
{
    /// <summary>
    /// Function object, with a name, input parameter(s) type(s), output parameter type, the logic etc.
    /// </summary>
    public class Function
    {
        private readonly ValueType[] parameterTypes;
        private readonly Func<IList<Elem>, bool> parameterChecker;

        internal int TextId { get; }
        internal string Name { get; }
        internal ValueType ValueType { get; }
        internal Func<IList<object>, object> Fun { get; }
        internal Func<IList<object>, CultureInfo, object> FunCulture { get; }

        /// <summary>
        /// Evaluates this function against given input parameters and culture. May throw expcetion if params are bad.
        /// </summary>
        /// <param name="parameters">The input parameters to the function logic.</param>
        /// <param name="culture">The cukture to use when parsing etc.</param>
        /// <param name="fullParamValidation">Set true to make a full parameter validation before running function logic. Will cost time.</param>
        /// <returns>The object that is the result from the evalutation.</returns>
        /// <exception cref="InvalidParameterException">If the parameters are bad or can't be validated.</exception>
        public object Evaluate(IList<object> parameters, CultureInfo culture = null, bool fullParamValidation = false)
        {
            if (fullParamValidation)
            {
                ValidateParameters(parameters, culture);
            }
            try
            {
                if (FunCulture != null)
                    return FunCulture(parameters, culture);
                return Fun(parameters);
            }
            catch (FormatException ex)
            {
                throw new InvalidParameterException($"One parameter was not in the correct format for function '{Name}': " + ex.Message, ex);
            }
        }

        public Function(string name, int textId, ValueType valueType, Func<IList<object>, object> fun,
            params ValueType[] parameterTypes)
        {
            this.Name = name;
            this.TextId = textId;
            this.ValueType = valueType;
            this.Fun = fun;
            this.parameterTypes = parameterTypes;
        }

        public Function(string name, int textId, ValueType valueType, Func<IList<object>, CultureInfo, object> funCulture,
            params ValueType[] parameterTypes)
        {
            this.Name = name;
            this.TextId = textId;
            this.ValueType = valueType;
            this.FunCulture = funCulture;
            this.parameterTypes = parameterTypes;
        }

        public Function(string name, int textId, ValueType valueType, Func<IList<object>, object> fun,
            Func<IList<Elem>, bool> parameterChecker, params ValueType[] parameterTypes)
        {
            this.Name = name;
            this.TextId = textId;
            this.ValueType = valueType;
            this.Fun = fun;
            this.parameterTypes = parameterTypes;
            this.parameterChecker = parameterChecker;
        }

        /// <summary>
        /// Checks the parsed parameters of this function to make sure they are the required number and matching types.
        /// </summary>
        /// <param name="elems">The elements that are the parameters.</param>
        /// <returns>True if parameters are OK.</returns>
        /// <exception cref="InvalidParameterException">When the paramers are woring type or number.</exception>
        internal virtual bool CheckParameters(IList<Elem> elems)
        {
            if (this.parameterChecker != null)
                if (!this.parameterChecker(elems))
                    return false;

            if (parameterTypes == null)
                return true;

            if (parameterTypes.Length > 0 && parameterTypes[0] == ValueType.AnyNumber)
                return true;

            if (parameterTypes.Length != elems.Count)
            {
                throw new InvalidParameterException("Function '" + this.Name + "' takes " + parameterTypes.Length +
                                                    " parameters, but " + elems.Count + " was given!");
            }

            if (parameterTypes.Length > 0)
            {
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    if (elems[i].ValueType != parameterTypes[i] && parameterTypes[i] != ValueType.Any && elems[i].ValueType != ValueType.Any)
                    {
                        throw new InvalidParameterException("Parameter type '" + elems[i].ValueType + "' at position " + i +
                                                            " is not valid for function '" + this.Name + "'!");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Validates that the given parameters can be used in this function.
        /// </summary>
        /// <param name="parameters">The parameters to validate.</param>
        /// <param name="culture">The culture to use when parsing etc.</param>
        /// <param name="dontThrow">Set true if this method shouldn't throw, just return false.</param>
        /// <returns>True if parameters are OK.</returns>
        /// <exception cref="InvalidParameterException">If any parameter is bad.</exception>
        public virtual bool ValidateParameters(IList<object> parameters, CultureInfo culture, bool dontThrow = false)
        {
            ValueType fixedType = ValueType.None;
            if (this.parameterTypes.Length > 0 && this.parameterTypes[0] == ValueType.AnyNumber)
            {
                if (this.parameterTypes.Length == 1)
                    return true;

                fixedType = this.parameterTypes[1];
                if (fixedType == ValueType.Any)
                    return true;

                for (int i = 1; i < parameters.Count; i++)
                {
                    if (!CheckParam(fixedType, i, parameters[i], culture, dontThrow))
                        return false;
                }
            }
            else
            {

                for (int i = 0; i < this.parameterTypes.Length; i++)
                {
                    var param = parameters[i];

                    var expectedType = fixedType;
                    if (fixedType != ValueType.None)
                    {
                        expectedType = parameterTypes[i];
                        if (!CheckParam(expectedType, i, param, culture, dontThrow))
                            return false;
                    }

                }
            }

            return true;
        }

        private bool CheckParam(ValueType expectedType, int paramIndex, object param, CultureInfo culture, bool dontThrow)
        {
            try
            {
                switch (expectedType)
                {
                    case ValueType.None:
                        break; ;
                    case ValueType.Any:
                        break;
                    case ValueType.Number:
                        if (param is decimal
                            || param is double
                            || param is float
                            || param is int
                            || param is long
                            || param is short
                            || param is byte
                            )
                            return true;
                        break;
                    case ValueType.Text:
                        if (param is string)
                            return true;
                        break;
                    case ValueType.Boolean:
                        if (param is bool)
                            return true;
                        break;
                    case ValueType.AnyNumber:
                        return true; // Though should not be here.
                    case ValueType.Date:
                        if (param is DateTime)
                            return true;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                if (!dontThrow)
                    MakeParamException(expectedType, paramIndex, param, ex);
            }

            if (dontThrow)
                return false;
            throw MakeParamException(expectedType, paramIndex, param, null);
        }

        private Exception MakeParamException(ValueType expectedType, int paramIndex, object param, Exception ex)
        {
            throw new InvalidParameterException($"For function '{this.Name}', the given parameter number {paramIndex} was of wrong type or malformatted! Expected type {expectedType}, actual type '{(param == null ? "null" : param.GetType().Name)}', value: '{param}'", ex);
        }

        public override string ToString()
        {
            return $"[Func: {Name} -> {ValueType}]";
        }
    }
}