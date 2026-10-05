using System;
using com.seadoggie.TFWRArchipelago.Service;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Components;

public abstract class BaseComponent : MonoBehaviour, IInjectable
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

    public abstract void OnInject();

    public abstract void Initialize();
}