using System;
using System.Collections.Generic;

namespace PaddleOcr
{
    public class KeywordMatcher
    {
        private readonly string[] _keywords;
        private readonly StringComparison _comparison;

        public KeywordMatcher(IEnumerable<string> keywords, StringComparison comparison = StringComparison.Ordinal)
        {
            ArgumentNullException.ThrowIfNull(keywords);
            _comparison = comparison;

            List<string> normalizedKeywords = new();
            HashSet<string> seen = new(GetComparer(comparison));

            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    continue;
                }

                var normalizedKeyword = keyword.Trim();
                if (seen.Add(normalizedKeyword))
                {
                    normalizedKeywords.Add(normalizedKeyword);
                }
            }

            _keywords = normalizedKeywords.ToArray();
        }

        public List<string> Match(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            List<string> matches = new();
            foreach (var keyword in _keywords)
            {
                if (text.Contains(keyword, _comparison))
                {
                    matches.Add(keyword);
                }
            }

            return matches;
        }

        private static StringComparer GetComparer(StringComparison comparison)
        {
            return comparison switch
            {
                StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
                StringComparison.CurrentCulture => StringComparer.CurrentCulture,
                StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
                StringComparison.InvariantCulture => StringComparer.InvariantCulture,
                StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
                _ => StringComparer.Ordinal
            };
        }
    }
}
