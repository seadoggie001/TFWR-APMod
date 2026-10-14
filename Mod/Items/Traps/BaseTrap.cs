using System.Collections;
using System.Reflection;

namespace com.seadoggie.TFWRArchipelago.Items.Traps;

public abstract class BaseTrap : BaseItem
{
    public void Start()
    {
        if (IsDisabled()) return;
        // Start the trap
        StartCoroutine(Trap());
    }

    protected bool IsDisabled()
    {
        // Get the trap's name
        string trapName = GetType().GetCustomAttribute<TrapAttribute>().Name;
        // Check if the trap is disabled
        return OptionHolder.GetString(trapName, "") != "enabled";
    }

    protected abstract IEnumerator Trap();
}