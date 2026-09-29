using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Lucrybpin.Feedback;
using Lucrybpin.Feedback.Demo;

public static class FeedbackValidation
{
    const string Scene = "Assets/Demo/Scenes/Demo.unity";
    const string Report = "Logs/feedback-validation.txt";
    static DamagePopup popup;
    static DamageFeedback feedback;
    static AutoAttack attacks;
    static float start;
    static int hits;
    static bool inspected;
    static bool failed;
    static double enterAfter;

    public static void ValidateAssetsAndCadence()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<DamageFeedback>("Assets/UnityDamageFeedback/Prefabs/Damage Feedback.prefab");
        Require(prefab != null && prefab.popupPrefab != null && prefab.hitSound != null, "Emitter dependencies");
        Require(prefab.voices.Length == 16, "Independent audio voices");
        foreach (var voice in prefab.voices) Require(voice != null && !voice.playOnAwake, "Audio reference");
        Require(prefab.popupPrefab.label.font != null && prefab.popupPrefab.shadow != null, "Popup references");
        foreach (string dependency in AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab), true))
            Require(!dependency.StartsWith("Assets/Demo") && !dependency.StartsWith("Assets/Editor"), "Module must be independent of demo");
        foreach (int agility in new[] { 0, 50, 500, 2000 })
        foreach (int fps in new[] { 10, 30, 60, 144 })
        {
            var timer = new GameObject("Timer validation").AddComponent<AutoAttack>();
            timer.AddAgility(agility);
            int count = 0;
            timer.Hit += damage => { Require(damage == 7, "Damage payload"); count++; };
            for (int i = 0; i < fps * 10; i++) timer.Tick(1f / fps);
            Require(Math.Abs(count - (.6f + agility / 50f) * 10f) <= 1.01f, "Cadence across frame rates");
            timer.AddAgility(int.MaxValue);
            Require(timer.Agility == 2000, "Upper agility clamp");
            timer.AddAgility(int.MinValue);
            Require(timer.Agility == 0, "Lower agility clamp");
            UnityEngine.Object.DestroyImmediate(timer.gameObject);
        }
        var changing = new GameObject("Rate change validation").AddComponent<AutoAttack>();
        int changedHits = 0;
        changing.Hit += _ => changedHits++;
        changing.Tick(.5f); // 0.3 of an attack.
        changing.AddAgility(50);
        changing.Tick(.5f); // 0.8 more -> exactly one hit, carrying progress.
        Require(changedHits == 1, "Preserve cycle progress when agility changes");
        UnityEngine.Object.DestroyImmediate(changing.gameObject);
        Debug.Log("PASS: prefab dependencies, payload, 16 cadence cases, agility bounds and rate changes.");
    }

    // Run with -batchmode -executeMethod FeedbackValidation.Run (without -quit).
    public static void Run()
    {
        try
        {
            ValidateAssetsAndCadence();
            EditorSceneManager.OpenScene(Scene);
            // Let Unity finish its startup/indexing callbacks before reloading the domain.
            enterAfter = EditorApplication.timeSinceStartup + 3;
            EditorApplication.update += EnterWhenReady;
        }
        catch (Exception e) { Finish(false, e.ToString()); }
    }

    static void EnterWhenReady()
    {
        if (EditorApplication.timeSinceStartup < enterAfter || EditorApplication.isUpdating || EditorApplication.isCompiling) return;
        EditorApplication.update -= EnterWhenReady;
        SessionState.SetBool("DamageFeedback.Validation", true);
        EditorApplication.EnterPlaymode();
    }

    [InitializeOnLoadMethod]
    static void Resume()
    {
        if (!SessionState.GetBool("DamageFeedback.Validation", false)) return;
        EditorApplication.playModeStateChanged += OnState;
        Application.logMessageReceived += OnLog;
    }

    static void OnState(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        try
        {
            feedback = UnityEngine.Object.FindFirstObjectByType<DamageFeedback>();
            attacks = UnityEngine.Object.FindFirstObjectByType<AutoAttack>();
            Require(feedback != null && attacks != null, "Demo scene wiring");
            attacks.AddAgility(2000);
            attacks.Hit += _ => hits++;
            popup = feedback.Show(123, new Vector3(1, -1, 0));
            popup.lifetime = .5f;
            popup.horizontalDrift = 0;
            Require(popup.label.text == "123" && popup.shadow.text == "123", "Prefab displays supplied damage");
            Require(popup.transform.parent == null, "Popups independent of moving targets");
            start = Time.time;
            EditorApplication.update += CheckPlay;
        }
        catch (Exception e) { Finish(false, e.ToString()); }
    }

    static void CheckPlay()
    {
        try
        {
            float elapsed = Time.time - start;
            if (!inspected && elapsed > .3f && popup != null)
            {
                Require(popup.transform.position.y > -1f, "Popup rises");
                Require(popup.label.color.a < 1f, "Popup fades");
                inspected = true;
            }
            if (elapsed < 2f) return;
            Require(inspected && popup == null, "Popup expires and destroys itself");
            Require(Math.Abs(hits - 40.6f * elapsed) <= 2f, "Live high-speed attack cadence");
            feedback.hitSound = null;
            var silent = feedback.Show(42, Vector3.zero);
            Require(silent != null && silent.label.text == "42", "Popup works without audio");
            feedback.enabled = false;
            Require(feedback.Show(42) == null, "Disabled emitter does not spawn");
            CapturePreview();
            Finish(!failed, "Checks: asset references, timer cases, live high-speed hits, popup text/movement/fade/destruction, optional audio, disabled emitter.");
        }
        catch (Exception e) { Finish(false, e.ToString()); }
    }

    static void CapturePreview()
    {
        var camera = feedback.facingCamera;
        var target = new RenderTexture(960, 600, 24);
        var previous = RenderTexture.active;
        var previousTarget = camera.targetTexture;
        var image = new Texture2D(960, 600, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 960, 600), 0, 0);
            image.Apply();
            Directory.CreateDirectory("Logs");
            File.WriteAllBytes("Logs/popup-preview.png", image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previous;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(image);
        }
    }

    static void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failed = true;
    }
    static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception("Validation failed: " + message);
    }
    static void Finish(bool pass, string message)
    {
        SessionState.SetBool("DamageFeedback.Validation", false);
        EditorApplication.update -= CheckPlay;
        Directory.CreateDirectory("Logs");
        File.WriteAllText(Report, (pass ? "PASS\n" : "FAIL\n") + message);
        Debug.Log((pass ? "VALIDATION PASS: " : "VALIDATION FAIL: ") + message);
        EditorApplication.Exit(pass ? 0 : 1);
    }
}
