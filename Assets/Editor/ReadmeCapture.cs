// ReadmeCapture.cs
// Development helper of the demo project (not part of the package): captures frames for the package README.
//   Frames: in Play Mode, renders Camera.main into a RenderTexture and writes numbered PNGs at a fixed rate.
//     The Editor does not need focus (Application.runInBackground is set for the session); the Game view
//     shows black while capturing (the camera renders off-screen).
//     GIFs are assembled outside Unity (ffmpeg).
//   Window: reads the screen pixels of an open editor window (e.g. the Inspector with live meters).
// Called from the Unity CLI through eval (reflection on Assembly-CSharp-Editor) or the menu.
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public static class ReadmeCapture
{
    private const string DefaultOutput = "Temp/ReadmeCapture";

    private static Camera s_camera;
    private static RenderTexture s_target;
    private static Texture2D s_readback;
    private static string s_directory;
    private static int s_remaining, s_index;
    private static double s_interval, s_next;

    public static bool IsCapturing => s_remaining > 0;

    [MenuItem("Audio Reactive/Dev/Capture README Frames (5 s)")]
    private static void CaptureFromMenu() => Debug.Log(StartFrames(DefaultOutput, 100, 20, 960, 540));

    // Starts capturing `count` frames at `fps` into `directory` (cleared first). Returns a status line.
    public static string StartFrames(string directory, int count, int fps, int width, int height)
    {
        if (!EditorApplication.isPlaying) return "Enter Play Mode first.";
        if (IsCapturing) return "Already capturing.";
        Camera camera = Camera.main;
        if (camera == null) return "No Camera.main in the scene.";

        if (Directory.Exists(directory)) Directory.Delete(directory, true);
        Directory.CreateDirectory(directory);

        s_camera = camera;
        s_target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
        {
            antiAliasing = 4
        };
        s_readback = new Texture2D(width, height, TextureFormat.RGB24, false);
        s_camera.targetTexture = s_target;
        // Keep the player loop running while the Editor is in the background (e.g. driven from the CLI).
        // Runtime-only flag: Player Settings are untouched and it resets when Play Mode ends.
        Application.runInBackground = true;
        s_directory = directory;
        s_remaining = count;
        s_index = 0;
        s_interval = 1.0 / fps;
        s_next = EditorApplication.timeSinceStartup + 0.5;   // let the first off-screen frames render
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        return $"Capturing {count} frames at {fps} fps into {Path.GetFullPath(directory)}";
    }

    private static void Tick()
    {
        if (s_camera == null) { Finish(); return; }
        double now = EditorApplication.timeSinceStartup;
        if (now < s_next) return;
        s_next += s_interval;
        if (s_next < now) s_next = now + s_interval;   // editor stalled: don't burst

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = s_target;
        s_readback.ReadPixels(new Rect(0, 0, s_target.width, s_target.height), 0, 0);
        s_readback.Apply(false);
        RenderTexture.active = previous;

        File.WriteAllBytes(Path.Combine(s_directory, $"frame_{s_index++:D4}.png"), s_readback.EncodeToPNG());
        if (--s_remaining <= 0) Finish();
    }

    private static void OnPlayModeChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.ExitingPlayMode) Finish();
    }

    private static void Finish()
    {
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        if (s_camera != null) s_camera.targetTexture = null;
        if (s_target != null) UnityEngine.Object.DestroyImmediate(s_target);
        if (s_readback != null) UnityEngine.Object.DestroyImmediate(s_readback);
        s_camera = null;
        s_target = null;
        s_readback = null;
        s_remaining = 0;
        Debug.Log($"[ReadmeCapture] {s_index} frames written to {s_directory}");
    }

    // Saves the on-screen pixels of the first open editor window whose type name is `windowTypeName`
    // (e.g. "InspectorWindow"). The window must be visible, not covered by other windows.
    public static string CaptureWindow(string windowTypeName, string path)
    {
        foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
        {
            if (window.GetType().Name != windowTypeName) continue;
            window.Repaint();
            float scale = EditorGUIUtility.pixelsPerPoint;
            Rect r = window.position;
            int width = Mathf.RoundToInt(r.width * scale), height = Mathf.RoundToInt(r.height * scale);
            Color[] pixels = InternalEditorUtility.ReadScreenPixel(
                new Vector2(r.x * scale, r.y * scale), width, height);
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.SetPixels(pixels);
            texture.Apply(false);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            return $"Saved {width}x{height} to {Path.GetFullPath(path)}";
        }
        return $"No open window of type {windowTypeName}.";
    }
}
