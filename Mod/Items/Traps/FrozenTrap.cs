using System.Collections;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Patches;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items.Traps;

[Trap(APItem.Names.FrozenTrap)]
public class FrozenTrap : BaseTrap
{
    protected override IEnumerator Trap()
    {
        DronePatch.IsFrozen = true;
        yield return new WaitForSecondsRealtime(TrapLength);
        DronePatch.IsFrozen = false;
        
        Completed();
    }
}