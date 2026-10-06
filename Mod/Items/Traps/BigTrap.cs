using System.Collections;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Patches;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items.Traps;

[Trap(APItem.BigTrap)]
public class BigTrap : BaseItem
{
    protected override string ItemName => nameof(BigTrap);
    private const float Size = 2.5f;
    public void Start()
    {
        StartCoroutine(Bigger());
    }
    
    private IEnumerator Bigger()
    {
        DronePatch.ModifiedSize = new Vector3(Size,Size,Size);
        yield return new WaitForSecondsRealtime(TrapLength);
        DronePatch.ModifiedSize = null;
        
        Completed();
    }
}