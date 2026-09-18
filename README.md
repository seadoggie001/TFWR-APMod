# Archipelago + The Farmer Was Replaced

This mod is an implementation of Archipelago in The Farmer Was Replaced. Archipelago is "is a cross-game modification system which randomizes different games, then uses the result to build a single unified multi-player game". To learn more about Archipelago, please see [their website](https://archipelago.gg/), especially their FAQ.

This mod randomizes the research tree and uses the research as items that get shuffled into the multiworld. Find items by completing various tasks primarily based on the original Steam achievements.

## Installation 

### Automatic - Mod

1. Download and install a mod manager such as Thunderstore Mod Manager or r2modman
2. Click **Install with App** at the top of the page
3. Run the game via the mod manager.

### Manual - Mod

1. Download and install BepInEx in the game folder. The game folder is likely located:
- Windows: `%AppData%\Steam\steamapps\common\The Farmer Was Replaced\`
- Linux: `/home/<username>/.local/share/Steam/steamapps/common/The Farmer Was Replaced/`
2. Download the latest version from GitHub or Thunderstore
3. Locate the mod's folder. Navigate to the game folder followed by: `BepInEx/plugins/TFWRArchipelago` You may need to create this directory.
4. If any previously installed files are in the mod folder, delete them first.
5. Extract the zip to the mod folder

### APWorld
Archipelago uses Python logic bundled into a zip to determine how to shuffle checks and items. You can download a copy of the TFWR APWorld on [GitHub.](https://github.com/seadoggie001/Archipelago)

## Options
When generating a multi-world, you have some choices about how to play the game.

### Goal
Pick what your end-goal is. What are you trying to achieve?
 - **Gold**: Collect 1,000 gold to win
 - **Dinosaur Tail**: Create a dinosaur tail that is 1,000 blocks long

### Randomized Crop Cost
By enabling this option, the mod will determine a new cost for crops. A few rules are followed:
- Hay is always free
- The game is always complete-able
- A crop has a 20% chance to cost two other crops

### Early Riser
Ensures that the `plant` function is placed early in the game. This helps to ensure a faster game.

### Grass Sanity
Add a check to each square of the farm. To collect it, harvest hay on that square.

### Crop Target \#\#
Replaces the number of crops that you need to collect for all locations with a matching value.
For example, `crop_target_100: 1234` means that instead of harvesting 100 Carrots to progress, you'd need 1,234

### Trap Percentage
Tune the percent of traps that replace filler items. A value of 0 will disable all traps
