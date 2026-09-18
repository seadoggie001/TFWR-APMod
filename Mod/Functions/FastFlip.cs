using System;
using System.Reflection;
using DroneLib.Function;
using DroneLib.Helpers;

namespace com.seadoggie.TFWRArchipelago.Functions;

public class FastFlip : BaseFunction
{
    public override string Name => "fast_flip";
    private const double Reduction = 3;

    public override void ValidateCall(FunctionValidation validationState)
        => NoParams(validationState.Parameters);

    public override double PerformAction(Execution execution, int droneId, object _ = null)
    {
        // Convert 1 second into OPs
        double ops = Math.Floor(1.0 / execution.sim.OpDuration.Seconds);
        // Reduce to 1/3 of the time
        ops /= Reduction;
        
        // Actually do the flip
        Drone drone = execution.sim.farm.drones[droneId];
        drone.DoAFlip();
        FieldInfo fieldInfo = typeof(Drone).GetField("animDuration", BindingFlags.Instance | BindingFlags.NonPublic);
        if (fieldInfo is null) throw new Exception("Failed to perform a fast flip");
        fieldInfo.SetValue(drone, Duration.FromSeconds(1 / Reduction));

        // Set the function's return value
        ProgramState programState = execution.States[droneId];
        programState.ReturnValue = new PyNone();

        // Increment time
        programState.AddAndConsumeOps(ops);
        return ops;
    }
}