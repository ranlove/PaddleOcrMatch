using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PaddleOcr
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // var imageDirectory = args.Length > 0
            //     ? args[0]
            //     : Path.Combine(AppContext.BaseDirectory, "Images");
            const string sourceDirectory = @"C:\Users\ryan.li\Desktop\Images";
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

            using PaddleOcrService ocr = new();
            var searcher = new ImageSearcher(ocr);

            var result = await searcher.SearchAsync(
                images,
                keywords,
                progress: (current, total) =>
                {
                    Console.WriteLine($"处理进度：{current}/{total}");
                });

            if (result.Count == 0)
            {
                Console.WriteLine("No keywords matched.");
                return;
            }

            foreach (var item in result)
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
            }
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