using System;
using System.Globalization;

namespace Gotoland.NextParserV2
{
    public class Formula
    {
        private Next parser;

        public string InputFormula { get; }
        internal Elem ParsedFunctionTree { get; private set; }
        public CultureInfo Cuture { get; }

        internal Formula(Next parser, string input, Elem tree, CultureInfo culture)
        {
            InputFormula = input;
            ParsedFunctionTree = tree;
            Cuture = culture;
            this.parser = parser;
        }

        public object Execute()
        {
            return parser.Execute(ParsedFunctionTree);
        }
    }
}