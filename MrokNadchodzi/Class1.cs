using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using HarmonyLib;

[BepInPlugin("czekoloczek.peak.mroknadchodzi", "Mrok Nadchodzi", "1.0.3")]
public class PeakMod : BaseUnityPlugin
{
    private AudioClip replacementClip;

    private void Awake()
    {
        Logger.LogInfo("Mrok nadchodzi was loaded!");
        var harmony = new Harmony("com.czekoloczek.peakmod.mroknadchodzi");
        harmony.PatchAll();
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(LoadAudio());
    }

    private IEnumerator LoadAudio()
    {
        string path = $"file://{Paths.PluginPath}/MrokNadchodzi/replacement.ogg";
        using (WWW www = new WWW(path))
        {
            yield return www;
            replacementClip = www.GetAudioClip(false, false, AudioType.OGGVORBIS);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only replace audio when scene name is Level_x
        if (System.Text.RegularExpressions.Regex.IsMatch(scene.name, @"^Level_\d+$"))
        {
            StartCoroutine(ReplaceAudio());
        }
    }

    private IEnumerator ReplaceAudio()
    {
        yield return new WaitForSeconds(1f);
        var audioObj = GameObject.Find("GAME/GUIManager/Canvas_HUD/TheFogRises/SFX");
        if (audioObj != null && replacementClip != null)
        {
            var audioSource = audioObj.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = replacementClip;
                Logger.LogInfo("Dźwięk został podmieniony.");
            }
        }
    }
}

// Patch for LocalizedText.RefreshText
[HarmonyPatch]
public static class LocalizedText_RefreshText_Patch
{
    static System.Reflection.MethodBase TargetMethod()
    {
        var type = AccessTools.TypeByName("LocalizedText");
        return AccessTools.Method(type, "RefreshText");
    }

    static void Postfix(object __instance)
    {
        var comp = __instance as Component;
        if (comp != null && comp.gameObject != null)
        {
            string path = comp.gameObject.transform.GetHierarchyPath();
            if (path.EndsWith("GAME/GUIManager/Canvas_HUD/TheFogRises/Fog"))
            {
                var tmp = comp.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "MROK NADCHODZI...";
                }
            }
        }
    }
}

// Helper do pobierania pełnej ścieżki obiektu
public static class TransformExtensions
{
    public static string GetHierarchyPath(this Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
    public static string GetHierarchyPath(this GameObject go) => go.transform.GetHierarchyPath();
}
