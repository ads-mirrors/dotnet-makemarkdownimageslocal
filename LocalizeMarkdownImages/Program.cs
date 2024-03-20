using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        List<string> imageFormats = new List<string>() {".jpg", ".jpeg", ".png", ".gif", ".svg"};
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide the path to the markdown file.");
            return;
        }

        string markdownFilePath = args[0];
        if (!File.Exists(markdownFilePath))
        {
            Console.WriteLine($"File not found: {markdownFilePath}");
            return;
        }

        string markdownContent = await File.ReadAllTextAsync(markdownFilePath);
        string localImageDirectory = Path.Combine(Path.GetDirectoryName(markdownFilePath), Path.GetFileNameWithoutExtension(markdownFilePath) + "_images");
        Directory.CreateDirectory(localImageDirectory);

        var imageUrlRegex = new Regex(@"\b(?:http|https):\/\/\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        foreach (Match match in imageUrlRegex.Matches(markdownContent))
        {
            string imageUrl = match.Groups[0].Value;
            Uri thisUri = new Uri(imageUrl);
            if (imageFormats.Contains(Path.GetExtension(thisUri.AbsolutePath).ToLower()))
            {
                string fileName = Path.GetFileName(thisUri.AbsolutePath);
                string localImagePath = Path.Combine(localImageDirectory, fileName);
                string downloadImageUrl = thisUri.Scheme + "://" + thisUri.DnsSafeHost + thisUri.AbsolutePath;

                await DownloadImageAsync(downloadImageUrl, localImagePath);

                markdownContent = markdownContent.Replace(imageUrl, Path.GetFileNameWithoutExtension(markdownFilePath) + $"_images/{fileName}");
            }
        }

        await File.WriteAllTextAsync(markdownFilePath, markdownContent);
        Console.WriteLine("Markdown document has been updated with local image paths.");
    }

    static async Task DownloadImageAsync(string imageUrl, string savePath)
    {
        using (var httpClient = new HttpClient())
        {
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            await File.WriteAllBytesAsync(savePath, imageBytes);
        }
    }
}