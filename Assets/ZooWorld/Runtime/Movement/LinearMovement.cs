using UnityEngine;
namespace ZooWorld
{
    [CreateAssetMenu(menuName = "Zoo World/Movement/Linear")]
    public sealed class LinearMovement : MovementProfile
    {
        [Min(0.1f)] public float Speed = 2.2f;
        public override IAnimalMotor CreateMotor() => new Motor(Speed);
        private sealed class Motor : IAnimalMotor
        {
            private readonly float speed;
            public Motor(float speed) { this.speed = speed; }
            public float VelocityResponse => 9f;
            public float VisualHeight => 0f;
            public Vector3 Step(Vector3 heading, float dt) => heading * speed;
        }
    }
}
