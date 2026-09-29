# Unity Damage Feedback

A small Unity damage-popup module with overlapping hit audio, an agility demo, and a Codex skill that helps integrate it into other games.

**Two runtime scripts. Editable prefabs. No character, tween library, TMP or third-party package required.** The module displays damage; your game owns combat and health.

[Português](#português) · [Try the demo](#try-the-demo) · [Use in your game](#use-in-your-game) · [Install the skill](#install-the-codex-skill)

## Try the demo

1. Clone/download this repository.
2. In Unity Hub, add **`assets/UnityProject`** (not the repository root).
3. Open with **Unity 6000.3.11f1**.
4. Open `Assets/Demo/Scenes/Demo.unity` and press Play.
5. Use **+ / -**; click **Step: 50** to toggle between 50 and 1 point.

| Agility | Attacks per second |
| ---: | ---: |
| 0 | 0.6 |
| 500 | 10.6 |
| 1000 | 20.6 |
| 2000 | 40.6 |

The formula is `0.6 + agility / 50`. This captures the rapid-hit feel of the original prototype; it is not Ragnarok Online's official ASPD formula. Demo damage defaults to 7. A fractional accumulator preserves progress when agility changes and handles multiple hits in a frame. Under low FPS, hits may bunch within a frame.

## Use in your game

1. Copy **`assets/UnityProject/Assets/UnityDamageFeedback`** into your project's `Assets`, including all `.meta` files and `LICENSE.txt`.
2. Drag **`UnityDamageFeedback/Prefabs/Damage Feedback.prefab`** into your scene. It includes the popup prefab, sound and 16 AudioSources.
3. Assign **Facing Camera** in the Inspector. Optionally assign **Popup Origin** for `Show(damage)`.
4. Call the emitter when your combat system confirms a hit:

```csharp
using UnityEngine;
using Lucrybpin.Feedback;

public class HitFeedbackAdapter : MonoBehaviour
{
    [SerializeField] DamageFeedback feedback;

    // Call from your existing confirmed-hit callback.
    public void OnDamageApplied(int actualDamage, Vector3 hitPosition)
    {
        feedback.Show(actualDamage, hitPosition);
    }
}
```

Do not copy `Assets/Demo`, `Assets/Editor`, `Packages` or `ProjectSettings` into your game. If your combat code has an assembly definition, add a reference to `Lucrybpin.DamageFeedback`. The module uses Unity's built-in audio support (`com.unity.modules.audio`); the demo also uses IMGUI. No runtime GetComponent/Find is used.

Already installed? Update the existing folder deliberately; a second copy duplicates Unity GUIDs. Keep any prefab customizations in variants when maintaining your own version.

### Customize

- **Damage Popup.prefab:** `Lifetime`, `Rise Height`, `Horizontal Drift`, `Fade Start`, `Scale` curve. Children `Number` and `Shadow` expose native TextMesh color, size, font and position. A replacement font also needs its matching material on each MeshRenderer.
- **Damage Feedback scene instance:** `Popup Prefab`, `Popup Origin`, `Facing Camera`, `Lanes` (default 1), `Lane Spacing`, `Hit Sound`, `Pitch Range`, `Volume Range`, `Voices`.
- `Show(int damage)` uses the assigned origin, falling back to emitter position.
- `Show(int damage, Vector3 worldPosition)` emits at a world-space point and returns the new popup. A disabled emitter or missing popup prefab returns null.
- Clearing `Hit Sound` gives silent visual feedback. The bundled synthetic impact can be replaced with your own clip.

Popups spawn detached from targets, so they survive target movement/destruction. Camera orientation is captured at spawn. Animation uses `Time.deltaTime`, so it pauses with scaled game time. The oldest audio voice is reused at capacity. This MVP uses Instantiate/Destroy rather than pooling; profile before using it for hundreds of simultaneous combatants.

**Compatibility:** validated on Unity 6000.3.11f1 with the **Built-in Render Pipeline**. URP/HDRP and older Unity versions are not validated; their text/material setup may need adaptation. The module can be integrated without using an AI assistant.

## Install the Codex skill

The repository root is a self-contained skill: `SKILL.md`, `agents/openai.yaml` and the source assets. Install it by asking Codex's skill installer to install this repository's root as **`skill-unity-damage-feedback`**, or clone it into your personal skills directory:

```sh
git clone https://github.com/lucrybpin/skill-unity-damage-feedback.git ~/.codex/skills/skill-unity-damage-feedback
```

On Windows PowerShell:

```powershell
git clone https://github.com/lucrybpin/skill-unity-damage-feedback.git "$HOME/.codex/skills/skill-unity-damage-feedback"
```

If you configured `CODEX_HOME`, use its `skills` directory instead. Start a new Codex session after installation. In your target project's workspace, ask:

> Use $skill-unity-damage-feedback to connect damage popups and hit audio to my enemy damage events.

The skill directs Codex to use the bundled assets, inspect the existing combat code and add a small adapter. It does not install itself or publish changes, and the game needs no AI at runtime. General background: [OpenAI skill documentation](https://developers.openai.com/plugins/concepts/skills).

## Repository layout

```text
SKILL.md                    Integration instructions for Codex
agents/openai.yaml          Skill display metadata
assets/UnityProject/        Standalone Unity demo project
  Assets/UnityDamageFeedback/  Copy this folder into other games
    Runtime/                DamageFeedback + DamagePopup
    Prefabs/                Emitter + popup, ready to use
    Audio/                  Original synthetic impact
  Assets/Demo/              Agility timer, controls, binding, scene
  Assets/Editor/            Asset setup and validation tools
scripts/generate_hit.py     Reproducible audio generator (Python stdlib)
```

## Validation and maintenance

In an isolated checkout, using the installed Unity executable as `Unity`:

```sh
Unity -batchmode -projectPath "$PWD/assets/UnityProject" -executeMethod FeedbackValidation.Run -logFile validation.log
```

Do **not** add `-quit`: the validator enters Play Mode, checks actual popup behavior and exits itself. Results go to `assets/UnityProject/Logs/feedback-validation.txt`. It checks prefab references, demo independence, cadence at 10/30/60/144 FPS, agility bounds, speed changes, live rapid attacks, popup text/movement/fade/destruction, silent feedback and disabled emitters. It does not prove perceptual audio quality or cross-pipeline compatibility.

To rebuild the checked-in prefabs and demo deliberately, use **Damage Feedback > Rebuild Demo Assets**, or `-executeMethod DemoSetup.Create -quit`. This replaces the bundled demo assets; it is not an integration command for an existing game. Regenerate the original impact with `python scripts/generate_hit.py`.

## License and sound

[MIT](LICENSE). The synthetic impact is included under the same license. Unity's built-in font is referenced, not redistributed. The original First Wind Form recording and Game Over font from the private prototype are not included. See [asset provenance](THIRD_PARTY_NOTICES.md).

## Português

Este repositório contém **o módulo pronto, uma demo e uma skill do Codex**.

- **Testar:** no Unity Hub, abra `assets/UnityProject`, carregue `Assets/Demo/Scenes/Demo.unity` e dê Play. Os botões distribuem agilidade; 2000 pontos chegam a 40,6 ataques/s.
- **Reutilizar:** copie só `Assets/UnityDamageFeedback` da demo para o `Assets` do seu jogo, preservando os `.meta`. Arraste `Damage Feedback.prefab` para a cena, atribua a câmera e chame `feedback.Show(dano, posição)` no evento de dano confirmado.
- **Personalizar:** abra `Damage Popup.prefab` para editar texto, sombra, duração, subida, fade e escala. O áudio fica no componente `DamageFeedback`.
- **Usar com IA:** instale este repositório na pasta pessoal de skills e peça: “Use $skill-unity-damage-feedback para adicionar os popups aos ataques deste projeto.”
- **Som público:** o impacto incluído é sintetizado; você pode substituí-lo pelo seu clip. O áudio e a fonte do protótipo original não foram redistribuídos.

Validado no Unity 6000.3.11f1, Built-in Render Pipeline. O módulo funciona sem IA e não altera a fórmula de dano do seu jogo.
