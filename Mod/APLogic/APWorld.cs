using System;
using System.Collections.Generic;
using System.Linq;
using com.seadoggie.TFWRArchipelago.Model;

namespace com.seadoggie.TFWRArchipelago.APLogic;

public class APWorld(APExport export)
{
    public List<APRegion> Regions { get; set; } = [];
    public List<APLocation> Locations { get; set; } = [];
    public IDictionary<string, int> ReceivedItems { get; set; } = new Dictionary<string, int>();

    public APLocation GetLocByName(string name) => Locations.FirstOrDefault(m => m.name == name);
    public APRegion GetRegionByName(string name) => Regions.FirstOrDefault(m => m.name == name);

    private readonly APExport _exportData = export;

    // Note: Do not replace a Location's rule with True to indicate that it was completed.
    // This could cause other rules to incorrectly return true. 

    public void ReEvaluate()
    {
        foreach (APRegion region in Regions) region.Rule.Evaluate(this);
        foreach (APLocation location in Locations) location.Rule.Evaluate(this);
    }

    public void ApplyCropCosts(Dictionary<string, List<string>> cropCosts)
    {
        // For each crop
        foreach (string cropName in cropCosts.Keys)
        {
            Crop crop = GetCropByName(cropName);
            // Find the region that produces this crop
            APRegion productionRegion =
                Regions.First(m => string.Equals(m.resource, crop.result, StringComparison.OrdinalIgnoreCase));

            // Build the rule for accessing the region producing the cost crops
            productionRegion.Rule &= Rule.AndMany(cropCosts[cropName].Select(costCrop =>
                new RegionAccessRule(Regions.First(m =>
                    string.Equals(m.resource, costCrop, StringComparison.OrdinalIgnoreCase)).name
                )
            ));

            // For each item this crop costs
            foreach (string costCrop in cropCosts[cropName])
            {
                // For each location requiring the crop
                foreach (APLocation apLocation in Locations.Where(m => m.statistic?.key == crop.result))
                {
                    // Find the location that requires the same amount of the costing crop
                    APLocation location = Locations.FirstOrDefault(m =>
                        m.statistic?.key == costCrop && m.statistic?.value == apLocation.statistic?.value);
                    if (location is null) continue;

                    // Prepend a location access rule
                    apLocation.Rule = new LocationAccessRule(location.name) & apLocation.Rule;
                }
            }
        }
    }

    private Crop GetCropByName(string crop)
    {
        return _exportData.crops.First(m => m.name == crop);
    }
}