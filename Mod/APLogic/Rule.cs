using System;
using System.Collections.Generic;
using com.seadoggie.TFWRArchipelago.Model;

namespace com.seadoggie.TFWRArchipelago.APLogic;

public abstract class Rule
{
    public bool Value { get; private set; }

    public bool Evaluate(APWorld apWorld)
    {
        if (Value) return Value;
        Value = ReEvaluate(apWorld);
        return Value;
    }

    public abstract bool ReEvaluate(APWorld apWorld);

    public static Rule operator &(Rule left, Rule right)
    {
        if (left is TrueRule) return right;
        if (right is TrueRule) return left;
        return new AndRule(left, right);
    }

    public static Rule operator |(Rule left, Rule right) => new OrRule(left, right);
    
    public static Rule AndMany(IEnumerable<Rule> rules)
    {
        Rule finalRule = null;
        foreach (Rule rule in rules)
        {
            if (finalRule is null)
            {
                finalRule = rule;
            }
            else
            {
                finalRule &= rule;
            }
        }

        return finalRule ?? new TrueRule();
    }
}

public class TrueRule : Rule
{
    public override bool ReEvaluate(APWorld _) => true;

    public override string ToString() => "TRUE";
}

public class RegionAccessRule(string regionName) : Rule
{
    private APRegion _region;

    public override bool ReEvaluate(APWorld apWorld)
    {
        _region ??= apWorld.GetRegionByName(regionName) ?? throw new Exception($"Region not found: {regionName}");
        return _region.Rule.Evaluate(apWorld);
    }

    public override string ToString() => $"CanAccessRegion({regionName})";
}

public class LocationAccessRule(string location) : Rule
{
    private APLocation _location;

    public override bool ReEvaluate(APWorld apWorld)
    {
        _location ??= apWorld.GetLocByName(location);
        return _location is null
            ? throw new Exception($"Failed to locate required location: {location}")
            : _location.Rule.Evaluate(apWorld);
    }

    public override string ToString() => $"CanAccessLocation({location})";
}

public class ItemRule(string item, int count = 1) : Rule
{
    public override bool ReEvaluate(APWorld apWorld) =>
        apWorld.ReceivedItems.TryGetValue(item, out int value) && value >= count;

    public override string ToString() => $"Has({item}" + (count > 1 ? $",{count}" : "") + ")";
}

public class AndRule(Rule left, Rule right) : Rule
{
    public override bool ReEvaluate(APWorld apWorld) => (left?.Evaluate(apWorld) ?? throw new Exception("Left rule is null"))
                                                        && (right?.Evaluate(apWorld) ?? throw new Exception("Right rule is null"));

    public override string ToString() => $"{left} AND {right}";
}

public class OrRule(Rule left, Rule right) : Rule
{
    public override bool ReEvaluate(APWorld apWorld) => left.Evaluate(apWorld) || right.Evaluate(apWorld);

    public override string ToString() => $"{left} OR {right}";
}