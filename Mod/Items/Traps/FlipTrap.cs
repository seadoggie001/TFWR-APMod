using System.Collections;
using com.seadoggie.TFWRArchipelago.Model;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items.Traps;

[Trap(APItem.Names.FlipTrap)]
public class FlipTrap : BaseTrap
{
    private static GameObject Farm => GameObject.Find("Farm");

    protected override IEnumerator Trap()
    {
        Farm.LeanScale(new Vector3(-1, -1, 1), 2);
        yield return new WaitForSecondsRealtime(TrapLength - 4);
        Farm.LeanScale(new Vector3(1, 1, 1), 2);
        
        Completed();
    }
}