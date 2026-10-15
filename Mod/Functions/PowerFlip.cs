using System;
using System.Reflection;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Patches;
using DroneLib.Function;
using DroneLib.Helpers;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Functions;

public class PowerFlip : BaseFunction
{
    public override string Name => "power_flip";

    public override void ValidateCall(FunctionValidation validationState)
    {
        try
        {
            // Validate that the parameters contains two numbers
            CorrectParams(validationState.Parameters, [
                typeof(PyNumber)
            ]);

            // Set up the program state to pass off moving the drone later
            ProgramState programState = validationState.Execution.States[validationState.DroneId];
            
            StoreArgument(programState, (double)(PyNumber)validationState.Parameters[0]);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogException("Teleport Exception", ex);
        }
    }

    public override double PerformAction(Execution execution, int droneId, object customArgument = null)
    {
        ProgramState state = execution.States[droneId];
        if (customArgument is null) throw new NullReferenceException();
        double powerToUse = (double) customArgument;
        double flipsToPerform = powerToUse * 30;
        
        Simulation sim = MainSimPatch.GetMainSim();
        if (sim is null)
        {
            Plugin.Log.LogError($"{nameof(PowerFlip)} Failed to locate main simulation. Cannot add items.");
            return 0;
        }

        // Remove the power
        if (!sim.farm.Items.Remove(StringIds.PowerItemId, powerToUse))
        {
            // Quit if power is missing
            Logger.LogWarning("Not enough power to perform this action", state);
            return 0;
        }
        
        // Actually do a flip
        Drone drone = execution.sim.farm.drones[droneId];
        drone.DoAFlip();
        
        // Tell the stats that we performed a bunch of flips (the flips are a lie!)
        GoalManager.Instance?.StatsService.Add("flips", flipsToPerform);

        // Set the function's return value
        ProgramState programState = execution.States[droneId];
        programState.ReturnValue = new PyNone();
        
        // Convert 1 second into OPs
        double ops = Math.Floor(1.0 / execution.sim.OpDuration.Seconds);

        // Increment time
        programState.AddAndConsumeOps(ops);
        return ops;
    }
}