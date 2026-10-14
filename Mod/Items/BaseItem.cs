using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items;

public abstract class BaseItem : MonoBehaviour
{
    public const int TrapLength = 15;

    protected void Completed() => Destroy(this);

    public void RaiseNotification(string message)
    {
        
    }
}