using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChatBackend
{
    public static class ImageConverter
    {
        public static async Task<string> GetBase64(IFormFile image, int width, int height)
        {
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var img = await Image.LoadAsync(memoryStream);
            using var memory = new MemoryStream();
            img.Mutate(x => x.Resize(width, height, KnownResamplers.Lanczos3));
            await img.SaveAsync(memory, new JpegEncoder());
            var fileBytes = memory.ToArray();
            return $"data:image/jpeg;base64,{Convert.ToBase64String(fileBytes)}";
        }
    }
}
