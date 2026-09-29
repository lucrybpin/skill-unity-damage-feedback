using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Lucrybpin.Feedback;
using Lucrybpin.Feedback.Demo;

public static class DemoSetup
{
    const string Module = "Assets/UnityDamageFeedback/";
    [MenuItem("Damage Feedback/Rebuild Demo Assets")]
    public static void Create()
    {
        // Explicit rebuild operation. Normal users open the committed scene instead.
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Module + "Prefabs");
        Directory.CreateDirectory("Assets/Demo/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var popupObject = new GameObject("Damage Popup");
        var popup = popupObject.AddComponent<DamagePopup>();
        popup.label = Text(popupObject.transform, "Number", font, Color.white, Vector3.zero, 1);
        popup.shadow = Text(popupObject.transform, "Shadow", font, Color.black, new Vector3(.025f, -.03f, .01f), 0);
        var popupAsset = PrefabUtility.SaveAsPrefabAsset(popupObject, Module + "Prefabs/Damage Popup.prefab");
        Object.DestroyImmediate(popupObject);
        var emitterObject = new GameObject("Damage Feedback");
        var emitter = emitterObject.AddComponent<DamageFeedback>();
        emitter.popupPrefab = popupAsset.GetComponent<DamagePopup>();
        emitter.hitSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Module + "Audio/SyntheticHit.wav");
        emitter.voices = new AudioSource[16];
        for (int i = 0; i < emitter.voices.Length; i++)
        {
            var voice = new GameObject("Hit Voice " + (i + 1));
            voice.transform.SetParent(emitterObject.transform, false);
            var source = voice.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            emitter.voices[i] = source;
        }
        var emitterAsset = PrefabUtility.SaveAsPrefabAsset(emitterObject, Module + "Prefabs/Damage Feedback.prefab");
        Object.DestroyImmediate(emitterObject);
        var camera = new GameObject("Camera").AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographic = true;
        camera.orthographicSize = 3f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.045f, .055f, .085f);
        camera.gameObject.AddComponent<AudioListener>();
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(emitterAsset);
        var feedback = instance.GetComponent<DamageFeedback>();
        var origin = new GameObject("Popup Origin").transform;
        origin.position = new Vector3(0, -.65f, 0);
        feedback.popupOrigin = origin;
        feedback.facingCamera = camera;
        PrefabUtility.RecordPrefabInstancePropertyModifications(feedback);
        var demo = new GameObject("Agility Demo");
        var attacks = demo.AddComponent<AutoAttack>();
        var binding = demo.AddComponent<DemoFeedbackBinding>();
        binding.attacks = attacks;
        binding.feedback = feedback;
        demo.AddComponent<AgilityControls>().attacks = attacks;
        EditorSceneManager.SaveScene(scene, "Assets/Demo/Scenes/Demo.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Demo/Scenes/Demo.unity", true) };
        PlayerSettings.companyName = "lucrybpin";
        PlayerSettings.productName = "Unity Damage Feedback";
        PlayerSettings.defaultScreenWidth = 960;
        PlayerSettings.defaultScreenHeight = 600;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        AssetDatabase.SaveAssets();
        FeedbackValidation.ValidateAssetsAndCadence();
    }

    static TextMesh Text(Transform parent, string name, Font font, Color color, Vector3 position, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        var text = go.AddComponent<TextMesh>();
        text.text = "7";
        text.font = font;
        text.fontSize = 88;
        text.characterSize = .055f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = color;
        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = font.material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sortingOrder = order;
        return text;
    }
}
