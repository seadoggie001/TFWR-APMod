namespace com.seadoggie.TFWRArchipelago.Constants;

/// <summary>
/// Erm... all the hats. I don't remember why.
/// </summary>
/// <param name="name"></param>
public class Hat(string name)
{
    public static readonly Hat Carrot = new("carrot_hat");
    public static readonly Hat Cactus = new("cactus_hat");
    public static readonly Hat Gold = new("gold_hat");
    public static readonly Hat Tree = new("tree_hat");
    public static readonly Hat Pumpkin = new("pumpkin_hat");
    public static readonly Hat Power = new("sunflower_hat");
    public static readonly Hat GoldenCarrot = new("golden_carrot_hat");
    public static readonly Hat GoldenCactus = new("golden_cactus_hat");
    public static readonly Hat GoldenGold = new("golden_gold_hat");
    public static readonly Hat GoldenTree = new("golden_tree_hat");
    public static readonly Hat GoldenPumpkin = new("golden_pumpkin_hat");
    public static readonly Hat GoldenPower = new("golden_power_hat");

    public static readonly Hat TrafficCone = new("traffic_cone");
    public static readonly Hat TrafficConeStack = new("traffic_cone_stack");
    public static readonly Hat Wizard = new("wizard_hat");
    public static readonly Hat Straw = new("straw_hat");

    public static readonly Hat WoodTrophy = new("wood_trophy_hat");
    public static readonly Hat SilverTrophy = new("silver_trophy_hat");
    public static readonly Hat GoldTrophy = new("gold_trophy_hat");
    
    public string Resource => name;
    public override string ToString() => name;
}