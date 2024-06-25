namespace PatientAnalytics.Models;

public class ReportResponse
{
    public ReportResponse(byte[] fileStream, string fileName)
    {
        this.fileStream = fileStream;
        this.fileName = fileName;
    }

    public byte[] fileStream;
    public string fileName;
}