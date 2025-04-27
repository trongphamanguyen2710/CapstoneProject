using DrowsinessDetectionServer.Models.FaceDataModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DrowsinessDetectionServer.Models.DatabaseModels;

public class FaceData : ICommonField
{
    [Key]
    public long Id { get; set; }
    public int Data1 { get; set; }
    public int Data2 { get; set; }
    public string Data3 { get; set; } = string.Empty;
    public string Data4 { get; set; } = string.Empty;

    [ForeignKey(nameof(Driver))]
    public long? DriverId { get; set; }
    [JsonIgnore]
    public virtual Driver Driver { get; set; } = null!;

    public FaceData()
    {

    }

    public FaceData(int data1, int data2, string data3, string data4, Driver driver)
    {
        Data1 = data1;
        Data2 = data2;
        Data3 = data3;
        Data4 = data4;
        Driver = driver;
    }

    public void Update(FaceDataEditRequest request)
    {
        Data1 = request.Data1;
        Data2 = request.Data2;
        Data3 = request.Data3;
        Data4 = request.Data4;
    }
}
