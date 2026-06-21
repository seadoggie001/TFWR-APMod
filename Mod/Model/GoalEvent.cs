namespace com.seadoggie.TFWRArchipelago.Model;

public class GoalEvent(long id, string name, GoalType goalType)
{
    /// <summary>
    /// The type of goal completed
    /// </summary>
    public readonly GoalType GoalType = goalType;
    /// <summary>
    /// The name of the location
    /// </summary>
    public readonly string Name = name;

    public readonly long Id = id;
}