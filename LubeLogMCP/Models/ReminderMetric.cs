namespace LubeLogMCP.Models
{
    // Matches lubelog's own ReminderMetric enum by NAME: the server parses this field with
    // Enum.TryParse(input.Metric, out ReminderMetric parsedMetric) - case-sensitive, no
    // ignoreCase - so the .ToString() of this enum must read exactly "Date"/"Odometer"/"Both".
    public enum ReminderMetric
    {
        Date = 0,
        Odometer = 1,
        Both = 2
    }
}
