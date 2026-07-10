using System.Text.Json.Serialization;

namespace LubeLogMCP.Models
{
    public class PostRequestModel
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;
        [JsonPropertyName("odometer")]
        public int Odometer { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("fuelConsumed")]
        public decimal FuelConsumed { get; set; }
        [JsonPropertyName("cost")]
        public decimal Cost { get; set; }
        [JsonPropertyName("isFillToFull")]
        public bool IsFillToFull { get; set; }
        [JsonPropertyName("missedFuelUp")]
        public bool MissedFuelUp { get; set; }
        [JsonPropertyName("extraFields")]
        public List<ExtraFieldPostModel> ExtraFields { get; set; } = new List<ExtraFieldPostModel>();
        [JsonPropertyName("equipmentRecordId")]
        public string EquipmentRecordId { get; set; } = string.Empty;
        [JsonPropertyName("partQuantity")]
        public decimal PartQuantity { get; set; }
        [JsonPropertyName("partNumber")]
        public string PartNumber { get; set; } = string.Empty;
        [JsonPropertyName("partSupplier")]
        public string PartSupplier { get; set; } = string.Empty;
        [JsonPropertyName("startingSoc")]
        public int StartingSoc { get; set; }
        [JsonPropertyName("endingSoc")]
        public int EndingSoc { get; set; }
    }
    public class ExtraFieldPostModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
