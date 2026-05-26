using SaveSystem;
using UnityEditor;
using UnityEngine;

public class MenuAction
{
    [MenuItem("TTTP Actions/Saves/Clear Binary Saves (.dat)")]
    private static void ClearBinarySaves()
    {
        string savePath = Application.persistentDataPath;
        bool shouldClear = EditorUtility.DisplayDialog(
            "Clear Binary Saves",
            $"Delete all .dat save files?\n\nPath:\n{savePath}",
            "Delete",
            "Cancel");

        if (!shouldClear)
        {
            return;
        }

        BinarySaveSystem.DeleteAllSaves();
        AssetDatabase.Refresh();
        Debug.Log($"[MenuAction] Cleared binary saves at: {savePath}");
    }

    [MenuItem("TTTP Actions/Saves/Clear PlayerPrefs")]
    private static void ClearPlayerPrefs()
    {
        bool shouldClear = EditorUtility.DisplayDialog(
            "Clear PlayerPrefs",
            "Delete all PlayerPrefs data for this project?",
            "Delete",
            "Cancel");

        if (!shouldClear)
        {
            return;
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[MenuAction] Cleared PlayerPrefs");
    }

    [MenuItem("TTTP Actions/Saves/Open Persistent Data Path")]
    private static void OpenPersistentDataPath()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }

    [MenuItem("TTTP Actions/Cards/Add Card in hand")]
    public static void AddHand()
    {
        //CardPoolController.Instance.AddCard(PlayerManager.Instance.GetCurrentPlayer());
        //CardPoolController.Instance.UpdateCardPosition(false);
    }

    [MenuItem("TTTP Actions/Cards/Add Card in full hand")]
    public static void AddFullHand()
    {
        //CardPoolController.Instance.ChangeCurrentPlayerView(PlayerManager.Instance.GetCurrentPlayer());
        //CardPoolController.Instance.UpdateCardPosition(false);
    }

    [MenuItem("TTTP Actions/Cards/Remove Card from hand")]
    public static void RemoveHand()
    {
        //CardPoolController.Instance.RemoveCard(PlayerManager.Instance.GetCurrentPlayer(),0);
        //CardPoolController.Instance.UpdateCardPosition(false);
    }

    [MenuItem("TTTP Actions/Cards/Remove Card from full hand")]
    public static void RemoveFullHand()
    {
        //CardPoolController.Instance.ResetHandPool(PlayerManager.Instance.GetCurrentPlayer());
        //CardPoolController.Instance.UpdateCardPosition(false);
    }

    [MenuItem("TTTP Actions/Cards/Update Card position")]
    public static void UpdateHand()
    {
        //CardPoolController.Instance.UpdateCardPosition(false);
    }

    [MenuItem("TTTP Actions/Mana/Reset Mana")]
    public static void ResetMana()
    {
        //ManaController.Instance.RestoreAllMana();
        //ManaController.Instance.UpdateManaUI();
    }

    [MenuItem("TTTP Actions/Mana/Add 1 bonus Mana")]
    public static void Bonus1Mana()
    {
        //ManaController.Instance.AddBonusMana(1);
        //ManaController.Instance.UpdateManaUI();
    }

    [MenuItem("Language/English")]
    static void English()
    {
        I2.Loc.LocalizationManager.CurrentLanguage = "English";
    }

    [MenuItem("Language/Russian")]
    static void Russian()
    {
        I2.Loc.LocalizationManager.CurrentLanguage = "Russian";
    }
}