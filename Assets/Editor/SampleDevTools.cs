// SampleDevTools.cs
// Development helpers of the demo project (not part of the package) for the package samples.
// Samples live in Packages/com.rotteneagle.audioreactive/Samples~, which Unity ignores.
//   Unlock: Samples~ -> Samples, so the samples are imported and editable in place (GUIDs kept).
//   Lock:   Samples -> Samples~ before shipping (open sample scenes are saved and closed first).
//   Import All Samples (test): imports every sample through the Package Manager API, exactly as a
//   user would, into Assets/Samples/Audio Reactive/<version>/.
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SampleDevTools
{
    private const string PackageName = "com.rotteneagle.audioreactive";
    private const string PackageRoot = "Packages/" + PackageName;
    private const string Locked = PackageRoot + "/Samples~";
    private const string Unlocked = PackageRoot + "/Samples";
    private const string DevScene = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Audio Reactive/Dev/Unlock Samples")]
    public static string Unlock()
    {
        string locked = Path.GetFullPath(Locked), unlocked = Path.GetFullPath(Unlocked);
        if (!Directory.Exists(locked)) return "Samples are already unlocked (or missing).";
        Directory.Move(locked, unlocked);
        AssetDatabase.Refresh();
        return "Unlocked: " + Unlocked;
    }

    [MenuItem("Audio Reactive/Dev/Lock Samples")]
    public static string Lock()
    {
        string locked = Path.GetFullPath(Locked), unlocked = Path.GetFullPath(Unlocked);
        if (!Directory.Exists(unlocked)) return "Samples are already locked.";

        EditorSceneManager.SaveOpenScenes();
        bool sampleSceneOpen = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).path.StartsWith(Unlocked)) sampleSceneOpen = true;
        if (sampleSceneOpen) EditorSceneManager.OpenScene(DevScene, OpenSceneMode.Single);

        Directory.Move(unlocked, locked);
        string meta = unlocked + ".meta";
        if (File.Exists(meta)) File.Delete(meta);
        AssetDatabase.Refresh();
        return "Locked: " + Locked;
    }

    [MenuItem("Audio Reactive/Dev/Import All Samples (test)")]
    public static string ImportAll()
    {
        int count = 0;
        foreach (Sample sample in Sample.FindByPackage(PackageName, null))
            if (sample.Import(Sample.ImportOptions.OverridePreviousImports | Sample.ImportOptions.HideImportWindow))
                count++;
        return $"Imported {count} samples.";
    }

    // Number of samples the Package Manager sees (for automated checks).
    public static string CountSamples()
    {
        int count = 0;
        foreach (Sample sample in Sample.FindByPackage(PackageName, null))
        {
            Debug.Log($"[SampleDevTools] {sample.displayName}: {sample.resolvedPath}");
            count++;
        }
        return $"{count} samples";
    }
}
