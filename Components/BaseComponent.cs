using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Components;

public class BaseComponent : MonoBehaviour
{
    protected Action OnDisabled;
    protected virtual void OnEnable()
    {
        DontDestroyOnLoad(this);
    }
    public virtual void OnDisable()
    {
        OnDisabled?.Invoke();
    }
}