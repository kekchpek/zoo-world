using UnityEngine;
namespace ZooWorld
{
    [CreateAssetMenu(menuName = "Zoo World/Movement/Jump")]
    public sealed class JumpMovement : MovementProfile
    {
        [Min(0.2f)] public float Interval = 1.4f;
        [Min(0.1f)] public float Distance = 1.8f;
        [Min(0.05f)] public float Duration = 0.38f;
        [Min(0f)] public float Height = 0.8f;
        public override IAnimalMotor CreateMotor() => new Motor(Interval, Distance, Duration, Height);
        private sealed class Motor : IAnimalMotor
        {
            private readonly float interval, distance, duration, height;
            private float phase;
            private Vector3 jumpHeading;
            public Motor(float interval, float distance, float duration, float height)
            {
                this.interval = Mathf.Max(interval, duration); this.distance = distance;
                this.duration = Mathf.Max(0.01f, duration); this.height = height;
            }
            public float VelocityResponse => float.PositiveInfinity;
            public float VisualHeight => phase < duration ? Mathf.Sin(phase / duration * Mathf.PI) * height : 0f;
            public Vector3 Step(Vector3 heading, float dt)
            {
                // Integrate overlap with flight windows: displacement stays fixed even if dt changes.
                float remaining = dt, flight = 0f;
                if (phase == 0f) jumpHeading = heading;
                while (remaining > 0.000001f)
                {
                    float slice = Mathf.Min(remaining, interval - phase);
                    flight += Mathf.Max(0f, Mathf.Min(phase + slice, duration) - Mathf.Min(phase, duration));
                    phase += slice; remaining -= slice;
                    if (phase >= interval - 0.000001f) { phase = 0f; jumpHeading = heading; }
                }
                return jumpHeading * (distance / duration) * (flight / Mathf.Max(dt, 0.000001f));
            }
        }
    }
}
