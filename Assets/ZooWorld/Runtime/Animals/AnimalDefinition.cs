using UnityEngine;
namespace ZooWorld
{
    public enum Diet { Prey, Predator }
    public enum BodyShape { Frog, Snake }

    [CreateAssetMenu(menuName = "Zoo World/Animal species")]
    public sealed class AnimalDefinition : ScriptableObject
    {
        public string DisplayName = "Animal";
        public Diet Diet;
        public BodyShape Shape;
        [Tooltip("Optional custom visual prefab; procedural primitive appearance is the fallback.")]
        public GameObject VisualPrefab;
        public Color Color = Color.green;
        [Min(0.1f)] public float Radius = 0.45f;
        [Min(0.1f)] public float Mass = 1f;
        [Min(0f)] public float SpawnWeight = 1f;
        public MovementProfile Movement;
    }
}
