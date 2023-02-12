using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Gotoland.NextParser
{
    /// <summary>
    /// Parser and evaluator of formulas. All instances are created with a specific culture, with <see cref="CultureInfo.InvariantCulture"/> as default.
    /// </summary>
    public partial class Next
    {
        private static readonly Dictionary<int, Next> localizedEngines = new Dictionary<int, Next>();

        private readonly CultureInfo culture = CultureInfo.InvariantCulture;
        private readonly Dictionary<string, Function> functions = new();
        private readonly Dictionary<string, Operator> operators = new();
        private Dictionary<string, Function> localizedFunctionNames;
        private ICellValueLookup cellValues;
        private HashSet<char> operatorStartingChars = new HashSet<char>();

        public static Next ForCulture(CultureInfo culture)
        {
            if (!localizedEngines.TryGetValue(culture.LCID, out var next))
            {
                next = new Next(culture);
                localizedEngines.Add(culture.LCID, next);
            }
            return next;
        }

        private void PreCalc()
        {
            foreach (var op in operators)
            {
                operatorStartingChars.Add(op.Key[0]);
            }
        }

        /// <summary>
        /// Creates a Next parser with invariant culture.
        /// </summary>
        public Next() : this(CultureInfo.InvariantCulture)
        {
        }

        /// <summary>
        /// Creates a Next parser with given culture.
        /// </summary>
        /// <param name="culture">The culture to use.</param>
        public Next(CultureInfo culture)
        {
            foreach (var fun in NextFunctions.AllFunctions)
                functions.Add(fun.Name, fun);

            foreach (var op in NextOperators.AllOperators)
                operators.Add(op.Name, op);

            this.culture = culture;
            this.LocalizeFunctionNames();
            this.PreCalc();
        }


        /// <summary>
        /// Parses the given formula string and returns a <see cref="Formula"/> object that can be used to evaluate the formula.
        /// </summary>
        /// <param name="formula">The input string to parse</param>
        /// <returns>A <see cref="Formula"/>.</returns>
        /// <exception cref="BadFormulaException">On bad syntax or other problems.</exception>
        public Formula CreateFormula(string formula)
        {
            var elem = this.Parse(formula);
            return new Formula(this, formula, elem, this.culture);
        }

        /// <summary>
        /// Parses the given input formula and executes it to return the result.
        /// </summary>
        /// <param name="input">The input formula.</param>
        /// <returns>The result of the formula evaluation.</returns>
        /// <exception cref="BadFormulaException">If the formula contains syntax errors or other problems.</exception>
        /// <exception cref="FormulaEvaluationException">If the evaluation fails.</exception>
        public object Evaluate(string input, ICellValueLookup cells = null)
        {
            var (_, res) = EvaluateDebug(input, null, cells);
            return res;
        }

        /// <summary>
        /// Parses the given input formula and executes it to return the result.
        /// </summary>
        /// <param name="input">The input formula.</param>
        /// <param name="variableSubstitutions"></param>
        /// <returns>The result of the formula evaluation.</returns>
        /// <exception cref="BadFormulaException">If the formula contains syntax errors or other problems.</exception>
        /// <exception cref="FormulaEvaluationException">If the evaluation fails.</exception>
        public object Evaluate(string input, Dictionary<string, object> variableSubstitutions)
        {
            var (_, res) = EvaluateDebug(input, variableSubstitutions);
            return res;
        }

        /// <summary>
        /// Parses the given input formula and executes it to return the result.
        /// </summary>
        /// <param name="input">The input formula.</param>
        /// <param name="variableSubstitutions"></param>
        /// <returns>The result of the formula evaluation.</returns>
        /// <exception cref="BadFormulaException">If the formula contains syntax errors or other problems.</exception>
        /// <exception cref="FormulaEvaluationException">If the evaluation fails.</exception>
        public (Elem elem, object result) EvaluateDebug(string input, Dictionary<string, object> variableSubstitutions, ICellValueLookup cells = null)
        {
            // TODO: Don't set this, pass it!
            this.cellValues = cells;

            // null or "" returns "", because then the result can be parsed and evaluated.
            if (string.IsNullOrEmpty(input))
                return (null, string.Empty);

            Log.Debug($"Parsing input: '{input}'");
            var elem = Parse(input.AsSpan());

            if (variableSubstitutions == null)
                return (elem, Execute(elem, null));

            Dictionary<string, Elem> parsedVariables = new Dictionary<string, Elem>();
            foreach (var subst in variableSubstitutions)
            {
                var var = ParseVariableValue(subst.Key, subst.Value);
                parsedVariables.Add(subst.Key, var);
            }
            return (elem, Execute(elem, parsedVariables));
        }

        /// <summary>
        /// Executes the parsed elem tree and returns the result.
        /// </summary>
        /// <param name="elem">The tree of parsed elements to execute.</param>
        /// <param name="variables">Variable substitutions. May be null.</param>
        /// <returns>The value of the execution. Null as input returns null as result.</returns>
        internal object Execute(Elem elem, Dictionary<string, Elem> variables = null)
        {
            if (elem == null)
                return null;
            try
            {
                switch (elem.Type)
                {
                    case ElemType.Number:
                    case ElemType.Text:
                    case ElemType.Constant:
                        return elem.Value;
                    case ElemType.Group:
                        // NOTE: Groups only have one child, the "top" elem.
                        return elem.Children.Any() ? Execute(elem.Children.First(), variables) : null;
                    case ElemType.Function:
                        var parameters = elem.Children.Select(c => Execute(c, variables)).ToList();
                        // In the case the parameter is a cell range, un-wrap it here. Better to evaluate the
                        // elem when we need it, then to have all the cell values as parameters to the function.
                        if (parameters.Count == 1 && parameters[0] is IEnumerable<object>)
                            parameters = (parameters[0] as IEnumerable<object>).ToList();
                        var fun = functions[elem.Name];
                        return fun.Evaluate(parameters, this.culture, true); // Full validation is ON!
                    case ElemType.Operator:
                        // NOTE TODO: Can definitely replace the Name with an "op-code" :-p int instead.
                        var op = operators[elem.Name];
                        // TODO: Possibly wrap the left and right calls in another try-catch to be able to add more info to the execption.
                        var left = Execute(elem.Left, variables);
                        var right = Execute(elem.Right, variables);
                        var result = op.Evaluate(left, right);
                        return result;
                    case ElemType.None:
                    case ElemType.Variable:
                        if (variables == null || !variables.TryGetValue(elem.Name, out var substitution))
                        {
                            throw new FormulaEvaluationException($"The substitution missing for variable '{elem.Name}'!");
                        }
                        return Execute(substitution, variables);
                    case ElemType.CellReference:
                        return GetCellValue(cellValues, elem.Value as Cell);
                    case ElemType.CellRange:
                        return GetCellRangeValue(cellValues, elem.Value as CellRange);
                    default:
                        throw new FormulaEvaluationException("Unknown element type! Type: " + elem.Type + ".", elem);
                }
            }
            catch (NextParsingException e)
            {
                e.Elem ??= elem;
                throw; // Will this throw the modified exception?
            }
        }

        private static IEnumerable<object> GetCellRangeValue(ICellValueLookup cellValues, CellRange cellRange)
        {
            return cellValues.GetRange((cellRange.From.Numbers, cellRange.From.Letters), (cellRange.To.Numbers, cellRange.To.Letters));
        }

        private static object GetCellValue(ICellValueLookup cellValues, Cell cell)
        {
            if (cellValues == null)
                throw new FormulaEvaluationException("There is no cell value lookup provided, but the formula contains cell references!");
            return cellValues[cell.Numbers, cell.Letters];
        }

        // TODO: Make internal again!
        public Elem Parse(ReadOnlySpan<char> input, Elem left = null, int offset = 0)
        {
            if (input.Length == 0)
            {
                return null;
            }

            var chunk = NextElement(input, offset);
            //Log.Debug($"Next elem: nextElem: '{nextElement}', rest: '{next}', next: '{next}'");
            Elem elem = null;
            Segment rest = chunk.Rest;
            var nextElement = input.Slice(chunk.Element);

            if (chunk.Element.IsEmpty)
            {
                if (chunk.Next == '(')
                {
                    (elem, rest) = ParseGroup(input.Slice(chunk.Rest), chunk.Element.Stop);
                }
                else if (chunk.Next == ')')
                {
                    while (left != null && left.Type != ElemType.Group)
                    {
                        left = left.Parent;
                    }
                    if (left == null || left.Type != ElemType.Group)
                    {
                        throw new BadFormulaException("One ')' too much? Element: (local)" + chunk.Element + "; [" + nextElement.ToString() + "]");
                    }
                }
                else
                {
                    // TODO: Hmmmm... error?!
                }
            }
            else if (char.IsLetter(nextElement[0]) && chunk.Next == '(')
            {
                (elem, rest) = ParseFunction(nextElement, input.Slice(chunk.Rest));
                rest = new Segment { Start = rest.Start + chunk.Rest.Start, Stop = rest.Stop + chunk.Rest.Stop };
            }
            else if (operators.ContainsKey(nextElement))
            {
                elem = ParseOperator(left, nextElement);
                Log.Debug($"Parsed operator: {elem.Name}", elem);
            }
            else if (decimal.TryParse(nextElement, out decimal d))
            {
                elem = ParseNumber(d);
                Log.Debug($"Parsed number: {d}", elem);
            }
            // Excel takes stuff like "1." and accepts it as "1.0" (= 1).
            else if (char.IsDigit(nextElement[0])
                && nextElement.EndsWith(this.culture.NumberFormat.NumberDecimalSeparator)
                && decimal.TryParse(nextElement.Slice(0, nextElement.Length - 1), out decimal dec))
            {
                elem = ParseNumber(dec);
            }
            else if (nextElement.StartsWith("\""))
            {
                elem = ParseText(nextElement);
            }
            else if (nextElement.StartsWith("@"))
            {
                elem = ParseVariable(nextElement);
            }
            else if (nextElement[0] == '$' ||
                (char.IsLetter(nextElement[0]) && char.IsNumber(nextElement[^1])))
            {
                elem = ParseCellReference(nextElement);
            }
            else
            {
                elem = ParseConstant(nextElement);
            }

            // Assigning index in the string is done for all elems.
            if (elem != null)
            {
                elem.Start = chunk.Element.Start;
                elem.Stop = chunk.Element.Stop;
            }

            if (elem != null && left != null && left.Type == ElemType.Operator)
            {
                FixUpOperatorThings(left, elem);
            }

            if (elem != null && !rest.IsEmpty)
            {
                // Something is not right. Nothing was taken away from the input.
                // This will cause a stack overflow.
                if (rest.Length == input.Length)
                {
                    throw new InvalidOperationException($"The parser failed to parse input; the rest product was same as input! Input: '{input}'.");
                }

                Log.Debug($"Parsing rest... '{rest}'");
                var right = Parse(input.Slice(rest), elem, chunk.Element.Stop);
                if (right == null || right == elem) // It has already been sorted.
                    return elem;

                // Some fixing left to do if this elem is an operator or the other one is.
                if (right.Type == ElemType.Operator)
                {
                    FixOperatorPrecedence(elem, right);
                }

                while (elem.Parent != null)
                    elem = elem.Parent;
                return elem;
            }

            return elem;
        }

        private void FixOperatorPrecedence(Elem elem, Elem right)
        {
            if (elem.Type != ElemType.Operator)
            {
                elem.Parent = right;
            }
            else if (elem.Type == ElemType.Operator)
            {
                var leftOp = operators[elem.Name];
                var rightOp = operators[right.Name];

                /*
                * The rank of the operator is its priority.
                * When two operators follow each other, the rank determines which of the
                * two will be executed first.
                * The operator with higher rank will be connected as the parent, with the
                * operator with lower rank connected below in the tree.
                * Since the tree is evaluated from the root/top node, using values from
                * the children, the lower ranking operator will operate first. With the
                * consequence that the higher ranking operator will operate on the result
                * of the lower ranking one.
                *
                *  9 / 3 - 1   (= 2)
                * Should be evaluated as (9 / 3) - 1. Therefore the division operator,
                * '/', is lower ranking than the subtraction operator, '-'.
                * The elem tree should therefore be
                *
                *         (-)
                *        /   \
                *      (/)   (1)
                *     /   \
                *   (9)   (3)
                *   
                */

                if (leftOp.Rank < rightOp.Rank)
                {
                    right.Left = elem;
                    elem.Parent = right;
                }
                else
                {
                    // Maybe the right one is already the parent, by 'mistake' form the parsing.
                    if (elem.Right == elem.Parent)
                    {
                        if (elem?.Right?.Parent == elem)
                        {
                            throw new NotImplementedException("This situation is weird!");
                        }
                        elem.Parent = elem.Right?.Parent;
                        if (elem.Right != null)
                        {
                            elem.Right.Parent = elem;
                        }
                    }
                    else if (right.Parent == null || right.Parent != elem)
                    {
                        elem.Right = right;
                        right.Parent = elem;
                        if (right.Left == elem)
                            right.Left = null;
                    }
                }
            }
        }

        private void FixUpOperatorThings(Elem left, Elem elem)
        {
            if (elem.Type == ElemType.Operator)
            {
                var leftParamPref = operators[left.Name].ParameterPreference;
                if (leftParamPref == OperatorParameters.PreferLeft)
                {
                    // Treat the Left as a unary operator.
                    //     (us)
                    //    /   \
                    // 50%     (?)
                    left.Right = elem;
                    if (left.Parent != null)
                    {
                        throw new BadFormulaException("Hmm.. Left already had a parent!");
                    }
                    left.Parent = elem;
                }
                else if (leftParamPref == OperatorParameters.PreferRight
                            || leftParamPref == OperatorParameters.OnlyTwo)
                {
                    // This requires us to be unary preferring right, or
                    // else the formula is bad(?)
                    //     (*)
                    //    /   \
                    //  (2)   (-)
                    //           \
                    //           (4)
                    var rightParamPref = operators[elem.Name].ParameterPreference;
                    if (rightParamPref == OperatorParameters.PreferLeft)
                    {
                        throw new BadFormulaException($"The operators '{left.Name}' and '{elem.Name}' can't follow each other!");
                    }
                    else if (rightParamPref == OperatorParameters.PreferRight)
                    {
                        left.Right = elem;
                        if (elem.Parent != null)
                        {
                            throw new BadFormulaException("Hmm.. Elem already had a parent!");
                        }
                        elem.Parent = left;
                    }
                }
                else
                {
                    throw new BadFormulaException($"The operators {left.Name} and {elem.Name} can't follow each other!", elem);
                }
                Log.Debug("Fixed up parent because of operator.", elem);
                Log.Debug("Parent:", elem.Parent);
            }
            else
            {
                left.Right = elem;
                elem.Parent = left;
                Log.Debug("Connected elem to tree. Parent:", elem.Parent);
            }
        }

        private Elem ParseCellReference(ReadOnlySpan<char> nextElement)
        {
            if (nextElement.Contains(':'))
            {
                // TODO optimize this split.
                var parts = nextElement.ToString().Split(':');
                if (parts.Length > 2)
                {
                    throw new BadFormulaException($"A cell range cannot have more than two parts! Was: '{nextElement}'");
                }
                var from = ParseCell(parts[0]);
                var to = ParseCell(parts[1]);
                return new Elem($"{from.letterPart}{from.number}:{to.letterPart}{to.number}", ElemType.CellRange)
                {
                    // TODO Remove this.
                    //Text = nextElement.ToString(),
                    Value = new CellRange
                    {
                        From = new Cell { Letters = from.letterPart, Numbers = from.number },
                        To = new Cell { Letters = to.letterPart, Numbers = to.number }
                    }
                };

            }
            else
            {
                var (letterPart, number) = ParseCell(nextElement);
                return new Elem($"{letterPart}{number}", ElemType.CellReference)
                {
                    //Text = nextElement.ToString(),
                    Value = new Cell { Letters = letterPart, Numbers = number }
                };
            }
        }

        private (string letterPart, int number) ParseCell(ReadOnlySpan<char> nextElement)
        {
            int index = nextElement.Length - 1;
            char c = nextElement[index];
            while (Char.IsDigit(c))
            {
                index--;
                c = nextElement[index];
            }
            var numberPart = nextElement.Slice(index + 1);
            var number = int.Parse(numberPart);
            var letterPart = nextElement.Slice(0, nextElement.Length - numberPart.Length);
            letterPart = letterPart.Trim('$'); // We don't need to care about the absolute refs, right?

            return (letterPart.ToString(), number);
        }

        private Elem ParseVariableValue(string variableName, object val)
        {
            if (val is null)
                return new Elem("Null", ElemType.Constant);

            if (val is string)
                return Parse(val as string);

            if (val is byte
                || val is short
                || val is int
                || val is long
                || val is float
                || val is double
                || val is decimal)
            {
                return new Elem(string.Empty, ElemType.Number)
                {
                    Value = Convert.ToDecimal(val),
                };
            }
            else if (val is DateTime)
            {
                return new Elem(string.Empty, ElemType.Constant)
                {
                    Value = val,
                };
            }

            var type = val.GetType().FullName;
            throw new BadFormulaException($"The given variable substitution was of unhandled type! Variable '{variableName}', type: '{type}' value: '{val}'.");
        }

        private Elem ParseVariable(ReadOnlySpan<char> nextElement)
        {
            Elem elem = new Elem(nextElement.ToString(), ElemType.Variable)
            {
                ValueType = ValueType.Any, // Can't know type until substitution during execution
            };
            return elem;
        }

        private Elem ParseOperator(Elem left, ReadOnlySpan<char> nextElement)
        {
            Elem elem;
            var op = operators[nextElement.ToString()];

            op.ValidateParameter(left);

            elem = new Elem(op.Name, ElemType.Operator)
            {
                Left = left,
                ValueType = op.ValueType
            };
            return elem;
        }

        private static Elem ParseNumber(decimal d)
        {
            return new Elem("", ElemType.Number)
            {
                // TODO Remove this
                //Text = nextElement.ToString(),
                Value = d,
                ValueType = ValueType.Number,
            };
        }

        private static Elem ParseText(ReadOnlySpan<char> nextElement)
        {
            if (!nextElement.EndsWith("\""))
                throw new BadFormulaException("Missing closing '\"' for text block!");

            return new Elem("", ElemType.Text)
            {
                // TODO Remove this.
                //Text = nextElement.Trim('\"').ToString(),
                Value = nextElement.Trim('\"').ToString(), // TODO User segment instead of string!
                ValueType = ValueType.Text,
            };
        }

        private (Elem elem, Segment rest) ParseGroup(ReadOnlySpan<char> input, int offset = 0)
        {
            List<((int, int), string)> throwAwayParas;
            var parametersResult = GetParameters(input, offset);
            var part = input.Slice(parametersResult.ParameterPart).Trim().TrimOneSpan(new[] { '(', ')' });
            var elem = new Elem("(...)", ElemType.Group)
            {
                // TODO Remove this.
                //Text = part.ToString(),
            };
            var child = Parse(part, left: null, offset);
            if (child != null)
            {
                elem.Children.Add(child);
                child.Parent = elem;
                elem.ValueType = child.ValueType;
            }

            return (elem, parametersResult.Rest);
        }

        private Elem ParseConstant(ReadOnlySpan<char> nextElement)
        {
            // ATM only handles bool constants.

            if (Boolean.TryParse(nextElement, out bool val))
            {
                var elem = new Elem("", ElemType.Constant)
                {
                    Value = val,
                    // TODO Remove this
                    //Text = nextElement.ToString(),
                    ValueType = ValueType.Boolean,
                };
                return elem;
            }
            else
            {
                if (nextElement.Trim() == this.culture.TextInfo.ListSeparator)
                    throw new BadFormulaException($"Misplaced list separator, '{nextElement}'!");
                throw new BadFormulaException("Unknown constant: '" + nextElement.ToString() + "'.");
            }
        }

        private (Elem, Segment) ParseFunction(ReadOnlySpan<char> nextElement, ReadOnlySpan<char> rest, int offset = 0)
        {
            var funName = nextElement.ToString();
            // Do we have the function?
            var fun = GetFunctionByName(funName)
                            ?? throw new BadFormulaException($"Missing function: '{funName}'.");

            var parametersResult = GetParameters(rest, offset);
            var part = rest.Slice(parametersResult.ParameterPart).Trim(); // Don't need the whitespace
            if (!part.EndsWith(")"))
                throw new BadFormulaException("Missing closing parenthesis on function call!");

            var parsedParameters = new List<Elem>();
            foreach (var param in parametersResult.Parameters)
            {
                var parameter = Parse(param.Text, left: null, param.Start);
                parsedParameters.Add(parameter);
            }

            fun.CheckParameters(parsedParameters);

            // TODO Remove name string. Use int?
            var elem = new Elem(funName, ElemType.Function)
            {
                ValueType = fun.ValueType,
                // TODO remove this
                //Text = nextElement.ToString(),
            };
            elem.Children.AddRange(parsedParameters);
            return (elem, parametersResult.Rest);
        }

        private Function GetFunctionByName(string name)
        {
            if (!this.functions.TryGetValue(name, out var fun))
                this.localizedFunctionNames.TryGetValue(name, out fun);
            return fun;
        }

        public ParametersResult GetParameters(ReadOnlySpan<char> input, int offset)

        {
            if (input[0] != '(')
                throw new BadFormulaException("The parameter input was missing an opening parenthesis!");

            var list = new List<ParsedParameter>();

            int left = 0;
            int right = 0;

            int index = 0;
            int start = 1; // first is always a '('
            bool insideQuote = false;
            bool addParam = false;
            while (index < input.Length
                   && (left == 0 || left != right))
            {
                char c = input[index];

                if (c == '\"')
                {
                    insideQuote = !insideQuote;
                }
                else if (!insideQuote)
                {
                    if (c == '(')
                    {
                        if (left == 0)
                            start = index + 1;
                        left++;
                    }
                    else if (c == ')')
                    {
                        right++;
                        addParam = left - right == 0;
                        //if (addParam)
                        //    index++;
                    }
                    // We can add parameters while we are at the "base" level.
                    // MAX(1, IF(2, 3), 4)
                    // 1, and IF(2, 4), and 4 are at the level where '(' is one more than ')'
                    // NOTE TODO: Looks like this assumed the list separator is only one char.
                    else if (c == culture.TextInfo.ListSeparator[0])
                    {
                        addParam = left - right == 1;
                    }

                    if (addParam)
                    {
                        addParam = false;

                        var stop = index - start;
                        var param = input.Slice(start, stop).ToString().Trim();

                        if (!string.IsNullOrWhiteSpace(param)) // Hmm.. having a blank string at this point should be an error?
                        {
                            var para = new ParsedParameter
                            {
                                Start = start + offset,
                                Stop = stop + offset,
                                Text = param,
                            };
                            list.Add(para);
                        }

                        start = index + 1;
                    }
                }

                index++;
            }

            var reminder = GetRest(input, index);
            var part = new Segment(0, Math.Min(input.Length, index));
            var result = new ParametersResult
            {
                Parameters = list,
                ParameterPart = part,
                Rest = reminder
            };
            return result;
        }

        /// <summary>
        /// Parses the next parse-able token from the input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="offset">The index of input in the input string.</param>
        /// <returns>A ChunkResult, pointing out the elem and the next char.</returns>
        public ChunkResult NextElement(ReadOnlySpan<char> input, int offset = 0)
        {
            if (input.IsEmpty)
                return new ChunkResult
                {
                    Element = new Segment(offset, offset),
                    Next = '\0',
                };

            bool haveQuote = false;
            bool trimmed = false;
            int i = 0;
            int start = 0;
            while (i < input.Length)
            {
                char c = input[i];

                if ('\"' == c)
                {
                    if (haveQuote)
                    {
                        i++; // Include \" in at the end.
                        break;
                    }
                    else haveQuote = true;
                }
                // If we are inside quotes, only \" counts.
                // If not inside quotes, everything goes!
                else if (!haveQuote)
                {
                    if (char.IsWhiteSpace(c)) // If we don't accept all UniCode whitespace chars, this can be faster.
                    {
                        // Skip initial whitespace
                        if (!trimmed)
                        {
                            i++;
                            start = i;
                            continue;
                        }
                        // But if this is whitespace after some other chars, break!
                        else break;
                    }
                    else
                    {
                        string chr = c.ToString();
                        trimmed = true;
                        if ('(' == c)
                            break;
                        // The list separator is a string, as it doesn't have to be just 1 char.
                        else if (culture.TextInfo.ListSeparator == chr)
                        {
                            // Do we have something in the pipe before the separator came?
                            // If not, the separator is the thing.
                            if (i - start <= 0)
                                i++;
                            break;
                        }
                        else if (')' == c)
                            break;

                        if (operatorStartingChars.Contains(c))
                        {
                            // If there was something before the operator, return that first.
                            if (i - start > 0)
                                break;

                            i++; // Include that first char.
                            string operatorWord = chr;

                            // Let's try to do a local search for operators.
                            while (i < input.Length - 1)
                            {
                                operatorWord += input[i];
                                if (!operators.Any(op => op.Key.StartsWith(operatorWord)))
                                    break; // Done, return part with the longest operator that matched
                                i++; // Also include this char
                            }
                            break;
                        }
                    }
                }

                i++;
            }

            char next = '\0';
            if (i >= 0 && i < input.Length)
                next = input[i];

            // We must chew off at least one char each time, or else we will loop forever.
            var chunk = new ChunkResult
            {
                Element = new Segment { Start = start, Stop = i },
                Rest = GetRest(input, i),
                Next = next
            };
            return chunk;
        }

        private static Segment GetRest(ReadOnlySpan<char> input, int i)
        {
            return input.Length <= i ?
                new Segment { Start = i, Stop = i }
                : new Segment { Start = i, Stop = input.Length }; // It's OK to overshoot.
        }

        private void LocalizeFunctionNames()
        {
            var translated = new Dictionary<string, Function>();
            foreach (var function in this.functions.Values)
            {
                var translation = TextManager.GetText(function.TextId);
                if (!translated.TryAdd(translation, function))
                {
                    throw new ArgumentException($"Double entry for function '{function.Name}' with textId = {function.TextId} and translated name '{translation}'!");
                }
            }
            this.localizedFunctionNames = translated;
        }
    }
}