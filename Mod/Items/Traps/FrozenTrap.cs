using System.Collections;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Patches;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items.Traps;

[Trap(APItem.FrozenTrap)]
public class FrozenTrap : BaseItem
{
    protected override string ItemName => nameof(FrozenTrap);
    public void Start()
    {
        StartCoroutine(Frozen());
    }

    private IEnumerator Frozen()
    {
        DronePatch.IsFrozen = true;
        yield return new WaitForSecondsRealtime(15);
        DronePatch.IsFrozen = false;
        
        Completed();
    }
}