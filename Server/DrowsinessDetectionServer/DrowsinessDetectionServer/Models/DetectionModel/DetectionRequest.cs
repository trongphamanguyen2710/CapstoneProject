using DrowsinessDetectionServer.Enum;

namespace DrowsinessDetectionServer.Models.DetectionModel;

public class DetectionCreateRequest
{
    public long SessionId { get; set; }
    public string Location { get; set; } = string.Empty;
    public DectectionType Type { get; set; }
}

public class DetectionResponseRequest
{
    public long DetectionId { get; set; }
}
