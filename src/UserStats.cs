namespace com.seadoggie.TFWRArchipelago;

/// <summary>
/// I think the idea here was to implement my own tracking of stats to enable custom statistics
/// </summary>
public static class UserStats
{
    private static readonly Dictionary<string, int> Stats = new ();

    private static readonly object LockObject = new ();
    
    public static int Add(string name, int count)
    {
        lock (LockObject)
        {
            if (Stats.ContainsKey(name))
            {
                Stats[name] += count;
            }
            else
            {
                Stats.Add(name, count);
            }

            return Stats[name];
        }
    }
}