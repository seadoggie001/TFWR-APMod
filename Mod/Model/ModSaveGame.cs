using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Model;

public class ModSaveGame
{
    /// <summary>
    /// I should probably use this field somewhere
    /// </summary>
    public string Version = "2";
    /// <summary>
    /// Statistics by action performed
    /// </summary>
    public List<Pair<string, double>> Statistics;
    /// <summary>
    /// Number of items received from AP
    /// </summary>
    public int ItemsReceived = 0;
    /// <summary>
    /// Grass sanity items completed
    /// </summary>
    public HashSet<Vector2Int> Grass = [];
}