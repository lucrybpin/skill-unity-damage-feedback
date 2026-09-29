using UnityEngine;

namespace Lucrybpin.Feedback
{
    // Presentation of one hit. References and visual settings live on the prefab.
    public sealed class DamagePopup : MonoBehaviour
    {
        public TextMesh label;
        public TextMesh shadow;
        [Min(.01f)] public float lifetime = 1.15f;
        public float riseHeight = 2.1f;
        public float horizontalDrift = .25f;
        [Range(0f, .99f)] public float fadeStart = .3f;
        public AnimationCurve scale = new AnimationCurve(
            new Keyframe(0f, .5f), new Keyframe(.09f, 1.25f), new Keyframe(1f, .65f));
        Vector3 origin, initialScale;
        Color color, shadowColor;
        float age, drift;
        bool initialized;

        public void Show(int damage)
        {
            if (label == null) { Destroy(gameObject); return; }
            if (!initialized)
            {
                initialScale = transform.localScale;
                color = label.color;
                if (shadow != null) shadowColor = shadow.color;
            }
            label.text = damage.ToString();
            if (shadow != null) shadow.text = label.text;
            origin = transform.position;
            drift = Random.Range(-horizontalDrift, horizontalDrift);
            age = 0f;
            initialized = true;
            transform.localScale = initialScale * scale.Evaluate(0f);
            label.color = color;
            if (shadow != null) shadow.color = shadowColor;
        }

        void Update()
        {
            if (!initialized) return;
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / Mathf.Max(.01f, lifetime));
            transform.position = origin + transform.rotation * new Vector3(drift * t, (1f - Mathf.Pow(1f - t, 3f)) * riseHeight, 0f);
            transform.localScale = initialScale * scale.Evaluate(t);
            float alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(fadeStart, 1f, t));
            label.color = new Color(color.r, color.g, color.b, color.a * alpha);
            if (shadow != null) shadow.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a * alpha);
            if (age >= lifetime) Destroy(gameObject);
        }
    }
}
