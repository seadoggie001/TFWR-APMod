using JetBrains.Annotations;

namespace com.seadoggie.TFWRArchipelago.Model;

public record Milestone
{
    public string Location;

    [CanBeNull] public string Achievement;

    public APLocation APLocation;

    /// <summary>
    /// The required number
    /// </summary>
    public double Target;

    /// <summary>
    /// Has the target been reached yet?
    /// </summary>
    public bool Triggered;
}