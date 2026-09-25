using System;
using System.Collections.Generic;

namespace com.seadoggie.TFWRArchipelago.Items;

/// <summary>
/// Specifies that a class handles an APItem.
/// </summary>
/// <remarks>Should be used on a MonoBehaviour class</remarks>
/// <param name="name">Name of the item that is handled</param>
/// <param name="alternateNames">Alternative names that are handled. Useful for TrapLink?</param>
[AttributeUsage(AttributeTargets.Class)]
public class ItemAttribute(string name, string[] alternateNames = null) : Attribute
{
    public readonly string Name = name;
    public readonly IEnumerable<string> AcceptedNames = alternateNames;
}