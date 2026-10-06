using System.Collections;
using com.seadoggie.TFWRArchipelago.Model;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items.Traps;

[Trap(APItem.FlipTrap)]
public class FlipTrap : BaseItem
{
    protected override string ItemName => nameof(FlipTrap);
    private static GameObject Farm => GameObject.Find("Farm");
    public void Start()
    {
        StartCoroutine(Flip());
    }

    private IEnumerator Flip()
    {
        Farm.LeanScale(new Vector3(-1, -1, 1), 2);
        yield return new WaitForSecondsRealtime(TrapLength - 4);
        Farm.LeanScale(new Vector3(1, 1, 1), 2);
        
        Completed();
    }
}