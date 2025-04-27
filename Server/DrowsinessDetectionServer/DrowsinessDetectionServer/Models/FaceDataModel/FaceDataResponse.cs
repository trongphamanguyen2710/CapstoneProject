using DrowsinessDetectionServer.Models.DatabaseModels;

namespace DrowsinessDetectionServer.Models.FaceDataModel;

public class FaceDataGetResponse : BaseResponse
{
    public FaceData? FaceData { get; set; }
}
