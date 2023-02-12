using System;

namespace Gotoland.NextParser
{
    public struct Segment
    {
        private static readonly Segment undefined = new Segment() { Start = -1, Stop = -1 };

        public Segment(int start, int stop)
        {
            Start = start;
            Stop = stop;
        }

        public static Segment Undefined => undefined;
        public int Start { get; init; }
        public int Stop { get; init; }
        public bool IsEmpty => Stop - Start == 0;

        public int Length => Stop - Start;

        public string SubString(string input)
        {
            return input?.Substring(Start, -Start + Math.Min(Stop, input.Length));
        }

        public override string ToString()
        {
            return $"{{{Start}, {Stop}}}";
        }
    }
}