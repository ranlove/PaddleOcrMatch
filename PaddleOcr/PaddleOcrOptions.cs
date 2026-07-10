namespace PaddleOcr
{
    public sealed class PaddleOcrOptions
    {
        public bool AllowRotateDetection { get; set; } = true;

        public bool Enable180Classification { get; set; } = false;

        public int? CpuMathThreadCount { get; set; }

        public int? DetectorMaxSize { get; set; }

        public int? MaxImageSideLength { get; set; } = 2048;
    }
}
