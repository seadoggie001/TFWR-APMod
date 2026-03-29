using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(SaveChooser))]
public class SaveChooserPatch
{
    private const string NewApButton = "NewAPButton";
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SaveChooser.Setup))]
    public static void Setup(SaveChooser __instance)
    {
        if (__instance.transform.Find(NewApButton) != null) return;

        Transform apButton = __instance.transform.Find("Scroll View/" + NewApButton);
        if (apButton != null) return;

        const float yPos = 171.2f;

        // Modify the Back button
        Transform backButton = __instance.transform.Find("Scroll View/BackButton");
        backButton.transform.localPosition = new Vector3(-155, yPos, 0);
        backButton.GetComponent<RectTransform>().sizeDelta = new Vector2(-10, -6);

        // Modify the New Game button
        Transform gameButton = __instance.transform.Find("Scroll View/NewGameButton");
        gameButton.transform.localPosition = new Vector3(-65, yPos, 0);
        gameButton.GetComponent<RectTransform>().sizeDelta = new Vector2(-50, -6);
        gameButton.GetComponent<ColoredButton>().Text = "New";
        gameButton.GetComponent<ColoredButton>().tooltipDescription = "Archipela-NO";

        // Copy the New Game button and modify it for New AP
        GameObject apGameObject = Object.Instantiate(gameButton.gameObject, gameButton.parent);
        apGameObject.name = NewApButton;
        apGameObject.transform.localPosition = new Vector3(40, yPos, 0);
        apGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(-25, -6);
        
        ColoredButton cButton  = apGameObject.GetComponent<ColoredButton>();
        cButton.Text = "New AP";
        cButton.onHeld.RemoveAllListeners();
        cButton.OnClick.RemoveAllListeners();
        cButton.tooltipName = "Archipela-gooooooo Game";
        cButton.tooltipDescription = "Create a new save specifically for an Archipelago slot";
        
        Button button = apGameObject.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(() =>
        {
            Plugin.Log.LogInfo("SaveChooserPatch NewAPButton OnClick");
            string saveName = SaveChooser.GenerateUnusedSaveName();
            string filePath = SaverPatch.GetFilePath(saveName);
            FileInfo fileInfo = new(filePath);
            if(!fileInfo.Exists && fileInfo.Directory is not null)
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            File.WriteAllText(filePath, JsonUtility.ToJson(new ModSaveGame()));
            __instance.CreateNewSave();
        });

        // Modify the Open Folder button
        Transform folderButton = __instance.transform.Find("Scroll View/OpenFolderButton");
        folderButton.transform.localPosition = new Vector3(145, yPos, 0);
        folderButton.GetComponent<RectTransform>().sizeDelta = new Vector2(-50, -6);
        folderButton.GetComponent<ColoredButton>().Text = "Folder";
    }
}