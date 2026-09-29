using UnityEngine;

namespace Lucrybpin.Feedback.Demo
{
    // Only the demo knows about AutoAttack; the reusable module does not.
    public sealed class DemoFeedbackBinding : MonoBehaviour
    {
        public AutoAttack attacks;
        public DamageFeedback feedback;
        void OnEnable() => attacks.Hit += OnHit;
        void OnDisable() => attacks.Hit -= OnHit;
        void OnHit(int damage) => feedback.Show(damage);
    }
}
