using System.Text.Json.Serialization;

namespace Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Conclusion
{
    Disease,
    Recovery,
    Death
}