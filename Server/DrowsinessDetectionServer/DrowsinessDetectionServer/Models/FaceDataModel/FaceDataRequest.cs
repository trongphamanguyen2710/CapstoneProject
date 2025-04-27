namespace DrowsinessDetectionServer.Models.FaceDataModel;

public class FaceDataAddRequest
{
    public long DriverId { get; set; }
    public int Data1 { get; set; }
    public int Data2 { get; set; }
    public string Data3 { get; set; } = string.Empty;
    public string Data4 { get; set; } = string.Empty;
}

public class FaceDataEditRequest
{
    public long FaceDataId { get; set; }
    public int Data1 { get; set; }
    public int Data2 { get; set; }
    public string Data3 { get; set; } = string.Empty;
    public string Data4 { get; set; } = string.Empty;
}
