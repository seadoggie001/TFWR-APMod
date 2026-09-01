using System.Diagnostics;
using System.Reflection;

namespace com.seadoggie.TFWRArchipelago.Model;

public class APOptions
{
    public APOptions()
    {
    }

    public APOptions(Dictionary<string, object> slotData)
    {
        foreach (PropertyInfo propertyInfo in typeof(APOptions).GetProperties())
        {
            if (Attribute.GetCustomAttribute(propertyInfo, typeof(SlotDataAttribute)) is not SlotDataAttribute data)
                continue;
            if (slotData.TryGetValue(data.Name, out object value))
            {
                if (value is not long longValue) continue;
                propertyInfo.SetValue(this, longValue);
            }
            else
            {
                Plugin.Log.LogWarning($"Option [\"{data.Name}\"] was not included, but was expected");
            }
        }

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
                Plugin.Log.LogWarning($"Crop Cost was randomized, but {cropOption} was not included");
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

    public string GoalName() => Goal == 0 ? "Gold Farmer" : "Size Matters";
    public bool CropCostsRandomized() => RandomizedCosts == 1;
    public bool GrassSanityEnabled() => GrassSanity == 1;
    public Dictionary<string, List<string>> CropCosts { get; }

    [SlotData("goal")] public long Goal { get; set; } = 0;
    [SlotData("crop_cost")] public long RandomizedCosts { get; set; } = 0;
    [SlotData("grass_sanity")] public long GrassSanity { get; set; } = 0;
    [SlotData("crop_target_10")] public double CropTarget10 { get; set; } = 10;
    [SlotData("crop_target_100")] public double CropTarget100 { get; set; } = 100;
    [SlotData("crop_target_1K")] public double CropTarget1K { get; set; } = 1000;
    [SlotData("crop_target_10K")] public double CropTarget10K { get; set; } = 10 * 1000;
    [SlotData("crop_target_100K")] public double CropTarget100K { get; set; } = 100 * 1000;
    [SlotData("crop_target_1M")] public double CropTarget1M { get; set; } = 1000 * 1000;
    [SlotData("crop_target_10M")] public double CropTarget10M { get; set; } = 10 * 1000 * 1000;
    [SlotData("crop_target_100M")] public double CropTarget100M { get; set; } = 100 * 1000 * 1000;
    [SlotData("crop_target_1B")] public double CropTarget1B { get; set; } = 1000 * 1000 * 1000;

    public override string ToString()
    {
        return
            $"[APOptions] GoalName: {GoalName()}, RandomizedCosts: {CropCostsRandomized()}, GrassSanity: {GrassSanityEnabled()}";
    }
}

public class SlotDataAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}