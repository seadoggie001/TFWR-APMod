using System;
using System.Reflection;
using DroneLib.Function;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Functions;

public class Teleport : BaseFunction
{
    public override string Name => "teleport";

    private const int OpCost = 200;
    
    /// <summary>
    /// Drone.UpdatePosition - Updates the position of the drone after movement
    /// </summary>
    private static MethodInfo _updatePositionMethod;

    public override void ValidateCall(FunctionValidation validationState)
    {
        try
        {
            // Validate that the parameters contains two numbers
            CorrectParams(validationState.Parameters, [
                typeof(PyNumber),
                typeof(PyNumber)
            ]);

            // Set up the program state to pass off moving the drone later
            ProgramState programState = validationState.Execution.States[validationState.DroneId];
            // StoreArgument(programState, );
            programState.currentSideEffectArgument2 = new Coords()
            {
                X = (int)(PyNumber)validationState.Parameters[0],
                Y = (int)(PyNumber)validationState.Parameters[1],
            };
        }
        catch (Exception ex)
        {
            Plugin.Log.LogException("Teleport Exception", ex);
        }
    }

    public override double PerformAction(Execution execution, int droneId, object customArgument = null)
    {
        ProgramState state = execution.States[droneId];
        Coords coords = (Coords) customArgument!;
        Drone drone = execution.sim.farm.drones[droneId];
        
        Plugin.Log.LogInfo($"[Teleport] Attempting to teleport to ({coords.X},{coords.Y})");

        // If the coords are outside the farm
        if (coords.X > execution.sim.farm.grid.WorldSize.x
            || coords.Y > execution.sim.farm.grid.WorldSize.y
            || coords.X < 0 
            || coords.Y < 0)
        {
            Logger.LogWarning("You may not teleport out of the farm! You're a drone, you're not magic.", state);
            return 0;
        }

        // Find any objects at the current/target position
        if (execution.sim.farm.grid.entities.TryGetValue(drone.pos, out FarmObject objAtCurrent)
            && execution.sim.farm.grid.entities.TryGetValue(coords.Vector2Int, out FarmObject objAtDest))
        {
            if (objAtDest is HedgePlant || objAtCurrent is HedgePlant)
            {
                Logger.LogWarning("You may not teleport into or out of a maze!", state);
                return 0;
            }

            if (drone.hat is DinosaurHat)
            {
                Logger.LogWarning("Dinosaurs may not teleport!", state);
                return 0;
            }
        }
        else
        {
            Plugin.Log.LogError("Failed to get entities at the positions for a teleport!");
            Logger.LogError("Teleportation failure: entity request. Please report to the mod author.", state);
            return 0;
        }

        // Find the UpdatePosition method
        _updatePositionMethod ??= typeof(Drone).GetMethod(
            "UpdatePosition",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(double)],
            null);
        if (_updatePositionMethod is null)
        {
            Plugin.Log.LogError("Failed to locate UpdatePosition method on Drone");
            Logger.LogError("Teleportation failure: position update. Please report to the mod author.", state);
            return 0;
        }

        // Update the drone's position and run the animations
        drone.pos = coords.Vector2Int;
        _updatePositionMethod.Invoke(drone, [OpCost]);
        
        // Pass some time
        state.OpCount += OpCost;
        
        // Return the number of Ops from ApplySideEffect
        return OpCost;
    }

    private class Coords
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Vector2Int Vector2Int => new(X, Y);
    }
}