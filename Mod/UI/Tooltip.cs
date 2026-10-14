using UnityEngine;
using UnityEngine.UIElements;

namespace com.seadoggie.TFWRArchipelago.UI;

// Modified version of https://github.com/plyoung/UIElements/blob/main/Scripts/Tooltip/Tooltip.cs
public class Tooltip : VisualElement
{
    [UnityEngine.Scripting.Preserve]
    public new class UxmlFactory : UxmlFactory<Tooltip, UxmlTraits>
    {
    }

    [UnityEngine.Scripting.Preserve]
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        private readonly UxmlIntAttributeDescription _delay = new() { name = "delay", defaultValue = 500 };

        private readonly UxmlIntAttributeDescription _fadeTime = new() { name = "fade-time", defaultValue = 15 };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            if (ve is not Tooltip item) return;
            item.Delay = _delay.GetValueFromBag(bag, cc);
            item.FadeTime = _fadeTime.GetValueFromBag(bag, cc);
        }
    }

    public class TooltipManipulator(Tooltip tooltip) : Manipulator
    {
        public override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseEnterEvent>(MouseIn);
            target.RegisterCallback<MouseOutEvent>(MouseOut);
        }

        public override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseEnterEvent>(MouseIn);
            target.UnregisterCallback<MouseOutEvent>(MouseOut);
        }

        private void MouseIn(MouseEnterEvent _) => tooltip.Show(target);

        private void MouseOut(MouseOutEvent _) => tooltip.Close();
    }

    public int Delay { get; private set; } = 500; // how long to wait before appearing
    public int FadeTime { get; private set; } = 15; // speed at which it fades in and out (lower = faster)

    private readonly Label _label;
    private IVisualElementScheduledItem _task;

    private const string USSClassName = "tooltip";
    private const string USSLabel = USSClassName + "__label";

    public Tooltip()
    {
        AddToClassList(USSClassName);

        pickingMode = PickingMode.Ignore;

        StyleLength padding = new(10);
        // label
        _label = new Label
        {
            pickingMode = PickingMode.Ignore,
            classList = { USSLabel }
        };
        Add(_label);

        style.position = Position.Absolute;
        style.visibility = Visibility.Hidden;
        style.opacity = 0f;
    }

    public virtual void Show(VisualElement target)
    {
        string msg = target.tooltip;
        if (string.IsNullOrWhiteSpace(msg)) return;

        if (FadeTime > 0.0f)
        {
            _task?.Pause();
            style.visibility = Visibility.Visible;
            style.opacity = 0f;
            _task = schedule
                .Execute(() => style.opacity = Mathf.Clamp01(resolvedStyle.opacity + 0.1f))
                .Every(FadeTime) // ms	
                .Until(() => resolvedStyle.opacity >= 1.0f)
                .StartingIn(Delay);
        }
        else
        {
            style.visibility = Visibility.Visible;
            style.opacity = 1f;
        }


        // check if there is position hint in tooltip
        char hint = 'B';
        if (msg.Length > 2 && msg[1] == ':')
        {
            hint = target.tooltip[0] switch
            {
                'B' => 'B',
                'b' => 'B',
                'T' => 'T',
                't' => 'T',
                'L' => 'L',
                'l' => 'L',
                'R' => 'R',
                'r' => 'R',
                _ => 'B'
            };

            msg = msg.Substring(2);
        }

        _label.text = msg;

        float styleTop, styleLeft;
        switch (hint)
        {
            case 'L': // left
                styleLeft = target.worldBound.xMin - worldBound.width - 5;
                styleTop = target.worldBound.center.y - (worldBound.height * 0.5f);
                break;
            case 'R': // right
                styleLeft = target.worldBound.xMax + 5;
                styleTop = target.worldBound.center.y - (worldBound.height * 0.5f);
                break;
            case 'T': //top
                styleLeft = target.worldBound.center.x - (worldBound.width * 0.5f);
                styleTop = target.worldBound.yMin - worldBound.height - 5;
                break;
            default: // bottom
                styleLeft = target.worldBound.center.x - (worldBound.width * 0.5f);
                styleTop = target.worldBound.yMax + 5;
                break;
        }
        
        // Note: this is weird because of the 75px offset used to skip over the item list
        styleTop -= 75;
        if (styleLeft < parent.worldBound.xMin) styleLeft = parent.worldBound.xMin;
        if (styleLeft + worldBound.width > parent.worldBound.xMax)
            styleLeft = parent.worldBound.xMax - worldBound.width;
        // if (styleTop < parent.worldBound.yMin) styleTop = parent.worldBound.yMin;
        // if (styleTop + worldBound.height > parent.worldBound.yMax)
        //     styleTop = parent.worldBound.yMax - worldBound.height;

        style.left = styleLeft;
        style.top = styleTop;
    }

    public virtual void Close()
    {
        if (FadeTime > 0.0f && resolvedStyle.visibility != Visibility.Hidden)
        {
            _task?.Pause();
            _task = schedule
                .Execute(() =>
                {
                    float o = Mathf.Clamp01(resolvedStyle.opacity - 0.1f);
                    style.opacity = o;
                    if (o <= 0.0f) style.visibility = Visibility.Hidden;
                })
                .Every(FadeTime) // ms						
                .Until(() => resolvedStyle.opacity <= 0.0f);
        }
        else
        {
            style.visibility = Visibility.Hidden;
            style.opacity = 0f;
        }
    }
}