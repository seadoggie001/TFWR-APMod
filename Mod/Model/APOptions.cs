using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.seadoggie.TFWRArchipelago.Logging;
using com.seadoggie.TFWRArchipelago.Service;

namespace com.seadoggie.TFWRArchipelago.Model;

public class APOptions
{
    [ModInject] private ILogger Logger { get; set; }

    public readonly Dictionary<double, double> ModifiedValues = [];

    public void LoadSlotData(Dictionary<string, object> slotData)
    {
        LoadStandardProperties(slotData);

        CropCosts = new Dictionary<string, List<string>>();
        if (!CropCostsRandomized()) return;
        IEnumerable<string> cropOptions =
        [
            "crops.Hay",
            "crops.Bush",
            "crops.Tree",
            "crops.Carrot",
            "crops.Cactus",
            "crops.Dinosaur",
            "crops.Sunflower",
            "crops.Pumpkin",
        ];

        foreach (string cropOption in cropOptions)
        {
            if (!slotData.TryGetValue(cropOption, out object cost))
            {
                Logger.LogWarning($"Crop Cost was randomized, but {cropOption} was not included");
                continue;
            }

            string cropName = cropOption.Replace("crops.", "").ToLower();

            // Dinosaurs don't need a cost, but apples do. I know it's weird, but trust me.
            cropName = cropName.Replace("dinosaur", "apple");

            if (cost == null) continue;
            Newtonsoft.Json.Linq.JArray array = (Newtonsoft.Json.Linq.JArray)cost;
            CropCosts[cropName] = array.Values<string>().ToList();
        }
    }

    public void LoadStandardProperties(Dictionary<string, object> slotData)
    {
        // for each property
        foreach (PropertyInfo propertyInfo in typeof(APOptions).GetProperties())
        {
            // If the property has a CropTarget attribute
            if (Attribute.GetCustomAttribute(propertyInfo, typeof(CropTargetAttribute)) is CropTargetAttribute
                cropTarget)
            {
                double actual = 0;
                // If the SlotData contains the crop target
                if (slotData.TryGetValue(cropTarget.Name, out object value))
                {
                    // Ignore non-long values
                    if (value is not long longValue) continue;

                    // Update the property
                    actual = longValue;
                }
                // Otherwise set the value to 
                else if ((double)propertyInfo.GetValue(this) == 0)
                {
                    actual = cropTarget.Target;
                }

                // Update the property
                propertyInfo.SetValue(this, actual);
                // Save the modified targets for later
                ModifiedValues.Add(cropTarget.Target, actual);
            }
            // if the property has a SlotData attribute
            else if (Attribute.GetCustomAttribute(propertyInfo, typeof(SlotDataAttribute)) is SlotDataAttribute data)
            {
                // If the property is in the SlotData
                if (slotData.TryGetValue(data.Name, out object value))
                {
                    // Ignore non-long values
                    if (value is not long longValue) continue;
                    // set the property's value
                    propertyInfo.SetValue(this, longValue);
                }
            }
        }
    }

    public string GoalName() => Goal == 0 ? "Gold Farmer" : "Size Matters";
    public bool CropCostsRandomized() => RandomizedCosts == 1;
    public bool GrassSanityEnabled() => GrassSanity == 1;
    public Dictionary<string, List<string>> CropCosts { get; private set; }

    [SlotData("goal")] public long Goal { get; set; } = 0;
    [SlotData("crop_cost")] public long RandomizedCosts { get; set; } = 0;
    [SlotData("grass_sanity")] public long GrassSanity { get; set; } = 0;

    [CropTarget("crop_target_10", 10)] public double CropTarget10 { get; set; }

    [CropTarget("crop_target_100", 100)] public double CropTarget100 { get; set; }

    [CropTarget("crop_target_1K", BigNumbers.K)]
    public double CropTarget1K { get; set; }

    [CropTarget("crop_target_10K", 10 * BigNumbers.K)]
    public double CropTarget10K { get; set; }

    [CropTarget("crop_target_100K", 100 * BigNumbers.K)]
    public double CropTarget100K { get; set; }

    [CropTarget("crop_target_1M", BigNumbers.M)]
    public double CropTarget1M { get; set; }

    [CropTarget("crop_target_10M", 10 * BigNumbers.M)]
    public double CropTarget10M { get; set; }

    [CropTarget("crop_target_100M", 100 * BigNumbers.M)]
    public double CropTarget100M { get; set; }

    [CropTarget("crop_target_1B", BigNumbers.B)]
    public double CropTarget1B { get; set; }

    public override string ToString()
    {
        return $"[APOptions] GoalName: {GoalName()}, " +
               $"RandomizedCosts: {CropCostsRandomized()}, " +
               $"GrassSanity: {GrassSanityEnabled()}";
    }
}

internal static class BigNumbers
{
    public const double K = 1000;
    public const double M = 1000 * 1000;
    public const double B = 1000 * 1000 * 1000;
};

public class CropTargetAttribute(string name, double target) : SlotDataAttribute(name)
{
    public double Target { get; set; } = target;
}

public class SlotDataAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}