using com.seadoggie.TFWRArchipelago.Service;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Components;

public class BaseComponent : MonoBehaviour
{
    protected Action OnDisabled;
    protected virtual void OnEnable()
    {
        InjectionService.Inject(this);
        DontDestroyOnLoad(this);
    }
    public virtual void OnDisable()
    {
        OnDisabled?.Invoke();
    }
}