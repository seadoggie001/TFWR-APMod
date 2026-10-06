using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Items;

public abstract class BaseItem : MonoBehaviour
{
    public const int TrapLength = 15;
    
    /// <summary>
    /// Name of the item. Used to create a log.
    /// </summary>
    protected abstract string ItemName { get; }

    protected void Completed() => Destroy(this);

    public void RaiseNotification(string message)
    {
        
    }
}