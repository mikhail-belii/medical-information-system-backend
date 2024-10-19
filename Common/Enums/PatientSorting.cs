using System.Text.Json.Serialization;

namespace Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PatientSorting
{
    NameAsc,
    NameDesc,
    CreateAsc,
    CreateDesc,
    InspectionAsc,
    InspectionDesc
}