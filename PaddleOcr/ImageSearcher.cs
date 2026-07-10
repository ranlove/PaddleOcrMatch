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

        public Task<int> SearchAsync(
            IEnumerable<string> images,
            IEnumerable<string> keywords,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase,
            Action<MatchResult> onMatch = null,
            Action<int, int, string> progress = null)
        {
            ArgumentNullException.ThrowIfNull(images);

            var imageArray = images as string[] ?? new List<string>(images).ToArray();
            KeywordMatcher matcher = new(keywords, comparison);
            if (imageArray.Length == 0)
            {
                return Task.FromResult(0);
            }

            var matchedCount = 0;
            for (var i = 0; i < imageArray.Length; i++)
            {
                var image = imageArray[i];
                progress?.Invoke(i + 1, imageArray.Length, image);

                string text;
                try
                {
                    text = _ocr.Recognize(image);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to recognize image: {image}", ex);
                }

                var hit = matcher.Match(text);
                if (hit.Count > 0)
                {
                    try
                    {
                        onMatch?.Invoke(new MatchResult
                        {
                            Image = image,
                            Text = text,
                            Keywords = hit
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to handle matched image: {image}", ex);
                    }

                    matchedCount++;
                }
            }

            return Task.FromResult(matchedCount);
        }
    }
}
