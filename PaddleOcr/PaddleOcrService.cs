using System;
using System.Threading;
using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;

namespace PaddleOcr
{
    public sealed class PaddleOcrService : IDisposable
    {
        private readonly ThreadLocal<PaddleOcrAll> _ocr =
            new(CreateOcrEngine);

        public string Recognize(string imageFile)
        {
            using var src = Cv2.ImRead(imageFile, ImreadModes.Color);
            if (src.Empty())
            {
                throw new InvalidOperationException($"Unable to load image: {imageFile}");
            }

            var result = _ocr.Value.Run(src);
            return result.Text;
        }

        public void Dispose()
        {
            _ocr.Dispose();
        }

        private static PaddleOcrAll CreateOcrEngine()
        {
            PaddleOcrAll ocr = new(LocalFullModels.ChineseV5, ConfigureCpuInference)
            {
                AllowRotateDetection = true,
                Enable180Classification = false
            };

            return ocr;
        }

        private static void ConfigureCpuInference(PaddleConfig config)
        {
            config.OneDnnEnabled = true;
            config.MemoryOptimized = true;
            config.GLogEnabled = false;
        }
    }
}
