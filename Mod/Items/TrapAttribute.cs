using System;

namespace com.seadoggie.TFWRArchipelago.Items;

/// <summary>
/// Specifies that a class handles a Trap.
/// </summary>
/// <remarks>Should be used on a MonoBehaviour class</remarks>
/// <param name="name">Name of the item that is handled</param>
/// <param name="alternateNames">Alternative names that are handled. Useful for TrapLink?</param>
[AttributeUsage(AttributeTargets.Class)]
public class TrapAttribute(string name, string[] alternateNames = null) : ItemAttribute(name, alternateNames)
{
}