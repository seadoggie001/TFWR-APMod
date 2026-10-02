using System;
using System.Reflection;
using com.seadoggie.TFWRArchipelago.Components;
using DroneLib.Function;
using DroneLib.Helpers;

namespace com.seadoggie.TFWRArchipelago.Functions;

public class DoubleFlip : BaseFunction
{
    public override string Name => "double_flip";
    private const double Reduction = 1;

    public override void ValidateCall(FunctionValidation validationState)
        => NoParams(validationState.Parameters);

    public override double PerformAction(Execution execution, int droneId, object _ = null)
    {
        // Convert 1 second into OPs
        double ops = Math.Floor(1.0 / execution.sim.OpDuration.Seconds);
        // Reduce the time
        ops /= Reduction;
        
        // Actually do the flip
        Drone drone = execution.sim.farm.drones[droneId];
        drone.DoAFlip();
        
        GoalManager.Instance?.StatsService.Add("flips", 1);
        
        FieldInfo fieldInfo = typeof(Drone).GetField("animDuration", BindingFlags.Instance | BindingFlags.NonPublic);
        if (fieldInfo is null) throw new Exception("Failed to perform a double flip");
        fieldInfo.SetValue(drone, Duration.FromSeconds(1 / Reduction));

        // Set the function's return value
        ProgramState programState = execution.States[droneId];
        programState.ReturnValue = new PyNone();

        // Increment time
        programState.AddAndConsumeOps(ops);
        return ops;
    }
}