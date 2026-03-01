using System.Reflection;

namespace com.seadoggie.TFWRArchipelago.Patches;

public static class MainSimPatch
{
    public static Simulation GetMainSim()
    {
        try
        {
            Type type = typeof(MainSim);
            FieldInfo fieldInfo = type.GetField("sim", BindingFlags.NonPublic | BindingFlags.Instance);
            object value = fieldInfo?.GetValue(MainSim.Inst);
            return (Simulation)value;
        }
        catch (Exception e)
        {
            Plugin.LogError("Failed to get MainSim.sim", e);
            return null;
        }
    }
}