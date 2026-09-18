using com.seadoggie.TFWRArchipelago.Patches;
using DroneLib.Item;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago;

/// <summary>
/// This item is used to display the AP icon in places. The item is not actually collected in-game.
/// </summary>
public class FakeAPItem : BaseItem
{
    /// <summary>
    /// Used to get the ItemId later
    /// </summary>
    public static FakeAPItem Instance { get; private set; }
    
    public FakeAPItem()
    {
        ItemSO = ScriptableObject.CreateInstance<ItemSO>();
        ItemSO.itemName = "Archipelago Item";
        // The rest of this is all nonsense?
        ItemSO.description = "Archipelago Stuff";
        ItemSO.docs = "I got some docs!?";
        ItemSO.enabled = true;
        ItemSO.name = "Some other Archipelago name?";
        ItemSO.trackStats = true;
        Instance = this;
    }
    
    /// <summary>
    /// Load the sprite from the Assets
    /// </summary>
    /// <returns></returns>
    public override Sprite LoadSprite() => Assets.Resources.Archipelago;
}