using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace com.seadoggie.TFWRArchipelago.Model;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Crop
{
    /// <summary>Name of the crop</summary>
    public string name { get; set; }
    
    /// <summary>Quantity level 1=>B 2=>100M 3=>100K</summary>
    public int tier { get; set; }
    
    /// <summary>Resource cost</summary>
    public IEnumerable<string> cost { get; set; }
    
    /// <summary>Resource produced when harvesting</summary>
    public string result { get; set; }
    
    /// <summary>Item that unlocks planting this</summary>
    public string item { get; set; }
    
    /// <summary>Should the cost be skipped for randomization</summary>
    public bool starting_crop { get; set; }
    
    /// <summary>Can the result be used as another crop's cost</summary>
    public bool used_for_cost { get; set; }
    
    /// <summary>Should the cost be randomized</summary>
    public bool randomized_cost { get; set; }
}