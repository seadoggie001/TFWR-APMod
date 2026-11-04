using DroneLib.Model;
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
        ItemSo = ScriptableObject.CreateInstance<ItemSO>();
        ItemSo.itemName = "Archipelago Item";
        // The rest of this is all nonsense?
        ItemSo.description = "Archipelago Stuff";
        ItemSo.docs = "I got some docs!?";
        ItemSo.enabled = true;
        ItemSo.name = "Some other Archipelago name?";
        ItemSo.trackStats = true;
        Instance = this;
    }
    
    /// <summary>
    /// Load the sprite from the Assets
    /// </summary>
    /// <returns></returns>
    public override Sprite LoadSprite() => Assets.Resources.Archipelago;
}