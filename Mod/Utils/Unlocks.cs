using com.seadoggie.TFWRArchipelago.Model;

namespace com.seadoggie.TFWRArchipelago.Utils;

/// <summary>
/// This class converts from AP/Human-readable item names to internal item names
/// </summary>
public static class Unlocks
{   
    public static string ItemToUnlock(string name)
    {
        return name switch
        {
            APItem.Names.Unlock => Unlock.AutoUnlock,
            APItem.Names.Cactus => Unlock.Cactus,
            APItem.Names.Carrot => Unlock.Carrots,
            APItem.Names.Costs => Unlock.Costs,
            APItem.Names.Debug => Unlock.Debug,
            APItem.Names.MoreDebug => Unlock.Debug2,
            APItem.Names.Dictionaries => Unlock.Dictionaries,
            APItem.Names.Dinosaurs => Unlock.Dinosaurs,
            APItem.Names.Expand => Unlock.Expand,
            APItem.Names.Fertilizer => Unlock.Fertilizer,
            APItem.Names.Functions => Unlock.Functions,
            APItem.Names.Grass => Unlock.Grass,
            APItem.Names.Hats => Unlock.Hats,
            APItem.Names.Import => Unlock.Import,
            APItem.Names.Lists => Unlock.Lists,
            APItem.Names.Loop => Unlock.Loops,
            APItem.Names.Mazes => Unlock.Mazes,
            APItem.Names.Megafarm => Unlock.Megafarm,
            APItem.Names.Operators => Unlock.Operators,
            APItem.Names.Plant => Unlock.Plant,
            APItem.Names.Polyculture => Unlock.Polyculture,
            APItem.Names.Pumpkins => Unlock.Pumpkins,
            APItem.Names.Senses => Unlock.Senses,
            APItem.Names.Simulation => Unlock.Simulation,
            APItem.Names.DroneSpeed => Unlock.Speed,
            APItem.Names.Sunflowers => Unlock.Sunflowers,
            APItem.Names.Timing => Unlock.Timing,
            APItem.Names.Trees => Unlock.Trees,
            APItem.Names.TheFarmersRemains => Unlock.TheFarmersRemains,
            APItem.Names.TopHat => Unlock.TopHat,
            APItem.Names.Utilities => Unlock.Utilities,
            APItem.Names.Variables => Unlock.Variables,
            APItem.Names.Watering => Unlock.Watering,
            APItem.Names.PowerFlip => Unlock.PowerFlip,
            APItem.Names.Teleport => Unlock.Teleport,
            // Leaderboard is intentionally disabled because mods.
            // APItem.Names.Leaderboard => Unlock.Leaderboard,
            _ => null
        };
    }
}