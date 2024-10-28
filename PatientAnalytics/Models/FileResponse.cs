using System.ComponentModel.DataAnnotations;

namespace PatientAnalytics.Models;

public record FileResponse(Stream Stream, string ContentType);

public class FilePayload
{
    [Required(ErrorMessage = "File must be provided")]
    public IFormFile File { get; set; } = null!;
}

public class BlazorFileUploadPayload
{
    public Stream Stream { get; init; }
    public string ContentType { get; init; }
}

