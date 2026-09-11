using com.seadoggie.TFWRArchipelago.Service;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.UI;

public abstract class BaseGUI : MonoBehaviour
{
    public virtual void Awake()
    {
        InjectionService.Inject(this);
    }
    public abstract bool IsMouseOverWindow();
}