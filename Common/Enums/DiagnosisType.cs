using System.Text.Json.Serialization;

namespace Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DiagnosisType
{
    Main,
    Concomitant,
    Complication
}