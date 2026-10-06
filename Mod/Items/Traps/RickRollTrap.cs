using System.Collections;
using com.seadoggie.TFWRArchipelago.Model;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items.Traps;

[Trap(APItem.RickRollTrap)]
public class RickRollTrap : BaseItem
{
    protected override string ItemName => nameof(RickRollTrap);
    public void Start()
    {
        StartCoroutine(RickRoll());
    }

    private IEnumerator RickRoll()
    {
        Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        yield return new WaitForSecondsRealtime(TrapLength);
        Completed();
    }
}