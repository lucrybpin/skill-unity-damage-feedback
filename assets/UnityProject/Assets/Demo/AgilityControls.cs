using UnityEngine;

namespace Lucrybpin.Feedback.Demo
{

// Input is the only component issuing commands to AutoAttack.
public sealed class AgilityControls : MonoBehaviour
{
    public AutoAttack attacks;
    int step = 50;
    GUIStyle title, label, button;

    void OnGUI()
    {
        if (title == null)
        {
            title = new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.MiddleCenter };
            label = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            button = new GUIStyle(GUI.skin.button) { fontSize = 24 };
        }
        var previousMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 960f, Screen.height / 600f, 1f));
        GUI.Label(new Rect(0, 28, 960, 45), "AGILITY / DAMAGE", title);
        GUI.Label(new Rect(0, 420, 960, 32), $"AGI {attacks.Agility}   |   {attacks.AttacksPerSecond:0.0} Attacks/s   |   {attacks.Interval * 1000:0} ms", label);
        GUI.enabled = attacks.Agility > 0;
        if (GUI.Button(new Rect(290, 465, 100, 58), "-", button)) attacks.AddAgility(-step);
        GUI.enabled = true;
        if (GUI.Button(new Rect(405, 465, 150, 58), $"Step: {step}", label)) step = step == 50 ? 1 : 50;
        GUI.enabled = attacks.Agility < AutoAttack.MaxAgility;
        if (GUI.Button(new Rect(570, 465, 100, 58), "+", button)) attacks.AddAgility(step);
        GUI.enabled = true;
        GUI.matrix = previousMatrix;
    }
}

}
