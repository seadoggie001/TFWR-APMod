using System;
using System.Linq;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Patches;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Items.Filler;

[Item(APItem.FreeHay)]
public class FreeHayFiller : BaseItem
{
    protected override string ItemName => nameof(FreeHayFiller);

    public void Start()
    {
        try
        {
            int? hayId = ResourceManager.GetAllItems().FirstOrDefault(m => m.itemName == "hay")?.itemId;
            if (hayId is null)
            {
                Log.LogError($"Failed to locate hay itemId. Cannot add items.");
                return;
            }

            Simulation sim = MainSimPatch.GetMainSim();
            if (sim is null)
            {
                Log.LogError("Failed to locate main simulation. Cannot add items.");
                return;
            }
            
            // Determine number of hay to gift
            double gifted = sim.farm.Items.GetNumber((int)hayId) * 0.2;
            gifted = Math.Floor(Math.Min(gifted, 10));
            // Add the hay
            sim.farm.Items.AddItem((int)hayId, gifted);
            // Update statistics too
            GoalManager.Instance?.RaiseStatEvent("hay", gifted);

            Completed();
        }
        catch (Exception ex)
        {
            Log.LogException($"{nameof(FreeHayFiller)}", ex);
        }
    }
}