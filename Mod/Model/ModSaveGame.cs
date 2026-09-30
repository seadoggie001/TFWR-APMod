using System.Collections.Generic;

namespace com.seadoggie.TFWRArchipelago.Model;

public class ModSaveGame
{
    /// <summary>
    /// I should probably use this field somewhere
    /// </summary>
    public string Version { get; private set; } = "2";

    /// <summary>
    /// Statistics by action performed
    /// </summary>
    public List<KeyValuePair<string, double>> Statistics { get; set; }

    /// <summary>
    /// Number of items received from AP
    /// </summary>
    public int ItemsReceived { get; set; } = 0;

    /// <summary>
    /// Grass sanity items completed
    /// </summary>
    public HashSet<Position> Grass { get; set; } = [];

    /// <summary>
    /// Include Options to support offline play
    /// </summary>
    public APOptions Options { get; set; }
}