using System;
using Gotoland.NextParser;

namespace Gotoland.NextParser.Tests
{
    [TestClass]
    public class NextHelperMethodTests
    {
        [TestMethod]
        public void FormulaNext_NextElement()
        {
            VerifyNextElement("MAX(2, 3)", (0, 3), "MAX", "(2, 3)", '(');
            VerifyNextElement("MAX(MAX(2, 3), 5)", (0, 3), "MAX", "(MAX(2, 3), 5)", '(');
            VerifyNextElement("MAX(2, 3), 5)", (0, 3), "MAX", "(2, 3), 5)", '(');
            VerifyNextElement("   2", (3, 4), "2", "", '\0');
            VerifyNextElement("true  ", (0, 4), "true", "  ", ' ');
        }

        [TestMethod]
        public void FormulaNext_GetParameters()
        {
            Verify("(2, 3)", "(2, 3)", "2", "3");
            Verify("(MAX(1), 4)", "(MAX(1), 4)", "MAX(1)", "4");
            Verify("(MAX(9, 8), 7)", "(MAX(9, 8), 7)", "MAX(9, 8)", "7");
            Verify("(IF(TRUE, \"text\", 2))", "(IF(TRUE, \"text\", 2))", "IF(TRUE, \"text\", 2)");
        }

        [TestMethod]
        public void FormulaNext_NextElement_MultiCharOperator()
        {
            var input = " >= 3";
            var chunk = new Next().NextElement(input);
            Assert.AreEqual(">=", input[chunk.Element.Start..chunk.Element.Stop]);

            input = " > 3";
            chunk = new Next().NextElement(input);
            Assert.AreEqual(">", input[chunk.Element.Start..chunk.Element.Stop]);

            input = " &3";
            chunk = new Next().NextElement(input);
            Assert.AreEqual("&", input[chunk.Element.Start..chunk.Element.Stop]);

            input = "&&&";
            chunk = new Next().NextElement(input);
            Assert.AreEqual(new ChunkResult
            {
                Element = new Segment { Start = 0, Stop = 1 },
                Next = '&',
                Rest = new Segment { Start = 1, Stop = 3 },
            }, chunk);
        }

        [TestMethod]
        public void FormulaNext_NextElement_CharAndOperatorTightly()
        {
            string elem, rest;
            char next;
            int start, stop;

            var chunk = new Next().NextElement("-10");
            Assert.AreEqual("-", "-10"[chunk.Element.Start..chunk.Element.Stop]);

            chunk = new Next().NextElement("-");
            Assert.AreEqual("-", "-"[chunk.Element.Start..chunk.Element.Stop]);

            chunk = new Next().NextElement("10-");
            Assert.AreEqual("10", "10-"[chunk.Element.Start..chunk.Element.Stop]);
        }

        private static void Verify((int start, int stop) position, string elem, string rest, char next, ((int start, int stop) position, string element, string rest, char next) actual)
        {
            Assert.AreEqual(position.start, actual.position.start);
            Assert.AreEqual(position.stop, actual.position.stop);
            Assert.AreEqual(elem, actual.element, "The element differed!");
            Assert.AreEqual(next, actual.next, "The next char differed!");
            Assert.AreEqual(rest, actual.rest, "The rest differed!");
        }

        private static void Verify(string input, string part, params string[] parameters)
        {
            var actual = new Next().GetParameters(input, 0);

            Assert.AreEqual(part, actual.ParameterPart.SubString(input), "Part was not same!");
            for (int i = 0; i < Math.Min(parameters.Length, actual.Parameters.Count); i++)
            {
                Assert.AreEqual(parameters[i], actual.Parameters[i].Text);
            }
            Assert.AreEqual(parameters.Length, actual.Parameters.Count, "Number of params not same!");
        }

        private static void VerifyNextElement(string input, (int start, int stop) position, string elem, string rest, char next)
        {
            var chunk = new Next().NextElement(input);
            Assert.AreEqual(position.start, chunk.Element.Start);
            Assert.AreEqual(position.stop, chunk.Element.Stop);
            Assert.AreEqual(elem, chunk.Element.SubString(input));
            Assert.AreEqual(rest, chunk.Rest.SubString(input));
            Assert.AreEqual(next, chunk.Next);
        }
    }
}