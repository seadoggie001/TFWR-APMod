using System.Collections;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Patches;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items.Traps;

[Trap(APItem.TinyTrap)]
public class TinyTrap : BaseItem
{
    protected override string ItemName => nameof(TinyTrap);
    
    private const float Size = 0.4f;
    
    public void Start()
    {
        StartCoroutine(Tiny());
    }
    
    private IEnumerator Tiny()
    {
        DronePatch.ModifiedSize = new Vector3(Size,Size,Size);
        yield return new WaitForSecondsRealtime(15);
        DronePatch.ModifiedSize = null;
        
        Completed();
    }
}