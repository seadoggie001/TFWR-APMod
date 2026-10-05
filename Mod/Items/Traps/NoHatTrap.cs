using System.Collections;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Patches;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items.Traps;

[Trap(APItem.NoHatTrap)]
public class NoHatTrap : BaseItem
{
    protected override string ItemName => nameof(NoHatTrap);

    [Log] private readonly ILogger<NoHatTrap> _log = null!;
    private static Hat _emprerorsHat = null;
    private static readonly HatSO Hatless = HatlessHat();
    public void Start()
    {
        StartCoroutine(NoHat());
    }

    private static HatSO HatlessHat()
    {
        HatSO hatless = ScriptableObject.CreateInstance<HatSO>();
        hatless.hatMesh = new Mesh();
        HatSO strawHat = ResourceManager.GetHat("straw_hat");
        hatless.className = nameof(Hat);
        hatless.droneFlyHeight = strawHat.droneFlyHeight;
        hatless.sound1 = strawHat.sound1;
        hatless.sound2 = strawHat.sound2;
        hatless.hatName = "The emperor's new hat!";
        hatless.hidden = true;
        hatless.isGolden = false;
        hatless.preventWrapping = false;
        hatless.rotateDroneToMove = false;
        return hatless;
    }

    private IEnumerator NoHat()
    {
        DronePatch.Hatless = true;
        Simulation simulation = MainSimPatch.GetMainSim();
        foreach (Drone drone in simulation.farm.drones)
        {
            _emprerorsHat ??= Hat.CreateHat(Hatless, simulation, drone);
            drone.hat = _emprerorsHat;
        }
        yield return new WaitForSecondsRealtime(15);
        DronePatch.Hatless = false;
        HatSO straw = ResourceManager.GetHat("straw_hat");
        simulation = MainSimPatch.GetMainSim();
        foreach (Drone drone in simulation.farm.drones)
        {
            drone.hat = Hat.CreateHat(straw, simulation, drone);
        }
        Log.LogInformation("Done with no hat");
        
        Completed();
    }
}