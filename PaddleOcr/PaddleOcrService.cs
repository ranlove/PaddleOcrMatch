using System;
using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;

namespace PaddleOcr
{
    public sealed class PaddleOcrService : IDisposable
    {
        private readonly PaddleOcrAll _ocr;
        private readonly PaddleOcrOptions _options;

        public PaddleOcrService(PaddleOcrOptions options = null)
        {
            _options = options ?? new PaddleOcrOptions();
            _ocr = CreateOcrEngine(_options);
        }

        public string Recognize(string imageFile)
        {
            using var src = Cv2.ImRead(imageFile, ImreadModes.Color);
            if (src.Empty())
            {
                throw new InvalidOperationException($"Unable to load image: {imageFile}");
            }

            using var normalized = ResizeIfNeeded(src, _options.MaxImageSideLength);
            var result = _ocr.Run(normalized);
            return result.Text;
        }

        public void Dispose()
        {
            _ocr.Dispose();
        }

        private static PaddleOcrAll CreateOcrEngine(PaddleOcrOptions options)
        {
            PaddleOcrAll ocr = new(LocalFullModels.ChineseV5, config => ConfigureCpuInference(config, options))
            {
                AllowRotateDetection = options.AllowRotateDetection,
                Enable180Classification = options.Enable180Classification
            };
            ocr.Detector.MaxSize = options.DetectorMaxSize;

            return ocr;
        }

        private static Mat ResizeIfNeeded(Mat src, int? maxImageSideLength)
        {
            if (!maxImageSideLength.HasValue)
            {
                return src.Clone();
            }

            var maxSide = Math.Max(src.Width, src.Height);
            if (maxSide <= maxImageSideLength.Value)
            {
                return src.Clone();
            }

            var scale = (double)maxImageSideLength.Value / maxSide;
            var width = Math.Max(1, (int)Math.Round(src.Width * scale));
            var height = Math.Max(1, (int)Math.Round(src.Height * scale));
            Mat resized = new();
            Cv2.Resize(src, resized, new Size(width, height), interpolation: InterpolationFlags.Area);
            return resized;
        }

        private static void ConfigureCpuInference(PaddleConfig config, PaddleOcrOptions options)
        {
            config.OneDnnEnabled = true;
            config.MemoryOptimized = true;
            config.GLogEnabled = false;

            if (options.CpuMathThreadCount.HasValue)
            {
                config.CpuMathThreadCount = Math.Max(1, options.CpuMathThreadCount.Value);
            }
        }
    }
}
