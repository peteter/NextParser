using System.Collections.Generic;

namespace Gotoland.NextParser
{
    public struct ParametersResult
    {
        public List<ParsedParameter> Parameters;
        public Segment ParameterPart;
        public Segment Rest;
    }

    public struct ParsedParameter
    {
        public int Start;
        public int Stop;
        public string Text;
    }
}