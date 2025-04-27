using DrowsinessDetectionServer.Models.DatabaseModels;

namespace DrowsinessDetectionServer.Models.DetectionModel;

public class DetectionGetResponse : BaseResponse
{
    public DetectionLog? Detection { get; set; }
}

public class DetectionGetListResponse : BaseResponse
{
    public List<DetectionLog>? DetectionLogs { get; set; }
}
