namespace com.seadoggie.TFWRArchipelago.Model;

public class APOptions
{
    public string GoalName { get; set; }
    public bool RandomizedCosts { get; set; }
    public Dictionary<string, List<string>> CropCosts { get; set; }
}