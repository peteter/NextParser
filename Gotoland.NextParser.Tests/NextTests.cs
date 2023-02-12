using System.Diagnostics;
using System.Globalization;
using Gotoland.NextParser;

namespace Gotoland.NextParser.Tests
{
    [TestClass]
    public class NextTests
    {
        [TestMethod]
        public void FormulaNext_Evaluate_Number()
        {
            Assert.ThrowsException<BadFormulaException>(() => new Next().Evaluate("1" + CultureInfo.InvariantCulture.TextInfo.ListSeparator));

            Assert.AreEqual(1m, new Next().Evaluate("1"));

            Assert.AreEqual(1m, new Next().Evaluate("1" + CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Number_Negative()
        {
            Assert.AreEqual(-1m, new Next().Evaluate("-1"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Parenthesis()
        {
            Assert.AreEqual(null, new Next().Evaluate("()"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_BlankSpace()
        {
            Assert.AreEqual(null, new Next().Evaluate(" \t\r\n"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Function_NoParameters()
        {
            Assert.ThrowsException<BadFormulaException>(() =>
                new Next().Evaluate("PI"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Function_WithoutParameters()
        {
            Assert.AreEqual(Convert.ToDecimal(Math.PI), new Next().Evaluate("PI()"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Function_TwoParameters_BadFormat()
        {
            Assert.ThrowsException<BadFormulaException>(() =>
                new Next().Evaluate("MAX(3, 2"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Function_TwoParameters()
        {
            Assert.AreEqual(3m, new Next().Evaluate("MAX(3, 2)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Function_TwoParameters_ComplexParameters()
        {
            Assert.AreEqual(51m, new Next().Evaluate("MAX(100/2, 100-49)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_StringLiteral_BadFormat()
        {
            Assert.ThrowsException<BadFormulaException>(() =>
                new Next().Evaluate("\"hello"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_StringLiteral()
        {
            Assert.AreEqual("hello", new Next().Evaluate("\"hello\""));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_StringLiteral_Whitespace()
        {
            Assert.AreEqual("hello", new Next().Evaluate("  \"hello\"\t"));
        }


        [TestMethod]
        public void FormulaNext_Evaluate_StringLiteral_Function()
        {
            Assert.AreEqual("hello, this is my FUN(1, 2) + 8 ", new Next().Evaluate("  \"hello, this is my FUN(1, 2) + 8 \""));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Number_BadFormat()
        {
            Assert.ThrowsException<BadFormulaException>(() =>
                new Next().Evaluate("1="));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Addition()
        {
            Assert.AreEqual(3m, new Next().Evaluate("1 + 2"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Addition_Many()
        {
            Assert.AreEqual(6m, new Next().Evaluate("1 + 2 + 3"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Addition_And_Subtraction()
        {
            Assert.AreEqual(9m, new Next().Evaluate("8 - 2 + 3"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Subtraction()
        {
            Assert.AreEqual(-1m, new Next().Evaluate("1 - 2"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Subtraction_NegativeNumber()
        {
            Assert.AreEqual(3m, new Next().Evaluate("1 - -2"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_NegativeFunction()
        {
            Assert.AreEqual(-3m, new Next().Evaluate("-MAX(3, 2)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_NestledFunctions()
        {
            Assert.AreEqual(8m, new Next().Evaluate("MAX(MAX(3, 2), 8)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Addition_FunctionAndNumber()
        {
            Assert.AreEqual(11m, new Next().Evaluate("MAX(3, 2, 1) + 8"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Addition_FunctionAndFunction()
        {
            Assert.AreEqual(23m, new Next().Evaluate("MAX(3, 2, 1) + MAX(20, 20)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Addition_WithNumbersAndParenthesis()
        {
            Assert.AreEqual(26m, new Next().Evaluate("(3 + 1 + 2) + 20"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_BooleanConstant()
        {
            Assert.AreEqual(true, new Next().Evaluate("true"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_FunctionWithTextParameters()
        {
            Assert.AreEqual(2m, new Next().Evaluate("COUNT(\"a\", \"b\")"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_If_Boolean()
        {
            Assert.AreEqual("True", new Next().Evaluate("IF(true, \"True\", \"False\")"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_If_String()
        {
            Assert.AreEqual(1m, new Next().Evaluate("IF(\"true\", 1, 2)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Ifs()
        {
            Assert.AreEqual(1m, new Next().Evaluate("IFS(\"true\", 1, \"false\", 2)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Ifs_Else()
        {
            Assert.AreEqual(3m, new Next().Evaluate("IFS(False, 1, False, 2, 3)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Ifs_TooFewParams()
        {
            Assert.ThrowsException<InvalidParameterException>(() => new Next().Evaluate("IFS(False)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Or_String()
        {
            Assert.AreEqual(true, new Next().Evaluate("OR(\"true\", \"false\")"));
            Assert.AreEqual(false, new Next().Evaluate("OR(\"false\", \"false\")"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_COUNT()
        {
            Assert.AreEqual(10m, new Next().Evaluate("COUNT(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Number_BadNumberFormat()
        {
            Assert.ThrowsException<BadFormulaException>(() =>
                new Next().Evaluate("2e"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_NegativeNegative()
        {
            Assert.AreEqual(7m, new Next().Evaluate("10 - (5 - 2)"));
            Assert.AreEqual(7m, new Next().Evaluate("10-(5-2)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_SubtractionOfFunctionValue()
        {
            Assert.AreEqual(5m, new Next().Evaluate("10 - MAX(5, 4)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Multiplication()
        {
            Assert.AreEqual(20m, new Next().Evaluate("10 * 2"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Division()
        {
            Assert.AreEqual(5m, new Next().Evaluate("10 / 2"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_OperatorPriority_DivisionOverSubtraction_FromRight()
        {
            Assert.AreEqual(8m, new Next().Evaluate("10 - 4 / 2"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_OperatorPriority_DivisionOverSubtraction_FromLeft()
        {
            Assert.AreEqual(2m, new Next().Evaluate("9 / 3 - 1"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_OperatorPriority_MultiplicationOverAddition_FromLeft()
        {
            Assert.AreEqual(21m, new Next().Evaluate("10 * 2 + 1"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_OperatorPriority_MultiplicationOverAddition_FromRight()
        {
            Assert.AreEqual(21m, new Next().Evaluate("1 + 10 * 2"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_WrongNumberOfParameters()
        {
            Assert.ThrowsException<InvalidParameterException>(() =>
                new Next().Evaluate("NEG()"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_WrongTypeOfParameter()
        {
            Assert.ThrowsException<InvalidParameterException>(() =>
                new Next().Evaluate("NEG(\"text\")"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_WrongTypeOfParameter_WhenExecuting()
        {
            Assert.ThrowsException<InvalidParameterException>(() =>
                new Next().Evaluate("NEG(IF(TRUE, \"text\", 2))"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_FunctionWithFunctionParameter()
        {
            Assert.AreEqual(-1m, new Next().Evaluate("NEG(IF(TRUE, 1, 2))"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_NegationOperator()
        {
            Assert.AreEqual(false, new Next().Evaluate("!True"));
            Assert.AreEqual(false, new Next().Evaluate("!1"));
            Assert.AreEqual(false, new Next().Evaluate("!10"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_GroupWithParenthesisOnly()
        {
            Assert.AreEqual(3m, new Next().Evaluate(" ( MAX(1, 3, 2) )"));
            Assert.AreEqual(8m, new Next().Evaluate("(MAX(2, 3, 8))"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_GroupOfValue()
        {
            Assert.AreEqual(1m, new Next().Evaluate("(1)"));
            Assert.AreEqual(1m, new Next().Evaluate(" ( 1 ) "));
            Assert.AreEqual(1m, new Next().Evaluate(" ( 1)"));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_GroupOfGroupOfValue()
        {
            Assert.AreEqual(1m, new Next().Evaluate("((1))"));
            Assert.AreEqual(1m, new Next().Evaluate(" (( 1 )) "));
            Assert.AreEqual(1m, new Next().Evaluate("( ( 1) )"));
        }

        [TestMethod]
        public void FormulaNext_Parse_Variable()
        {
            new Next().Parse("@var");
        }

        [TestMethod]
        public void FormulaNext_Parse_Variable_Evaluate_MissingSubstitution()
        {
            try
            {
                new Next().Evaluate("@var");

            }
            catch (FormulaEvaluationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("@var"));
                return;
            }
            Assert.Fail("Should not get here!");
        }

        [TestMethod]
        public void FormulaNext_Parse_Variable_Evaluate_Withsubstitution_Number()
        {
            Assert.AreEqual(3m, new Next().Evaluate("@var", new Dictionary<string, object> { { "@var", (byte)3 } }));
        }

        [TestMethod]
        public void FormulaNext_Parse_Variable_Evaluate_Withsubstitution_Text()
        {
            Assert.AreEqual("Hello!", new Next().Evaluate("@var", new Dictionary<string, object> { { "@var", "\"Hello!\"" } }));
        }

        [TestMethod]
        public void FormulaNext_Parse_Variable_Evaluate_Withsubstitution_DateTime()
        {
            Assert.AreEqual(new DateTime(2019, 8, 4), new Next().Evaluate("@var", new Dictionary<string, object> { { "@var", new DateTime(2019, 8, 4) } }));
        }

        [TestMethod]
        public void FormulaNext_Parse_Variable_Evaluate_Withsubstitution_Function()
        {
            Assert.AreEqual(29m, new Next().Evaluate("@var", new Dictionary<string, object> { { "@var", "ABS( -29)" } }));
        }

        [TestMethod]
        public void FormulaNext_Parse_Variable_Evaluate_Withsubstitution_InFunction()
        {
            Assert.AreEqual(100m, new Next().Evaluate("MAX(1, @var, 10)", new Dictionary<string, object> { { "@var", 100 } }));
        }

        [TestMethod]
        public void FormulaNext_Parse_Variable_Evaluate_Withsubstitution_Mix()
        {
            var variables = new Dictionary<string, object>()
            {
                { "@one", 1 },
                { "@fun", "COUNT(0, 0)" }
            };
            Assert.AreEqual(2m, new Next().Evaluate("MAX(@one, @fun, 0)", variables));
        }

        [TestMethod]
        public void FormulaNext_Parse_Variable_Evaluate_Withsubstitution_CrossReference()
        {
            var variables = new Dictionary<string, object>()
            {
                { "@fun", "MAX(@one, 0)" },
                { "@one", 1 },
            };
            Assert.AreEqual(19m, new Next().Evaluate("ABS(@fun - 20)", variables));
        }

        [TestMethod]
        public void FormulaNext_Parse_Operator_LessThan()
        {
            Assert.AreEqual(true, new Next().Evaluate("2 < 3"));
            Assert.AreEqual(false, new Next().Evaluate("3 < 3"));
            Assert.AreEqual(false, new Next().Evaluate("4 < 3"));
        }

        [TestMethod]
        public void FormulaNext_Parse_Operator_LessThanOrEqual()
        {
            Assert.AreEqual(true, new Next().Evaluate("2 <= 3"));
            Assert.AreEqual(true, new Next().Evaluate("3 <= 3"));
            Assert.AreEqual(false, new Next().Evaluate("4 <= 3"));
        }

        [TestMethod]
        public void FormulaNext_Parse_Operator_GreaterThan()
        {
            Assert.AreEqual(false, new Next().Evaluate("2 > 3"));
            Assert.AreEqual(false, new Next().Evaluate("3 > 3"));
            Assert.AreEqual(true, new Next().Evaluate("4 > 3"));
        }

        [TestMethod]
        public void FormulaNext_Parse_Operator_GreaterThanOrEqual()
        {
            Assert.AreEqual(false, new Next().Evaluate("2 >= 3"));
            Assert.AreEqual(true, new Next().Evaluate("3 >= 3"));
            Assert.AreEqual(true, new Next().Evaluate("4 >= 3"));
        }

        [TestMethod]
        public void FormulaNext_Parse_Operator_Exponential()
        {
            Assert.AreEqual(4m, new Next().Evaluate("2^2"));
            Assert.AreEqual(1m, new Next().Evaluate("2^0"));
            Assert.AreEqual(16m, new Next().Evaluate("4^2"));
        }

        [TestMethod]
        public void FormulaNext_Parse_Operator_Percent()
        {
            Assert.AreEqual(25m, new Next().Evaluate("50% * (100 / 2)"));
            Assert.AreEqual(25m, new Next().Evaluate("25% * 100"));
            Assert.AreEqual(25m, new Next().Evaluate("25%* 100"));
        }

        [TestMethod]
        public void FormulaNext_Parse_Multiplication()
        {
            Assert.AreEqual(3m, new Next().Evaluate("(1+2)"));
            Assert.AreEqual(9m, new Next().Evaluate("(1+2)*3"));
        }

        [TestMethod]
        public void FormulaNext_Parse_CellReference()
        {
            ICellValueLookup cells = new CellValues(new Dictionary<string, object>
            {
                { "C4", 10},
                { "A10", 100 },
                { "D9", 1000},
            });

            Assert.AreEqual(14m, new Next().Evaluate("SUM(4, 10)", cells));
            Assert.ThrowsException<FormulaEvaluationException>(() => new Next().Evaluate("SUM(C4,A10)", (ICellValueLookup)null));
            Assert.AreEqual(110m, new Next().Evaluate("SUM(C4,A10)", cells));
            Assert.AreEqual(1110m, new Next().Evaluate("SUM(C$4,$A$10, $D9)", cells));
        }

        [TestMethod]
        public void FormulaNext_Parse_CellRange()
        {
            ICellValueLookup cells = new TestRangeCellValueLookup();

            Assert.AreEqual(5044m, new Next().Evaluate("SUM(C4:C100)", cells));
        }

        [TestMethod]
        public void FormulaNext_Evaluate_Date_Operator_Date()
        {
            Assert.AreEqual(new TimeSpan(1, 0, 0, 0),
                new Next().Evaluate("DATE(2023, 01, 01) - DATE(2022, 12, 31)"));
        }

        /// <summary>
        /// Test creation.
        /// </summary>
        [TestMethod, Ignore]
        public void FormulaEngine_Creation()
        {
            for (int i = 0; i < 1000; i++)
            {
                var engine = new Next(CultureInfo.InvariantCulture);
                engine.Evaluate("IF(OR(\"true\", \"false\"), 1, 2)");
                var txt = engine.ToString();
            }

            for (int a = 0; a < 10; a++)
            {
                var timer = new Stopwatch();

                //var engine = new Next(CultureInfo.InvariantCulture);

                Thread.Sleep(200);
                GC.GetTotalMemory(true);
                Thread.Sleep(200);

                timer.Start();
                for (int i = 0; i < 1000; i++)
                {
                    var engine = new Next(CultureInfo.InvariantCulture);
                    engine.Evaluate("IF(OR(\"true\", \"false\"), 1, 2)");
                }
                timer.Stop();
                Console.WriteLine(timer.ElapsedMilliseconds);
            }
        }

        public class TestRangeCellValueLookup : ICellValueLookup
        {
            public object this[int row, string column] => row;
            public object this[string sheet, int row, string column] => row;

            public IEnumerable<object> GetRange((int row, string column) from, (int row, string column) to)
            {
                for (int y = from.row; y <= to.row; y++)
                {
                    yield return this[y, from.column];
                }
            }
        }
    }
}