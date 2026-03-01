using com.seadoggie.TFWRArchipelago.Constants;

namespace com.seadoggie.TFWRArchipelago;

/// <summary>
/// This class converts from AP/Human-readable item/location names to internal item/location names
/// </summary>
public static class Unlocks
{
    public static string ItemToUnlock(string name)
    {
        return name switch
        {
            APItem.AutoUnlock => Unlock.AutoUnlock,
            APItem.Cactus => Unlock.Cactus,
            APItem.Carrots => Unlock.Carrots,
            APItem.Costs => Unlock.Costs,
            APItem.Debug => Unlock.Debug,
            APItem.Debug2 => Unlock.Debug2,
            APItem.Dictionaries => Unlock.Dictionaries,
            APItem.Dinosaurs => Unlock.Dinosaurs,
            APItem.Expand => Unlock.Expand,
            APItem.Fertilizer => Unlock.Fertilizer,
            APItem.Functions => Unlock.Functions,
            APItem.Grass => Unlock.Grass,
            APItem.Hats => Unlock.Hats,
            APItem.Import => Unlock.Import,
            APItem.Leaderboard => Unlock.Leaderboard,
            APItem.Lists => Unlock.Lists,
            APItem.Loops => Unlock.Loops,
            APItem.Mazes => Unlock.Mazes,
            APItem.Megafarm => Unlock.Megafarm,
            APItem.Operators => Unlock.Operators,
            APItem.Plant => Unlock.Plant,
            APItem.Polyculture => Unlock.Polyculture,
            APItem.Pumpkins => Unlock.Pumpkins,
            APItem.Senses => Unlock.Senses,
            APItem.Simulation => Unlock.Simulation,
            APItem.Speed => Unlock.Speed,
            APItem.Sunflowers => Unlock.Sunflowers,
            APItem.Timing => Unlock.Timing,
            APItem.Trees => Unlock.Trees,
            APItem.TheFarmersRemains => Unlock.TheFarmersRemains,
            APItem.TopHat => Unlock.TopHat,
            APItem.Utilities => Unlock.Utilities,
            APItem.Variables => Unlock.Variables,
            APItem.Watering => Unlock.Watering,
            _ => null
        };
    }
    
    public static string AchievementToLocation(string name)
    {
        return name switch
        {
            Achievement.RunYourFirstCode => APLocation.RunYourFirstCode,
            Achievement.InfiniteLoop => APLocation.InfiniteLoop,
            Achievement.Flip => APLocation.Flip,
            Achievement.PlantBush => APLocation.PlantBush,
            Achievement.Hay1K => APLocation.Hay1K,
            Achievement.PlantCarrot => APLocation.PlantCarrot,
            Achievement.Wood1K => APLocation.Wood1K,
            Achievement.PlantTree => APLocation.PlantTree,
            Achievement.PlantPumpkin => APLocation.PlantPumpkin,
            Achievement.Pumpkin1K => APLocation.Pumpkin1K,
            Achievement.PetThePiggy => APLocation.PetThePiggy,
            Achievement.HigherOrderProgramming => APLocation.HigherOrderProgramming,
            Achievement.PlantSunflower => APLocation.PlantSunflower,
            Achievement.MudFarmer => APLocation.MudFarmer,
            Achievement.Power1K => APLocation.Power1K,
            Achievement.PlantCactus => APLocation.PlantCactus,
            Achievement.Cacti1K => APLocation.Cacti1K,
            Achievement.LongDino => APLocation.LongDino,            
            Achievement.StackOverflow => APLocation.StackOverflow,
            Achievement.SpawnMaze => throw new NotImplementedException(APLocation.SpawnMaze),
            Achievement.WrongOrder => APLocation.WrongOrder,
            Achievement.CircularImport => APLocation.CircularImport,
            Achievement.CauseARuntimeError => APLocation.CauseARuntimeError,
            Achievement.SizeMatters => APLocation.SizeMatters,
            Achievement.Healer => APLocation.Healer,
            Achievement.Chaos => APLocation.Chaos,
            Achievement.EquipHat => APLocation.EquipHat,
            Achievement.DinoHat => APLocation.DinoHat,
            Achievement.FashionShow => APLocation.FashionShow,
            _ => null
        };
    }
}