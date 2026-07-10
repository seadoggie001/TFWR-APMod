namespace com.seadoggie.TFWRArchipelago.Model;

public class APOptions
{
    public APOptions()
    {
    }

    public APOptions(Dictionary<string, object> slotData)
    {
        if (slotData.TryGetValue("goal", out object goalValue) && goalValue is long goal)
        {
            GoalName = 0 == goal
                ? "Gold Farmer"
                : "Size Matters";
        }
        else
        {
            Plugin.Log.LogWarning("Goal option was not included, but was expected");
        }

        if (slotData.TryGetValue("crop_cost", out object cropCostValue) && cropCostValue is long cropCost)
        {
            RandomizedCosts = 1 == cropCost;
        }
        else
        {
            Plugin.Log.LogWarning("Crop Cost option was not included, but was expected");
        }

        
        if (slotData.TryGetValue("grass_sanity", out object grassValue) && grassValue is long grass)
        {
            GrassSanity = 1 == grass;
        }
        else
        {
            Plugin.Log.LogWarning("Grass Sanity option was not included, but was expected");
        }
        
        CropCosts = new Dictionary<string, List<string>>();
        if (RandomizedCosts)
        {
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

                Newtonsoft.Json.Linq.JArray array = (Newtonsoft.Json.Linq.JArray)cost;
                CropCosts[cropName] = array.Values<string>().ToList();
            }
        }
    }

    public string GoalName { get; set; } = "Gold Farmer";
    public bool RandomizedCosts { get; set; } = false;
    public Dictionary<string, List<string>> CropCosts { get; set; }
    public bool GrassSanity { get; set; } = false;

    public override string ToString()
    {
        return $"[APOptions] GoalName: {GoalName}, RandomizedCosts: {RandomizedCosts}, GrassSanity: {GrassSanity}";
    }
}