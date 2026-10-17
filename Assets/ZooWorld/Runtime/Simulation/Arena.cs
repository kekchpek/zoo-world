using UnityEngine;
namespace ZooWorld
{
    public sealed class Arena
    {
        private readonly Camera camera;
        public Arena(Camera camera) { this.camera = camera; }
        private Vector3 Ground(float x, float y)
        {
            var ray = camera.ViewportPointToRay(new Vector3(x, y, 0));
            new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float distance);
            return ray.GetPoint(distance);
        }
        public Rect Bounds
        {
            get
            {
                // Reserve the header and footer so the simulation remains visible beneath uGUI.
                Vector3 a = Ground(0.04f, 0.13f), b = Ground(0.96f, 0.78f);
                return Rect.MinMaxRect(a.x, a.z, b.x, b.z);
            }
        }
        public Vector3 RandomPoint(float radius)
        {
            Rect b = Bounds;
            return new Vector3(Random.Range(b.xMin + radius, b.xMax - radius), 0.5f,
                Random.Range(b.yMin + radius, b.yMax - radius));
        }
        public bool GetReturnHeading(Vector3 position, float radius, out Vector3 heading)
        {
            Rect b = Bounds;
            Vector3 nearest = new Vector3(Mathf.Clamp(position.x, b.xMin + radius, b.xMax - radius), position.y,
                Mathf.Clamp(position.z, b.yMin + radius, b.yMax - radius));
            heading = nearest - position;
            if (heading.sqrMagnitude < 0.001f) return false;
            // Aim well inside the screen to avoid chattering along an edge.
            heading = new Vector3(b.center.x - position.x, 0, b.center.y - position.z).normalized;
            return true;
        }
    }
}
