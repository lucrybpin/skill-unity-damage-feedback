using System;
using UnityEngine;

namespace Lucrybpin.Feedback.Demo
{
    public sealed class AutoAttack : MonoBehaviour
    {
        public const int MaxAgility = 2000;
        [SerializeField, Range(0, MaxAgility)] int agility;
        [SerializeField, Min(1)] int damage = 7;
        public int Agility => agility;
        public float AttacksPerSecond => .6f + agility / 50f;
        public float Interval => 1f / AttacksPerSecond;
        public event Action<int> Hit;
        float progress;

        public void AddAgility(int points) => agility = (int)Math.Max(0L, Math.Min(MaxAgility, (long)agility + points));
        void Update() => Tick(Time.deltaTime);
        public void Tick(float deltaTime)
        {
            progress += Mathf.Max(0f, deltaTime) * AttacksPerSecond;
            while (progress >= 1f)
            {
                progress -= 1f;
                Hit?.Invoke(damage);
            }
        }
    }
}
