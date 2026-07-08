using System.Collections.Generic;

namespace PaddleOcr
{
    public class MatchResult
    {
        public string Image { get; set; } = "";

        public string Text { get; set; } = "";

        public List<string> Keywords { get; set; } = new List<string>();
    }
}
