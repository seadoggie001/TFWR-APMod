namespace com.seadoggie.TFWRArchipelago;

// ToDo: There's probably a better or more standard way to do this

/// <summary>
/// This class converts from AP/Human-readable item/location names to internal item/location names
/// </summary>
public static class Unlocks
{
    public static string Item(string name)
    {
        return name switch
        {
            "Drone Speed" => "speed",
            _ => throw new NotImplementedException()
        };
    }
    
    public static string Location(string name)
    {
        return name switch
        {
            _ => throw new NotImplementedException()
        };
    }
}