using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using com.seadoggie.TFWRArchipelago.APLogic;

namespace com.seadoggie.TFWRArchipelago.Model;

/// <summary>
/// Region data used in Python
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class APRegion
{
    /// <summary>Name of the region</summary>
    public string name { get; set; }

    /// <summary>Name of the parent region</summary>
    public string parent { get; set; }

    /// <summary>Logic requirements - convertable into a Rule</summary>
    public List<Requirement> requirements { get; set; } = [];

    /// <summary>Name of the entrance</summary>
    public string entrance_name { get; set; }

    /// <summary>Name of the resource produced</summary>
    public string resource { get; set; }

    [JsonIgnore] public Rule Rule { get; set; }
}