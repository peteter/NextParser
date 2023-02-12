namespace Gotoland.NextParserV2
{
    /// <summary>
    /// This class imitates a legacy localization class that lookup strings with an id.
    /// </summary>
    public static class TextManager
    {
        public static string GetText(int id)
        {
            return id.ToString();
        }
    }
}