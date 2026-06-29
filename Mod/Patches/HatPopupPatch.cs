using System.Collections;
using System.Reflection;
using com.seadoggie.TFWRArchipelago.Utils;

namespace com.seadoggie.TFWRArchipelago.Patches;

public static class HatPopupPatch
{
    public static void ShowWithoutHat(string itemName)
    {
        try
        {
            HatPopup popup = HatPopup.Inst;
            if (popup is not null)
            {
                Type popupType = popup.GetType();
                FieldInfo text = popupType.GetField("text", BindingFlags.NonPublic | BindingFlags.Instance);
                MarkdownText mdText = (MarkdownText)text?.GetValue(popup);
                mdText?.UpdateText($"## New Item Unlocked\n`\"{itemName}\"`");

                MethodInfo showPopupAnimation =
                    popupType.GetMethod("ShowPopupAnimation", BindingFlags.NonPublic | BindingFlags.Instance);
                popup.StartCoroutine((IEnumerator)showPopupAnimation?.Invoke(popup, null));
                return;
            }

            Plugin.Log.LogWarning("Failed to get HatPopup instance");
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(ShowWithoutHat)}", e);
        }
    }
}