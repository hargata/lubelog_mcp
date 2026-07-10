namespace LubeLogMCP.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public bool IsElectric { get; set; } = false;
        public string VehicleIdentifier { get; set; } = "LicensePlate";
        public string Identifier { get {
                if (VehicleIdentifier == "LicensePlate")
                {
                    return LicensePlate;
                }
                else
                {
                    if (ExtraFields.Any(x => x.Name == VehicleIdentifier))
                    {
                        return ExtraFields?.FirstOrDefault(x => x.Name == VehicleIdentifier)?.Value ?? "N/A";
                    }
                    else
                    {
                        return "N/A";
                    }
                }
            } }
    }
    public class ExtraField
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
