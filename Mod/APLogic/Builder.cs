using System;
using System.Collections.Generic;
using System.Linq;
using com.seadoggie.TFWRArchipelago.Model;

namespace com.seadoggie.TFWRArchipelago.APLogic;

public class Builder(APExport apExport)
{
    public static HashSet<string> Items { get; set; } = null;
    public static HashSet<string> Locations { get; set; }
    public static HashSet<string> Regions { get; set; }
    public static Dictionary<string, Rule> CustomRulesDict { get; set; }

    private readonly List<APLocation> _locations = apExport.locations.ToList();
    private readonly List<APRegion> _regions = apExport.regions.ToList();

    public APWorld Build()
    {
        CustomRulesDict ??= new Dictionary<string, Rule>
        {
            { "AnyHatItems", CustomRules.AnyHatItems },
            { "Really Big Farm", CustomRules.ReallyBigFarm },
            { "Maxed out farm", CustomRules.MaxedOutFarm },
            { "Can till", CustomRules.CanTill },
            { "Crops that can produce Weird Substance", CustomRules.WeirdSubstanceCrops },
        };
        Locations ??= [.._locations.Select(m => m.name)];
        Regions ??= [.._regions.Select(m => m.name)];
        Items ??= [..APItem.Names.Unlocks];

        // Locate the start region
        APRegion start = _regions.FirstOrDefault(m => m.parent is null);
        if (start is null) throw new NullReferenceException(nameof(start));

        // Build the start region
        APRegion startRegion = _regions.First(m => string.Equals(m.name, "Start", StringComparison.OrdinalIgnoreCase));
        startRegion.Rule = new TrueRule();

        // Create the world
        APWorld apWorld = new(apExport);

        // Add regions with rules 
        foreach (APRegion region in _regions)
        {
            region.Rule = new TrueRule();
            if (!string.IsNullOrWhiteSpace(region.parent))
            {
                region.Rule = new RegionAccessRule(region.parent)
                              & (
                                  region.requirements != null
                                      ? RuleBuilder(region.requirements)
                                      : new TrueRule()
                              );
            }

            apWorld.Regions.Add(region);
        }

        // Add locations with rules 
        foreach (APLocation location in _locations)
        {
            location.Rule = new RegionAccessRule(location.region)
                            & (
                                location.requirements != null
                                    ? RuleBuilder(location.requirements)
                                    : new TrueRule()
                            );
            apWorld.Locations.Add(location);
        }

        return apWorld;
    }

    private static Rule RuleBuilder(IEnumerable<Requirement> requirements)
    {
        List<Rule> rules = [];
        
        foreach (Requirement requirement in requirements)
        {
            if (Items.Contains(requirement.name))
            {
                rules.Add(new ItemRule(requirement.name, requirement.count));
            }
            else if (Locations.Contains(requirement.name))
            {
                rules.Add(new LocationAccessRule(requirement.name));
            }
            else if (Regions.Contains(requirement.name))
            {
                rules.Add(new RegionAccessRule(requirement.name));
            }
            else if (CustomRulesDict.TryGetValue(requirement.name, out Rule customRule))
            {
                rules.Add(customRule);
            }
            else
            {
                throw new Exception($"What kind of thing is this? {requirement.name}");
            }
        }

        return Rule.AndMany(rules);
    }
}

public static class CustomRules
{
    public static readonly Rule CanTill = new ItemRule(APItem.Names.Carrot);
    public static readonly Rule ReallyBigFarm = new ItemRule(APItem.Names.Expand, 9);
    public static readonly Rule AnyHatItems = new ItemRule(APItem.Names.Hats) | new ItemRule(APItem.Names.TopHat);

    public static readonly Rule MaxedOutFarm =
        new ItemRule(APItem.Names.Expand, 9)
        & new ItemRule(APItem.Names.DroneSpeed, 5)
        & new ItemRule(APItem.Names.Megafarm, 5)
        & new ItemRule(APItem.Names.Functions);

    public static readonly Rule WeirdSubstanceCrops =
        // Multiple grass
        new ItemRule(APItem.Names.Grass, 2)
        // Plant carrots
        | (new LocationAccessRule("Carrots") & new ItemRule(APItem.Names.Carrot, 2))
        // Trees
        | new LocationAccessRule("Tree")
        // Multiple Cacti
        | (new LocationAccessRule("Cactus") & new ItemRule(APItem.Names.Cactus, 2))
        // Pumpkins
        | new LocationAccessRule("Pumpkins");
}