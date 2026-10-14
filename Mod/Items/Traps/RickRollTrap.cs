using System.Collections;
using com.seadoggie.TFWRArchipelago.Model;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items.Traps;

[Trap(APItem.Names.RickRollTrap)]
public class RickRollTrap : BaseTrap
{
    protected override IEnumerator Trap()
    {
        Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        yield return new WaitForSecondsRealtime(TrapLength);
        Completed();
    }
}