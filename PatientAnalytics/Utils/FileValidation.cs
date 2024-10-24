using System.Drawing;
using System.Net.Mime;

namespace PatientAnalytics.Utils;

public static class FileValidation
{
    public static bool IsValidImageFile(IFormFile file)
    {
        var validImageTypes = new[]
        {
            MediaTypeNames.Image.Png, 
            MediaTypeNames.Image.Jpeg, 
            MediaTypeNames.Image.Webp,
            MediaTypeNames.Image.Svg
        };
        
        return validImageTypes.Contains(file.ContentType);
    }
}

