using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PaddleOcr
{
    internal class Program
    {
        private const int MaxImageSideLength = 2048;
        private static readonly int CpuMathThreadCount = Math.Max(1, Environment.ProcessorCount/2);

        static async Task Main(string[] args)
        {
            // var imageDirectory = args.Length > 0
            //     ? args[0]
            //     : Path.Combine(AppContext.BaseDirectory, "Images");
            const string sourceDirectory = @"C:\Users\ryan.li\Desktop\ocr\files";
            const string targetDirectory = @"C:\Users\ryan.li\Desktop\TargetImages";

            if (!Directory.Exists(sourceDirectory))
            {
                await Console.Error.WriteLineAsync($"Image directory does not exist: {sourceDirectory}");
                return;
            }

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
            
            string[] patterns = { "*.jpg", "*.jpeg", "*.png", "*.bmp" };
            var images = patterns
                .SelectMany(pattern => Directory.GetFiles(sourceDirectory, pattern, SearchOption.TopDirectoryOnly))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (images.Length == 0)
            {
                Console.WriteLine($"No supported images were found in: {sourceDirectory}");
                return;
            }

            Console.WriteLine($"原始文件夹图片总数：{images.Length}");

            string[] keywords =
            {
                "营业执照",
                "腾讯",
                "百度",
                "中华人民共和国",
                "阿里巴巴"
            };

            using PaddleOcrService ocr = new(new PaddleOcrOptions
            {
                AllowRotateDetection = true,
                Enable180Classification = false,
                CpuMathThreadCount = CpuMathThreadCount,
                MaxImageSideLength = MaxImageSideLength
            });
            var searcher = new ImageSearcher(ocr);

            var matchCount = await searcher.SearchAsync(
                images,
                keywords,
                onMatch: item =>
                {
                    var copiedFilePath = CopyMatchedImage(item.Image, targetDirectory);

                    Console.WriteLine(item.Image);
                    Console.WriteLine($"已复制到：{copiedFilePath}");
                    Console.WriteLine("命中关键词：");

                    foreach (var k in item.Keywords)
                    {
                        Console.WriteLine(k);
                    }

                    Console.WriteLine();
                },
                progress: (current, total, image) =>
                {
                    Console.WriteLine($"处理进度：{current}/{total}，正在处理：{Path.GetFileName(image)}");
                });

            if (matchCount == 0)
            {
                Console.WriteLine("No keywords matched.");
                return;
            }

            Console.WriteLine($"命中图片总数：{matchCount}");
        }

        private static string CopyMatchedImage(string sourceImagePath, string targetDirectory)
        {
            var fileName = Path.GetFileName(sourceImagePath);
            var targetFilePath = Path.Combine(targetDirectory, fileName);

            File.Copy(sourceImagePath, targetFilePath, overwrite: true);
            return targetFilePath;
        }
    }
}