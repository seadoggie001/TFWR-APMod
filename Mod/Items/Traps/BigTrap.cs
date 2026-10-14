using System.Collections;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Patches;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items.Traps;

[Trap(APItem.Names.BigTrap)]
public class BigTrap : BaseTrap
{
    private const float Size = 2.5f;
    
    protected override IEnumerator Trap()
    {
        DronePatch.ModifiedSize = new Vector3(Size,Size,Size);
        yield return new WaitForSecondsRealtime(TrapLength);
        DronePatch.ModifiedSize = null;
        
        Completed();
    }
}