namespace Gotoland.NextParserV2
{
    public struct ChunkResult
    {
        public Segment Element { get; set; }
        public Segment Rest { get; set; }
        public char Next { get; set; }

        public override string ToString()
        {
            return $"(Elem: {Element}, Rest: {Rest}, Next: '{Next.ToString()}')";
        }
    }
}