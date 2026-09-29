---
name: skill-unity-damage-feedback
description: Integrate reusable Unity damage popup prefabs and overlapping hit audio into an existing combat system, or create the included agility/attack-speed demo. Use when adding this module or reproducing its rapid-hit feedback.
---

# Unity damage feedback

The working source is bundled in `assets/UnityProject/Assets/UnityDamageFeedback/`. It contains two runtime components, two configured prefabs and an original synthetic impact. Use these assets rather than recreating their behavior from memory. The optional standalone demo is `assets/UnityProject/` (Unity 6000.3.11f1, Built-in Render Pipeline).

## Integrate into an existing game

1. Inspect the project's Unity version, render pipeline, combat hit/damage callback and scene conventions. Read existing project instructions. Keep the integration limited to feedback unless the user also requests combat changes.
2. Copy the complete `UnityDamageFeedback` folder, including `.meta` files and `LICENSE.txt`, into the target `Assets` directory. If this module is already installed, inspect changes before updating it; do not install a second copy with the same GUIDs. The runtime assembly is `Lucrybpin.DamageFeedback`, namespace `Lucrybpin.Feedback`.
3. Instantiate `Prefabs/Damage Feedback.prefab` in the intended scene. It already references `Damage Popup.prefab`, the audio clip and 16 independent AudioSources. Assign a cached `Facing Camera` and, for the one-argument API, `Popup Origin` in the Inspector. Do not introduce runtime Find/GetComponent calls or a new player/enemy architecture just to wire feedback.
4. Connect one confirmed damage event to `feedback.Show(actualDamage, worldPosition)`. Keep damage calculation, attack timing and health changes in the existing game. Use a small adapter to subscribe/unsubscribe events. The emitter itself has no dependency on AutoAttack or the demo.
5. Tune the popup prefab and scene instance. One lane is the default; use `Lanes` and `Lane Spacing` for optional spreading. Preserve the user's chosen font/audio when supplied and suitable for their intended use. If changing TextMesh fonts, also assign the matching font material on each MeshRenderer.
6. Compile in the target project. Verify a real hit produces one popup, popups rise/fade/disappear, and rapid hits overlap without freezing. Check audio voices, scene references and any assembly-definition reference needed by the caller. Report what was actually tested.

## API and behavior

```csharp
using Lucrybpin.Feedback;
// Cache/assign DamageFeedback in the Inspector.
feedback.Show(actualDamage, hitPosition);
```

`Show(int)` uses Popup Origin, falling back to the emitter position. `Show(int, Vector3)` uses world coordinates. Both return the spawned DamagePopup, or null if disabled/missing a popup prefab. Audio is optional. Popups are not parented to the target and face the assigned camera's rotation at spawn; they do not continuously billboard. Animation uses scaled game time. Reusing a voice at capacity cuts its previous sound.

The default renderer is native TextMesh with Unity's built-in font material. It was validated with the Built-in Render Pipeline only. For URP/HDRP or existing TMP/UI systems, inspect compatibility and adapt the renderer/material if needed, preserving the Show API. Do not promise cross-pipeline visual compatibility without testing it.

## Optional agility demo

Only include `Assets/Demo` when requested. Open `Assets/Demo/Scenes/Demo.unity` and Play. Its AutoAttack rate is `0.6 + agility / 50` attacks/s, up to 2000 agility (40.6/s). This is inspired by the prototype, not Ragnarok's official ASPD formula. The buttons switch between 1 and 50 point steps. `DemoFeedbackBinding` is the example adapter.

Do not copy demo controls or overwrite a target project's Packages/ProjectSettings during a normal integration. DRIVES may be suggested if designing a new non-trivial entity, but the popup module does not require six layers.

## Assets and validation

The bundled sound is an original synthetic substitute. The original prototype's First Wind Form recording and Game Over font are not distributed. See `THIRD_PARTY_NOTICES.md`; preserve `LICENSE.txt` when copying the module.

For repository maintenance, `Assets/Editor/DemoSetup.cs` rebuilds bundled assets explicitly; normal use needs no rebuild. `FeedbackValidation.Run` validates the source demo using Unity batch mode without `-quit`, and exits with its own result. Commands and integration details are in `README.md`. These editor tools are outside the reusable module and must not be copied into a consuming project by default.
