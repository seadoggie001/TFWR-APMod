using com.seadoggie.TFWRArchipelago.Constants;

namespace com.seadoggie.TFWRArchipelago;

/// <summary>
/// This class converts from AP/Human-readable item names to internal item names
/// </summary>
public static class Unlocks
{   
    public static string ItemToUnlock(string name)
    {
        return name switch
        {
            APItem.Unlock => Unlock.AutoUnlock,
            APItem.Cactus => Unlock.Cactus,
            APItem.Carrot => Unlock.Carrots,
            APItem.Costs => Unlock.Costs,
            APItem.Debug => Unlock.Debug,
            APItem.MoreDebug => Unlock.Debug2,
            APItem.Dictionaries => Unlock.Dictionaries,
            APItem.Dinosaurs => Unlock.Dinosaurs,
            APItem.Expand => Unlock.Expand,
            APItem.Fertilizer => Unlock.Fertilizer,
            APItem.Functions => Unlock.Functions,
            APItem.Grass => Unlock.Grass,
            APItem.Hats => Unlock.Hats,
            APItem.Import => Unlock.Import,
            APItem.Lists => Unlock.Lists,
            APItem.Loop => Unlock.Loops,
            APItem.Mazes => Unlock.Mazes,
            APItem.Megafarm => Unlock.Megafarm,
            APItem.Operators => Unlock.Operators,
            APItem.Plant => Unlock.Plant,
            APItem.Polyculture => Unlock.Polyculture,
            APItem.Pumpkins => Unlock.Pumpkins,
            APItem.Senses => Unlock.Senses,
            APItem.Simulation => Unlock.Simulation,
            APItem.DroneSpeed => Unlock.Speed,
            APItem.Sunflowers => Unlock.Sunflowers,
            APItem.Timing => Unlock.Timing,
            APItem.Trees => Unlock.Trees,
            APItem.TheFarmersRemains => Unlock.TheFarmersRemains,
            APItem.TopHat => Unlock.TopHat,
            APItem.Utilities => Unlock.Utilities,
            APItem.Variables => Unlock.Variables,
            APItem.Watering => Unlock.Watering,
            // Leaderboard is intentionally disabled because mods.
            // APItem.Leaderboard => Unlock.Leaderboard,
            _ => null
        };
    }
}