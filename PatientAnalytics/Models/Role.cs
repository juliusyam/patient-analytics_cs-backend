using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PatientAnalytics.Models;

[JsonConverter(typeof(StringEnumConverter))]
public enum Role
{
    [EnumMember(Value = "SuperAdmin")]
    SuperAdmin,
    
    [EnumMember(Value = "Admin")]
    Admin,
    
    [EnumMember(Value = "Doctor")]
    Doctor
}

