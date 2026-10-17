using UnityEngine;
namespace ZooWorld
{
    // Assets hold immutable configuration; every animal gets its own stateful motor.
    public abstract class MovementProfile : ScriptableObject
    {
        public abstract IAnimalMotor CreateMotor();
    }
    public interface IAnimalMotor
    {
        Vector3 Step(Vector3 heading, float dt);
        float VisualHeight { get; }
        float VelocityResponse { get; }
    }
}
