using UnityEngine;

namespace Lucrybpin.Feedback
{
    // Call from your own confirmed-hit event. This component never calculates damage.
    public sealed class DamageFeedback : MonoBehaviour
    {
        public DamagePopup popupPrefab;
        public Transform popupOrigin;
        public Camera facingCamera;
        [Min(1)] public int lanes = 1;
        public float laneSpacing = .65f;
        public AudioClip hitSound;
        public AudioSource[] voices = new AudioSource[0];
        public Vector2 pitchRange = new Vector2(.88f, 1.14f);
        public Vector2 volumeRange = new Vector2(.55f, .65f);
        int nextVoice, nextLane;
        float lastPitch;

        public DamagePopup Show(int damage)
        {
            return Show(damage, popupOrigin != null ? popupOrigin.position : transform.position);
        }

        public DamagePopup Show(int damage, Vector3 worldPosition)
        {
            if (!isActiveAndEnabled || popupPrefab == null) return null;
            Quaternion rotation = facingCamera != null ? facingCamera.transform.rotation : Quaternion.identity;
            int count = Mathf.Max(1, lanes);
            float offset = (nextLane - (count - 1) * .5f) * laneSpacing;
            nextLane = (nextLane + 1) % count;
            // Independent of the target transform: moving/destroying a target leaves hits in place.
            var popup = Instantiate(popupPrefab, worldPosition + rotation * Vector3.right * offset, rotation);
            popup.Show(damage);
            PlaySound();
            return popup;
        }

        void PlaySound()
        {
            if (hitSound == null || voices == null || voices.Length == 0) return;
            nextVoice %= voices.Length;
            var source = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            if (source == null || !source.isActiveAndEnabled) return;
            float low = Mathf.Clamp(Mathf.Min(pitchRange.x, pitchRange.y), .1f, 3f);
            float high = Mathf.Clamp(Mathf.Max(pitchRange.x, pitchRange.y), .1f, 3f);
            float pitch = Random.Range(low, high);
            if (Mathf.Abs(pitch - lastPitch) < .04f)
                pitch = lastPitch > (low + high) * .5f ? low : high;
            lastPitch = pitch;
            // Separate sources preserve the pitch of overlapping voices.
            source.Stop();
            source.clip = hitSound;
            source.pitch = pitch;
            source.volume = Mathf.Clamp01(Random.Range(Mathf.Min(volumeRange.x, volumeRange.y), Mathf.Max(volumeRange.x, volumeRange.y)));
            source.Play();
        }

        void OnDisable()
        {
            if (voices == null) return;
            foreach (var source in voices) if (source != null) source.Stop();
        }
    }
}
