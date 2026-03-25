using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace erp.Module.Helpers;

public static class ImageHelper
{
    public static byte[]? GetThumbnailBytes(byte[]? sourceData)
    {
        if (sourceData == null || sourceData.Length == 0) return null;

        // Usamos ImageSharp para redimensionar a 100x100
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG_LOG] Procesando miniatura. Source data length: {sourceData.Length}");
            using (var image = Image.Load(sourceData))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(100, 100),
                    Mode = ResizeMode.Max
                }));

                using (var ms = new MemoryStream())
                {
                    image.Save(ms, image.Metadata.DecodedImageFormat ?? SixLabors.ImageSharp.Formats.Png.PngFormat.Instance);
                    var resultData = ms.ToArray();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG_LOG] Miniatura generada con éxito. Result length: {resultData.Length}");
                    return resultData;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al procesar miniatura: {ex.Message}");
            return sourceData; // Fallback
        }
    }
}
