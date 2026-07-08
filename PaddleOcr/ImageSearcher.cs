using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaddleOcr
{
    public class ImageSearcher
    {
        private readonly PaddleOcrService _ocr;

        public ImageSearcher(PaddleOcrService ocr)
        {
            _ocr = ocr;
        }

        public Task<List<MatchResult>> SearchAsync(
            IEnumerable<string> images,
            IEnumerable<string> keywords,
            StringComparison comparison = StringComparison.Ordinal,
            Action<int, int> progress = null)
        {
            ArgumentNullException.ThrowIfNull(images);

            var imageArray = images as string[] ?? new List<string>(images).ToArray();
            KeywordMatcher matcher = new(keywords, comparison);
            List<MatchResult> results = new();

            for (var i = 0; i < imageArray.Length; i++)
            {
                var image = imageArray[i];
                progress?.Invoke(i + 1, imageArray.Length);

                var text = _ocr.Recognize(image);
                var hit = matcher.Match(text);

                if (hit.Count == 0)
                {
                    continue;
                }

                results.Add(new MatchResult
                {
                    Image = image,
                    Text = text,
                    Keywords = hit
                });
            }

            return Task.FromResult(results);
        }
    }
}
