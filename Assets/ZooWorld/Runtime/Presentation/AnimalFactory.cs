using UnityEngine;
namespace ZooWorld
{
    public sealed class AnimalFactory
    {
        private readonly Material[] materials;
        private readonly PhysicsMaterial contact;
        private readonly AnimalDefinition[] definitions;
        public AnimalFactory(AnimalDefinition[] definitions)
        {
            this.definitions = definitions;
            contact = new PhysicsMaterial("Animal contact") { bounciness = .75f, dynamicFriction = 0f, staticFriction = 0f, bounceCombine = PhysicsMaterialCombine.Maximum };
            materials = new Material[definitions.Length];
            for (int i = 0; i < definitions.Length; i++)
                materials[i] = new Material(Shader.Find("Standard")) { color = definitions[i].Color };
        }
        public AnimalActor Create(AnimalDefinition definition, Vector3 position, int id, Arena arena, ZooSimulation simulation, Transform parent)
        {
            var root = new GameObject(definition.DisplayName + " #" + id);
            root.transform.SetParent(parent); root.transform.position = position;
            var visual = new GameObject("Appearance").transform; visual.SetParent(root.transform, false);
            int index = System.Array.IndexOf(definitions, definition);
            Material material = materials[index];
            if (definition.VisualPrefab != null)
            {
                Object.Instantiate(definition.VisualPrefab, visual, false);
            }
            else if (definition.Shape == BodyShape.Frog)
            {
                Part(visual, "Body", Vector3.zero, new Vector3(.85f,.5f,.85f), material);
                Part(visual, "Left leg", new Vector3(-.4f,-.12f,-.2f), new Vector3(.4f,.23f,.65f), material);
                Part(visual, "Right leg", new Vector3(.4f,-.12f,-.2f), new Vector3(.4f,.23f,.65f), material);
                Part(visual, "Left eye", new Vector3(-.24f,.26f,.25f), Vector3.one*.22f, material);
                Part(visual, "Right eye", new Vector3(.24f,.26f,.25f), Vector3.one*.22f, material);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    Part(visual, "Segment " + i, new Vector3(Mathf.Sin(i*1.4f)*.1f, -.15f, .4f-i*.26f),
                        new Vector3(.45f-i*.045f,.35f-i*.03f,.5f), material);
            }
            root.AddComponent<SphereCollider>().sharedMaterial = contact; root.AddComponent<Rigidbody>();
            var actor = root.AddComponent<AnimalActor>();
            actor.Initialize(definition, id, arena, simulation, visual);
            return actor;
        }
        private static void Part(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere); go.name = name;
            go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = material;
        }
        public void Dispose() { Object.Destroy(contact); foreach (Material material in materials) Object.Destroy(material); }
    }
}
